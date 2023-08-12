using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Arithmic;
using Tiger;
using UserControl = System.Windows.Controls.UserControl;

namespace Charm;

public partial class LogView : UserControl
{
    private StringBuilder _logsBuffer = new();
    private System.Timers.Timer _timer = new(2000);

    public LogView()
    {
        InitializeComponent();

        Log.BindDelegate(OnLogEvent);
        _timer.Elapsed += OnTimer;
        _timer.Start();
    }

    // todo amortize this, as can cause huge thread issues when a threaded process logs a call when the receiver
    // requires a ui thread dispatch (or just some way to stop this being terrible)
    private void OnLogEvent(object? sender, LogEventArgs e)
    {
        if (e.Verbosity > LogVerbosity.Info)
        {
            return;
        }

        _logsBuffer.AppendLine(e.Message);
    }

    private void OnTimer(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        Dispatcher.Invoke(() =>
        {
            LogBox.AppendText(_logsBuffer.ToString());
            LogBox.ScrollToEnd();
        });
        _logsBuffer.Clear();
        _timer.Start();
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

    static void DispatcherUnhandledException
        (object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            LogException(e.Exception);
        }
        finally
        {
            Application.Current.Shutdown();
        }
    }

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
        Log.Flush();
    }
}
