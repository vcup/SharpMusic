namespace SharpMusic.DllHellP.Exceptions;

public class FFmpegFindStreamException : FFmpegException
{
    public FFmpegFindStreamException(string message) : base(message)
    {
    }

    public FFmpegFindStreamException(string message, int errorCode) : base(message, errorCode)
    {
    }

    public FFmpegFindStreamException(int errorCode) : base(errorCode)
    {
    }
}