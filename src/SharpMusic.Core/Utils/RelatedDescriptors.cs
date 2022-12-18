using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using SharpMusic.Core.Descriptor;

[assembly:InternalsVisibleTo("SharpMusic.CoreTests")]
namespace SharpMusic.Core.Utils;

/// <summary>
/// link collection with getter delegate, when the collection items has change, reaction in linked instance
/// </summary>
/// <typeparam name="T">storage type of collection</typeparam>
/// <typeparam name="TOwnerType">the instance own by what type</typeparam>
internal sealed class RelatedDescriptors<T, TOwnerType> : ObservableCollection<T>
    where T : IDescriptor
    where TOwnerType : IDescriptor
{
    private readonly Func<T, RelatedDescriptors<TOwnerType, T>> _linkedImplGetter;
    private readonly TOwnerType _instance;

    /// <summary>
    /// linker func for get linked collection and storage instance of owner type
    /// </summary>
    /// <param name="linkedImplGetter">getter for linked collection</param>
    /// <param name="instance">when this collection has change, where instance will reaction in linked collection</param>
    public RelatedDescriptors(Func<T, RelatedDescriptors<TOwnerType, T>> linkedImplGetter, TOwnerType instance)
    {
        _linkedImplGetter = linkedImplGetter;
        _instance = instance;
        CollectionChanged += CollectionChangedAddOrRemoved;
    }

    private void CollectionChangedAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add
                when args.NewItems![0] is T newItem
                     && _linkedImplGetter(newItem).All(i => i.Guid != _instance.Guid):
                // direct use private Items for avoid notify twice
                _linkedImplGetter(newItem).Items.Add(_instance);
                break;
            case NotifyCollectionChangedAction.Remove
                when args.OldItems![0] is T oldItem:
                _linkedImplGetter(oldItem).Items.Remove(_instance);
                break;
            case NotifyCollectionChangedAction.Reset:
                // do nothing before reset because items already cleaned
                break;
            case NotifyCollectionChangedAction.Replace
                when args.NewItems![0] is T newItem
                     && args.OldItems![0] is T oldItem
                     && newItem.Guid != oldItem.Guid:
                _linkedImplGetter(oldItem).Items.Remove(_instance);
                _linkedImplGetter(newItem).Items.Add(_instance);
                break;
            case NotifyCollectionChangedAction.Move:
                // do nothing when move
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.Action));
        }
    }

    protected override void ClearItems()
    {
        // clear collection does not broadcast change, delete for each items
        foreach (var item in this)
        {
            _linkedImplGetter(item).Items.Remove(_instance);
        }

        base.ClearItems();
    }

    public void AddWithoutNotify(T item)
    {
        Items.Add(item);
    }

    public bool RemoveWithoutNotify(T item)
    {
        return Items.Remove(item);
    }

    public void RemoveAtWithoutNotify(int index)
    {
        Items.RemoveAt(index);
    }

    public void InsertWithoutNotify(int index, T item)
    {
        Items.Insert(index, item);
    }

    public void ClearWithoutNotify()
    {
        Items.Clear();
    }
}