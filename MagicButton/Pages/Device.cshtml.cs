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
    public class DeviceModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public DeviceModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public DeviceConfig DeviceConfig { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {

            if (id == null)
            {
                // CREATE mode
                DeviceConfig = new DeviceConfig
                {
                    // set any sensible defaults here if you want
                };
                return Page();
            }

            // EDIT mode

            var deviceconfig = await _context.DeviceConfigs.FirstOrDefaultAsync(m => m.Id == id);
            DeviceConfig = deviceconfig;
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

            var isNew = DeviceConfig.Id == Guid.Empty || !DeviceConfigExists(DeviceConfig.Id);
            if (isNew)
            {
                if (DeviceConfig.Id == Guid.Empty)
                    DeviceConfig.Id = Guid.NewGuid();

                _context.DeviceConfigs.Add(DeviceConfig);
            }
            else
            {
                _context.Attach(DeviceConfig).State = EntityState.Modified;
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceConfigExists(DeviceConfig.Id))
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

        private bool DeviceConfigExists(Guid id)
        {
            return _context.DeviceConfigs.Any(e => e.Id == id);
        }
    }
}
