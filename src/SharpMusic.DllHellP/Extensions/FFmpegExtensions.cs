using SharpMusic.DllHellP.LowLevelImpl;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class FFmpegExtensions
{
    public static bool MoveNextAudioPacket(this FFmpegSource source)
    {
        var result = true;
        while (source.Format is SampleFormat.None && (result = source.MoveNext()))
        {
        }

        return result;
    }

    public static unsafe bool MoveNext(this FFmpegSource source, int streamIndex)
    {
        var result = true;
        while (source.Stream->index == streamIndex && (result = source.MoveNext()))
        {
        }

        return result;
    }
}