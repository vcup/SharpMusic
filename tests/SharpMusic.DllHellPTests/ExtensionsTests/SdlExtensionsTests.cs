using static SDL2.SDL;

namespace SharpMusic.DllHellPTests.ExtensionsTests;

public class SdlExtensionsTests
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
    public void SampleFormatToSdlFmt_PassUnsigned8_GetU8()
    {
        // arrange
        const SampleFormat format = SampleFormat.Unsigned8;

        // act
        var result = format.ToSdlFmt();

        // assert
        Assert.That(result, Is.EqualTo(AUDIO_U8));
    }

    [Test]
    public void SampleFormatToSdlFmt_PassSigned16_GetS16()
    {
        // arrange
        const SampleFormat format = SampleFormat.Signed16;

        // act
        var result = format.ToSdlFmt();

        // assert
        Assert.That(result, Is.EqualTo(AUDIO_S16));
    }

    [Test]
    public void SampleFormatToSdlFmt_PassSigned32_GetS32()
    {
        // arrange
        const SampleFormat format = SampleFormat.Signed32;

        // act
        var result = format.ToSdlFmt();

        // assert
        Assert.That(result, Is.EqualTo(AUDIO_S32));
    }
}