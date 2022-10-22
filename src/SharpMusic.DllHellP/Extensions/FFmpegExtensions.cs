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

    public static SampleFormat ToSampleFormat(this AVCodecParameters parameters)
    {
        return parameters.format switch
        {
            0 when parameters.bits_per_coded_sample is 8 => SampleFormat.Unsigned8,
            > 0 when parameters.bits_per_coded_sample is 16 => SampleFormat.Signed16,
            > 0 when parameters.bits_per_coded_sample is 32 => SampleFormat.Signed32,
            _ => throw new ArgumentOutOfRangeException(nameof(parameters), parameters, null)
        };
    }
}