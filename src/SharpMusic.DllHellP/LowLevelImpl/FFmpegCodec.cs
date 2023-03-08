using System.Collections;
using System.Diagnostics;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Exceptions;
using SharpMusic.DllHellP.Extensions;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// decode <see cref="AVPacket"/> from <see cref="FFmpegSource"/>, provide pointer of <see cref="AVFrame"/>
/// </summary>
public class FFmpegCodec : IEnumerator<IntPtr>
{
    private readonly FFmpegSource _source;
    private readonly bool _isDecoder;
    private readonly unsafe AVCodecContext* _codecCtx;
    private readonly unsafe AVFrame* _frame;
    private readonly unsafe AVPacket* _packet;
    private readonly int _streamIndex;
    private bool _isDisposed;

    private unsafe FFmpegCodec(FFmpegSource source, bool isDecoder)
    {
        _source = source;
        _isDecoder = isDecoder;
        _streamIndex = _source.Stream->index;
        var codec = isDecoder
            ? avcodec_find_decoder(_source.Stream->codecpar->codec_id)
            : avcodec_find_encoder(_source.Stream->codecpar->codec_id);

        _codecCtx = avcodec_alloc_context3(codec);
        var ret = avcodec_parameters_to_context(_codecCtx, _source.Stream->codecpar);
        Debug.Assert(ret is 0);

        ret = avcodec_open2(_codecCtx, codec, null);
        Debug.Assert(ret >= 0);
        _packet = (AVPacket*)source.Current;
        if (!isDecoder) return;
        _frame = av_frame_alloc();
    }

    public static FFmpegCodec CreateDecoder(FFmpegSource source) => new(source, true);
    public static FFmpegCodec CreateEncoder(FFmpegSource source) => new(source, false);

    public unsafe bool EncodeFrameAndWrite(AVFrame* frame)
    {
        if (_isDecoder || _isDisposed) return false;
        var ret = avcodec_send_frame(_codecCtx, frame);
        Debug.Assert(ret >= 0);
        do
        {
            ret = avcodec_receive_packet(_codecCtx, _packet);
            _packet->time_base = _codecCtx->time_base;
            if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) break;
            if (ret < 0) throw new FFmpegException(ret);
        } while (_source.WritePacket());

        return true;
    }

    public unsafe bool MoveNext()
    {
        if (!_isDecoder || _isDisposed || !_source.MoveNext(_streamIndex)) return false;
        var ret = avcodec_send_packet(_codecCtx, _packet);
        if (ret < 0) return false;
        ret = avcodec_receive_frame(_codecCtx, _frame);
        _frame->time_base = _codecCtx->time_base;
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

    ~FFmpegCodec()
    {
        Dispose(!_isDisposed);
    }
}