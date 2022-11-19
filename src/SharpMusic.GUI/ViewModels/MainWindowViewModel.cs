using System.Reactive;
using ReactiveUI;
using SharpMusic.Core.Management;

namespace SharpMusic.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly PlaybackManager _playbackManager;

    public MainWindowViewModel()
    {
        _playbackManager = new PlaybackManager();
    }

    public ReactiveCommand<Unit, Unit> MinimizeWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public void PlayOrResume() => _playbackManager.PlayOrResume();

    public void Stop() => _playbackManager.Stop();

    public void PlayNext() => _playbackManager.PlayNext();

    public void PlayPrev() => _playbackManager.PlayPrev();

    public long PlayPosition
    {
        get => _playbackManager.PlayPositionTicks;
        set => _playbackManager.PlayPositionTicks = value;
    }

    public long PlaybackTime => _playbackManager.PlaybackTimeTicks;
}