using MagicButton.Data;
using MagicButton.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Device.Gpio;
namespace MagicButton.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly MagicDbContext _context;
        public IndexModel(ILogger<IndexModel> logger, MagicDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public bool isFirstRun { get; set; }

        public async Task OnGetAsync()
        {
            isFirstRun = await _context.DeviceConfigs.AnyAsync() == false;
        }


        public static async Task LetsParty()
        {

            // choose your numbering scheme & pins
            var cfg = new PiIo.PiGpioServiceConfig
            {
                Numbering = PinNumberingScheme.Logical,   // or Board if you prefer
                RedLedPin = 27,
                AmberLedPin = 22,
                GreenLedPin = 17,
                ActiveBuzzerPin = 5,
                ButtonPin = 6,
                ButtonPullUp = true,                      // button to GND
                Debounce = TimeSpan.FromMilliseconds(40),
                ButtonEffectDuration = TimeSpan.FromSeconds(10),
                BuzzerOn = TimeSpan.FromMilliseconds(200),
                BuzzerOff = TimeSpan.FromMilliseconds(200)
            };

            using var svc = new PiIo.PiGpioService(cfg);
            svc.Start();

            var ledTask = svc.LedDanceAsync(TimeSpan.FromSeconds(30));
            var buzzerTask = svc.PlayBuzzerAsync(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));
            await Task.WhenAll(ledTask, buzzerTask);
        }
    }
}
