using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Field;

namespace Charm;

public partial class Source2ConfigView : UserControl
{
    public Source2ConfigView()
    {
        InitializeComponent();
    }
    
    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private void PopulateConfigPanel()
    {
        S2ConfigPanel.Children.Clear();

        TextBlock header = new TextBlock();
        header.Text = "Source 2 Settings";
        header.FontSize = 30;
        S2ConfigPanel.Children.Add(header);

        TextBlock lbl = new TextBlock();
        lbl.Text = "Only S&Box is supported";
        lbl.FontSize = 10;
        S2ConfigPanel.Children.Add(lbl);

        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Source 2 tools path";
        var val = ConfigHandler.GetSource2Path();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += Source2Path_OnClick;
        S2ConfigPanel.Children.Add(cpp);

        TextBlock lbl2 = new TextBlock();
        lbl2.Text = "Currently not used for anything";
        lbl2.FontSize = 15;
        S2ConfigPanel.Children.Add(lbl2);
        
        // Enable source 2 shader generation
        ConfigSettingControl cbe = new ConfigSettingControl();
        cbe.SettingName = "Generate shaders (.shader)";
        bool bval2 = ConfigHandler.GetS2ShaderExportEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += S2ShaderExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cbe);
        
        // Enable vmat material generation
        // ### Might as well just make vmats by default
        // ConfigSettingControl cef = new ConfigSettingControl();
        // cef.SettingName = "Generate materials (vmat)";
        // bool bval = ConfigHandler.GetS2VMATExportEnabled();
        // cef.SettingValue = bval.ToString();
        // cef.ChangeButton.Click += S2VMATExportEnabled_OnClick;
        // S2ConfigPanel.Children.Add(cef);

        // Enable vmdl model generation
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.SettingName = "Generate models (.vmdl)";
        bool bval = ConfigHandler.GetS2VMDLExportEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += S2VMDLExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cfe);
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

    private void Source2Path_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenSource2PathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }
    
    private void ExportSavePath_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.OpenExportSavePathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }

    private void S2ShaderExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetS2ShaderExportEnabled(!ConfigHandler.GetS2ShaderExportEnabled());
        PopulateConfigPanel();
    }

    private void S2VMATExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetS2VMATExportEnabled(!ConfigHandler.GetS2VMATExportEnabled());
        PopulateConfigPanel();
    }

     private void S2VMDLExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        ConfigHandler.SetS2VMDLExportEnabled(!ConfigHandler.GetS2VMDLExportEnabled());
        PopulateConfigPanel();
    }
}