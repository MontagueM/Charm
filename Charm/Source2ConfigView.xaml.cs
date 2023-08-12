using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class Source2ConfigView : UserControl
{
    public Source2ConfigView()
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
        S2ConfigPanel.Children.Clear();


        TextBlock header = new TextBlock();
        header.Text = "Source 2 Settings";
        header.FontSize = 30;
        S2ConfigPanel.Children.Add(header);

        TextBlock lbl = new TextBlock();
        lbl.Text = "Currently, only S&Box is supported";
        lbl.FontSize = 10;
        S2ConfigPanel.Children.Add(lbl);

        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Source 2 tools path";
        var val = _config.GetSource2Path();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += Source2Path_OnClick;
        S2ConfigPanel.Children.Add(cpp);

        TextBlock lbl2 = new TextBlock();
        lbl2.Text = "Currently not used for anything";
        lbl2.FontSize = 15;
        S2ConfigPanel.Children.Add(lbl2);

        // Enable source 2 shader generation
        ConfigSettingControl cbe = new ConfigSettingControl();
        cbe.SettingName = "Generate shaders (vfx)";
        bool bval2 = _config.GetS2ShaderExportEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += S2ShaderExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cbe);

        // Enable vmat material generation
        // ### Might as well just make vmats by default
        // ConfigSettingControl cef = new ConfigSettingControl();
        // cef.SettingName = "Generate materials (vmat)";
        // bool bval = ConfigSubsystem.GetS2VMATExportEnabled();
        // cef.SettingValue = bval.ToString();
        // cef.ChangeButton.Click += S2VMATExportEnabled_OnClick;
        // S2ConfigPanel.Children.Add(cef);

        // Enable vmdl model generation
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.SettingName = "Generate models (vmdl)";
        bool bval = _config.GetS2VMDLExportEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += S2VMDLExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cfe);
    }

    private void Source2Path_OnClick(object sender, RoutedEventArgs e)
    {
        OpenSource2PathDialog();
        PopulateConfigPanel();
    }

    public void OpenSource2PathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            // Steam\steamapps\common\sbox\bin\win64
            dialog.Description = "Select the folder where your S&Box installation is located";
            bool success = false;
            while (!success)
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = _config.TrySetSource2Path(dialog.SelectedPath);
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

    private void S2ShaderExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2ShaderExportEnabled(!_config.GetS2ShaderExportEnabled());
        PopulateConfigPanel();
    }

    private void S2VMATExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2VMATExportEnabled(!_config.GetS2VMATExportEnabled());
        PopulateConfigPanel();
    }

     private void S2VMDLExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2VMDLExportEnabled(!_config.GetS2VMDLExportEnabled());
        PopulateConfigPanel();
    }
}
