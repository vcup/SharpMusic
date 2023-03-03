using SDL2;

namespace SharpMusic.DllHellP.Utils;

public static class SdlHelper
{

    public static uint SDL_OpenAudioDevice(IntPtr? device, int iscapture, ref SDL.SDL_AudioSpec wantSpec, out SDL.SDL_AudioSpec obtained, int allowedChanges)
    {
        return device is null
            ? SDL.SDL_OpenAudioDevice(null, iscapture, ref wantSpec, out obtained, allowedChanges)
            : SDL.SDL_OpenAudioDevice(device.Value, iscapture, ref wantSpec, out obtained, allowedChanges);
    }
}