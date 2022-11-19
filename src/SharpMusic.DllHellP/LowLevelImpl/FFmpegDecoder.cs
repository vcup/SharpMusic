using System.Collections;
using System.Diagnostics;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// decode <see cref="AVPacket"/> from <see cref="FFmpegSource"/>, provide pointer of <see cref="AVFrame"/>
/// </summary>
public class FFmpegDecoder : IEnumerable<IntPtr>, IDisposable
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
        Debug.Assert(ret is 0);

        ret = avcodec_open2(_codecCtx, codec, null);
        Debug.Assert(ret >= 0);
    }

    
    /// <summary>
    /// get enumerator to iteration <see cref="IntPtr"/> of <see cref="AVFrame"/>
    /// </summary>
    /// <returns><see cref="IntPtr"/> point to <see cref="AVPacket"/></returns>
    public unsafe IEnumerator<IntPtr> GetEnumerator()
    {
        return new Enumerator(_codecCtx, _source.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    private class Enumerator : IEnumerator<IntPtr>
    {
        private readonly unsafe AVCodecContext* _codecCtx;
        private readonly IEnumerator<IntPtr> _packets;
        private readonly unsafe AVFrame* _frame;
        private bool _isDisposed;

        public unsafe Enumerator(AVCodecContext* codecCtx, IEnumerator<IntPtr> packets)
        {
            _codecCtx = codecCtx;
            _packets = packets;
            _frame = av_frame_alloc();
        }

        public unsafe bool MoveNext()
        {
            if (_isDisposed || !_packets.MoveNext()) return false;
            var pkt = _packets.Current;
            var ret = avcodec_send_packet(_codecCtx, (AVPacket*)pkt);
            if (ret < 0) return false;
            ret = avcodec_receive_frame(_codecCtx, _frame);
            return ret >= 0;
        }

        public void Reset()
        {
            _packets.Reset();
        }

        public unsafe IntPtr Current => (IntPtr)_frame;

        object IEnumerator.Current => Current;

        public void Dispose()
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
        avcodec_close(_codecCtx);
        _isDisposed = true;
    }
}