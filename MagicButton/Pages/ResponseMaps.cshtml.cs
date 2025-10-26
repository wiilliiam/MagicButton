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
    public class ResponseMapsModel : PageModel
    {
        private readonly MagicButton.Data.MagicDbContext _context;

        public ResponseMapsModel(MagicButton.Data.MagicDbContext context)
        {
            _context = context;
        }

        public IList<ResponseMapping> ResponseMapping { get;set; } = default!;

        public async Task OnGetAsync()
        {
            ResponseMapping = await _context.ResponseMappings
                .Include(r => r.DeviceConfig)
                .Include(r => r.Led).ToListAsync();
        }
    }
}
