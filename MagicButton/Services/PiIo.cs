using System;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;

namespace PiIo
{
    public interface IPiGpioService : IDisposable
    {
        void Start();                          // open pins & start listening
        void Stop();                           // stop listening & turn everything off

        // Manual controls (handy for tests)
        void SetLedStates(bool red, bool amber, bool green);
        Task PlayBuzzerAsync(TimeSpan duration, TimeSpan on, TimeSpan off, CancellationToken ct = default);
        Task LedDanceAsync(TimeSpan duration, CancellationToken ct = default);

        // Later expansion points
        void RegisterTransceiver(object transceiver); // placeholder for SPI/UART wrapper
    }

    public sealed class PiGpioServiceConfig
    {
        public PinNumberingScheme Numbering { get; init; } = PinNumberingScheme.Logical;

        // Outputs
        public int RedLedPin { get; init; }
        public int AmberLedPin { get; init; }
        public int GreenLedPin { get; init; }
        public int ActiveBuzzerPin { get; init; }

        // Button
        public int ButtonPin { get; init; }
        public bool ButtonPullUp { get; init; } = true;   // true if wired to GND with pull-up
        public TimeSpan Debounce { get; init; } = TimeSpan.FromMilliseconds(40);

        // Behaviour
        public TimeSpan ButtonEffectDuration { get; init; } = TimeSpan.FromSeconds(10);
        public TimeSpan BuzzerOn { get; init; } = TimeSpan.FromMilliseconds(200);
        public TimeSpan BuzzerOff { get; init; } = TimeSpan.FromMilliseconds(200);
    }

    public sealed class PiGpioService : IPiGpioService
    {
        private readonly PiGpioServiceConfig _cfg;
        private readonly GpioController _gpio;

        private CancellationTokenSource? _effectCts;
        private DateTime _lastButtonEdge = DateTime.MinValue;
        private bool _started;

        public PiGpioService(PiGpioServiceConfig cfg, GpioController? controller = null)
        {
            _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            _gpio = controller ?? new GpioController(_cfg.Numbering);
        }

        public void Start()
        {
            if (_started) return;

            // LEDs
            OpenOut(_cfg.RedLedPin);
            OpenOut(_cfg.AmberLedPin);
            OpenOut(_cfg.GreenLedPin);

            // Buzzer (active buzzer: HIGH = sound, LOW = silent for most modules)
            OpenOut(_cfg.ActiveBuzzerPin);

            // Button
            var pinMode = _cfg.ButtonPullUp ? PinMode.InputPullUp : PinMode.InputPullDown;
            if (!_gpio.IsPinOpen(_cfg.ButtonPin))
                _gpio.OpenPin(_cfg.ButtonPin, pinMode);
            else
                _gpio.SetPinMode(_cfg.ButtonPin, pinMode);

            // Watch both edges so we can debounce simply; trigger action on "pressed"
            var pressedEdge = _cfg.ButtonPullUp ? PinEventTypes.Falling : PinEventTypes.Rising;

            _gpio.RegisterCallbackForPinValueChangedEvent(_cfg.ButtonPin, PinEventTypes.Rising | PinEventTypes.Falling, (pin, args) =>
            {
                // Software debounce
                var now = DateTime.UtcNow;
                if (now - _lastButtonEdge < _cfg.Debounce) return;
                _lastButtonEdge = now;

                if (args.ChangeType == pressedEdge)
                {
                    // Cancel any current effect and start a fresh one
                    _effectCts?.Cancel();
                    _effectCts?.Dispose();
                    _effectCts = new CancellationTokenSource();

                    _ = RunButtonEffectAsync(_effectCts.Token);
                }
            });

            // Ensure everything starts off
            SetLedStates(false, false, false);
            SetBuzzer(false);

            _started = true;
        }

        public void Stop()
        {
            if (!_started) return;

            _effectCts?.Cancel();
            _effectCts?.Dispose();
            _effectCts = null;

            // Turn everything off
            SetLedStates(false, false, false);
            SetBuzzer(false);

            // Unregister callbacks & close pins
            try { _gpio.UnregisterCallbackForPinValueChangedEvent(_cfg.ButtonPin, null); } catch { /* ignore */ }

            SafeClose(_cfg.RedLedPin);
            SafeClose(_cfg.AmberLedPin);
            SafeClose(_cfg.GreenLedPin);
            SafeClose(_cfg.ActiveBuzzerPin);
            SafeClose(_cfg.ButtonPin);

            _started = false;
        }

        public void SetLedStates(bool red, bool amber, bool green)
        {
            _gpio.Write(_cfg.RedLedPin, red ? PinValue.High : PinValue.Low);
            _gpio.Write(_cfg.AmberLedPin, amber ? PinValue.High : PinValue.Low);
            _gpio.Write(_cfg.GreenLedPin, green ? PinValue.High : PinValue.Low);
        }

        public async Task PlayBuzzerAsync(TimeSpan duration, TimeSpan on, TimeSpan off, CancellationToken ct = default)
        {
            var deadline = DateTime.UtcNow + duration;
            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                SetBuzzer(true);
                await Task.Delay(on, ct).ConfigureAwait(false);
                SetBuzzer(false);
                if (off > TimeSpan.Zero)
                    await Task.Delay(off, ct).ConfigureAwait(false);
            }
            SetBuzzer(false);
        }

        public async Task LedDanceAsync(TimeSpan duration, CancellationToken ct = default)
        {
            // Simple pattern: chase + blink all
            var deadline = DateTime.UtcNow + duration;
            while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
            {
                // chase
                SetLedStates(true, false, false); await Task.Delay(120, ct);
                SetLedStates(false, true, false); await Task.Delay(120, ct);
                SetLedStates(false, false, true); await Task.Delay(120, ct);

                // bounce
                SetLedStates(false, true, false); await Task.Delay(120, ct);

                // all flash
                SetLedStates(true, true, true); await Task.Delay(160, ct);
                SetLedStates(false, false, false); await Task.Delay(100, ct);
            }

            SetLedStates(false, false, false);
        }

        public void RegisterTransceiver(object transceiver)
        {
            // Keep simple for now. Later you can accept an interface, e.g. IRadio or ISpiDevice wrapper.
            // This method exists to “reserve” the surface area and avoid breaking changes later.
            // Example future use: store and configure SPI/UART/I2C device here.
            _ = transceiver ?? throw new ArgumentNullException(nameof(transceiver));
        }

        public void Dispose()
        {
            Stop();
            _gpio?.Dispose();
        }

        // ===== Internals =====

        private async Task RunButtonEffectAsync(CancellationToken ct)
        {
            var t1 = LedDanceAsync(_cfg.ButtonEffectDuration, ct);
            var t2 = PlayBuzzerAsync(_cfg.ButtonEffectDuration, _cfg.BuzzerOn, _cfg.BuzzerOff, ct);

            try
            {
                await Task.WhenAll(t1, t2).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { /* expected on cancel */ }
            finally
            {
                // ensure all off
                SetLedStates(false, false, false);
                SetBuzzer(false);
            }
        }

        private void OpenOut(int pin)
        {
            if (!_gpio.IsPinOpen(pin)) _gpio.OpenPin(pin, PinMode.Output);
            else _gpio.SetPinMode(pin, PinMode.Output);
            _gpio.Write(pin, PinValue.Low);
        }

        private void SetBuzzer(bool on) => _gpio.Write(_cfg.ActiveBuzzerPin, on ? PinValue.High : PinValue.Low);

        private void SafeClose(int pin)
        {
            if (_gpio.IsPinOpen(pin))
            {
                try { _gpio.Write(pin, PinValue.Low); } catch { /* ignore */ }
                try { _gpio.ClosePin(pin); } catch { /* ignore */ }
            }
        }
    }
}
