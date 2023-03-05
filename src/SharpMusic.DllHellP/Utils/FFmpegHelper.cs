using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.Utils;

public static class FFmpegHelper
{
    // ReSharper disable InconsistentNaming
    private const int ErrorMessageBufferSize = 1024;
    public static readonly AVRational AV_TIME_BASE_Q = new() { num = 1, den = AV_TIME_BASE };
    // ReSharper restore InconsistentNaming

    public static unsafe string AvStringErrorCode(int errorCode)
    {
        if (errorCode > 0) errorCode = -errorCode;
        var buffer = stackalloc byte[ErrorMessageBufferSize];
        av_strerror(errorCode, buffer, ErrorMessageBufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message ?? string.Empty;
    }

    public static unsafe SampleFormat GetSampleFormat(AVCodecParameters* parameters)
    {
        if (parameters->codec_type is AVMediaType.AVMEDIA_TYPE_AUDIO) return GetSampleFormat(parameters->format);
        var message = $"codec_type must be {AVMediaType.AVMEDIA_TYPE_AUDIO} instead of {parameters->codec_type}";
        throw new ArgumentException(message, nameof(parameters));
    }

    public static SampleFormat GetSampleFormat(int format) => format switch
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
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

    public static unsafe int GetBitDepth(AVCodecParameters* parameters)
    {
        return parameters->bits_per_raw_sample is not 0
            ? parameters->bits_per_raw_sample
            : parameters->bits_per_coded_sample;
    }
}