using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Adc;

namespace GhostRadio.Hardware;

public class HardwareInterface : IDisposable
{
    private readonly GpioController _gpio;
    private readonly Mcp3008 _adc;
    private readonly SpiDevice _spiDevice;
    private bool _disposed = false;

    // GPIO pins
    private const int PowerSwitchPin = 17;
    
    // MCP3008 channels
    private const int TunerChannel = 0;
    private const int VolumeChannel = 1;

    public HardwareInterface()
    {
        _gpio = new GpioController();
        
        // Initialize power switch pin
        _gpio.OpenPin(PowerSwitchPin, PinMode.InputPullUp);
        
        // Try SPI device 0.0 instead of 0.1 - maybe hardware uses different CS
        var spiSettings = new SpiConnectionSettings(0, 0)
        {
            ClockFrequency = 500000, // Slower clock - try 500kHz
            Mode = SpiMode.Mode0,
            DataBitLength = 8,
            ChipSelectLineActiveState = PinValue.Low
        };
        
        _spiDevice = SpiDevice.Create(spiSettings);
        Console.WriteLine($"DEBUG: SPI device created: {_spiDevice != null}");
        
        // Initialize MCP3008
        _adc = new Mcp3008(_spiDevice);
        Console.WriteLine($"DEBUG: MCP3008 initialized");
    }

    public bool ReadPowerSwitch()
    {
        return _gpio.Read(PowerSwitchPin) == PinValue.Low; // Active low with pull-up
    }

    public double ReadAnalogPercentage(int channel)
    {
        var rawValue = _adc.Read(channel);
        Console.WriteLine($"DEBUG: Channel {channel} raw value: {rawValue}");
        
        // Test all channels to see if any have data
        for (int i = 0; i < 8; i++)
        {
            var testValue = _adc.Read(i);
            if (testValue > 0)
                Console.WriteLine($"DEBUG: Found non-zero value {testValue} on channel {i}");
        }
        
        return (rawValue / 1023.0) * 100.0;
    }

    public double ReadInvertedAnalogPercentage(int channel)
    {
        return 100.0 - ReadAnalogPercentage(channel);
    }

    public double ReadVolumePercentage()
    {
        var rawValue = _adc.Read(VolumeChannel);
        Console.WriteLine($"DEBUG: Volume channel raw value: {rawValue}");
        var normalizedValue = rawValue / 1023.0;
        var linearVolume = normalizedValue * 100.0;

        if (linearVolume >= 90.0)
        {
            return linearVolume;
        }
        else
        {
            var scaled = Math.Pow(linearVolume / 90.0, 0.15) * 90.0;
            return scaled < 30 ? 0.0 : scaled;
        }
    }

    public double ReadTunerPercentage()
    {
        return ReadAnalogPercentage(TunerChannel);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _gpio?.Dispose();
            _spiDevice?.Dispose();
            _adc?.Dispose();
            _disposed = true;
        }
    }
}