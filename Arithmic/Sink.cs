namespace Arithmic;

public interface ISink
{
    public void OnLogEvent(object sender, LogEventArgs e);
}
