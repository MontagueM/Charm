using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Tiger.Schema;


public class Dye : Tag<SScope>
{
    public Dye(FileHash hash) : base(hash) { }

    public DyeInfo GetDyeInfo()
    {
        // Bungie stopped using the DyeInfo file? Gotta do it the messy way I guess
        //TigerFile tag = FileResourcer.Get().GetFile(_tag.DyeInfoHeader.GetReferenceHash());
        //return tag.GetData().ToType<DyeInfo>();

        DynamicArray<Vec4> values = _tag.CBufferData;
        DyeInfo dyeInfo = new();

        dyeInfo.DetailDiffuseTransform = values[0].Vec;
        dyeInfo.DetailNormalTransform = values[1].Vec;
        dyeInfo.SpecAaTransform = values[2].Vec;
        dyeInfo.PrimaryAlbedoTint = values[3].Vec;
        dyeInfo.PrimaryEmissiveTintColorAndIntensityBias = values[4].Vec;
        dyeInfo.PrimaryMaterialParams = values[5].Vec;
        dyeInfo.PrimaryMaterialAdvancedParams = values[6].Vec;
        dyeInfo.PrimaryRoughnessRemap = values[7].Vec;
        dyeInfo.PrimaryWornAlbedoTint = values[8].Vec;
        dyeInfo.PrimaryWearRemap = values[9].Vec;
        dyeInfo.PrimaryWornRoughnessRemap = values[10].Vec;
        dyeInfo.PrimaryWornMaterialParameters = values[11].Vec;
        dyeInfo.SecondaryAlbedoTint = values[12].Vec;
        dyeInfo.SecondaryEmissiveTintColorAndIntensityBias = values[13].Vec;
        dyeInfo.SecondaryMaterialParams = values[14].Vec;
        dyeInfo.SecondaryMaterialAdvancedParams = values[15].Vec;
        dyeInfo.SecondaryRoughnessRemap = values[16].Vec;
        dyeInfo.SecondaryWornAlbedoTint = values[17].Vec;
        dyeInfo.SecondaryWearRemap = values[18].Vec;
        dyeInfo.SecondaryWornRoughnessRemap = values[19].Vec;
        dyeInfo.SecondaryWornMaterialParameters = values[20].Vec;

        return dyeInfo;
    }

    // Very few cases where this will get used, based on the Primer shader
    public static DyeInfo DefaultDye()
    {
        DyeInfo defaultDye = new();

        defaultDye.DetailDiffuseTransform = new(1.5f, 1.5f, 0f, 0f);
        defaultDye.DetailNormalTransform = new(1.5f, 1.5f, 0f, 0f);
        defaultDye.SpecAaTransform = Vector4.Zero;
        defaultDye.PrimaryAlbedoTint = new(0.1743545, 0.1743545, 0.1743545, 1.0);
        defaultDye.PrimaryEmissiveTintColorAndIntensityBias = new(0, 0, 0, 1.0);
        defaultDye.PrimaryMaterialParams = Vector4.Zero;
        defaultDye.PrimaryMaterialAdvancedParams = new(0f, -1f, 0f, 1f);
        defaultDye.PrimaryRoughnessRemap = new(0.8108102, 0.16216499, 0, 0.5);
        defaultDye.PrimaryWornAlbedoTint = new(0.1746474, 0.1746474, 0.1746474, 1.0);
        defaultDye.PrimaryWearRemap = new(-0.5f, 4f, 0f, 1f);
        defaultDye.PrimaryWornRoughnessRemap = new(0.818018, 0.13693798, 0, 0.5);
        defaultDye.PrimaryWornMaterialParameters = Vector4.Zero;

        defaultDye.SecondaryAlbedoTint = new(0.1746474, 0.1746474, 0.1746474, 1.0);
        defaultDye.SecondaryEmissiveTintColorAndIntensityBias = new(0, 0, 0, 1.0);
        defaultDye.SecondaryMaterialParams = Vector4.Zero;
        defaultDye.SecondaryMaterialAdvancedParams = new(0f, -1f, 0f, 1f);
        defaultDye.SecondaryRoughnessRemap = new(0.798198, 0.13873982, 0, 0.5);
        defaultDye.SecondaryWornAlbedoTint = new(0.1746474, 0.1746474, 0.1746474, 1.0);
        defaultDye.SecondaryWearRemap = new(-0.5f, 4f, 0f, 1f);
        defaultDye.SecondaryWornRoughnessRemap = new(0.854054, 0.046848, 0, 0.5);
        defaultDye.SecondaryWornMaterialParameters = Vector4.Zero;

        return defaultDye;
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
        foreach (STextureTag entry in _tag.Textures)
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
public struct SScope
{
    public long FileSize;
    public StringPointer DevName;
    public long Unk10;

    [SchemaField(0x48)]
    public DynamicArray<STextureTag> Textures;
    public TigerHash Unk58;
    public TigerHash Unk5C;
    public DynamicArray<SUInt8> Bytecode;
    public DynamicArray<Vec4> BytecodeConstants;

    [SchemaField(0x90)]
    public DynamicArray<Vec4> CBufferData;

    [SchemaField(0xB0)]
    public int CBufferSlot;
    public FileHash Vec4Container; // Is just empty sometimes for some reason
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
        if (_tag.DetailDiffuse is not null)
            TextureExtractor.SaveTextureToFile($"{savePath}/{_tag.DetailDiffuse.Hash}", _tag.DetailDiffuse.GetScratchImage());
        if (_tag.DetailNormal is not null)
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


