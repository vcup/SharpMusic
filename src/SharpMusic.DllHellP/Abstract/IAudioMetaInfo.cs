using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Abstract;

public interface IAudioMetaInfo
{
    public Uri Uri { get; }
    public TimeSpan Duration { get; }
    public long BitRate { get; }
    public int BitDepth { get; }
    public int Channels { get; }
    public int SampleRate { get; }
    public SampleFormat Format { get; }
}