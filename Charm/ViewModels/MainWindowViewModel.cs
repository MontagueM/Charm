using System.Windows.Input;
using ReactiveUI;

namespace Charm.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public ICommand ExportMapToAtlas { get; }

    public MainWindowViewModel()
    {
        ExportMapToAtlas = ReactiveCommand.CreateFromTask(async () =>
        {
            
        });
    }
}