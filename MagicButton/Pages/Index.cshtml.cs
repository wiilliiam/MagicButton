using MagicButton.Data;
using MagicButton.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Device.Gpio;
using System.Diagnostics;
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

        public async Task OnGetCommandAsync(string action)
        {

            switch (action)
            {
                case "reboot":
                    {
                        _ = RunSudoAsync("reboot");
                        _logger.LogWarning("System reboot initiated via web command.");
                        break;
                    }
                case "shutdown":
                    {
                        _ = RunSudoAsync("shutdown", "-h", "now");
                        _logger.LogWarning("System shutdown initiated via web command.");
                        break;
                    }
                case "reset":
                    {
                        // Delete and recreate the database
                        try
                        {
                            await _context.Database.EnsureDeletedAsync();
                            await _context.Database.MigrateAsync();
                            _logger.LogWarning("Database reset initiated via web command.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error during database reset via web command.");
                        }
                        break;
                    }
                default:
                    break;
            }

        }

        private static Task RunSudoAsync(string command, params string[] args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "sudo",
                ArgumentList = { command },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            foreach (var a in args) psi.ArgumentList.Add(a);

            var p = new Process { StartInfo = psi, EnableRaisingEvents = false };
            p.Start();
            return Task.CompletedTask;
        }

    }
}
