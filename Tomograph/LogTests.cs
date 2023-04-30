using Arithmic;

namespace Tomograph;

[TestClass]
public class LogTests
{
    [TestInitialize]
    public void Initialize()
    {
        Log.Clear();
    }
    
    [TestMethod]
    public void FileSink()
    {
        List<string> messages = new List<string>();
        Log.Info("Info message");
        messages.Add("Info message");
        Log.Warning("Warning message");
        messages.Add("Warning message");
        Log.AddSink<FileSink>();
        Log.Error("Error message");
        messages.Add("Error message");
        Log.Fatal("Fatal message");
        messages.Add("Fatal message");

        string logFileName = Helpers.GetNonPublicStaticField(typeof(FileSink), "_filePath");
        string[] logFileContents = File.ReadAllLines(logFileName);
        
        Assert.AreEqual(messages.Count, logFileContents.Length);
        
        for (int i = 0; i < logFileContents.Length; i++)
        {
            StringAssert.EndsWith(logFileContents[i], messages[i]);
        }
    }
}