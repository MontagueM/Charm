﻿using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "2E058080", 0x28)]
public struct SActivity_ROI
{
    public long FileSize;
    public Tag<S36068080> LocationNames;
    [SchemaField(0x10)]
    public DynamicArray<S0A418080> Bubbles;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0A418080", 0x4)]
public struct S0A418080
{
    public Tag<SBubbleDefinition> ChildMapReference;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "36068080", 0x38)]
public struct S36068080
{
    public long FileSize;
    public StringPointer ActivityDevName;
    public StringHash Unk10; // unsure if string hash
    [SchemaField(0x18)]
    public DynamicArray<SDB068080> BubbleNames;
    public DynamicArray<S7D088080> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DB068080", 0x18)]
public struct SDB068080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    public StringHash BubbleName;
    public short Unk0C;
    public short ThisIndex;
    public int BubbleIndex;  // index to S0A418080 in SActivity_ROI
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "7D088080", 0xAC)]
public struct S7D088080
{
    public StringHash BubbleName;
    //[SchemaField(0x18)]
    //public DynamicArray<S42048080> Unk18;

    // Bunch of Vec4s for some reason
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "42048080", 2)]
public struct S42048080
{
    public short Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "16068080", 0x68)]
public struct SUnkActivity_ROI
{
    public long FileSize;
    public TigerHash LocationName;  // these all have actual string hashes but have no string container given directly
    [SchemaField(0x18)]
    public uint Unk18;
    public TigerHash Unk1C;
    public TigerHash Unk20;
    public TigerHash Unk24;
    public LocalizedStrings LocalizedStrings;
    [SchemaField(0x30)]
    public StringPointer ActivityDevName;
    //[SchemaField(0x48)]
    //public DynamicArray<S0C068080> Unk48;
    //[SchemaField(0x58)]
    //public DynamicArray<S3F078080> Unk58;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "3F078080", 0x8)]
public struct S3F078080
{
    public StringHash Unk00;
    public int Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "0C068080", 0x18)]
public struct S0C068080
{
    public StringHash LocationName;
    [SchemaField(0x08)]
    public DynamicArray<SA8068080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "A8068080", 0x3C)]
public struct SA8068080
{
    //public uint Unk00;
    //public StringHash UnkName0;
    //public uint Unk0C;
    //[SchemaField(0x1C)]
    //public uint Unk1C;
    //public uint Unk20;
    //[SchemaField(0x40)]
    //public StringHash UnkName1;
    //public Tag Unk44;
    //public uint Unk48;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "48018080", 0x28)]
public struct S48018080
{
    public long FileSize;
    [SchemaField(0x10)]
    public Tag<S48018080_Child> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x28)]
public struct S48018080_Child
{
    public long FileSize;
    public StringHash ActivityDevName;
    [SchemaField(0x10)]
    public DynamicArray<S14008080> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "14008080", 0x4)]
public struct S14008080
{
    public FileHash Unk00; // Can be SUnkActivity_ROI or something else (Based on tag type?)
}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "60928080", 0x4)]
//public struct S60928080
//{
//    public Tag<S62948080> Unk00;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "62948080", 0x58)]
//public struct S62948080
//{
//    public long FileSize;
//    public TigerHash Unk08; //BubbleName?
//    public TigerHash Unk0C; //ActivityPhaseName?

//    [SchemaField(0x38)]
//    public DynamicArray<S64948080> Unk38;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "64948080", 0x1C)]
//public struct S64948080
//{
//    [SchemaField(0x8)]
//    public DynamicArray<S66948080> Unk08;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "66948080", 0x4)]
//public struct S66948080
//{
//    public Tag<S68948080> Unk00;
//}

//[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "68948080", 0x20)]
//public struct S68948080
//{
//    public long FileSize;
//    public Tag<SMapDataTable> DataTable;
//}

