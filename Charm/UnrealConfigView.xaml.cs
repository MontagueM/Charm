using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Field;

namespace Charm;

public partial class UnrealConfigView : UserControl
{
    public UnrealConfigView()
    {
        InitializeComponent();
    }
    
    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private void PopulateConfigPanel()
    {
        UnrealConfigPanel.Children.Clear();

        TextBlock header = new TextBlock();
        header.Text = "Unreal Engine Settings";
        header.FontSize = 30;
        UnrealConfigPanel.Children.Add(header);

        // Unreal interop path
        ConfigSettingControl cui = new ConfigSettingControl();
        cui.SettingName = "Unreal content path";
        var val = ConfigHandler.GetUnrealInteropPath();
        cui.SettingValue = val == "" ? "Not set" : val;
        cui.ChangeButton.Click += UnrealInteropPath_OnClick;
        UnrealConfigPanel.Children.Add(cui);
        
        // Enable UE5 interop
        ConfigSettingControl cii = new ConfigSettingControl();
        cii.SettingName = "Generate Unreal Engine importing files";
        bool bval = ConfigHandler.GetUnrealInteropEnabled();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropEnabled_OnClick;
        UnrealConfigPanel.Children.Add(cii);
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
}