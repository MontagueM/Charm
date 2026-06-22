namespace Tiger;

//[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
//public class Tag64Attribute : Attribute
//{
//}

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
    public int ArraySizeConst { get; set; } = 1;  // used for marshalled fixed arrays

    // Used to mark that this field no longer exists in this strategy onwards
    public bool Obsolete { get; set; } = false;

    // Used to mark that this field is a Tag64, replaces the Tag64 Attribute as that would not allow one version to have Tag64 but not another
    // Obviously make sure that you only set this on an actual tag
    public bool Tag64 { get; set; } = false;

    public SchemaFieldAttribute()
    {
        Offset = -1; // Required
    }

    public SchemaFieldAttribute(int offset)
    {
        Offset = offset;
    }

    public SchemaFieldAttribute(int offset, TigerStrategy strategy) : base(strategy)
    {
        Offset = offset;
    }

    public SchemaFieldAttribute(TigerStrategy strategy) : base(strategy)
    {
        Offset = -1; // Required
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class NoLoadAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = true)]
public class SchemaStructAttribute : StrategyAttribute
{
    public int SerializedSize { get; }

    private uint _classID;
    public uint ClassID
    {
        get => _classID;
        set { _classID = value; }
    }

    /// <summary>
    /// Initializes a new instance of the SchemaStructAttribute class with the specified strategy and size.
    /// </summary>
    /// <param name="strategy">The strategy to use.</param>
    /// <param name="serializedSize">The size, in bytes, of the serialized structure.</param>
    public SchemaStructAttribute(TigerStrategy strategy, int serializedSize) : base(strategy)
    {
        ClassID = TigerHash.InvalidHash32;
        SerializedSize = serializedSize;
    }

    /// <summary>
    /// Initializes a new instance of the SchemaStructAttribute class with the specified size.
    /// </summary>
    /// <param name="serializedSize">The size, in bytes, of the serialized structure.</param>
    public SchemaStructAttribute(int serializedSize)
    {
        ClassID = TigerHash.InvalidHash32;
        SerializedSize = serializedSize;
    }

    /// <summary>
    /// Initializes a new instance of the SchemaStructAttribute class with the specified class ID and size.
    /// This version should only be used for primitive types such as Vectors
    /// </summary>
    /// <param name="classID">The ID, as a uint, identifying the class.</param>
    /// <param name="serializedSize">The size, in bytes, of the serialized structure.</param>
    public SchemaStructAttribute(uint classID, int serializedSize)
    {
        ClassID = classID;
        SerializedSize = serializedSize;
    }

    /// <summary>
    /// Initializes a new instance of the SchemaStructAttribute class with the specified strategy, class ID, and size.
    /// serialized size.
    /// </summary>
    /// <param name="strategy">The strategy to use.</param>
    /// <param name="classID">The ID, as a uint, identifying the class.</param>
    /// <param name="serializedSize">The size, in bytes, of the serialized structure.</param>
    public SchemaStructAttribute(TigerStrategy strategy, uint classID, int serializedSize) : base(strategy)
    {
        ClassID = classID;
        SerializedSize = serializedSize;
    }


    [Obsolete("Class IDs should now be defined as a uint (Ex: 0x8080ABCD")]
    public SchemaStructAttribute(string classID, int serializedSize)
    {
        ClassID = LegacyStringIDToUInt(classID);
        SerializedSize = serializedSize;
    }

    [Obsolete("Class IDs should now be defined as a uint (Ex: 0x8080ABCD)")]
    public SchemaStructAttribute(TigerStrategy strategy, string classID, int serializedSize) : base(strategy)
    {
        ClassID = LegacyStringIDToUInt(classID);
        SerializedSize = serializedSize;
    }

    private uint LegacyStringIDToUInt(string classID)
    {
        byte[] bytes = Helpers.HexStringToByteArray(classID);
        if (classID.StartsWith("8080") && !classID.EndsWith("8080"))
            Array.Reverse(bytes);

        return BitConverter.ToUInt32(bytes);
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
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class NonSchemaTypeAttribute : StrategyAttribute
{
    public int Type { get; }
    public HashSet<int> SubTypes { get; } = new();

    public NonSchemaTypeAttribute(int type, int[] subTypes)
    {
        Type = type;
        SubTypes.UnionWith(subTypes);
    }

    public NonSchemaTypeAttribute(TigerStrategy strategy, int type, int[] subTypes) : base(strategy)
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
    public DepotManifestVersion? DepotManifestVersionLanguage { get; }

    public StrategyMetadataAttribute(string packagePrefix, uint appId = 0, uint depotIdMain = 0, ulong manifestIdMain = 0, uint depotIdLanguage = 0, ulong manifestIdLanguage = 0)
    {
        PackagePrefix = packagePrefix;
        if (depotIdMain != 0 && manifestIdMain != 0)
        {
            DepotManifestVersionMain = new DepotManifestVersion(appId, depotIdMain, manifestIdMain);
        }
        if (depotIdLanguage != 0 && manifestIdLanguage != 0)
        {
            DepotManifestVersionLanguage = new DepotManifestVersion(appId, depotIdLanguage, manifestIdLanguage);
        }
    }
}
