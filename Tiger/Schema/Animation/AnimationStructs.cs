using System.Runtime.InteropServices;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

// todo move runtime rig structs and enums into own files

public enum AnimationCodecType : short
{
    Raw = 0,
    Compressed = 1,
    Uncompressed = 2,
    PoseQuantized = 3,
}

public enum SpaceType : short
{
    RELATIVE = 0,
    PARENT = 1,
    OBJECT = 2,
    UNK_3 = 3,
    BIND = 4,
    AVERAGE = 5,
    AIM = 6,
}

public enum ConstraintType : sbyte
{
    PARENT = 0,
    POINT_AND_ORIENT = 1,
}

public enum IkFunctionType : sbyte
{
    NONE = -1,
    IK_3 = 0,
    RFK = 1
}

public enum LayerType : byte
{
    Base = 0,
    Additive = 1,
    Replacement = 2
}

#region Animation Tag
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005A1, 0x170)] // A1058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F49, 0x190)] // 498F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BE0, 0x190)] // E08B8080
public struct SAnimationClip
{
    public long FileSize;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public ResourcePointer StaticBoneData; // 408B8080
    public ResourcePointer AnimatedBoneData; // 438B8080 compressed, 428B8080 uncompressed

    // currently unused, idk what they're used for, no point in loading
    //public ResourcePointer Unk20; // 4C8B8080, 4B8B8080
    //public ResourcePointer Unk28; // 488B8080, ??
    //public ResourcePointer Unk30; // 2A8B8080, ??
    //public ResourcePointer Unk38; // 4C8B8080, ??
    //public ResourcePointer Unk40; // 4C8B8080, ??
    //public ResourcePointer Unk48; // 428B8080, ??
    // unsure of any pointers at 50, 58

    [SchemaField(0x58, TigerStrategy.DESTINY1_RISE_OF_IRON)] // I think?
    [SchemaField(0x60, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public ResourcePointer RootMotionTrack; // 428B8080, ??

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public CycleTransform CycleTransform;
    public DynamicArray<SStringHash> Unk90;

    [SchemaField(0x90, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xA0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public int UnkA0;
    public int UnkA4;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public DynamicStruct<ControlMapsD1> ControlMapsD1;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicStruct<ControlMapsD2> ControlMapsD2;

    [SchemaField(0x110, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x120, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash AnimationHash;

    [SchemaField(0x128, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x13C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x140, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short FrameCount;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x13E, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x142, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short NodeCount;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x140, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x144, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public short RigControlCount;

    [SchemaField(0x12A, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public byte NodeCountD1;
    [SchemaField(0x12B, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public byte RigControlCountD1;


    [SchemaField(0x12C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x142, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x146, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public byte ExtractionMode; // root motion extraction mode
    public byte PoseSpace; // 1 line, 2 disk, 3 plane, 4 sphere

    [SchemaField(0x130, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x150, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<SStringHash> Unk150; // hash of the retarget profile
    public DynamicArray<S80808C08> FrameEvents;
    public DynamicArray<S80808C5F> RigComponents;

    [SchemaField(0x164, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x180, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public byte Unk180;
    public LayerType LayerType; // unsure, 0 base clip, 1 additive or pose layer, 2 replacement layer

    public int GetRigControlCount()
    {
        if (Strategy.IsD1())
            return RigControlCountD1;

        return RigControlCount;
    }

    public int GetNodeCount()
    {
        if (Strategy.IsD1())
            return NodeCountD1;

        return NodeCount;
    }
}

[NonSchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x60)]
public struct ControlMapsD1 // dumb dumb dumb why bungo
{
    public DynamicArray<SUInt8> StaticScaleControlMap;
    public DynamicArray<SUInt8> StaticRotationControlMap;
    public DynamicArray<SUInt8> StaticTranslationControlMap;
    public DynamicArray<SUInt8> AnimatedScaleControlMap;
    public DynamicArray<SUInt8> AnimatedRotationControlMap;
    public DynamicArray<SUInt8> AnimatedTranslationControlMap;
}

[NonSchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x60)]
public struct ControlMapsD2
{
    public DynamicArray<SUInt16> StaticScaleControlMap;
    public DynamicArray<SUInt16> StaticRotationControlMap;
    public DynamicArray<SUInt16> StaticTranslationControlMap;
    public DynamicArray<SUInt16> AnimatedScaleControlMap;
    public DynamicArray<SUInt16> AnimatedRotationControlMap;
    public DynamicArray<SUInt16> AnimatedTranslationControlMap;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808004A9, 0x68)] // A9048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F6F, 0x48)] // 6F8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B40, 0x48)] // 408B8080
public struct S80808B40
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public int FrameCount;
    public FloatQuantise ScaleStreamQuantization;
    public Float3Quantise TranslationStreamQuantization;

    [SchemaField(0x38, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public DynamicArrayUnloaded<SUInt16> StreamData;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005E6, 0x48)] // E6058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F71, 0x48)] // 718F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B42, 0x48)] // 428B8080
public struct S80808B42
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;

    [SchemaField(0x10)]
    public int FrameCount;

    [SchemaField(0x18)]
    public DynamicArrayUnloaded<SUInt16> StreamData;
    public DynamicArrayUnloaded<SReal32> QuantizationExtents;
    public DynamicArrayUnloaded<SReal32> QuantizationMinimums;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800432, 0x80)] //32048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F72, 0x80)] //728F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B43, 0x80)] //438B8080
public struct S80808B43
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;

    public float ErrorValue;  // guess
    public float CompressionRate;  // guess

    public DynamicArrayUnloaded<SInt16> UncompressedData;
    public DynamicArrayUnloaded<SInt16> CompressedData;
    public DynamicArrayUnloaded<SUInt8> KeyframeDeltas;
    public DynamicArrayUnloaded<SUInt8> InterpolationData;
    public DynamicArrayUnloaded<SReal32> QuantizationExtents;
    public DynamicArrayUnloaded<SReal32> QuantizationMinimums;
    public DynamicArrayUnloaded<SInt16> Unk70; // can be SInt32 sometimes???
}

// codec 0
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080097F, 0x48)] // 7F098080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F73, 0x48)] // 738F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B44, 0x48)] // 448B8080
public struct S80808B44
{
    public AnimationCodecType CodecType;
    public short ScaleStreamCount;
    public short RotationStreamCount;
    public short TranslationStreamCount;

    [SchemaField(0x10)]
    public int FrameCount;

    [SchemaField(0x18)]
    public DynamicArrayUnloaded<SReal32> Scales; // unsure
    public DynamicArrayUnloaded<Vec4> Rotations; // quat
    public DynamicArrayUnloaded<Vec4> Translations; // xyz
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F7E, 0x60)] // 7E8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B4E, 0x60)] // 4E8B8080
public struct S80808B4E
{
    public short Unk00;
    public short Unk02;

    [SchemaField(0x8)]
    public float Unk08;

    [SchemaField(0x20)]
    public DynamicArray<SInt16> Unk20;
    public DynamicArray<SUInt8> Unk30;
    public DynamicArray<SUInt8> Unk40;
    public DynamicArray<SInt16> Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F59, 0x10)] // 598F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B2A, 0x10)] // 2A8B8080
public struct S80808B2A
{
    public DynamicArray<S80808B2D> Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F5C, 0x40)] // 5C8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B2D, 0x40)] // 2D8B8080
public struct S80808B2D
{
    public short Unk00;
    public short Unk02;

    [SchemaField(0x10)]
    public short Unk10;
    public short Unk12;
    public TigerHash Unk14;

    [SchemaField(0x20)]
    public Vector4 Unk20;
    public Vector4 Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F7C, 0x50)] // 7C8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B4C, 0x50)] // 4C8B8080
public struct S80808B4C
{
    public short Unk00;
    public short Unk02;
    [SchemaField(0xC)]
    public int FrameCount;
    public DynamicArray<S80808B52> Unk10;
    public DynamicArray<SUInt16> Unk20;
    public DynamicArray<SReal32> Unk30;
    public DynamicArray<SReal32> Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F82, 0x2)] // 828F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B52, 0x2)] // 528B8080
public struct S80808B52
{
    public ushort Unk00;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808004E1, 0x8)] // E1048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080902B, 0x8)] // 2B908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C08, 0x8)] // 088C8080
public struct S80808C08
{
    public ResourcePointer Pointer; // 0A/11/12/13/1A8C8080
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C11, 0x10)] // 118C8080
public struct S80808C11
{
    public short Unk00;
    public short Unk02;
    public int Unk04;
    public TigerHash Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C12, 0x10)] // 128C8080
public struct S80808C12
{
    public short Unk00;
    public short Unk02;
    public TigerHash Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080060E, 0x20)] // 0E068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809033, 0x20)] // 33908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C13, 0x38)] // 138C8080
public struct S80808C13 // 'tag frame event'?
{
    public short StartFrame;
    public short Unk02; // Camera Type? 0 none, 1 first, 2 third, 3 first and third?
    public short Unk04; // Creation Type? 0 attached?

    [SchemaField(0x8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public TigerHash MarkerName;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public StringPointer TagPath;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public FileHash EventTag;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080893A, 0x8)] // 3A908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C1A, 0x8)] // 1A8C8080
public struct S80808C1A
{
    public short Unk00;
    public short Unk02;
    public short Unk04;
    public short Unk06;
}

[StructLayout(LayoutKind.Sequential, Size = 0x8)]
public struct FloatQuantise
{
    public float Extent;
    public float Minimum;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct Float3Quantise
{
    public Vector3 Extent;
    public Vector3 Minimum;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct Float4Quantise
{
    public Vector4 Extent;
    public Vector4 Minimum;
}

[StructLayout(LayoutKind.Sequential, Size = 0x20)]
public struct CycleTransform
{
    public Vector4 Rot; // quat
    public Vector3 Trans;
    public float Scale;
}
#endregion

#region Runtime Rig
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808008B2, 0x70)] // B2088080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F96, 0x70)] // 968F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B66, 0x70)] // 668B8080, component 0x8
public struct S80808B66
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080099B, 0x120)] // 9B098080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F8F, 0x130)] // 8F8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B5F, 0x140)] // 5F8B8080, component 0x10
public struct S80808B5F
{
    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xA8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808C5F> RigComponents;
    public DynamicArray<S80808C56> ControlsRelations;
    public DynamicArray<S80809F4F> ControlsTransforms;
    public DynamicArray<SInt16> IKControls;

    [SchemaField(0xC0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xD0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xE8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80808C51> Profiles;

    [SchemaField(0xC8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0xD8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xF0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<SInt16> BoneToControl;
    public DynamicArray<SInt16> ControlToBone;

    [SchemaField(0x100, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x110, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x128, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808BDA> ControlNameToBoneIndex;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808005E3, 0x18)] // E3058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809071, 0x18)] // 71908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C51, 0x18)] // 518C8080
public struct S80808C51
{
    [SchemaField(0x8)]
    public DynamicArray<S80808C53> Profiles;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808003CF, 0x38)] // CF038080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809073, 0x38)] // 73908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C53, 0x38)] // 538C8080
public struct S80808C53
{
    public DynamicArray<SStringHash> Names;
    public float TranslationScale;
    public float Unk14;
    // 3 dynamic arrays after, idk what for
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800380, 0x8)] // 80038080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080907F, 0x8)] // 7F908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C5F, 0x8)] // 5F8C8080
public struct S80808C5F
{
    public TigerHash Unk00;
    public int Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808006C8, 0x8)] // C8068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808FFE, 0x8)] // FE8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BDA, 0x8)] // DA8B8080
public struct S80808BDA
{
    public TigerHash Hash;
    public int Count;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80809076, 0x30)] // 38068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80809076, 0x34)] // 76908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808C56, 0x34)] // 568C8080
public struct S80808C56
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public TigerHash Unk00;

    public SpaceDef RotationSpace0;
    public SpaceDef RotationSpace1;
    public float AimWeight;
    public short AimTarget;
    public sbyte AimSource; // 0 aim dir, 1 look dir, 2 point at bone
    public sbyte Unk17;
    public SpaceDef TranslationSpace0;
    public SpaceDef TranslationSpace1;
    public float AimWeight2;
    public short AimTarget2;
    public sbyte AimSource2; // 0 aim dir, 1 look dir, 2 point at bone
    public sbyte Unk2B;
    public short BoneIndex;
    public short IkChainStartBoneIndex;
    public ConstraintType ConstraintType;
    public IkFunctionType IkFunctionType;
    public byte PoleAxis; // unknown if always 0
}

[StructLayout(LayoutKind.Sequential, Size = 0x6)]
public struct SpaceDef
{
    public short BoneIndex0;
    public short BoneIndex1;
    public SpaceType Type;
}
#endregion
