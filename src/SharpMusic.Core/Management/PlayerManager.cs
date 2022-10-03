using CSCore.SoundOut;
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

    public long PlayPositionTicks
    {
        get => _player.Position.Ticks;
        set => _player.Position = TimeSpan.FromTicks(value);
    }

    public long PlaybackTimeTicks => _player.Lenght.Ticks;

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
        if (_player.PlaybackState is PlaybackState.Paused)
        {
            _player.Resume();
        }

        if (_player.AudioSource is null)
        {
            if (CurrentAudioSource is not null)
            {
                _player.Open(CurrentAudioSource);
            }
        }
        else
        {
            _player.Play();
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