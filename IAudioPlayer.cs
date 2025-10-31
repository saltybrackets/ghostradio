namespace GhostRadio;

public interface IAudioPlayer : IDisposable
{
    bool IsPlaying { get; }
    string? CurrentAudioSource { get; }
    void SetVolume(double volumePercentage);
    void PlayStreamingAudio(string audioUrl);
    void PlayLocalAudio(string? filePath, bool loop = false);
    void Stop();
}
