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
public class StrategyMetadata : Attribute
{
    internal string _packagePrefix;
    internal Type _packageType;

    public string PackagePrefix => _packagePrefix;
    public Type PackageType => _packageType;
    public DepotManifestVersion? DepotManifestVersionMain { get; }
    public DepotManifestVersion? DepotManifestVersionAudio { get; }

    public StrategyMetadata(string packagePrefix, Type packageType, uint appId = 0, uint depotIdMain = 0, ulong manifestIdMain = 0, uint depotIdAudio = 0, ulong manifestIdAudio = 0)
    {
        _packagePrefix = packagePrefix;
        _packageType = packageType;
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