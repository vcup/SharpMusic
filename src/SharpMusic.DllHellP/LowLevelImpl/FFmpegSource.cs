using System.Collections;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegSource : ISoundSource, IAudioMetaInfo, IDisposable, IEnumerable<AVPacket>
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

        Format = (*_stream->codecpar).ToSampleFormat();
    }

    public Uri Uri { get; }
    public unsafe TimeSpan Duration => TimeSpan.FromTicks(_formatCtx->duration * 10); // 1tick = 10us
    public TimeSpan Position { get; set; }
    public unsafe long BitRate => _formatCtx->bit_rate;
    public unsafe int BitDepth => _stream->codecpar->bits_per_coded_sample;
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

        fixed (AVFormatContext** formatCtx = &_formatCtx)
        {
            avformat_close_input(formatCtx);
        }
        _isDisposed = true;
    }

    public unsafe IEnumerator<AVPacket> GetEnumerator()
    {
        return new PacketEnumerator(_formatCtx);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class PacketEnumerator : IEnumerator<AVPacket>
    {
        private readonly unsafe AVFormatContext* _ctx;
        private readonly unsafe AVPacket* _pkt;

        public unsafe PacketEnumerator(AVFormatContext* ctx)
        {
            _ctx = ctx;
            _pkt = av_packet_alloc();
        }

        public unsafe void Dispose()
        {
            av_packet_unref(_pkt);
        }

        public unsafe bool MoveNext()
        {
            var ret = av_read_frame(_ctx, _pkt);

            return ret >= 0;
        }

        public unsafe AVPacket Current => *_pkt;

        object IEnumerator.Current => Current;

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }

    ~FFmpegSource()
    {
        Dispose();
    }
}