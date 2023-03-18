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
    private long _totalSamples; // only for encoder
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
        _frame = av_frame_alloc();
        _frame->sample_rate = _codecCtx->sample_rate;
        if (isDecoder)
        {
            avcodec_send_packet(_codecCtx, _packet);
            return;
        }
        _frame->nb_samples = _codecCtx->frame_size;
        if (_codecCtx->frame_size is 0) _frame->nb_samples = _codecCtx->frame_size = 1024;
        _frame->format = (int)_codecCtx->sample_fmt;
        ret = av_channel_layout_copy(&_frame->ch_layout, &_codecCtx->ch_layout);
        if (ret < 0) throw new FFmpegException(ret);
        ret = av_frame_get_buffer(_frame, 0);
        if (ret < 0) throw new FFmpegException(ret);
    }

    public static FFmpegCodec CreateDecoder(FFmpegSource source) => new(source, true);

    /// <summary>
    /// create encoder with <see cref="FFmpegSource"/> for encoding and write frame into the source.
    /// </summary>
    /// <example>
    /// usage:
    /// <code><c>
    /// using var source = new FFmpegSource(...);
    /// var codecPar = avcodec_parameters_alloc();
    /// codecPar->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;
    /// codecPar->codec_id = AVCodecID.AV_CODEC_ID_FLAC;
    /// FFmpegHelper.TurningParameters(codecPar);
    /// source.AddStream(codecPar);
    /// source.WriteHeader();
    /// using var encoder = FFmpegCodec.CreateEncoder(source);
    /// ...
    /// </c></code>
    /// normally need set CodecId and turning the parameters as will,
    /// that can be use best codec parameter and frame size instead of 1024 samples per frame.
    /// </example>
    /// <param name="source">the encoder will write frame as packet into the source</param>
    /// <returns>instance of <see cref="FFmpegCodec"/> for encode frame</returns>
    public static FFmpegCodec CreateEncoder(FFmpegSource source) => new(source, false);

    public unsafe bool EncodeFrameAndWrite()
    {
        if (_isDecoder || _isDisposed) return false;
        _frame->pts = _totalSamples += _frame->nb_samples;
        var ret = avcodec_send_frame(_codecCtx, _frame);
        Debug.Assert(ret >= 0);
        do
        {
            ret = avcodec_receive_packet(_codecCtx, _packet);
            _packet->time_base = _codecCtx->time_base;
            _packet->stream_index = _streamIndex;
            if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) break;
            if (ret < 0) throw new FFmpegException(ret);
        } while (_source.WritePacket());

        return true;
    }

    public unsafe void FlushAndCloseEncoder()
    {
        if (_isDecoder || _isDisposed) return;
        var ret = avcodec_send_frame(_codecCtx, null);
        Debug.Assert(ret >= 0);
        do
        {
            ret = avcodec_receive_packet(_codecCtx, _packet);
            _packet->time_base = _codecCtx->time_base;
            _packet->stream_index = _streamIndex;
            if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) break;
            if (ret < 0) throw new FFmpegException(ret);
        } while (_source.WritePacket());

        Dispose();
    }

    public unsafe bool MoveNext()
    {
        if (!_isDecoder || _isDisposed) return false;
        while (true)
        {
            var ret = avcodec_receive_frame(_codecCtx, _frame);
            if (ret is 0 || ret == AVERROR(AVERROR_EOF)) break;
            if (ret == AVERROR(EAGAIN))
            {
                if (!_source.MoveNext(_streamIndex)) return false;
                av_packet_rescale_ts(_packet, _packet->time_base, _codecCtx->time_base);
                ret = avcodec_send_packet(_codecCtx, _packet);
            }

            if (ret < 0) throw new FFmpegException(ret);
        }

        _frame->time_base = _codecCtx->time_base;
        return true;
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
        fixed (AVFrame** frame = &_frame)
        {
            av_frame_free(frame);
        }
        avcodec_close(_codecCtx);
        _isDisposed = true;
    }

    ~FFmpegCodec()
    {
        Dispose(!_isDisposed);
    }
}