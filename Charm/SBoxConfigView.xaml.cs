using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tiger;

namespace Charm;

public partial class SBoxConfigView : UserControl
{
    public SBoxConfigView()
    {
        InitializeComponent();
        _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    }

    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private ConfigSubsystem _config;

    private void PopulateConfigPanel()
    {
        SBoxConfigPanel.Children.Clear();

        TextBlock header = new TextBlock();
        header.Text = "S&Box Settings";
        header.FontSize = 30;
        SBoxConfigPanel.Children.Add(header);

        //TextBlock lbl = new TextBlock();
        //lbl.Text = "Currently, only S&Box is supported";
        //lbl.FontSize = 10;
        //S2ConfigPanel.Children.Add(lbl);

        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "S&Box tools path";
        var val = _config.GetSBoxToolsPath();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += SBoxToolsPath_OnClick;
        SBoxConfigPanel.Children.Add(cpp);

        // Enable source 2 shader generation
        ConfigSettingControl cbe = new ConfigSettingControl();
        cbe.SettingName = "Generate shaders (.shader)";
        bool bval2 = _config.GetSBoxShaderExportEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += SBoxShaderExportEnabled_OnClick;
        SBoxConfigPanel.Children.Add(cbe);

        // Enable vmdl model generation
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.SettingName = "Generate models (.vmdl)";
        bool bval = _config.GetSBoxModelExportEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += SBoxModelExportEnabled_OnClick;
        SBoxConfigPanel.Children.Add(cfe);
    }

    private void SBoxToolsPath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenSBoxToolsPathDialog();
        PopulateConfigPanel();
    }

    public void OpenSBoxToolsPathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            // Steam\steamapps\common\sbox\bin\win64
            dialog.Description = "Select the folder where your S&Box tools are located (sbox\\bin\\win64)";
            bool success = false;
            while (!success)
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = _config.TrySetSBoxToolsPath(dialog.SelectedPath);
                }
                else
                {
                    return;
                }

                if (!success)
                {
                    MessageBox.Show(
                        "Directory selected is invalid, please select the correct directory. (Steam/steamapps/common/sbox/bin/win64)");
                }
            }
        }
    }

    private void SBoxShaderExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSBoxShaderExportEnabled(!_config.GetSBoxShaderExportEnabled());
        PopulateConfigPanel();
    }

    private void SBoxMaterialExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSBoxMaterialExportEnabled(!_config.GetSBoxMaterialExportEnabled());
        PopulateConfigPanel();
    }

    private void SBoxModelExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSBoxModelExportEnabled(!_config.GetSBoxModelExportEnabled());
        PopulateConfigPanel();
    }
}
