﻿using SharpMusic.Core.Descriptor;

namespace SharpMusic.Core.ExpandInfo;

public class StaffList : List<TrackParticipants>
{
    public StaffList(Album album)
    {
        SourceAlbum = album;
    }

    public StaffList(Album album, IEnumerable<TrackParticipants> tracksParticipants)
        : base(tracksParticipants)
    {
        SourceAlbum = album;
    }

    public Album SourceAlbum { get; }
}