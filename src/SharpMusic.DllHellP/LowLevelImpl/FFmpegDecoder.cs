using System.Collections;
using System.ComponentModel;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegDecoder : IEnumerable<AVFrame>, IDisposable
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
        return new Enumerator(_codecCtx, _source.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    private class Enumerator : IEnumerator<AVFrame>
    {
        private readonly unsafe AVCodecContext* _codecCtx;
        private readonly IEnumerator<AVPacket> _packets;
        private readonly unsafe AVFrame* _frame;
        private bool _isDisposed;

        public unsafe Enumerator(AVCodecContext* codecCtx, IEnumerator<AVPacket> packets)
        {
            _codecCtx = codecCtx;
            _packets = packets;
            _frame = av_frame_alloc();
        }

        public unsafe bool MoveNext()
        {
            if (!_packets.MoveNext()) return false;
            var pkt = _packets.Current;
            var ret = avcodec_send_packet(_codecCtx, &pkt);
            if (ret < 0) return false;
            ret = avcodec_receive_frame(_codecCtx, _frame);
            return ret >= 0;
        }

        public void Reset()
        {
            _packets.Reset();
        }

        public unsafe AVFrame Current => *_frame;

        object IEnumerator.Current => Current;

        public unsafe void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private unsafe void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) return;
            av_frame_unref(_frame);
            _isDisposed = true;
        }

        ~Enumerator()
        {
            Dispose();
        }
    }

    public unsafe AVCodecContext* AvCodecCtx => _codecCtx;

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