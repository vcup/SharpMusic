using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Utils;
using static SDL2.SDL;

namespace SharpMusic.DllHellP.Extensions;

public static class SampleFormatExtensions
{
    public static ushort ToSdlFmt(this SampleFormat format, bool fallback = false)
    {
        return fallback
            ? format switch
            {
                SampleFormat.None => throw new NotSupportedException(),
                SampleFormat.Unsigned8 or SampleFormat.Unsigned8Planar => AUDIO_U8,
                SampleFormat.Signed16 or SampleFormat.Signed16Planar => AUDIO_S16SYS,
                SampleFormat.Unsigned16 => AUDIO_U16SYS,
                SampleFormat.Signed32 or SampleFormat.Signed32Planar => AUDIO_S32SYS,
                SampleFormat.Float32 or SampleFormat.Float32Planar => AUDIO_F32SYS,
                SampleFormat.Double or SampleFormat.DoublePlanar or
                    SampleFormat.Signed64 or SampleFormat.Signed64Planar or
                    SampleFormat.Other or _ => AUDIO_S16SYS
            }
            : format switch
            {
                SampleFormat.None => throw new NotSupportedException(),
                SampleFormat.Unsigned8 => AUDIO_U8,
                SampleFormat.Signed16 => AUDIO_S16SYS,
                SampleFormat.Unsigned16 => AUDIO_U16SYS,
                SampleFormat.Signed32 => AUDIO_S32SYS,
                SampleFormat.Float32 => AUDIO_F32SYS,
                SampleFormat.Double or SampleFormat.DoublePlanar or
                    SampleFormat.Unsigned8Planar or
                    SampleFormat.Signed16Planar or
                    SampleFormat.Signed32Planar or
                    SampleFormat.Float32Planar or
                    SampleFormat.Signed64 or SampleFormat.Signed64Planar or
                    SampleFormat.Other => throw new NotSupportedException
                        ($"{format.ToString()} can not map to SDL2 format when {nameof(fallback)} is false"),
                _ =>
                    throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
    }

    public static AVSampleFormat ToFmt(this SampleFormat format, bool allowPlanar = true)
    {
        var result = format switch
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

        return allowPlanar ? result : ffmpeg.av_get_packed_sample_fmt(result);
    }
}