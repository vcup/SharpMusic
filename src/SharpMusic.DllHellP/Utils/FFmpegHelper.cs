using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.Utils;

public static class FFmpegHelper
{
    // ReSharper disable InconsistentNaming
    private const int ErrorMessageBufferSize = 1024;
    public static readonly AVRational AV_TIME_BASE_Q = new() { num = 1, den = AV_TIME_BASE };
    // ReSharper restore InconsistentNaming

    /// <summary>
    /// get stringify ffmpeg error message from error code
    /// </summary>
    /// <param name="errorCode">getting message from error code</param>
    /// <returns>ffmpeg error message of error code</returns>
    public static unsafe string AvStringErrorCode(int errorCode)
    {
        if (errorCode > 0) errorCode = -errorCode;
        var buffer = stackalloc byte[ErrorMessageBufferSize];
        av_strerror(errorCode, buffer, ErrorMessageBufferSize);
        var message = Marshal.PtrToStringAnsi((IntPtr)buffer);
        return message ?? string.Empty;
    }

    /// <summary>
    /// mapping parameters->format to <see cref="SampleFormat"/>
    /// </summary>
    /// <remarks>
    /// <seealso cref="GetSampleFormat(AVSampleFormat)"/>
    /// </remarks>
    /// <param name="parameters"></param>
    /// <returns>mapped <see cref="SampleFormat"/> from parameters->format</returns>
    /// <exception cref="ArgumentException">
    /// parameters->codec_type is not <see cref="AVMediaType.AVMEDIA_TYPE_AUDIO"/>
    /// </exception>
    public static unsafe SampleFormat GetSampleFormat(AVCodecParameters* parameters)
    {
        if (parameters->codec_type is AVMediaType.AVMEDIA_TYPE_AUDIO)
            return GetSampleFormat((AVSampleFormat)parameters->format);
        var message = $"codec_type must be {AVMediaType.AVMEDIA_TYPE_AUDIO} instead of {parameters->codec_type}";
        throw new ArgumentException(message, nameof(parameters));
    }

    /// <summary>
    /// mapping <see cref="AVSampleFormat"/> to <see cref="SampleFormat"/>
    /// </summary>
    /// <remarks>
    /// argument 'format' can cast from <see cref="AVFrame"/>.<see cref="AVFrame.format"/><br/>
    /// see https://ffmpeg.org/doxygen/trunk/structAVFrame.html#aed14fa772ce46881020fd1545c86432c
    /// </remarks>
    /// <param name="format">format of <see cref="AVSampleFormat"/></param>
    /// <returns>mapped <see cref="SampleFormat"/> from <see cref="AVSampleFormat"/></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// format out of range in enum <see cref="AVSampleFormat"/>
    /// </exception>
    public static SampleFormat GetSampleFormat(AVSampleFormat format) => format switch
    {
        AVSampleFormat.AV_SAMPLE_FMT_NONE => SampleFormat.None,
        AVSampleFormat.AV_SAMPLE_FMT_U8 => SampleFormat.Unsigned8,
        AVSampleFormat.AV_SAMPLE_FMT_S16 => SampleFormat.Signed16,
        AVSampleFormat.AV_SAMPLE_FMT_S32 => SampleFormat.Signed32,
        AVSampleFormat.AV_SAMPLE_FMT_FLT => SampleFormat.Float32,
        AVSampleFormat.AV_SAMPLE_FMT_DBL => SampleFormat.Double,
        AVSampleFormat.AV_SAMPLE_FMT_U8P => SampleFormat.Unsigned8Planar,
        AVSampleFormat.AV_SAMPLE_FMT_S16P => SampleFormat.Signed16Planar,
        AVSampleFormat.AV_SAMPLE_FMT_S32P => SampleFormat.Signed32Planar,
        AVSampleFormat.AV_SAMPLE_FMT_FLTP => SampleFormat.Float32Planar,
        AVSampleFormat.AV_SAMPLE_FMT_DBLP => SampleFormat.DoublePlanar,
        AVSampleFormat.AV_SAMPLE_FMT_S64 => SampleFormat.Signed64,
        AVSampleFormat.AV_SAMPLE_FMT_S64P => SampleFormat.Signed64Planar,
        AVSampleFormat.AV_SAMPLE_FMT_NB => SampleFormat.Other,
        // TODO: throw message too vague
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
    };

    public static unsafe int GetBitDepth(AVCodecParameters* parameters)
    {
        return parameters->bits_per_raw_sample is not 0
            ? parameters->bits_per_raw_sample
            : parameters->bits_per_coded_sample;
    }

    /// <summary>
    /// turning the sample_fmt|sample_rate|ch_layout of <see cref="AVCodecContext"/> as best supported
    /// </summary>
    /// <remarks>the par->codec_id is required</remarks>
    public static unsafe void TurningParameters(AVCodecParameters* par)
    {
        if (par->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO) return;
        // TODO: test it can only use encoder to find best support parameters
        var codec = avcodec_find_encoder(par->codec_id);
        // use first support sample format
        par->format = (int)codec->sample_fmts[0];

        // select best sample rate
        {
            var p = codec->supported_samplerates;
            var bp = 0;
            while (p is not null && *p is not 0)
            {
                if (bp is 0 || Math.Abs(44100 - *p) < Math.Abs(44100 - bp))
                {
                    bp = *p;
                }

                p++;
            }

            if (p is not null && *p is not 0) par->sample_rate = bp;
            else par->sample_rate = 44100;
        }
        // select best channel layout
        {
            var p = codec->ch_layouts;
            AVChannelLayout* bc = null;
            while (p is not null && p->nb_channels is not 0)
            {
                if (bc is null || p->nb_channels > bc->nb_channels) bc = p;
                p++;
            }

            if (bc is not null) av_channel_layout_copy(&par->ch_layout, bc);
            else
            {
                var src = new AVChannelLayout
                {
                    order = AVChannelOrder.AV_CHANNEL_ORDER_NATIVE,
                    nb_channels = 2,
                    u = new AVChannelLayout_u
                    {
                        mask = AV_CH_LAYOUT_STEREO,
                    }
                };
                av_channel_layout_copy(&par->ch_layout, &src);
            }
        }
    }
}