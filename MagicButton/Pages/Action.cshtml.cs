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
using System.Text.Json;

namespace MagicButton.Pages
{
    public class ActionModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public ActionModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ActionConfig ActionConfig { get; set; } = default!;
        private async Task PopulateDeviceConfigsAsync()
        {
            ViewData["DeviceConfigId"] = new SelectList(
                await _context.DeviceConfigs
                    .OrderBy(d => d.DeviceId) // or whatever display you prefer
                    .ToListAsync(),
                "Id",
                "DeviceId"
            );
        }
        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateDeviceConfigsAsync();
            
            var id = await _context.ActionConfigs.FirstOrDefaultAsync();

            if (id == null)
            {
                // CREATE mode
                ActionConfig = new ActionConfig
                {
                    // set any sensible defaults here if you want
                };
                return Page();
            }

            // EDIT mode
            ActionConfig = id;

      


            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateDeviceConfigsAsync();
                return Page();
            }

            var isNew = ActionConfig.Id == Guid.Empty || !ActionConfigExists(ActionConfig.Id);

            if (isNew)
            {
                if (ActionConfig.Id == Guid.Empty)
                    ActionConfig.Id = Guid.NewGuid();


                _context.ActionConfigs.Add(ActionConfig);
            }
            else
            {
                // Update path
                _context.Attach(ActionConfig).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActionConfigExists(ActionConfig.Id))
                    return NotFound();

                throw;
            }

            return RedirectToPage("./Index");
        }

        private bool ActionConfigExists(Guid id)
        {
            return _context.ActionConfigs.Any(e => e.Id == id);
        }
    }
}
