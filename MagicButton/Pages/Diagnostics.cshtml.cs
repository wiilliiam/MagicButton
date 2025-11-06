using MagicButton.Data;
using MagicButton.Data.Models;
using MagicButton.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Device.Gpio;

namespace MagicButton.Pages
{
    public class DiagnosticsModel : PageModel
    {

        private readonly MagicDbContext _context;
        private readonly BackgroundJobService _bj;
        public DiagnosticsModel(MagicDbContext context, BackgroundJobService bj)
        {                    
            _context = context;
            _bj = bj;
        }


        public int GreenPin { get; set; }
        public int RedPin { get; set; }
        public async Task OnGetAsync()
        {

            var greenPin = await _context.Leds.Where(x => x.Color == LedColor.Green).FirstOrDefaultAsync();
            if (greenPin != null)
            {
                GreenPin = greenPin.Pin;

            }

            var redPin = await _context.Leds.Where(x => x.Color == LedColor.Red).FirstOrDefaultAsync();
            if (redPin != null)
            {
                RedPin = redPin.Pin;

            }
        }

        public async Task OnGetApiTestAsync()
        {

            var device = await _context.DeviceConfigs.Include(x=>x.Actions).FirstOrDefaultAsync();
            if (device != null)
            {
                // Log or perform actions with the device
                Console.WriteLine($"API Test for Device ID: {device.DeviceId}");
                await _bj.HandleSinglePressAsync(device, _context, CancellationToken.None);
               
            }

            await OnGetAsync();
        }

    }
}
