using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CharmAvalon.ViewModels;
using ReactiveUI;

namespace CharmAvalon.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);

        // InteractionContext<MusicStoreViewModel, AlbumViewModel?> interaction?;
    }
}
