using System.Collections.Specialized;
using SharpMusic.Core.ExpandInfo;

namespace SharpMusic.Core.Descriptor;

public class Album : IDescriptor
{
    private List<Artist>? _trackArtists;

    internal Album(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        Description = string.Empty;
        var artists = new CustomObservableImpl<Artist>(ArtistsReset);
        artists.CollectionChanged += ArtistsAddOrRemoved;
        Artists = artists;
        var tracks = new CustomObservableImpl<Music>(TracksReset);
        tracks.CollectionChanged += TracksAddOrRemoved;
        Tracks = tracks;
        StaffList = new StaffList(this);
    }

    public Album() : this(Guid.NewGuid())
    {
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public IList<Artist> Artists { get; }

    public IList<Music> Tracks { get; }

    public List<Artist> TracksArtists => _trackArtists ??= Tracks
        .SelectMany(i => i.Artists)
        .DistinctBy(i => i.Guid)
        .ToList();

    public DateOnly ReleaseDate { get; set; }

    public AlbumType Type { get; set; }

    public StaffList StaffList { get; set; }

    private void ArtistsAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is Artist newItem
            && newItem.Albums.All(i => i.Guid != Guid)
            && newItem.Albums is CustomObservableImpl<Album> implA)
        {
            implA.AddWithoutNotify(this);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is Artist { Albums: CustomObservableImpl<Album> implR })
        {
            implR.RemoveWithoutNotify(this);
        }
    }

    private void ArtistsReset(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not CustomObservableImpl<Artist> impl) return;
        foreach (var item in impl)
        {
            (item.Albums as CustomObservableImpl<Album>)!.RemoveWithoutNotify(this);
        }
    }

    private void TracksAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is Music newItem
            && newItem.AlbumsIncluded.All(i => i.Guid != Guid)
            && newItem.AlbumsIncluded is CustomObservableImpl<Album> implA)
        {
            implA.AddWithoutNotify(this);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is Music { AlbumsIncluded: CustomObservableImpl<Album> implR })
        {
            implR.RemoveWithoutNotify(this);
        }
    }

    private void TracksReset(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not CustomObservableImpl<Music> impl) return;
        foreach (var item in impl)
        {
            (item.AlbumsIncluded as CustomObservableImpl<Album>)!.RemoveWithoutNotify(this);
        }
    }
}