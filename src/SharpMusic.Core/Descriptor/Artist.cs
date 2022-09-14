﻿namespace SharpMusic.Core.Descriptor;

public class Artist : IDescriptor
{
    private List<Music>? _musics;

    internal Artist(Guid guid)
    {
        Guid = guid;
    }

    public Artist() : this(Guid.NewGuid())
    {
        Names = new List<string>();
        Description = string.Empty;
        Albums = new List<Album>();
        JoinedGroups = new List<ArtistsGroup>();
    }

    public Guid Guid { get; protected init; }

    public IList<string> Names { get; set; }

    public string Description { get; set; }

    public List<Album> Albums { get; }

    public List<ArtistsGroup> JoinedGroups { get; }
}