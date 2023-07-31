using System;
using System.IO;
using Arithmic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Tiger;

namespace CharmAvalonia;

public partial class LogView : UserControl
{
    public LogView()
    {
        InitializeComponent();

        Log.BindDelegate(OnLogEvent);
    }

    // todo amortize this, as can cause huge thread issues when a threaded process logs a call when the receiver
    // requires a ui thread dispatch (or just some way to stop this being terrible)
    private void OnLogEvent(object? sender, LogEventArgs e)
    {
        if (e.Verbosity > LogVerbosity.Info)
        {
            return;
        }

        // Dispatcher.UIThread.Invoke(() =>
        // {
        //     LogBox.Text += e.Message + Environment.NewLine;
        //     // LogBox.ScrollToEnd();
        // });
    }
}

public class LogHandler
{
    public static void Initialise(LogView logView)
    {
        // Application.Current.DispatcherUnhandledException += DispatcherUnhandledException;
        #if !DEBUG
        AppDomain.CurrentDomain.UnhandledException += CatchUnhandledException;
        #endif
    }

#if !DEBUG
    static void CatchUnhandledException
        (object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            LogException((Exception)e.ExceptionObject);
        }
        finally
        {
            Application.Current.Shutdown();
        }
    }
    #endif

    // static void DispatcherUnhandledException
    //     (object sender, DispatcherUnhandledExceptionEventArgs e)
    // {
    //     try
    //     {
    //         LogException(e.Exception);
    //     }
    //     finally
    //     {
    //         Application.Current.Shutdown();
    //     }
    // }

    public static void LogException(Exception ex)
    {
        Log.Fatal("\n### Crash ###\n" + ex.Source + ex.InnerException + ex + ex.Message + ex.StackTrace);
        Log.Fatal("ConfigSubsystem file:\n" + File.ReadAllText("Charm.exe.config"));
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        if (config.GetPackagesPath(config.GetCurrentStrategy()) != String.Empty)
            Log.Fatal("Number of packages:\n" + Directory.GetFiles(config.GetPackagesPath(config.GetCurrentStrategy())).Length);
        if (config.GetExportSavePath() != String.Empty)
            Log.Fatal("Exported directory:\n" + string.Join("\n", Directory.GetFiles(config.GetExportSavePath()))
            + "\n" + string.Join("\n", Directory.GetDirectories(config.GetExportSavePath())));
        // Log.CloseAndFlush();
    }
}
