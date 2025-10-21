namespace MagicButton.Data
{
    public enum RequestMethod { GET, POST }
    public enum PressKind { Single = 0, Double = 1, Long = 2 }
    public enum LedColor { Red = 0, Amber = 1, Green = 2, Blue = 3, White = 4, Custom = 5 }
    public enum LedPattern { Solid = 0, BlinkSlow = 1, BlinkFast = 2, Pulse = 3 }
}
