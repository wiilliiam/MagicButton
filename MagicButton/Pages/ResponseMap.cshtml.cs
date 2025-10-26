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
    public class ResponseMapModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public ResponseMapModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ResponseMapping ResponseMapping { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var responsemapping =  await _context.ResponseMappings.FirstOrDefaultAsync(m => m.Id == id);
            if (responsemapping == null)
            {
                return NotFound();
            }
            ResponseMapping = responsemapping;
           ViewData["DeviceConfigId"] = new SelectList(_context.DeviceConfigs, "Id", "DeviceId");
           ViewData["LedId"] = new SelectList(_context.Leds, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(ResponseMapping).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResponseMappingExists(ResponseMapping.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ResponseMappingExists(long id)
        {
            return _context.ResponseMappings.Any(e => e.Id == id);
        }
    }
}
