using System.Runtime.InteropServices;

namespace SharpMusic.DllHellPTests.LowLevelImplTests;

public class FFmpegResamplerTests
{
    private const int LENGTH = 1000;
    private const AVSampleFormat FORMAT = AVSampleFormat.AV_SAMPLE_FMT_U8;
    private unsafe AVCodec* _codec;
    private unsafe AVCodecContext* _codecCtx;
    private unsafe AVFrame* _frame;

    [SetUp]
    public unsafe void Setup()
    {
        _codec = avcodec_find_encoder(AVCodecID.AV_CODEC_ID_MP3);
        _codecCtx = avcodec_alloc_context3(_codec);
        _codecCtx->sample_fmt = FORMAT;
        av_channel_layout_copy(&_codecCtx->ch_layout, _codec->ch_layouts);
        _codecCtx->sample_rate = *_codec->supported_samplerates;

        _frame = av_frame_alloc();
        _frame->format = (int)FORMAT;
        av_channel_layout_copy(&_frame->ch_layout, _codec->ch_layouts);
        _frame->nb_samples = LENGTH;
        av_frame_get_buffer(_frame, 0);
    }

    [TearDown]
    public unsafe void TearDown()
    {
        fixed (AVCodecContext** codecCtx = &_codecCtx)
        fixed (AVFrame** frame = &_frame)
        {
            avcodec_free_context(codecCtx);
            av_frame_free(frame);
        }
    }

    [Test]
    public unsafe void WriteFrame_DataBufferOneByOne_WriteCorrect()
    {
        // arrange
        var rng = new Random();
        var data = new byte[1, LENGTH];
        for (var i = 0; i < LENGTH; i++)
        {
            data[0, i] = (byte)rng.Next(0, 256);
        }
        using var resampler = new FFmpegResampler((nint)_codecCtx,
            AVSampleFormat.AV_SAMPLE_FMT_U8, *_codec->ch_layouts, *_codec->supported_samplerates, false);

        // act
        var result = resampler.WriteFrame(_frame, data, out var netRemainingSamples);
        var frameData = new byte[LENGTH];
        Marshal.Copy((IntPtr)_frame->data[0], frameData, 0, LENGTH);
        
        // assert
        Assert.That(result, Is.True);
        Assert.That(netRemainingSamples, Is.EqualTo(0));
        CollectionAssert.AreEqual(data, frameData);
    }
}