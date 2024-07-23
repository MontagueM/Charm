using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Arithmic;
using Tiger;
using Tiger.Schema;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace Charm;

public partial class GeneralConfigView : UserControl
{
    public GeneralConfigView()
    {
        InitializeComponent();
        _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    }

    public void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        PopulateConfigPanel();
    }

    private ComboBox _packagePathStrategyComboBox;
    private ConfigSubsystem _config;
    private static TigerStrategy _packagePathStrategy = TigerStrategy.NONE;

    private void PopulateConfigPanel()
    {
        GeneralConfigPanel.Children.Clear();

        // Strategy
        ConfigSettingComboControl cs = new ConfigSettingComboControl();
        cs.SettingName = "Game Version";
        TigerStrategy csval = _config.GetCurrentStrategy();
        cs.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TigerStrategy>(); //MakeEnumComboBoxItems((TigerStrategy val) => Strategy.HasConfiguration(val));
        cs.SettingsCombobox.SelectedIndex = cs.SettingsCombobox.ItemsSource.Cast<ComboBoxItem>().ToList().FindIndex(x => (TigerStrategy)x.Tag == csval);
        if (cs.SettingsCombobox.SelectedIndex == -1)
        {
            cs.SettingsCombobox.SelectedIndex = 0;
        }
        cs.SettingsCombobox.SelectionChanged += PackagePathStrategyComboBox_OnSelectionChanged;
        cs.SettingsCombobox.SelectionChanged += CurrentStrategy_OnSelectionChanged;
        cs.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(cs);

        // Packages path
        if (_packagePathStrategy == TigerStrategy.NONE)
        {
            _packagePathStrategy = _config.GetCurrentStrategy();
        }

        ConfigSettingControl cpp = new ConfigSettingControl();
        // cpp.Settings.Children.Add(_packagePathStrategyComboBox);
        cpp.SettingName = "Packages Path";
        if (_packagePathStrategy == TigerStrategy.NONE)
        {
            cpp.SettingValue = "Cannot set packages path without a version selected";
            cpp.ChangeButton.IsEnabled = false;
        }
        else
        {
            var packagesPath = _config.GetPackagesPath(_packagePathStrategy);
            cpp.SettingValue = packagesPath == "" ? "Not Set (Required)" : packagesPath;
            cpp.ChangeButton.Click += PackagesPath_OnClick;
        }
        GeneralConfigPanel.Children.Add(cpp);


        // Save path
        ConfigSettingControl csp = new ConfigSettingControl();
        csp.SettingName = "Export Save Path";
        string exportSavePath = _config.GetExportSavePath();
        csp.SettingValue = exportSavePath == "" ? "Not Set (Required)" : exportSavePath;
        csp.ChangeButton.Click += ExportSavePath_OnClick;
        GeneralConfigPanel.Children.Add(csp);

        // Enable combined extraction folder for maps
        ConfigSettingToggleControl cef = new ConfigSettingToggleControl();
        cef.SettingName = "Single Folder Extraction For Maps";
        var bval = _config.GetSingleFolderMapsEnabled();
        cef.SettingValue = bval.ToString();
        cef.ChangeButton.Click += SingleFolderMapsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cef);

        // Enable individual static extraction with maps
        ConfigSettingToggleControl cfe = new ConfigSettingToggleControl();
        cfe.SettingName = "Export Individual Models With Maps";
        bval = _config.GetIndvidualStaticsEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += IndvidualStaticsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cfe);

        // Disabled because it just doesn't work atm
        _config.SetUseCustomRenderer(false);
        //ConfigSettingControl cucr = new ConfigSettingControl();
        //cucr.SettingName = "Use custom renderer";
        //bval = _config.GetUseCustomRenderer();
        //cucr.SettingValue = bval.ToString();
        //cucr.ChangeButton.Click += UseCustomRenderer_OnClick;
        //GeneralConfigPanel.Children.Add(cucr);

        // Output texture format
        ConfigSettingComboControl ctf = new ConfigSettingComboControl();
        ctf.SettingName = "Output Texture Format";
        ctf.SettingLabel = "(Use PNG or TGA in Blender)";
        TextureExportFormat etfval = _config.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TextureExportFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(ctf);

    }

    private void PackagePathStrategyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TigerStrategy strategy = (TigerStrategy)(((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedItem as ComboBoxItem).Tag;
        if (_packagePathStrategy != strategy)
        {
            _packagePathStrategy = strategy;
            PopulateConfigPanel();
        }
    }

    private List<ComboBoxItem> MakeEnumComboBoxItems<T>() where T : Enum
    {
        return MakeEnumComboBoxItems<T>((T val) => true);
    }

    private List<ComboBoxItem> MakeEnumComboBoxItems<T>(Func<T, bool> filterAction) where T : Enum
    {
        List<ComboBoxItem> items = new List<ComboBoxItem>();
        foreach (T val in Enum.GetValues(typeof(T)))
        {
            if (filterAction(val))
            {
                items.Add(new ComboBoxItem { Content = TagItem.GetEnumDescription(val), Tag = val });
            }
        }
        return items;
    }

    private void PackagesPath_OnClick(object sender, RoutedEventArgs e)
    {
        //TigerStrategy strategy = (TigerStrategy)(_packagePathStrategyComboBox.SelectedItem as ComboBoxItem).Tag;
        OpenPackagesPathDialog(_packagePathStrategy);
        PopulateConfigPanel();
    }

    private bool OpenPackagesPathDialog(TigerStrategy strategy)
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            // todo customise this per strategy?
            dialog.Description = "Select the folder where your packages for the relevant version (*.pkg) are located";
            bool success = false;
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                success = _config.TrySetPackagePath(dialog.SelectedPath, strategy);
            }
            else
            {
                return false;
            }

            if (!success)
            {
                MessageBox.Show("Directory selected is invalid, please select the correct packages directory.");
                return false;
            }
            else
            {
                return Strategy.AddNewStrategy(strategy, _config.GetPackagesPath(strategy));
            }
        }
    }

    private void ExportSavePath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenExportSavePathDialog();
        PopulateConfigPanel();
        if (ConsiderShowingMainMenu())
        {
            var _mainWindow = Window.GetWindow(this) as MainWindow;
            _mainWindow.SetCurrentTab("MAIN MENU");
        }

    }

    private bool ConsiderShowingMainMenu()
    {
        if (_config.GetPackagesPath(Strategy.CurrentStrategy) != "" && _config.GetExportSavePath() != "")
        {
            var _mainWindow = Window.GetWindow(this) as MainWindow;
            if (_mainWindow.MainMenuTab.Visibility == Visibility.Visible) // already showing
                return false;

            _mainWindow.ShowMainMenu();
            return true;
        }
        return false;
    }

    private void OpenExportSavePathDialog()
    {
        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        {
            dialog.Description = "Select the folder to export to";
            bool success = false;
            while (!success)
            {
                DialogResult result = dialog.ShowDialog();
                if (result is DialogResult.OK)
                {
                    string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    if (dialog.SelectedPath.Contains(exeDirectory + "\\") || dialog.SelectedPath == exeDirectory)
                    {
                        MessageBox.Show("You cannot export to the same directory as the executable.");
                        continue;
                    }

                    success = _config.TrySetExportSavePath(dialog.SelectedPath);
                }
                else if (result is DialogResult.Cancel or DialogResult.Abort)
                {
                    return;
                }

                if (!success)
                {
                    MessageBox.Show("Directory selected is invalid, please select the correct directory.");
                }
            }
        }
    }

    private void BlenderInteropEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_config.GetBlenderInteropEnabled())
        {
            MessageBox.Show("Blender will NOT import shaders. Please have moderate Blender shader knowledge.");
        }
        _config.SetBlenderInteropEnabled(!_config.GetBlenderInteropEnabled());
        PopulateConfigPanel();
    }

    private void SingleFolderMapsEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSingleFolderMapsEnabled(!_config.GetSingleFolderMapsEnabled());
        PopulateConfigPanel();
    }

    private void IndvidualStaticsEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetIndvidualStaticsEnabled(!_config.GetIndvidualStaticsEnabled());
        PopulateConfigPanel();
    }

    private void OutputTextureFormat_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var index = ((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedIndex;
        _config.SetOutputTextureFormat((TextureExportFormat)index);
        TextureExtractor.SetTextureFormat(_config.GetOutputTextureFormat());
        PopulateConfigPanel();
    }

    private void CurrentStrategy_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        TigerStrategy strategy = (TigerStrategy)(((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedItem as ComboBoxItem).Tag;
        if (!Strategy.HasConfiguration(strategy))
        {
            Log.Warning($"Strategy {strategy} has no configuration set.");
            if (!OpenPackagesPathDialog(strategy))
            {
                Strategy.SetStrategy(TigerStrategy.NONE);
                PopulateConfigPanel();
                return;
            }
        }
        _config.SetCurrentStrategy(strategy);
        Strategy.SetStrategy(_config.GetCurrentStrategy());
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }

    private void UseCustomRenderer_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetUseCustomRenderer(!_config.GetUseCustomRenderer());
        PopulateConfigPanel();
    }
}
