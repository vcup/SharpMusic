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
            SampleFormat.Unsigned16 => throw new NotSupportedException(),
            SampleFormat.Signed32 => AVSampleFormat.AV_SAMPLE_FMT_S32,
            SampleFormat.Float32 => AVSampleFormat.AV_SAMPLE_FMT_FLT,
            SampleFormat.Double => AVSampleFormat.AV_SAMPLE_FMT_DBL,
            SampleFormat.Unsigned8Planar => AVSampleFormat.AV_SAMPLE_FMT_U8P,
            SampleFormat.Signed16Planar => AVSampleFormat.AV_SAMPLE_FMT_S16P,
            SampleFormat.Signed32Planar => AVSampleFormat.AV_SAMPLE_FMT_S32P,
            SampleFormat.Float32Planar => AVSampleFormat.AV_SAMPLE_FMT_FLTP,
            SampleFormat.DoublePlanar => AVSampleFormat.AV_SAMPLE_FMT_DBLP,
            SampleFormat.Signed64 => AVSampleFormat.AV_SAMPLE_FMT_S64,
            SampleFormat.Signed64Planar => AVSampleFormat.AV_SAMPLE_FMT_S64P,
            SampleFormat.Other => AVSampleFormat.AV_SAMPLE_FMT_NB,
            _ => // already cover all case of SampleFormat enum
                // dotCover disable next line
                throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public static unsafe SampleFormat GetSampleFormat(AVCodecParameters* parameters)
    {
        if (parameters->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO)
        {
            var message = $"codec_type must be {AVMediaType.AVMEDIA_TYPE_AUDIO} instead of {parameters->codec_type}";
            throw new
                ArgumentException(message, nameof(parameters));
        }

        return parameters->format switch
        {
            (int)AVSampleFormat.AV_SAMPLE_FMT_NONE => SampleFormat.None,
            (int)AVSampleFormat.AV_SAMPLE_FMT_U8 => SampleFormat.Unsigned8,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S16 => SampleFormat.Signed16,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S32 => SampleFormat.Signed32,
            (int)AVSampleFormat.AV_SAMPLE_FMT_FLT => SampleFormat.Float32,
            (int)AVSampleFormat.AV_SAMPLE_FMT_DBL => SampleFormat.Double,
            (int)AVSampleFormat.AV_SAMPLE_FMT_U8P => SampleFormat.Unsigned8Planar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S16P => SampleFormat.Signed16Planar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S32P => SampleFormat.Signed32Planar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_FLTP => SampleFormat.Float32Planar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_DBLP => SampleFormat.DoublePlanar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S64 => SampleFormat.Signed64,
            (int)AVSampleFormat.AV_SAMPLE_FMT_S64P => SampleFormat.Signed64Planar,
            (int)AVSampleFormat.AV_SAMPLE_FMT_NB => SampleFormat.Other,
            // TODO: throw message too vague
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