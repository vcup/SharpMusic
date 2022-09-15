using SharpMusic.Core.Player;

var soundSource = new Uri("https://vcup.moe/e/_bazaar records - e^(x+i)＜ 3u.wav");
var player = new Player();
player.Open(soundSource);

player.Play();
Console.ReadKey();
player.Pause();
Console.ReadKey();
player.Resume();
Console.ReadKey();
player.Stop();
