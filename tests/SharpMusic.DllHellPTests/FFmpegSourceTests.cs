namespace SharpMusic.DllHellPTests;

public class FFmpegSourceTests
{
    private const string BaseUri = "https://vcup.moe/Share/SharpMusic.DllHellPTests";
    private const string A0 = $@"{BaseUri}/SoundSourceTests`0.flac";

    /// <summary>
    /// Temporary TestFixture, because remote data has not been determined
    /// </summary>
    [Test]
    public void Master_GetProperties_AllIsNotDefault()
    {
        var uri = new Uri(A0);
        using var source = new FFmpegSource(uri);

        Assert.AreEqual(uri, source.Uri);
        Assert.Greater(source.Duration.Ticks, 0);
        Assert.Greater(source.BitRate, 0);
        Assert.Greater(source.BitDepth, 0);
        Assert.Greater(source.Channels, 0);
        Assert.Greater(source.SampleRate, 0);
    }
}