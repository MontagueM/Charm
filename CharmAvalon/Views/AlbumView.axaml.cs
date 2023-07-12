using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CharmAvalon.Views;

public partial class AlbumView : UserControl
{
    public AlbumView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

