namespace GhostRadio.Services;

public class RadioStateService
{
    private readonly GhostRadioController _ghostRadio;
    private readonly IHardwareInterface _hardware;

    public RadioStateService(GhostRadioController ghostRadio, IHardwareInterface hardware)
    {
        _ghostRadio = ghostRadio;
        _hardware = hardware;
    }

    public RadioState GetCurrentState()
    {
        bool powerSwitch = _hardware.ReadPowerSwitch();
        double tunerValue = _hardware.ReadTunerPercentage();
        double volumeValue = _hardware.ReadVolumePercentage();

        return new RadioState
        {
            IsPoweredOn = powerSwitch,
            TunerPosition = tunerValue,
            VolumeLevel = volumeValue,
            CurrentStationUrl = _ghostRadio.CurrentStationUrl
        };
    }
}

public class RadioState
{
    public bool IsPoweredOn { get; set; }
    public double TunerPosition { get; set; }
    public double VolumeLevel { get; set; }
    public string? CurrentStationUrl { get; set; }
}
