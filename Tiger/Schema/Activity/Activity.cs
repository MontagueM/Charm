using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;
using Tiger;

namespace Tiger.Schema;

public class Activity : Tag<SActivity>
{
    public Activity(FileHash hash) : base(hash)
    {
    }

    // protected override void ParseStructs()
    // {
    //     // Getting the string container
    //     StringContainer sc;
    //     using (var handle = GetHandle())
    //     {
    //         handle.BaseStream.Seek(0x28, SeekOrigin.Begin);
    //         var tag = PackageHandler.GetTag<D2Class_8B8E8080>(new FileHash(handle.ReadUInt64()));
    //         sc = tag._tag.StringContainer;
    //     }
    //     Header = ReadHeader<D2Class_8E8E8080>(sc);
    // }
}

[StructLayout(LayoutKind.Sequential, Size = 0x78)]
public struct SActivity
{
    public long FileSize;
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    public TigerHash Unk0C;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public ResourcePointer Unk18;  // 6A988080 + 20978080
    public FileHash64 Unk20;  // some weird kind of parent thing with names, contains the string container for this tag
    [SchemaField(0x40)]
    public DynamicArray<D2Class_26898080> Unk40;
    public DynamicArray<D2Class_24898080> Unk50;
    public TigerHash Unk60;
    public FileHash Unk64;  // an entity thing
    public FileHash64 UnkActivity68;
}

[StructLayout(LayoutKind.Sequential, Size = 0x78)]
public struct D2Class_8B8E8080
{
    public long FileSize;
    public TigerHash LocationName;
    [SchemaField(0x10), DestinyField(FieldType.TagHash64)]
    public StringContainer StringContainer;
    public FileHash Events;
    public FileHash Patrols;
    public uint Unk28;
    public FileHash Unk2C;
    public DynamicArray<D2Class_DE448080> TagBags;
    [SchemaField(0x48)]
    public DynamicArray<D2Class_2E898080> Activities;
    public StringPointer DestinationName;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_DE448080
{
    public Tag Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_2E898080
{
    public TigerHash ShortActivityName;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    public TigerHash Unk10;
    public StringPointer ActivityName;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_26898080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x18)]
    public TigerHash BubbleName2;
    [SchemaField(0x20)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;
    [SchemaField(0x30)]
    public int Unk30;
    [SchemaField(0x38)]
    public DynamicArray<D2Class_48898080> Unk38;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_48898080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    public TigerHash ActivityPhaseName;
    public TigerHash ActivityPhaseName2;
    public Tag<D2Class_898E8080> UnkEntityReference;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_898E8080
{
    public long FileSize;
    public long Unk08;
    public ResourcePointer Unk10;  // 46938080 has dialogue table, 45938080 unk, 19978080 unk
    public Tag Unk14;  // D2Class_898E8080 entity script stuff
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_46938080
{
    [Tag64]
    public Tag DialogueTable;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_19978080
{
    [Tag64]
    public Tag DialogueTable;
    public TigerHash Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_18978080
{
    [Tag64]
    public Tag DialogueTable;
    public TigerHash Unk10;
    [SchemaField(0x18)]
    public TigerHash Unk18;
    public int Unk1C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_17978080
{
    [Tag64]
    public Tag DialogueTable;
    public TigerHash Unk10;
    [SchemaField(0x18)]
    public TigerHash Unk18;
    public int Unk1C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_45938080
{
    [Tag64]
    public Tag DialogueTable;
    [SchemaField(0x18)]
    public DynamicArray<D2Class_28998080> Unk18;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_44938080
{
    [Tag64]
    public Tag DialogueTable;
    [SchemaField(0x18)]
    public DynamicArray<D2Class_28998080> Unk18;
    [SchemaField(0x3C)]
    public int Unk3C;
    public float Unk40;
    public TigerHash Unk44;
    [SchemaField(0x50)]
    public int Unk50;
}

/// <summary>
/// Generally used in ambients to provide dialogue and music together.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_D5908080
{
    [Tag64]
    public Tag DialogueTable;
    [SchemaField(0x38)]
    public Tag<D2Class_EB458080> Music;
    public int Unk3C;
    public float Unk40;
    public TigerHash Unk44;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_28998080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_1A978080
{
    [Tag64]
    public Tag Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_478F8080
{
    [Tag64]
    public Tag Unk00;
}

/// <summary>
/// Stores static map data for activities
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_24898080
{
    public TigerHash LocationName;
    public TigerHash ActivityName;
    public TigerHash BubbleName;
    [SchemaField(0x10)]
    public ResourcePointer Unk10;  // 0F978080, 53418080
    public DynamicArray<D2Class_48898080> Unk18;
    public DynamicArray<D2Class_1D898080> MapReferences;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_1D898080
{
    [Tag64]
    public Tag<D2Class_1E898080> MapReference;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_53418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_54418080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0xC)]
    public int Unk0C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_0F978080
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x28)]
    public long Unk28;
    public DynamicArray<D2Class_DD978080> Unk30;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_DD978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
}

/// <summary>
/// Directive table + audio links for activity directives.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x84)]
public struct D2Class_6A988080
{
    public DynamicArray<D2Class_28898080> DirectiveTables;
    public DynamicArray<D2Class_B7978080> DialogueTables;
    public TigerHash StartingBubbleName;
    public TigerHash Unk24;
    [SchemaField(0x2C)]
    public Tag<D2Class_EB458080> Music;
}

/// <summary>
/// Directive table for public events so no audio linked.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_20978080
{
    public DynamicArray<D2Class_28898080> PEDirectiveTables;
    [SchemaField(0x20)]
    public TigerHash StartingBubbleName;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_28898080
{
    public Tag<D2Class_C78E8080> DirectiveTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x14)]
public struct D2Class_B7978080
{
    [Tag64]
    public Tag<D2Class_B8978080> DialogueTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C78E8080
{
    public long FileSize;
    public DynamicArray<D2Class_C98E8080> DirectiveTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x80)]
public struct D2Class_C98E8080
{
    public TigerHash Hash;
    public int Unk04;

    [SchemaField(0x10)]
    public StringReference64 NameString;
    [SchemaField(0x28)]
    public StringReference64 DescriptionString;
    [SchemaField(0x40)]
    public StringReference64 ObjectiveString;
    [SchemaField(0x58)]
    public StringReference64 Unk58;
    [SchemaField(0x70)]
    public int ObjectiveTargetCount;
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_0B978080
{
    public StringPointer BubbleName;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x40)]
    public DynamicArray<D2Class_0C008080> Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_0C008080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
}

#region Audio

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_EB458080
{
    public long FileSize;
    public StringPointer MusicTemplateName;
    [Tag64]
    public Tag MusicTemplateTag; // F0458080

    public long Unk20;
    public DynamicArray<D2Class_ED458080> Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_ED458080
{
    public ResourcePointer Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_F5458080
{
    public StringPointer WwiseMusicLoopName;
    [Tag64]
    public WwiseSound MusicLoopSound;
    public DynamicArray<D2Class_FB458080> Unk18;
    public TigerHash Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_F7458080
{
    public StringPointer AmbientMusicSetName;
    [Tag64]
    public Tag<D2Class_50968080> AmbientMusicSet;
    public DynamicArray<D2Class_FA458080> Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_50968080
{
    public long FileSize;
    public DynamicArray<D2Class_318A8080> Unk08;
    public TigerHash Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_318A8080
{
    [Tag64]
    public WwiseSound MusicLoopSound;
    public float Unk10;
    public TigerHash Unk14;
    public float Unk18;
    public TigerHash Unk1C;
    public float Unk20;
    public TigerHash Unk24;
    public int Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_FA458080
{
    public TigerHash Unk00;
    [SchemaField(8)]
    public StringPointer EventName;

    public TigerHash Unk10;  // eventhash? idk
    public TigerHash Unk14;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_FB458080
{
    public TigerHash Unk00;
    [SchemaField(8)]
    public StringPointer EventName;

    public int Unk10;
    public int Unk14;
    public TigerHash EventHash;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_F0458080
{
    public long FileSize;
    public int Unk08;
    public int Unk0C;
    public int WwiseSwitchKey;
}


#endregion
