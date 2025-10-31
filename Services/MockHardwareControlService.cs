namespace GhostRadio.Services;

public class MockHardwareControlService
{
    private readonly MockHardwareInterface? _mockHardware;
    public bool IsMockMode { get; }

    public MockHardwareControlService(IHardwareInterface hardware)
    {
        _mockHardware = hardware as MockHardwareInterface;
        IsMockMode = _mockHardware != null;
    }

    public void SetPower(bool powerState)
    {
        _mockHardware?.SetPower(powerState);
    }

    public void SetTuner(double tunerValue)
    {
        _mockHardware?.SetTuner(tunerValue);
    }

    public void SetVolume(double volumeValue)
    {
        _mockHardware?.SetVolume(volumeValue);
    }
}
