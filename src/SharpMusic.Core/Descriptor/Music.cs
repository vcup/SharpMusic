using SharpMusic.Core.Utils;

namespace SharpMusic.Core.Descriptor;

public class Music : IDescriptor
{
    private Artist[]? _artists;

    internal Music(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        AlbumsIncluded = new CustomObservableImpl<Album, Music>(
            i => (i.Tracks as CustomObservableImpl<Music, Album>)!, this);
        SoundSource = new List<Uri>();
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

    /// <summary>
    /// earliest album to included this music corresponds to the artists in the staff list
    /// </summary>
    public IList<Artist> Artists => _artists ??=
        AlbumsIncluded
            .FirstOrDefault(i => i.StaffList.Any())?
            .StaffList
            .First(i => i.SourceMusic.Guid == Guid)
            .Keys
            .ToArray()
        ?? Array.Empty<Artist>();

    /// <summary>
    /// return the albums of included this music, sort by release date
    /// </summary>
    public IList<Album> AlbumsIncluded { get; }

    public IList<Uri> SoundSource { get; }
}