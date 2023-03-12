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
    private FFmpegCodec? _decoder;
    private FFmpegResampler? _resampler;
    private int _playingIndex;
    private PlaybackMode _playbackMode;

    public PlaybackManager()
    {
        _output = new();
        SoundInfoCache = new();
        Playlist = new(Guid.Empty);
    }

    public TimeSpan PlayPosition
    {
        get => _source?.Position ?? TimeSpan.Zero;
        set
        {
            if (_source is not null) _source.Position = value;
        }
    }

    public long PlaybackTimeTicks => _source is not null ? _source.Duration.Ticks : 0;

    public PlaybackState PlaybackState => _output.State;

    /// <summary>
    /// control output volume, value range: 0~100
    /// </summary>
    public int Volume
    {
        get => (int)(_output.Volume / (_output.MaxVolume / 100.0));
        set => _output.Volume = (int)(value * (_output.MaxVolume / 100.0));
    }

    public void Mute() => _output.IsMute = !_output.IsMute;

    public PlaybackMode PlaybackMode
    {
        get => _playbackMode;
        set
        {
            if (_source is not null)
            {
                switch (value)
                {
                    case PlaybackMode.LoopAll:
                        _source.SourceEofEvent -= LoopAllOnSourceEof;
                        _source.SourceEofEvent += LoopAllOnSourceEof;
                        break;
                    case PlaybackMode.Shuffle:
                        _source.SourceEofEvent -= ShuffleOnSourceEof;
                        _source.SourceEofEvent += ShuffleOnSourceEof;
                        break;
                    case PlaybackMode.LoopSingle:
                        _source.SourceEofEvent -= LoopSingleOnSourceEof;
                        _source.SourceEofEvent += LoopSingleOnSourceEof;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                void LoopAllOnSourceEof(FFmpegSource sender)
                {
                    PlayNext();
                }

                void ShuffleOnSourceEof(FFmpegSource sender)
                {
                    _playingIndex = Random.Shared.Next(Playlist.Count());
                    ReOpenCurrentMusic();
                    PlayOrResume();
                }

                void LoopSingleOnSourceEof(FFmpegSource sender)
                {
                    sender.ResetStream();
                }
            }

            _playbackMode = value;
        }
    }

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
        _source.MoveNextAudioPacket();
        _decoder = FFmpegCodec.CreateDecoder(_source);
        _resampler = _source.Format.IsSupportFFmpegAndSdl2()
            ? null
            : new FFmpegResampler
                (_decoder.AvCodecCtx, _source.Format.ToFmt(false), _source.ChannelLayout, _source.SampleRate);
        _output.Open(_source, _decoder, _resampler);
        // rebinding event
        PlaybackMode = _playbackMode;
    }

    ~PlaybackManager()
    {
        _output.Dispose();
        _source?.Dispose();
        _decoder?.Dispose();
        _resampler?.Dispose();
    }
}