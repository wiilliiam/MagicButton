
using MagicButton.Data;
using MagicButton.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text;



var builder = WebApplication.CreateBuilder(args);


bool isDebug()
{
    return builder.Environment.IsDevelopment();

    //|| builder.Configuration.GetValue<bool>("DebugMode");
}
string logPath = "App_Data/magicbutton.log";// isDebug() ? "App_Data/magicbutton.log" : "/var/log/magicbutton.log";
string connectionString = "App_Data/magic.db";//isDebug() ? "App_Data/magic.db" : "/var/lib/magicbutton.db";

builder.Services.AddDbContext<MagicDbContext>(opt => opt.UseSqlite($"Data Source={connectionString}"));

if (!isDebug())
{
   builder.Services.AddHostedService<BackgroundJobService>();
}


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("magic");




builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File(logPath,
                  rollingInterval: RollingInterval.Day,
                  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                  retainedFileCountLimit: 7,
                  shared: true)
    .Enrich.FromLogContext());

builder.Services.AddSingleton<IGpioService, GpioService>();
var app = builder.Build();

// Automatically create db file + apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MagicDbContext>();
    db.Database.Migrate(); // <-- auto migrate
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();
app.UseAntiforgery();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


string logPathWithDateSuffix = logPath.Replace("magicbutton", "magicbutton" + DateTime.Now.ToString("yyyyMMdd"));
app.MapGet("/logs/tail", async (
    IConfiguration cfg,
    int? lines,
    CancellationToken ct) =>
{
    var path = cfg["LogFiles:MagicButton"] ?? logPathWithDateSuffix;
    var max = Math.Clamp(lines ?? 500, 50, 5000);

    var last = await LogTailer.ReadLastLinesAsync(path, max, ct);

    // Return preformatted HTML (safe & lightweight for HTMX)
    var sb = new StringBuilder("<pre style=\"white-space:pre-wrap;margin:0\">");
    foreach (var l in last)
    {
        sb.Append(System.Net.WebUtility.HtmlEncode(l)).Append('\n');
    }
    sb.Append("</pre>");
    return Results.Content(sb.ToString(), "text/html; charset=utf-8");
});

// optional: download full current log
app.MapGet("/logs/download", (IConfiguration cfg) =>
{
    var path = cfg["LogFiles:MagicButton"] ?? logPathWithDateSuffix;
    if (!System.IO.File.Exists(path)) return Results.NotFound("Log file not found.");
    return Results.File(path, "text/plain", "magicbutton.log");
});

app.MapPost("/gpio/on", ([FromForm]int pin, IGpioService gpio) =>
{
    gpio.Set(pin, true);
    return Results.NoContent();
});

app.MapPost("/gpio/off", ([FromForm]int pin, IGpioService gpio) =>
{
    gpio.Set(pin, false);
    return Results.NoContent();
});


app.Run();
