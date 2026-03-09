using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Arithmic;
using UserControl = System.Windows.Controls.UserControl;

namespace Charm;

// TODO, Remove since LogView isnt even used anywhere anymore
// the log file/cmd log is good enough
public partial class LogView : UserControl
{
    //private StringBuilder _logsBuffer = new();
    //private System.Timers.Timer _timer = new(2000);

    public LogView()
    {
        InitializeComponent();

        //Log.BindDelegate(OnLogEvent);
        //_timer.Elapsed += OnTimer;
        //_timer.Start();
    }

    // todo amortize this, as can cause huge thread issues when a threaded process logs a call when the receiver
    // requires a ui thread dispatch (or just some way to stop this being terrible)
    private void OnLogEvent(object? sender, LogEventArgs e)
    {
        //if (e.Verbosity > LogVerbosity.Info)
        //{
        //    return;
        //}

        //_logsBuffer.AppendLine(e.Message);
    }

    private void OnTimer(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        //if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished)
        //{
        //    _timer.Stop();
        //    _timer.Elapsed -= OnTimer;
        //    return;
        //}

        //try
        //{
        //    Dispatcher.InvokeAsync(() =>
        //    {
        //        if (_logsBuffer.Length == 0) return;
        //        LogBox.AppendText(_logsBuffer.ToString());
        //        LogBox.ScrollToEnd();
        //        _logsBuffer.Clear();
        //    }, DispatcherPriority.Background);
        //}
        //catch (TaskCanceledException)
        //{
        //    _timer.Stop();
        //    _timer.Elapsed -= OnTimer;
        //}
        //_logsBuffer.Clear();
        //_timer.Start();
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
        Log.Flush();
    }
}
