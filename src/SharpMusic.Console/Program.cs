using System.Diagnostics;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.LowLevelImpl;
using static SDL2.SDL;

const string album = @"https://vcup.moe/Share/SharpMusic.DllHellPTests/SoundSourceTests`0.flac";

Debug.Assert(SDL_Init(SDL_INIT_AUDIO) is 0);
using var source = new FFmpegSource(new Uri(album, UriKind.RelativeOrAbsolute));
using var decoder = new FFmpegDecoder(source);
using var resampler =
    new FFmpegResampler(decoder.AvCodecCtx, source.Format.ToFmt(), source.ChannelLayout, source.SampleRate);
using var device = new SdlAudioOutput();
using var @out = device.Open(source, decoder, resampler);
device.Play();
device.VolumeByPercent = 50;
Debug.Assert(device.Volume is 64);

Console.ReadLine();
device.IsMute = true;
Console.ReadLine();
device.IsMute = false;
Console.ReadLine();
device.Dispose();

Console.ReadLine();

return 0;