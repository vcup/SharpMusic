using SharpMusic.Core.Playback;

namespace SharpMusic.Core.Management;

public class PlayerManager
{
    private readonly Player _player;
    private AudioSource? _currentAudioSource;

    public PlayerManager(Player player, IEnumerable<AudioSource> audioSources)
    {
        _player = player;
        _currentAudioSource = null;
        AudioSourceList = audioSources.ToList();
    }

    public PlayerManager(IEnumerable<AudioSource> audioSources) : this(new Player(), audioSources)
    {
    }

    public PlayerManager() : this(Array.Empty<AudioSource>())
    {
    }

    public long PlayPositionTicks
    {
        get => _player.Position.Ticks;
        set => _player.Position = TimeSpan.FromTicks(value);
    }

    public long PlaybackTimeTicks => _player.Lenght.Ticks;

    public PlaybackState PlaybackState => _player.PlaybackState;

    public List<AudioSource> AudioSourceList { get; }

    public AudioSource? CurrentAudioSource
    {
        get => _currentAudioSource ?? AudioSourceList.FirstOrDefault();
        set
        {
            if (value is null) return;
            if (!AudioSourceList.Contains(value))
            {
                AudioSourceList.Add(value);
            }

            _currentAudioSource = value;
        }
    }

    public void PlayOrResume()
    {
        switch (_player.PlaybackState)
        {
            case PlaybackState.Paused:
                _player.Resume();
                return;
            case PlaybackState.Playing:
                _player.Pause();
                return;
            case PlaybackState.Buffering:
                return;
            case PlaybackState.Stopped:
                if (_player.AudioSource is null)
                {
                    if (CurrentAudioSource is null) return;
                    _player.Open(CurrentAudioSource);
                    _player.Play();
                }
                else
                {
                    _player.Play();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Pause()
    {
        _player.Pause();
    }

    public void Stop()
    {
        _player.Stop();
    }

    public void PlayNext()
    {
        if (CurrentAudioSource is null) return;
        if (_player.PlaybackState is PlaybackState.Playing)
        {
            _player.Stop();
        }

        var index = AudioSourceList.IndexOf(CurrentAudioSource);
        if (++index == AudioSourceList.Count)
        {
            index = 0; // AudioSource.Count never equal 0 when CurrentAudioSource is not null
        }

        _player.Open(CurrentAudioSource = AudioSourceList[index]);
        _player.Play();
    }

    public void PlayPrev()
    {
        if (CurrentAudioSource is null) return;
        if (_player.PlaybackState is PlaybackState.Playing)
        {
            _player.Stop();
        }

        var index = AudioSourceList.IndexOf(CurrentAudioSource);
        if (--index < 0) index = AudioSourceList.Count - 1;

        _player.Open(CurrentAudioSource = AudioSourceList[index]);
        _player.Play();
    }
}