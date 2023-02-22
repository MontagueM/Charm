using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Field.Entities;
using Field.General;
using Field.Models;
using Field.Statics;

namespace Field;

public class StaticMapData : Tag
{
    public D2Class_AD938080 Header;

    public StaticMapData(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AD938080>();
    }

    public void LoadArrangedIntoFbxScene(FbxHandler fbxHandler)
    {
        Parallel.ForEach(Header.InstanceCounts, c =>
        {
            var s = Header.Statics[c.StaticIndex].Static;
            var parts = s.Load(ELOD.MostDetail);
            fbxHandler.AddStaticInstancesToScene(parts, Header.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), s.Hash);
        });
    }

    public void LoadIntoFbxScene(FbxHandler fbxHandler, string savePath, bool bSaveShaders, bool saveCBuffers)
    {
        List<D2Class_BD938080> extractedStatics = Header.Statics.DistinctBy(x => x.Static.Hash).ToList();

        Parallel.ForEach(extractedStatics, s =>
        {
            var parts = s.Static.Load(ELOD.MostDetail);
            fbxHandler.AddStaticToScene(parts, s.Static.Hash);
            s.Static.SaveMaterialsFromParts(savePath, parts, bSaveShaders, saveCBuffers);
        });

        Parallel.ForEach(Header.InstanceCounts, c =>
        {
            var model = Header.Statics[c.StaticIndex].Static;
            fbxHandler.InfoHandler.AddStaticInstances(Header.Instances.Skip(c.InstanceOffset).Take(c.InstanceCount).ToList(), model.Hash);
        });
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0xC0)]
public struct D2Class_AD938080
{
    public long FileSize;
    [DestinyOffset(0x18), DestinyField(FieldType.TagHash)]
    public Tag<D2Class_B1938080> ModelOcclusionBounds;
    [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
    public List<D2Class_406D8080> Instances;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0B008080> Unk50;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_BD938080> Statics;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_286D8080> InstanceCounts;
    [DestinyOffset(0x98)]
    public DestinyHash Unk98;
    [DestinyOffset(0xA0)]
    public Vector4 UnkA0; // likely a bound corner
    public Vector4 UnkB0; // likely the other bound corner
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_B1938080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_B3938080> InstanceBounds;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_B3938080
{
    public Vector4 Corner1;
    public Vector4 Corner2;
    public DestinyHash Unk20;
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_406D8080
{
    public Vector4 Rotation;
    public Vector3 Position;
    public Vector3 Scale;  // Only X is used as a global scale
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct D2Class_BD938080
{
    [DestinyField(FieldType.TagHash)]
    public StaticContainer Static;
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct D2Class_286D8080
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
[StructLayout(LayoutKind.Sequential, Size = 0x60)]
public struct D2Class_1E898080
{
    public long FileSize;
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_01878080> ChildMapReference;
    [DestinyOffset(0x10), DestinyField(FieldType.String64)] // actually wrong, not a String64 instead StringNoContainer
    public string MapName;
    public int Unk1C;
    [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
    public List<D2Class_C9968080> Unk40;
    [DestinyField(FieldType.TagHash64)] 
    public Tag Unk50;  // some kind of parent thing, very strange weird idk
}

/// <summary>
/// Basically same table as in the child tag, but in a weird format. Never understood what its for.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_C9968080
{
    [DestinyField(FieldType.TagHash64)] 
    public Tag Unk00;
}

/// <summary>
/// The one below the top reference, actually contains useful information.
/// First of MapResources is what I call "ambient entities", second is always the static map.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x60)]
public struct D2Class_01878080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)] 
    public List<D2Class_03878080> MapResources;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_03878080
{
    [DestinyField(FieldType.TagHash64)] 
    public Tag<D2Class_07878080> MapResource;
}

/// <summary>
/// A map resource, contains all the data used to make a map.
/// This is quite similar to EntityResource, but with more children.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_07878080
{
    public long FileSize;
    public long Unk08;
    [DestinyOffset(0x28), DestinyField(FieldType.TablePointer)] 
    public List<D2Class_09878080> DataTables;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_09878080
{
    [DestinyField(FieldType.TagHash)] 
    public Tag<D2Class_83988080> DataTable;
}

/// <summary>
/// A map data table, containing data entries. 
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_83988080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)] 
    public List<D2Class_85988080> DataEntries;
}


/// <summary>
/// A data entry. Can be static maps, entities, etc. with a defined world transform.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x90)]
public struct D2Class_85988080
{
    public Vector4 Rotation;
    public Vector4 Translation;
    [DestinyOffset(0x28), DestinyField(FieldType.TagHash64, true)]
    public Entity Entity;
    [DestinyOffset(0x78), DestinyField(FieldType.ResourcePointer)]
    public dynamic? DataResource;
}

/// <summary>
/// Data resource containing a static map.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C96C8080
{
    [DestinyOffset(0x8)] 
    public DestinyHash Unk08;
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag<D2Class_0D6A8080> StaticMapParent;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_0D6A8080
{
    // no filesize
    [DestinyOffset(0x8), DestinyField(FieldType.TagHash)] 
    public StaticMapData StaticMap;  // could make it StaticMapData but dont want it to load it, could have a NoLoad option
    [DestinyOffset(0x2C)]
    public DestinyHash Unk2C;
}

/// <summary>
/// Boss entity data resource?
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_19808080
{
    // todo rest of this
    // [DestinyField(FieldType.ResourcePointer)]
    // public dynamic? Unk00;
    [DestinyOffset(0x24)]
    public DestinyHash EntityName;
}

/// <summary>
/// Unk data resource, maybe lights for entities?
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_5E6C8080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // D2Class_716C8080, might be related to lights for entities?
    [DestinyOffset(0x1C)]
    public DestinyHash Unk1C;
}

/// <summary>
/// Unk data resource, maybe lights for entities?
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_636A8080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // D2Class_656C8080, might be related to lights for entities?
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_B5678080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // D2Class_A16D8080, has some materials and coords
}

/// <summary>
/// Audio data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_6F668080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash64)]
    public Tag AudioContainer;  // 38978080 audio container
}

/// <summary>
/// Spatial audio data resource, contains a list of positions to play an audio container.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x48)]
public struct D2Class_6D668080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash64)]
    public Tag AudioContainer;  // 38978080 audio container
    [DestinyOffset(0x30), DestinyField(FieldType.TablePointer)]
    public List<D2Class_94008080> AudioPositions;
    public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_94008080
{
    public Vector4 Translation;
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_B58C8080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // B78C8080
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_A36A8080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // A76A8080
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_55698080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // 5B698080, lights/volumes/smth maybe cubemaps idk
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_7B918080
{
    [DestinyField(FieldType.RelativePointer)]
    public dynamic? Unk00;
}

/// <summary>
/// Havok volume data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_21918080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag HavokVolume;  // type 27 subtype 0
    public DestinyHash Unk14;
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C0858080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // C2858080
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C26A8080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // C46A8080
}


/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_222B8080
{
    [DestinyOffset(0x10)]
    public DestinyHash Unk10;
}

/// <summary>
/// Unk data resource.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_04868080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash)]
    public Tag Unk10;  // 24878080, smth related to havok volumes
}


#endregion