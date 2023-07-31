using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace CharmAvalonia;

public partial class ConfigSettingControl : UserControl
{
    public readonly ConfigSettingControlViewModel ViewModel;
    public ConfigSettingControl()
    {
        InitializeComponent();

        ViewModel = new ConfigSettingControlViewModel();
        DataContext = ViewModel;
    }
}

public class ConfigSettingControlViewModel : ReactiveObject
{
    private string _settingName;
    public string SettingName
    {
        get
        {
            return _settingName;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _settingName, value);
        }
    }

    private string _settingValue;
    public string SettingValue
    {
        get
        {
            return _settingValue;
        }
        set
        {
            this.RaiseAndSetIfChanged(ref _settingValue, value);
        }
    }
}
