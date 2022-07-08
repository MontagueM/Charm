using System;
using System.IO;
using System.Windows.Controls;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.RichTextBox.Themes;
using Serilog.Templates;

namespace Charm;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();
    }
}

public class LogHandler
{
    public static void Initialise(LogView logView)
    {
        InitLogger(logView);
    }

    private static void InitLogger(LogView logView)
    {
        string outputTemplate = "{Timestamp:HH:mm:ss} [{Level}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        if (File.Exists("charm.log"))
            File.Delete("charm.log");
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("charm.log", outputTemplate: outputTemplate)
            .WriteTo.RichTextBox(logView.LogBox,
                theme: RichTextBoxConsoleTheme.Colored,
                outputTemplate: outputTemplate)
            .Enrich.FromLogContext()
            .MinimumLevel.Verbose()
            .CreateLogger();
        Log.Information("Logger initialised");
    }
}