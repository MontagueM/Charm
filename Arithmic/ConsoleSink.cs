using System.Diagnostics;

namespace Arithmic;

public class ConsoleSink : ISink
{
    public void OnLogEvent(object sender, LogEventArgs e)
    {
#if DEBUG
        bool shouldPrint = true;
#else
        bool shouldPrint = e.Verbosity <= LogVerbosity.Info;
#endif
        if (shouldPrint)
        {
            Console.WriteLine(e.Message);
            Debug.WriteLine(e.Message);
        }
    }
}
