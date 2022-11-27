using System.Diagnostics;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// decode <see cref="AVPacket"/> from <see cref="FFmpegSource"/>, provide pointer of <see cref="AVFrame"/>
/// </summary>
public class FFmpegDecoder : IAsyncEnumerable<IntPtr>, IDisposable
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
    /// get async enumerator to iteration <see cref="IntPtr"/> of <see cref="AVFrame"/>
    /// </summary>
    /// <returns><see cref="IntPtr"/> point to <see cref="AVPacket"/></returns>
    public unsafe IAsyncEnumerator<IntPtr> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new AsyncEnumerator(_codecCtx, _source.GetAsyncEnumerator(cancellationToken), cancellationToken);
    }

    private class AsyncEnumerator : IAsyncEnumerator<IntPtr>
    {
        private readonly unsafe AVCodecContext* _codecCtx;
        private readonly IAsyncEnumerator<IntPtr> _packets;
        private readonly CancellationToken _token;
        private readonly unsafe AVFrame* _frame;
        private bool _isDisposed;

        public unsafe AsyncEnumerator(AVCodecContext* codecCtx, IAsyncEnumerator<IntPtr> packets,
            CancellationToken token)
        {
            _codecCtx = codecCtx;
            _packets = packets;
            _token = token;
            _frame = av_frame_alloc();
        }

        public unsafe IntPtr Current => (IntPtr)_frame;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (_isDisposed || _token.IsCancellationRequested || !await _packets.MoveNextAsync()) return false;
            var pkt = _packets.Current;
            unsafe
            {
                var ret = avcodec_send_packet(_codecCtx, (AVPacket*)pkt);
                if (ret < 0) return false;
                ret = avcodec_receive_frame(_codecCtx, _frame);
                return ret >= 0;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsync(!_isDisposed);
            GC.SuppressFinalize(this);
        }

        private async ValueTask DisposeAsync(bool disposing)
        {
            if (!disposing || _isDisposed) return;
            ReleaseUnmanagedResource();
            await _packets.DisposeAsync();
            _isDisposed = true;
        }

        private unsafe void ReleaseUnmanagedResource()
        {
            av_frame_unref(_frame);
        }

        ~AsyncEnumerator()
        {
            ReleaseUnmanagedResource();
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