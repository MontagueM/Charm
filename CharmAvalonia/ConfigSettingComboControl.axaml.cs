using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace CharmAvalonia;

public partial class ConfigSettingComboControl : UserControl
{
    public readonly ConfigSettingComboControlViewModel ViewModel;

    public ConfigSettingComboControl()
    {
        InitializeComponent();

        ViewModel = new ConfigSettingComboControlViewModel();
        DataContext = ViewModel;
    }
}

public class ConfigSettingComboControlViewModel : ReactiveObject
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
}

