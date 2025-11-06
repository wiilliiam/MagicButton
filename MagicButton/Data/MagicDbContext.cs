using MagicButton.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace MagicButton.Data
{
    public sealed class MagicDbContext : DbContext
    {
        public DbSet<DeviceConfig> DeviceConfigs => Set<DeviceConfig>();
        public DbSet<ActionConfig> ActionConfigs => Set<ActionConfig>();
        public DbSet<Led> Leds => Set<Led>();
        public DbSet<ResponseMapping> ResponseMappings => Set<ResponseMapping>();
        public DbSet<ButtonPress> ButtonPresses => Set<ButtonPress>();

        public MagicDbContext(DbContextOptions<MagicDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder b)
        {
     

            // DeviceConfig
            var cfg = b.Entity<DeviceConfig>();
            cfg.ToTable("device_config");
            cfg.HasKey(x => x.Id);
            cfg.HasIndex(x => x.DeviceId).IsUnique();
            cfg.Property(x => x.DeviceId).IsRequired().HasMaxLength(100);

            cfg.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            cfg.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ActionConfig
            var act = b.Entity<ActionConfig>();
            act.ToTable("action_config");
            act.HasKey(x => x.Id);
            act.HasIndex(x => new { x.DeviceConfigId, x.Kind }).IsUnique(); // 1 row per kind
            act.Property(x => x.Url).HasMaxLength(1000);
            act.HasOne(x => x.DeviceConfig).WithMany(x => x.Actions)
               .HasForeignKey(x => x.DeviceConfigId).OnDelete(DeleteBehavior.Cascade);

            // Led
            var led = b.Entity<Led>();
            led.ToTable("led");
            led.HasKey(x => x.Id);
            led.HasIndex(x => new { x.DeviceConfigId, x.Pin }).IsUnique(); // pin unique per device
            led.Property(x => x.Name).HasMaxLength(100);
            led.HasOne(x => x.DeviceConfig).WithMany(x => x.Leds)
               .HasForeignKey(x => x.DeviceConfigId).OnDelete(DeleteBehavior.Cascade);

            // ResponseMapping
            var map = b.Entity<ResponseMapping>();
            map.ToTable("response_mapping");
            map.HasKey(x => x.Id);
            map.HasIndex(x => x.DeviceConfigId);
            map.HasIndex(x => new { x.DeviceConfigId, x.TransportError, x.CodeStart, x.CodeEnd, x.Priority });
            map.Property(x => x.DurationMs).HasDefaultValue(2000);
            map.HasOne(x => x.DeviceConfig).WithMany(x => x.ResponseMappings)
               .HasForeignKey(x => x.DeviceConfigId).OnDelete(DeleteBehavior.Cascade);
            map.HasOne(x => x.Led).WithMany().HasForeignKey(x => x.LedId).OnDelete(DeleteBehavior.Restrict);

            // ButtonPress
            var press = b.Entity<ButtonPress>();
            press.ToTable("button_press");
            press.HasKey(x => x.Id);
            press.Property(x => x.Id).ValueGeneratedOnAdd();
            press.HasIndex(x => new { x.DeviceId, x.PressedAtUtc });
            press.HasIndex(x => x.PressedAtUtc);
            press.Property(x => x.ResponseBodySnippet).HasMaxLength(2000);
            press.Property(x => x.Error).HasMaxLength(500);
            press.Property(x => x.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            press.HasOne(x => x.DeviceConfig).WithMany(x => x.Presses)
                 .HasForeignKey(x => x.DeviceConfigId).OnDelete(DeleteBehavior.Cascade);
            press.HasOne(x => x.ActionConfig).WithMany().HasForeignKey(x => x.ActionConfigId).OnDelete(DeleteBehavior.SetNull);
            press.HasOne(x => x.Led).WithMany().HasForeignKey(x => x.LedId).OnDelete(DeleteBehavior.SetNull);
        }

        public override int SaveChanges()
        {
            TouchTimestamps();
            return base.SaveChanges();
        }
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            TouchTimestamps();
            return base.SaveChangesAsync(ct);
        }
        void TouchTimestamps()
        {
            var now = DateTime.Now;
            foreach (var e in ChangeTracker.Entries<DeviceConfig>())
            {
                if (e.State == EntityState.Added) e.Entity.CreatedAtUtc = now;
                if (e.State is EntityState.Added or EntityState.Modified) e.Entity.UpdatedAtUtc = now;
            }
            foreach (var e in ChangeTracker.Entries<ButtonPress>())
            {
                if (e.State == EntityState.Added) e.Entity.CreatedAtUtc = now;
            }
        }
    }
}
