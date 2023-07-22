using System.Runtime.InteropServices;

namespace Tiger.Audio;

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_B8978080
{
    public long FileSize;
    public DynamicArray<D2Class_28978080> Unk08;
    public DynamicArray<D2Class_29978080> Unk18;
}

[StructLayout(LayoutKind.Sequential, Size = 8)]
public struct D2Class_28978080
{
    public TigerHash Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_29978080
{
    public TigerHash Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Unk08;
}

/// <summary>
/// Group of D2Class_33978080, used for accessing random sounds to play out of a bundle.
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x48)]
public struct D2Class_2F978080
{
    [SchemaField(0x10), Tag64]
    public Tag Unk10;
    //[SchemaField(0x1C)]
    public TigerHash Unk20;
    public TigerHash Unk24;
    public TigerHash Unk28;
    public TigerHash Unk2C;
    [SchemaField(0x38)]
    public float Unk38;
    [SchemaField(0x40)]
    public ResourcePointer Unk40; // 2A978080, 2D978080
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_2A978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public long Unk10;
    public long Unk18;
    public TigerHash Unk20;
    [SchemaField(0x28)]
    public DynamicArray<D2Class_2F978080> Unk28;
}

[StructLayout(LayoutKind.Sequential, Size = 0x88)]
public struct D2Class_33978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    [SchemaField(0x18), Tag64]
    public WwiseSound Sound1;
    public StringReference64 Unk28;
    [SchemaField(0x40)]
    public float Unk40;
    public Tag Unk44;
    [SchemaField(0x48), Tag64]
    public WwiseSound Sound2;
    public StringReference64 Unk58;
    [SchemaField(0x70)]
    public float Unk70;
    public Tag Unk74;
    public TigerHash Unk78;
    public TigerHash NarratorString;
    public float Unk80;
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public struct D2Class_2D978080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public Tag Unk08;
    public TigerHash Unk0C;
    public float Unk10;
    [SchemaField(0x18)]
    public TigerHash Unk18;
    [SchemaField(0x20)]
    public DynamicArray<D2Class_30978080> Unk20;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_30978080
{
    [Tag64]
    public Tag Unk00;
    public TigerHash Unk10;
    public TigerHash Unk14;
    public TigerHash Unk18;
    public TigerHash Unk1C;
    public ResourcePointer Unk20; //33978080 or 2A978080
}

[StructLayout(LayoutKind.Sequential, Size = 0x38)]
public struct D2Class_38978080
{
    public long FileSize;
    public TigerHash Unk08;
    public TigerHash Unk0C;
    public TigerHash Unk10;
    [SchemaField(0x14)]
    public Tag<D2Class_418A8080> Unk14;
    public Tag<D2Class_63838080> Unk18;
    [SchemaField(0x20)]
    public DynamicArray<FileHash> Unk20;
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
    public BKHD SoundBank;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_438A8080
{
    public long FileSize;
}
