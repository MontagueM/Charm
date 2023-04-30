namespace Arithmic;

public class FileSink : ISink
{
    private static readonly string LogDirectory = "./Logs";
    private static string? _filePath;

    public void OnLogEvent(object sender, LogEventArgs e)
    {
        if (_filePath == null)
        {
            Directory.CreateDirectory(LogDirectory);
            _filePath = Path.Join(LogDirectory, $"log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        }
        
        File.AppendAllText(_filePath, e.Message + Environment.NewLine);
    }
}