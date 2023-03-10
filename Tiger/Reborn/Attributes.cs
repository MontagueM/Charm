namespace Tiger.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class TigerSchema : Attribute
{
    internal int _val;
    public int Offset
    {
        get {
            return _val;
        }
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
    internal Type _packageType;

    public string PackagePrefix => _packagePrefix;

    public Type PackageType => _packageType;

    public StrategyMetadata(string packagePrefix, Type packageType)
    {
        _packagePrefix = packagePrefix;
        _packageType = packageType;
    }
}