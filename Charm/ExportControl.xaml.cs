using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public enum ExportTypeFlag // This isn't really needed anymore
{
    [Description("Everything")]
    Full = 1,
    [Description("Minimal (no terrain)")]
    Minimal = 2,
    //[Description("Terrain only")]
    //TerrainOnly = 4,
    //[Description("Pre-arranged map")]
    //ArrangedMap = 8,
}

public partial class ExportControl : UserControl
{
    private bool _bExportFunctionSet = false;
    private Action<ExportInfo> _routedFunction = null;
    private bool _disableLoadingBar = false;

    public ExportControl()
    {
        InitializeComponent();
        DisabledOverlay.Visibility = Visibility.Visible;
    }

    // private void SetExportName(string name)
    // {
    //     ExportName.Text = $"Exporting: {name}";
    // }

    public void SetExportFunction(Action<ExportInfo> function, int exportTypeFlags, bool disableLoadingBar = false)
    {
        _disableLoadingBar = disableLoadingBar;
        if (_bExportFunctionSet)
            return;
        _routedFunction = function;
        ExportButton.Click += ExportFunction;
        _bExportFunctionSet = true;
        SetExportTypes(exportTypeFlags);
    }

    private void SetExportTypes(int exportTypeFlags)
    {
        ExportComboBox.Items.Clear();
        var values = Enum.GetValues(typeof(ExportTypeFlag)).Cast<ExportTypeFlag>().ToList();
        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (((int)value & exportTypeFlags) == (int)value)
            {
                string name = TagItem.GetEnumDescription(value);
                ExportComboBox.Items.Add(new ComboBoxItem
                {
                    Content = name,
                    IsSelected = i == 0,
                    DataContext = value
                });
            }
        }
    }

    private ExportTypeFlag GetSelectedExportType()
    {
        return (ExportTypeFlag)(ExportComboBox.SelectedItem as ComboBoxItem).DataContext;
    }

    public async void ExportFunction(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        ExportInfo info = (ExportInfo)btn.Tag;
        info.ExportType = GetSelectedExportType();
        if (!_disableLoadingBar)
        {
            MainWindow.Progress.SetProgressStages(new List<string>
            {
                $"Exporting {info.Name} {info.Hash}"
            });
        }
        await Task.Run(() =>
        {
            RoutedFunction(info);
        });
        if (!_disableLoadingBar)
        {
            MainWindow.Progress.CompleteStage();
        }
    }

    public void RoutedFunction(ExportInfo info)
    {
        _routedFunction(info);
    }

    public void SetExportInfo(string name, TigerHash hash)
    {
        if (_bExportFunctionSet && DisabledOverlay.Visibility == Visibility.Visible)
            DisabledOverlay.Visibility = Visibility.Hidden;
        ExportInfo info = new() { Name = name, Hash = hash };
        // SetExportName(name);
        ExportButton.Tag = info;
    }

    public void SetExportInfo(TigerHash hash)
    {
        SetExportInfo(hash, hash);
    }
}

public struct ExportInfo
{
    private string _name = String.Empty;

    public string Name
    {
        get => _name == String.Empty ? Hash : _name;
        set => _name = value;
    }
    public TigerHash Hash;
    public ExportTypeFlag ExportType = ExportTypeFlag.Full;

    public ExportInfo()
    {
        Hash = new TigerHash();
    }
}
