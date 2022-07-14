using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Field.General;

namespace Charm;

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

    private void SetExportName(string name)
    {
        ExportName.Text = $"Exporting: {name}";
    }

    public void SetExportFunction(Action<ExportInfo> function, bool disableLoadingBar=false)
    {
        _disableLoadingBar = disableLoadingBar;
        if (_bExportFunctionSet) 
            return;
        _routedFunction = function;
        ExportButton.Click += ExportFunction;
        _bExportFunctionSet = true;
    }

    public async void ExportFunction(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        ExportInfo info = (ExportInfo)btn.Tag;
        if (!_disableLoadingBar)
        {
            MainWindow.Progress.SetProgressStages(new List<string>
            {
                $"exporting {info.Name} {info.Hash}"
            });
        }
        await Task.Run(() =>
        {
            _routedFunction(info);
        });
        if (!_disableLoadingBar)
        {
            MainWindow.Progress.CompleteStage();
        }
    }
    
    public void SetExportInfo(string name, DestinyHash hash)
    {
        if (_bExportFunctionSet && DisabledOverlay.Visibility == Visibility.Visible )
            DisabledOverlay.Visibility = Visibility.Hidden;
        ExportInfo info = new ExportInfo {Name = name, Hash = hash};
        SetExportName(name);
        ExportButton.Tag = info;
    }
    
    public void SetExportInfo(DestinyHash hash)
    {
        SetExportInfo(hash, hash);
    }
}

public struct ExportInfo
{
    public string Name;
    public DestinyHash Hash;
}