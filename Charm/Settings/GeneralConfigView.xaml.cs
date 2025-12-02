using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        _config = TigerInstance.GetSubsystem<ConfigSubsystem>();
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
        bool bVal;

        #region General
        // ---- General settings panel ----
        GeneralConfigPanel.Children.Clear();

        // Strategy
        ConfigSettingComboControl cs = new();
        cs.SettingName = "Game Version";
        TigerStrategy csval = _config.GetCurrentStrategy();
        cs.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TigerStrategy>(); //MakeEnumComboBoxItems((TigerStrategy val) => Strategy.HasConfiguration(val));
        cs.SettingsCombobox.SelectedIndex = cs.SettingsCombobox.ItemsSource.Cast<ComboBoxItem>().ToList().FindIndex(x => (TigerStrategy)x.Tag == csval);
        if (cs.SettingsCombobox.SelectedIndex == -1)
        {
            cs.SettingsCombobox.SelectedIndex = 0;
        }
        cs.SettingsCombobox.SelectionChanged += CurrentStrategy_OnSelectionChanged;

        cs.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(cs);

        // Packages path
        if (_packagePathStrategy == TigerStrategy.NONE)
        {
            _packagePathStrategy = _config.GetCurrentStrategy();
        }

        ConfigSettingControl cpp = new();
        // cpp.Settings.Children.Add(_packagePathStrategyComboBox);
        cpp.SettingName = "Packages Path";
        if (_packagePathStrategy == TigerStrategy.NONE)
        {
            cpp.SettingValue = "Cannot set packages path without a version selected";
            cpp.ChangeButton.IsEnabled = false;
        }
        else
        {
            string packagesPath = _config.GetPackagesPath(_packagePathStrategy);
            cpp.SettingValue = packagesPath == "" ? "Not Set (Required)" : packagesPath;
            cpp.ChangeButton.Click += PackagesPath_OnClick;
        }
        GeneralConfigPanel.Children.Add(cpp);


        // Save path
        ConfigSettingControl csp = new();
        csp.SettingName = "Export Save Path";
        string exportSavePath = _config.GetExportSavePath();
        csp.SettingValue = exportSavePath == "" ? "Not Set (Required)" : exportSavePath;
        csp.ChangeButton.Click += ExportSavePath_OnClick;
        GeneralConfigPanel.Children.Add(csp);

        // Output texture format
        ConfigSettingComboControl ctf = new();
        ctf.SettingName = "Output Texture Format";
        ctf.SettingLabel = "(Use PNG or TGA in Blender)";
        TextureExportFormat etfval = _config.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TextureExportFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.Visibility = Visibility.Hidden;
        GeneralConfigPanel.Children.Add(ctf);
        #endregion

        #region Materials
        // ---- Material settings panel ----
        MaterialsConfigPanel.Children.Clear();

        // Whether to export shader hlsl files, always true if S&Box shaders are enabled
        ConfigSettingToggleControl hlsl = new();
        hlsl.SettingName = "Export Shader HLSL";
        hlsl.SettingLabel = "Save shader hlsl code, can slow down larger exports such as maps.";
        bVal = _config.GetSaveShaderHLSL();
        hlsl.SettingValue = bVal.ToString();
        hlsl.ChangeButton.Click += SaveShaderHLSL_OnClick;
        MaterialsConfigPanel.Children.Add(hlsl);
        #endregion

        #region Misc
        // ---- Misc settings panel ----
        MiscConfigPanel.Children.Clear();

        // Store all exported map assets in a single "Maps/Assets/" folder  
        // instead of "ExportPath/(MapName)/".
        ConfigSettingToggleControl cfe = new();
        cfe.SettingName = "Unified Map Asset Exports";
        cfe.SettingLabel = "Export all map assets to a single \"Maps/Assets/\" folder.";
        bVal = _config.GetSingleFolderMapAssetsEnabled();
        cfe.SettingValue = bVal.ToString();
        cfe.ChangeButton.Click += SingleFolderMapAssetsEnabled_OnClick;
        MiscConfigPanel.Children.Add(cfe);


        ConfigSettingToggleControl disBg = new();
        disBg.SettingName = "Animated Background";
        disBg.SettingLabel = "Requires a restart to take effect.";
        bVal = _config.GetAnimatedBackground();
        disBg.SettingValue = bVal.ToString();
        disBg.ChangeButton.Click += AnimatedBackground_OnClick;
        MiscConfigPanel.Children.Add(disBg);

        ConfigSettingToggleControl disME = new();
        disME.SettingName = "Motion Effects";
        disME.SettingLabel = "Enables a fake parallax effect when moving the mouse in menus.";
        bVal = _config.GetMotionEffects();
        disME.SettingValue = bVal.ToString();
        disME.ChangeButton.Click += MotionEffects_OnClick;
        MiscConfigPanel.Children.Add(disME);

        ConfigSettingToggleControl disHL = new();
        disHL.SettingName = "Holofoil Effect";
        disHL.SettingLabel = "Enables the Holofoil effect on weapon icons in the API View";
        bVal = _config.GetHolofoilShader();
        disHL.SettingValue = bVal.ToString();
        disHL.ChangeButton.Click += HolofoilShader_OnClick;
        MiscConfigPanel.Children.Add(disHL);
        #endregion
    }

    private List<ComboBoxItem> MakeEnumComboBoxItems<T>() where T : Enum
    {
        return MakeEnumComboBoxItems<T>((T val) => true);
    }

    private List<ComboBoxItem> MakeEnumComboBoxItems<T>(Func<T, bool> filterAction) where T : Enum
    {
        List<ComboBoxItem> items = new();
        foreach (T val in Enum.GetValues(typeof(T)))
        {
            if (filterAction(val))
            {
                items.Add(new ComboBoxItem { Content = EnumExtensions.GetEnumDescription(val).ToUpper(), Tag = val });
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
                    if (dialog.SelectedPath.Contains("."))
                    {
                        MessageBox.Show("Export path can not contain a period, this currently breaks texture exporting.");
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

    private void SingleFolderMapAssetsEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSingleFolderMapAssetsEnabled(!_config.GetSingleFolderMapAssetsEnabled());
        PopulateConfigPanel();
    }

    private void OutputTextureFormat_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        int index = ((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedIndex;
        _config.SetOutputTextureFormat((TextureExportFormat)index);
        TextureExtractor.SetTextureFormat(_config.GetOutputTextureFormat());
        PopulateConfigPanel();
    }

    // This is a mess
    private async void CurrentStrategy_OnSelectionChanged(object sender, RoutedEventArgs e)
    {
        var prevStrat = Strategy.CurrentStrategy;
        TigerStrategy targetStrategy = (TigerStrategy)(((sender as ComboBox).DataContext as ConfigSettingComboControl).SettingsCombobox.SelectedItem as ComboBoxItem).Tag;

        MainWindow.Progress.SetProgressStage(
            $"Changing from {prevStrat.GetEnumDescription()} to {targetStrategy.GetEnumDescription()}");

        // dumb but allows with progress view to show up without wrapping shit in a task.run, which causes more headaches
        await Task.Delay(100);

        bool hasConfig = Strategy.HasConfiguration(targetStrategy);
        if (!hasConfig)
        {
            Log.Warning($"Strategy {targetStrategy} has no configuration set.");
            Strategy.SetStrategy(TigerStrategy.NONE);

            bool result = OpenPackagesPathDialog(targetStrategy);
            if (!result)
            {
                MainWindow.Progress.CompleteStage();
                SetStrategy(prevStrat);
                return;
            }
        }

        MainWindow.Progress.CompleteStage();
        SetStrategy(targetStrategy);
        ConsiderShowingMainMenu();
    }

    private void SetStrategy(TigerStrategy targetStrategy)
    {
        _packagePathStrategy = targetStrategy;
        _config.SetCurrentStrategy(targetStrategy);
        Strategy.SetStrategy(targetStrategy);

        PopulateConfigPanel();
    }

    private void AnimatedBackground_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetAnimatedBackground(!_config.GetAnimatedBackground());
        PopulateConfigPanel();
    }

    private void MotionEffects_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetMotionEffects(!_config.GetMotionEffects());
        PopulateConfigPanel();
    }

    private void HolofoilShader_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetHolofoilShader(!_config.GetHolofoilShader());
        PopulateConfigPanel();
    }

    private void SaveShaderHLSL_OnClick(object sender, RoutedEventArgs e)
    {
        _config.SetSaveShaderHLSL(!_config.GetSaveShaderHLSL());
        PopulateConfigPanel();
    }
}

public class ConfigViewSectionData
{
    public string Icon { get; set; }
    public string Title { get; set; }
}
