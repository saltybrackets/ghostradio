namespace GhostRadio;

public class MockAudioPlayer : IAudioPlayer
{
    private bool _isPlaying = false;
    private string? _currentAudioSource;
    private double _volume = 50.0;
    private bool _disposed = false;

    public bool IsPlaying => _isPlaying;
    public string? CurrentAudioSource => _currentAudioSource;
    public string? CurrentTrackTitle => null;
    public string? CurrentTrackArtist => null;

    public MockAudioPlayer()
    {
        Console.WriteLine("Mock audio player initialized (no actual audio playback)");
    }

    public void SetVolume(double volumePercentage)
    {
        _volume = Math.Clamp(volumePercentage, 0, 100);
    }

    public void PlayStreamingAudio(string audioUrl)
    {
        if (_currentAudioSource == audioUrl && _isPlaying)
        {
            return; // Already "playing"
        }

        _currentAudioSource = audioUrl;
        _isPlaying = true;
        Console.WriteLine($"Mock: Now playing stream: {audioUrl}");
    }

    public void PlayLocalAudio(string? filePath, bool loop = false)
    {
        if (_currentAudioSource == filePath && _isPlaying)
        {
            return; // Already "playing"
        }

        _currentAudioSource = filePath;
        _isPlaying = true;
        string loopText = loop ? " (looping)" : "";
        Console.WriteLine($"Mock: Now playing local file: {filePath}{loopText}");
    }

    public void Stop()
    {
        _isPlaying = false;
        _currentAudioSource = null;
        Console.WriteLine("Mock: Audio stopped");
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        Stop();
        _disposed = true;
    }
}
