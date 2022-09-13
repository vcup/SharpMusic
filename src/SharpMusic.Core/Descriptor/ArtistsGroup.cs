namespace SharpMusic.Core.Descriptor;

public class ArtistsGroup : Artist
{
    internal ArtistsGroup()
    {
        Organizer = new Artist();
        Members = new List<Artist>();
    }

    public Artist Organizer { get; set; }

    public List<Artist> Members { get; }
}