using LibVLCSharp.Shared;

namespace GhostRadio;

public class AudioPlayer : IDisposable
{
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayer;
    private string? _currentAudioSourceSource;
    private bool _disposed = false;

    /// <summary>
    /// Whether audio is currently playing.
    /// </summary>
    public bool IsPlaying => _mediaPlayer.IsPlaying;

    /// <summary>
    /// Which station is currently playing.
    /// </summary>
    public string? CurrentAudioSource => _currentAudioSourceSource;
    
    public AudioPlayer()
    {
        Core.Initialize();
        _libVlc = new LibVLC();
        _mediaPlayer = new MediaPlayer(_libVlc);
    }

    /// <summary>
    /// Set current volume of the AudioPlayer.
    /// </summary>
    /// <param name="volumePercentage">A value between 0 and 1.</param>
    public void SetVolume(double volumePercentage)
    {
        int volume = Math.Max(0, Math.Min(100, (int)volumePercentage));
        _mediaPlayer.Volume = volume;
    }

    /// <summary>
    /// Stream audio from a given URL.
    /// </summary>
    /// <param name="audioUrl">URL of audio to stream.</param>
    public void PlayStreamingAudio(string audioUrl)
    {
        if (_currentAudioSourceSource == audioUrl && _mediaPlayer.IsPlaying)
        {
            return; // Already playing
        }

        _currentAudioSourceSource = audioUrl;
        
        Media media = new Media(_libVlc, audioUrl, FromType.FromLocation);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
    }

    /// <summary>
    /// Play audio from a local file source.
    /// </summary>
    /// <param name="filePath">Path to a local file to play.</param>
    public void PlayLocalAudio(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Local audio file not found: {filePath}");
            return;
        }

        if (_currentAudioSourceSource == filePath 
            && _mediaPlayer.IsPlaying)
        {
            return; // Already playing.
        }

        _currentAudioSourceSource = filePath;
        
        Media media = new Media(_libVlc, filePath);
        _mediaPlayer.Media = media;
        _mediaPlayer.Play();
    }

    /// <summary>
    /// Stop audio playback.
    /// </summary>
    public void Stop()
    {
        _mediaPlayer.Stop();
        _currentAudioSourceSource = null;
    }

    public void Dispose()
    {
        if (_disposed) 
            return;
        
        _mediaPlayer.Stop();
        _mediaPlayer.Dispose();
        _libVlc.Dispose();
        _disposed = true;
    }
}