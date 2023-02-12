namespace SharpMusic.DllHellPTests;

public static class Constants
{
    private const string BASE_URI = "https://vcup.moe/Share/SharpMusic.DllHellPTests";

    /// <summary>
    /// TODO: summary there file meta information
    /// </summary>
    public const string Album0 = $@"{BASE_URI}/SoundSourceTests`0.flac";

    public const string Album1 = $@"{BASE_URI}/SoundSourceTests`1.wav";
    public const string Album2 = $@"{BASE_URI}/SoundSourceTests`2";

    public const string EmptyFile = $@"{BASE_URI}/SoundSourceTests`EmptyFile";
    public const string NoStream = $@"{BASE_URI}/SoundSourceTests`NoStream";
    public const string Video0 = $@"{BASE_URI}/SoundSourceTests`Video0";

    public static IEnumerable<object[]> GetMetaInfos(params string[] keys) =>
        keys.Select(i => new object[] { i, Descriptions[i] });

    private static readonly Dictionary<string, IAudioMetaInfo> Descriptions = new()
    {
        {
            Album0, new AudioMetaInfo
            {
                Uri = new Uri(Album0),
                Duration = new TimeSpan(0),
                BitRate = 0,
                BitDepth = 0,
                Channels = 0,
                SampleRate = 0,
                Format = SampleFormat.None
            }
        },
        {
            Album1, new AudioMetaInfo
            {
                Uri = new Uri(Album1),
                Duration = new TimeSpan(0),
                BitRate = 0,
                BitDepth = 0,
                Channels = 0,
                SampleRate = 0,
                Format = SampleFormat.None
            }
        },
        {
            Album2, new AudioMetaInfo
            {
                Uri = new Uri(Album2),
                Duration = new TimeSpan(0),
                BitRate = 0,
                BitDepth = 0,
                Channels = 0,
                SampleRate = 0,
                Format = SampleFormat.None
            }
        },
        {
            Video0, new AudioMetaInfo
            {
                Uri = new Uri(Video0),
                Duration = new TimeSpan(0),
                BitRate = 0,
                BitDepth = 0,
                Channels = 0,
                SampleRate = 0,
                Format = SampleFormat.None
            }
        },
    };

    private class AudioMetaInfo : IAudioMetaInfo
    {
        public Uri? Uri { get; init; }
        public TimeSpan Duration { get; init; }
        public long BitRate { get; init; }
        public int BitDepth { get; init; }
        public int Channels { get; init; }
        public int SampleRate { get; init; }
        public SampleFormat Format { get; init; }
    }
}
