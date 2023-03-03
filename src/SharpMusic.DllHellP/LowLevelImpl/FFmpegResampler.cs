using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// provide resample frame method, using swr api
/// </summary>
public class FFmpegResampler : IDisposable
{
    private readonly unsafe AVCodecContext* _codecCtx;
    private readonly AVSampleFormat _format;
    private readonly AVChannelLayout _channel;
    private readonly int _sampleRate;
    private readonly bool _isOutput;
    private readonly unsafe SwrContext* _swrCtx;
    private bool _isDisposed;

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

    public unsafe byte[] ResampleFrame(IntPtr frame)
    {
        if (!_isOutput || _isDisposed) return Array.Empty<byte>();
        var pFrame = (AVFrame*)frame;
        long nbSamples;
        var maxNbSamples = nbSamples = av_rescale_rnd(
            pFrame->nb_samples, _sampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
        );

        byte** resampledData = null;
        var lineSize = 0;
        var ret = av_samples_alloc_array_and_samples(
            &resampledData, &lineSize, _channel.nb_channels, (int)nbSamples, _format, 0
        );
        Debug.Assert(ret >= 0);

        nbSamples = av_rescale_rnd(
            swr_get_delay(_swrCtx, _codecCtx->sample_rate) +
            pFrame->nb_samples, _sampleRate, _codecCtx->sample_rate, AVRounding.AV_ROUND_UP
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
            ret = swr_convert(_swrCtx, resampledData, (int)nbSamples, pFrame->extended_data, pFrame->nb_samples);
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