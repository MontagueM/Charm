using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Field.Textures;

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
        
        // Save path
        ConfigSettingControl csp = new ConfigSettingControl();
        csp.SettingName = "Export save path";
        val = ConfigHandler.GetExportSavePath();
        csp.SettingValue = val == "" ? "Not set" : val;
        csp.ChangeButton.Click += ExportSavePath_OnClick;
        ConfigPanel.Children.Add(csp);

        // Unreal interop path
        ConfigSettingControl cui = new ConfigSettingControl();
        cui.SettingName = "Unreal content path";
        val = ConfigHandler.GetUnrealInteropPath();
        cui.SettingValue = val == "" ? "Not set" : val;
        cui.ChangeButton.Click += UnrealInteropPath_OnClick;
        ConfigPanel.Children.Add(cui);
        
        // Enable UE5 interop
        ConfigSettingControl cii = new ConfigSettingControl();
        cii.SettingName = "Generate unreal engine importing files";
        bool bval = ConfigHandler.GetUnrealInteropEnabled();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropEnabled_OnClick;
        ConfigPanel.Children.Add(cii);
        
        // Enable combined extraction folder for maps
        ConfigSettingControl cef = new ConfigSettingControl();
        cef.SettingName = "Use single folder extraction for maps";
        bval = ConfigHandler.GetSingleFolderMapsEnabled();
        cef.SettingValue = bval.ToString();
        cef.ChangeButton.Click += SingleFolderMapsEnabled_OnClick;
        ConfigPanel.Children.Add(cef);
        
        // Output texture format
        ConfigSettingComboControl ctf = new ConfigSettingComboControl();
        ctf.SettingName = "Output texture format";
        ETextureFormat etfval = ConfigHandler.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<ETextureFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.ChangeButton.Click += OutputTextureFormat_OnClick;
        ConfigPanel.Children.Add(ctf);
    }

    private List<ComboBoxItem> MakeEnumComboBoxItems<T>() where T : Enum
    {
        List<ComboBoxItem> items = new List<ComboBoxItem>();
        foreach (T val in Enum.GetValues(typeof(T)))
        {
            items.Add(new ComboBoxItem { Content = TagItem.GetEnumDescription(val) });
        }
        return items;
    }
    
    private void ConsiderShowingMainMenu()
    {
        if (ConfigHandler.DoesPathKeyExist("packagesPath") && ConfigHandler.DoesPathKeyExist("exportSavePath"))
        {
            var _mainWindow = Window.GetWindow(this) as MainWindow;
            _mainWindow.ShowMainMenu();
        }
    }

    private void PackagesPath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenPackagesPathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }
    
    private void ExportSavePath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenExportSavePathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }
    
    private void UnrealInteropPath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenUnrealInteropPathDialog();
        PopulateConfigPanel();
    }
    
    private void UnrealInteropEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ConfigHandler.DoesPathKeyExist("unrealInteropPath"))
        {
            MessageBox.Show("Please set the path to the Unreal Engine content folder first.");
            return;
        }
        ConfigHandler.SetUnrealInteropEnabled(!ConfigHandler.GetUnrealInteropEnabled());
        PopulateConfigPanel();
    }
    
    private void SingleFolderMapsEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetSingleFolderMapsEnabled(!ConfigHandler.GetSingleFolderMapsEnabled());
        PopulateConfigPanel();
    }
    
    private void OutputTextureFormat_OnClick(object sender, RoutedEventArgs e)
    {
        // ConfigHandler.SetOutputTextureFormat();
        var index = ((sender as Button).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedIndex;
        ConfigHandler.SetOutputTextureFormat((ETextureFormat)index);
        PopulateConfigPanel();    
    }
}