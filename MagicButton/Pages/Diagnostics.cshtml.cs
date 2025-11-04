using MagicButton.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Device.Gpio;

namespace MagicButton.Pages
{
    public class DiagnosticsModel : PageModel
    {

        private readonly MagicDbContext _context;

        public DiagnosticsModel(MagicDbContext context)
        {                    
            _context = context;
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


    }
}
