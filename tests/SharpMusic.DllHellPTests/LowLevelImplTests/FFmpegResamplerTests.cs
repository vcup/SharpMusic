using SharpMusic.DllHellPTests.FFWrappers;
using SharpMusic.DllHellPTests.Utils;

namespace SharpMusic.DllHellPTests.LowLevelImplTests;

public class FFmpegResamplerTests
{
    [Test]
    public unsafe void WriteFrame_UseInOutput_ThrowNotSupportException()
    {
        var chLayout = StereoChannelLayout;

        const AVSampleFormat format = AVSampleFormat.AV_SAMPLE_FMT_U8;
        const int length = 0;
        using var ffFrame = new FFFrame(format, chLayout, length);
        var frame = ffFrame.Frame;

        using var ffEncoder = new FFEncoder(AVCodecID.AV_CODEC_ID_FIRST_AUDIO, format, &chLayout);
        var encoder = ffEncoder.CodecCtx;

        using var resampler = new FFmpegResampler((IntPtr)encoder,
            format, encoder->ch_layout, encoder->sample_rate);

        // act & assert
        Assert.Catch<NotSupportedException>(() => resampler.WriteFrame(frame, new byte[0, 0], out _));
    }

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

    // ReSharper disable once InconsistentNaming
    private static AVSampleFormat[] WriteFrame_DataMoreThanBuffer_WritePossibleAndCache__Formats =
            WriteFrame_DataBufferOneByOne_WriteCorrect__Formats.ToArray();

    // ReSharper disable once InconsistentNaming
    private static (int data, int frame, int net)[] WriteFrame_DataMoreThanBuffer_WritePossibleAndCache__Lengths =
    {
        (8000, 7999, -1),
        (2048, 1024, -1024),
        (4096, 1024, -3072),
    };

    [Test]
    public unsafe void WriteFrame_DataMoreThanBuffer_WritePossibleAndCache(
        [ValueSource(nameof(WriteFrame_DataMoreThanBuffer_WritePossibleAndCache__Formats))]
        AVSampleFormat format,
        [ValueSource(nameof(WriteFrame_DataMoreThanBuffer_WritePossibleAndCache__Lengths))]
        (int data, int frame, int net) length
    )
    {
        // arrange
        var chLayout = StereoChannelLayout;
        var data = SampleDataHelper.RandomSampleData(length.data, format, chLayout);

        using var ffFrame = new FFFrame(format, chLayout, length.frame);
        var frame = ffFrame.Frame;

        using var ffEncoder = new FFEncoder(AVCodecID.AV_CODEC_ID_FIRST_AUDIO, format, &chLayout);
        var encoder = ffEncoder.CodecCtx;

        using var resampler = new FFmpegResampler((IntPtr)encoder,
            format, encoder->ch_layout, encoder->sample_rate, false);

        // act
        var result = resampler.WriteFrame(frame, data, out var netRemainingSamples);

        // assert
        Assert.That(result, Is.True);
        Assert.That(netRemainingSamples, Is.EqualTo(length.net));
        var cutData = SampleDataHelper.CutSamples(data, format, chLayout, length.net);
        CollectionAssert.AreEqual(cutData, ffFrame);
    }

    // ReSharper disable once InconsistentNaming
    private static AVSampleFormat[] WriteFrame_DataLessThanBuffer_WriteAllData__Formats =
            WriteFrame_DataBufferOneByOne_WriteCorrect__Formats.ToArray();

    // ReSharper disable once InconsistentNaming
    private static (int data, int frame, int net)[] WriteFrame_DataLessThanBuffer_WriteAllData__Lengths =
    {
        (7999, 8000, 1),
        (1024, 2048, 1024),
        (1024, 4096, 3072),
    };

    [Test]
    public unsafe void WriteFrame_DataLessThanBuffer_WriteAllData(
        [ValueSource(nameof(WriteFrame_DataLessThanBuffer_WriteAllData__Formats))]
        AVSampleFormat format,
        [ValueSource(nameof(WriteFrame_DataLessThanBuffer_WriteAllData__Lengths))]
        (int data, int frame, int net) length
    )
    {
        // arrange
        var chLayout = StereoChannelLayout;
        var data = SampleDataHelper.RandomSampleData(length.data, format, chLayout);

        using var ffFrame = new FFFrame(format, chLayout, length.frame);
        var frame = ffFrame.Frame;

        using var ffEncoder = new FFEncoder(AVCodecID.AV_CODEC_ID_FIRST_AUDIO, format, &chLayout);
        var encoder = ffEncoder.CodecCtx;

        using var resampler = new FFmpegResampler((IntPtr)encoder,
            format, encoder->ch_layout, encoder->sample_rate, false);

        // act
        var result = resampler.WriteFrame(frame, data, out var netRemainingSamples);

        // assert
        Assert.That(result, Is.False);
        Assert.That(netRemainingSamples, Is.EqualTo(length.net));
        var cutData = SampleDataHelper.CutSamples(frame, -length.net);
        CollectionAssert.AreEqual(data, cutData);
    }

    // ReSharper disable once InconsistentNaming
    private const bool T = true;
    private const bool F = false;

    // ReSharper disable once InconsistentNaming
    private static AVSampleFormat[] WriteFrame_MultipleCall_CompleteCycle__Formats =
            WriteFrame_DataBufferOneByOne_WriteCorrect__Formats.ToArray();

    // ReSharper disable once InconsistentNaming
    private static (int data, int frame, int[] net, bool[]give, bool[] full)[] WriteFrame_MultipleCall_CompleteCycle__CycleDetail =
    {
        (
            1152, 1024,
            new[] { -0128, +0896, -0256, +0768, -0384, +0640, -0512, +0512, -0640, +0384, -0768, +0256, -0896, +0128, -1024, 0 },
            new[] { T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F },
            new[] { T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , T }
        ),
        (
            1024, 1152,
            new[] { +0128, -0896, +0256, -0768, +0384, -0640, +0512, -0512, +0640, -0384, +0768, -0256, +0896, -0128, +1024, 0 },
            new[] { T    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T },
            new[] { F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T    , F    , T }
        ),
        (
            1024, 4608,
            new[] { +3584, +2560, +1536, +0512, -0512, +4096, +3072, +2048, +1024, 0 },
            new[] { T    , T    , T    , T    , T    , F    , T    , T    , T    , T },
            new[] { F    , F    , F    , F    , T    , F    , F    , F    , F    , T }
        ),
        (
            4608, 1024,
            new[] { -3584, -2560, -1536, -0512, +0512, -4096, -3072, -2048, -1024, 0 },
            new[] { T    ,  F   , F    , F    , F    , T    , F    , F    , F    , F },
            new[] { T    ,  T   , T    , T    , F    , T    , T    , T    , T    , T }
        )
    };

    [Test]
    public unsafe void WriteFrame_MultipleCall_CompleteCycle(
        [ValueSource(nameof(WriteFrame_MultipleCall_CompleteCycle__Formats))]
        AVSampleFormat format,
        [ValueSource(nameof(WriteFrame_MultipleCall_CompleteCycle__CycleDetail))]
        (int data, int frame, int[] net, bool[]give, bool[] full) cycleDetail)
    {
        // arrange
        var chLayout = StereoChannelLayout;

        using var ffFrame = new FFFrame(format, chLayout, cycleDetail.frame);
        var frame = ffFrame.Frame;

        using var ffEncoder = new FFEncoder(AVCodecID.AV_CODEC_ID_FIRST_AUDIO, format, &chLayout);
        var encoder = ffEncoder.CodecCtx;

        using var resampler = new FFmpegResampler((IntPtr)encoder,
            format, encoder->ch_layout, encoder->sample_rate, false);

        var net = new List<int>();
        var full = new List<bool>();
        var allData = new List<byte[,]>();
        var allWroteData = new List<byte[,]>();

        // act
        for (var i = 0; i < cycleDetail.net.Length; i++)
        {
            var data = cycleDetail.give[i]
                ? SampleDataHelper.RandomSampleData(cycleDetail.data, format, chLayout)
                : new byte[0, 0];

            var isFull = resampler.WriteFrame(frame, data, out var netRemainingSamples);
            net.Add(netRemainingSamples);
            full.Add(isFull);

            if (cycleDetail.give[i]) allData.Add(data);
            if (isFull) allWroteData.Add(SampleDataHelper.CutSamples(frame, 0));
        }

        // assert
        CollectionAssert.AreEqual(cycleDetail.net, net, "Unexpected net remaining samples");
        CollectionAssert.AreEqual(cycleDetail.full, full, "Unexpected frame full flag");
        var concatenatedAllData = SampleDataHelper.Concat2DArraysByRow(allData.ToArray());
        var concatenatedWroteData = SampleDataHelper.Concat2DArraysByRow(allWroteData.ToArray());
        CollectionAssert.AreEqual(concatenatedAllData, concatenatedWroteData, "Unexpected wrote data");
    }
}