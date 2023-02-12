using static SharpMusic.DllHellP.Utils.FFmpegHelper;

namespace SharpMusic.DllHellP.Exceptions;

public class FFmpegException : Exception
{
    private readonly string? _errorString;
    private readonly string? _message;

    public FFmpegException(string message)
    {
        _message = message;
    }

    public FFmpegException(string message, int errorCode)
    {
        ErrorCode = errorCode < 0 ? errorCode : -errorCode;
        _errorString = AvStringErrorCode(errorCode);
        _message = message;
    }

    public FFmpegException(int errorCode)
    {
        ErrorCode = errorCode < 0 ? errorCode : -errorCode;
        _errorString = AvStringErrorCode(errorCode);
    }

    public int ErrorCode { get; init; }

    public string ErrorString => _errorString ?? string.Empty;

    public string MessageString => _message ?? string.Empty;

    public override string Message => _message is null || _errorString is null
        ? _message ?? _errorString ?? base.Message
        : _message + " -> " + _errorString;
}