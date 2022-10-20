using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegSource : ISoundSource, IDisposable, IAsyncEnumerable<AVPacket>
{
    private readonly unsafe AVFormatContext* _formatCtx;
    private readonly unsafe AVStream* _stream;
    private bool _isDisposed;

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
        }

        for (var i = 0; i < _formatCtx->nb_streams; i++)
        {
            if (_formatCtx->streams[i]->codecpar->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO) continue;
            _stream = _formatCtx->streams[i];
            break;
        }

        Format = SampleFormat.None; // tmp
    }

    public Uri Uri { get; }
    public unsafe TimeSpan Duration => TimeSpan.FromTicks(_formatCtx->duration * 10); // 1tick = 10us
    public TimeSpan Position { get; set; }
    public unsafe long BitRate => _formatCtx->bit_rate;
    public unsafe int BitDepth => _stream->codecpar->bits_per_coded_sample;
    public unsafe int Channels => _stream->codecpar->ch_layout.nb_channels;
    public unsafe int SampleRate => _stream->codecpar->sample_rate;
    public SampleFormat Format { get; }

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing) return;

        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            avformat_close_input(formatCtx);
        }
        _isDisposed = true;
    }

    public unsafe IAsyncEnumerator<AVPacket> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        return new PacketEnumerator(_formatCtx, cancellationToken);
    }

    private class PacketEnumerator : IAsyncEnumerator<AVPacket>
    {
        private readonly CancellationToken _token;
        private readonly unsafe AVFormatContext* _ctx;
        private readonly unsafe AVPacket* _pkt;

        public unsafe PacketEnumerator(AVFormatContext* ctx, CancellationToken token)
        {
            _token = token;
            _ctx = ctx;
            _pkt = av_packet_alloc();
        }

        public unsafe ValueTask DisposeAsync()
        {
            av_packet_unref(_pkt);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            await Task.Delay(1, _token);
            if (_token.IsCancellationRequested)
            {
                await DisposeAsync();
                return false;
            }

            int ret;

            unsafe
            {
                ret = av_read_frame(_ctx, _pkt);
            }

            return ret >= 0;
        }

        public unsafe AVPacket Current => *_pkt;
    }

    ~FFmpegSource()
    {
        Dispose();
    }
}