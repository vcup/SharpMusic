using static SDL2.SDL;

namespace SharpMusic.DllHellPTests.ExtensionsTests;

public class SampleFormatExtensionsTests
{
    [Test]
    public void SampleFormatToSdlFmt_PassNone_ThrowNotSupportedException()
    {
        // arrange
        const SampleFormat format = SampleFormat.None;

        // act & assert
        Assert.Catch<NotSupportedException>(() =>
            format.ToSdlFmt()
        );
    }

    [Test]
    public void SampleFormatToSdlFmt_PassNoneWithFallback_ThrowNotSupportedException()
    {
        // arrange
        const SampleFormat format = SampleFormat.None;

        // act & assert
        Assert.Catch<NotSupportedException>(() =>
            format.ToSdlFmt(true)
        );
    }

    [TestCase(SampleFormat.Double)]
    [TestCase(SampleFormat.DoublePlanar)]
    [TestCase(SampleFormat.Unsigned8Planar)]
    [TestCase(SampleFormat.Signed16Planar)]
    [TestCase(SampleFormat.Signed32Planar)]
    [TestCase(SampleFormat.Float32Planar)]
    [TestCase(SampleFormat.Signed64)]
    [TestCase(SampleFormat.Signed64Planar)]
    [TestCase(SampleFormat.Other)]
    public void SampleFormatToSdlFmt_PassSdlNotSupportFormat_ThrowNotSupportedException(SampleFormat format)
    {
        // act
        var exception = Assert.Catch<NotSupportedException>(() =>
            format.ToSdlFmt()
        );

        // assert
        Assert.That(exception?.Message, Is.Not.Null);
        Assert.That(exception!.Message, Is.Not.Empty);
    }

    public static object[] SampleFormatToSdlFmtMapValuesSource { get; } =
    {
        new object[] { SampleFormat.Unsigned8, AUDIO_U8 },
        new object[] { SampleFormat.Signed16, AUDIO_S16SYS },
        new object[] { SampleFormat.Unsigned16, AUDIO_U16SYS },
        new object[] { SampleFormat.Signed32, AUDIO_S32SYS },
        new object[] { SampleFormat.Float32, AUDIO_F32SYS },
    };

    [TestCaseSource(nameof(SampleFormatToSdlFmtMapValuesSource))]
    public void SampleFormatToSdlFmt_MapValue_ReturnExceptValue(SampleFormat format, ushort except)
    {
        // act
        var result = format.ToSdlFmt();

        // assert
        Assert.That(result, Is.EqualTo(except));
    }

    public static object[] SampleFormatToSdlFmtMapValuesWithFallbackSource { get; } =
    {
        new object[] { SampleFormat.Unsigned8, AUDIO_U8 },
        new object[] { SampleFormat.Unsigned8Planar, AUDIO_U8 },
        new object[] { SampleFormat.Signed16, AUDIO_S16SYS },
        new object[] { SampleFormat.Signed16Planar, AUDIO_S16SYS },
        new object[] { SampleFormat.Unsigned16, AUDIO_U16SYS },
        new object[] { SampleFormat.Signed32, AUDIO_S32SYS },
        new object[] { SampleFormat.Signed32Planar, AUDIO_S32SYS },
        new object[] { SampleFormat.Float32, AUDIO_F32SYS },
        new object[] { SampleFormat.Float32Planar, AUDIO_F32SYS },
        new object[] { SampleFormat.Double, AUDIO_S16SYS },
        new object[] { SampleFormat.DoublePlanar, AUDIO_S16SYS },
        new object[] { SampleFormat.Signed64, AUDIO_S16SYS },
        new object[] { SampleFormat.Signed64Planar, AUDIO_S16SYS },
        new object[] { SampleFormat.Other, AUDIO_S16SYS },
    };

    [TestCaseSource(nameof(SampleFormatToSdlFmtMapValuesWithFallbackSource))]
    public void SampleFormatToSdlFmt_MapValueWithFallback_ReturnExceptValue(SampleFormat format, ushort except)
    {
        // act
        var result = format.ToSdlFmt(true);

        // assert
        Assert.That(result, Is.EqualTo(except));
    }

    [Test]
    public void SampleFormatToFmt_PassUnsigned16_ThrowNotSupportedException()
    {
        // arrange
        const SampleFormat format = SampleFormat.Unsigned16;

        // act & assert
        Assert.Catch<NotSupportedException>(() =>
            format.ToFmt()
        );
    }

    public static (SampleFormat format, AVSampleFormat except)[] SampleFormatToFmtPassMappedValueReturnExceptValueSource
    {
        get;
    } =
    {
        (SampleFormat.None, AVSampleFormat.AV_SAMPLE_FMT_NONE),
        (SampleFormat.Unsigned8, AVSampleFormat.AV_SAMPLE_FMT_U8),
        (SampleFormat.Signed16, AVSampleFormat.AV_SAMPLE_FMT_S16),
        (SampleFormat.Signed32, AVSampleFormat.AV_SAMPLE_FMT_S32),
        (SampleFormat.Float32, AVSampleFormat.AV_SAMPLE_FMT_FLT),
        (SampleFormat.Double, AVSampleFormat.AV_SAMPLE_FMT_DBL),
        (SampleFormat.Unsigned8Planar, AVSampleFormat.AV_SAMPLE_FMT_U8P),
        (SampleFormat.Signed16Planar, AVSampleFormat.AV_SAMPLE_FMT_S16P),
        (SampleFormat.Signed32Planar, AVSampleFormat.AV_SAMPLE_FMT_S32P),
        (SampleFormat.Float32Planar, AVSampleFormat.AV_SAMPLE_FMT_FLTP),
        (SampleFormat.DoublePlanar, AVSampleFormat.AV_SAMPLE_FMT_DBLP),
        (SampleFormat.Signed64, AVSampleFormat.AV_SAMPLE_FMT_S64),
        (SampleFormat.Signed64Planar, AVSampleFormat.AV_SAMPLE_FMT_S64P),
        (SampleFormat.Other, AVSampleFormat.AV_SAMPLE_FMT_NB),
    };

    [TestCaseSource(nameof(SampleFormatToFmtPassMappedValueReturnExceptValueSource))]
    public void SampleFormatToFmt_PassMappedValue_ReturnExceptValue((SampleFormat format, AVSampleFormat except) args)
    {
        // arrange
        var format = args.format;
        var except = args.except;

        // act
        var result = format.ToFmt();

        // assert
        Assert.That(result, Is.EqualTo(except));
    }


    public static (SampleFormat format, AVSampleFormat except)[]
        SampleFormatToFmtPassMappedValueWithDenyPlanarReturnExceptValueSource { get; } =
    {
        (SampleFormat.None, AVSampleFormat.AV_SAMPLE_FMT_NONE),
        (SampleFormat.Unsigned8, AVSampleFormat.AV_SAMPLE_FMT_U8),
        (SampleFormat.Signed16, AVSampleFormat.AV_SAMPLE_FMT_S16),
        (SampleFormat.Signed32, AVSampleFormat.AV_SAMPLE_FMT_S32),
        (SampleFormat.Float32, AVSampleFormat.AV_SAMPLE_FMT_FLT),
        (SampleFormat.Double, AVSampleFormat.AV_SAMPLE_FMT_DBL),
        (SampleFormat.Unsigned8Planar, AVSampleFormat.AV_SAMPLE_FMT_U8P),
        (SampleFormat.Signed16Planar, AVSampleFormat.AV_SAMPLE_FMT_S16P),
        (SampleFormat.Signed32Planar, AVSampleFormat.AV_SAMPLE_FMT_S32P),
        (SampleFormat.Float32Planar, AVSampleFormat.AV_SAMPLE_FMT_FLTP),
        (SampleFormat.DoublePlanar, AVSampleFormat.AV_SAMPLE_FMT_DBLP),
        (SampleFormat.Signed64, AVSampleFormat.AV_SAMPLE_FMT_S64),
        (SampleFormat.Signed64Planar, AVSampleFormat.AV_SAMPLE_FMT_S64P),
        (SampleFormat.Other, AVSampleFormat.AV_SAMPLE_FMT_NB),
    };

    [TestCaseSource(nameof(SampleFormatToFmtPassMappedValueWithDenyPlanarReturnExceptValueSource))]
    public void SampleFormatToFmt_PassMappedValueWithDenyPlanar_ReturnExceptValue(
        (SampleFormat format, AVSampleFormat except) args)
    {
        // arrange
        var format = args.format;
        var except = args.except;

        // act
        var result = format.ToFmt(true);

        // assert
        Assert.That(result, Is.EqualTo(except));
    }

    [TestCase(SampleFormat.Unsigned8)]
    [TestCase(SampleFormat.Signed16)]
    [TestCase(SampleFormat.Signed32)]
    [TestCase(SampleFormat.Float32)]
    public void SampleFormatIsSupportFFmpegAndSdl2_PassExceptSupportValue_ReturnTrue(SampleFormat format)
    {
        // act
        var result = format.IsSupportFFmpegAndSdl2();

        // assert
        Assert.That(result, Is.True);
    }
}