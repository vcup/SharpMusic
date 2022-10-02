using System.Reactive;
using ReactiveUI;

namespace SharpMusic.GUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, Unit> MinimizeWindowCommand { get; } = ReactiveCommand.Create(() => { });

    public ReactiveCommand<Unit, Unit> CloseWindowCommand { get; } = ReactiveCommand.Create(() => { });
}