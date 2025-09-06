using GhostRadio.Audio;
using GhostRadio.Hardware;
using GhostRadio.Services;

namespace GhostRadio;

public class GhostRadioController
{
    private readonly HardwareInterface _hardware;
    private readonly AudioPlayer _audioPlayer;
    private readonly StationService _stationService;
    private readonly string _staticFile;
    private readonly int _updateIntervalMs;
    
    private bool _powerState = false;
    private string? _currentStationUrl = null;

    public GhostRadioController(
        HardwareInterface hardware, 
        AudioPlayer audioPlayer, 
        StationService stationService,
        string staticFile,
        int updateIntervalMs)
    {
        _hardware = hardware;
        _audioPlayer = audioPlayer;
        _stationService = stationService;
        _staticFile = staticFile;
        _updateIntervalMs = updateIntervalMs;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Read hardware inputs
                var powerSwitch = _hardware.ReadPowerSwitch();
                var tunerValue = _hardware.ReadTunerPercentage();
                var volumeValue = _hardware.ReadVolumePercentage();

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
                        _audioPlayer.Stop();
                        _currentStationUrl = null;
                    }
                }

                if (_powerState)
                {
                    // Update volume
                    _audioPlayer.SetVolume(volumeValue);

                    // Handle station tuning
                    var stationUrl = _stationService.GetStationUrl(tunerValue);
                    
                    if (stationUrl != null)
                    {
                        // Found a station
                        if (_currentStationUrl != stationUrl)
                        {
                            Console.WriteLine($"\nTuning to station: {stationUrl}");
                            _audioPlayer.PlayStation(stationUrl);
                            _currentStationUrl = stationUrl;
                        }
                    }
                    else
                    {
                        // No station found, play static
                        if (_currentStationUrl != _staticFile)
                        {
                            Console.WriteLine($"\nNo station matched. Playing static.");
                            _audioPlayer.PlayStaticFile(_staticFile);
                            _currentStationUrl = _staticFile;
                        }
                    }

                    // Optional: Log current state periodically
                    if (DateTime.Now.Millisecond % 1000 < _updateIntervalMs)
                    {
                        var station = _stationService.GetStation(tunerValue);
                        var stationInfo = station != null ? $"Station: {station.Url}" : "Static";
                        Console.Write($"\rPower: {_powerState}  Tuner: {tunerValue:6.1f}  Volume: {volumeValue:6.1f}  {stationInfo}");
                    }
                }

                await Task.Delay(_updateIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(); // New line before exit
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in main loop: {ex.Message}");
                await Task.Delay(1000, cancellationToken); // Wait before retrying
            }
        }
        
        _audioPlayer.Stop();
    }
}