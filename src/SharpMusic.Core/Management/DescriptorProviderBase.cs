namespace SharpMusic.Core.Management;

public abstract class DescriptorProviderBase : IDisposable
{
    protected bool IsDisposed;
    protected readonly DescriptorManager Manager;

    public DescriptorProviderBase(DescriptorManager manager)
    {
        Manager = manager;
    }

    public abstract void Execute();

    public virtual async Task ExecuteAsync()
    {
        await Task.Run(Execute);
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
    }
}