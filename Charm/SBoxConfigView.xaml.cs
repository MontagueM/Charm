using System;
using System.Collections.Generic;
using System.IO;
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

        // S&Box tools path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "S&Box tools path";
        var val = _config.GetSBoxToolsPath();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += SBoxToolsPath_OnClick;
        SBoxConfigPanel.Children.Add(cpp);

        // S&Box content folder path
        ConfigSettingControl content = new ConfigSettingControl();
        content.SettingName = "Content Folder path";
        var content_path = _config.GetSBoxContentPath();
        content.SettingValue = content_path == "" ? "Not set" : content_path;
        content.ChangeButton.Click += SBoxContentPath_OnClick;
        SBoxConfigPanel.Children.Add(content);
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

    private void SBoxContentPath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenSBoxContentPathDialog();
        PopulateConfigPanel();
    }

    public void OpenSBoxContentPathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = "Select the folder where compilied content will be stored";
            bool success = false;
            while (!success)
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = _config.TrySetSBoxContentPath(dialog.SelectedPath);
                }
                else
                {
                    return;
                }

                if (!success)
                {
                    MessageBox.Show(
                        "Directory selected is invalid, please select a valid directory");
                }
                else
                {
                    Directory.CreateDirectory(dialog.SelectedPath);
                    if(!File.Exists($"{dialog.SelectedPath}/.sbproj"))
                    {
                        CreateSBProject(dialog.SelectedPath);
                    }
                }
            }
        }
    }

    private void CreateSBProject(string path)
    {
        string project_template = "{\r\n  \"Title\": \"Destiny Resources\",\r\n  \"Type\": \"content\",\r\n  \"Tags\": null,\r\n  \"Schema\": 1,\r\n  \"HasAssets\": true,\r\n  \"AssetsPath\": \"\",\r\n  \"Resources\": null,\r\n  \"MenuResources\": null,\r\n  \"HasCode\": false,\r\n  \"CodePath\": null,\r\n  \"PackageReferences\": [],\r\n  \"EditorReferences\": null,\r\n  \"Metadata\": {}\r\n}";

        File.WriteAllText($"{path}/.sbproj", project_template);
    }
}
