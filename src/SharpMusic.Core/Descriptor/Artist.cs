using System.Collections.Specialized;

namespace SharpMusic.Core.Descriptor;

public class Artist : IDescriptor
{
    internal Artist(Guid guid)
    {
        Guid = guid;
        Names = new List<string>();
        Description = string.Empty;
        var album = new CustomObservableImpl<Album>(AlbumsReset);
        album.CollectionChanged += AlbumsAddOrRemoved;
        Albums = album;
        JoinedGroups = new List<ArtistsGroup>();
    }

    public Artist() : this(Guid.NewGuid())
    {
    }

    public Guid Guid { get; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public IList<Album> Albums { get; }

    public IList<ArtistsGroup> JoinedGroups { get; }

    private void AlbumsAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is Album newItem
            && newItem.Artists.All(i => i.Guid != Guid)
            && newItem.Artists is CustomObservableImpl<Artist> implA)
        {
            implA.AddWithoutNotify(this);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is Album removedItem
                 && removedItem.Artists is CustomObservableImpl<Artist> implR)
        {
            implR.Remove(this);
        }
    }

    private void AlbumsReset(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not CustomObservableImpl<Album> impl) return;
        foreach (var item in impl)
        {
            item.Artists.Remove(this);
        }
    }
}