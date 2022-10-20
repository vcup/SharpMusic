using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.LowLevelImpl;

namespace SharpMusic.Core.Management;

public class PlayerManager
{
    public PlayerManager()
    {
        throw new NotImplementedException();
    }

    public long PlayPositionTicks
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public long PlaybackTimeTicks => throw new NotImplementedException();

    public PlaybackState PlaybackState => throw new NotImplementedException();

    public List<Uri> AudioSourceList => throw new NotImplementedException();

    public FFmpegSource PlayingAudioSource
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void PlayOrResume()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void PlayNext()
    {
        throw new NotImplementedException();
    }

    public void PlayPrev()
    {
        throw new NotImplementedException();
    }
}