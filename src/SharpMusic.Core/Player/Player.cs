using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using SharpMusic.Core.Player.Extensions;

namespace SharpMusic.Core.Player;

public class Player : IDisposable
{
    private readonly ISoundOut _soundOut;
    private IWaveSource? _waveSource;

    internal Player(MultiMediaDevice device)
    {
        Device = device;
        if (Device.Type.HasFlag(MultiMediaDeviceType.DirectSound))
        {
            _soundOut = new DirectSoundOut();
        }
        else if (Device.Type.HasFlag(MultiMediaDeviceType.Wasapi))
        {
            _soundOut = new WasapiOut();
        }
        else if (Device.Type.HasFlag(MultiMediaDeviceType.WaveOut))
        {
            _soundOut = new WaveOut();
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    internal Player() : this(new MultiMediaDevice())
    {
    }

    private void CheckSoundOutIsInitialized()
    {
        if (_soundOut.IsInitialized()) return;
        if (_waveSource is not null)
        {
            _soundOut.Initialize(_waveSource);
        }
        else
        {
            throw new InvalidOperationException(); // TODO: Add description
        }
    }

    public MultiMediaDevice Device { get; set; }

    public IDisposable Open(Uri uri)
    {
        var waveSource = CodecFactory.Instance
            .GetCodec(uri)
            .ToMono();

        return _waveSource = waveSource;
    }

    public void Play()
    {
        CheckSoundOutIsInitialized();
        _soundOut.Play();
    }

    public void Pause()
    {
        CheckSoundOutIsInitialized();
        _soundOut.Pause();
    }

    public void Resume()
    {
        CheckSoundOutIsInitialized();
        _soundOut.Resume();
    }

    public void Stop()
    {
        CheckSoundOutIsInitialized();
        _soundOut.Stop();
    }

    public void Dispose()
    {
        _soundOut.Dispose();
        _waveSource?.Dispose();
    }
}