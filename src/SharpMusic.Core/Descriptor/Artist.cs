using SharpMusic.Core.Utils;

namespace SharpMusic.Core.Descriptor;

public class Artist : IDescriptor
{
    internal Artist(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        Description = string.Empty;
        Albums = new CustomObservableImpl<Album, Artist>(
            i => (i.Artists as CustomObservableImpl<Artist, Album>)!, this);
        JoinedGroups = new CustomObservableImpl<ArtistsGroup, Artist>(
            i => (i.Members as CustomObservableImpl<Artist, ArtistsGroup>)!, this);
    }

    public Artist() : this(Guid.NewGuid())
    {
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public IList<Album> Albums { get; }

    public IList<ArtistsGroup> JoinedGroups { get; }
}