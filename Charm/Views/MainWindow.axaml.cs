using System;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Tiger;

namespace Charm.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Strategy.SetStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601);
    }
}
