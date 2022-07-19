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
        cii.SettingName = "Generate Unreal Engine importing files";
        bool bval = ConfigHandler.GetUnrealInteropEnabled();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropEnabled_OnClick;
        ConfigPanel.Children.Add(cii);

        // Enable Blender interop
        ConfigSettingControl cbe = new ConfigSettingControl();
        cbe.SettingName = "Generate Blender importing script";
        bool bval2 = ConfigHandler.GetBlenderInteropEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += BlenderInteropEnabled_OnClick;
        ConfigPanel.Children.Add(cbe);
        
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
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.Visibility = Visibility.Hidden;
        //ctf.ChangeButton.Click += OutputTextureFormat_OnClick;
        ConfigPanel.Children.Add(ctf);

        TextBlock lbl = new TextBlock();
        lbl.Text = "(Use PNG or TGA in Blender)";
        lbl.FontSize = 15;
        ConfigPanel.Children.Add(lbl);

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

    private void BlenderInteropEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ConfigHandler.GetBlenderInteropEnabled())
        {
            MessageBox.Show("Blender will NOT import shaders. Please have moderate Blender shader knowledge.");
        }
        ConfigHandler.SetBlenderInteropEnabled(!ConfigHandler.GetBlenderInteropEnabled());
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
        TextureExtractor.SetTextureFormat(ConfigHandler.GetOutputTextureFormat());
        PopulateConfigPanel();    
    }
    private void OutputTextureFormat_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var index = ((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedIndex;
        ConfigHandler.SetOutputTextureFormat((ETextureFormat)index);
        TextureExtractor.SetTextureFormat(ConfigHandler.GetOutputTextureFormat());
        PopulateConfigPanel();    
    }
}