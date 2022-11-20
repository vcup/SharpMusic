using System.Diagnostics;
using SharpMusic.Core.Descriptor;
using SharpMusic.Core.Management;
using static SDL2.SDL;

const string album0 = @"https://vcup.moe/Share/SharpMusic.DllHellPTests/SoundSourceTests`0.flac";
const string album1 = @"https://vcup.moe/Share/SharpMusic.DllHellPTests/SoundSourceTests`1.wav";
const string album2 = @"https://vcup.moe/Share/SharpMusic.DllHellPTests/SoundSourceTests`2";

Debug.Assert(SDL_Init(SDL_INIT_AUDIO) is 0);
var manager = new PlaybackManager();
manager.Playlist.Add(new Music
{
    SoundSource = { new Uri(album0) }
});
manager.Playlist.Add(new Music
{
    SoundSource = { new Uri(album1) }
});
manager.Playlist.Add(new Music
{
    SoundSource = { new Uri(album2) }
});

manager.PlayOrResume();
Console.ReadLine();
manager.PlayNext();
Console.ReadLine();
manager.PlayNext();
Console.ReadLine();
manager.PlayNext();

Console.ReadLine();
manager.PlayPrev();
Console.ReadLine();
manager.PlayPrev();

return 0;