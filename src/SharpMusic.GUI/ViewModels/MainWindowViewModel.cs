using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using SharpMusic.Core.Management;
using SharpMusic.Core.Playback;

namespace SharpMusic.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly PlayerManager _playerManager;

    public MainWindowViewModel()
    {
        _playerManager = new PlayerManager(new []
        {
            new AudioSource(new Uri("https://vcup.moe/e/_bazaar records - e^(x+i)＜ 3u.wav")),
            new AudioSource(new Uri("https://vcup.moe/e/森羅万象 - シンクロ0.wav")),
        });
        Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(100);
                if (_playerManager.PlaybackState is not PlaybackState.Stopped) continue;
                this.RaisePropertyChanged(nameof(PlayPosition));
            }
            // ReSharper disable once FunctionNeverReturns
        });
    }

    public ReactiveCommand<Unit, Unit> MinimizeWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public void PlayOrResume() => _playerManager.PlayOrResume();

    public void Stop() => _playerManager.Stop();

    public void PlayNext() => _playerManager.PlayNext();

    public void PlayPrev() => _playerManager.PlayPrev();

    public long PlayPosition
    {
        get => _playerManager.PlayPositionTicks;
        set => _playerManager.PlayPositionTicks = value;
    }

    public long PlaybackTime => _playerManager.PlaybackTimeTicks;
}