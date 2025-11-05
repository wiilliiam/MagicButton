using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MagicButton.Data;
using MagicButton.Data.Models;

namespace MagicButton.Pages
{
    public class PressModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public PressModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        public ButtonPress ButtonPress { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var buttonpress = await _context.ButtonPresses.FirstOrDefaultAsync(m => m.Id == id);

            if (buttonpress is not null)
            {
                ButtonPress = buttonpress;

                return Page();
            }

            return NotFound();
        }
    }
}
