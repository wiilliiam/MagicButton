
using MagicButton.Data;
using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("Connection String is missing!");
builder.Services.AddDbContext<MagicDbContext>(opt => opt.UseSqlite($"Data Source={connectionString}"));


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient("magic");

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

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
