using SharpMusic.DllHellP.LowLevelImpl;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class FFmpegExtensions
{
    public static void MoveNextAudioPacket(this FFmpegSource source)
    {
        while (source.Format is SampleFormat.None && source.MoveNext())
        {
        }
    }
}