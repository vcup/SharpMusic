namespace SharpMusic.Core.Descriptor;

public class Music : IDescriptor
{
    public Music()
    {
        Guid = Guid.NewGuid();
        Names = new List<string>();
        Description = string.Empty;
        Artists = new List<Artist>();
        Album = new Album();
    }
    
    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description
    {
        get => string.Empty;
        set{}
    }

    public Album Album { get; set; }
    
    public List<Artist> Artists { get; }
    
    /// <summary>
    /// return the albums of included this music
    /// </summary>
    public List<Album> AlbumsIncluded { get; }
    
    public bool IsSingle { get; set; }
}