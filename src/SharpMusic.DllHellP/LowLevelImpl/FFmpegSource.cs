using System.Collections;
using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// provide pointer of <see cref="AVPacket"/> and some meta information of the stream
/// </summary>
public class FFmpegSource : ISoundSource, IAudioMetaInfo, IDisposable, IEnumerable<IntPtr>
{
    private readonly unsafe AVFormatContext* _formatCtx;
    private readonly unsafe AVStream* _stream;
    private readonly int _streamIndex;
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
            _stream = _formatCtx->streams[_streamIndex = i];
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

    /// <summary>
    /// get enumerator to iteration <see cref="IntPtr"/> of <see cref="AVPacket"/>
    /// </summary>
    /// <returns><see cref="IntPtr"/> point to <see cref="AVPacket"/></returns>
    public unsafe IEnumerator<IntPtr> GetEnumerator()
    {
        return new PacketEnumerator(_formatCtx, _streamIndex);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private class PacketEnumerator : IEnumerator<IntPtr>
    {
        private readonly unsafe AVFormatContext* _ctx;
        private readonly int _index;
        private readonly unsafe AVPacket* _pkt;
        private bool _isDisposed;

        public unsafe PacketEnumerator(AVFormatContext* ctx, int index)
        {
            _ctx = ctx;
            _index = index;
            _pkt = av_packet_alloc();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (!disposing && _isDisposed) return;
            av_packet_unref(_pkt);
            _isDisposed = true;
        }

        public unsafe bool MoveNext()
        {
            if (_isDisposed) return false;
            av_packet_unref(_pkt);
            var ret = av_read_frame(_ctx, _pkt);
            while (ret >= 0 && _pkt->stream_index != _index)
            {
                ret = av_read_frame(_ctx, _pkt);
            }

            return ret >= 0;
        }

        public unsafe IntPtr Current => (IntPtr)_pkt;

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