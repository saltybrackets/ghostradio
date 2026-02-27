namespace GhostRadio.Services;

public class RadioStateService
{
    private readonly GhostRadioController _ghostRadio;
    private readonly RadioStationMap _radioStationMap;

    public RadioStateService(GhostRadioController ghostRadio, RadioStationMap radioStationMap)
    {
        _ghostRadio = ghostRadio;
        _radioStationMap = radioStationMap;
    }

    public RadioState GetCurrentState()
    {
        // Use cached values from the controller instead of reading hardware directly
        // This prevents SPI bus contention and race conditions
        var stationUrl = _ghostRadio.CurrentStationUrl ?? "";
        var station = _radioStationMap.GetStationByUrl(stationUrl);

        return new RadioState
        {
            IsPoweredOn = _ghostRadio.PowerState,
            TunerPosition = _ghostRadio.TunerPosition,
            VolumeLevel = _ghostRadio.VolumeLevel,
            CurrentStationUrl = _ghostRadio.CurrentStationUrl,
            CurrentStationTitle = station?.Title,
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
    public string? CurrentStationTitle { get; set; }
    public string? TrackTitle { get; set; }
    public string? TrackArtist { get; set; }
}
