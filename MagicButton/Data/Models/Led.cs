namespace MagicButton.Data.Models
{
    // A physical LED on a GPIO pin
    public sealed class Led
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeviceConfigId { get; set; }
        public DeviceConfig DeviceConfig { get; set; } = default!;

        public string Name { get; set; } = "LED";
        public LedColor Color { get; set; } = LedColor.Custom;
        public int Pin { get; set; }                // GPIO pin number
        public bool ActiveLow { get; set; } = false;
    }
}
