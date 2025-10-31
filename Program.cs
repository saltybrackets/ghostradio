using GhostRadio.Services;
using Microsoft.AspNetCore.Components;

namespace GhostRadio;

public static class Program
{
    private const string? StaticFile = "static.wav";
    private const int UpdateIntervalMs = 50;
    private const string StationsFilePath = "stations.json";

    public static async Task Main(string[] args)
    {
        // Parse command line arguments
        bool consoleMode = args.Contains("--console") || args.Contains("-c");
        bool webMode = args.Contains("--web") || args.Contains("-w");
        bool mockMode = args.Contains("--mock") || args.Contains("-m");
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
                Console.WriteLine("  -c, --console    Run in console-only mode (no web interface)");
                Console.WriteLine("  -w, --web        Run in web-only mode (no console output)");
                Console.WriteLine("  -m, --mock       Use mock hardware (for testing without GPIO/SPI)");
                Console.WriteLine("  -v, --verbose    Enable verbose logging");
                Console.WriteLine("  -h, --help       Show this help");
                Console.WriteLine("Arguments:");
                Console.WriteLine("  duration         Test duration in seconds (optional)");
                Console.WriteLine();
                Console.WriteLine("Default: Runs web interface on port 80 (or 5000 in mock mode)");
                return;
            }
            else if (int.TryParse(arg, out int duration))
            {
                testDurationSeconds = duration;
            }
        }

        // Default to web-only mode if neither specified
        if (!consoleMode && !webMode)
        {
            webMode = true;
        }

        if (webMode)
        {
            await RunWithWebInterface(consoleMode, testDurationSeconds, verboseLogging, mockMode);
        }
        else
        {
            await RunConsoleOnly(testDurationSeconds, verboseLogging, mockMode);
        }
    }

    private static async Task RunWithWebInterface(bool alsoRunConsole, int? testDurationSeconds, bool verboseLogging, bool mockMode)
    {
        var builder = WebApplication.CreateBuilder();

        // Configure Kestrel to listen on port 80 (production) or 5000 (mock mode)
        int port = mockMode ? 5000 : 80;
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(port);
        });

        // Add services to the container
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Register hardware and audio as singletons
        if (mockMode)
        {
            builder.Services.AddSingleton<IHardwareInterface, MockHardwareInterface>();
            builder.Services.AddSingleton<IAudioPlayer, MockAudioPlayer>();
        }
        else
        {
            builder.Services.AddSingleton<IHardwareInterface, HardwareInterface>();
            builder.Services.AddSingleton<IAudioPlayer, AudioPlayer>();
        }

        // Load and register station map
        var radioStationMap = RadioStationMap.Load(StationsFilePath);
        builder.Services.AddSingleton(radioStationMap);

        // Register GhostRadio controller
        builder.Services.AddSingleton(sp => new GhostRadioController(
            hardware: sp.GetRequiredService<IHardwareInterface>(),
            audioPlayer: sp.GetRequiredService<IAudioPlayer>(),
            radioStations: sp.GetRequiredService<RadioStationMap>(),
            staticFilePath: StaticFile!,
            updateIntervalMs: UpdateIntervalMs,
            verboseLogging: verboseLogging
        ));

        // Register services
        builder.Services.AddSingleton(sp => new StationManagementService(
            StationsFilePath,
            sp.GetRequiredService<RadioStationMap>()
        ));
        builder.Services.AddSingleton<RadioStateService>();
        builder.Services.AddSingleton(sp => new MockHardwareControlService(
            sp.GetRequiredService<IHardwareInterface>()
        ));

        // Register hosted service to run the radio controller
        builder.Services.AddHostedService<GhostRadioHostedService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<Components.App>()
            .AddInteractiveServerRenderMode();

        Console.WriteLine("GhostRadio starting...");
        Console.WriteLine($"Web interface available at: http://localhost{(port == 80 ? "" : $":{port}")}");

        if (testDurationSeconds.HasValue)
        {
            Console.WriteLine($"Running in test mode for {testDurationSeconds.Value} seconds...");
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(testDurationSeconds.Value));
            await app.RunAsync(cts.Token);
        }
        else
        {
            await app.RunAsync();
        }
    }

    private static async Task RunConsoleOnly(int? testDurationSeconds, bool verboseLogging, bool mockMode)
    {
        Console.WriteLine("GhostRadio starting (console mode)...");

        if (verboseLogging)
        {
            Console.WriteLine("Verbose logging enabled.");
        }

        using IHardwareInterface hardware = mockMode
            ? new MockHardwareInterface()
            : new HardwareInterface();
        using IAudioPlayer audioPlayer = mockMode
            ? new MockAudioPlayer()
            : new AudioPlayer();
        RadioStationMap radioStations = RadioStationMap.Load(StationsFilePath);
        GhostRadioController ghostRadio = new GhostRadioController(
            hardware: hardware,
            audioPlayer: audioPlayer,
            radioStations: radioStations,
            staticFilePath: StaticFile!,
            updateIntervalMs: UpdateIntervalMs,
            verboseLogging: verboseLogging);

        Console.WriteLine("GhostRadio initialized. Press Ctrl+C to exit.");

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cancellationTokenSource.Cancel();
        };

        if (testDurationSeconds.HasValue)
        {
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(testDurationSeconds.Value));
        }

        await ghostRadio.RunAsync(cancellationTokenSource.Token);

        Console.WriteLine("GhostRadio stopped.");
    }
}