using System.Diagnostics;
using System.Runtime.InteropServices;
using static SDL2.SDL;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;

namespace SharpMusic.DllHellP.LowLevelImpl;

/// <summary>
/// handle SDL Audio Subsystem resource, provide volume adjust playback control etc.
/// </summary>
public class SdlAudioOutput : ISoundOutput, IDisposable
{
    private SdlOutDisposable? _out;
    private bool _isDisposed;

    public SdlAudioOutput()
    {
        Device = null!;
        Volume = MaxVolume;
    }

    public bool IsMute { get; set; }
    public int Volume { get; set; }

    public int VolumeByPercent
    {
        get => (int)(Volume / (MaxVolume / 100.0));
        set => Volume = (int)(value * (MaxVolume / 100.0));
    }

    public int MinVolume => 0;
    public int MaxVolume => SDL_MIX_MAXVOLUME;
    public PlaybackState State { get; private set; }
    public SdlAudioDevice Device { get; init; }
    public SDL_AudioSpec Spec => _out?.Spec ?? new SDL_AudioSpec();

    public IDisposable Open(SDL_AudioSpec wantSpec)
    {
        _out?.Dispose();
        State = PlaybackState.Stopped;
        return _out = new SdlOutDisposable(Device, wantSpec);
    }

    public IDisposable Open(IAudioMetaInfo info, IEnumerable<IntPtr> frames, FFmpegResampler resampler)
    {
        var provider = new SdlAudioProvider(this, frames, resampler);
        var wantSpec = new SDL_AudioSpec
        {
            freq = info.SampleRate,
            format = info.Format.ToSdlFmt(),
            channels = (byte)info.Channels,
            samples = 1024,
            callback = provider.AudioCallback,
            userdata = IntPtr.Zero,
        };
        return Open(wantSpec);
    }

    public void Play()
    {
        Resume();
        State = PlaybackState.Playing;
    }

    public void Pause()
    {
        if (_out is null) return;
        SDL_PauseAudioDevice(_out.AudioDeviceId, 1);
        State = PlaybackState.Paused;
    }

    public void Resume()
    {
        if (_out is null) return;
        SDL_PauseAudioDevice(_out.AudioDeviceId, 0);
        State = PlaybackState.Playing;
    }

    public void Stop()
    {
        _out?.Dispose();
        State = PlaybackState.Stopped;
    }

    private class SdlOutDisposable : IDisposable
    {
        public readonly SDL_AudioSpec Spec;
        public readonly uint AudioDeviceId;
        private bool _isDisposed;

        public SdlOutDisposable(SdlAudioDevice? device, SDL_AudioSpec wantSpec)
        {
            AudioDeviceId = SDL_OpenAudioDevice(device?.DeviceName, 0, ref wantSpec, out Spec, 0);
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

    /// <summary>
    /// combine Frame provider (like <see cref="FFmpegDecoder"/>) and <see cref="FFmpegResampler"/> for provider sound wave to SDL Audio Subsystem
    /// </summary>
    private class SdlAudioProvider
    {
        private readonly SdlAudioOutput _owner;
        private readonly IEnumerator<IntPtr> _frames;
        private readonly FFmpegResampler _resampler;
        private byte[] _audioBuffer;
        private int _index;

        /// <param name="owner">Provider want know owner state for adjust volume and call <see cref="SdlAudioOutput.Stop"/> when end of stream</param>
        /// <param name="frames">normally assign <see cref="FFmpegDecoder"/></param>
        /// <param name="resampler"></param>
        public SdlAudioProvider(SdlAudioOutput owner, IEnumerable<IntPtr> frames, FFmpegResampler resampler)
        {
            _owner = owner;
            _frames = frames.GetEnumerator();
            _resampler = resampler;
            _audioBuffer = Array.Empty<byte>();
        }

        public unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int len)
        {
            while (len > 0)
            {
                if (_index >= _audioBuffer.Length)
                {
                    if (!_frames.MoveNext()) break;
                    _audioBuffer = _resampler.ResampleFrame(_frames.Current);
                    _index = 0;
                    Debug.Assert(_audioBuffer.Length is not 0);
                }

                var processLen = _audioBuffer.Length - _index;
                if (processLen > len)
                {
                    processLen = len;
                }

                if (_owner.IsMute)
                {
                    ExternMethod.RtlZeroMemory(stream, processLen);
                }
                else if (_owner.Volume is SDL_MIX_MAXVOLUME)
                {
                    Marshal.Copy(_audioBuffer, _index, stream, processLen);
                }
                else
                {
                    fixed (byte* data = _audioBuffer)
                    {
                        ExternMethod.RtlZeroMemory(stream, processLen);
                        SDL_MixAudioFormat(stream, (IntPtr)(data + _index), AUDIO_S16, (uint)processLen, _owner.Volume);
                    }
                }


                len -= processLen;
                stream += processLen;
                _index += processLen;
            }

            if (len <= 0) return;
            ExternMethod.RtlZeroMemory(stream, len);
            _owner.Stop();
        }
    }

    public void Dispose()
    {
        Dispose(true);
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