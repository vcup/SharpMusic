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
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null) // dotCover disable this line
        };
    }

    public static unsafe SampleFormat GetSampleFormat(AVCodecParameters* parameters)
    {
        if (parameters->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO) return SampleFormat.None;
        return parameters->format switch
        {
            -1 => SampleFormat.None,
            0 => SampleFormat.Unsigned8,
            1 => SampleFormat.Signed16,
            2 => SampleFormat.Signed32,
            _ => throw new ArgumentOutOfRangeException(nameof(parameters), (IntPtr)parameters, null)
        };
    }

    public static unsafe int GetBitDepth(AVCodecParameters* parameters)
    {
        return parameters->bits_per_raw_sample is not 0
            ? parameters->bits_per_raw_sample
            : parameters->bits_per_coded_sample;
    }
}