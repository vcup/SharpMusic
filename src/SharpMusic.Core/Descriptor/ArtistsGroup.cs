using System.Collections.Specialized;

namespace SharpMusic.Core.Descriptor;

public class ArtistsGroup : Artist
{
    internal ArtistsGroup(Guid guid, Artist organizer) : base(guid)
    {
        Organizer = organizer;
        var members = new CustomObservableImpl<Artist>(MembersReset);
        members.CollectionChanged += MembersAddOrRemoved;
        Members = members;
    }

    public ArtistsGroup(Artist organizer) : this(Guid.NewGuid(), organizer)
    {
    }

    public Artist Organizer { get; set; }

    public IList<Artist> Members { get; }

    private void MembersAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is Artist newItem
            && newItem.JoinedGroups.All(i => i.Guid != this.Guid))
        {
            newItem.JoinedGroups.Add(this);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is Artist removedItem)
        {
            removedItem.JoinedGroups.Remove(this);
        }
    }

    private void MembersReset(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (sender is not CustomObservableImpl<Artist> impl) return;
        foreach (var item in impl)
        {
            item.JoinedGroups.Remove(this);
        }
    }
}