using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Abstract;

public interface ISoundOutput
{
    public bool IsMute { get; set; }
    public int Volume { get; set; }
    public int MinVolume { get; }
    public int MaxVolume { get; }
    public PlaybackState State { get; }
    public SdlAudioDevice Device { get; }

    public void Play();
    public void Pause();
    public void Resume();
    public void Stop();
}