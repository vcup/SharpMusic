using System.Reflection;
using CSCore.SoundOut;

namespace SharpMusic.Core.Player.Extensions;

public static class SoundOutExtensions
{
    private static readonly Dictionary<ISoundOut, bool> Cache = new();

    public static bool IsInitialized(this ISoundOut soundOut)
    {
        if (Cache.TryGetValue(soundOut, out var flag) && flag)
        {
            return flag;
        }

        FieldInfo? field;
        switch (soundOut) // use Reflection to get non-publish status of Initialized
        {
            case DirectSoundOut:
                field = typeof(DirectSoundOut)
                    .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field is null)
                {
                    throw new InvalidOperationException(); // TODO: Add description
                }

                break;
            case WasapiOut:
                field = typeof(WasapiOut)
                    .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field is null)
                {
                    throw new InvalidOperationException(); // TODO: Add description
                }

                break;
            case WaveOut:
                field = typeof(WaveOut)
                    .GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field is null)
                {
                    throw new InvalidOperationException(); // TODO: Add description
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(); // TODO: Add description
        }

        flag = (bool)field.GetValue(soundOut)!;
        return Cache[soundOut] = flag;
    }
}