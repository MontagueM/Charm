using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using Tiger;

namespace Charm.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand ExportMapToAtlas { get; }
    public ReactiveCommand<Unit, Unit> OpenHash { get; }

    public MainWindowViewModel()
    {
    }

}
