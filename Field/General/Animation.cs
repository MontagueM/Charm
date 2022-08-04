using System.Runtime.InteropServices;
using Field.General;
using Newtonsoft.Json;
using SharpDX;
using Vector3 = Field.Models.Vector3;
using Vector4 = Field.Models.Vector4;

namespace Field;

/// Animations require a skeleton to work correctly, but these are given above. Therefore, we
/// cannot allow the direct search of animations and instead need to understand the interfaces
/// of the animations so we can pair it with skeletons correctly.


/// <summary>
/// 
/// </summary>
public class Animation : Tag
{
    public D2Class_E08B8080 Header;
    private Dictionary<int, AnimationNode> _nodes = null;
    public List<AnimationTrack> Tracks = null;
    public static readonly int FrameRate = 30;
    public Vector3 tra;
    public Vector3 rot;
    public Vector3 flipTra;
    public Vector3 flipRot;
    public string[] traXYZ = { "X", "Y", "Z" };
    public string[] rotXYZ = { "X", "Y", "Z" };

    public Animation(TagHash hash) : base(hash)
    {
    }
    
    public Animation(TagHash hash, bool disableLoad) : base(hash, disableLoad)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_E08B8080>();
    }
    
    public void ParseTag()
    {
        Parse();
    }

    /// <summary>
    /// Parsing the tag data into the raw, fully uncompressed data so it can be used by people.
    /// </summary>
    public void Load()
    {
        if (_nodes != null)
            return;
        MakeAnimationNodes();
        ParseStaticData();
        ParseAnimatedData();
        MakeAnimationTracks();
        var a = 0;
    }

    private void MakeAnimationTracks()
    {
        Tracks = new List<AnimationTrack>();
        foreach (var node in _nodes)
        {
            var track = new AnimationTrack(Header.FrameCount, node.Value);
            Tracks.Add(track);
        }
    }

    private void ParseAnimatedData()
    {
        if ((object)Header.AnimatedBoneData == null)
            return;
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

    public void SaveToFile(string savePath)
    {
        AnimationHeaderJson obj = new AnimationHeaderJson();
        obj.frame_count = Header.FrameCount;
        obj.duration_in_frames = Header.FrameCount - 1;
        obj.node_count = Header.NodeCount;
        obj.rig_control_count = Header.RigControlCount;
        obj.static_bone_data = new AnimationBoneDataJson();
        obj.animated_bone_data = new AnimationBoneDataJson();
        obj.static_bone_data.transform_stream_header = new TransformStreamHeaderJson();
        var staticHeader = (D2Class_408B8080)Header.StaticBoneData;
        obj.static_bone_data.transform_stream_header.scale_stream_count = staticHeader.ScaleStreamCount;
        obj.static_bone_data.transform_stream_header.rotation_stream_count = staticHeader.RotationStreamCount;
        obj.static_bone_data.transform_stream_header.translation_stream_count = staticHeader.TranslationStreamCount;
        // obj.static_bone_data.transform_stream_header.streams = new StreamsJson();
        // obj.static_bone_data.transform_stream_header.streams.frame_count = staticHeader.FrameCount;
        // obj.static_bone_data.transform_stream_header.streams.frames = new List<FrameJson>();
        obj.animated_bone_data.transform_stream_header = new TransformStreamHeaderJson();
        var animatedHeader = (D2Class_428B8080)Header.AnimatedBoneData;
        obj.animated_bone_data.transform_stream_header.scale_stream_count = animatedHeader.ScaleStreamCount;
        obj.animated_bone_data.transform_stream_header.rotation_stream_count = animatedHeader.RotationStreamCount;
        obj.animated_bone_data.transform_stream_header.translation_stream_count = animatedHeader.TranslationStreamCount;
        // obj.animated_bone_data.transform_stream_header.streams = new StreamsJson();
        // obj.animated_bone_data.transform_stream_header.streams.frame_count = animatedHeader.FrameCount;
        // obj.animated_bone_data.transform_stream_header.streams.frames = new List<FrameJson>();

        obj.bone_data_frames = new Dictionary<int, FrameJson>();
            
        foreach (var track in Tracks)
        {
            obj.bone_data_frames.Add(track.TrackIndex, new FrameJson
            {
                scales = track.TrackScales,
                rotations = track.TrackRotations,
                translations = track.TrackTranslations
            });
        }
        
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(new StreamWriter(savePath), obj);
    }

    private struct AnimationHeaderJson
    {
        public int frame_count;
        public int duration_in_frames;
        public int node_count;
        public int rig_control_count;
        public AnimationBoneDataJson static_bone_data;
        public AnimationBoneDataJson animated_bone_data;
        public Dictionary<int, FrameJson> bone_data_frames; // not correct but want it here
    }

    private struct AnimationBoneDataJson
    {
        public TransformStreamHeaderJson transform_stream_header;
        public List<int> scale_control_map;
        public List<int> rotation_control_map;
        public List<int> translation_control_map;
    }

    private struct TransformStreamHeaderJson
    {
        public int scale_stream_count;
        public int rotation_stream_count;
        public int translation_stream_count;
        public StreamsJson streams;
    }

    private struct StreamsJson
    {
        public int frame_count;
        public List<FrameJson> frames;
    }

    private struct FrameJson
    {
        public List<float> scales;
        public List<Vector3> rotations;
        public List<Vector3> translations;
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

/// <summary>
/// An animation track represents the full animation of a single node.
/// </summary>
public struct AnimationTrack
{
    public int TrackIndex = -1;
    public List<float> TrackTimes = new List<float>();
    public List<float> TrackScales = new List<float>();
    public List<Vector3> TrackRotations = new List<Vector3>();
    public List<Vector3> TrackTranslations = new List<Vector3>();
    
    public AnimationTrack(int frameCount, AnimationNode animationNode)
    {
        TrackIndex = animationNode.NodeIndex;

        for (int i = 0; i < frameCount; i++)
        {
            TrackTimes.Add((float)i / Animation.FrameRate);
        }
        
        if (animationNode.ScaleStream.Count == 1) // static
        {
            TrackScales = TrackTimes.Select(x => animationNode.ScaleStream[0]).ToList();
        }
        else if (animationNode.ScaleStream.Count == 0)
        {
            throw new NotImplementedException();
        }
        else
        {
            TrackScales = animationNode.ScaleStream.ToList();
        }
        
        if (animationNode.RotationStream.Count == 1) // static
        {
            TrackRotations = TrackTimes.Select(x => animationNode.RotationStream[0].QuaternionToEulerAnglesZYX()).ToList();
        }
        else if (animationNode.RotationStream.Count == 0)
        {
            throw new NotImplementedException();
        }
        else
        {
            TrackRotations = animationNode.RotationStream.Select(x => x.QuaternionToEulerAnglesZYX()).ToList();
        }
        // testing reassignment
        // TrackRotations = TrackRotations.Select(x => new Vector3(-x.Z, -x.X, x.Y)).ToList();
        // xyz
        // TrackRotations = TrackRotations.Select(x => new Vector3(x.X, x.Y, x.Z)).ToList();
        
        if (animationNode.TranslationStream.Count == 1) // static
        {
            TrackTranslations = TrackTimes.Select(x => animationNode.TranslationStream[0]).ToList();
        }
        else if (animationNode.TranslationStream.Count == 0)
        {
            throw new NotImplementedException();
        }
        else
        {
            TrackTranslations = animationNode.TranslationStream.ToList();
        }
        // TrackTranslations = TrackTranslations.Select(x => new Vector3(-x.Z, -x.X, x.Y)).ToList();
        // tried xyz, xzy
        // TrackTranslations = TrackTranslations.Select(x => new Vector3(x.Z, x.X, x.Y)).ToList();
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
        if (Math.Round(corrected.Magnitude, 4) != 1)
            throw new Exception("Quaternion magnitude is not 1");
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
    public dynamic? Unk20;  // 4c8b8080, 4b8b8080
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk28;  // 488b8080, 468B8080
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

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_4B8B8080
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(0xC)]
    public int FrameCount;
    public float Unk10;
    public float Unk14;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> Unk18;
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

[StructLayout(LayoutKind.Sequential, Size = 0x40)]
public struct D2Class_468B8080
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(0xC)] 
    public int Unk0C;
    public float Unk10;
    public float Unk14;
    public float Unk18;
    public float Unk1C;
    [DestinyOffset(0x30), DestinyField(FieldType.TablePointer)]
    public List<D2Class_0A008080> Unk30;
}

#endregion

#region EOF Resources

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct D2Class_088C8080
{
    [DestinyField(FieldType.ResourcePointer)]
    public dynamic? Unk00;  // 0a/11/12/13/1a8c8080
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

[StructLayout(LayoutKind.Sequential, Size = 6)]
public struct D2Class_1A8C8080
{
    public short Unk00;
    public short Unk02;
    public short Unk04;
}

[StructLayout(LayoutKind.Sequential, Size = 0x28)]
public struct D2Class_0A8C8080
{
    public short Unk00;
    public short Unk02;
    [DestinyOffset(8), DestinyField(FieldType.RelativePointer)] 
    public string ResourceName;
    [DestinyField(FieldType.TagHash64)]
    public Tag Resource;  // object_behaviours.tft
}

#endregion