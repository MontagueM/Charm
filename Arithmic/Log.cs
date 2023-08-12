using System.Runtime.CompilerServices;

namespace Arithmic;

public enum LogVerbosity
{
    Fatal,
    Error,
    Warning,
    Info,
    Verbose,
    Debug
}

public class LogEventArgs : EventArgs
{
    public LogVerbosity Verbosity { get; set; }
    public string Category { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
}

public static class Log
{
    public static List<LogEventArgs> LogHistory { get; } = new();
    private static event EventHandler<LogEventArgs> OnLogEvent = delegate { };

    public delegate void FlushEventHandler();
    public static event FlushEventHandler OnFlushEvent = delegate { };

    public static T AddSink<T>() where T : ISink
    {
        T sink = (T)Activator.CreateInstance(typeof(T));

        OnLogEvent += sink.OnLogEvent;

        foreach (LogEventArgs logEvent in LogHistory)
        {
            sink.OnLogEvent(null, logEvent);
        }

        return sink;
    }

    public static void BindDelegate(Action<object?, LogEventArgs> action)
    {
        OnLogEvent += delegate (object? sender, LogEventArgs args) { action(sender, args); };

        foreach (LogEventArgs logEvent in LogHistory)
        {
            action(null, logEvent);
        }
    }

    public static void Debug(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Debug, message, callerMethodName, callerFile);
    }

    public static void Verbose(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Verbose, message, callerMethodName, callerFile);
    }

    public static void Info(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Info, message, callerMethodName, callerFile);
    }

    public static void Warning(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Warning, message, callerMethodName, callerFile);
    }

    public static void Error(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Error, message, callerMethodName, callerFile);
    }

    public static void Fatal(string message, [CallerMemberName] string callerMethodName = "", [CallerFilePath] string callerFile = "")
    {
        LogEvent(LogVerbosity.Fatal, message, callerMethodName, callerFile);
    }

    private static void LogEvent(LogVerbosity verbosity, string message, string callerMethodName, string callerFile)
    {
        // disable if debugging things
        if (verbosity == LogVerbosity.Debug)
        {
            return;
        }

        string formattedMessage = MakeFormattedMessage(verbosity, message, callerMethodName, callerFile);
        LogEventArgs logEventArgs = new LogEventArgs { Verbosity = verbosity, Message = formattedMessage, Time = DateTime.Now };
        LogHistory.Add(logEventArgs);
        OnLogEvent(null, logEventArgs);
    }

    private static string MakeFormattedMessage(LogVerbosity verbosity, string message, string callerMethodName, string callerFile)
    {
        return $"[{DateTime.Now:yy/MM/dd HH:mm:ss.fff}] [{verbosity}] [{callerFile.Split('\\').Last().Split('.').First()}::{callerMethodName}] {message}";
    }

    public static void Clear()
    {
        LogHistory.Clear();
    }

    public static void Flush()
    {
        OnFlushEvent();
    }
}
