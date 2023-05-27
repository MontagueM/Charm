using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Charm.ViewModels;
using Tiger;
namespace Charm.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var config = Strategy.GetStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
        config.PackagesDirectory = "I:/v6307/packages/";
        Strategy.UpdateStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307, config);
        Strategy.SetStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
    }

    private void OpenConfigPanel_OnClick(object? sender, RoutedEventArgs e)
    {

    }

    private void OpenLogPanel_OnClick(object? sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}
