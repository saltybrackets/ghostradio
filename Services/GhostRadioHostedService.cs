namespace GhostRadio.Services;

public class GhostRadioHostedService : IHostedService
{
    private readonly GhostRadioController _ghostRadio;
    private Task? _executingTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public GhostRadioHostedService(GhostRadioController ghostRadio)
    {
        _ghostRadio = ghostRadio;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("GhostRadio controller starting...");
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _executingTask = _ghostRadio.RunAsync(_cancellationTokenSource.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        Console.WriteLine("GhostRadio controller stopping...");
        _cancellationTokenSource?.Cancel();

        await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }
}
