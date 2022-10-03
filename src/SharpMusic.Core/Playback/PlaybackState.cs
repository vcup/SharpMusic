namespace SharpMusic.Core.Playback;

public enum PlaybackState
{
    Stopped   = 0b0000,
    Playing   = 0b0001,
    Paused    = 0b0010,
    Buffering = 0b1000,
}