namespace SharpMusic.Core.Descriptor.DescriptorExtension;

public static class DescriptorExtension
{
    public static IEnumerable<IDescriptor> GetAssociatedDescriptors(this IDescriptor instance,
        SearchDescriptorOption option = SearchDescriptorOption.None)
    {
        var result = instance switch
        {
            Music music => music.Artists
                .Select(i => (IDescriptor)i)
                .Concat(music.AlbumsIncluded),
            Album album => album.Artists
                .Select(i => (IDescriptor)i)
                .Concat(album.Tracks)
                .Concat(album.TracksArtists),
            ArtistsGroup artistGroup => artistGroup.Members
                .SelectMany(i => GetAssociatedDescriptors(i, SearchDescriptorOption.IgnoreArtistGroup)),
            Artist artist => artist.Albums
                .Select(i => (IDescriptor)i)
                .Concat(artist.JoinedGroups),
            Playlist playlist => playlist.Concat(playlist.GetAssociatedDescriptors()),
            _ => throw new ArgumentException()
        };

        result = result
            .Where(i => !(option.HasFlag(SearchDescriptorOption.IgnoreMusic) && i is Music))
            .Where(i => !(option.HasFlag(SearchDescriptorOption.IgnoreAlbum) && i is Album))
            .Where(i => !(option.HasFlag(SearchDescriptorOption.IgnoreArtist) && i is Artist))
            .Where(i => !(option.HasFlag(SearchDescriptorOption.IgnoreArtistGroup) && i is ArtistsGroup))
            .Where(i => !(option.HasFlag(SearchDescriptorOption.IgnorePlaylist) && i is Playlist));

        return result;
    }
}