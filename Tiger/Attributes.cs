namespace Tiger;

public class Tag64Attribute : Attribute
{
}

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

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class NoLoadAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class SchemaStructAttribute : StrategyAttribute
{
    public string ClassHash { get; }
    public int SerializedSize { get; }

    public SchemaStructAttribute(int serializedSize)
    {
        ClassHash = "";
        SerializedSize = serializedSize;
    }

    public SchemaStructAttribute(string classHash, int serializedSize)
    {
        ClassHash = classHash;
        SerializedSize = serializedSize;
    }

    public SchemaStructAttribute(TigerStrategy strategy, int serializedSize) : base(strategy)
    {
        ClassHash = "FFFFFFFF";
        SerializedSize = serializedSize;
    }

    public SchemaStructAttribute(TigerStrategy strategy, string classHash, int serializedSize) : base(strategy)
    {
        ClassHash = classHash;
        SerializedSize = serializedSize;
    }
}

/// <summary>
/// -1 type or empty subtype represents "any"
/// </summary>
[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class NonSchemaStructAttribute : StrategyAttribute
{
    public int Type { get; } = -1;
    public HashSet<int> SubTypes { get; } = new();
    public int SerializedSize { get; }

    public NonSchemaStructAttribute(int serializedSize)
    {
        SerializedSize = serializedSize;
    }

    public NonSchemaStructAttribute(int serializedSize, int type)
    {
        SerializedSize = serializedSize;
        Type = type;
    }

    public NonSchemaStructAttribute(int serializedSize, int type, int subType)
    {
        SerializedSize = serializedSize;
        Type = type;
        SubTypes.Add(subType);
    }

    public NonSchemaStructAttribute(int serializedSize, int type, int[] subTypes)
    {
        SerializedSize = serializedSize;
        Type = type;
        SubTypes.UnionWith(subTypes);
    }

    public NonSchemaStructAttribute(TigerStrategy strategy, int serializedSize) : base(strategy)
    {
        SerializedSize = serializedSize;
    }

    public NonSchemaStructAttribute(TigerStrategy strategy, int serializedSize, int type) : base(strategy)
    {
        SerializedSize = serializedSize;
        Type = type;
    }

    public NonSchemaStructAttribute(TigerStrategy strategy, int serializedSize, int type, int subType) : base(strategy)
    {
        SerializedSize = serializedSize;
        Type = type;
        SubTypes.Add(subType);
    }

    public NonSchemaStructAttribute(TigerStrategy strategy, int serializedSize, int type, int[] subTypes) : base(strategy)
    {
        SerializedSize = serializedSize;
        Type = type;
        SubTypes.UnionWith(subTypes);
    }
}

/// <summary>
/// -1 type or empty subtype represents "any"
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NonSchemaTypeAttribute : Attribute
{
    public int Type { get; } = -1;
    public HashSet<int> SubTypes { get; } = new();

    public NonSchemaTypeAttribute(int type, int subType)
    {
        Type = type;
        SubTypes.Add(subType);
    }

    public NonSchemaTypeAttribute(int type, int[] subTypes)
    {
        Type = type;
        SubTypes.UnionWith(subTypes);
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
