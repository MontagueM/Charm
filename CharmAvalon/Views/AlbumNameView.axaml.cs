using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CharmAvalon.Views;

public partial class AlbumNameView : UserControl
{
    public AlbumNameView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

