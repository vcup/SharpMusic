using System.Collections;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Exceptions;
using SharpMusic.DllHellP.Utils;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// provide pointer of <see cref="AVPacket"/> and some meta information of the stream
/// </summary>
public class FFmpegSource : ISoundSource, IAudioMetaInfo, IEnumerator<IntPtr>
{
    private readonly object _lock = new();
    private readonly unsafe AVFormatContext* _formatCtx;
    private readonly unsafe AVPacket* _pkt;
    private unsafe AVStream* _stream;
    private int _streamIndex;
    private bool _isDisposed;

    private static readonly AVRational Second2Ticks =
        new() { num = 1, den = (int)TimeSpan.TicksPerSecond }; // is mean 1/10000000

    public unsafe FFmpegSource(Uri uri)
    {
        Uri = uri;
        _stream = null;
        _streamIndex = -1;
        int ret;
        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            ret = avformat_open_input(formatCtx, uri.OriginalString, null, null);
            if (ret < 0)
            {
                throw new FFmpegOpenInputException($"Cannot open uri {uri.OriginalString}", ret);
            }
        }

        ret = avformat_find_stream_info(_formatCtx, null);
        if (ret < 0)
        {
            // hasn't way to make an file to cover FFmpegFindStreamException
            // dotCover disable next line
            throw new FFmpegFindStreamException($@"Could not find stream information from {uri.OriginalString}", ret);
        }

        _pkt = av_packet_alloc();
    }

    public Uri Uri { get; }

    public unsafe TimeSpan Duration => TimeSpan.FromTicks(_formatCtx->duration * 10); // 1us = 10tick

    public unsafe TimeSpan Position
    {
        get
        {
            if (_pkt->duration <= 0) return TimeSpan.Zero;
            var dts = _pkt->dts;

            var timeBase = _stream->time_base;
            // av_rescale_q -> a*b/c
            return TimeSpan.FromTicks(av_rescale_q(dts, timeBase, Second2Ticks));
        }
        set
        {
            if (value.Ticks is 0) ResetStream();
            else SeekStream(value);
        }
    }

    public unsafe void ResetStream()
    {
        av_seek_frame(_formatCtx, _streamIndex, 0, AVSEEK_FLAG_BYTE);
    }

    public unsafe void SeekStream(TimeSpan time)
    {
        // timestamp unit is us, 1us = 10Tick
        var timestamp = av_rescale_q(time.Ticks / 10, FFmpegHelper.AV_TIME_BASE_Q, _stream->time_base);

        lock (_lock)
        {
            av_seek_frame(_formatCtx, _streamIndex, timestamp, AVSEEK_FLAG_FRAME);
        }
    }

    public unsafe long BitRate => _formatCtx->bit_rate;
    public unsafe int BitDepth => FFmpegHelper.GetBitDepth(_stream->codecpar);
    public unsafe int Channels => _stream->codecpar->ch_layout.nb_channels;
    public unsafe AVChannelLayout ChannelLayout => _stream->codecpar->ch_layout;
    public unsafe int SampleRate => _stream->codecpar->sample_rate;
    public SampleFormat Format { get; private set; }

    internal unsafe AVCodecParameters* AvCodecParameters => _stream->codecpar;

    public unsafe bool WritePacket()
    {
        if (!_pkt->time_base.Equals(_stream->time_base))
            av_packet_rescale_ts(_pkt, _pkt->time_base, _stream->time_base);
        _pkt->stream_index = _streamIndex;
        var ret = av_interleaved_write_frame(_formatCtx, _pkt);
        if (ret < 0) throw new FFmpegException(ret);
        return true;
    }

    public delegate void FFmpegSourceEofHandler(FFmpegSource sender);

    public unsafe bool MoveNext()
    {
        if (_isDisposed) return false;

        av_packet_unref(_pkt);
        var ret = av_read_frame(_formatCtx, _pkt);

        if (_pkt->stream_index != _streamIndex)
        {
            _stream = _formatCtx->streams[_streamIndex = _pkt->stream_index];
            Format = _stream->codecpar->codec_type is AVMediaType.AVMEDIA_TYPE_AUDIO
                ? FFmpegHelper.GetSampleFormat((AVSampleFormat)_stream->codecpar->format)
                : SampleFormat.None;
        }

        _pkt->time_base = _stream->time_base;
        if (ret >= 0) return true;

        if (ret != AVERROR_EOF) throw new FFmpegReadingFrameException(ret);


        // invoke event from another thread because this method normally called
        // SDL_CloseAudioDevice() in they AudioCallback on event register
        // it will block the thread
        InvokeEventAsync();

        return false;
    }

    private async void InvokeEventAsync()
    {
        await Task.Run(() => SourceEofEvent?.Invoke(this));
    }

    public event FFmpegSourceEofHandler? SourceEofEvent;

    /// <inheritdoc cref="ResetStream"/>
    public void Reset() => ResetStream();

    /// <summary>
    /// pointer to <see cref="AVPacket"/>
    /// </summary>
    public unsafe IntPtr Current => (IntPtr)_pkt;

    /// <inheritdoc cref="Current"/>
    object IEnumerator.Current => Current;

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed) return;
        fixed (AVPacket** pkt = &_pkt)
        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            avformat_close_input(formatCtx);
            av_packet_free(pkt);
        }

        _isDisposed = true;
    }

    ~FFmpegSource()
    {
        Dispose(!_isDisposed);
    }
}