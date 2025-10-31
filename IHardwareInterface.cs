namespace GhostRadio;

public interface IHardwareInterface : IDisposable
{
    bool ReadPowerSwitch();
    double ReadVolumePercentage();
    double ReadTunerPercentage();
}
