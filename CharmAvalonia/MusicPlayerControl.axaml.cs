using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CharmAvalonia;

public partial class MusicPlayerControl : UserControl
{
    public MusicPlayerControl()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

