using System.Reactive;
using ReactiveUI;
using SharpMusic.Core.Management;

namespace SharpMusic.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly PlayerManager _playerManager;

    public MainWindowViewModel()
    {
        _playerManager = new PlayerManager();
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