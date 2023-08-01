using System.Timers;

namespace Arithmic;

public class FileSink : ISink
{
    private static readonly string LogDirectory = "./Logs";
    private static string? _filePath;

    private Queue<string> _loqQueue = new();

    public void OnLogEvent(object sender, LogEventArgs e)
    {
        if (_filePath == null)
        {
            Directory.CreateDirectory(LogDirectory);
            _filePath = Path.Join(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            System.Timers.Timer timer = new(2000);
            timer.Elapsed += OnTimer;
            timer.AutoReset = true;
        }

        _loqQueue.Enqueue(e.Message);
    }

    private void OnTimer(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        var a = 0;
        // File.AppendAllText(_filePath, e.Message + Environment.NewLine);
    }
}
