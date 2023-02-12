using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.Utils;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public static class FFmpegHelper
{
    private const int ErrorMessageBufferSize = 1024;
    public static readonly AVRational AV_TIME_BASE_Q = new() { num = 1, den = AV_TIME_BASE };

    public static unsafe string AvStringErrorCode(int errorCode)
    {
        if (errorCode > 0) errorCode = -errorCode;
        var buffer = stackalloc byte[ErrorMessageBufferSize];
        av_strerror(errorCode, buffer, ErrorMessageBufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message ?? string.Empty;
    }
}