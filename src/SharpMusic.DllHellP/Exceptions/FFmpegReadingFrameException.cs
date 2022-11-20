namespace SharpMusic.DllHellP.Exceptions;

public class FFmpegReadingFrameException : FFmpegException
{
    public FFmpegReadingFrameException(string message) : base(message)
    {
    }

    public FFmpegReadingFrameException(string message, int errorCode) : base(message, errorCode)
    {
    }

    public FFmpegReadingFrameException(int errorCode) : base(errorCode)
    {
    }
}