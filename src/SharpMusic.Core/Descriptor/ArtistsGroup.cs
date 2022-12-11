using SharpMusic.Core.Utils;

namespace SharpMusic.Core.Descriptor;

public class ArtistsGroup : Artist
{
    internal ArtistsGroup(Guid guid, Artist organizer) : base(guid)
    {
        Organizer = organizer;
        Members = new RelatedDescriptors<Artist, ArtistsGroup>(
            i => (i.JoinedGroups as RelatedDescriptors<ArtistsGroup, Artist>)!, this);
    }

    public ArtistsGroup(Artist organizer) : this(Guid.NewGuid(), organizer)
    {
    }

    public Artist Organizer { get; set; }

    public IList<Artist> Members { get; }
}