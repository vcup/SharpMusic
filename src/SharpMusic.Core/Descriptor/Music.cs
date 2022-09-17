using System.Collections.Specialized;

namespace SharpMusic.Core.Descriptor;

public class Music : IDescriptor
{
    private Artist[]? _artists;

    internal Music(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        var albumsIncluded = new CustomObservableImpl<Album>(AlbumsIncludedReset);
        albumsIncluded.CollectionChanged += AlbumsIncludedAddOrRemoved;
        AlbumsIncluded = albumsIncluded;
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

    private void AlbumsIncludedAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is Album newItem
            && newItem.Tracks.All(i => i.Guid != Guid)
            && newItem.Tracks is CustomObservableImpl<Music> implA)
        {
            implA.AddWithoutNotify(this);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is Album { Tracks: CustomObservableImpl<Music> implR })
        {
            implR.RemoveWithoutNotify(this);
        }
    }

    private void AlbumsIncludedReset(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not CustomObservableImpl<Album> impl) return;
        foreach (var item in impl)
        {
            (item.Tracks as CustomObservableImpl<Music>)!.RemoveWithoutNotify(this);
        }
    }
}