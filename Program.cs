using GhostRadio.Audio;
using GhostRadio.Hardware;
using GhostRadio.Services;

namespace GhostRadio;

public class Program
{
    public const string StaticFile = "static.wav";
    public const int UpdateIntervalMs = 50;
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("GhostRadio starting...");
        
        using var hardware = new HardwareInterface();
        using var audioPlayer = new AudioPlayer();
        var stationService = new StationService();
        
        var ghostRadio = new GhostRadioController(hardware, audioPlayer, stationService, StaticFile, UpdateIntervalMs);
        
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