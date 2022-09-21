namespace SharpMusic.Core.Management.ManagementExtension;

public static class DescriptorProviderBaseExtension
{
    public static void Run(this DescriptorProviderBase provider)
    {
        if (provider.IsDisposed) return;
        using (provider)
        {
            if (provider.UseAsyncMethod)
            {
                provider.ExecuteAsync().Wait();
            }
            else
            {
                provider.Execute();
            }
        }
    }
}