using System.Windows;
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

        //TextBlock header = new TextBlock();
        //header.Text = "Source 2 Settings";
        //header.FontSize = 30;
        //S2ConfigPanel.Children.Add(header);

        //TextBlock lbl = new TextBlock();
        //lbl.Text = "Currently, only S&Box is supported";
        //lbl.FontSize = 10;
        //S2ConfigPanel.Children.Add(lbl);

        // Packages path
        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Source 2 Tools Path";
        var val = _config.GetSource2Path();
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += Source2Path_OnClick;
        S2ConfigPanel.Children.Add(cpp);

        // Enable source 2 shader generation
        ConfigSettingToggleControl cbe = new ConfigSettingToggleControl();
        cbe.SettingName = "Generate Shaders";
        bool bval2 = _config.GetS2ShaderExportEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += S2ShaderExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cbe);

        // Enable vmdl model generation
        ConfigSettingToggleControl cfe = new ConfigSettingToggleControl();
        cfe.SettingName = "Generate Models";
        bool bval = _config.GetS2VMDLExportEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += S2VMDLExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cfe);

        // Resize textures to nearest power of 2
        ConfigSettingToggleControl pw2 = new ConfigSettingToggleControl();
        pw2.SettingName = "Resize Textures to Nearest Power of 2";
        bool bval3 = _config.GetS2TexPow2Enabled();
        pw2.SettingValue = bval3.ToString();
        pw2.ChangeButton.Click += S2TexPow2Enabled_OnClick;
        S2ConfigPanel.Children.Add(pw2);
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
        if (_config.GetS2ShaderExportEnabled())
        {
            _config.SetIndvidualStaticsEnabled(true);
            _config.SetS2TexPow2Enabled(true);
            _config.SetExportMaterials(true);
        }
        PopulateConfigPanel();
    }

    private void S2VMDLExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2VMDLExportEnabled(!_config.GetS2VMDLExportEnabled());
        if (_config.GetS2VMDLExportEnabled())
            _config.SetIndvidualStaticsEnabled(true);
        PopulateConfigPanel();
    }

    private void S2TexPow2Enabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2TexPow2Enabled(!_config.GetS2TexPow2Enabled());
        PopulateConfigPanel();
    }
}
