using System.Runtime.InteropServices;
using Field.General;
using Field.Models;

namespace Field;

public class Animation : Tag
{
    public D2Class_E08B8080 Header;
    private Dictionary<int, AnimationNode> _nodes;

    public Animation(TagHash hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_E08B8080>();
    }

    /// <summary>
    /// Parsing the tag data into the raw, fully uncompressed data so it can be used by people.
    /// </summary>
    public void Load()
    {
        MakeAnimationNodes();
        ParseStaticData();
        ParseAnimatedData();
        var a = 0;
    }

    private void ParseAnimatedData()
    {
        if (Header.AnimatedBoneData is D2Class_428B8080 uncomp)
        {
            if (uncomp.CodecType != (AnimationCodecType)2)
                throw new NotImplementedException();
            ParseAnimatedUncompressedCodec2(uncomp);
        }
        else if (Header.AnimatedBoneData is D2Class_438B8080 comp)
        {
            if (comp.CodecType != (AnimationCodecType)1)
                throw new NotImplementedException();
            ParseAnimatedCompressedCodec1(comp);
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    private void ParseAnimatedCompressedCodec1(D2Class_438B8080 header)
    {
        throw new NotImplementedException();
    }

    private void ParseAnimatedUncompressedCodec2(D2Class_428B8080 header)
    {
        Dictionary<int, FloatQuantise> scaleQuants = new Dictionary<int, FloatQuantise>();
        Dictionary<int, Float3Quantise> translateQuants = new Dictionary<int, Float3Quantise>();
        Dictionary<int, Float4Quantise> rotateQuants = new Dictionary<int, Float4Quantise>();
        int offset = 0;
        foreach (var index in Header.AnimatedScaleControlMap)
        {
            
            var quant = new FloatQuantise
            {
                Minimum = header.SRTQuantisationMinimums[offset].Value,
                Extent = header.SRTQuantisationExtents[offset].Value
            };
            scaleQuants.Add(index.Value, quant);
            offset++;
        }
        foreach (var index in Header.AnimatedRotationControlMap)
        {
            
            var quant = new Float4Quantise
            {
                Minimum = new Vector4
                (
                    header.SRTQuantisationMinimums[offset].Value,
                    header.SRTQuantisationMinimums[offset+1].Value,
                    header.SRTQuantisationMinimums[offset+2].Value,
                    header.SRTQuantisationMinimums[offset+3].Value
                ),
                Extent = new Vector4
                (
                    header.SRTQuantisationExtents[offset].Value,
                    header.SRTQuantisationExtents[offset+1].Value,
                    header.SRTQuantisationExtents[offset+2].Value,
                    header.SRTQuantisationExtents[offset+3].Value
                ),
            };
            rotateQuants.Add(index.Value, quant);
            offset += 4;
        }
        foreach (var index in Header.AnimatedTranslationControlMap)
        {
            var quant = new Float3Quantise
            {
                Minimum = new Vector3
                (
                    header.SRTQuantisationMinimums[offset].Value,
                    header.SRTQuantisationMinimums[offset+1].Value,
                    header.SRTQuantisationMinimums[offset+2].Value
                ),
                Extent = new Vector3
                (
                    header.SRTQuantisationExtents[offset].Value,
                    header.SRTQuantisationExtents[offset+1].Value,
                    header.SRTQuantisationExtents[offset+2].Value
                ),
            };
            translateQuants.Add(index.Value, quant);
            offset += 3;
        }
        
        using (var stream = new AnimationStream(header.StreamData))
        {
            foreach (var index in Header.AnimatedScaleControlMap)
            {
                for (int i = 0; i < header.FrameCount; i++)
                {
                    var scale = stream.ReadQuantisedFloat(scaleQuants[index.Value]);
                    _nodes[index.Value].ScaleStream.Add(scale);   
                }
            }

            foreach (var index in Header.AnimatedRotationControlMap)
            {
                for (int i = 0; i < header.FrameCount; i++)
                {
                    var rot = stream.ReadQuantisedFloat4(rotateQuants[index.Value]);
                    _nodes[index.Value].RotationStream.Add(rot);
                }
            }

            foreach (var index in Header.AnimatedTranslationControlMap)
            {
                for (int i = 0; i < header.FrameCount; i++)
                {
                    var tra = stream.ReadQuantisedFloat3(translateQuants[index.Value]);
                    _nodes[index.Value].TranslationStream.Add(tra);
                }
            }
        }
    }
    
    /// <summary>
    /// Fills the _nodes dictionary with empties to be filled with good data.
    /// </summary>
    private void MakeAnimationNodes()
    {
        _nodes = new Dictionary<int, AnimationNode>();
        for (var i = 0; i < Header.NodeCount; i++)
        {
            _nodes.Add(i, new AnimationNode(i));
        }
    }
    
    /// <summary>
    /// 1. Converts the data to its true values
    /// 2. Adds the static data to the AnimationNodes dict
    /// </summary>
    private void ParseStaticData()
    {
        var staticHeader = (D2Class_408B8080)Header.StaticBoneData;
        if (staticHeader.CodecType != AnimationCodecType._animation_codec_type_pose_quantized)
            throw new NotImplementedException();

        // As the data is stored as a short array, we use a custom reader.
        using (var stream = new AnimationStream(staticHeader.StreamData))
        {
            foreach (var index in Header.StaticScaleControlMap)
            {
                var scale = stream.ReadQuantisedFloat(staticHeader.ScaleStreamQuantisation);
                _nodes[index.Value].ScaleStream.Add(scale);
            }
            foreach (var index in Header.StaticRotationControlMap)
            {
                var rot = stream.ReadQuantisedFloat4(new FloatQuantise { Minimum = 2, Extent = -1 });
                _nodes[index.Value].RotationStream.Add(rot);
            }
            foreach (var index in Header.StaticTranslationControlMap)
            {
                var tra = stream.ReadQuantisedFloat3(staticHeader.TranslationStreamQuantisation);
                _nodes[index.Value].TranslationStream.Add(tra);
            }
        }
    }
}

/// <summary>
/// It can be assumed that an AnimationNode with 1 frame is a static node.
/// </summary>
public struct AnimationNode
{
    public int NodeIndex = -1;
    // todo these lists should be dicts instead if we want to use multiple threads to maintain frame order
    public List<float> ScaleStream = new List<float>();
    public List<Vector4> RotationStream = new List<Vector4>();
    public List<Vector3> TranslationStream = new List<Vector3>();
    
    public AnimationNode(int nodeIndex)
    {
        NodeIndex = nodeIndex;
    }
}

public class AnimationStream : IDisposable
{
    private List<ushort> _data;
    private int _index;
    
    public AnimationStream(List<D2Class_0A008080> ushortStream)
    {
        _data = ushortStream.Select(x => x.Value).ToList();
        _index = 0;
    }

    public float ReadFloat()
    {
        float value = _data[_index++] / 65535f;
        return value;
    }

    public float ReadQuantisedFloat(FloatQuantise quantisation)
    {
        float value = ReadFloat();
        float corrected = value * quantisation.Minimum + quantisation.Extent;
        return corrected;
    }

    public void Dispose()
    {
        _data.Clear();
        _index = 0;
    }

    public Vector4 ReadFloat4()
    {
        return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
    }
    
    public Vector3 ReadFloat3()
    {
        return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
    }
    
    public Vector3 ReadQuantisedFloat3(Float3Quantise quantisation)
    {
        var vec = ReadFloat3();
        var corrected = new Vector3(
            vec.X * quantisation.Minimum.X + quantisation.Extent.X,
            vec.Y * quantisation.Minimum.Y + quantisation.Extent.Y,
            vec.Z * quantisation.Minimum.Z + quantisation.Extent.Z);
        return corrected;
    }

    public Vector4 ReadQuantisedFloat4(FloatQuantise quantisation)
    {
        var vec = ReadFloat4();
        var corrected = new Vector4(
            vec.X * quantisation.Minimum + quantisation.Extent,
            vec.Y * quantisation.Minimum + quantisation.Extent,
            vec.Z * quantisation.Minimum + quantisation.Extent,
            vec.W * quantisation.Minimum + quantisation.Extent);
        return corrected;
    }
    
    public Vector4 ReadQuantisedFloat4(Float4Quantise quantisation)
    {
        var vec = ReadFloat4();
        var corrected = new Vector4(
            vec.X * quantisation.Minimum.X + quantisation.Extent.X,
            vec.Y * quantisation.Minimum.Y + quantisation.Extent.Y,
            vec.Z * quantisation.Minimum.Z + quantisation.Extent.Z,
            vec.W * quantisation.Minimum.W + quantisation.Extent.W);
        return corrected;
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
    public ushort Value;
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

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct Float4Quantise
{
    public Vector4 Minimum;
    public Vector4 Extent;
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