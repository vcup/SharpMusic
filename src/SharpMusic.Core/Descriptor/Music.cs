namespace SharpMusic.Core.Descriptor;

public class Music : IDescriptor
{
    internal Music(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        Artists = new List<Artist>();
        AlbumsIncluded = new List<Album>();
    }

    public Music() : this(Guid.NewGuid())
    {
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    /// <summary>
    /// disabled property
    /// </summary>
    public string Description
    {
        get => string.Empty;
        set { }
    }

    public IList<Artist> Artists { get; }

    /// <summary>
    /// return the albums of included this music, sort by release date
    /// </summary>
    public IList<Album> AlbumsIncluded { get; }
}