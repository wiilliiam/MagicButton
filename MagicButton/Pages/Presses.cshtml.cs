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
    public class PressesModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public PressesModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        public IList<ButtonPress> ButtonPress { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ButtonPress = await _context.ButtonPresses
                .Include(b => b.ActionConfig)
                .Include(b => b.DeviceConfig)
                .Include(b => b.Led).ToListAsync();
        }
    }
}
