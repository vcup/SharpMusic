namespace SharpMusic.DllHellPTests.ExtensionsTests;

public class FFmepgHelperTests
{
    // reference 'https://ffmpeg.org/doxygen/trunk/group__lavu__misc.html#ga9a84bba4713dfced21a1a56163be1f48'
    [TestCase(AVMediaType.AVMEDIA_TYPE_UNKNOWN)]
    [TestCase(AVMediaType.AVMEDIA_TYPE_VIDEO)]
    [TestCase(AVMediaType.AVMEDIA_TYPE_DATA)]
    [TestCase(AVMediaType.AVMEDIA_TYPE_SUBTITLE)]
    [TestCase(AVMediaType.AVMEDIA_TYPE_ATTACHMENT)]
    [TestCase(AVMediaType.AVMEDIA_TYPE_NB)]
    public unsafe void GetSampleFormat_PassInvalidCodecType_ThrowArgumentException(AVMediaType type)
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = type
        };
        var pointer = &parameters;

        // act
        var result = Assert.Catch<ArgumentException>(() =>
            GetSampleFormat(pointer)
        )!;

        // assert
        Assert.That(result.ParamName, Is.EqualTo(nameof(parameters)));
        Assert.That(result.Message.Contains(type.ToString(), StringComparison.Ordinal), Is.True);
    }

    // ReSharper disable once InconsistentNaming
    private static (AVSampleFormat avFmt, SampleFormat fmt)[]
        GetSampleFormat_PassAvCodecParameter_GetSampleFormat__Source =
        {
            (AVSampleFormat.AV_SAMPLE_FMT_NONE, SampleFormat.None),
            (AVSampleFormat.AV_SAMPLE_FMT_U8, SampleFormat.Unsigned8),
            (AVSampleFormat.AV_SAMPLE_FMT_S16, SampleFormat.Signed16),
            (AVSampleFormat.AV_SAMPLE_FMT_S32, SampleFormat.Signed32),
            (AVSampleFormat.AV_SAMPLE_FMT_FLT, SampleFormat.Float32),
            (AVSampleFormat.AV_SAMPLE_FMT_DBL, SampleFormat.Double),
            (AVSampleFormat.AV_SAMPLE_FMT_U8P, SampleFormat.Unsigned8Planar),
            (AVSampleFormat.AV_SAMPLE_FMT_S16P, SampleFormat.Signed16Planar),
            (AVSampleFormat.AV_SAMPLE_FMT_S32P, SampleFormat.Signed32Planar),
            (AVSampleFormat.AV_SAMPLE_FMT_FLTP, SampleFormat.Float32Planar),
            (AVSampleFormat.AV_SAMPLE_FMT_DBLP, SampleFormat.DoublePlanar),
            (AVSampleFormat.AV_SAMPLE_FMT_S64, SampleFormat.Signed64),
            (AVSampleFormat.AV_SAMPLE_FMT_S64P, SampleFormat.Signed64Planar),
            (AVSampleFormat.AV_SAMPLE_FMT_NB, SampleFormat.Other),
        };

    [TestCaseSource(nameof(GetSampleFormat_PassAvCodecParameter_GetSampleFormat__Source))]
    public unsafe void GetSampleFormat_PassAvCodecParameter_GetSampleFormat(
        (AVSampleFormat avFmt, SampleFormat fmt) args)
    {
        // arrange
        var parameter = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = (int)args.avFmt,
        };

        // act
        var result = GetSampleFormat(&parameter);

        // assert
        Assert.That(result, Is.EqualTo(args.fmt));
    }

    [TestCaseSource(nameof(GetSampleFormat_PassAvCodecParameter_GetSampleFormat__Source))]
    public void GetSampleFormat_PassAvSampleFormat_GetSampleFormat((AVSampleFormat avFmt, SampleFormat fmt) args)
    {
        // arrange
        var avSampleFormat = args.avFmt;
        var sampleFormat = args.fmt;

        // act
        var result = GetSampleFormat(avSampleFormat);

        // assert
        Assert.That(result, Is.EqualTo(sampleFormat));
    }

    [Test]
    public void GetSampleFormat_InvalidFormat_ThrowArgumentOutOfRangeException()
    {
        // arrange
        const AVSampleFormat format = (AVSampleFormat)int.MaxValue;

        // act
        var result = Assert.Catch<ArgumentOutOfRangeException>(() =>
            GetSampleFormat(format)
        )!;

        // assert
        Assert.That(result.ParamName, Is.EqualTo(nameof(format)));
        Assert.That(result.ActualValue, Is.EqualTo(format));
    }

    [Test]
    public unsafe void GetBitDepth_NonZeroBitDepthInRaw_ReturnThem()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            bits_per_raw_sample = 1,
        };

        // act
        var result = GetBitDepth(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public unsafe void GetBitDepth_ZeroBitDepthInRaw_ReturnCodedBitDepth()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            bits_per_raw_sample = 0,
            bits_per_coded_sample = 1,
        };

        // act
        var result = GetBitDepth(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(1));
    }
}