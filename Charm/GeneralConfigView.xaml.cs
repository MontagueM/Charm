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
using Orientation = System.Windows.Controls.Orientation;
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

        TextBlock header = new TextBlock();
        header.Text = "General Settings";
        header.FontSize = 30;
        GeneralConfigPanel.Children.Add(header);

        StackPanel sp = new();
        sp.Orientation = Orientation.Horizontal;
        GeneralConfigPanel.Children.Add(sp);

        // Packages path
        _packagePathStrategyComboBox = new ComboBox();
        _packagePathStrategyComboBox.ItemsSource = MakeEnumComboBoxItems<TigerStrategy>();
        _packagePathStrategyComboBox.SelectionChanged += PackagePathStrategyComboBox_OnSelectionChanged;
        if (_packagePathStrategy == TigerStrategy.NONE)
        {
            _packagePathStrategy = _config.GetCurrentStrategy();
        }

        _packagePathStrategyComboBox.SelectedIndex = (int)_packagePathStrategy;
        sp.Children.Add(_packagePathStrategyComboBox);

        ConfigSettingControl cpp = new ConfigSettingControl();
        cpp.SettingName = "Packages path";
        var val = _config.GetPackagesPath(_packagePathStrategy);
        cpp.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += PackagesPath_OnClick;
        cpp.Margin = new Thickness(10, 0, 0, 0);
        sp.Children.Add(cpp);

        // Save path
        ConfigSettingControl csp = new ConfigSettingControl();
        csp.SettingName = "Export save path";
        val = _config.GetExportSavePath();
        csp.SettingValue = val == "" ? "Not set" : val;
        csp.ChangeButton.Click += ExportSavePath_OnClick;
        GeneralConfigPanel.Children.Add(csp);

        // Enable Blender interop //force people to use the addon now >:)
        //ConfigSettingControl cbe = new ConfigSettingControl();
        //cbe.SettingName = "Generate Blender importing script";
        //bool bval2 = _config.GetBlenderInteropEnabled();
        //cbe.SettingValue = bval2.ToString();
        //cbe.ChangeButton.Click += BlenderInteropEnabled_OnClick;
        //GeneralConfigPanel.Children.Add(cbe);

        // Enable combined extraction folder for maps
        ConfigSettingControl cef = new ConfigSettingControl();
        cef.SettingName = "Use single folder extraction for maps";
        var bval = _config.GetSingleFolderMapsEnabled();
        cef.SettingValue = bval.ToString();
        cef.ChangeButton.Click += SingleFolderMapsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cef);

        // Enable individual static extraction with maps
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.SettingName = "Export individual static meshes with maps";
        bval = _config.GetIndvidualStaticsEnabled();
        cfe.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += IndvidualStaticsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cfe);

        // Output texture format
        ConfigSettingComboControl ctf = new ConfigSettingComboControl();
        ctf.SettingName = "Output texture format";
        TextureExportFormat etfval = _config.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TextureExportFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(ctf);

        TextBlock lbl = new TextBlock();
        lbl.Text = "(Use PNG or TGA in Blender)";
        lbl.FontSize = 15;
        GeneralConfigPanel.Children.Add(lbl);

        // Strategy
        ConfigSettingComboControl cs = new ConfigSettingComboControl();
        cs.SettingName = "Game version";
        TigerStrategy csval = _config.GetCurrentStrategy();
        cs.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems((TigerStrategy val) => Strategy.HasConfiguration(val));
        cs.SettingsCombobox.SelectedIndex = cs.SettingsCombobox.ItemsSource.Cast<ComboBoxItem>().ToList().FindIndex(x => (TigerStrategy)x.Tag == csval);
        cs.SettingsCombobox.SelectionChanged += CurrentStrategy_OnSelectionChanged;
        cs.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(cs);

    }

    private void PackagePathStrategyComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TigerStrategy strategy = (TigerStrategy)(_packagePathStrategyComboBox.SelectedItem as ComboBoxItem).Tag;
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
        TigerStrategy strategy = (TigerStrategy)_packagePathStrategyComboBox.SelectedIndex;
        OpenPackagesPathDialog(strategy);
        PopulateConfigPanel();
    }

    private void OpenPackagesPathDialog(TigerStrategy strategy)
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
                return;
            }

            if (!success)
            {
                MessageBox.Show("Directory selected is invalid, please select the correct packages directory.");
            }
            else
            {
                Strategy.AddNewStrategy(strategy, _config.GetPackagesPath(strategy));
            }
        }
    }

    private void ExportSavePath_OnClick(object sender, RoutedEventArgs e)
    {
        OpenExportSavePathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }

    private void ConsiderShowingMainMenu()
    {
        if (_config.GetPackagesPath(Strategy.CurrentStrategy) != "" && _config.GetExportSavePath() != "")
        {
            var _mainWindow = Window.GetWindow(this) as MainWindow;
            _mainWindow.ShowMainMenu();
        }
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
            Log.Warning($"Strategy {strategy} has no configuration so cannot select it.");
        }
        _config.SetCurrentStrategy(strategy);
        Strategy.SetStrategy(_config.GetCurrentStrategy());
        PopulateConfigPanel();
    }
}
