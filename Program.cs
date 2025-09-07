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
        bool verboseLogging = false;
        
        foreach (string arg in args)
        {
            if (arg == "--verbose" || arg == "-v")
            {
                verboseLogging = true;
            }
            else if (arg == "--help" || arg == "-h")
            {
                Console.WriteLine("Usage: GhostRadio [options] [duration]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -v, --verbose    Enable verbose logging");
                Console.WriteLine("  -h, --help       Show this help");
                Console.WriteLine("Arguments:");
                Console.WriteLine("  duration         Test duration in seconds (optional)");
                return;
            }
            else if (int.TryParse(arg, out int duration))
            {
                testDurationSeconds = duration;
                Console.WriteLine($"Running in test mode for {duration} seconds...");
            }
        }
        
        if (verboseLogging)
        {
            Console.WriteLine("Verbose logging enabled.");
        }
        
        using HardwareInterface hardware = new HardwareInterface();
        using AudioPlayer audioPlayer = new AudioPlayer();
        RadioStationMap radioStations = RadioStationMap.Load("stations.json");
        GhostRadio ghostRadio = new GhostRadio(
            hardware: hardware, 
            audioPlayer: audioPlayer, 
            radioStations: radioStations, 
            staticFilePath: StaticFile, 
            updateIntervalMs: UpdateIntervalMs,
            verboseLogging: verboseLogging);
        
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