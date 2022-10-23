using static SDL2.SDL;
using static SharpMusic.DllHellP.Utils.SdlHelper;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.LowLevelImpl;

public class SdlAudioOutput : ISoundOutput, IDisposable
{
    private SdlOutDisposable? _out;
    private bool _isDisposed;

    public SdlAudioOutput()
    {
        Device = new SdlAudioDevice();
    }

    public bool IsMute { get; set; }
    public int Volume { get; set; }
    public int MinVolume { get; }
    public int MaxVolume { get; }
    public PlaybackState State { get; set; }
    public SdlAudioDevice Device { get; init; }
    public SDL_AudioSpec Spec => _out?.Spec ?? new SDL_AudioSpec();

    public IDisposable Open(SDL_AudioSpec wantSpec)
    {
        _out?.Dispose();
        return _out = new SdlOutDisposable(Device, wantSpec);
    }

    public IDisposable Open(IAudioMetaInfo info, SdlAudioProvider audioProvider)
    {
        var wantSpec = new SDL_AudioSpec
        {
            freq = info.SampleRate,
            format = info.Format.ToSdlFmt(),
            channels = (byte)info.Channels,
            samples = 1024,
            callback = audioProvider.AudioCallback,
            userdata = IntPtr.Zero,
        };
        return Open(wantSpec);
    }

    public void Play()
    {
        Resume();
    }

    public void Pause()
    {
        if (_out is null) return;
        SDL_PauseAudioDevice(_out.AudioDeviceId, 1);
    }

    public void Resume()
    {
        if (_out is null) return;
        SDL_PauseAudioDevice(_out.AudioDeviceId, 0);
    }

    public void Stop()
    {
        _out?.Dispose();
    }

    private class SdlOutDisposable : IDisposable
    {
        public readonly SDL_AudioSpec Spec;
        public readonly uint AudioDeviceId;
        private bool _isDisposed;

        public SdlOutDisposable(SdlAudioDevice? device, SDL_AudioSpec wantSpec)
        {
            AudioDeviceId = SDL_OpenAudioDevice(device?.ToPtr(), 0, ref wantSpec, out Spec, 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing || _isDisposed) return;
            SDL_CloseAudioDevice(AudioDeviceId);
            _isDisposed = true;
        }

        ~SdlOutDisposable()
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing || _isDisposed) return;
        _out?.Dispose();
        _isDisposed = true;
    }

    ~SdlAudioOutput()
    {
        Dispose();
    }
}