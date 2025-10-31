using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Adc;

namespace GhostRadio;

public class HardwareInterface : IHardwareInterface
{
    private readonly GpioController _gpio;
    private readonly Mcp3008 _adc;
    private readonly SpiDevice _spiDevice;
    private bool _disposed = false;

    // GPIO pins
    private const int PowerSwitchPin = 17;
    
    // Software SPI pins (standard SPI0 pins)
    private const int SpiClockPin = 11;   // SCLK
    private const int SpiMosiPin = 10;    // MOSI  
    private const int SpiMisoPin = 9;     // MISO
    private const int SpiChipSelectPin = 8; // CE0
    
    // MCP3008 channels
    private const int TunerChannel = 0;
    private const int VolumeChannel = 1;

    public HardwareInterface()
    {
        _gpio = new GpioController();
        
        // Initialize power switch pin
        _gpio.OpenPin(PowerSwitchPin, PinMode.InputPullUp);
        
        // Initialize software SPI pins
        _gpio.OpenPin(SpiClockPin, PinMode.Output);
        _gpio.OpenPin(SpiMosiPin, PinMode.Output);
        _gpio.OpenPin(SpiMisoPin, PinMode.Input);
        _gpio.OpenPin(SpiChipSelectPin, PinMode.Output);
        
        // Set initial states
        _gpio.Write(SpiClockPin, PinValue.Low);
        _gpio.Write(SpiMosiPin, PinValue.Low);
        _gpio.Write(SpiChipSelectPin, PinValue.High); // Chip select is active low
        
        Console.WriteLine("Hardware interface initialized (software SPI mode)");
        
        // Note: _adc and _spiDevice are not used in software SPI mode
        _adc = null!;
        _spiDevice = null!;
    }

    /// <summary>
    /// Return that state of the power switch.
    /// </summary>
    /// <returns>True if on, false if off.</returns>
    public bool ReadPowerSwitch()
    {
        return _gpio.Read(PowerSwitchPin) == PinValue.Low; // Active low with pull-up
    }

    /// <summary>
    /// Read the value of an analog signal as a percentae between its min and max values.
    /// </summary>
    /// <param name="channel">MCP3008 channel to read signal from.</param>
    /// <returns>Signal value as a percentage.</returns>
    public double ReadAnalogPercentage(int channel)
    {
        int rawValue = ReadMcp3008Channel(channel);
        return (rawValue / 1023.0) * 100.0;
    }
    
    /// <summary>
    /// Get the current value of the volume signal as a percentage between its min and max values.
    /// "Normalizes" this value to represent more closely what the human ear would expect.
    /// </summary>
    /// <returns>Volume value as a percentage.</returns>
    public double ReadVolumePercentage()
    {
        int rawValue = ReadMcp3008Channel(VolumeChannel);
        double normalizedValue = rawValue / 1023.0;
        double linearVolume = normalizedValue * 100.0;

        if (linearVolume >= 90.0)
        {
            return linearVolume;
        }
        
        double scaled = Math.Pow(linearVolume / 90.0, 0.15) * 90.0;
        return scaled < 30 ? 0.0 : scaled;
    }

    /// <summary>
    /// Get the current value of the tuner signal as a percentage between its min and max values.
    /// </summary>
    /// <returns>Tuner value as a percentage.</returns>
    public double ReadTunerPercentage()
    {
        // Note: This is inverted because the tuner had to be installed upside down.
        return 100.0 - ReadAnalogPercentage(TunerChannel);
    }

    public void Dispose()
    {
        if (_disposed) 
            return;
        
        _gpio?.Dispose();
        _spiDevice?.Dispose();
        _adc?.Dispose();
        _disposed = true;
    }
    
    private int ReadMcp3008Channel(int channel)
    {
        // Software SPI implementation matching gpiozero behavior
        _gpio.Write(SpiChipSelectPin, PinValue.Low); // Start transaction
        
        // Send command: start bit + single-ended + channel
        int command = 0x18 | channel; // 11000 + 3-bit channel (0x18 = 24 = 11000 binary)
        
        int result = 0;
        
        // Send 5 command bits
        for (int i = 4; i >= 0; i--)
        {
            _gpio.Write(SpiMosiPin, (command & (1 << i)) != 0 ? PinValue.High : PinValue.Low);
            _gpio.Write(SpiClockPin, PinValue.High);
            Thread.Sleep(1); // Small delay for signal stability
            _gpio.Write(SpiClockPin, PinValue.Low);
            Thread.Sleep(1);
        }
        
        // Read 12 result bits (ignore first null bit, then 10 data bits + 1 extra)
        for (int i = 11; i >= 0; i--)
        {
            _gpio.Write(SpiClockPin, PinValue.High);
            Thread.Sleep(1);
            if (i < 10 && _gpio.Read(SpiMisoPin) == PinValue.High) // Only read data bits 9-0
            {
                result |= 1 << i;
            }
            _gpio.Write(SpiClockPin, PinValue.Low);
            Thread.Sleep(1);
        }
        
        _gpio.Write(SpiChipSelectPin, PinValue.High); // End transaction
        
        return result;
    }
}