using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using SharpMusic.Core.Playback.Extensions;

namespace SharpMusic.Core.Playback;

public class Player : IDisposable
{
    private readonly ISoundOut _soundOut;
    private IWaveSource? _waveSource;

    public Player(MultiMediaDevice device)
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

    public Player() : this(new MultiMediaDevice())
    {
    }

    public PlaybackState PlaybackState => _soundOut.PlaybackState;

    public TimeSpan Position
    {
        get => _waveSource?.GetPosition() ?? TimeSpan.Zero;
        set => _waveSource?.SetPosition(value);
    }

    public TimeSpan Lenght => _waveSource?.GetLength() ?? TimeSpan.Zero;

    public int Volume
    {
        get
        {
            CheckSoundOutIsInitialized();
            return Math.Max(0, Math.Min(100, (int)(_soundOut.Volume * 100)));
        }
        set
        {
            CheckSoundOutIsInitialized();
            _soundOut.Volume = Math.Max(0, Math.Min(1, value / 100f));
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
}