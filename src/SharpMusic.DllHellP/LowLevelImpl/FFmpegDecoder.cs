using System.Collections;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;
using SharpMusic.DllHellP.Abstract;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegDecoder : IFFmpegDecoder, IDisposable
{
    private readonly FFmpegSource _source;
    private readonly unsafe AVCodecContext* _codecCtx;
    private bool _isDisposed;

    public unsafe FFmpegDecoder(FFmpegSource source)
    {
        _source = source;
        var codec = avcodec_find_decoder(_source.AvCodecParameters->codec_id);
        _codecCtx = avcodec_alloc_context3(codec);
        var ret = avcodec_parameters_to_context(_codecCtx, _source.AvCodecParameters);
        if (ret != 0)
        {
            throw new ArgumentException();
        }

        ret = avcodec_open2(_codecCtx, codec, null);
        if (ret < 0)
        {
            throw new ArgumentException();
        }
    }

    public unsafe IEnumerator<AVFrame> GetEnumerator()
    {
        return _source.Select(packet => // possible memory leak
        {
            var frame = av_frame_alloc();
            avcodec_send_packet(_codecCtx, &packet);
            avcodec_receive_frame(_codecCtx, frame);
            return *frame;
        }).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public unsafe AVCodecContext AvCodecCtx => *_codecCtx;

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing && _isDisposed) return;
        _source.Dispose();
        avcodec_close(_codecCtx);
        _isDisposed = true;
    }
}