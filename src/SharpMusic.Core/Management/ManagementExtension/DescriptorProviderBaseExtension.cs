using System.Reflection;

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

    public static DescriptorProviderBase? ActiveProviderFrom(this DescriptorManager manager, string path)
    {
        var assembly = Assembly.LoadFile(path);
        var type = assembly.GetTypes().FirstOrDefault(i => i.IsAssignableTo(typeof(DescriptorProviderBase)));
        if (type is null)
        {
            return null;
        }

        return Activator.CreateInstance(type, manager) as DescriptorProviderBase;
    }
}