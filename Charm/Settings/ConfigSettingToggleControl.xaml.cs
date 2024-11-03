using System.Windows.Controls;

namespace Charm;

public partial class ConfigSettingToggleControl : UserControl
{
    public ConfigSettingToggleControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SettingName { get; set; }
    public string SettingValue { get; set; }
    public string SettingLabel { get; set; }
}
