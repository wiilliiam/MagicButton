namespace MagicButton.Data.Models
{
    // Map HTTP response codes (or ranges / transport errors) to a LED + pattern
    public sealed class ResponseMapping
    {
        public long Id { get; set; }

        public Guid DeviceConfigId { get; set; }
        public DeviceConfig DeviceConfig { get; set; } = default!;
        public Guid LedId { get; set; }
        public Led Led { get; set; } = default!;

        // Either match a range, OR set TransportError=true (e.g., timeout/DNS)
        public int? CodeStart { get; set; }     // inclusive
        public int? CodeEnd { get; set; }       // inclusive
        public bool TransportError { get; set; } // network/exception, no HTTP code

        public LedPattern Pattern { get; set; } = LedPattern.Solid;
        public int DurationMs { get; set; } = 2000; // how long to show pattern
        public int Priority { get; set; } = 0;      // higher wins if multiple match
    }
}
