using System;
using System.IO;
using Arithmic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tiger;

namespace Charm.Views.Misc;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();

        Log.BindDelegate(OnLogEvent);
    }

    private void OnLogEvent(object sender, LogEventArgs e)
    {
        LogBox.Text += e.Message + Environment.NewLine;
    }
}

