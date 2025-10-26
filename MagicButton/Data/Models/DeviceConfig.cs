using System.ComponentModel.DataAnnotations;

namespace MagicButton.Data.Models
{
    public sealed class DeviceConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Display(Name = "Button name")]
        public string DeviceId { get; set; } = default!;    // unique, human-readable
        public int ButtonPin { get; set; } = 17;
        public bool ButtonPullUp { get; set; } = true;

        // Press timing (ms)
        public int DebounceMs { get; set; } = 150;
        public int DoublePressWindowMs { get; set; } = 350;     // max gap for double
        public int LongPressThresholdMs { get; set; } = 3000;    // hold >= long press


        public int RetriesMaxAttempts { get; set; } = 5;
        public int RetriesBaseDelayMs { get; set; } = 2000;

        [StringLength(50)]
        public string? Password { get; set; }

        // Audit
        public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.Now;

        // Nav
        public ICollection<ActionConfig> Actions { get; set; } = new List<ActionConfig>();
        public ICollection<Led> Leds { get; set; } = new List<Led>();
        public ICollection<ResponseMapping> ResponseMappings { get; set; } = new List<ResponseMapping>();
        public ICollection<ButtonPress> Presses { get; set; } = new List<ButtonPress>();
    }
}
