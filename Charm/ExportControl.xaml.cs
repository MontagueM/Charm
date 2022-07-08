using System.Windows;
using System.Windows.Controls;
using Field.General;

namespace Charm;

public partial class ExportView : UserControl
{
    private bool _bExportFunctionSet = false;
    
    public ExportView()
    {
        InitializeComponent();
        DisabledOverlay.Visibility = Visibility.Visible;
    }

    private void SetExportName(string name)
    {
        ExportName.Text = $"Exporting: {name}";
    }

    public void SetExportFunction(RoutedEventHandler function)
    {
        if (_bExportFunctionSet) 
            return;
        ExportButton.Click += function;
        _bExportFunctionSet = true;
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