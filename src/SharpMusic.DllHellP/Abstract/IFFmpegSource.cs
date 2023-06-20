using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract.Delegate;

namespace SharpMusic.DllHellP.Abstract;

public interface IFFmpegSource : IAudioMetaInfo, IEnumerator<IntPtr>
{
    public TimeSpan Position { get; set; }

    public void ResetStream();

    public void SeekStream(TimeSpan time);

    /// <summary>
    /// Add an new <see cref="AVStream"/> to AVFormatContext and switch to the stream
    /// </summary>
    /// <param name="parameters">parameters will copy to stream</param>
    public unsafe void AddStream(AVCodecParameters* parameters);

    /// <summary>
    /// set current stream to the index
    /// </summary>
    /// <param name="index">index of stream</param>
    /// <exception cref="ArgumentOutOfRangeException">index out of range</exception>
    public void SetCurrentStream(int index);

    public unsafe AVStream* Stream { get; }

    public int LengthStreams { get; }

    public AVChannelLayout ChannelLayout { get; }

    public event FFmpegSourceEofHandler? SourceEofEvent;

    /// <summary>
    /// write header into media file
    /// </summary>
    public void WriteHeader();

    public bool WritePacket();

    public void WriteAndCloseSource();
}