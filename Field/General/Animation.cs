using System.Runtime.InteropServices;
using Field.General;
using Field.Models;

namespace Field;

public class Animation : Tag
{
    public D2Class_E08B8080 Header;
    
    public Animation(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_E08B8080>();
    }

    public void Load()
    {
        var b = Header;
        var a = 0;
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x190)]
public struct D2Class_E08B8080
{
    public long FileSize;

    [DestinyOffset(0x10), DestinyField(FieldType.ResourcePointer)]
    public dynamic? StaticBoneData;  // 408b8080
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? AnimatedBoneData;  // 438b8080 compressed, 428b8080 uncompressed
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk20;  // 4c8b8080
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk28;  // 488b8080
    [DestinyOffset(0x60), DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk60;  // 428b8080 if compressed
    [DestinyOffset(0x70)]
    public Vector4 Unk70;
    public Vector4 Unk80;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_70008080> Unk90;
    public int UnkA0;
    public int UnkA4;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> StaticScaleControlMap;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> StaticRotationControlMap;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> StaticTranslationControlMap;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> AnimatedScaleControlMap;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> AnimatedRotationControlMap;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> AnimatedTranslationControlMap;
    [DestinyOffset(0x118)] 
    public int Unk118;
    public int Unk11C;
    public DestinyHash AnimationHash;
    public DestinyHash Unk124;
    public DestinyHash Unk128;
    public int Unk12C;
    public int Unk130;
    [DestinyOffset(0x140)]
    public short FrameCount;
    public short NodeCount;
    public short RigControlCount;
    public short Unk144;
    [DestinyOffset(0x150), DestinyField(FieldType.TablePointer)]
    public List<D2Class_70008080> Unk150;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_088C8080> Unk160;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_5F8C8080> Unk170;
}

#region General

[StructLayout(LayoutKind.Sequential, Size = 2)]
public struct D2Class_0A008080
{
    public short Index;
}

public enum AnimationCodecType : short
{
    _animation_codec_type_raw = 0,
    _animation_codec_type_pose_quantized = 3,
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct FloatQuantise
{
    public float Minimum;
    public float Extent;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct Float3Quantise
{
    public Vector3 Minimum;
    public Vector3 Extent;
}

/// <summary>
/// "static_bone_data"
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x68)]
public struct D2Class_408B8080
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;
    [DestinyOffset(0x10)] 
    public int FrameCount;
    public FloatQuantise ScaleStreamQuantisation;
    public Float3Quantise TranslationStreamQuantisation;
    [DestinyOffset(0x38), DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> StreamData;
}

/// <summary>
/// "animated_bone_data"
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x48)]
public struct D2Class_428B8080
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;
    [DestinyOffset(0x10)] 
    public int FrameCount;
    [DestinyOffset(0x18), DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> StreamData;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> SRTQuantisationMinimums;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> SRTQuantisationExtents;
}

/// <summary>
/// "animated_bone_data"
/// </summary>
[StructLayout(LayoutKind.Sequential, Size = 0x80)]
public struct D2Class_438B8080
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;
    public float ErrorValue;  // guess
    public float CompressionRate;  // guess
    [DestinyOffset(0x20), DestinyField(FieldType.TablePointer)]
    public List<D2Class_06008080> Unk20;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk30;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk40;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> Unk50;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> Unk60;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_06008080> Unk70;
}

[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public struct D2Class_4C8B8080 // basically same frame/quantisation stuff
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(0xC)]
    public int FrameCount;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_528B8080> Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> Unk20;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> Unk30;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0F008080> Unk40;
}

[StructLayout(LayoutKind.Sequential, Size = 2)]
public struct D2Class_528B8080
{
    public short Unk00;
}

[StructLayout(LayoutKind.Sequential, Size = 0x60)]
public struct D2Class_488B8080
{
    public short Unk00;
    public short Unk02;
    public float Unk04;
    public float Unk08;
    [DestinyOffset(0x10), DestinyField(FieldType.TablePointer)]
    public List<D2Class_06008080> Unk10;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_06008080> Unk20;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk30;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk40;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_06008080> Unk50;
}

#endregion

#region EOF Resources

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct D2Class_088C8080
{
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk00;  // 11/12/138c8080
}

[StructLayout(LayoutKind.Sequential, Size = 0xc)]
public struct D2Class_118C8080
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(0x8)]
    public DestinyHash Unk08;
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct D2Class_128C8080
{
    public short Unk00;
    public short Unk02;
    public DestinyHash Unk04;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_138C8080
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(0x8)]
    public DestinyHash Unk08;
    [DestinyOffset(0x10), DestinyField(FieldType.RelativePointer)] 
    public string ResourceName;
    [DestinyField(FieldType.TagHash64)]
    public Tag Resource;  // can be WwiseSound (wwise_event) or Entity (pattern tft)
}

#endregion