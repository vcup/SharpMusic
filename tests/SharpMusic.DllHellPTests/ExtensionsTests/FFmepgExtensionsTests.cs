
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
}