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
}