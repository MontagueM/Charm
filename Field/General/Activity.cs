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
		// Getting the string container
		StringContainer sc;
		using (var handle = GetHandle())
		{
			handle.BaseStream.Seek(0x28, SeekOrigin.Begin);
			var tag = PackageHandler.GetTag<D2Class_8B8E8080>(new TagHash(handle.ReadUInt64()));
			sc = tag.Header.StringContainer;
		}
		Header = ReadHeader<D2Class_8E8E8080>(sc);
	}
}

[StructLayout(LayoutKind.Sequential, Size = 0x78)]
public struct D2Class_8E8E8080
{
	public long FileSize;
	public DestinyHash LocationName;  // these all have actual string hashes but have no string container given directly
	public DestinyHash Unk0C;
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

[StructLayout(LayoutKind.Sequential, Size = 0x78)]
public struct D2Class_8B8E8080
{
	public long FileSize;
	public DestinyHash LocationName;
	[DestinyOffset(0x10), DestinyField(FieldType.TagHash64)]
	public StringContainer StringContainer;
	[DestinyField(FieldType.TagHash)]
	public Tag Events;
	[DestinyField(FieldType.TagHash)]
	public Tag Patrols;
	public uint Unk28;
	[DestinyField(FieldType.TagHash)]
	public Tag Unk2C;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_DE448080> TagBags;
	[DestinyOffset(0x48), DestinyField(FieldType.TablePointer)]
	public List<D2Class_2E898080> Activities;
	[DestinyField(FieldType.RelativePointer)]
	public string DestinationName;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_DE448080
{
	[DestinyField(FieldType.TagHash)]
	public Tag Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_2E898080
{
	public DestinyHash ShortActivityName;
	[DestinyOffset(0x8)]
	public DestinyHash Unk08;
	public DestinyHash Unk10;
	[DestinyField(FieldType.RelativePointer)]
	public string ActivityName;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
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
	[DestinyOffset(0x38), DestinyField(FieldType.TablePointer)]
	public List<D2Class_48898080> Unk38;
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
	public dynamic? Unk10;  // 46938080 has dialogue table, 45938080 unk, 19978080 unk
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

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_19978080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;

	public DestinyHash Unk10;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_18978080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;

	public DestinyHash Unk10;
	[DestinyOffset(0x18)]
	public DestinyHash Unk18;
	public int Unk1C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_17978080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;

	public DestinyHash Unk10;
	[DestinyOffset(0x18)]
	public DestinyHash Unk18;
	public int Unk1C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_45938080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;
	[DestinyOffset(0x18), DestinyField(FieldType.TablePointer)]
	public List<D2Class_28998080> Unk18;
	[DestinyOffset(0x3C)]
	public int Unk3C;
	public float Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
public struct D2Class_44938080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;
	[DestinyOffset(0x18), DestinyField(FieldType.TablePointer)]
	public List<D2Class_28998080> Unk18;
	[DestinyOffset(0x3C)]
	public int Unk3C;
	public float Unk40;
	public DestinyHash Unk44;
	[DestinyOffset(0x50)]
	public int Unk50;
}

/// <summary>
/// Generally used in ambients to provide dialogue and music together.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_D5908080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag DialogueTable;
	[DestinyOffset(0x38), DestinyField(FieldType.TagHash)]
	public Tag<D2Class_EB458080> Music;
	public int Unk3C;
	public float Unk40;
	public DestinyHash Unk44;
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
	public dynamic? Unk10;  // 0F978080, 53418080
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_48898080> Unk18;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_1D898080> MapReferences;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_1D898080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag<D2Class_1E898080> MapReference;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_53418080
{
	public DestinyHash Unk00;
	public DestinyHash Unk04;
	[DestinyOffset(0xC)]
	public int Unk0C;
}

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_54418080
{
	public DestinyHash Unk00;
	public DestinyHash Unk04;
	[DestinyOffset(0xC)]
	public int Unk0C;
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
[StructLayout(LayoutKind.Sequential, Size = 0x84)]
public struct D2Class_6A988080
{
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_28898080> DirectiveTables;
	[DestinyField(FieldType.TagHash64)]
	public Tag<D2Class_B8978080> DialogueTable;
	public DestinyHash StartingBubbleName;
	public DestinyHash Unk24;
	[DestinyOffset(0x2C), DestinyField(FieldType.TagHash)]
	public Tag<D2Class_EB458080> Music;

}

[StructLayout(LayoutKind.Sequential, Size = 0x14)]
public struct D2Class_B7978080
{
	[DestinyField(FieldType.TagHash64)]
	public Tag<D2Class_B8978080> DialogueTable;
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
	public Tag<D2Class_C78E8080> DirectiveTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_C78E8080
{
	public long FileSize;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_C98E8080> DirectiveTable;
}

[StructLayout(LayoutKind.Sequential, Size = 0x80)]
public struct D2Class_C98E8080
{
	public DestinyHash Hash;
	public int Unk04;

	[DestinyOffset(0x10), DestinyField(FieldType.String64)]
	public string NameString;
	[DestinyOffset(0x28), DestinyField(FieldType.String64)]
	public string DescriptionString;
	[DestinyOffset(0x40), DestinyField(FieldType.String64)]
	public string ObjectiveString;
	[DestinyOffset(0x58), DestinyField(FieldType.String64)]
	public string Unk58;
	[DestinyOffset(0x70)]
	public int ObjectiveTargetCount;
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

#region Audio

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_EB458080
{
	public long FileSize;
	[DestinyField(FieldType.RelativePointer)]
	public string MusicTemplateName;
	[DestinyField(FieldType.TagHash64)]
	public Tag MusicTemplateTag; // F0458080

	public long Unk20;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_ED458080> Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_ED458080
{
	[DestinyField(FieldType.ResourcePointer)]
	public dynamic? Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_F5458080
{
	[DestinyField(FieldType.RelativePointer)]
	public string WwiseMusicLoopName;
	[DestinyField(FieldType.TagHash64)]
	public WwiseSound MusicLoopSound;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_FB458080> Unk18;

	public DestinyHash Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_F7458080
{
	[DestinyField(FieldType.RelativePointer)]
	public string AmbientMusicSetName;
	[DestinyField(FieldType.TagHash64)]
	public Tag<D2Class_50968080> AmbientMusicSet;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_FA458080> Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_50968080
{
	public long FileSize;
	[DestinyField(FieldType.TablePointer)]
	public List<D2Class_318A8080> Unk08;
	public DestinyHash Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_318A8080
{
	[DestinyField(FieldType.TagHash64)]
	public WwiseSound MusicLoopSound;

	public float Unk10;
	public DestinyHash Unk14;
	public float Unk18;
	public DestinyHash Unk1C;
	public float Unk20;
	public DestinyHash Unk24;
	public int Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_FA458080
{
	public DestinyHash Unk00;
	[DestinyOffset(8), DestinyField(FieldType.RelativePointer)]
	public string EventName;

	public DestinyHash Unk10;  // eventhash? idk
	public DestinyHash Unk14;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct D2Class_FB458080
{
	public DestinyHash Unk00;
	[DestinyOffset(8), DestinyField(FieldType.RelativePointer)]
	public string EventName;

	public int Unk10;
	public int Unk14;
	public DestinyHash EventHash;
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