using Tiger.Schema.Entity;

namespace Tiger.Schema.Animation;

public class Animation : Tag<SAnimationClip>
{
    public Animation(FileHash hash, bool shouldParse = true) : base(hash, shouldParse)
    {
    }

    public void LoadAnimation()
    {
        AnimationHash = _tag.AnimationHash;
        FrameCount = _tag.FrameCount;
        NodeCount = _tag.NodeCount;
        RigControlCount = _tag.RigControlCount;
        RigComponents = _tag.RigComponents;

        RetargetKeys = _tag.Unk150.Select(x => x.StringHash.Hash32).ToArray();
        LayerType = _tag.LayerType;

        IsLayer = _tag.LayerType == LayerType.Additive;
        IsReplacementLayer = _tag.LayerType == LayerType.Replacement;
        IsPoseLayer = _tag.PoseSpace != 0; // unsure

        using TigerReader reader = GetReader();
        Maps = new ControlMaps
        {
            StaticScale = PrimitiveArrays.ReadUInt16(reader, _tag.StaticScaleControlMap),
            StaticRotation = PrimitiveArrays.ReadUInt16(reader, _tag.StaticRotationControlMap),
            StaticTranslation = PrimitiveArrays.ReadUInt16(reader, _tag.StaticTranslationControlMap),
            AnimatedScale = PrimitiveArrays.ReadUInt16(reader, _tag.AnimatedScaleControlMap),
            AnimatedRotation = PrimitiveArrays.ReadUInt16(reader, _tag.AnimatedRotationControlMap),
            AnimatedTranslation = PrimitiveArrays.ReadUInt16(reader, _tag.AnimatedTranslationControlMap),
        };

        Static = ReadStatic(reader);
        Animated = ReadAnimated(reader);
    }

    public TigerHash AnimationHash;
    public uint[] RetargetKeys = Array.Empty<uint>();

    public LayerType LayerType;
    public bool IsLayer;
    public bool IsReplacementLayer;
    public bool IsPoseLayer;

    public int FrameCount;
    public int NodeCount;
    public int RigControlCount;
    public CodecData? Static;
    public CodecData? Animated;
    public ControlMaps Maps = new();
    public List<S80808C5F> RigComponents;

    private CodecData? ReadStatic(TigerReader reader)
    {
        if (_tag.StaticBoneData.GetValue(reader) is S80808B40 staticBoneData)
            return FromPoseQuantized(staticBoneData, reader);
        else if (_tag.StaticBoneData.ResourceClassHash != TigerHash.InvalidHash32)
            throw new Exception($"{Hash}: Should be 0x80808B40 but is {_tag.StaticBoneData.ResourceClassHash:X8}");

        return null;
    }

    private CodecData? ReadAnimated(TigerReader reader)
    {
        if (_tag.AnimatedBoneData.GetValue(reader) is S80808B43 compressed)
            return FromCompressed(compressed, reader);
        else if (_tag.AnimatedBoneData.GetValue(reader) is S80808B42 uncompressed)
            return FromUncompressed(uncompressed, reader);
        else if (_tag.AnimatedBoneData.GetValue(reader) is S80808B40 poseQuantized)
            return FromPoseQuantized(poseQuantized, reader);
        else if (_tag.AnimatedBoneData.ResourceClassHash != TigerHash.InvalidHash32)
            throw new NotImplementedException($"Unknown {_tag.AnimatedBoneData.ResourceClassHash:X8}");

        return null;
    }
    private void CheckCodec(AnimationCodecType actual, AnimationCodecType expected)
    {
        if (actual != expected)
            throw new Exception($"{Hash}: Should be {expected} but is {actual}");
    }

    private CodecData FromPoseQuantized(S80808B40 data, TigerReader reader)
    {
        CheckCodec(data.CodecType, AnimationCodecType.PoseQuantized);
        return new CodecData
        {
            CodecType = data.CodecType,
            ScaleStreamCount = data.ScaleStreamCount,
            RotationStreamCount = data.RotationStreamCount,
            TranslationStreamCount = data.TranslationStreamCount,
            FrameCount = data.FrameCount,
            ScaleExtent = data.ScaleStreamQuantization.Extent,
            ScaleMinimum = data.ScaleStreamQuantization.Minimum,
            TranslationExtent = new[] { data.TranslationStreamQuantization.Extent.X, data.TranslationStreamQuantization.Extent.Y, data.TranslationStreamQuantization.Extent.Z },
            TranslationMinimum = new[] { data.TranslationStreamQuantization.Minimum.X, data.TranslationStreamQuantization.Minimum.Y, data.TranslationStreamQuantization.Minimum.Z },
            StreamData = PrimitiveArrays.ReadUInt16(reader, data.StreamData),
        };
    }

    private CodecData FromUncompressed(S80808B42 data, TigerReader reader)
    {
        CheckCodec(data.CodecType, AnimationCodecType.Uncompressed);
        return new CodecData
        {
            CodecType = data.CodecType,
            ScaleStreamCount = data.ScaleStreamCount,
            RotationStreamCount = data.RotationStreamCount,
            TranslationStreamCount = data.TranslationStreamCount,
            FrameCount = data.FrameCount,
            StreamData = PrimitiveArrays.ReadUInt16(reader, data.StreamData),
            QuantizationExtents = PrimitiveArrays.ReadSingle(reader, data.QuantizationExtents),
            QuantizationMinimums = PrimitiveArrays.ReadSingle(reader, data.QuantizationMinimums),
        };
    }

    private CodecData FromCompressed(S80808B43 data, TigerReader reader)
    {
        CheckCodec(data.CodecType, AnimationCodecType.Compressed);
        return new CodecData
        {
            CodecType = data.CodecType,
            ScaleStreamCount = data.ScaleStreamCount,
            RotationStreamCount = data.RotationStreamCount,
            TranslationStreamCount = data.TranslationStreamCount,
            UncompressedData = PrimitiveArrays.ReadInt16(reader, data.UncompressedData),
            CompressedData = PrimitiveArrays.ReadInt16(reader, data.CompressedData),
            KeyframeDeltas = PrimitiveArrays.ReadByte(reader, data.KeyframeDeltas),
            InterpolationData = PrimitiveArrays.ReadByte(reader, data.InterpolationData),
            QuantizationExtents = PrimitiveArrays.ReadSingle(reader, data.QuantizationExtents),
            QuantizationMinimums = PrimitiveArrays.ReadSingle(reader, data.QuantizationMinimums),
            UnkArray7 = PrimitiveArrays.ReadInt16(reader, data.Unk70),
        };
    }

    // decodes the clip and runs it on the rig
    public BoneAnimation[] Evaluate(
        EntityRuntimeRig rig, EntitySkeleton skeleton,
        bool localSpace = true, RetargetMode mode = RetargetMode.Auto)
    {
        var controls = AnimationDecoder.Decode(this);
        var objectSpace = RigRetarget.Retarget(this, controls, skeleton, rig, mode);
        return localSpace ? RigRetarget.ToLocalSpace(this, objectSpace, skeleton) : objectSpace;
    }

    public FbxAnimationData EvaluateForFbx(
        EntityRuntimeRig rig, EntitySkeleton skeleton, string name, LayerType layerType,
        double frameRate = 30)
    {
        var objectSpace = Evaluate(rig, skeleton, false);
        return FbxAnimationBuilder.Build(name, objectSpace, skeleton, layerType, FrameCount, frameRate);
    }

    public RootMotion? ReadRootMotion()
    {
        using TigerReader reader = GetReader();
        if (_tag.RootMotionTrack.GetValue(reader) is not S80808B42 track)
        {
            if (_tag.RootMotionTrack.ResourceClassHash != TigerHash.InvalidHash32)
                throw new NotSupportedException($"{Hash}: root motion is {_tag.RootMotionTrack.ResourceClassHash:X8}, should be 80808B42");

            return null;
        }

        var data = FromUncompressed(track, reader);
        if (data.ScaleStreamCount != 0 || data.RotationStreamCount != 1 || data.TranslationStreamCount != 1)
            throw new NotSupportedException($"{Hash}: root motion should be one rotation and one translation track");

        if (data.FrameCount != _tag.FrameCount)
            throw new Exception($"{Hash}: root motion has {data.FrameCount} frames, the clip has {_tag.FrameCount}");

        var streams = AnimationDecoder.DecodeStreams(data, _tag.FrameCount);
        var c = _tag.CycleTransform;

        var cycle = new RigTransform(
            System.Numerics.Quaternion.Normalize(new System.Numerics.Quaternion(c.Rot.X, c.Rot.Y, c.Rot.Z, c.Rot.W)),
            new System.Numerics.Vector3(c.Trans.X, c.Trans.Y, c.Trans.Z),
            c.Scale);

        return new RootMotion(_tag.ExtractionMode, cycle, streams.Rotations[0], streams.Translations[0]);
    }
}

public class CodecData
{
    public AnimationCodecType CodecType;
    public int ScaleStreamCount;
    public int RotationStreamCount;
    public int TranslationStreamCount;
    public int FrameCount;
    public float[] QuantizationExtents = Array.Empty<float>();
    public float[] QuantizationMinimums = Array.Empty<float>();
    public short[] UncompressedData = Array.Empty<short>();
    public short[] CompressedData = Array.Empty<short>();
    public byte[] KeyframeDeltas = Array.Empty<byte>();
    public byte[] InterpolationData = Array.Empty<byte>();
    public short[] UnkArray7 = Array.Empty<short>();
    public ushort[] StreamData = Array.Empty<ushort>();
    public float ScaleExtent;
    public float ScaleMinimum;
    public float[] TranslationExtent = new float[3];
    public float[] TranslationMinimum = new float[3];
}

public class ControlMaps
{
    public ushort[] StaticScale = Array.Empty<ushort>();
    public ushort[] StaticRotation = Array.Empty<ushort>();
    public ushort[] StaticTranslation = Array.Empty<ushort>();
    public ushort[] AnimatedScale = Array.Empty<ushort>();
    public ushort[] AnimatedRotation = Array.Empty<ushort>();
    public ushort[] AnimatedTranslation = Array.Empty<ushort>();
}

