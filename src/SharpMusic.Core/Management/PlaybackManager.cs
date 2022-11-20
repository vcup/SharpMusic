using SharpMusic.Core.Descriptor;
using SharpMusic.Core.Utils;
using SharpMusic.DllHellP.Abstract;
using SharpMusic.DllHellP.Extensions;
using SharpMusic.DllHellP.Utils;
using SharpMusic.DllHellP.LowLevelImpl;

namespace SharpMusic.Core.Management;

public class PlaybackManager
{
    private readonly SdlAudioOutput _output;
    private FFmpegSource? _source;
    private FFmpegDecoder? _decoder;
    private FFmpegResampler? _resampler;
    private int _playingIndex;

    public PlaybackManager()
    {
        _output = new();
        SoundInfoCache = new();
        Playlist = new(Guid.Empty);
    }

    public long PlayPositionTicks
    {
        get => _source is not null ? _source.Position.Ticks : 0;
        set => throw new NotImplementedException();
    }

    public long PlaybackTimeTicks => _source is not null ? _source.Duration.Ticks : 0;

    public PlaybackState PlaybackState => _output.State;

    public PlaybackMode PlaybackMode { get; set; }

    public Playlist Playlist { get; }

    public Dictionary<Music, IAudioMetaInfo> SoundInfoCache { get; }

    public IAudioMetaInfo PlayingSoundInfo => _source is not null ? _source : FFmpegSourceMetaInfo.Empty;

    public void PlayOrResume()
    {
        if (!Playlist.Any()) return;
        switch (_output.State)
        {
            case PlaybackState.Stopped:
                _output.Play();
                break;
            case PlaybackState.Paused:
                _output.Resume();
                break;
            case PlaybackState.Playing:
                break;
            case PlaybackState.Buffering:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_output.State is PlaybackState.Playing || _source is not null) return;
        ReOpenCurrentMusic();
        _output.Play();
    }

    public void Pause()
    {
        _output.Pause();
    }

    public void Stop()
    {
        _output.Stop();
    }

    public void PlayNext()
    {
        if (!Playlist.Any()) return;
        _playingIndex++;
        _playingIndex %= Playlist.Count();
        ReOpenCurrentMusic();
        PlayOrResume();
    }

    public void PlayPrev()
    {
        if (!Playlist.Any()) return;
        if (_playingIndex-- is 0) _playingIndex = Playlist.Count() - 1;
        ReOpenCurrentMusic();
        PlayOrResume();
    }

    private void ReOpenCurrentMusic()
    {
        _output.Stop();
        _source?.Dispose();
        _decoder?.Dispose();
        _resampler?.Dispose();

        _source = new FFmpegSource(Playlist[_playingIndex].SoundSource.First());
        _decoder = new FFmpegDecoder(_source);
        _resampler =
            new FFmpegResampler(_decoder.AvCodecCtx, _source.Format.ToFmt(), _source.ChannelLayout, _source.SampleRate);
        _output.Open(_source, _decoder, _resampler);
    }

    ~PlaybackManager()
    {
        _output.Dispose();
        _source?.Dispose();
        _decoder?.Dispose();
        _resampler?.Dispose();
    }
}