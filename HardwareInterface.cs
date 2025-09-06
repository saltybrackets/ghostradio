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
        
        // Back to spidev0.1 since 0.0 doesn't exist
        var spiSettings = new SpiConnectionSettings(0, 1)
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
        // Try raw SPI communication instead of using MCP3008 library
        var rawValue = ReadMcp3008Channel(channel);
        Console.WriteLine($"DEBUG: Channel {channel} raw value: {rawValue} (raw SPI)");
        
        return (rawValue / 1023.0) * 100.0;
    }
    
    private int ReadMcp3008Channel(int channel)
    {
        // MCP3008 SPI protocol: send 3 bytes, get 3 bytes back
        // Start bit (1) + SGL/DIFF (1 for single-ended) + D2 D1 D0 (channel) + don't care bits
        var command = new byte[3];
        command[0] = 0x01; // Start bit
        command[1] = (byte)(0x80 | (channel << 4)); // Single-ended + channel
        command[2] = 0x00; // Don't care
        
        var response = new byte[3];
        _spiDevice.TransferFullDuplex(command, response);
        
        // Extract 10-bit result from response[1] and response[2]
        var result = ((response[1] & 0x03) << 8) | response[2];
        return result;
    }

    public double ReadInvertedAnalogPercentage(int channel)
    {
        return 100.0 - ReadAnalogPercentage(channel);
    }

    public double ReadVolumePercentage()
    {
        var rawValue = ReadMcp3008Channel(VolumeChannel);
        Console.WriteLine($"DEBUG: Volume channel raw value: {rawValue} (raw SPI)");
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