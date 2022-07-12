using System.Runtime.InteropServices;
using Field.General;
using Field.Strings;

namespace Field;

public class Activity : Tag
{
    public D2Class_8E8E8080 Header;

    public Activity(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_8E8E8080>();
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x78)]
public struct D2Class_8E8E8080
{
    public long FileSize;
    public DestinyHash LocationName;  // these all have actual string hashes but have no string container given directly
    public DestinyHash ActivityName;
    public DestinyHash Unk10;
    public DestinyHash Unk14;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk18;  // 6A988080 + 20978080
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk20;  // some weird kind of parent thing with names, contains the string container for this tag
    [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
    public List<D2Class_26898080> Unk40;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_24898080> Unk50;
    public DestinyHash Unk60;
    [DestinyField(FieldType.TagHash)]
    public Tag Unk64;  // an entity thing
    [DestinyField(FieldType.TagHash64)] 
    public Tag UnkActivity68;
}

[StructLayout(LayoutKind.Sequential, Size = 0x90)]
public struct D2Class_26898080
{
    public DestinyHash LocationName;
    public DestinyHash ActivityName;
    public DestinyHash BubbleName;
    public DestinyHash Unk0C;
    public DestinyHash Unk10;
    [DestinyOffset(0x18)]
    public DestinyHash BubbleName2;
    [DestinyOffset(0x20)]
    public DestinyHash Unk20;
    public DestinyHash Unk24;
    public DestinyHash Unk28;
    [DestinyOffset(0x30)]
    public int Unk30;
    [DestinyOffset(0x48)]
    public DestinyHash Unk48;
    [DestinyOffset(0x50)]
    public DestinyHash Unk50;
    public DestinyHash Unk54;
    public DestinyHash Unk58;
    [DestinyOffset(0x6A)] 
    public short Unk6A;
    [DestinyOffset(0x70), DestinyField(FieldType.TablePointer)]
    public List<D2Class_48898080> Unk70;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_48898080
{
    public DestinyHash LocationName;
    public DestinyHash ActivityName;
    public DestinyHash BubbleName;
    public DestinyHash ActivityPhaseName;
    public DestinyHash ActivityPhaseName2;
    [DestinyField(FieldType.TagHash)] 
    public Tag<D2Class_898E8080> UnkEntityReference;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_898E8080
{
    public long FileSize;
    public long Unk08;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk10;  // 46938080 has dialogue table, 45938080 unk
    [DestinyField(FieldType.TagHash)]
    public Tag Unk14;  // D2Class_898E8080 entity script stuff
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_46938080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag DialogueTable;
    [DestinyOffset(0x3C)] 
    public int Unk3C;
    public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_45938080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
    [DestinyOffset(0x18), DestinyField(FieldType.TablePointer)]
    public List<D2Class_28998080> Unk18;
    [DestinyOffset(0x3C)] 
    public int Unk3C;
    public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_28998080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
    public DestinyHash Unk08;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_1A978080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_478F8080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_24898080
{
    public DestinyHash LocationName;
    public DestinyHash ActivityName;
    public DestinyHash BubbleName;
    [DestinyOffset(0x10), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk10;  // 0F978080
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_48898080> Unk18;

    [DestinyField(FieldType.TagHash64)]
    public Tag<D2Class_1E898080> MapReference;
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_0F978080
{
    [DestinyField(FieldType.RelativePointer)]
    public string BubbleName;
    public DestinyHash Unk08;
    public DestinyHash Unk0C;
    public DestinyHash Unk10;
    [DestinyOffset(0x28)]
    public long Unk28;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_DD978080> Unk30;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_DD978080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
    public DestinyHash Unk08;
}

/// <summary>
/// Directive table + audio links for activity directives.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x88)]
public struct D2Class_6A988080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_28898080> DirectiveTables;
    [DestinyField(FieldType.TagHash64)] 
    public Tag DialogueTable;
    public DestinyHash StartingBubbleName;
    public DestinyHash Unk24;
    [DestinyOffset(0x2C), DestinyField(FieldType.TagHash)]
    public Tag MusicTable;
    
}

/// <summary>
/// Directive table for public events so no audio linked.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_20978080
{
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_28898080> PEDirectiveTables;
    [DestinyOffset(0x20)]
    public DestinyHash StartingBubbleName;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_28898080
{
    [DestinyField(FieldType.TagHash)]
    public Tag DialogueTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_0B978080
{
    [DestinyField(FieldType.RelativePointer)]
    public string BubbleName;
    public DestinyHash Unk08;
    public DestinyHash Unk0C;
    public DestinyHash Unk10;
    [DestinyOffset(0x40), DestinyField(FieldType.TablePointer)]
    public List<D2Class_0C008080> Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_0C008080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
}
