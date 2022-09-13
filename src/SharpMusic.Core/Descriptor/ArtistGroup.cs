namespace SharpMusic.Core.Descriptor;

public class ArtistGroup : IDescriptor
{
    internal ArtistGroup()
    {
        Guid = Guid.NewGuid();
        Names = new List<string>();
        Description = string.Empty;
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public Artist Organizer { get; set; }

    public List<Artist> Members { get; set; }
}