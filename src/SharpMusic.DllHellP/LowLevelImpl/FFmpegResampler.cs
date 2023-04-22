using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// provide resample frame method, using lib swresample api
/// </summary>
public class FFmpegResampler : IDisposable
{
    private readonly unsafe AVCodecContext* _codecCtx;
    private readonly AVSampleFormat _format;
    private readonly AVChannelLayout _channel;
    private readonly int _sampleRate;
    private readonly bool _isOutput;
    private readonly unsafe SwrContext* _swrCtx;
    private int _wroteIndex; // only for output
    private bool _isDisposed;

    /// <summary>
    /// wrapper core feature for lib swresample, convert samples from a format to another
    /// </summary>
    /// <param name="codecCtx">
    /// pointer of <see cref="AVCodecContext"/>, get frame format from the context
    /// </param>
    /// <param name="format">convert samples data to the format, when isOutput is true, it meaning output format</param>
    /// <param name="channel">convert samples data to the channel layout, similar as format</param>
    /// <param name="sampleRate">convert samples data to the sample rate, similar as format</param>
    /// <param name="isOutput">is use for output from frame when true, otherwise is for input to frame</param>
    public unsafe FFmpegResampler(IntPtr codecCtx, AVSampleFormat format, AVChannelLayout channel,
        int sampleRate, bool isOutput = true)
    {
        _codecCtx = (AVCodecContext*)codecCtx;
        _format = format;
        _channel = channel;
        _sampleRate = sampleRate;
        _isOutput = isOutput;
        fixed (SwrContext** swrCtx = &_swrCtx)
        fixed (AVChannelLayout* pChannel = &_channel)
        {
            var ret = isOutput
                ? swr_alloc_set_opts2(swrCtx, pChannel, _format, _sampleRate,
                    &_codecCtx->ch_layout, _codecCtx->sample_fmt, _codecCtx->sample_rate, 0, null)
                : swr_alloc_set_opts2(swrCtx, &_codecCtx->ch_layout, _codecCtx->sample_fmt, _codecCtx->sample_rate,
                    pChannel, _format, _sampleRate, 0, null);
            Debug.Assert(ret >= 0);
            ret = swr_init(*swrCtx);
            Debug.Assert(ret is 0);
        }
    }

    /// <summary>
    /// convert the <see cref="AVFrame"/> to byte array
    /// </summary>
    /// <remarks>
    /// when disposed or is not initialized for output, return empty array of byte
    /// </remarks>
    /// <param name="frame">
    /// pointer to <see cref="AVFrame"/>, must received from initialized <see cref="AVCodecContext"/>
    /// </param>
    /// <returns>converted byte array with specified argument in constructor</returns>
    public unsafe byte[] ResampleFrame(AVFrame* frame)
    {
        if (!_isOutput || _isDisposed) return Array.Empty<byte>();
        long nbSamples;
        var maxNbSamples = nbSamples = av_rescale_rnd(
            frame->nb_samples, _sampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
        );

        byte** resampledData = null;
        var lineSize = 0;
        var ret = av_samples_alloc_array_and_samples(
            &resampledData, &lineSize, _channel.nb_channels, (int)nbSamples, _format, 0
        );
        Debug.Assert(ret >= 0);

        nbSamples = av_rescale_rnd(
            swr_get_delay(_swrCtx, _codecCtx->sample_rate) +
            frame->nb_samples, _sampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
        );
        Debug.Assert(ret > 0);

        if (nbSamples > maxNbSamples && *resampledData is not null)
        {
            av_free(*resampledData);
            ret = av_samples_alloc(
                resampledData, &lineSize, _channel.nb_channels, (int)nbSamples, _format, 1
            );

            Debug.Assert(ret >= 0);
        }

        int resampledDataSize;
        if (_swrCtx is not null)
        {
            ret = swr_convert(_swrCtx, resampledData, (int)nbSamples, frame->extended_data, frame->nb_samples);
            Debug.Assert(ret >= 0);

            resampledDataSize = av_samples_get_buffer_size(&lineSize, _channel.nb_channels, ret, _format, 1);
            Debug.Assert(resampledDataSize >= 0);
        }
        else
        {
            throw new NullReferenceException();
        }

        var result = new byte[resampledDataSize];

        Marshal.Copy((IntPtr)(*resampledData), result, 0, resampledDataSize);

        if (*resampledData is not null)
        {
            av_freep(resampledData);
        }

        av_freep(&resampledData);

        return result;
    }

    /// <summary>
    /// write samples into frame with bytes array
    /// </summary>
    /// <example>
    /// usage: see also <see cref="FFmpegCodec.CreateEncoder"/>
    /// <code><c>
    /// ... make decoder and encoder, see FFmpeg.Codec ...
    /// var resampler = new FFmpegResampler(encoder.AvCodecCtx, inSource.Format.ToFmt(), inSource.ChannelLayout, inSource.SampleRate, false);
    /// var inCodecCtx = (AVCodecContext*)decoder.AvCodecCtx;
    /// var isPlanar = av_sample_fmt_is_planar(inCodecCtx->sample_fmt) is not 0;
    /// var height = isPlanar ? inCodecCtx->ch_layout.nb_channels : 1;
    /// var remainingSamples = 0;
    /// var frame = (AVFrame*)decoder.Current;
    /// var dFrame = (AVFrame*)encoder.Current;
    /// do
    /// {
    ///     if (remainingSamples >= 0)
    ///     {
    ///         if (!decoder.MoveNext()) break;
    ///         var lineSize = frame->nb_samples * av_get_bytes_per_sample(inCodecCtx->sample_fmt);
    ///         if (!isPlanar) lineSize *= inCodecCtx->ch_layout.nb_channels;
    ///         var data = new byte[height, lineSize];
    ///         fixed (byte* pData = data)
    ///         {
    ///             if (isPlanar)
    ///             {
    ///                 var lpData = pData;
    ///                 for (var i = 0; i &lt; height; i++)
    ///                 {
    ///                     Buffer.MemoryCopy(frame->extended_data[i], lpData, lineSize, lineSize);
    ///                     lpData += lineSize;
    ///                 }
    ///             }
    ///             else Buffer.MemoryCopy(frame->extended_data[0], pData, lineSize, lineSize);
    ///         }
    ///         if (resampler.WriteFrame(dFrame, data, out remainingSamples)) encoder.EncodeFrameAndWrite();
    ///     }
    ///     else if (resampler.WriteFrame(dFrame, new byte[height, 0], out remainingSamples))
    ///         encoder.EncodeFrameAndWrite();
    /// } while (true);
    /// ......
    /// </c></code>
    /// </example>
    /// <param name="frame">the <see cref="AVFrame"/> will write</param>
    /// <param name="samples">samples, width i.e line size must evenly divisible the bytes per sample</param>
    /// <param name="netRemainingSamples">
    /// remaining samples, zero when all samples has wrote,
    /// negative meaning cached samples, positive meaning need fill more samples into frame.
    /// </param>
    /// <returns>
    /// return true when all input already write into frame or the frame already fill up samples
    /// </returns>
    /// <exception cref="NotSupportedException">the resampler is init with output or already disposed</exception>
    public unsafe bool WriteFrame(AVFrame* frame, byte[,] samples, out int netRemainingSamples)
    {
        if (_isOutput || _isDisposed) throw new NotSupportedException();

        var isPacked = av_sample_fmt_is_planar(_format) is 0;
        var inLineSize = samples.GetLength(1);
        var inSamples = inLineSize / av_get_bytes_per_sample(_format);
        if (isPacked) inSamples /= _channel.nb_channels;

        var frameFormat = (AVSampleFormat)frame->format;
        var frameIsPacket = av_sample_fmt_is_planar(frameFormat) is 0;
        var ppFrameSamples = stackalloc byte*[frameIsPacket ? 1 : frame->ch_layout.nb_channels];
        if (frameIsPacket)
        {
            ppFrameSamples[0] = frame->extended_data[0] + _wroteIndex * av_get_bytes_per_sample(frameFormat) *
                _channel.nb_channels;
        }
        else
        {
            for (var i = 0; i < frame->ch_layout.nb_channels; i++)
            {
                ppFrameSamples[i] = frame->extended_data[i] + _wroteIndex * av_get_bytes_per_sample(frameFormat);
            }
        }

        var ppSamples = stackalloc byte*[isPacked ? 1 : _channel.nb_channels];
        fixed (byte* pSamples = samples)
        {
            if (isPacked)
            {
                ppSamples[0] = pSamples;
            }
            else
            {
                for (var i = 0; i < _channel.nb_channels; i++)
                {
                    ppSamples[i] = pSamples + samples.GetLength(1) * i;
                }
            }

            var ret = swr_convert(_swrCtx, ppFrameSamples, frame->nb_samples - _wroteIndex, ppSamples, inSamples);
            _wroteIndex += ret;
        }
        var bufferSamples = swr_get_delay(_swrCtx, _sampleRate);
        var freeBuffer = frame->nb_samples - _wroteIndex;
        netRemainingSamples = freeBuffer - (int)bufferSamples;

        if (freeBuffer is not 0) return false;
        _wroteIndex = 0;
        return true;
    }

    public void Dispose()
    {
        Dispose(!_isDisposed);
        GC.SuppressFinalize(this);
    }

    private unsafe void Dispose(bool disposing)
    {
        if (!disposing && _isDisposed) return;
        fixed (SwrContext** swrCtx = &_swrCtx)
        {
            swr_free(swrCtx);
        }

        _isDisposed = true;
    }
}