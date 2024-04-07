using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tiger.Schema;


public class Dye : Tag<SDye>
{
    public Dye(FileHash hash) : base(hash) { }

    public DyeInfo GetDyeInfo()
    {
        TigerFile tag = FileResourcer.Get().GetFile(_tag.DyeInfoHeader.GetReferenceHash());
        return tag.GetData().ToType<DyeInfo>();
    }

    private static Dictionary<uint, string> ChannelNames = new()
    {
        {662199250, "ArmorPlate"},
        {1367384683, "ArmorSuit"},
        {218592586, "ArmorCloth"},
        {1667433279, "Weapon1"},
        {1667433278, "Weapon2"},
        {1667433277, "Weapon3"},
        {3073305669, "ShipUpper"},
        {3073305668, "ShipDecals"},
        {3073305671, "ShipLower"},
        {1971582085, "SparrowUpper"},
        {1971582084, "SparrowEngine"},
        {1971582087, "SparrowLower"},
        {373026848, "GhostMain"},
        {373026849, "GhostHighlights"},
        {373026850, "GhostDecals"},
    };

    public static string GetChannelName(TigerHash channelHash)
    {
        return ChannelNames[channelHash.Hash32];
    }

    public void ExportTextures(string savePath, TextureExportFormat outputTextureFormat)
    {
        TextureExtractor.SetTextureFormat(outputTextureFormat);
        foreach (var entry in _tag.DyeTextures)
        {
            TextureExtractor.SaveTextureToFile($"{savePath}/{entry.Texture.Hash}", entry.Texture.GetScratchImage());
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct DyeInfo
{
    [Description("DiffTrans")]
    public Vector4 DetailDiffuseTransform;
    [Description("NormTrans")]
    public Vector4 DetailNormalTransform;
    public Vector4 SpecAaTransform;
    [Description("CPrime")]
    public Vector4 PrimaryAlbedoTint;
    [Description("CPrimeEmit")]
    public Vector4 PrimaryEmissiveTintColorAndIntensityBias;
    [Description("PrimeMatParams")]
    public Vector4 PrimaryMaterialParams;
    [Description("PrimeAdvMatParams")]
    public Vector4 PrimaryMaterialAdvancedParams;
    [Description("PrimeRoughMap")]
    public Vector4 PrimaryRoughnessRemap;
    [Description("CPrimeWear")]
    public Vector4 PrimaryWornAlbedoTint;
    [Description("PrimeWearMap")]
    public Vector4 PrimaryWearRemap;
    [Description("PrimeWornRoughMap")]
    public Vector4 PrimaryWornRoughnessRemap;
    [Description("PrimeWornMatParams")]
    public Vector4 PrimaryWornMaterialParameters;
    [Description("CSecon")]
    public Vector4 SecondaryAlbedoTint;
    [Description("CSeconEmit")]
    public Vector4 SecondaryEmissiveTintColorAndIntensityBias;
    [Description("SeconMatParams")]
    public Vector4 SecondaryMaterialParams;
    [Description("SeconAdvMatParams")]
    public Vector4 SecondaryMaterialAdvancedParams;
    [Description("SeconRoughMap")]
    public Vector4 SecondaryRoughnessRemap;
    [Description("CSeconWear")]
    public Vector4 SecondaryWornAlbedoTint;
    [Description("SeconWearMap")]
    public Vector4 SecondaryWearRemap;
    [Description("SeconWornRoughMap")]
    public Vector4 SecondaryWornRoughnessRemap;
    [Description("SeconWornMatParams")]
    public Vector4 SecondaryWornMaterialParameters;
}


[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "BA6D8080", 0x378)]
public struct SDye
{
    public long FileSize;
    public StringPointer DevName;
    public long Unk10;

    [SchemaField(0x48)]
    public DynamicArray<STextureTag64> DyeTextures;
    public TigerHash Unk58;
    public TigerHash Unk5C;

    [SchemaField(0x90)]
    public DynamicArray<Vec4> DyeData;

    [SchemaField(0xB0)]
    public int UnkB0;
    public FileHash DyeInfoHeader;
}

public class DyeD1 : Tag<SDye_D1>
{
    public DyeD1(FileHash hash) : base(hash) { }

    private static Dictionary<uint, string> ChannelNames = new()
    {
        {662199250, "ArmorPlate"},
        {1367384683, "ArmorSuit"},
        {218592586, "ArmorCloth"},
        {1667433279, "Weapon1"},
        {1667433278, "Weapon2"},
        {1667433277, "Weapon3"},
        {3073305669, "ShipUpper"},
        {3073305668, "ShipDecals"},
        {3073305671, "ShipLower"},
        {1971582085, "SparrowUpper"},
        {1971582084, "SparrowEngine"},
        {1971582087, "SparrowLower"},
        {373026848, "GhostMain"},
        {373026849, "GhostHighlights"},
        {373026850, "GhostDecals"},
    };

    public static string GetChannelName(TigerHash channelHash)
    {
        return ChannelNames[channelHash.Hash32];
    }

    public void ExportTextures(string savePath, TextureExportFormat outputTextureFormat)
    {
        TextureExtractor.SetTextureFormat(outputTextureFormat);
        TextureExtractor.SaveTextureToFile($"{savePath}/{_tag.DetailDiffuse.Hash}", _tag.DetailDiffuse.GetScratchImage());
        TextureExtractor.SaveTextureToFile($"{savePath}/{_tag.DetailNormal.Hash}", _tag.DetailNormal.GetScratchImage());
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "F41A8080", 0xD0)]
public struct SDye_D1
{
    public long FileSize;
    public StringPointer DevName;

    [SchemaField(0x10)]
    public int SlotTypeIndex; // 0 Armor, 1 Cloth, 2 Suit

    [SchemaField(0x20)]
    public Texture Decal;

    [SchemaField(0x30)]
    public Vector4 DecalAlphaMapTransform;
    public int DecalBlendOption;

    [SchemaField(0x50)]
    public Vector4 SpecularProperties;

    public Texture DetailDiffuse;
    public Texture DetailNormal;

    [SchemaField(0x70)]
    public Vector4 DetailTransform;
    public Vector4 DetailNormalContributionStrength;
    public Vector4 PrimaryColor;
    public Vector4 SecondaryColor;
    public Vector4 SubsurfaceScatteringStrength;
}


