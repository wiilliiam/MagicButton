namespace MagicButton.Data.Models
{
    // One row per “function” (Single/Double/Long), with optional overrides
    public sealed class ActionConfig
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeviceConfigId { get; set; }
        public DeviceConfig DeviceConfig { get; set; } = default!;
        public PressKind Kind { get; set; }

        // If null/empty, fall back to device defaults
        public RequestMethod? Method { get; set; }
        public string? Url { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
        public Dictionary<string, object>? ExtraPayload { get; set; }
        public int? RetriesMaxAttempts { get; set; }
        public int? RetriesBaseDelayMs { get; set; }
        public bool? RetriesJitter { get; set; }
    }
}
