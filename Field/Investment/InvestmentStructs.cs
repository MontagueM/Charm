using System.Runtime.InteropServices;
using Field.Entities;
using Field.General;

namespace Field;

/// <summary>
/// Stores all the inventory item definitions in a huge hashmap.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_97798080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)] 
    public List<D2Class_9B798080> InventoryItemDefinitionEntries;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_9B798080
{
    public DestinyHash InventoryItemHash;
    [DestinyOffset(0x10)]
    public TagHash InventoryItem;  // I don't want to parse all these, should be Tag<D2Class_9D798080>
}

#region InventoryItemDefinition

/// <summary>
/// Inventory item definition.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x120)]
public struct D2Class_9D798080
{
    public long FileSize;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk08;  // D2Class_E4768080
    [DestinyOffset(0x18), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk18;  // D2Class_E7778080
    [DestinyOffset(0x50), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk50;
    [DestinyOffset(0x78), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk78;  // D2Class_81738080
    [DestinyOffset(0x88), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk88;  // D2Class_7F738080
    [DestinyOffset(0x90), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk90;  // D2Class_77738080
    [DestinyOffset(0xA8)]
    public DestinyHash InventoryItemHash;
    public DestinyHash UnkAC;
    [DestinyOffset(0x110), DestinyField(FieldType.TablePointer)]
    public List<D2Class_05798080> TraitIndices;
}

[StructLayout(LayoutKind.Sequential, Size = 0x90)]
public struct D2Class_E4768080
{
    [DestinyOffset(0x48)]
    public DestinyHash Unk48;
    public DestinyHash Unk4C;
    [DestinyOffset(0x54)] 
    public int Unk54;
    [DestinyOffset(0x70)] 
    public int Unk70;
    [DestinyOffset(0x88)] 
    public float Unk88;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_E7778080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_387A8080> Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_387A8080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3A7A8080> Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_3A7A8080
{
    public int Unk00;
    public int Unk04;
}

[StructLayout(LayoutKind.Sequential, Size = 0x70)]
public struct D2Class_DC778080
{
    [DestinyOffset(0x08)]
    public short Unk08;
    [DestinyOffset(0x60), DestinyField(FieldType.TablePointer)]
    public List<D2Class_DE778080> Unk60;
}

[StructLayout(LayoutKind.Sequential, Size = 2)]
public struct D2Class_DE778080
{
    public short Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 2)]
public struct D2Class_05798080
{
    public short Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_81738080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_86738080> InvestmentStats;  // "investmentStats" from API
}

/// <summary>
/// "investmentStat" from API
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_86738080
{
    public int StatTypeIndex;  // "statTypeHash" from API
    public int Value;  // "value" from API
}

[StructLayout(LayoutKind.Sequential, Size = 2)]
public struct D2Class_7F738080
{
    public short Unk00;
}


/// <summary>
/// "translationBlock" from API
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x60)]
public struct D2Class_77738080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_7D738080> Arrangements;  // "arrangements" from API
    [DestinyOffset(0x48), DestinyField(FieldType.TablePointer)]
    public List<D2Class_7B738080> LockedDyes;  // "lockedDyes" from API
    public short WeaponPatternIndex;  // "weaponPatternHash" from API
}

/// <summary>
/// "arrangement" from API
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_7D738080
{
    public short ClassHash;  // "classHash" from API
    public short ArtArrangementHash;  // "artArrangementHash" from API
}

/// <summary>
/// "lockedDyes" from API
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_7B738080
{
    public short ChannelIndex;  // "channelHash" from API
    public short DyeIndex;  // "dyeHash" from API
}

#endregion

#region ArtArrangement

/// <summary>
/// Stores all the art arrangement hashes in an index-accessed list.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_F2708080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)] 
    public List<D2Class_ED6F8080> ArtArrangementHashes;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_ED6F8080
{
    public DestinyHash ArtArrangementHash;
}

#endregion

#region ApiEntities

/// <summary>
/// Entity assignment tag header. The assignment can be accessed via the art arrangement index.
/// The file is massive so I don't auto-parse it.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_CE558080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_D4558080> ArtArrangementEntityAssignments;
    // [DestinyField(FieldType.TablePointer)]
    // public List<D2Class_D8558080> FinalAssignment;  // this is not needed as the above table has resource pointers
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_D4558080
{
    public DestinyHash ArtArrangementHash;
    [DestinyOffset(0x8)] 
    public DestinyHash MasculineSingleEntityAssignment; // things like armour only have 1 entity, so can skip the jumps
    public DestinyHash FeminineSingleEntityAssignment;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_D7558080> MultipleEntityAssignments;
}


[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_D7558080
{
    [DestinyField(FieldType.ResourceInTablePointer)]  // todo write this code in the tag parser
    public D2Class_D8558080 EntityAssignmentResource;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_D8558080
{
    public long Unk00;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_DA558080> EntityAssignments;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_DA558080
{
    public DestinyHash EntityAssignmentHash;
}

/// <summary>
/// The "final" assignment map of assignment hash : entity hash
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_434F8080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_454F8080> EntityArrangementMap;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_454F8080 : IComparer<D2Class_454F8080>
{
    public DestinyHash AssignmentHash;
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_A36F8080> EntityParent;
    
    public int Compare(D2Class_454F8080 x, D2Class_454F8080 y)
    {
        if (x.AssignmentHash.Equals(y.AssignmentHash)) return 0;
        return x.AssignmentHash.CompareTo(y.AssignmentHash);
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_A36F8080
{
    public long FileSize;
    [DestinyField(FieldType.TagHash64)]
    public TagHash Entity;  // These is Entity but dont want to load every API Entity in the entire game
}


#endregion