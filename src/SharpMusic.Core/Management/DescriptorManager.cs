using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.Management;

/// <summary>
/// storage related Descriptors
/// </summary>
public class DescriptorManager
{
    private readonly List<Music> _musics;
    private readonly List<Album> _albums;
    private readonly List<Artist> _artists;
    private readonly List<ArtistsGroup> _artistsGroups;
    private readonly List<Playlist> _playlists;

    public DescriptorManager()
    {
        _musics = new List<Music>();
        _albums = new List<Album>();
        _artists = new List<Artist>();
        _artistsGroups = new List<ArtistsGroup>();
        _playlists = new List<Playlist>();
    }

    private IEnumerable<IDescriptor> GetAllDescriptor() =>
        (_musics as IEnumerable<IDescriptor>)
            .Concat(_albums)
            .Concat(_artists)
            .Concat(_artistsGroups)
            .Concat(_playlists);

    /// <summary>
    /// storage descriptor, only storage once same descriptor
    /// </summary>
    /// <param name="item">will storage descriptor</param>
    /// <exception cref="ArgumentException"></exception>
    public void Add(IDescriptor item)
    {
        switch (item)
        {
            case Music music when _musics.All(i => i.Guid != item.Guid):
                _musics.Add(music);
                break;
            case Album album when _albums.All(i => i.Guid != item.Guid):
                _albums.Add(album);
                break;
            case ArtistsGroup artistGroup when _artistsGroups.All(i => i.Guid != item.Guid):
                _artistsGroups.Add(artistGroup);
                break;
            case Artist artist when _artists.All(i => i.Guid != item.Guid):
                _artists.Add(artist);
                break;
            case Playlist playlist when _playlists.All(i => i.Guid != item.Guid):
                _playlists.Add(playlist);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(item));
        }
    }

    /// <summary>
    /// storage via many descriptor, ignore exited descriptor
    /// </summary>
    /// <param name="items">enumerable of storage</param>
    /// <exception cref="ArgumentOutOfRangeException">enumerable contained does not support type of descriptor</exception>
    public void Add(IEnumerable<IDescriptor> items)
    {
        foreach (var item in items.ExceptBy(GetAllDescriptor().Select(i => i.Guid), i => i.Guid))
        {
            switch (item)
            {
                case Music music:
                    _musics.Add(music);
                    break;
                case Album album:
                    _albums.Add(album);
                    break;
                case ArtistsGroup artistGroup:
                    _artistsGroups.Add(artistGroup);
                    break;
                case Artist artist:
                    _artists.Add(artist);
                    break;
                case Playlist playlist:
                    _playlists.Add(playlist);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(items), item, $"Descriptor Guid is: {item.Guid}");
            }
        }
    }
}