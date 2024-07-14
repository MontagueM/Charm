using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class CollectionItemControl : UserControl
{
    private static MainWindow _mainWindow = null;

    public CollectionItemControl()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON) // TODO?
            ItemInspectButton.Visibility = Visibility.Collapsed;
    }

    private void InspectAPIItem_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        ApiItem apiItem = Container.DataContext as ApiItem;

        APIItemView apiItemView = new APIItemView(apiItem);
        _mainWindow.MakeNewTab(apiItem.ItemName, apiItemView);
        _mainWindow.SetNewestTabSelected();
    }
}
