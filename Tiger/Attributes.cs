namespace Tiger;

public abstract class StrategyAttribute : Attribute
{
    public TigerStrategy Strategy { get; }

    public StrategyAttribute()
    {
        Strategy = TigerStrategy.NONE;
    }
    
    public StrategyAttribute(TigerStrategy strategy)
    {
        Strategy = strategy;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class SchemaFieldAttribute : StrategyAttribute
{
    public int Offset { get; }

    public SchemaFieldAttribute(int offset)
    {
        Offset = offset;
    }
    
    public SchemaFieldAttribute(int offset, TigerStrategy strategy) : base(strategy)
    {
        Offset = offset;
    }
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class SchemaStructAttribute : StrategyAttribute
{
    public string ClassHash { get; }
    public int SerializedSize { get; }

    public SchemaStructAttribute(string classHash, int serializedSize)
    {
        ClassHash = classHash;
        SerializedSize = serializedSize;
    }
    
    public SchemaStructAttribute(TigerStrategy strategy, string classHash, int serializedSize) : base(strategy)
    {
        ClassHash = classHash;
        SerializedSize = serializedSize;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class StrategyClassAttribute : StrategyAttribute
{
    public StrategyClassAttribute(TigerStrategy strategy) : base(strategy)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class SchemaTypeAttribute : Attribute
{
    public int SerializedSize { get; }

    public SchemaTypeAttribute(int serializedSize)
    {
        SerializedSize = serializedSize;
    }
}

public struct DepotManifestVersion
{
    public uint AppId;
    public uint DepotId;
    public ulong ManifestId;
    
    public DepotManifestVersion(uint appId, uint depotId, ulong manifestId)
    {
        AppId = appId;
        DepotId = depotId;
        ManifestId = manifestId;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class StrategyMetadataAttribute : Attribute
{
    public string PackagePrefix { get; }
    public DepotManifestVersion? DepotManifestVersionMain { get; }
    public DepotManifestVersion? DepotManifestVersionAudio { get; }

    public StrategyMetadataAttribute(string packagePrefix, uint appId = 0, uint depotIdMain = 0, ulong manifestIdMain = 0, uint depotIdAudio = 0, ulong manifestIdAudio = 0)
    {
        PackagePrefix = packagePrefix;
        if (depotIdMain != 0 && manifestIdMain != 0)
        {
            DepotManifestVersionMain = new DepotManifestVersion(appId, depotIdMain, manifestIdMain);
        }
        if (depotIdAudio != 0 && manifestIdAudio != 0)
        {
            DepotManifestVersionAudio = new DepotManifestVersion(appId, depotIdAudio, manifestIdAudio);
        }
    }
}