namespace Tiger.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TigerSchema : Attribute
{
    internal int _val;
    public int Offset
    {
        get { return _val; }
    }


    public TigerSchema(int size)
    {
        // _val = d2Offset;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class StrategyMetadata : Attribute
{
    internal string _packagePrefix;
    
    public string PackagePrefix
    {
        get { return _packagePrefix; }
    }
    
    public StrategyMetadata(string packagePrefix)
    {
        _packagePrefix = packagePrefix;
    }
}