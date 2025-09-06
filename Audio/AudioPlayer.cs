using LibVLCSharp.Shared;

namespace GhostRadio.Audio;

public class AudioPlayer : IDisposable
{
    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private string? _currentStationUrl;
    private bool _disposed = false;

    public AudioPlayer()
    {
        Core.Initialize();
        _libVLC = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVLC);
    }

    public void SetVolume(double volumePercentage)
    {
        var volume = Math.Max(0, Math.Min(100, (int)volumePercentage));
        _mediaPlayer.Volume = volume;
    }

    public void PlayStation(string stationUrl)
    {
        if (_currentStationUrl == stationUrl && _mediaPlayer.IsPlaying)
        {
            return; // Already playing this station
        }

        _currentStationUrl = stationUrl;
        
        var media = new Media(_libVLC, stationUrl, FromType.FromLocation);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
    }

    public void PlayStaticFile(string staticFilePath)
    {
        if (!File.Exists(staticFilePath))
        {
            Console.WriteLine($"Static file not found: {staticFilePath}");
            return;
        }

        if (_currentStationUrl == staticFilePath && _mediaPlayer.IsPlaying)
        {
            return; // Already playing static
        }

        _currentStationUrl = staticFilePath;
        
        var media = new Media(_libVLC, staticFilePath, FromType.FromPath);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
    }

    public void Stop()
    {
        _mediaPlayer.Stop();
        _currentStationUrl = null;
    }

    public bool IsPlaying => _mediaPlayer.IsPlaying;

    public string? CurrentStation => _currentStationUrl;

    public void Dispose()
    {
        if (!_disposed)
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
            _disposed = true;
        }
    }
}