namespace GhostRadio.Services;

public class RadioStateService
{
    private readonly GhostRadioController _ghostRadio;

    public RadioStateService(GhostRadioController ghostRadio)
    {
        _ghostRadio = ghostRadio;
    }

    public RadioState GetCurrentState()
    {
        // Use cached values from the controller instead of reading hardware directly
        // This prevents SPI bus contention and race conditions
        return new RadioState
        {
            IsPoweredOn = _ghostRadio.PowerState,
            TunerPosition = _ghostRadio.TunerPosition,
            VolumeLevel = _ghostRadio.VolumeLevel,
            CurrentStationUrl = _ghostRadio.CurrentStationUrl,
            TrackTitle = _ghostRadio.CurrentTrackTitle,
            TrackArtist = _ghostRadio.CurrentTrackArtist
        };
    }
}

public class RadioState
{
    public bool IsPoweredOn { get; set; }
    public double TunerPosition { get; set; }
    public double VolumeLevel { get; set; }
    public string? CurrentStationUrl { get; set; }
    public string? TrackTitle { get; set; }
    public string? TrackArtist { get; set; }
}
