namespace MagicButton.Data.Models
{
    // Each button press event (single/double/long)
    public sealed class ButtonPress
    {
        public long Id { get; set; }
        public Guid DeviceConfigId { get; set; }
        public DeviceConfig DeviceConfig { get; set; } = default!;
        public Guid? ActionConfigId { get; set; }
        public ActionConfig? ActionConfig { get; set; }

        public string DeviceId { get; set; } = default!;
        public PressKind Kind { get; set; } = PressKind.Single;

        public DateTime PressedAtUtc { get; set; } = DateTime.Now;
        public bool Simulated { get; set; }

        public string PayloadJson { get; set; } = "{}";
        public int AttemptCount { get; set; } = 1;
        public bool Queued { get; set; } = false;

        public bool Success { get; set; }
        public int? ResponseStatusCode { get; set; }
        public bool TransportError { get; set; }   // true if no HTTP status
        public int DurationMs { get; set; }        // request round-trip

        public Guid? LedId { get; set; }           // LED chosen by mapping
        public Led? Led { get; set; }

        public LedPattern? LedPattern { get; set; } // pattern used
        public int? LedDurationMs { get; set; }

        public string? ResponseBodySnippet { get; set; }
        public string? Error { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.Now;
    }
}
