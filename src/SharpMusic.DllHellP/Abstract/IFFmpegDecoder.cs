using FFmpeg.AutoGen;

namespace SharpMusic.DllHellP.Abstract;

public interface IFFmpegDecoder : IEnumerable<AVFrame>
{
    public AVCodecContext AvCodecCtx { get; }
}