using System.ComponentModel.DataAnnotations;

namespace MagicButton.Data.Models
{
    // A physical LED on a GPIO pin
    public sealed class Led
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DeviceConfigId { get; set; }
        public DeviceConfig? DeviceConfig { get; set; } = default!;

        [Required]
        public string? Name { get; set; } 
        public LedColor Color { get; set; }
        public int Pin { get; set; }                // GPIO pin number
        public bool ActiveLow { get; set; } = false;
    }
}
