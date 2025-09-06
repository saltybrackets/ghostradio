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
        
        // Initialize SPI for MCP3008
        var spiSettings = new SpiConnectionSettings(0, 1)
        {
            ClockFrequency = 1000000,
            Mode = SpiMode.Mode0
        };
        
        _spiDevice = SpiDevice.Create(spiSettings);
        _adc = new Mcp3008(_spiDevice);
    }

    public bool ReadPowerSwitch()
    {
        return _gpio.Read(PowerSwitchPin) == PinValue.Low; // Active low with pull-up
    }

    public double ReadAnalogPercentage(int channel)
    {
        var rawValue = _adc.Read(channel);
        return (rawValue / 1023.0) * 100.0;
    }

    public double ReadInvertedAnalogPercentage(int channel)
    {
        return 100.0 - ReadAnalogPercentage(channel);
    }

    public double ReadVolumePercentage()
    {
        var rawValue = _adc.Read(VolumeChannel) / 1023.0;
        var linearVolume = rawValue * 100.0;

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