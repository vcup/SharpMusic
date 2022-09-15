using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SharpMusic.Core.Descriptor;

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
}