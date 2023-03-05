using System.Diagnostics;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
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

    public IDisposable Open(IAudioMetaInfo info, IEnumerator<IntPtr> frames, FFmpegResampler? resampler)
    {
        var provider = new SdlAudioProvider(this, frames, resampler);
        var wantSpec = new SDL_AudioSpec
        {
            freq = info.SampleRate,
            format = info.Format.ToSdlFmt(true),
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
            // WARNING: never call this from AudioCallback, that will stop the thread and any thread for calling this method
            SDL_CloseAudioDevice(AudioDeviceId);
            _isDisposed = true;
        }

        ~SdlOutDisposable()
        {
            Dispose();
        }
    }

    /// <summary>
    /// combine Frame provider (like <see cref="FFmpegCodec"/>) and <see cref="FFmpegResampler"/> for provider sound wave to SDL Audio Subsystem
    /// </summary>
    private class SdlAudioProvider
    {
        private readonly SdlAudioOutput _owner;
        private readonly IEnumerator<IntPtr> _frames;
        private readonly FFmpegResampler? _resampler;
        private byte[] _audioBuffer;
        private int _index;

        /// <param name="owner">Provider want know owner state for adjust volume and call <see cref="SdlAudioOutput.Stop"/> when end of stream</param>
        /// <param name="frames">normally assign <see cref="FFmpegCodec"/></param>
        /// <param name="resampler">using resample to get audio wave, direct use frame->extended_data when null</param>
        public SdlAudioProvider(SdlAudioOutput owner, IEnumerator<IntPtr> frames, FFmpegResampler? resampler = null)
        {
            _owner = owner;
            _frames = frames;
            _resampler = resampler;
            _audioBuffer = Array.Empty<byte>();
        }

        public unsafe void AudioCallback(IntPtr userdata, IntPtr stream, int remainingLen)
        {
            var bufferLength = 0;
            AVFrame* pFrame = null;
            while (remainingLen > 0)
            {
                if (_index >= bufferLength)
                {
                    if (!_frames.MoveNext()) break;
                    if (_resampler is null)
                    {
                        bufferLength = pFrame->nb_samples * pFrame->ch_layout.nb_channels *
                                       ffmpeg.av_get_bytes_per_sample((AVSampleFormat)pFrame->format);
                    }
                    else
                    {
                        _audioBuffer = _resampler.ResampleFrame((AVFrame*)_frames.Current);
                        bufferLength = _audioBuffer.Length;
                    }

                    _index = 0;
                    Debug.Assert(bufferLength is not 0);
                }

                var processLen = bufferLength - _index;
                if (processLen > remainingLen)
                {
                    processLen = remainingLen;
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
                        var src = _resampler is null
                            ? *pFrame->extended_data + _index
                            : data + _index;
                        SDL_MixAudioFormat(stream, (IntPtr)src, _owner._out!.Spec.format,
                            (uint)processLen, _owner.Volume);
                    }
                }


                remainingLen -= processLen;
                stream += processLen;
                _index += processLen;
            }

            _owner.State = PlaybackState.Playing;
            if (remainingLen <= 0) return;
            ExternMethod.RtlZeroMemory(stream, remainingLen);
            _owner.Pause();
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