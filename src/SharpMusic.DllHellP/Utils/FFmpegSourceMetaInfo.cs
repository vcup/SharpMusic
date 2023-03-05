using FFmpeg.AutoGen;
using SharpMusic.DllHellP.Abstract;
using static FFmpeg.AutoGen.ffmpeg;

namespace SharpMusic.DllHellP.Utils;

public class FFmpegSourceMetaInfo : IAudioMetaInfo
{
    private FFmpegSourceMetaInfo()
    {
        Uri = new Uri("");
    }

    public unsafe FFmpegSourceMetaInfo(Uri uri)
    {
        Uri = uri;
        AVFormatContext* formatCtx;
        var ret = avformat_open_input(&formatCtx, uri.OriginalString, null, null);
        
        if (ret < 0)
        {
            throw new InvalidOperationException($"Cannot open uri {uri.OriginalString}");
        }

        ret = avformat_find_stream_info(formatCtx, null);
        if (ret < 0)
        {
            throw new ArgumentException($@"Could not find stream information from {uri.OriginalString}", nameof(uri));
        }
        
        for (var i = 0; i < formatCtx->nb_streams; i++)
        {
            if (formatCtx->streams[i]->codecpar->codec_type is not AVMediaType.AVMEDIA_TYPE_AUDIO) continue;
            var stream = formatCtx->streams[i];
            Duration = TimeSpan.FromTicks(formatCtx->duration * 10);
            BitRate = formatCtx->bit_rate;
            BitDepth = stream->codecpar->bits_per_coded_sample;
            Channels = stream->codecpar->ch_layout.nb_channels;
            SampleRate = stream->codecpar->sample_rate;
            Format = FFmpegHelper.GetSampleFormat(stream->codecpar);
            break;
        }

        avformat_close_input(&formatCtx);
    }

    public Uri Uri { get; }
    public TimeSpan Duration { get; }
    public long BitRate { get; }
    public int BitDepth { get; }
    public int Channels { get; }
    public int SampleRate { get; }
    public SampleFormat Format { get; }

    public static readonly FFmpegSourceMetaInfo Empty = new();
}