using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using SharpMusic.GUI.ViewModels;

namespace SharpMusic.GUI.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(b => b(ViewModel!.MinimizeWindowCommand.Subscribe(_ => WindowState = WindowState.Minimized)));
        this.WhenActivated(b => b(ViewModel!.CloseWindowCommand.Subscribe(_ => Close())));
    }
}