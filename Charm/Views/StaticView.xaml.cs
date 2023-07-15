using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Charm.Objects;

namespace Charm.Views;

public partial class StaticView : UserControl
{
    public StaticView()
    {
        InitializeComponent();
    }

    // todo reconcile with the other copies of this code
    // private void StaticView_OnLoaded(object sender, RoutedEventArgs e)
    // {
    //     Task.Run((DataContext as HashListItemModel).Load);
    // }
    //
    // private void StaticView_OnUnloaded(object sender, RoutedEventArgs e)
    // {
    //     Task.Run((DataContext as HashListItemModel).Unload);
    // }

}
