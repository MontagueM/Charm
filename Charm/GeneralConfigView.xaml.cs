using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Field;

namespace Charm;

public partial class GeneralConfigView : UserControl
{
    public GeneralConfigView()
    {
        InitializeComponent();
    }
    
    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private void PopulateConfigPanel()
    {
        GeneralConfigPanel.Children.Clear();
        
        TextBlock header = new TextBlock();
        header.Text = "General Settings";
        header.FontSize = 30;
        GeneralConfigPanel.Children.Add(header);

        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Packages path";
        var val = ConfigHandler.GetPackagesPath();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += PackagesPath_OnClick;
        GeneralConfigPanel.Children.Add(cpp);
        
        // Save path
        ConfigSettingControl csp = new ConfigSettingControl();
        csp.SettingName = "Export save path";
        val = ConfigHandler.GetExportSavePath();
        csp.SettingValue = val == "" ? "Not set" : val;
        csp.ChangeButton.Click += ExportSavePath_OnClick;
        GeneralConfigPanel.Children.Add(csp);

        // Enable Blender interop //force people to use the addon now >:)
        //ConfigSettingControl cbe = new ConfigSettingControl();
        //cbe.SettingName = "Generate Blender importing script";
        //bool bval2 = ConfigHandler.GetBlenderInteropEnabled();
        //cbe.SettingValue = bval2.ToString();
        //cbe.ChangeButton.Click += BlenderInteropEnabled_OnClick;
        //GeneralConfigPanel.Children.Add(cbe);
        
        // Enable combined extraction folder for maps
        ConfigSettingControl cef = new ConfigSettingControl();
        cef.SettingName = "Use single folder extraction for maps";
        var bval = ConfigHandler.GetSingleFolderMapsEnabled();
        cef.SettingValue = bval.ToString();
        cef.ChangeButton.Click += SingleFolderMapsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cef);

        // Enable individual static extraction with maps
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.SettingName = "Export individual static meshes with maps";
        bval = ConfigHandler.GetIndvidualStaticsEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += IndvidualStaticsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cfe);

        // Enable individual entity extraction with maps
        ConfigSettingControl ents = new ConfigSettingControl();
        ents.SettingName = "Export individual entities with maps";
        bval = ConfigHandler.GetIndvidualEntitiesEnabled();
        ents.SettingValue = bval.ToString();
        ents.ChangeButton.Click += IndvidualEntitiesEnabled_OnClick;
        GeneralConfigPanel.Children.Add(ents);

        // Enable CBuffer export to own txt
        ConfigSettingControl cbuffers = new ConfigSettingControl();
        cbuffers.SettingName = "Save CBuffers";
        bval = ConfigHandler.GetSaveCBuffersEnabled();
        cbuffers.SettingValue = bval.ToString();
        cbuffers.ChangeButton.Click += SaveCBuffersEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cbuffers);

        // Output texture format
        ConfigSettingComboControl ctf = new ConfigSettingComboControl();
        ctf.SettingName = "Output texture format";
        ETextureFormat etfval = ConfigHandler.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<ETextureFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(ctf);

        TextBlock lbl = new TextBlock();
        lbl.Text = "(Use PNG or TGA in Blender)";
        lbl.FontSize = 15;
        GeneralConfigPanel.Children.Add(lbl);

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

    private void IndvidualStaticsEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetIndvidualStaticsEnabled(!ConfigHandler.GetIndvidualStaticsEnabled());
        PopulateConfigPanel();
    }

    private void IndvidualEntitiesEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetIndvidualEntitiesEnabled(!ConfigHandler.GetIndvidualEntitiesEnabled());
        PopulateConfigPanel();
    }

    private void SaveCBuffersEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetSaveCBuffersEnabled(!ConfigHandler.GetSaveCBuffersEnabled());
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