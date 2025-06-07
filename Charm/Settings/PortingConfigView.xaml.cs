using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class PortingConfigView : UserControl
{
    public PortingConfigView()
    {
        InitializeComponent();
        _config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    }

    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateS2ConfigPanel();
        PopulateUEConfigPanel();
    }

    private ConfigSubsystem _config;

    private void PopulateS2ConfigPanel()
    {
        S2ConfigPanel.Children.Clear();

        // Enable source 2 shader generation
        ConfigSettingToggleControl cbe = new();
        cbe.SettingName = "Generate Shaders";
        bool bval2 = _config.GetS2ShaderExportEnabled();
        cbe.SettingValue = bval2.ToString();
        cbe.ChangeButton.Click += S2ShaderExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cbe);

        // Enable vmdl model generation
        ConfigSettingToggleControl cfe = new();
        cfe.SettingName = "Generate Models";
        bool bval = _config.GetS2VMDLExportEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += S2VMDLExportEnabled_OnClick;
        S2ConfigPanel.Children.Add(cfe);

        // Resize textures to nearest power of 2
        ConfigSettingToggleControl pw2 = new();
        pw2.SettingName = "Resize Textures to Nearest Power of 2";
        bool bval3 = _config.GetS2TexPow2Enabled();
        pw2.SettingValue = bval3.ToString();
        pw2.ChangeButton.Click += S2TexPow2Enabled_OnClick;
        S2ConfigPanel.Children.Add(pw2);
    }

    private void PopulateUEConfigPanel()
    {
        UnrealConfigPanel.Children.Clear();

        // Unreal interop path
        ConfigSettingControl cui = new();
        cui.SettingName = "Unreal Content Path";
        string val = _config.GetUnrealInteropPath();
        cui.SettingValue = val == "" ? "Not set" : val;
        cui.ChangeButton.Click += UnrealInteropPath_OnClick;
        UnrealConfigPanel.Children.Add(cui);

        // Enable UE5 interop
        ConfigSettingToggleControl cii = new();
        cii.SettingName = "Generate Unreal Engine Importing Files";
        bool bval = _config.GetUnrealInteropEnabled();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropEnabled_OnClick;
        UnrealConfigPanel.Children.Add(cii);
    }

    private void Source2Path_OnClick(object sender, RoutedEventArgs e)
    {
        PopulateS2ConfigPanel();
    }

    private void S2ShaderExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2ShaderExportEnabled(!_config.GetS2ShaderExportEnabled());
        if (_config.GetS2ShaderExportEnabled())
        {
            _config.SetS2TexPow2Enabled(true);
        }
        PopulateS2ConfigPanel();
    }

    private void S2VMDLExportEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2VMDLExportEnabled(!_config.GetS2VMDLExportEnabled());
        PopulateS2ConfigPanel();
    }

    private void S2TexPow2Enabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetS2TexPow2Enabled(!_config.GetS2TexPow2Enabled());
        PopulateS2ConfigPanel();
    }

    private void UnrealInteropPath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenUnrealInteropPathDialog();
        PopulateUEConfigPanel();
    }

    public void OpenUnrealInteropPathDialog()
    {
        ShowUEMessage();
        return;
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            bool success = false;
            while (!success)
            {
                dialog.Description = "Select the folder where you want to import to unreal engine (eg Content folder)";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    success = _config.TrySetUnrealInteropPath(dialog.SelectedPath);
                }
                else
                {
                    return;
                }
            }
        }
    }

    private void UnrealInteropEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetUnrealInteropEnabled(false);
        ShowUEMessage();
        PopulateUEConfigPanel();
        return;

        if (_config.GetUnrealInteropPath() == "")
        {
            MessageBox.Show("Please set the path to the Unreal Engine content folder first.");
            return;
        }
        _config.SetUnrealInteropEnabled(!_config.GetUnrealInteropEnabled());
        PopulateUEConfigPanel();
    }

    private void ShowUEMessage()
    {
        MessageBox.Show("Unreal Engine importing is no longer supported at this current moment." +
            "\nAll the discoveries with maps and rendering ((skyboxes, lights, global channels, etc) and what not have made things a little complicated and I personally have little experience in scripting for UE." +
            "\n\nAnyone is welcome to contribute!", "Just a heads up.");
    }
}
