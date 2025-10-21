namespace MagicButton.Data.Models
{
    public sealed class DeviceConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string DeviceId { get; set; } = default!;    // unique, human-readable
        public int ButtonPin { get; set; } = 17;
        public bool ButtonPullUp { get; set; } = true;

        // Press timing (ms)
        public int DebounceMs { get; set; } = 150;
        public int DoublePressWindowMs { get; set; } = 350;     // max gap for double
        public int LongPressThresholdMs { get; set; } = 700;    // hold >= long press

        // Default request options (used if ActionConfig doesn't override)
        public RequestMethod DefaultMethod { get; set; } = RequestMethod.POST;
        public string DefaultUrl { get; set; } = "";            // optional
        public Dictionary<string, string> DefaultHeaders { get; set; } = new();
        public Dictionary<string, object> DefaultExtraPayload { get; set; } = new();
        public int RetriesMaxAttempts { get; set; } = 5;
        public int RetriesBaseDelayMs { get; set; } = 500;
        public bool RetriesJitter { get; set; } = true;
        public bool QueueEnabled { get; set; } = true;
        public string QueuePath { get; set; } = "/var/lib/magicbutton/outbox.db";

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
