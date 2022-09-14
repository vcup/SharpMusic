using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SharpMusic.Core.Descriptor;

public class ArtistsGroup : Artist
{
    internal ArtistsGroup(Guid guid, Artist organizer) : base(guid)
    {
        Organizer = organizer;
        var members = new ImplMembers(this);
        members.CollectionChanged += (sender, args) =>
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
            //else if (args.Action.HasFlag(NotifyCollectionChangedAction.Reset))
            //{
            // will do something on override method
            //}
        };
        Members = members;
    }

    public ArtistsGroup(Artist organizer) : this(Guid.NewGuid(), organizer)
    {
    }

    public Artist Organizer { get; set; }

    public IList<Artist> Members { get; }

    private class ImplMembers : ObservableCollection<Artist>
    {
        private readonly ArtistsGroup _owner;

        public ImplMembers(ArtistsGroup owner)
        {
            _owner = owner;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.JoinedGroups.Remove(_owner);
            }

            base.ClearItems();
        }
    }
}