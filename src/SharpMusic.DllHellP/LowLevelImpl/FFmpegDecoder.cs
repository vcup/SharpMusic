using System.Collections;
using System.Diagnostics;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// decode <see cref="AVPacket"/> from <see cref="FFmpegSource"/>, provide pointer of <see cref="AVFrame"/>
/// </summary>
public class FFmpegDecoder : IEnumerator<IntPtr>
{
    private readonly FFmpegSource _source;
    private readonly unsafe AVCodecContext* _codecCtx;
    private readonly unsafe AVFrame* _frame;
    private bool _isDisposed;

    public unsafe FFmpegDecoder(FFmpegSource source)
    {
        _source = source;
        var codec = avcodec_find_decoder(_source.AvCodecParameters->codec_id);
        _codecCtx = avcodec_alloc_context3(codec);
        _frame = av_frame_alloc();
        var ret = avcodec_parameters_to_context(_codecCtx, _source.AvCodecParameters);
        Debug.Assert(ret is 0);

        ret = avcodec_open2(_codecCtx, codec, null);
        Debug.Assert(ret >= 0);
    }

    public unsafe bool MoveNext()
    {
        if (_isDisposed || !_source.MoveNext()) return false;
        var pkt = _source.Current;
        var ret = avcodec_send_packet(_codecCtx, (AVPacket*)pkt);
        if (ret < 0) return false;
        ret = avcodec_receive_frame(_codecCtx, _frame);
        return ret >= 0;
    }

    public void Reset() => _source.Reset();

    /// <summary>
    /// pointer to <see cref="AVFrame"/>
    /// </summary>
    public unsafe IntPtr Current => (IntPtr)_frame;

    /// <inheritdoc cref="Current"/>
    object IEnumerator.Current => Current;

    /// <summary>
    /// pointer to <see cref="AVCodecContext"/>
    /// </summary>
    public unsafe IntPtr AvCodecCtx => (IntPtr)_codecCtx;

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing && _isDisposed) return;
        _source.Dispose();
        av_frame_unref(_frame);
        avcodec_close(_codecCtx);
        _isDisposed = true;
    }

    ~FFmpegDecoder()
    {
        Dispose(!_isDisposed);
    }
}