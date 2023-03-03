namespace SharpMusic.Core.Descriptor.DescriptorExtension;

[Flags]
public enum SearchDescriptorOption
{
    None              = 0b0000_0000_0000_0000,
    IgnoreMusic       = 0b0000_0000_0000_0001,
    IgnoreAlbum       = 0b0000_0000_0000_0010,
    IgnoreArtist      = 0b0000_0000_0000_0100,
    IgnoreArtistGroup = 0b0000_0000_0000_1000,
    IgnorePlaylist    = 0b0000_0000_0001_0000,
    OnlyMusic         = 0b0000_0000_0001_1110,
    OnlyAlbum         = 0b0000_0000_0001_1101,
    OnlyArtist        = 0b0000_0000_0001_1011,
    OnlyArtistGroup   = 0b0000_0000_0001_0111,
    OnlyPlaylist      = 0b0000_0000_0000_1111,
}