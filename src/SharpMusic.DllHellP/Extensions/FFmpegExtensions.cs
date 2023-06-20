using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class FFmpegExtensions
{
    /// <summary>
    /// read to next packet with <see cref="AVMediaType.AVMEDIA_TYPE_AUDIO"/>
    /// </summary>
    /// <param name="source">the <see cref="IFFmpegSource"/> will reading packet</param>
    /// <returns>
    /// true if the source has moved to an packet with <see cref="AVMediaType.AVMEDIA_TYPE_AUDIO"/>
    /// false if the source already read to end
    /// </returns>
    public static bool MoveNextAudioPacket(this IFFmpegSource source)
    {
        var result = source.MoveNext();
        while (source.Format is SampleFormat.None && (result = source.MoveNext()))
        {
        }

        return result;
    }

    /// <summary>
    /// read to next packet with specify stream index
    /// </summary>
    /// <param name="source">the <see cref="IFFmpegSource"/> will reading packet</param>
    /// <param name="streamIndex">wanted packet with the stream index</param>
    /// <returns>
    /// true if the source has moved to an packet with <see cref="AVMediaType.AVMEDIA_TYPE_AUDIO"/>
    /// false if the source already read to end
    /// </returns>
    public static unsafe bool MoveNext(this IFFmpegSource source, int streamIndex)
    {
        var result = source.MoveNext();
        while (source.Stream->index != streamIndex && (result = source.MoveNext()))
        {
        }

        return result;
    }
}