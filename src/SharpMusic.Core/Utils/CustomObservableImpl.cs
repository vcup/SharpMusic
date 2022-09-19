using System.Collections.ObjectModel;
using System.Collections.Specialized;
using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.Utils;

internal sealed class CustomObservableImpl<T, TOwnerType> : ObservableCollection<T>
    where T : IDescriptor
    where TOwnerType : IDescriptor
{
    private readonly Func<T, CustomObservableImpl<TOwnerType, T>> _linkedImplGetter;
    private readonly TOwnerType _instance;

    public CustomObservableImpl(Func<T, CustomObservableImpl<TOwnerType, T>> linkedImplGetter, TOwnerType instance)
    {
        _linkedImplGetter = linkedImplGetter;
        _instance = instance;
        CollectionChanged += CollectionChangedAddOrRemoved;
    }

    private void CollectionChangedAddOrRemoved(object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action.HasFlag(NotifyCollectionChangedAction.Add)
            && args.NewItems![0] is T newItem
            && _linkedImplGetter(newItem).All(i => i.Guid != _instance.Guid))
        {
            _linkedImplGetter(newItem).Items.Add(_instance);
        }
        else if (args.Action.HasFlag(NotifyCollectionChangedAction.Remove)
                 && args.OldItems![0] is T oldItem)
        {
            _linkedImplGetter(oldItem).Items.Remove(_instance);
        }
    }

    protected override void ClearItems()
    {
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