using System.Diagnostics;

namespace Arithmic;

public class ConsoleSink : ISink
{
    public void OnLogEvent(object sender, LogEventArgs e)
    {
        if (e.Verbosity <= LogVerbosity.Info)
        {
            Console.WriteLine(e.Message);
            Debug.WriteLine(e.Message);
        }
    }
}
