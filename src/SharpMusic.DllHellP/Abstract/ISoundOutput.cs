﻿
namespace SharpMusic.DllHellP.Abstract;

public interface ISoundOutput
{
    public int Volume { get; set; }
    public PlaybackState State { get; set; }

    public void Play();
    public void Pause();
    public void Resume();
    public void Stop();
}