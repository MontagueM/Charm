using System.Text;
using System.Timers;
using Timer = System.Threading.Timer;

namespace Arithmic;

public class FileSink : ISink
{
    private static readonly string LogDirectory = "./Logs";
    private static string? _filePath;

    private readonly StringBuilder _logsBuffer = new();
    private readonly System.Timers.Timer _timer = new(2000);

    public void OnLogEvent(object sender, LogEventArgs e)
    {
        if (_filePath == null)
        {
            Directory.CreateDirectory(LogDirectory);
            _filePath = Path.Join(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");

            _timer.Elapsed += OnTimer;
            _timer.AutoReset = false;
            _timer.Start();

            Log.OnFlushEvent += () =>
            {
                lock (_logsBuffer)
                {
                    File.AppendAllText(_filePath, _logsBuffer.ToString());
                    _logsBuffer.Clear();
                }
            };
        }

        lock (_logsBuffer)
        {
            _logsBuffer.AppendLine(e.Message);
        }
    }

    private void OnTimer(object? sender, ElapsedEventArgs elapsedEventArgs)
    {
        lock (_logsBuffer)
        {
            File.AppendAllText(_filePath, _logsBuffer.ToString());
            _logsBuffer.Clear();
        }
        _timer.Start();
    }
}
