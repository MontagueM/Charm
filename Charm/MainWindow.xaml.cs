using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field;
using Field.General;

namespace Charm;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Hide tab by default
        MainTabControl.Visibility = Visibility.Hidden;
            
        // Check if packages path exists in config
        ConfigHandler.CheckPackagesPathIsValid();
        MainTabControl.Visibility = Visibility.Visible;

        // Initialise FNV handler
        FnvHandler.Initialise();
            
        // Initialise global string cache
        PackageHandler.GenerateGlobalStringContainerCache();
            
        // Get all hash64
        TagHash64Handler.Initialise();
        
        // Initialise fbx handler
        FbxHandler.Initialise();
    }

    private void OpenConfigPanel_OnClick(object sender, RoutedEventArgs e)
    {
        TabItem newTab = new TabItem();
        newTab.Header = "Configuration";
        newTab.Content = new ConfigView();
        MainTabControl.Items.Add(newTab);
        MainTabControl.SelectedItem = newTab;
    }
}