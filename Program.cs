namespace GhostRadio;

public static class Program
{
    private const string? StaticFile = "static.wav";
    private const int UpdateIntervalMs = 50;
    
    public static async Task Main(string[] args)
    {
        Console.WriteLine("GhostRadio starting...");
        
        // Parse command line arguments
        int? testDurationSeconds = null;
        if (args.Length > 0 && int.TryParse(args[0], out int duration))
        {
            testDurationSeconds = duration;
            Console.WriteLine($"Running in test mode for {duration} seconds...");
        }
        
        using HardwareInterface hardware = new HardwareInterface();
        using AudioPlayer audioPlayer = new AudioPlayer();
        RadioStationMap radioStations = RadioStationMap.Load("stations.json");
        GhostRadioController ghostRadio = new GhostRadioController(
            hardware: hardware, 
            audioPlayer: audioPlayer, 
            radioStations: radioStations, 
            staticFilePath: StaticFile, 
            updateIntervalMs: UpdateIntervalMs);
        
        Console.WriteLine("GhostRadio initialized. Press Ctrl+C to exit.");
        
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };
        
        // Set timeout if test duration is specified
        if (testDurationSeconds.HasValue)
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(testDurationSeconds.Value));
        }
        
        await ghostRadio.RunAsync(cancellationTokenSource.Token);
        
        Console.WriteLine("GhostRadio stopped.");
    }
}