using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class DareItemControl : UserControl
{
    private static MainWindow _mainWindow = null;

    public DareItemControl()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        if (Strategy.IsD1()) // TODO?
            ItemInspectButton.Visibility = Visibility.Collapsed;
    }

    private void InspectAPIItem_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        DareItem apiItem = Container.DataContext as DareItem;

        ItemView apiItemView = new(apiItem.Item);
        _mainWindow.MakeNewTab(apiItem.ItemName, apiItemView);
        _mainWindow.SetNewestTabSelected();
    }
}
