using System.Diagnostics.CodeAnalysis;

namespace SharpMusic.DllHellPTests.LowLevelImplTests;

public class FFmpegSourceTests
{
    [ExcludeFromCodeCoverage]
    // TODO: solve this
    [Ignore($"hasn't way to make an file to test MetaInfo")]
    [TestCaseSource(typeof(Constants), nameof(GetMetaInfos), new object[]
    {
        new[]
        {
            Album0, Album1, Album2, Video0
        }
    })]
    public void Constructor_GetProperties_ReadingMetaInfoCorrect(string uri, IAudioMetaInfo excepted)
    {
        // arrange & act
        using var source = new FFmpegSource(new Uri(uri));

        // assert
        Assert.That(source.Format, Is.EqualTo(excepted.Format));
        Assert.That(source.Uri, Is.EqualTo(excepted.Uri));
        Assert.That(source.BitRate, Is.EqualTo(excepted.BitRate));
        Assert.That(source.BitDepth, Is.EqualTo(excepted.BitDepth));
        Assert.That(source.Channels, Is.EqualTo(excepted.Channels));
        Assert.That(source.SampleRate, Is.EqualTo(excepted.SampleRate));
        Assert.That(source.Duration, Is.EqualTo(excepted.Duration));
    }

    [Test]
    public void Constructor_PassInvalidUri_ThrowFFmpegOpenInputException()
    {
        // arrange
        var uri = new Uri(EmptyFile);

        // act
        var exception = Assert.Catch<FFmpegOpenInputException>(() =>
            _ = new FFmpegSource(uri))!;

        // assert
        Assert.That(exception.MessageString, Is.EqualTo($"Cannot open uri {uri.OriginalString}"));
        Assert.That(exception.ErrorCode, Is.EqualTo(AVERROR_INVALIDDATA));
        Assert.That(exception.ErrorString, Is.EqualTo(AvStringErrorCode(AVERROR_INVALIDDATA)));
    }

    [Test]
    [ExcludeFromCodeCoverage]
    // TODO: solve this
    [Ignore($"hasn't way to make an file for testing exception {nameof(FFmpegFindStreamException)}")]
    public void Constructor_PassDoesNotContainedStreamUri_ThrowFFmpegFindStreamException()
    {
        // arrange
        var uri = new Uri(NoStream);

        // act
        var exception = Assert.Catch<FFmpegFindStreamException>(() =>
            _ = new FFmpegSource(uri))!;

        // assert
        Assert.That(exception.MessageString,
            Is.EqualTo("Could not find stream information from " + uri.OriginalString));
        Assert.That(exception.ErrorString, Is.EqualTo(string.Empty));
    }
}