using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.Management;

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

    public void Add(IDescriptor item)
    {
        switch (item)
        {
            case Music music:
                Add(music);
                break;
            case Album album:
                Add(album);
                break;
            case ArtistsGroup artistGroup:
                Add(artistGroup);
                break;
            case Artist artist:
                Add(artist);
                break;
            case Playlist playlist:
                Add(playlist);
                break;
            default:
                throw new ArgumentException();
        }
    }

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
                    throw new ArgumentException();
            }
        }
    }

    private void Add(Music music)
    {
        if (_musics.All(i => i.Guid != music.Guid))
        {
            _musics.Add(music);
        }
    }

    private void Add(Album album)
    {
        if (_albums.All(i => i.Guid != album.Guid))
        {
            _albums.Add(album);
        }
    }

    private void Add(Artist artist)
    {
        if (_artists.All(i => i.Guid != artist.Guid))
        {
            _artists.Add(artist);
        }
    }

    private void Add(ArtistsGroup artistsGroup)
    {
        if (_artistsGroups.All(i => i.Guid != artistsGroup.Guid))
        {
            _artistsGroups.Add(artistsGroup);
        }
    }

    private void Add(Playlist playlist)
    {
        if (_playlists.All(i => i.Guid != playlist.Guid))
        {
            _playlists.Add(playlist);
        }
    }
}