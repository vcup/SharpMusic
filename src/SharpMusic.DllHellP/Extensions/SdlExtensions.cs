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
            SampleFormat.Signed16 => AUDIO_S16,
            SampleFormat.Signed32 => AUDIO_S32,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null) // dotCover disable this line
        };
    }
}