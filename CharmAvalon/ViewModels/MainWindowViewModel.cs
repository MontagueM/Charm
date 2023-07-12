using System.Windows.Input;
using ReactiveUI;

namespace CharmAvalon.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        BuyMusicCommand = ReactiveCommand.Create(() =>
        {
            // Code here will be executed when the button is clicked.
        });
    }

    public ICommand BuyMusicCommand { get; }
}
