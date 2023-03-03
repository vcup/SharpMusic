namespace SharpMusic.DllHellP.Abstract;

public interface ISoundSource
{
    public Uri Uri { get; }

    public TimeSpan Duration { get; }
    public TimeSpan Position { get; set; }

    public void ResetStream();

    public void SeekStream(TimeSpan time);
}