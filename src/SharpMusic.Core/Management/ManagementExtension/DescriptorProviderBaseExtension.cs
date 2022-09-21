namespace SharpMusic.Core.Management.ManagementExtension;

public static class DescriptorProviderBaseExtension
{
    public static void Run(this DescriptorProviderBase provider)
    {
        using (provider)
        {
            if (provider.UseAsyncMethod && !provider.IsDisposed)
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