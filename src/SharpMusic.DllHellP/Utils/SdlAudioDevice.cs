using static SDL2.SDL;

namespace SharpMusic.DllHellP.Utils;

public class SdlAudioDevice
{
    static SdlAudioDevice()
    {
        AvailableInputDevices = null!;
        AvailableOutputDevices = null!;
        FlushDevices();
    }

    public SdlAudioDevice(int deviceId, bool isCapture = false)
    {
        DeviceId = deviceId;
        IsCapture = isCapture;
        DeviceName = SDL_GetAudioDeviceName(deviceId, isCapture ? 1 : 0);
    }

    public int DeviceId { get; }

    public bool IsCapture { get; }

    public string DeviceName { get; }

    public static SdlAudioDevice[] AvailableOutputDevices { get; private set; }
    public static SdlAudioDevice[] AvailableInputDevices { get; private set; }

    public static void FlushDevices()
    {
        AvailableOutputDevices = Enumerable
            .Range(0, SDL_GetNumAudioDevices(0))
            .Select(i => new SdlAudioDevice(i))
            .ToArray();
        AvailableInputDevices = Enumerable
            .Range(0, SDL_GetNumAudioDevices(1))
            .Select(i => new SdlAudioDevice(i, true))
            .ToArray();
    }
}