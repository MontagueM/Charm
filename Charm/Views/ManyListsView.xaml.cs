using System.Windows;
using System.Windows.Controls;
using Charm.Objects;

namespace Charm.Views;

public partial class ManyListsView : UserControl
{
    public ManyListsView()
    {
        InitializeComponent();
    }

    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ((e.AddedItems[0] as TabItem)?.Content as FileControl)?.Load();
    }
}

