using System.Runtime.InteropServices;

namespace Tiger.Schema;

public class Map : Tag<SMapContainer>
{
    public Map(FileHash fileHash) : base(fileHash)
    {
    }
}

public class StaticMapData : Tag<SStaticMapData>
{
    public StaticMapData(FileHash hash) : base(hash)
    {
    }

    public void LoadArrangedIntoFbxScene(FbxHandler fbxHandler)
    {
        Parallel.ForEach(_tag.InstanceCounts, c =>
        {
            var s = _tag.Statics[c.StaticIndex].Static;
            var parts = s.Load(ExportDetailLevel.MostDetailed);
            fbxHandler.AddStaticInstancesToScene(parts, _tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), s.Hash);
        });
    }

    public void LoadIntoFbxScene(FbxHandler fbxHandler, string savePath, bool bSaveShaders)
    {
        List<SStaticMeshHash> extractedStatics = _tag.Statics.DistinctBy(x => x.Static.Hash).ToList();

        Parallel.ForEach(extractedStatics, s =>
        {
            var parts = s.Static.Load(ExportDetailLevel.MostDetailed);
            fbxHandler.AddStaticToScene(parts, s.Static.Hash);
            s.Static.SaveMaterialsFromParts(savePath, parts, bSaveShaders);
        });

        Parallel.ForEach(_tag.InstanceCounts, c =>
        {
            var model = _tag.Statics[c.StaticIndex].Static;
            fbxHandler.InfoHandler.AddStaticInstances(_tag.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), model.Hash);
        });
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6D968080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AD938080", 0xC0)]
public struct SStaticMapData
{
    public long FileSize;
    [SchemaField(0x18)]
    public Tag<SOcclusionBounds> ModelOcclusionBounds;
    [SchemaField(0x40)]
    public DynamicArray<SStaticMeshInstanceTransform> Instances;
    public DynamicArray<SUnknownUInt> Unk50;
    [SchemaField(0x58, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x78, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArray<SStaticMeshHash> Statics;
    public DynamicArray<SStaticMeshInstanceMap> InstanceCounts;
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x98, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk98;
    [SchemaField(0x80, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 UnkA0; // likely a bound corner
    public Vector4 UnkB0; // likely the other bound corner
}

[SchemaStruct("0B008080", 0x04)]
public struct SUnknownUInt
{
    public uint Unk00;
}


[SchemaStruct("B1938080", 0x18)]
public struct SOcclusionBounds
{
    public long FileSize;
    public DynamicArray<SMeshInstanceOcclusionBounds> InstanceBounds;
}

[SchemaStruct("B3938080", 0x30)]
public struct SMeshInstanceOcclusionBounds
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public TigerHash Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A3718080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "406D8080", 0x40)]
public struct SStaticMeshInstanceTransform
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7D968080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "BD938080", 0x4)]
public struct SStaticMeshHash
{
    public StaticMesh Static;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "90718080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "286D8080", 0x8)]
public struct SStaticMeshInstanceMap
{
    public short InstanceCount;
    public short InstanceOffset;
    public short StaticIndex;
    public short Unk06;
}

#region Parent/other structures for maps


/// <summary>
/// The very top reference for all map-related things.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AE7D8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "1E898080", 0x60)]
public struct SBubbleParent
{
    public long FileSize;
    public Tag<SBubbleDefinition> ChildMapReference;
    [SchemaField(0x10)] // todo i dont get this
    public StringHash MapName;
    public int Unk1C;
    [SchemaField(0x40)]
    public DynamicArray<D2Class_C9968080> Unk40;
    [Tag64, SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag Unk50;  // some kind of parent thing, very strange weird idk
}

/// <summary>
/// Basically same table as in the child tag, but in a weird format. Never understood what its for.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "44968080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C9968080", 0x10)]
public struct D2Class_C9968080
{
    [Tag64]
    public Tag Unk00;
}

/// <summary>
/// The one below the top reference, actually contains useful information.
/// First of MapResources is what I call "ambient entities", second is always the static map.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E0918080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "01878080", 0x60)]
public struct SBubbleDefinition
{
    public long FileSize;
    public DynamicArray<SMapContainerEntry> MapResources;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C1848080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "03878080", 0x10)]
public struct SMapContainerEntry
{
    [Tag64]
    public Tag<SMapContainer> MapContainer;
}

/// <summary>
/// A map resource, contains data used to make a map.
/// This is quite similar to EntityResource, but with more children.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "548A8080", 0x38)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "07878080", 0x38)]
public struct SMapContainer
{
    public long FileSize;
    public long Unk08;
    [SchemaField(0x28)]
    public DynamicArray<SMapDataTableEntry> MapDataTables;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B08B8080", 4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "09878080", 4)]
public struct SMapDataTableEntry
{
    public Tag<SMapDataTable> MapDataTable;
}

/// <summary>
/// A map data table, containing data entries.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "D6998080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "83988080", 0x18)]
public struct SMapDataTable
{
    public long FileSize;
    public DynamicArray<SMapDataEntry> DataEntries;
}


/// <summary>
/// A data entry. Can be static maps, entities, etc. with a defined world transform.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "D8998080", 0x90)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "85988080", 0x90)]
public struct SMapDataEntry
{
    // for now we'll process both entities as if they were the same
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true), Tag64, NoLoad]
    public Entity.Entity EntitySK;
    [SchemaField(0x28, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 Rotation;
    public Vector4 Translation;
    [SchemaField(0x28, TigerStrategy.DESTINY2_WITCHQUEEN_6307), Tag64, NoLoad]
    public Entity.Entity EntityWQ;
    [SchemaField(0x68)]
    public uint Unk68;
    [SchemaField(0x78)]
    public ResourcePointer DataResource;

    public FileHash GetEntityHash()
    {
        if (EntitySK != null)
            return EntitySK.Hash;
        else
            return EntityWQ.Hash;
    }
}

/// <summary>
/// Data resource containing a static map.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B3718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C96C8080", 0x18)]
public struct SMapDataResource
{
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public Tag<SStaticMapParent> StaticMapParent;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F46E8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "0D6A8080", 0x30)]
public struct SStaticMapParent
{
    // no filesize
    [SchemaField(0x8)]
    public StaticMapData StaticMap;  // could make it StaticMapData but dont want it to load it, could have a NoLoad option
    [SchemaField(0x24, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x2C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk2C;
}

// /// <summary>
// /// Boss entity data resource?
// /// </summary>
// [SchemaStruct("19808080", 0x50)]
// public struct D2Class_19808080
// {
//     // todo rest of this
//     // [DestinyField(FieldType.ResourcePointer)]
//     // public dynamic? Unk00;
//     [SchemaField(0x24)]
//     public TigerHash EntityName;
// }
//
// /// <summary>
// /// Unk data resource, maybe lights for entities?
// /// </summary>
// [SchemaStruct("5E6C8080", 0x20)]
// public struct D2Class_5E6C8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // D2Class_716C8080, might be related to lights for entities?
//     [SchemaField(0x1C)]
//     public TigerHash Unk1C;
// }
//
// /// <summary>
// /// Unk data resource, maybe lights for entities?
// /// </summary>
// [SchemaStruct("636A8080", 0x18)]
// public struct D2Class_636A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // D2Class_656C8080, might be related to lights for entities?
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("B5678080", 0x18)]
// public struct D2Class_B5678080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // D2Class_A16D8080, has some materials and coords
// }
//
// /// <summary>
// /// Audio data resource.
// /// </summary>
// [SchemaStruct("6F668080", 0x30)]
// public struct D2Class_6F668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
// }
//
// /// <summary>
// /// Spatial audio data resource, contains a list of positions to play an audio container.
// /// </summary>
// [SchemaStruct("6D668080", 0x48)]
// public struct D2Class_6D668080
// {
//     [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
//     public Tag AudioContainer;  // 38978080 audio container
//     [SchemaField(0x30), DestinyField(FieldType.TablePointer)]
//     public List<D2Class_94008080> AudioPositions;
//     public float Unk40;
// }
//
// [SchemaStruct("94008080", 0x10)]
// public struct D2Class_94008080
// {
//     public Vector4 Translation;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("B58C8080", 0x18)]
// public struct D2Class_B58C8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // B78C8080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("A36A8080", 0x18)]
// public struct D2Class_A36A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // A76A8080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("55698080", 0x18)]
// public struct D2Class_55698080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 5B698080, lights/volumes/smth maybe cubemaps idk
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("7B918080", 0x18)]
// public struct D2Class_7B918080
// {
//     [DestinyField(FieldType.RelativePointer)]
//     public dynamic? Unk00;
// }
//
// /// <summary>
// /// Havok volume data resource.
// /// </summary>
// [SchemaStruct("21918080", 0x20)]
// public struct D2Class_21918080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag HavokVolume;  // type 27 subtype 0
//     public TigerHash Unk14;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C0858080", 0x18)]
// public struct D2Class_C0858080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C2858080
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("C26A8080", 0x18)]
// public struct D2Class_C26A8080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // C46A8080
// }
//
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("222B8080", 0x18)]
// public struct D2Class_222B8080
// {
//     [SchemaField(0x10)]
//     public TigerHash Unk10;
// }
//
// /// <summary>
// /// Unk data resource.
// /// </summary>
// [SchemaStruct("04868080", 0x18)]
// public struct D2Class_04868080
// {
//     [SchemaField(0x10), DestinyField(FieldType.FileHash)]
//     public Tag Unk10;  // 24878080, smth related to havok volumes
// }


#endregion
