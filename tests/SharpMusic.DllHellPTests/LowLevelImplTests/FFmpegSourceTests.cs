using System.Diagnostics.CodeAnalysis;

namespace SharpMusic.DllHellPTests.LowLevelImplTests;

public class FFmpegSourceTests
{
    /// <summary>
    /// Temporary TestFixture, because remote data has not been determined
    /// </summary>
    [Test]
    public void Master_GetProperties_AllIsNotDefault()
    {
        var uri = new Uri(Album0);
        using var source = new FFmpegSource(uri);

        Assert.That(source.Uri, Is.EqualTo(uri));
        Assert.That(source.Duration.Ticks, Is.GreaterThan(0));
        Assert.That(source.BitRate, Is.GreaterThan(0));
        Assert.That(source.BitDepth, Is.GreaterThan(0));
        Assert.That(source.Channels, Is.GreaterThan(0));
        Assert.That(source.SampleRate, Is.GreaterThan(0));
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