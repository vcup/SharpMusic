namespace SharpMusic.DllHellPTests;

public class FFmpegSourceTests
{
    private const string BASE_URI = "https://vcup.moe/Share/SharpMusic.DllHellPTests";
    private const string A0 = $@"{BASE_URI}/SoundSourceTests`0.flac";

    /// <summary>
    /// Temporary TestFixture, because remote data has not been determined
    /// </summary>
    [Test]
    public void Master_GetProperties_AllIsNotDefault()
    {
        var uri = new Uri(A0);
        using var source = new FFmpegSource(uri);

        Assert.That(source.Uri, Is.EqualTo(uri));
        Assert.That(source.Duration.Ticks, Is.GreaterThan(0));
        Assert.That(source.BitRate, Is.GreaterThan(0));
        Assert.That(source.BitDepth, Is.GreaterThan(0));
        Assert.That(source.Channels, Is.GreaterThan(0));
        Assert.That(source.SampleRate, Is.GreaterThan(0));
    }
}