using static SharpMusic.DllHellP.Utils.FFmpegHelper;

namespace SharpMusic.DllHellP.Exceptions;

public class FFmpegException : Exception
{
    public FFmpegException(string message) : base(message)
    {
    }

    public FFmpegException(string message, int errorCode) : base($"{message}->{AvStringErrorCode(errorCode)}")
    {
    }

    public FFmpegException(int errorCode) : base(AvStringErrorCode(errorCode))
    {
    }
}