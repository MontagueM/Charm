﻿using System.Runtime.InteropServices;
using DirectXTexNet;
using Tiger.Schema.Model;
using Tiger.Schema.Audio;
using Tiger.Schema.Investment;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Entity;

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "0F9C8080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "D89A8080", 0x98)]
public struct SEntity
{
    public long FileSize;
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x08, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<D2Class_CD9A8080> EntityResources;
    public DynamicArrayUnloaded<D2Class_8F9A8080> Unk18;
    public long Zeros28;
    public long Zeros30;
    public TigerHash Unk38;
    public int Zeros3C;
    public DynamicArrayUnloaded<D2Class_F09A8080> Unk40;
    public DynamicArrayUnloaded<D2Class_ED9A8080> Unk50;
    public DynamicArrayUnloaded<D2Class_EB9A8080> Unk60;
    public DynamicArrayUnloaded<D2Class_06008080> Unk70;
    public TigerHash Unk80;
    public TigerHash Unk84;
    public TigerHash Unk88;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "259C8080", 8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "F09A8080", 8)]
public struct D2Class_F09A8080
{
    public TigerHash Unk00;
    public ushort Unk04;
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "229C8080", 0x28)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "ED9A8080", 0x28)]
public struct D2Class_ED9A8080
{
    public short Unk00;
    public short Unk02;
    public short Unk04;
    public short Unk06;
    public int Unk08;  // some pointer
    // public TagTypeHash Unk0C;  // the class of the final target resource
    // [DestinyField(FieldType.ResourceInTagWeird)]
    // public dynamic? Resource10;  // non-standard resource in tag, the resource type is actually the one before and its like a double-pointer thing. means nothing to me so wont parse these kinds.
    // [SchemaField(0x20), DestinyField(FieldType.FileHash)]
    // public Tag Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "209C8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "EB9A8080", 0x18)]
public struct D2Class_EB9A8080
{
    public ResourceInTagPointer Resource00;

    public long Unk10;
}

[SchemaStruct("06008080", 2)]
public struct D2Class_06008080
{
    public short Unk0;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "049C8080", 0xC)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "CD9A8080", 0xC)]
public struct D2Class_CD9A8080  // entity resource entry
{
    public EntityResource Resource;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "369C8080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "069B8080", 0xA0)]
public struct D2Class_069B8080  // Entity resource
{
    public long FileSize;
    public ResourcePointer Unk08;
    public ResourcePointer Unk10;
    public ResourcePointer Unk18;
    // public Table ResourceTable20;
    // public Table ResourceTable30;
    [SchemaField(0x40)]
    public DynamicArrayUnloaded<D2Class_7C908080> ResourceTable40;
    // public Table ResourceTable50;
    [SchemaField(0x60)]
    public DynamicArrayUnloaded<D2Class_6E908080> ResourceTable60;
    // public Table ResourceTable70;
    [SchemaField(0x80)]
    public Tag UnkHash80;
    public Tag UnkHash84;  // 819A8080
    // Rest is unknown
}

[SchemaStruct("7C908080", 0x10)]
public struct D2Class_7C908080
{
    public ResourcePointerWithClass ResourcePointer00;
    public TigerHash ResourceClassHash08;
    public short Unk0C;
    public short Unk0E;
}

[SchemaStruct("6E908080", 8)]
public struct D2Class_6E908080
{
    public RelativePointer RelativePointer00;
}

/*
 * The external material map provides the mapping of external material index -> material tag
 * could be these external materials are dynamic themselves - we'll extract them all but select the first
 */
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "BD728080", 0x340)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "8F6D8080", 0x450)]
public struct D2Class_8F6D8080
{
    [SchemaField(0x1DC, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x224, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public EntityModel Model;
    [SchemaField(0x310, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]  // todo shadowkeep
    public Tag<D2Class_1C6E8080> TexturePlates;
    // todo there might be a nice way to combine these two and namespace the struct, and instead use properties not fields? would mean all the mess is in the data side not the logic, basically mapping one onto the other. Possible to make it so everything maps onto the latest given, so it only needs to be done for older versions.
    [SchemaField(0x300, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, Obsolete = true)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntrySK> ExternalMaterialsMapSK;
    [SchemaField(0x3C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<SExternalMaterialMapEntryWQ> ExternalMaterialsMapWQ;
    [SchemaField(0x310, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x400, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<D2Class_14008080> ExternalMaterials;
}

#region Texture Plates
// todo move this
public class TexturePlate : Tag<D2Class_919E8080>
{
    public TexturePlate(FileHash hash) : base(hash)
    {
    }

    public ScratchImage? MakePlatedTexture()
    {
        var dimension = GetPlateDimensions();
        if (dimension.X == 0)
        {
            return null;
        }

        using TigerReader reader = GetReader();
        bool bSrgb = _tag.PlateTransforms[reader, 0].Texture.IsSrgb();
        ScratchImage outputPlate = TexHelper.Instance.Initialize2D(bSrgb ? DXGI_FORMAT.B8G8R8A8_UNORM_SRGB : DXGI_FORMAT.B8G8R8A8_UNORM, dimension.X, dimension.Y, 1, 0, 0);

        foreach (var transform in _tag.PlateTransforms.Enumerate(reader))
        {
            ScratchImage original = transform.Texture.GetScratchImage();
            ScratchImage resizedOriginal = original.Resize(transform.Scale.X, transform.Scale.Y, TEX_FILTER_FLAGS.SEPARATE_ALPHA);
            TexHelper.Instance.CopyRectangle(resizedOriginal.GetImage(0, 0, 0), 0, 0, transform.Scale.X, transform.Scale.Y, outputPlate.GetImage(0, 0, 0), bSrgb ? TEX_FILTER_FLAGS.SEPARATE_ALPHA : 0, transform.Translation.X, transform.Translation.Y);
            original.Dispose();
            resizedOriginal.Dispose();
        }
        return outputPlate;
    }

    public void SavePlatedTexture(string savePath)
    {
        var simg = MakePlatedTexture();
        if (simg != null)
        {
            Texture.SavetoFile(savePath, simg);
        }
    }

    public IntVector2 GetPlateDimensions()
    {
        int maxDimension = 0;  // plate must be square
        foreach (var transform in _tag.PlateTransforms.Enumerate(GetReader()))
        {
            if (transform.Translation.X + transform.Scale.X > maxDimension)
            {
                maxDimension = transform.Translation.X + transform.Scale.X;
            }
            if (transform.Translation.Y + transform.Scale.Y > maxDimension)
            {
                maxDimension = transform.Translation.Y + transform.Scale.Y;
            }
        }

        // Find power of two that fits this dimension, ie round up the exponent to nearest integer
        maxDimension = (int)Math.Pow(2, Math.Ceiling(Math.Log2(maxDimension)));

        return new IntVector2(maxDimension, maxDimension);
    }
}

/// <summary>
/// Texture plate header that stores all the texture plates used for the EntityModel.
/// </summary>
[SchemaStruct("1C6E8080", 0x38)]
public struct D2Class_1C6E8080
{
    public long FileSize;
    [SchemaField(0x18)]
    public byte Unk18;
    public byte Unk19;
    public short Unk1A;
    public int Unk1C;
    public int Unk20;
    public int Unk24;
    public TexturePlate AlbedoPlate;
    public TexturePlate NormalPlate;
    public TexturePlate GStackPlate;
    public TexturePlate DyemapPlate;
}

/// <summary>
/// Texture plate that stores the data for placing textures on a canvas.
/// </summary>
[SchemaStruct("919E8080", 0x20)]
public struct D2Class_919E8080
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<D2Class_939E8080> PlateTransforms;
}

[SchemaStruct("939E8080", 0x14)]
public struct D2Class_939E8080
{
    public Texture Texture;
    public IntVector2 Translation;
    public IntVector2 Scale;
}

#endregion

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "C5728080", 0x8)]
public struct SExternalMaterialMapEntrySK
{
    public ushort MaterialCount;
    public ushort MaterialStartIndex;
    public ushort Unk04;  // maybe some kind of LOD or dynamic marker
    public ushort Unk06;
}

[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "976D8080", 0xC)]
public struct SExternalMaterialMapEntryWQ
{
    public int MaterialCount;
    public int MaterialStartIndex;
    public int Unk08;  // maybe some kind of LOD or dynamic marker
}

[SchemaStruct("14008080", 0x4)]
public struct D2Class_14008080
{
    public IMaterial Material;
}

[SchemaStruct("8F9A8080", 0x38)]
public struct D2Class_8F9A8080
{
    // public InlineGlobalPointer Unk0;
    [SchemaField(0x10)]
    public TigerHash Unk10;
    // public InlineGlobalPointer Unk18;
    [SchemaField(0x28)]
    public TigerHash Unk28;
}

[SchemaStruct("9F9A8080", 0x20)]
public struct D2Class_9F9A8080
{

}

[SchemaStruct("5F6E8080", 0x40)]
public struct D2Class_5F6E8080
{
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B8728080", 0x200)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "8A6D8080", 0x2E0)]
public struct D2Class_8A6D8080
{
}

[SchemaStruct("F39A8080", 0x10)]
public struct D2Class_F39A8080
{

}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "45858080", 0x100)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "DD818080", 0x100)]
public struct D2Class_DD818080
{
    public ResourceInTagPointer Unk00;
    [SchemaField(0x30)]
    public DynamicArray<D2Class_DC818080> Unk30;
    public DynamicArray<D2Class_40868080> Unk40;
}

[SchemaStruct("DC818080", 0x40)]
public struct D2Class_DC818080
{
    public ResourceInTagPointer Unk00;
    public ResourcePointer Unk10;
    [SchemaField(0x20)]
    public DynamicArray<D2Class_4F9F8080> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "759F8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "4F9F8080", 0x20)]
public struct D2Class_4F9F8080
{
    public Tiger.Schema.Vector4 Rotation;
    public Tiger.Schema.Vector4 Translation;
}

[SchemaStruct("40868080", 8)]
public struct D2Class_40868080
{
    public ushort Unk00;
    public ushort Unk02;
    public uint Unk04;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "46858080", 0xF0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "DE818080", 0x108)]
public struct D2Class_DE818080
{
    public ResourceInTagPointer Unk00;
    [SchemaField(0x48)]
    public ResourcePointer Unk48;
    public Tag Unk50;  // 239B8080 WQ, 549C8080 SK
    [SchemaField(0x60, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public ResourcePointer Unk68;
    public Tag Unk70;  // 239B8080 WQ, 549C8080 SK
    [SchemaField(0x78, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x88, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public TigerHash Unk88;
    public TigerHash Unk8C;  // this is actually zeros in SK
    public DynamicArrayUnloaded<D2Class_42868080> NodeHierarchy;
    public DynamicArrayUnloaded<D2Class_4F9F8080> DefaultObjectSpaceTransforms;
    public DynamicArrayUnloaded<D2Class_4F9F8080> DefaultInverseObjectSpaceTransforms;
    public DynamicArrayUnloaded<D2Class_06008080> RangeIndexMap;
    public DynamicArrayUnloaded<D2Class_06008080> InnerIndexMap;
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 UnkE0;
    [SchemaField(0xD8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xF0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicArrayUnloaded<D2Class_E1818080> UnkF0; // lod distance?
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "088A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "42868080", 0x10)]
public struct D2Class_42868080
{
    public TigerHash NodeHash;
    public int ParentNodeIndex;
    public int FirstChildNodeIndex;
    public int NextSiblingNodeIndex;
}

[SchemaStruct("E1818080", 0x18)]
public struct D2Class_E1818080
{
    public ResourceInTagPointer Unk00;
    public long Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A5738080", 0xA0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "076F8080", 0xA0)]
public struct SEntityModel  // Entity model
{
    public long FileSize;
    [SchemaField(0x10)]
    public DynamicArrayUnloaded<SEntityModelMesh> Meshes;
    [SchemaField(0x20)]
    public Vector4 Unk20;
    public long Unk30;
    [SchemaField(0x38)]
    public long UnkFlags38;
    [SchemaField(0x40)]
    public long Unk40;
    [SchemaField(0x50)]
    public Vector4 ModelScale;
    public Vector4 ModelTranslation;
    public Vector2 TexcoordScale;
    public Vector2 TexcoordTranslation;
    public Vector4 Unk80;
    public TigerHash Unk90;
    public TigerHash Unk94;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "78738080", 0x88)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C56E8080", 0x80)]
public struct SEntityModelMesh
{
    public VertexBuffer Vertices1;  // vert file 1 (positions)
    public VertexBuffer Vertices2;  // vert file 2 (texcoords/normals)
    public VertexBuffer OldWeights;  // old weights
    public TigerHash Unk0C;  // nothing ever
    public IndexBuffer Indices;  // indices
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer VertexColour;  // vertex colour
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public VertexBuffer SinglePassSkinningBuffer;  // single pass skinning buffer
    public int Zeros1C;
    public DynamicArrayUnloaded<D2Class_CB6E8080> Parts;
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, ArraySizeConst = 48)]
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307, ArraySizeConst = 37)]
    public short[] StagePartOffsets;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7E738080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "CB6E8080", 0x24)]
public struct D2Class_CB6E8080  // TODO use DCG to figure out what this is
{
    public IMaterial Material;  // AA6D8080
    public short VariantShaderIndex;  // variant_shader_index
    public short PrimitiveType;
    public uint IndexOffset;
    public uint IndexCount;
    public uint Unk10;  // might be number of strips?
    public short ExternalIdentifier;  // external_identifier
    public short Unk16;  // some kind of index
    // need to check this on WQ, theres no way its an int
    public int Flags;
    [SchemaField(0x1A, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public byte GearDyeChangeColorIndex;   // sbyte gear_dye_change_color_index
    public ELodCategory LodCategory;
    public byte Unk1E;
    public byte LodRun;  // lod_run
    [SchemaField(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int Unk20; // variant_shader_index?
}

[SchemaStruct("5B6D8080", 0x320)]
public struct D2Class_5B6D8080
{
    // Full of relative pointer shit
    // Tables start at 0x1f0
    [SchemaField(0x210)]
    public DynamicArray<D2Class_0B008080> Unk210;
    [SchemaField(0x220)]
    public DynamicArray<D2Class_D99E8080> Unk220;
    // there are more tables
}

[SchemaStruct("0B008080", 4)]
public struct D2Class_0B008080
{
    public uint Unk00;
}

[SchemaStruct("D99E8080", 0x10)]
public struct D2Class_D99E8080
{
    public DynamicArray<D2Class_0B008080> Unk00;
}

[SchemaStruct("6C6D8080", 0x480)]
public struct D2Class_6C6D8080
{
    [SchemaField(0x38)]
    public DynamicArray<D2Class_F79A8080> Unk38;
    [SchemaField(0x224)]
    public EntityModel PhysicsModel;
    [SchemaField(0x2E8)]
    public DynamicArray<D2Class_9E958080> Unk2E8;
    // there are more tables
    [SchemaField(0x470)]
    public Tag Unk470;  // 606D8080
}

[SchemaStruct("F79A8080", 0x18)]
public struct D2Class_F79A8080
{
    public ulong Unk00;
}

[SchemaStruct("9E958080", 4)]
public struct D2Class_9E958080
{
    public uint Unk00;
}

[SchemaStruct("668B8080", 0x70)]
public struct D2Class_668B8080
{
    public ResourceInTagPointer Unk00;
    [SchemaField(0x30)]
    public DynamicArray<D2Class_628B8080> Unk30;
}

[SchemaStruct("628B8080", 0x30)]
public struct D2Class_628B8080
{
    public Vector4 Unk00;
}

[SchemaStruct("5F8B8080", 0x140)]
public struct D2Class_5F8B8080
{
    public ResourceInTagPointer Unk00;
    [SchemaField(0xA8)]
    public DynamicArray<D2Class_5F8C8080> UnkA8;
    public DynamicArray<D2Class_568C8080> UnkB8;
    public DynamicArray<D2Class_4F9F8080> UnkC8;
    public DynamicArray<D2Class_06008080> UnkD8;
    [SchemaField(0xF0)]
    public DynamicArray<D2Class_06008080> UnkF0;
    public DynamicArray<D2Class_06008080> Unk100;
    [SchemaField(0x128)]
    public DynamicArray<D2Class_DA8B8080> Unk128;
    public int Unk138;
    public short Unk13C;
    public short Unk13E;
}

[SchemaStruct("5F8C8080", 8)]
public struct D2Class_5F8C8080
{
    public TigerHash Unk00;
    public int Unk04;
}

[SchemaStruct("568C8080", 0x34)]
public struct D2Class_568C8080
{
    public TigerHash Unk00;
    public short Unk04;
    public short Unk06;
    public short Unk08;
    public short Unk0A;
    public short Unk0C;
    public short Unk0E;
    public float Unk10;
    public int Unk14;

    public short Unk18;
    public short Unk1A;
    public short Unk1C;
    public short Unk1E;
    public short Unk20;
    public short Unk22;
    public float Unk24;
    public int Unk28;

    public short Unk2C;
    public short Unk2E;
    public sbyte Unk30;
    public sbyte Unk31;
    public sbyte Unk32;
    public sbyte Unk33;
}

[SchemaStruct("DA8B8080", 8)]
public struct D2Class_DA8B8080
{
    public TigerHash Unk00;
    public int Unk04;
}

[SchemaStruct("13268080", 0x830)]
public struct D2Class_13268080
{
    public ResourceInTagPointer Unk00;
    // lots of array stuff
}

[SchemaStruct("F8258080", 0x830)]
public struct D2Class_F8258080
{
    public ResourceInTagPointer Unk00;
    // lots of array stuff
    [SchemaField(0xA8), MarshalAs(UnmanagedType.ByValArray, SizeConst = 33)]
    public FileHash[] UnkA8;
}

[SchemaStruct("41268080", 0xBA0)]
public struct D2Class_41268080
{
    public ResourceInTagPointer Unk00;
    // lots of array stuff
    [SchemaField(0x1E0)]
    public DynamicArray<D2Class_0B008080> Unk1E0;
    public DynamicArray<D2Class_0F008080> Unk1F0;
    public DynamicArray<D2Class_90008080> Unk200;

    [SchemaField(0x240)]
    public DynamicArray<D2Class_0B008080> Unk240;
    public DynamicArray<D2Class_0F008080> Unk250;
    public DynamicArray<D2Class_90008080> Unk260;

    [SchemaField(0x5F8)]
    public DynamicArray<D2Class_86268080> Unk5F8;
    [SchemaField(0x658)]
    public DynamicArray<D2Class_86268080> Unk658;
    [SchemaField(0x6B8)]
    public DynamicArray<D2Class_86268080> Unk6B8;
    [SchemaField(0x718)]
    public DynamicArray<D2Class_86268080> Unk718;
    [SchemaField(0x778)]
    public DynamicArray<D2Class_86268080> Unk778;
    [SchemaField(0x7D8)]
    public DynamicArray<D2Class_86268080> Unk7D8;
    [SchemaField(0x838)]
    public DynamicArray<D2Class_86268080> Unk838;
    [SchemaField(0x898)]
    public DynamicArray<D2Class_86268080> Unk898;
    [SchemaField(0x8F8)]
    public DynamicArray<D2Class_86268080> Unk8F8;
    [SchemaField(0x958)]
    public DynamicArray<D2Class_86268080> Unk958;
    [SchemaField(0x9B8)]
    public DynamicArray<D2Class_86268080> Unk9B8;
    [SchemaField(0xA18)]
    public DynamicArray<D2Class_86268080> UnkA18;
    [SchemaField(0xA78)]
    public DynamicArray<D2Class_86268080> UnkA78;
    [SchemaField(0xAD8)]
    public DynamicArray<D2Class_86268080> UnkAD8;
    [SchemaField(0xB38)]
    public DynamicArray<D2Class_86268080> UnkB38;

    [SchemaField(0xB70)]
    public DynamicArray<D2Class_72268080> Unk6E8;
}

[SchemaStruct("0F008080", 4)]
public struct D2Class_0F008080
{
    public float Unk00;
}

[SchemaStruct("90008080", 0x10)]
public struct D2Class_90008080
{
    public Vector4 Unk00;
}

[SchemaStruct("86268080", 0x14)]
public struct D2Class_86268080
{
    [SchemaField(0x4)]
    public Tag Unk04;
}

[SchemaStruct("72268080", 0x210)]
public struct D2Class_72268080
{
    [SchemaField(0x200)]
    public TigerHash Unk200;
    public int Unk204;
    public int Unk208;
}

[SchemaStruct("3C268080", 0x348)]
public struct D2Class_3C268080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("B1288080", 0xC30)]
public struct D2Class_B1288080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0xA8)]
    public DynamicArray<D2Class_0F008080> UnkA8;
    [SchemaField(0xB8)]
    public DynamicArray<D2Class_BC288080> UnkB8;
    [SchemaField(0xC8)]
    public DynamicArray<D2Class_BE288080> UnkC8;
    [SchemaField(0x790)]
    public DynamicArray<D2Class_E4288080> Unk790;
    [SchemaField(0x7D0)]
    public DynamicArray<D2Class_E4288080> Unk7D0;
    [SchemaField(0x810)]
    public DynamicArray<D2Class_E4288080> Unk810;
}

[SchemaStruct("BC288080", 0xC)]
public struct D2Class_BC288080
{
}

[SchemaStruct("BE288080", 0x18)]
public struct D2Class_BE288080
{
}

[SchemaStruct("E4288080", 0x38)]
public struct D2Class_E4288080
{
}

[SchemaStruct("9B288080", 0x4C0)]
public struct D2Class_9B288080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x270)]
    public DynamicArray<D2Class_2B948080> Unk270;
}

[SchemaStruct("2B948080", 0x100)]
public struct D2Class_2B948080
{
    // Some set of vectors with rotation and translation data but interspersed with other data
}

[SchemaStruct("32288080", 0x160)]
public struct D2Class_32288080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("31288080", 0x108)]
public struct D2Class_31288080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("9C818080", 0xE0)]
public struct D2Class_9C818080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("9D818080", 0x108)]
public struct D2Class_9D818080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x38)]
    public DynamicArray<D2Class_F79A8080> Unk38;
    [SchemaField(0xB8)]
    public DynamicArray<D2Class_A9818080> UnkB8;
}

[SchemaStruct("A9818080", 0x30)]
public struct D2Class_A9818080
{
    public Vector4 Unk00;
    public Vector4 Unk10;
    public int Unk20;
    public int Unk24;
    public int Unk28;
    public int Unk2C;
}

[SchemaStruct("DA5E8080", 0x940)]
public struct D2Class_DA5E8080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("DB5E8080", 0x240)]
public struct D2Class_DB5E8080
{
    public ResourceInTagPointer Unk00;
}

[SchemaStruct("12848080", 0x50)]
public struct D2Class_12848080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x30)]
    public DynamicArray<D2Class_1A848080> Unk30;
}

[SchemaStruct("1A848080", 0x10)]
public struct D2Class_1A848080
{
    public TigerHash Unk00;
}

[SchemaStruct("0E848080", 0xA0)]
public struct D2Class_0E848080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x38)]
    public DynamicArray<D2Class_F79A8080> Unk38;
    [SchemaField(0x88)]
    public DynamicArray<D2Class_1B848080> Unk88;
}

[SchemaStruct("1B848080", 0x18)]
public struct D2Class_1B848080
{
    public int Unk00;
    public float Unk04;
    public DynamicArray<D2Class_1D848080> Unk08;
}

[SchemaStruct("1D848080", 0x18)]
public struct D2Class_1D848080
{
    public int Unk00;
    public int Unk04;
    public Tag Unk08;
    public int Unk0C;
    public int Unk10;
    public int Unk14;
}

[SchemaStruct("21868080", 0x40)]
public struct D2Class_21868080
{
}

[SchemaStruct("6A918080", 0x2C0)]
public struct D2Class_6A918080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x1D0)]
    public DynamicArray<D2Class_07008080> Unk1D0;
    [SchemaField(0x1F0)]
    public DynamicArray<D2Class_07008080> Unk1F0;
}

[SchemaStruct("07008080", 4)]
public struct D2Class_07008080
{
    public uint Unk00;
}

[SchemaStruct("46868080", 0x4D8)]
public struct D2Class_46868080
{
    public ResourceInTagPointer Unk00;

    [SchemaField(0x38)]
    public DynamicArray<D2Class_F79A8080> Unk38;
    [SchemaField(0x480)]
    public DynamicArray<D2Class_77878080> Unk480;
    [SchemaField(0x4C0)]
    public DynamicArray<D2Class_37878080> Unk4C0;
}

[SchemaStruct("77878080", 0x90)]
public struct D2Class_77878080
{
    public TigerHash Unk00;
    public TigerHash Unk04;
    [SchemaField(0x14)]
    public float Unk14;
    public long Unk18;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
    public Vector4 Unk60;
    public Vector4 Unk70;
    public int Unk80;
    public int Unk84;
    public int Unk88;
    public int Unk8C;
}


[SchemaStruct("37878080", 4)]
public struct D2Class_37878080
{
    public Tag Unk00;
}

// [SchemaStruct("C96C8080", 0x800)]
// public struct D2Class_20408080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
//
//     [SchemaField(0x350)]
//     public DynamicArray<D2Class_DE408080> Unk350;
//     [SchemaField(0x380)]
//     public DynamicArray<D2Class_28218080> Unk380;
//     [SchemaField(0x3D0)]
//     public DynamicArray<D2Class_E0408080> Unk3D0;
//     [SchemaField(0x7A0)]
//     public DynamicArray<D2Class_51408080> Unk7A0;
// }
//
// [SchemaStruct("C96C8080", 4)]
// public struct D2Class_DE408080
// {
//     public float Unk00;
// }
//
// [SchemaStruct("C96C8080", 0x28)]
// public struct D2Class_28218080
// {
//     public float Unk00;
//     public float Unk04;
//     public float Unk08;
//     // [D2FieldOffset(0x10), D2FieldType(26)]
//     // public byte Unk0x10;
//     [SchemaField(0x20)]
//     public uint Unk20;
// }
//
// [SchemaStruct("C96C8080", 0x70)]
// public struct D2Class_E0408080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
// }
//
// [SchemaStruct("C96C8080", 0x40)]
// public struct D2Class_51408080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
// }

// [SchemaStruct("C96C8080", 0x668)]
// public struct D2Class_FC3F8080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
//
//     [SchemaField(0x1D0)]
//     public DynamicArray<D2Class_91408080> Unk1D0;
//     [SchemaField(0x1F0)]
//     public DynamicArray<D2Class_3B408080> Unk1F0;
//     [SchemaField(0x390)]
//     public DynamicArray<D2Class_1C408080> Unk390;
//     [SchemaField(0x3D0)]
//     public DynamicArray<D2Class_E1408080> Unk3D0;
// }
//
// [SchemaStruct("C96C8080", 0x160)]
// public struct D2Class_91408080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
//
//     [SchemaField(0x78)]
//     public DynamicArray<D2Class_74408080> Unk78;
// }
//
// [SchemaStruct("C96C8080", 0x10)]  // size is actually 8 but we need this to make it work
// public struct D2Class_74408080
// {
//     [SchemaField(0xC), DestinyField(FieldType.Resource)]
//     public dynamic? UnkC;
// }
//
// [SchemaStruct("C96C8080", 0x38)]
// public struct D2Class_8D408080
// {
//     [SchemaField(0x10)]
//     public float Unk10;
//     [SchemaField(0x20), DestinyField(FieldType.FileHash)]
//     public Tag Unk20;
//     [SchemaField(0x30)]
//     public TigerHash Unk30;
// }
//
// [SchemaStruct("C96C8080", 0xC)]  // Size is actually 8 but we need this to make it work
// public struct D2Class_3B408080
// {
//     [SchemaField(0x8), DestinyField(FieldType.Resource)]
//     public dynamic? UnkC;
// }
//
// [SchemaStruct("C96C8080", 0x10)]
// public struct D2Class_42408080
// {
// }
//
// [SchemaStruct("C96C8080", 0xC)]
// public struct D2Class_1C408080
// {
//     public TigerHash Unk00;
//     public int Unk04;
//     public int Unk08;
// }
//
// [SchemaStruct("C96C8080", 0x78)]
// public struct D2Class_E1408080
// {
//     [DestinyField(FieldType.ResourceInTag)]
//     public dynamic? Unk00;
//
//     // loads of floats
// }
//
// [SchemaStruct("C96C8080", 0x28)]
// public struct D2Class_FD3F8080
// {
// }
//
// [SchemaStruct("C96C8080", 0xB00)]
// public struct D2Class_C5348080
// {
//     public D2Class_052E8080 Unk0x0;
//     public byte UnkAF4;
//     public byte UnkAF8;
// }
//
// [SchemaStruct("C96C8080", 0xB00)]
// public struct D2Class_052E8080
// {
//     [SchemaField(0x650)]
//     public byte Unk650;
//     [SchemaField(0x750)]
//     public byte Unk750;
//     [SchemaField(0x751)]
//     public bool Unk751;
//     [SchemaField(0x7D2)]
//     public bool Unk7D2;
//     [SchemaField(0x800)]
//     public float Unk800;
//     // [D2FieldOffset(0x804), D2FieldType(44)]
//     // public byte Unk0x804;
//     // [D2FieldOffset(0x808), D2FieldType(45)]
//     // public byte Unk0x808;
//     [SchemaField(0x768)]
//     public DynamicArray<D2Class_D2398080> Unk768;
// }
//
// [SchemaStruct("C96C8080", 0x30)]
// public struct D2Class_D2398080
// {
//     // [D2FieldOffset(0x20), D2FieldType(32)]  // resource pointer
//     // public byte Unk0x20;
// }
//
// [SchemaStruct("C96C8080", 0x70)]
// public struct D2Class_64338080
// {
//     [SchemaField(0x48)]
//     public DynamicArray<D2Class_71338080> Unk48;
// }
//
// [SchemaStruct("C96C8080", 0x30)]
// public struct D2Class_71338080
// {
// }

// General, parents that reference Entity

[SchemaStruct("30898080", 0x28)]
public struct D2Class_30898080
{
    public long FileSize;
    public DynamicArray<D2Class_34898080> Unk08;
    public DynamicArray<D2Class_33898080> Unk18;
}

[SchemaStruct("34898080", 0x20)]
public struct D2Class_34898080
{
}

[SchemaStruct("33898080", 0x20)]
public struct D2Class_33898080
{
    public StringPointer TagPath;
    [Tag64]
    public Tag Tag;  // if .pattern.tft, then Entity - if .budget_set.tft, then parent of itself
    public StringPointer TagNote;
}

[SchemaStruct("ED9E8080", 0x58)]
public struct D2Class_ED9E8080
{
    public long FileSize;
    [SchemaField(0x18)]
    public Tag Unk18;
    [SchemaField(0x28)]
    public DynamicArray<D2Class_F19E8080> Unk28;
}

[SchemaStruct("F19E8080", 0x18)]
public struct D2Class_F19E8080
{
    public StringPointer TagPath;
    [Tag64]
    public Tag Tag;  // if .pattern.tft, then Entity
}

[SchemaStruct("7E988080", 8)]
public struct D2Class_7E988080
{
    public Tag Unk00;
    public Tag Unk08;
}

[SchemaStruct("44318080", 8)]
public struct D2Class_44318080
{
    public long FileSize;
    [Tag64]
    public Entity? Entity;
}

#region Named entities

//I think this is the old struct for named bags, it seems like it changed to 1D478080?

//[SchemaStruct("C96C8080", 0x50)]
//public struct D2Class_75988080
//{
//    public long FileSize;
//    // [DestinyField(FieldType.RelativePointer)]
//    // public string DestinationGlobalTagBagName;
//    public FileHash DestinationGlobalTagBag;
//    // [SchemaField(0x20)]
//    // public FileHash PatrolTable1;
//    // [SchemaField(0x28), DestinyField(FieldType.RelativePointer)]
//    // public string PatrolTableName;
//    // public FileHash PatrolTable2;
//}

[SchemaStruct("1D478080", 0x18)]
public struct D2Class_1D478080
{
    public long FileSize;
    public DynamicArray<D2Class_D3598080> DestinationGlobalTagBags;
}

[SchemaStruct("D3598080", 0x10)]
public struct D2Class_D3598080
{
    public FileHash DestinationGlobalTagBag;
    [SchemaField(0x8)]
    public StringPointer DestinationGlobalTagBagName;
}

#endregion

#region Audio

[SchemaStruct("6E358080", 0x6b8)]
public struct D2Class_6E358080
{
    [SchemaField(0x648)]
    public DynamicArray<D2Class_9B318080> PatternAudioGroups;
}

[SchemaStruct("9B318080", 0x128)]
public struct D2Class_9B318080
{
    public TigerHash WeaponContentGroup1Hash;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x18), Tag64]
    public FileHash StringContainer;  // idk why but i presume debug strings
    public TigerHash WeaponContentGroup2Hash;  // "weaponContentGroupHash" from API
    // theres other stringcontainer stuff but skipping it
    [SchemaField(0xA0), Tag64]
    public Entity? WeaponSkeletonEntity;
    [SchemaField(0xD0), Tag64]
    public Tag<D2Class_A36F8080> AudioGroup;
    public float UnkE0;
    [SchemaField(0x110)]
    public float Unk110;
}

[SchemaStruct("0D8C8080", 0x18)]
public struct D2Class_0D8C8080
{
    public long FileSize;
    public DynamicArray<D2Class_0F8C8080> Audio;
}

[SchemaStruct("0F8C8080", 0x18)]
public struct D2Class_0F8C8080
{
    public TigerHash WwiseEventHash;
    [SchemaField(0x8)]
    public DynamicArray<D2Class_138C8080> Sounds;
}

[SchemaStruct("138C8080", 0x28)]
public struct D2Class_138C8080
{
    public short Unk00;
    public short Unk02;
    [SchemaField(0x8)]
    public TigerHash Unk08;
    [SchemaField(0x10)]
    public StringPointer WwiseEventName;
    [Tag64]
    public WwiseSound Sound;
}

[SchemaStruct("97318080", 0x540)]
public struct D2Class_97318080
{
}

[SchemaStruct("F62C8080", 0xB0)]
public struct D2Class_F62C8080
{
}

[SchemaStruct("F42C8080", 0x338)]
public struct D2Class_F42C8080
{
    [SchemaField(0x2c8)]
    public DynamicArray<D2Class_FA2C8080> PatternAudioGroups;
}

[SchemaStruct("FA2C8080", 0x258)]
public struct D2Class_FA2C8080
{
    [SchemaField(0x10)]
    public TigerHash WeaponContentGroupHash; // "weaponContentGroupHash" from API
    public TigerHash Unk14;
    public TigerHash Unk18;
    [SchemaField(0x30)]
    public TigerHash WeaponTypeHash1; // "weaponTypeHash" from API
    public byte Unk34;
    public byte Unk35;
    public byte Unk36;
    public byte Unk37;
    public float Unk38;
    public float Unk3C;
    public float Unk40;
    [SchemaField(0x48)]
    public int Unk48;
    [SchemaField(0x50)]
    public TigerHash Unk50;
    [SchemaField(0x60), Tag64]
    public Tag Unk60;
    [SchemaField(0x78), Tag64]
    public Tag Unk78;
    [SchemaField(0x90), Tag64]
    public Tag Unk90;
    [SchemaField(0xA8), Tag64]
    public Tag UnkA8;
    [SchemaField(0xC0), Tag64]
    public Tag UnkC0;
    [SchemaField(0xD8), Tag64]
    public Tag UnkD8;
    [SchemaField(0xF0), Tag64]
    public Tag<D2Class_A36F8080> AudioEntityParent;
    [SchemaField(0x120)]
    public TigerHash WeaponTypeHash2; // "weaponTypeHash" from API
    [SchemaField(0x130), Tag64]
    public Tag Unk130;
    [SchemaField(0x148), Tag64]
    public Tag Unk148;
    [SchemaField(0x168)]
    public int Unk168;
    [SchemaField(0x178)]
    public int Unk178;
    [SchemaField(0x180)]
    public float Unk180;
    public float Unk184;
    public float Unk188;
    public int Unk18C;
    public int Unk190;
    public float Unk194;
    public float Unk198;
    [SchemaField(0x1A8)]
    public int Unk1A8;
    public float Unk1AC;
    [SchemaField(0x1C0), Tag64]
    public Tag Unk1C0;
    [SchemaField(0x1D8), Tag64]
    public Tag Unk1D8;

    // public DynamicArray<D2Class_87978080> Unk1E8;
    // public DynamicArray<D2Class_84978080> Unk1F8;
    // public DynamicArray<D2Class_062D8080> Unk208;
    [SchemaField(0x248), Tag64]
    public Tag Unk248;
}


[SchemaStruct("2D098080", 0xA0)]
public struct D2Class_2D098080
{
    public long FileSize;
    public TigerHash Unk08;
    [SchemaField(0x18), Tag64]
    public Entity? Unk18;
    [SchemaField(0x30), Tag64]
    public Entity? Unk30;
    [SchemaField(0x48), Tag64]
    public Entity? Unk48;
    [SchemaField(0x60), Tag64]
    public Entity? Unk60;
    [SchemaField(0x78), Tag64]
    public Entity? Unk78;
    [SchemaField(0x90), Tag64]
    public Entity? Unk90;
}

[SchemaStruct("79818080", 0x390)]
public struct D2Class_79818080
{
    [SchemaField(0x1a8)]
    public DynamicArray<D2Class_F1918080> WwiseSounds1;
    public DynamicArray<D2Class_F1918080> WwiseSounds2;
}

[SchemaStruct("F1918080", 0x18)]
public struct D2Class_F1918080
{
    [SchemaField(0x10)]
    public ResourcePointer Unk10;
}


[SchemaStruct("40668080", 0x68)]
public struct D2Class_40668080
{
    [SchemaField(0x28), Tag64]
    public WwiseSound Sound;
}

[SchemaStruct("72818080", 0x18)]
public struct D2Class_72818080
{
}

[SchemaStruct("00488080", 0x20)]
public struct D2Class_00488080
{
}

[SchemaStruct("79948080", 0x300)]
public struct D2Class_79948080
{
}

[SchemaStruct("E3918080", 0x40)]
public struct D2Class_E3918080
{
}

#endregion


[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2999, "7F6B8080", 0x1C0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "95668080", 0x1E0)]
public struct CubemapResource //Dataresource for cubemaps
{
    [SchemaField(0x20)]
    public Vector4 CubemapSize; //XYZ, no W
    public Vector4 CubemapPosition; //Cubemap texture lines up with this one

    [SchemaField(0xF0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 UnkF0; //This might actually be position? Similar to other but in GDC image this one looked more correct

    [SchemaField(0x140, TigerStrategy.DESTINY2_SHADOWKEEP_2999)]
    [SchemaField(0x100, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Vector4 CubemapRotation;

    [SchemaField(0x190, TigerStrategy.DESTINY2_SHADOWKEEP_2999)]
    [SchemaField(0x1B0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public StringPointer CubemapName;

    [SchemaField(0x198, TigerStrategy.DESTINY2_SHADOWKEEP_2999)]
    [SchemaField(0x1B8, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Texture CubemapTexture;

    [SchemaField(0x1A0, TigerStrategy.DESTINY2_SHADOWKEEP_2999)]
    [SchemaField(0x1C0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Texture Unk1C0; //Sometype of reflection tint texture idk
}
