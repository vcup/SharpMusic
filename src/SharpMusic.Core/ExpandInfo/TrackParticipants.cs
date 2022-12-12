using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.ExpandInfo;

/// <summary>
/// Staff and ContributionType of music on an album
/// </summary>
public class TrackParticipants : Dictionary<Artist, StaffContributionType>
{
    public TrackParticipants(Music sourceMusic, Album sourceAlbum)
    {
        SourceMusic = sourceMusic;
        SourceAlbum = sourceAlbum;
    }

    public TrackParticipants(
        Music sourceMusic, Album sourceAlbum, IEnumerable<KeyValuePair<Artist, StaffContributionType>> collection)
        : base(collection)
    {
        SourceMusic = sourceMusic;
        SourceAlbum = sourceAlbum;
    }

    public Music SourceMusic { get; }

    public Album SourceAlbum { get; }
}