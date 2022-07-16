using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
// using System.Windows.Forms;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using Serilog.Sinks.RichTextBox.Themes;
using Serilog.Templates;
using UserControl = System.Windows.Controls.UserControl;

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
        
        // Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
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
    
    static void CatchUnhandledException
        (object sender, UnhandledExceptionEventArgs e)
    {
        Exception ex;
        try
        {
            ex = (Exception)e.ExceptionObject;
            
            Log.Fatal("\n### Crash ###\n" + ex.Source + ex.InnerException + ex + ex.Message + ex.StackTrace);
            Log.Fatal("Config file:\n" + File.ReadAllText("Charm.exe.config"));
            Log.CloseAndFlush();
        }
        finally
        {
            Application.Current.Shutdown();
        }
    }
    
    static void DispatcherUnhandledException
        (object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Exception ex;
        try
        {
            ex = (Exception)e.Exception;
            
            Log.Fatal("\n### Crash ###\n" + ex.Source + ex.InnerException + ex + ex.Message + ex.StackTrace);
            Log.Fatal("Config file:\n" + File.ReadAllText("Charm.exe.config"));
            Log.CloseAndFlush();
        }
        finally
        {
            Application.Current.Shutdown();
        }
    }
}