using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.ExpandInfo;

public class StaffList
{
    public StaffList(Album album, IEnumerable<TrackParticipants>? tracksParticipants)
    {
        SourceAlbum = album;
        TracksParticipantsList = tracksParticipants?.ToList() ?? new List<TrackParticipants>();
    }

    public Album SourceAlbum { get; }
    
    public List<TrackParticipants> TracksParticipantsList { get; }
}