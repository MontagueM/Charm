using System.Windows.Controls;

namespace Charm;

public partial class ConfigSettingControl : UserControl
{
    public ConfigSettingControl()
    {
        InitializeComponent();
        DataContext = this;
    }
    
    public string SettingName { get; set; }
    
    public string SettingValue { get; set; }
}