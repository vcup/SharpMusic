using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegResampler : IDisposable
{
    private readonly unsafe AVCodecContext* _codecCtx;
    private readonly AVSampleFormat _outFormat;
    private readonly AVChannelLayout _outChannel;
    private readonly int _outSampleRate;
    private readonly unsafe SwrContext* _swrCtx;
    private bool _isDisposed;

    public unsafe FFmpegResampler(AVCodecContext* codecCtx, AVSampleFormat outFormat, AVChannelLayout outChannel,
        int outSampleRate)
    {
        _codecCtx = codecCtx;
        _outFormat = outFormat;
        _outChannel = outChannel;
        _outSampleRate = outSampleRate;
        fixed (SwrContext** swrCtx = &_swrCtx)
        fixed (AVChannelLayout* pOutChannel = &_outChannel)
        {
            var ret = swr_alloc_set_opts2(swrCtx, pOutChannel, _outFormat, _outSampleRate,
                &codecCtx->ch_layout, codecCtx->sample_fmt, codecCtx->sample_rate, 0, null);
            Debug.Assert(ret >= 0);
            ret = swr_init(*swrCtx);
            Debug.Assert(ret is 0);
        }
    }

    public unsafe byte[] ResampleFrame(AVFrame* frame)
    {
        if (_isDisposed) return Array.Empty<byte>();
        long nbSamples;
        var maxNbSamples = nbSamples = av_rescale_rnd(
            frame->nb_samples, _outSampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
        );

        byte** resampledData = null;
        var lineSize = 0;
        var ret = av_samples_alloc_array_and_samples(
            &resampledData, &lineSize, _outChannel.nb_channels, (int)nbSamples, _outFormat, 0
        );
        Debug.Assert(ret >= 0);

        nbSamples = av_rescale_rnd(
            swr_get_delay(_swrCtx, _codecCtx->sample_rate) +
            frame->nb_samples, _outSampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
        );
        Debug.Assert(ret > 0);

        if (nbSamples > maxNbSamples && *resampledData is not null)
        {
            av_free(*resampledData);
            ret = av_samples_alloc(
                resampledData, &lineSize, _outChannel.nb_channels, (int)nbSamples, _outFormat, 1
            );

            Debug.Assert(ret >= 0);
        }

        int resampledDataSize;
        if (_swrCtx is not null)
        {
            ret = swr_convert(_swrCtx, resampledData, (int)nbSamples, frame->extended_data, frame->nb_samples);
            Debug.Assert(ret >= 0);

            resampledDataSize = av_samples_get_buffer_size(&lineSize, _outChannel.nb_channels, ret, _outFormat, 1);
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