namespace SharpMusic.DllHellP.Abstract;

public interface ISoundSource
{
    public Uri Uri { get; }

    public TimeSpan Duration { get; }
    public TimeSpan Position { get; set; }
    
    public int BitRate { get; }
    public int BitDepth { get; }
    public int Channels { get; }
    public int SampleRate { get; }
    
    public SampleFormat Format { get; }
}