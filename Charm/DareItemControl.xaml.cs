using System.Windows;
using System.Windows.Controls;

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
