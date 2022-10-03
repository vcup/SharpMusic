namespace SharpMusic.Core.Playback;

public class AudioSource
{
    public AudioSource(Uri uri)
    {
        Uri = uri;
    }

    public Uri Uri { get; }
}