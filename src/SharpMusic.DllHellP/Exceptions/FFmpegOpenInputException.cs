namespace SharpMusic.DllHellP.Exceptions;

public class FFmpegOpenInputException : FFmpegException
{
    public FFmpegOpenInputException(string message) : base(message)
    {
    }

    public FFmpegOpenInputException(string message, int errorCode) : base(message, errorCode)
    {
    }

    public FFmpegOpenInputException(int errorCode) : base(errorCode)
    {
    }
}