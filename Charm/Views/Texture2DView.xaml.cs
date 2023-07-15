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

    // todo reconcile with the other copies of this code
    // private void Texture2DView_OnLoaded(object sender, RoutedEventArgs e)
    // {
    //     Task.Run((DataContext as HashListItemModel).Load);
    // }
    //
    // private void Texture2DView_OnUnloaded(object sender, RoutedEventArgs e)
    // {
    //     Task.Run((DataContext as HashListItemModel).Unload);
    // }



    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {

    }
}
