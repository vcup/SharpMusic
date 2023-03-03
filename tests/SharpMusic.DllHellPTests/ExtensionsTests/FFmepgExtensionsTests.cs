using FFmpeg.AutoGen;

namespace SharpMusic.DllHellPTests.ExtensionsTests;

public class FFmepgExtensionsTests
{
    [Test]
    public void SampleFormatToFmt_PassNone_GetAVSampleFormatNone()
    {
        // arrange
        const SampleFormat format = SampleFormat.None;

        // act
        var result = format.ToFmt();

        // asset
        Assert.That(result, Is.EqualTo(AVSampleFormat.AV_SAMPLE_FMT_NONE));
    }

    [Test]
    public void SampleFormatToFmt_PassU8_GetAVSampleFormatU8()
    {
        // arrange
        const SampleFormat format = SampleFormat.Unsigned8;

        // act
        var result = format.ToFmt();

        // asset
        Assert.That(result, Is.EqualTo(AVSampleFormat.AV_SAMPLE_FMT_U8));
    }

    [Test]
    public void SampleFormatToFmt_PassS16_GetAVSampleFormatS16()
    {
        // arrange
        const SampleFormat format = SampleFormat.Signed16;

        // act
        var result = format.ToFmt();

        // asset
        Assert.That(result, Is.EqualTo(AVSampleFormat.AV_SAMPLE_FMT_S16));
    }

    [Test]
    public void SampleFormatToFmt_PassS32_GetAVSampleFormatS32()
    {
        // arrange
        const SampleFormat format = SampleFormat.Signed32;

        // act
        var result = format.ToFmt();

        // asset
        Assert.That(result, Is.EqualTo(AVSampleFormat.AV_SAMPLE_FMT_S32));
    }

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
            FFmpegExtensions.GetSampleFormat(pointer)
        )!;

        // assert
        Assert.That(result.ParamName, Is.EqualTo(nameof(parameters)));
        Assert.That(result.Message.Contains(type.ToString(), StringComparison.Ordinal), Is.True);
    }

    [Test]
    public unsafe void GetSampleFormat_AVSampleFormatNone_GetNone()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = (int)AVSampleFormat.AV_SAMPLE_FMT_NONE
        };

        // act
        var result = FFmpegExtensions.GetSampleFormat(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(SampleFormat.None));
    }

    [Test]
    public unsafe void GetSampleFormat_AVSampleFormatU8_GetUnsigned8()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = (int)AVSampleFormat.AV_SAMPLE_FMT_U8
        };

        // act
        var result = FFmpegExtensions.GetSampleFormat(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(SampleFormat.Unsigned8));
    }

    [Test]
    public unsafe void GetSampleFormat_AVSampleFormatS16_GetSigned16()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16
        };

        // act
        var result = FFmpegExtensions.GetSampleFormat(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(SampleFormat.Signed16));
    }

    [Test]
    public unsafe void GetSampleFormat_AVSampleFormatS32_GetSigned32()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = (int)AVSampleFormat.AV_SAMPLE_FMT_S32
        };

        // act
        var result = FFmpegExtensions.GetSampleFormat(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(SampleFormat.Signed32));
    }

    [Test]
    public unsafe void GetSampleFormat_InvalidFormat_ThrowArgumentOutOfRangeException()
    {
        // arrange
        var parameters = new AVCodecParameters
        {
            codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO,
            format = int.MaxValue
        };
        var pointer = &parameters;

        // act
        var result = Assert.Catch<ArgumentOutOfRangeException>(() =>
            FFmpegExtensions.GetSampleFormat(pointer)
        )!;

        // assert
        Assert.That(result.ParamName, Is.EqualTo(nameof(parameters)));
        Assert.That(result.ActualValue, Is.EqualTo((nint)(&parameters)));
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
        var result = FFmpegExtensions.GetBitDepth(&parameters);

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
        var result = FFmpegExtensions.GetBitDepth(&parameters);

        // assert
        Assert.That(result, Is.EqualTo(1));
    }
}