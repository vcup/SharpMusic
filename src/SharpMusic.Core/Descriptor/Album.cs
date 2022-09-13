namespace SharpMusic.Core.Descriptor;

public class Album : IDescriptor
{
    private List<Artist>? _trackArtists;

    internal Album()
    {
        Guid = Guid.NewGuid();
        Names = new List<string>();
        Description = string.Empty;
        Artists = new List<Artist>();
        Tracks = new List<Music>();
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public List<Artist> Artists { get; }

    public List<Music> Tracks { get; }

    public List<Artist> TracksArtists => _trackArtists ??= Tracks
        .SelectMany(i => i.Artists)
        .DistinctBy(i => i.Guid)
        .ToList();

    public DateOnly ReleaseDate { get; set; }

    public bool IsSingleAlbum { get; set; }
}