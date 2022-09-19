using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SharpMusic.Core.Utils;

internal sealed class CustomObservableImpl<T> : ObservableCollection<T>
{
    private readonly NotifyCollectionChangedEventHandler _handler;

    public CustomObservableImpl(NotifyCollectionChangedEventHandler handler)
    {
        _handler = handler;
    }

    protected override void ClearItems()
    {
        _handler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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