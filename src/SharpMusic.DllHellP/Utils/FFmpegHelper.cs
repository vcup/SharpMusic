using System.Runtime.InteropServices;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.Utils;

public static class FFmpegHelper
{
    private const int ErrorMessageBufferSize = 1024;

    public static unsafe string AvStringErrorCode(int errorCode)
    {
        var buffer = stackalloc byte[ErrorMessageBufferSize];
        av_strerror(errorCode, buffer, ErrorMessageBufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message ?? string.Empty;
    }
}