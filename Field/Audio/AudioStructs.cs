using System.Runtime.InteropServices;
using Field.General;

namespace Field;

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_B8978080
{
    public long FileSize;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_28978080> Unk08;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_29978080> Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_28978080
{
    public DestinyHash Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_29978080
{
    public DestinyHash Unk00;
    [DestinyOffset(0x8), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk08;
}

/// <summary>
/// Group of D2Class_33978080, used for accessing random sounds to play out of a bundle.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x48)]
public struct D2Class_2F978080
{
    [DestinyOffset(0x10), DestinyField(FieldType.TagHash64)]
    public Tag Unk10;
    //[DestinyOffset(0x1C)] 
    public DestinyHash Unk20;
    public DestinyHash Unk24;
    public DestinyHash Unk28;
    public DestinyHash Unk2C;
    [DestinyOffset(0x38)]
    public float Unk38;
    [DestinyOffset(0x40), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk40; // 2A978080, 2D978080
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_2A978080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
    public DestinyHash Unk08;
    public DestinyHash Unk0C;
    public long Unk10;
    public long Unk18;
    public DestinyHash Unk20;
    [DestinyOffset(0x28), DestinyField(FieldType.TablePointer)]
    public List<D2Class_2F978080> Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x88)]
public struct D2Class_33978080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
    public DestinyHash Unk08;
    public DestinyHash Unk0C;
    [DestinyOffset(0x18), DestinyField(FieldType.TagHash64)]
    public WwiseSound Sound1;
    [DestinyField(FieldType.String64)]
    public string Unk28;
    [DestinyOffset(0x40)] 
    public float Unk40;
    [DestinyField(FieldType.TagHash)]
    public Tag Unk44;
    [DestinyOffset(0x48), DestinyField(FieldType.TagHash64)]
    public WwiseSound Sound2;
    [DestinyField(FieldType.String64)]
    public string Unk58;
    [DestinyOffset(0x70)] 
    public float Unk70;
    [DestinyField(FieldType.TagHash)]
    public Tag Unk74;
    public DestinyHash Unk78;
    public DestinyHash NarratorString;
    public float Unk80;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_2D978080
{
    public DestinyHash Unk00;
    public DestinyHash Unk04;
    [DestinyField(FieldType.TagHash)]
    public Tag Unk08;
    public DestinyHash Unk0C;
    public float Unk10;
    [DestinyOffset(0x18)]
    public DestinyHash Unk18;
    [DestinyOffset(0x20), DestinyField(FieldType.TablePointer)]
    public List<D2Class_30978080> Unk20;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_30978080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
    public DestinyHash Unk10;
    public DestinyHash Unk14;
    public DestinyHash Unk18;
    public DestinyHash Unk1C;
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk20; //33978080 or 2A978080
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_38978080
{
    public long FileSize;
    public DestinyHash Unk08;
    public DestinyHash Unk0C;
    public DestinyHash Unk10;
    [DestinyOffset(0x14), DestinyField(FieldType.TagHash)]
    public Tag<D2Class_418A8080> Unk14;
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_63838080> Unk18;
    [DestinyOffset(0x20), DestinyField(FieldType.TablePointer)]
    public List<Wem> Unk20;
    [DestinyField(FieldType.TagHash)]
    public Tag<D2Class_438A8080> Unk30;
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_418A8080
{
    public long Unk00;
    public float Unk08;
}

[StructLayout(LayoutKind.Sequential, Size = 4)]
public struct D2Class_63838080
{
    [DestinyField(FieldType.TagHash)]
    public BKHD SoundBank;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_438A8080
{
    public long FileSize;
}
