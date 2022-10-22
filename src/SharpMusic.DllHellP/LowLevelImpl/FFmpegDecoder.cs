using System.Collections;
using FFmpeg.AutoGen;
using static FFmpeg.AutoGen.ffmpeg;
using SharpMusic.DllHellP.Abstract;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class FFmpegDecoder : IFFmpegDecoder
{
    private readonly FFmpegSource _source;
    private readonly unsafe AVCodecContext* _codecCtx;


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
        return _source.Select(packet =>
        {
            var frame = av_frame_alloc();
            avcodec_send_packet(_codecCtx, &packet);
            avcodec_receive_frame(_codecCtx, frame);
            return *frame;
        }).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public unsafe AVCodecContext AvCodecCtx => *_codecCtx;
}