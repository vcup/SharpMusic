using SharpMusic.Core.Playback;

var soundSource = new Uri("https://vcup.moe/e/_bazaar records - e^(x+i)＜ 3u.wav");
using var player = new Player();
using var disposable = player.Open(soundSource);

player.Play();
Console.ReadKey();
player.Pause();
Console.ReadKey();
player.Resume();
Console.ReadKey();
player.Stop();