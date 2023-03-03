using static SDL2.SDL;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class SdlExtensions
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
}