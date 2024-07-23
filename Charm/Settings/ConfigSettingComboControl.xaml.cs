using System.Windows.Controls;

namespace Charm;

public partial class ConfigSettingComboControl : UserControl
{
    public ConfigSettingComboControl()
    {
        InitializeComponent();
        DataContext = this;
    }

    public string SettingName { get; set; }
    public string SettingLabel { get; set; }
}
