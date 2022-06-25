using System;
using System.Windows;
using System.Windows.Controls;

namespace Charm;

public partial class ConfigView : UserControl
{
    public ConfigView()
    {
        InitializeComponent();
    }
    
    private void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private void PopulateConfigPanel()
    {
        ConfigPanel.Children.Clear();
        
        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Packages path";
        var val = ConfigHandler.GetPackagesPath();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += PackagesPath_OnClick;
        ConfigPanel.Children.Add(cpp);

        // Unreal interop path
        ConfigSettingControl cui = new ConfigSettingControl();
        cui.SettingName = "Unreal content path";
        val = ConfigHandler.GetUnrealInteropPath();
        cui.SettingValue = val == "" ? "Not set" : val;
        cui.ChangeButton.Click += UnrealInteropPath_OnClick;
        ConfigPanel.Children.Add(cui);
        
        // Unreal interop import to child folder boolean
        ConfigSettingControl cii = new ConfigSettingControl();
        cii.SettingName = "Import to unreal into a child folder of the content path";
        bool bval = ConfigHandler.GetUnrealInteropImportToChildFolder();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropChildFolder_OnClick;
        ConfigPanel.Children.Add(cii);
    }

    private void PackagesPath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenPackagesPathDialog();
        PopulateConfigPanel();
    }
    
    private void UnrealInteropPath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenUnrealInteropPathDialog();
        PopulateConfigPanel();
    }
    
    private void UnrealInteropChildFolder_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetUnrealInteropImportToChildFolder(!ConfigHandler.GetUnrealInteropImportToChildFolder());
        PopulateConfigPanel();
    }
}