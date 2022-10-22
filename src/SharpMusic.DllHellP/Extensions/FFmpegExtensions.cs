using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class FFmpegExtensions
{
    public static AVSampleFormat ToFmt(this SampleFormat format)
    {
        return format switch
        {
            SampleFormat.None => AVSampleFormat.AV_SAMPLE_FMT_NONE,
            SampleFormat.Unsigned8 => AVSampleFormat.AV_SAMPLE_FMT_U8,
            SampleFormat.Signed16 => AVSampleFormat.AV_SAMPLE_FMT_S16,
            SampleFormat.Signed32 => AVSampleFormat.AV_SAMPLE_FMT_S32,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}