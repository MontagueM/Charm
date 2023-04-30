namespace Tiger;

public interface ICommandlet
{
    public void Run(CharmArgs args);
}

public class CharmArgs
{
    private string[] _args { get; }

    public CharmArgs()
    {
        _args = Environment.GetCommandLineArgs();
    }

    public CharmArgs(string[] args)
    {
        _args = args;
    }

    public bool GetArgValue(string argName, out string value)
    {
        value = GetArgValue(argName);
        return value != null;
    }

    public bool IsArgPresent(string argName)
    {
        return _args.Any(x => x.StartsWith($"-{argName}", StringComparison.InvariantCultureIgnoreCase));
    }

    public string? GetArgValue(string argName)
    {
        for (int i = 0; i < _args.Length; i++)
        {
            if (_args[i].StartsWith($"-{argName}", StringComparison.InvariantCultureIgnoreCase) && _args[i].Contains("="))
            {
                return _args[i].Split("=")[1];
            }
        }

        return null;
    }

    public List<string>? GetArgValues(string argName)
    {
        string? value = GetArgValue(argName);
        if (value == null)
        {
            return null;
        }

        return value.Split("+").ToList();
    }
}
