using Arithmic;
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
        NodeCount = _tag.GetNodeCount();
        RigControlCount = _tag.GetRigControlCount();
        RigComponents = _tag.RigComponents;

        RetargetKeys = _tag.Unk150.Select(x => x.StringHash.Hash32).ToArray();
        LayerType = _tag.LayerType;

        IsLayer = _tag.LayerType == LayerType.Additive;
        IsReplacementLayer = _tag.LayerType == LayerType.Replacement;
        IsPoseLayer = _tag.PoseSpace != 0; // unsure

        using TigerReader reader = GetReader();

        if (Strategy.IsD1())
        {
            var controlMaps = _tag.ControlMapsD1.Value;
            Maps = new ControlMaps
            {
                StaticScale = PrimitiveArrays.ReadByte(reader, controlMaps.StaticScaleControlMap).Select(value => (ushort)value).ToArray(),
                StaticRotation = PrimitiveArrays.ReadByte(reader, controlMaps.StaticRotationControlMap).Select(value => (ushort)value).ToArray(),
                StaticTranslation = PrimitiveArrays.ReadByte(reader, controlMaps.StaticTranslationControlMap).Select(value => (ushort)value).ToArray(),
                AnimatedScale = PrimitiveArrays.ReadByte(reader, controlMaps.AnimatedScaleControlMap).Select(value => (ushort)value).ToArray(),
                AnimatedRotation = PrimitiveArrays.ReadByte(reader, controlMaps.AnimatedRotationControlMap).Select(value => (ushort)value).ToArray(),
                AnimatedTranslation = PrimitiveArrays.ReadByte(reader, controlMaps.AnimatedTranslationControlMap).Select(value => (ushort)value).ToArray(),
            };
        }
        else
        {
            var controlMaps = _tag.ControlMapsD2.Value;
            Maps = new ControlMaps
            {
                StaticScale = PrimitiveArrays.ReadUInt16(reader, controlMaps.StaticScaleControlMap),
                StaticRotation = PrimitiveArrays.ReadUInt16(reader, controlMaps.StaticRotationControlMap),
                StaticTranslation = PrimitiveArrays.ReadUInt16(reader, controlMaps.StaticTranslationControlMap),
                AnimatedScale = PrimitiveArrays.ReadUInt16(reader, controlMaps.AnimatedScaleControlMap),
                AnimatedRotation = PrimitiveArrays.ReadUInt16(reader, controlMaps.AnimatedRotationControlMap),
                AnimatedTranslation = PrimitiveArrays.ReadUInt16(reader, controlMaps.AnimatedTranslationControlMap),
            };
        }


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
        {
            var classId = Helpers.GetClassHashForStrategy(typeof(S80808B40), Strategy.CurrentStrategy);
            throw new Exception($"{Hash}: Should be 0x{classId:X8} but is {_tag.StaticBoneData.ResourceClassHash:X8}");
        }

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
        else if (_tag.AnimatedBoneData.GetValue(reader) is S80808B44 raw)
            return FromRaw(raw, reader);
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

    private CodecData FromRaw(S80808B44 data, TigerReader reader)
    {
        CheckCodec(data.CodecType, AnimationCodecType.Raw);
        return new CodecData
        {
            CodecType = data.CodecType,
            ScaleStreamCount = data.ScaleStreamCount,
            RotationStreamCount = data.RotationStreamCount,
            TranslationStreamCount = data.TranslationStreamCount,
            FrameCount = data.FrameCount,
            RawScales = PrimitiveArrays.ReadSingle(reader, data.Scales),
            RawRotations = PrimitiveArrays.ReadVector4(reader, data.Rotations),
            RawTranslations = PrimitiveArrays.ReadVector4(reader, data.Translations),
        };
    }

    // decodes the clip and runs it on the rig
    public BoneAnimation[] Evaluate(
        EntityRuntimeRig rig, EntitySkeleton skeleton,
        bool localSpace = true)
    {
        var controls = AnimationDecoder.Decode(this);
        var objectSpace = RigRetarget.Retarget(this, controls, skeleton, rig);
        return localSpace ? RigRetarget.ToLocalSpace(this, objectSpace, skeleton) : objectSpace;
    }

    public FbxAnimationData EvaluateForFbx(
        EntityRuntimeRig rig, EntitySkeleton skeleton, string name, LayerType layerType,
        double frameRate = 30)
    {
        var objectSpace = Evaluate(rig, skeleton, false);
        return FbxAnimationBuilder.Build(name, objectSpace, skeleton, layerType, FrameCount, frameRate);
    }

    public FbxAnimationData EvaluateWeaponForFbx(
        EntityRuntimeRig rig, EntitySkeleton rigSkeleton, EntitySkeleton weaponSkeleton, string name, LayerType layerType,
        double frameRate = 30)
    {
        int first = AnimationSkinning.WeaponFirstBone(rigSkeleton, weaponSkeleton);
        var objectSpace = Evaluate(rig, rigSkeleton, false);
        var weaponBones = weaponSkeleton.BoneNodes;

        var anchor = RigTransform.Compose(
            RigTransform.FromObjectSpace(weaponBones[0].DefaultObjectSpaceTransform),
            ToRigTransform(objectSpace[first], 0).Inverse());

        var weapon = new BoneAnimation[weaponBones.Count];
        for (int i = 0; i < weapon.Length; i++)
        {
            BoneAnimation source = objectSpace[first + i];
            int frames = source.Rotations.Length;
            var bone = new BoneAnimation
            {
                BoneHash = source.BoneHash,
                Scales = new System.Numerics.Vector3[frames],
                Rotations = new System.Numerics.Vector4[frames],
                Translations = new System.Numerics.Vector3[frames],
            };

            for (int f = 0; f < frames; f++)
            {
                RigTransform t = RigTransform.Compose(anchor, ToRigTransform(source, f));
                bone.Rotations[f] = new System.Numerics.Vector4(t.Rotation.X, t.Rotation.Y, t.Rotation.Z, t.Rotation.W);
                bone.Translations[f] = t.Translation;
                bone.Scales[f] = source.Scales[f] * anchor.Scale;
            }
            weapon[i] = bone;
        }

        return FbxAnimationBuilder.Build(name, weapon, weaponSkeleton, layerType, FrameCount, frameRate);
    }

    private static RigTransform ToRigTransform(BoneAnimation bone, int frame)
    {
        var r = bone.Rotations[frame];
        return new RigTransform(
            System.Numerics.Quaternion.Normalize(new System.Numerics.Quaternion(r.X, r.Y, r.Z, r.W)),
            bone.Translations[frame],
            bone.Scales[frame].X);
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


    public void Debug()
    {
        using TigerReader reader = GetReader();
        if (_tag.StaticBoneData.GetValue(reader) is S80808B40)
            _ = false;
        else if (_tag.StaticBoneData.ResourceClassHash != TigerHash.InvalidHash32)
        {
            var classId = Helpers.GetClassHashForStrategy(typeof(S80808B40), Strategy.CurrentStrategy);
            Log.Debug($"{Hash} StaticBoneData: Should be 0x{classId:X8} but is {_tag.StaticBoneData.ResourceClassHash:X8}");
        }

        if (_tag.AnimatedBoneData.GetValue(reader) is S80808B43 or S80808B42 or S80808B40)
            _ = false;
        else if (_tag.AnimatedBoneData.GetValue(reader) is S80808B44 raw)
            Log.Debug($"{Hash} AnimatedBoneData: Raw ({raw.CodecType})");
        else if (_tag.AnimatedBoneData.ResourceClassHash != TigerHash.InvalidHash32)
            Log.Debug($"{Hash} AnimatedBoneData: Unknown {_tag.AnimatedBoneData.ResourceClassHash:X8}");
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
    public float[] RawScales = Array.Empty<float>();
    public System.Numerics.Vector4[] RawRotations = Array.Empty<System.Numerics.Vector4>();
    public System.Numerics.Vector4[] RawTranslations = Array.Empty<System.Numerics.Vector4>();
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

