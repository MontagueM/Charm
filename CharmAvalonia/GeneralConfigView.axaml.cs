using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Arithmic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Tiger;
using Tiger.Schema;
using Window = Avalonia.Controls.Window;

namespace CharmAvalonia;

public partial class GeneralConfigView : UserControl
{
    public GeneralConfigView()
    {
        InitializeComponent();

        _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    }

    public void OnControlLoaded(object? sender, VisualTreeAttachmentEventArgs visualTreeAttachmentEventArgs)
    {
        PopulateConfigPanel();
    }

    private ComboBox _packagePathStrategyComboBox;
    private ConfigSubsystem _config;
    private static TigerStrategy _packagePathStrategy = TigerStrategy.NONE;

    private void PopulateConfigPanel()
    {
        if (GeneralConfigPanel.Children.Count > 0)
        {
            GeneralConfigPanel.Children.Clear();
        }
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
        cpp.ViewModel.SettingName = "Packages path";
        var val = _config.GetPackagesPath(_packagePathStrategy);
        cpp.ViewModel.SettingValue = val == "" ? "Not set" : val;
        cpp.ChangeButton.Click += PackagesPath_OnClick;
        sp.Children.Add(cpp);

        // Save path
        ConfigSettingControl csp = new ConfigSettingControl();
        csp.ViewModel.SettingName = "Export save path";
        val = _config.GetExportSavePath();
        csp.ViewModel.SettingValue = val == "" ? "Not set" : val;
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
        cef.ViewModel.SettingName = "Use single folder extraction for maps";
        var bval = _config.GetSingleFolderMapsEnabled();
        cef.ViewModel.SettingValue = bval.ToString();
        cef.ChangeButton.Click += SingleFolderMapsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cef);

        // Enable individual static extraction with maps
        ConfigSettingControl cfe = new ConfigSettingControl();
        cfe.ViewModel.SettingName = "Export individual static meshes with maps";
        bval = _config.GetIndvidualStaticsEnabled();
        cfe.ViewModel.SettingValue = bval.ToString();
        cfe.ChangeButton.Click += IndvidualStaticsEnabled_OnClick;
        GeneralConfigPanel.Children.Add(cfe);

        // Output texture format
        ConfigSettingComboControl ctf = new ConfigSettingComboControl();
        ctf.ViewModel.SettingName = "Output texture format";
        TextureExportFormat etfval = _config.GetOutputTextureFormat();
        ctf.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems<TextureExportFormat>();
        ctf.SettingsCombobox.SelectedIndex = (int)etfval;
        ctf.SettingsCombobox.SelectionChanged += OutputTextureFormat_OnSelectionChanged;
        ctf.ChangeButton.IsVisible = false;
        GeneralConfigPanel.Children.Add(ctf);

        TextBlock lbl = new TextBlock();
        lbl.Text = "(Use PNG or TGA in Blender)";
        lbl.FontSize = 15;
        GeneralConfigPanel.Children.Add(lbl);

        // Strategy
        ConfigSettingComboControl cs = new ConfigSettingComboControl();
        cs.ViewModel.SettingName = "Game version";
        TigerStrategy csval = _config.GetCurrentStrategy();
        var x = MakeEnumComboBoxItems((TigerStrategy val) => Strategy.HasConfiguration(val));
        cs.SettingsCombobox.ItemsSource = MakeEnumComboBoxItems((TigerStrategy val) => Strategy.HasConfiguration(val));
        cs.SettingsCombobox.SelectedIndex = cs.SettingsCombobox.ItemsSource.Cast<ComboBoxItem>().ToList().FindIndex(x => (TigerStrategy)x.Tag == csval);
        cs.SettingsCombobox.SelectionChanged += CurrentStrategy_OnSelectionChanged;
        cs.ChangeButton.IsVisible = false;
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

    private ObservableCollection<ComboBoxItem> MakeEnumComboBoxItems<T>() where T : Enum
    {
        return MakeEnumComboBoxItems<T>((T val) => true);
    }

    public static string GetEnumDescription(Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        if (attributes != null && attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }

    private ObservableCollection<ComboBoxItem> MakeEnumComboBoxItems<T>(Func<T, bool> filterAction) where T : Enum
    {
        ObservableCollection<ComboBoxItem> items = new ObservableCollection<ComboBoxItem>();
        foreach (T val in Enum.GetValues(typeof(T)))
        {
            if (filterAction(val))
            {
                items.Add(new ComboBoxItem { Content = GetEnumDescription(val), Tag = val });
            }
        }
        return items;
    }

    private void ConsiderShowingMainMenu()
    {
        if (_config.GetPackagesPath(Strategy.CurrentStrategy) != "" && _config.GetExportSavePath() != "")
        {
            var _mainWindow = Window.GetTopLevel(this) as MainWindow;
            _mainWindow.ShowMainMenu();
        }
    }

    private async void PackagesPath_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs routedEventArgs)
    {
        TigerStrategy strategy = (TigerStrategy)_packagePathStrategyComboBox.SelectedIndex;
        await OpenPackagesPathDialog(strategy);
        PopulateConfigPanel();

        ConsiderShowingMainMenu();
    }

    private async Task OpenPackagesPathDialog(TigerStrategy strategy)
    {
        IReadOnlyList<IStorageFolder> folders = await (TopLevel.GetTopLevel(this) as Window)?.StorageProvider.OpenFolderPickerAsync(new(){AllowMultiple = false, Title = "Select the folder where your packages for the relevant version (*.pkg) are located"});
        if (folders.Count > 0)
        {
            IStorageFolder folder = folders[0];
            bool success = _config.TrySetPackagePath(folder.Path.AbsolutePath, strategy);

            if (success)
            {
                Strategy.AddNewStrategy(strategy, _config.GetPackagesPath(strategy));
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Failed to set package path", "Directory selected is invalid, please select the correct packages directory.", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }
    }

    private async void ExportSavePath_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs routedEventArgs)
    {
        await OpenExportSavePathDialog();
        PopulateConfigPanel();
        ConsiderShowingMainMenu();
    }

    private async Task OpenExportSavePathDialog()
    {
        // using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        // {
        //     dialog.Description = "Select the folder to export to";
        //     bool success = false;
        //     while (!success)
        //     {
        //         DialogResult result = dialog.ShowDialog();
        //         if (result is DialogResult.OK)
        //         {
        //             string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //             if (dialog.SelectedPath.Contains(exeDirectory + "\\") || dialog.SelectedPath == exeDirectory)
        //             {
        //                 MessageBox.Show("You cannot export to the same directory as the executable.");
        //                 continue;
        //             }
        //
        //             success = _config.TrySetExportSavePath(dialog.SelectedPath);
        //         }
        //         else if (result is DialogResult.Cancel or DialogResult.Abort)
        //         {
        //             return;
        //         }
        //
        //         if (!success)
        //         {
        //             MessageBox.Show("Directory selected is invalid, please select the correct directory.");
        //         }
        //     }
        // }
        IReadOnlyList<IStorageFolder> folders = await (TopLevel.GetTopLevel(this) as Window)?.StorageProvider.OpenFolderPickerAsync(new(){AllowMultiple = false, Title = "Select the folder to export to"});
        if (folders.Count > 0)
        {
            IStorageFolder folder = folders[0];
            string path = folder.Path.AbsolutePath;

            string exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (path.Contains(exeDirectory + "\\") || path == exeDirectory)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Failed to set export path", "You cannot export to the same directory as the executable.", ButtonEnum.Ok);
                await box.ShowAsync();
            }

            bool success = _config.TrySetExportSavePath(path);

            if (!success)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Failed to set export path", "Directory selected is invalid, please select the correct directory.", ButtonEnum.Ok);
                await box.ShowAsync();
            }
        }
    }

    private void BlenderInteropEnabled_OnClick(object sender, RoutedEventArgs e)
    {
        if (!_config.GetBlenderInteropEnabled())
        {
            // MessageBox.Show("Blender will NOT import shaders. Please have moderate Blender shader knowledge.");
        }
        _config.SetBlenderInteropEnabled(!_config.GetBlenderInteropEnabled());
        PopulateConfigPanel();
    }

    private void SingleFolderMapsEnabled_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs routedEventArgs)
    {
        _config.SetSingleFolderMapsEnabled(!_config.GetIndvidualStaticsEnabled());
        PopulateConfigPanel();
    }

    private void IndvidualStaticsEnabled_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs routedEventArgs)
    {
        _config.SetIndvidualStaticsEnabled(!_config.GetIndvidualStaticsEnabled());
        PopulateConfigPanel();
    }

    private void OutputTextureFormat_OnSelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        var index = (selectionChangedEventArgs.Source as ComboBox).SelectedIndex;
        _config.SetOutputTextureFormat((TextureExportFormat)index);
        TextureExtractor.SetTextureFormat(_config.GetOutputTextureFormat());
        PopulateConfigPanel();
    }

    private void CurrentStrategy_OnSelectionChanged(object? sender, SelectionChangedEventArgs selectionChangedEventArgs)
    {
        TigerStrategy strategy = (TigerStrategy)(selectionChangedEventArgs.AddedItems[0] as ComboBoxItem).Tag;
        if (!Strategy.HasConfiguration(strategy))
        {
            Log.Warning($"Strategy {strategy} has no configuration so cannot select it.");
        }
        _config.SetCurrentStrategy(strategy);
        Strategy.SetStrategy(_config.GetCurrentStrategy());
        PopulateConfigPanel();
    }
}

