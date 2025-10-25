using System.Collections.Concurrent;
using System.Device.Gpio;

public interface IGpioService : IDisposable
{
    void Set(int pin, bool high);
    void EnsureOutput(int pin);
}

public sealed class GpioService : IGpioService
{
    private readonly GpioController _gpio;
    private readonly ConcurrentDictionary<int, byte> _opened = new(); // value unused; we just track
    private readonly ILogger<GpioService> _logger;
    public GpioService(ILogger<GpioService> logger)
    {

        _logger = logger;
        try
        {
            _gpio = new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize GPIO controller. Are you running on a compatible platform with GPIO support?");
        }

        _logger = logger;
    }
    public void EnsureOutput(int pin)
    {
        if (_opened.TryAdd(pin, 0))
        {
            if (!_gpio.IsPinOpen(pin))
            {
                _gpio.OpenPin(pin, PinMode.Output);
                _gpio.Write(pin, PinValue.Low);
            }
        }
    }

    public void Set(int pin, bool high)
    {
        EnsureOutput(pin);
        _gpio.Write(pin, high ? PinValue.High : PinValue.Low);
    }

    public void Dispose()
    {
        foreach (var pin in _opened.Keys)
        {
            try
            {
                _gpio.Write(pin, PinValue.Low);
                _gpio.ClosePin(pin);
            }
            catch { /* swallow on shutdown */ }
        }
        _gpio.Dispose();
    }
}
