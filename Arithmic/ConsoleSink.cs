namespace Arithmic;

public class ConsoleSink : ISink
{
    public void OnLogEvent(object sender, LogEventArgs e)
    {
        Console.WriteLine(e.Message);
    }
}