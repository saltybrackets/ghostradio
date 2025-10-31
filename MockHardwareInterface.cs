namespace GhostRadio;

public class MockHardwareInterface : IHardwareInterface
{
    private bool _powerState = true;
    private double _tunerValue = 50.0;
    private double _volumeValue = 75.0;
    private readonly Random _random = new Random();
    private readonly Timer? _updateTimer;
    private bool _disposed = false;
    private bool _autoMode = false; // Auto-change mode disabled by default

    public MockHardwareInterface(bool autoMode = false)
    {
        Console.WriteLine("Mock hardware interface initialized");
        _autoMode = autoMode;

        if (_autoMode)
        {
            Console.WriteLine("Auto-mode enabled: Simulating radio hardware with slowly changing values...");
            _updateTimer = new Timer(UpdateValues, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }
        else
        {
            Console.WriteLine("Manual mode: Use web interface to control hardware values");
        }
    }

    // Public methods for manual control
    public void SetPower(bool powerState)
    {
        _powerState = powerState;
    }

    public void SetTuner(double tunerValue)
    {
        _tunerValue = Math.Clamp(tunerValue, 0.0, 100.0);
    }

    public void SetVolume(double volumeValue)
    {
        _volumeValue = Math.Clamp(volumeValue, 0.0, 100.0);
    }

    public bool ReadPowerSwitch()
    {
        return _powerState;
    }

    public double ReadVolumePercentage()
    {
        return _volumeValue;
    }

    public double ReadTunerPercentage()
    {
        return _tunerValue;
    }

    private void UpdateValues(object? state)
    {
        if (!_autoMode) return;

        // Randomly adjust tuner (simulate slow turning)
        double tunerChange = (_random.NextDouble() - 0.5) * 2.0; // -1 to +1
        _tunerValue = Math.Clamp(_tunerValue + tunerChange, 0.0, 100.0);

        // Randomly adjust volume (simulate slow turning)
        double volumeChange = (_random.NextDouble() - 0.5) * 3.0; // -1.5 to +1.5
        _volumeValue = Math.Clamp(_volumeValue + volumeChange, 0.0, 100.0);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _updateTimer?.Dispose();
        _disposed = true;
    }
}
