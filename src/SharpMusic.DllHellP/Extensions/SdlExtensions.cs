using static SDL2.SDL;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.Extensions;

public static class SdlExtensions
{
    public static ushort ToSdlFmt(this SampleFormat format)
    {
        return format switch
        {
            SampleFormat.None => throw new NotSupportedException(),
            SampleFormat.Unsigned8 => AUDIO_U8,
            SampleFormat.Signed16 => AUDIO_S16SYS,
            SampleFormat.Unsigned16 => AUDIO_U16SYS,
            SampleFormat.Signed32 => AUDIO_S32SYS,
            SampleFormat.Float32 => AUDIO_F32SYS,
            SampleFormat.Double or
                SampleFormat.Unsigned8Planar or
                SampleFormat.Signed16Planar or
                SampleFormat.Signed32Planar or
                SampleFormat.Float32Planar or
                SampleFormat.DoublePlanar or
                SampleFormat.Signed64 or
                SampleFormat.Signed64Planar or
                // TODO: handle as exception, e.g. NotSupportException
                SampleFormat.Other => AUDIO_S16SYS,
            _ => // already cover all case of SampleFormat enum
                // dotCover disable next line
                throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}