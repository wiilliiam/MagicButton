using System.Device.Gpio;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using MagicButton.Data;
using MagicButton.Data.Models;

namespace MagicButton.Services
{
    public class BackgroundJobService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(IServiceScopeFactory scopeFactory,
                                    IHttpClientFactory httpClientFactory,
                                    ILogger<BackgroundJobService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ListenForButtonPress(stoppingToken);
                }
                catch (OperationCanceledException) { /* normal on shutdown */ }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ListenForButtonPress loop");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
        }

        /// <summary>
        /// Blocks until it classifies a press. Handles single/double/long.
        /// </summary>
        private async Task ListenForButtonPress(CancellationToken ct)
        {
            // pull current config + action overrides each cycle (lets you hot-edit in DB)
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MagicDbContext>();

            // You can change how you pick the active device row (e.g., by DeviceId env var)
            var device = await db.DeviceConfigs
                .Include(d => d.Actions)
                .FirstOrDefaultAsync(ct)
                ?? throw new InvalidOperationException("No DeviceConfig found.");

            var pin = device.ButtonPin;
            var pullUp = device.ButtonPullUp;
            var debounceMs = Math.Max(1, device.DebounceMs);
            var longMs = Math.Max(debounceMs + 1, device.LongPressThresholdMs);
            var dblMs = Math.Max(debounceMs + 1, device.DoublePressWindowMs);

            using var gpio = new GpioController();
            if (!gpio.IsPinOpen(pin))
                gpio.OpenPin(pin, PinMode.InputPullUp); // default safe
            if (!pullUp)
            {
                // If your board/driver supports InputPullDown, switch:
                try { gpio.SetPinMode(pin, PinMode.InputPullDown); }
                catch { /* fall back to InputPullUp above */ }
            }

            // Helpers for level semantics depending on pull-up/down wiring
            bool IsPressed() => pullUp ? gpio.Read(pin) == PinValue.Low : gpio.Read(pin) == PinValue.High;

            // Wait for initial press
            await WaitForConditionAsync(IsPressed, ct);

            var pressStarted = DateTime.UtcNow;

            // Debounce: wait a bit while still pressed, but don’t force a hold
            await Task.Delay(debounceMs, ct);

            // Track how long it stays held to decide long press
            while (IsPressed())
            {
                if ((DateTime.UtcNow - pressStarted).TotalMilliseconds >= longMs)
                {
                    // LONG PRESS: shutdown
                    await HandleLongPressAsync(device, db, ct);
                    return;
                }

                await Task.Delay(10, ct);
            }

            // Button released before long threshold -> could be single or double
            var firstReleasedAt = DateTime.UtcNow;

            // Stop bounce: tiny wait
            await Task.Delay(debounceMs, ct);

            // Look for 2nd press within window
            var doubleDeadline = firstReleasedAt.AddMilliseconds(dblMs);
            while (DateTime.UtcNow < doubleDeadline)
            {
                if (IsPressed())
                {
                    // Confirmed DOUBLE: reboot
                    await HandleDoublePressAsync(device, db, ct);
                    return;
                }

                await Task.Delay(10, ct);
            }

            // SINGLE press
            await HandleSinglePressAsync(device, db, ct);
        }


        private async Task HandleSinglePressAsync(DeviceConfig device, MagicDbContext db, CancellationToken ct)
        {
            var action = device.Actions.FirstOrDefault(a => a.Kind == PressKind.Single);
            RequestMethod method = (RequestMethod)action!.Method!;
            var url = action?.Url;

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Single press: No URL configured. Skipping HTTP call.");
                await SavePressAsync(db, device, PressKind.Single, success: false, error: "No URL configured");
                return;
            }

 

            var client = _httpClientFactory.CreateClient("magicbutton");
   

            var sw = Stopwatch.StartNew();
            HttpResponseMessage? resp = null;
            string? bodySnippet = null;
            int? status = null;
            bool transportErr = false;
            string? err = null;

            try
            {
                resp = await SendAsync(client, method, url, action.ExtraPayload, ct);
                status = (int)resp.StatusCode;

                // optional small snippet for auditing (avoid huge bodies)
                if (resp.Content != null)
                {
                    var s = await resp.Content.ReadAsStringAsync(ct);
                    bodySnippet = s.Length > 500 ? s[..500] : s;
                }
            }
            catch (Exception ex)
            {
                transportErr = true;
                err = ex.Message;
                _logger.LogError(ex, "Single press HTTP failed");
            }
            finally
            {
                sw.Stop();
            }

            bool ok = resp is { IsSuccessStatusCode: true };

            await SavePressAsync(
                db, device, PressKind.Single,
                success: ok,
                statusCode: status,
                transportError: transportErr,
                durationMs: (int)sw.ElapsedMilliseconds,
                responseSnippet: bodySnippet,
                error: err
            );

            // --- LED feedback ---
            try
            {
                int pin = ok ? 17 : 27; //22 yellow
                if (pin > 0)
                {
                    _ = Task.Run(async () =>
                    {
                        await SignalLedAsync(pin, TimeSpan.FromSeconds(20), ct);
                    });
                }
            }
            catch (Exception ledEx)
            {
                _logger.LogWarning(ledEx, "LED feedback failed");
            }

        }
        // move to device config^^
        public int GreenLedPin { get; set; } = 17; // example BCM numbers
        public int RedLedPin { get; set; } = 27;
        private async Task HandleDoublePressAsync(DeviceConfig device, MagicDbContext db, CancellationToken ct)
        {
            _logger.LogWarning("Double press -> reboot");
            await SavePressAsync(db, device, PressKind.Double, success: true);
            _ = RunSudoAsync("reboot"); // fire-and-forget; system will terminate us anyway
        }

        private async Task HandleLongPressAsync(DeviceConfig device, MagicDbContext db, CancellationToken ct)
        {
            _logger.LogWarning("Long press -> shutdown");
            await SavePressAsync(db, device, PressKind.Long, success: true);
            _ = RunSudoAsync("shutdown", "-h", "now");
        }

        private static async Task WaitForConditionAsync(Func<bool> check, CancellationToken ct)
        {
            while (!check())
            {
                await Task.Delay(10, ct);
            }
        }

        private static async Task<HttpResponseMessage> SendAsync(HttpClient client, RequestMethod method, string url, object payload, CancellationToken ct)
        {

            switch (method) {
                case RequestMethod.GET:
                    return await client.GetAsync(url, ct);
                case RequestMethod.POST:
                    return await client.PostAsJsonAsync(url, payload, ct);
                default:
                    return await client.PostAsJsonAsync(url, payload, ct);
            }
                        
        }

        private async Task SavePressAsync(
            MagicDbContext db,
            DeviceConfig device,
            PressKind kind,
            bool success,
            int? statusCode = null,
            bool transportError = false,
            int? durationMs = null,
            string? responseSnippet = null,
            string? error = null)
        {
            var entity = new ButtonPress
            {
                DeviceConfigId = device.Id,
                DeviceConfig = device,
                DeviceId = device.DeviceId,
                Kind = kind,
                Success = success,
                ResponseStatusCode = statusCode,
                TransportError = transportError,
                DurationMs = durationMs ?? 0,
                ResponseBodySnippet = responseSnippet,
                Error = error,
                CreatedAtUtc = DateTime.UtcNow
            };

            db.ButtonPresses.Add(entity);
            await db.SaveChangesAsync();
        }

        private static Task RunSudoAsync(string command, params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sudo",
                ArgumentList = { command },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            var p = new Process { StartInfo = psi, EnableRaisingEvents = false };
            p.Start();
            return Task.CompletedTask;
        }
        private async Task SignalLedAsync(int pin, TimeSpan duration, CancellationToken ct)
        {
            using var gpio = new GpioController();
            gpio.OpenPin(pin, PinMode.Output);

            gpio.Write(pin, PinValue.High); // ON
            try
            {
                await Task.Delay(duration, ct);
            }
            finally
            {
                gpio.Write(pin, PinValue.Low); // OFF
                gpio.ClosePin(pin);
            }
        }


    }
}
