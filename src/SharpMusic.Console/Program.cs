using System.Diagnostics;
using System.Text;
using SharpMusic.Core.Descriptor;
using SharpMusic.Core.Management;
using SharpMusic.DllHellP.Utils;
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

manager.Volume = 35;

Console.OutputEncoding = Encoding.UTF8;
while (true)
{
    var info = Console.ReadKey();
    switch (info.Key)
    {
        case ConsoleKey.MediaPlay:
        case ConsoleKey.P:
            if (manager.PlaybackState is PlaybackState.Playing) manager.Pause();
            else manager.PlayOrResume();
            break;
        case ConsoleKey.MediaNext:
        case ConsoleKey.RightArrow when (info.Modifiers & ConsoleModifiers.Control) is not 0:
        case ConsoleKey.N:
            manager.PlayNext();
            break;
        case ConsoleKey.MediaPrevious:
        case ConsoleKey.LeftArrow when (info.Modifiers & ConsoleModifiers.Control) is not 0:
        case ConsoleKey.B:
            manager.PlayPrev();
            break;
        case ConsoleKey.VolumeMute:
        case ConsoleKey.M:
            manager.Mute();
            break;
        case ConsoleKey.UpArrow:
            manager.Volume += 5;
            break;
        case ConsoleKey.DownArrow:
            manager.Volume -= 5;
            break;
        case ConsoleKey.LeftArrow:
            manager.PlayPosition -= TimeSpan.FromSeconds(5);
            break;
        case ConsoleKey.RightArrow:
            manager.PlayPosition += TimeSpan.FromSeconds(5);
            break;
        case ConsoleKey.Backspace when (info.Modifiers & ConsoleModifiers.Control) is not 0:
            return;
    }
    Console.SetCursorPosition(0, Console.GetCursorPosition().Top);
    Console.WriteLine($"{info.KeyChar}:{Path.GetFileName(manager.PlayingSoundInfo.Uri.OriginalString)}");
    Console.SetCursorPosition(0, Console.GetCursorPosition().Top - 1);
}