using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Exceptions;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// provide pointer of <see cref="AVPacket"/> and some meta information of the stream
/// </summary>
public class FFmpegSource : ISoundSource, IAudioMetaInfo, IDisposable, IAsyncEnumerable<IntPtr>
{
    private readonly object _lock = new();
    private readonly unsafe AVFormatContext* _formatCtx;
    private readonly unsafe AVStream* _stream;
    private readonly int _streamIndex;
    private readonly IAsyncEnumerator<IntPtr> _pktEnumerator;
    private bool _isDisposed;

    private static readonly AVRational Second2Ticks =
        new() { num = 1, den = (int)TimeSpan.TicksPerSecond }; // is mean 1/10000000

    public unsafe FFmpegSource(Uri uri)
    {
        Uri = uri;
        int ret;
        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            ret = avformat_open_input(formatCtx, uri.OriginalString, null, null);
            if (ret < 0)
            {
                throw new InvalidOperationException($"Cannot open uri {uri.OriginalString}");
            }
        }

        ret = avformat_find_stream_info(_formatCtx, null);
        if (ret < 0)
        {
            throw new ArgumentException($@"Could not find stream information from {uri.OriginalString}", nameof(uri));
        }

        for (var i = 0; i < _formatCtx->nb_streams; i++)
        {
            if (_formatCtx->streams[i]->codecpar->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO) continue;
            _stream = _formatCtx->streams[_streamIndex = i];
            break;
        }

        _pktEnumerator = new PacketAsyncEnumerator(this, _formatCtx, _streamIndex);

        Format = FFmpegExtensions.GetSampleFormat(_stream->codecpar);
    }

    public Uri Uri { get; }
    public unsafe TimeSpan Duration => TimeSpan.FromTicks(_formatCtx->duration * 10); // 1us = 10tick

    public unsafe TimeSpan Position
    {
        get
        {
            var pkt = (AVPacket*)_pktEnumerator.Current;
            if (pkt->duration <= 0) return TimeSpan.Zero;
            var dts = pkt->dts;

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
    public unsafe int BitDepth => FFmpegExtensions.GetBitDepth(_stream->codecpar);
    public unsafe int Channels => _stream->codecpar->ch_layout.nb_channels;
    public unsafe AVChannelLayout ChannelLayout => _stream->codecpar->ch_layout;
    public unsafe int SampleRate => _stream->codecpar->sample_rate;
    public SampleFormat Format { get; }

    internal unsafe AVCodecParameters* AvCodecParameters => _stream->codecpar;

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing) return;
        var task = _pktEnumerator.DisposeAsync();
        while (!task.IsCompleted)
        {
        }

        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            avformat_close_input(formatCtx);
        }

        _isDisposed = true;
    }

    /// <summary>
    /// get async enumerator to iteration <see cref="IntPtr"/> of <see cref="AVPacket"/>
    /// </summary>
    /// <param name="cancellationToken">inherit from interface, but is will ignore</param>
    /// <returns><see cref="IntPtr"/>point to <see cref="AVPacket"/></returns>
    public IAsyncEnumerator<IntPtr> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _pktEnumerator;
    }

    public delegate void FFmpegSourceEofHandler(FFmpegSource sender);

    public event FFmpegSourceEofHandler? SourceEofEvent;

    private class PacketAsyncEnumerator : IAsyncEnumerator<IntPtr>
    {
        private readonly FFmpegSource _owner;
        private readonly unsafe AVFormatContext* _ctx;
        private readonly int _index;
        private readonly unsafe AVPacket* _pkt;
        private bool _isDisposed;

        public unsafe PacketAsyncEnumerator(FFmpegSource owner, AVFormatContext* ctx, int index)
        {
            _owner = owner;
            _ctx = ctx;
            _index = index;
            _pkt = av_packet_alloc();
        }

        public unsafe IntPtr Current => (IntPtr)_pkt;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_isDisposed) return false;
            var ret = 0;
            await Task.Run(() =>
            {
                unsafe
                {
                    do
                    {
                        av_packet_unref(_pkt);
                        ret = av_read_frame(_ctx, _pkt);
                    } while (ret >= 0 && _pkt->stream_index != _index);
                }
            });

            if (ret >= 0) return true;

            if (ret != AVERROR_EOF) throw new FFmpegReadingFrameException(ret);

            // invoke event from another thread because this method may call by AudioCallback, it will block the thread
            InvokeEventAsync();

            return false;
        }

        private async void InvokeEventAsync()
        {
            await Task.Run(() => _owner.SourceEofEvent?.Invoke(_owner));
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        private ValueTask DisposeAsync(bool disposing)
        {
            if (!disposing && _isDisposed) return ValueTask.CompletedTask;
            ReleaseUnmanagedResource();
            _isDisposed = true;
            return ValueTask.CompletedTask;
        }

        private unsafe void ReleaseUnmanagedResource()
        {
            av_packet_unref(_pkt);
        }

        ~PacketAsyncEnumerator()
        {
            ReleaseUnmanagedResource();
        }
    }

    ~FFmpegSource()
    {
        Dispose();
    }
}