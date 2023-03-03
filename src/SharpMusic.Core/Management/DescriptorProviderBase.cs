namespace SharpMusic.Core.Management;

public abstract class DescriptorProviderBase : IDisposable
{
    internal readonly bool UseAsyncMethod;
    protected readonly DescriptorManager Manager;

    public DescriptorProviderBase(DescriptorManager manager, bool useAsyncMethod = true)
    {
        Manager = manager;
        UseAsyncMethod = useAsyncMethod;
    }

    public virtual void Execute()
    {
        throw new NotImplementedException();
    }

    public virtual Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }

    protected void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            // do something
        }

        IsDisposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool IsDisposed { get; protected set; }
}