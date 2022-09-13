namespace SharpMusic.Core.Descriptor;

public class Artist : IDescriptor
{
    private List<Music>? _musics;

    internal Artist()
    {
        Guid = Guid.NewGuid();
        Names = new List<string>();
        Description = string.Empty;
        Albums = new List<Album>();
        JoinedGroups = new List<ArtistsGroup>();
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public List<Album> Albums { get; }

    public List<Music> Musics => _musics ??= Albums
        .SelectMany(i => i.Tracks)
        .DistinctBy(i => i.Guid)
        .ToList();
    
    public List<ArtistsGroup> JoinedGroups { get; }
}