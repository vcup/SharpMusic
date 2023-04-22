using SharpMusic.DllHellPTests.FFWrappers;
using SharpMusic.DllHellPTests.Utils;

namespace SharpMusic.DllHellPTests.LowLevelImplTests;

public class FFmpegResamplerTests
{
    // ReSharper disable once InconsistentNaming
    private static AVSampleFormat[] WriteFrame_DataBufferOneByOne_WriteCorrect__Formats =
    {
        AVSampleFormat.AV_SAMPLE_FMT_U8,
        AVSampleFormat.AV_SAMPLE_FMT_U8P,
        AVSampleFormat.AV_SAMPLE_FMT_S16,
        AVSampleFormat.AV_SAMPLE_FMT_S16P,
        AVSampleFormat.AV_SAMPLE_FMT_FLT,
        AVSampleFormat.AV_SAMPLE_FMT_FLTP,
        AVSampleFormat.AV_SAMPLE_FMT_DBL,
        AVSampleFormat.AV_SAMPLE_FMT_DBLP,
    };

    [Test]
    public unsafe void WriteFrame_DataBufferOneByOne_WriteCorrect(
        [ValueSource(nameof(WriteFrame_DataBufferOneByOne_WriteCorrect__Formats))]
        AVSampleFormat format,
        [Values(8000, 44100, 48000)] int length)
    {
        // arrange
        var chLayout = StereoChannelLayout;
        var data = SampleDataHelper.RandomSampleData(length, format, chLayout);

        using var ffFrame = new FFFrame(format, chLayout, length);
        var frame = ffFrame.Frame;

        using var ffEncoder = new FFEncoder(AVCodecID.AV_CODEC_ID_FIRST_AUDIO, format, &chLayout);
        var encoder = ffEncoder.CodecCtx;

        using var resampler = new FFmpegResampler((IntPtr)encoder,
            format, encoder->ch_layout, encoder->sample_rate, false);


        // act
        var result = resampler.WriteFrame(frame, data, out var netRemainingSamples);

        // assert
        Assert.That(result, Is.True);
        Assert.That(netRemainingSamples, Is.EqualTo(0));
        CollectionAssert.AreEqual(data, ffFrame);
    }
}