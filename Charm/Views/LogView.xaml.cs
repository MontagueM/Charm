using System;
using System.IO;
using System.Windows.Controls;
using Arithmic;
using Tiger;

namespace Charm.Views;

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

