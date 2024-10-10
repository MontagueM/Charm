using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class UnrealConfigView : UserControl
{
    public UnrealConfigView()
    {
        InitializeComponent();
        _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    }

    private ConfigSubsystem _config;


    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private void PopulateConfigPanel()
    {
        UnrealConfigPanel.Children.Clear();

        //TextBlock header = new TextBlock();
        //header.Text = "Unreal Engine Settings";
        //header.FontSize = 30;
        //UnrealConfigPanel.Children.Add(header);

        // Unreal interop path
        ConfigSettingControl cui = new ConfigSettingControl();
        cui.SettingName = "Unreal Content Path";
        var val = _config.GetUnrealInteropPath();
        cui.SettingValue = val == "" ? "Not set" : val;
        cui.ChangeButton.Click += UnrealInteropPath_OnClick;
        UnrealConfigPanel.Children.Add(cui);

        // Enable UE5 interop
        ConfigSettingToggleControl cii = new ConfigSettingToggleControl();
        cii.SettingName = "Generate Unreal Engine Importing Files";
        bool bval = _config.GetUnrealInteropEnabled();
        cii.SettingValue = bval.ToString();
        cii.ChangeButton.Click += UnrealInteropEnabled_OnClick;
        UnrealConfigPanel.Children.Add(cii);
    }

    private void UnrealInteropPath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenUnrealInteropPathDialog();
        PopulateConfigPanel();
    }

    public void OpenUnrealInteropPathDialog()
    {
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
        MessageBox.Show("Unreal Engine exporting is currently broken.\nNo ETA on a fix, I'm sorry.");
        PopulateConfigPanel();
        return;

        if (_config.GetUnrealInteropPath() == "")
        {
            MessageBox.Show("Please set the path to the Unreal Engine content folder first.");
            return;
        }
        _config.SetUnrealInteropEnabled(!_config.GetUnrealInteropEnabled());
        PopulateConfigPanel();
    }
}
