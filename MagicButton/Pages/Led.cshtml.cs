using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MagicButton.Data;
using MagicButton.Data.Models;

namespace MagicButton.Pages
{
    public class LedModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public LedModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Led Led { get; set; } = default!;
        public SelectList LedColours { get; set; }
        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            ViewData["DeviceConfigId"] = new SelectList(_context.DeviceConfigs, "Id", "DeviceId");

            var allColours = Enum.GetValues(typeof(LedColor)).Cast<LedColor>().ToList();   


            LedColours = new SelectList(allColours);


            if (id == null)
            {
                // CREATE mode
                Led = new Led
                {
                    // set any sensible defaults here if you want
                };

                var exitingColours = await _context.Leds.Select(x => x.Color).ToListAsync();

                foreach (var colour in exitingColours)
                {
                    allColours.Remove(colour);
                }

                return Page();
            }

            // EDIT mode

            var led = await _context.Leds.FindAsync(id);
            Led = led;

            
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["DeviceConfigId"] = new SelectList(_context.DeviceConfigs, "Id", "DeviceId");

            var allColours = Enum.GetValues(typeof(LedColor)).Cast<LedColor>().ToList();



            if (!ModelState.IsValid)
            {
                LedColours = new SelectList(allColours);
                return Page();
            }
            var isNew = Led.Id == Guid.Empty || !LedExists(Led.Id);
            if (isNew)
            {
                if (Led.Id == Guid.Empty)
                    Led.Id = Guid.NewGuid();

                var existingPins = await _context.Leds.Select(x => x.Pin).ToListAsync();

                if (existingPins.Any(x => x == Led.Pin))
                {
                    // Duplicate pin
                    TempData["ErrorMessage"] = "Sorry but that GPIO Pin has already been added.";
                    LedColours = new SelectList(allColours);
                    return Page();
                }


                _context.Leds.Add(Led);
            }
            else
            {
                _context.Attach(Led).State = EntityState.Modified;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LedExists(Led.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (isNew)
            {
                TempData["SuccessMessage"] = "LED created successfully. Would you like to add another one just now?";
                LedColours = new SelectList(allColours);
                return RedirectToPage("./Led");
            }

            return RedirectToPage("./Index");
        }

        private bool LedExists(Guid id)
        {
            return _context.Leds.Any(e => e.Id == id);
        }
    }
}
