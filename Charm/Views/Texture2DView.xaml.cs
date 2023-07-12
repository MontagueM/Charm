using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Charm.Objects;

namespace Charm.Views;

public partial class Texture2DView : UserControl
{
    public Texture2DView()
    {
        InitializeComponent();
    }

    private void Texture2DView_OnLoaded(object sender, RoutedEventArgs e)
    {
        Task.Run((DataContext as TextureListItemModel).Load2);
    }

    private void Texture2DView_OnUnloaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"Texture2DView_OnUnloaded {(DataContext as TextureListItemModel).Hash}");
    }
}
