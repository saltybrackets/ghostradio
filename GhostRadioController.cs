namespace GhostRadio;

public class GhostRadioController(
    IHardwareInterface hardware,
    IAudioPlayer audioPlayer,
    RadioStationMap radioStations,
    string staticFilePath,
    int updateIntervalMs,
    bool verboseLogging = false)
{
    private bool _powerState = false;
    private string? _currentStationUrl = null;
    private double _lastTunerValue = 0;
    private double _lastVolumeValue = 0;

    public string? CurrentStationUrl => _currentStationUrl;
    public bool PowerState => _powerState;
    public double TunerPosition => _lastTunerValue;
    public double VolumeLevel => _lastVolumeValue;
    public string? CurrentTrackTitle => audioPlayer.CurrentTrackTitle;
    public string? CurrentTrackArtist => audioPlayer.CurrentTrackArtist;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Read hardware inputs
                bool powerSwitch = hardware.ReadPowerSwitch();
                double tunerValue = hardware.ReadTunerPercentage();
                double volumeValue = hardware.ReadVolumePercentage();

                // Cache values for web UI
                _lastTunerValue = tunerValue;
                _lastVolumeValue = volumeValue;

                // Handle power state changes
                if (powerSwitch != _powerState)
                {
                    _powerState = powerSwitch;
                    
                    if (_powerState)
                    {
                        Console.WriteLine("Power ON");
                    }
                    else
                    {
                        Console.WriteLine("Power OFF");
                        audioPlayer.Stop();
                        _currentStationUrl = string.Empty;
                    }
                }

                if (_powerState)
                {
                    // Update volume
                    audioPlayer.SetVolume(volumeValue);

                    // Handle station tuning
                    string stationUrl = radioStations.GetStationUrl(tunerValue);
                    
                    if (!string.IsNullOrEmpty(stationUrl))
                    {
                        // Found a station
                        if (_currentStationUrl != stationUrl)
                        {
                            Console.WriteLine($"\nTuning to station: {stationUrl}");
                            audioPlayer.PlayStreamingAudio(stationUrl);
                            _currentStationUrl = stationUrl;
                        }
                    }
                    else
                    {
                        // No station found, play static
                        if (_currentStationUrl != staticFilePath)
                        {
                            Console.WriteLine($"\nNo station matched. Playing static.");
                            audioPlayer.PlayLocalAudio(staticFilePath, loop: true);
                            _currentStationUrl = staticFilePath;
                        }
                    }

                    // Optional: Log current state periodically
                    if (verboseLogging
                        && DateTime.Now.Millisecond % 1000 < updateIntervalMs)
                    {
                        RadioStation? station = radioStations.GetStation(tunerValue);
                        string stationInfo = station != null ? $"Station: {station.Url}" : "Static";
                        Console.Write($"\rPower: {_powerState}  Tuner: {tunerValue:F1}  Volume: {volumeValue:F1}  {stationInfo}");
                    }
                }

                await Task.Delay(updateIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in main loop: {ex.Message}");
                await Task.Delay(1000, cancellationToken); // Wait before retrying
            }
        }
        
        audioPlayer.Stop();
    }
}
