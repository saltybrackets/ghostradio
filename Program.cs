using GhostRadio.Audio;
using GhostRadio.Hardware;
using GhostRadio.Services;

namespace GhostRadio;

class Program
{
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("GhostRadio starting...");
        
        using var hardware = new HardwareInterface();
        using var audioPlayer = new AudioPlayer();
        var stationService = new StationService();
        
        var ghostRadio = new GhostRadioController(hardware, audioPlayer, stationService);
        
        Console.WriteLine("GhostRadio initialized. Press Ctrl+C to exit.");
        
        var cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };
        
        await ghostRadio.RunAsync(cancellationTokenSource.Token);
        
        Console.WriteLine("GhostRadio stopped.");
    }
}

public class GhostRadioController
{
    private const string StaticFile = "static.wav";
    private const int UpdateIntervalMs = 50;
    
    private readonly HardwareInterface _hardware;
    private readonly AudioPlayer _audioPlayer;
    private readonly StationService _stationService;
    
    private bool _powerState = false;
    private string? _currentStationUrl = null;

    public GhostRadioController(
        HardwareInterface hardware, 
        AudioPlayer audioPlayer, 
        StationService stationService)
    {
        _hardware = hardware;
        _audioPlayer = audioPlayer;
        _stationService = stationService;
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
                            Console.WriteLine($"Tuning to station: {stationUrl} (tuner: {tunerValue:F1})");
                            _audioPlayer.PlayStation(stationUrl);
                            _currentStationUrl = stationUrl;
                        }
                    }
                    else
                    {
                        // No station found, play static
                        if (_currentStationUrl != StaticFile)
                        {
                            Console.WriteLine($"Playing static (tuner: {tunerValue:F1})");
                            _audioPlayer.PlayStaticFile(StaticFile);
                            _currentStationUrl = StaticFile;
                        }
                    }

                    // Optional: Log current state periodically
                    if (DateTime.Now.Millisecond % 1000 < UpdateIntervalMs)
                    {
                        var station = _stationService.GetStation(tunerValue);
                        var stationInfo = station != null ? $"Station: {station.Url}" : "Static";
                        Console.WriteLine($"Tuner: {tunerValue:F1}, Volume: {volumeValue:F1}, {stationInfo}");
                    }
                }

                await Task.Delay(UpdateIntervalMs, cancellationToken);
            }
            catch (OperationCanceledException)
            {
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

