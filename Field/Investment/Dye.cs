using System.ComponentModel;
using System.Runtime.InteropServices;
using Field.General;
using Field.Models;
using Field;

namespace Field.Investment;

public class Dye : Tag
{
    public D2Class_BA6D8080 Header;
    
    public Dye(TagHash hash) : base(hash)
    {
        
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_BA6D8080>();
    }

    public DyeInfo GetDyeInfo()
    {
        Tag tag = PackageHandler.GetTag(typeof(Tag), PackageHandler.GetEntryReference(Header.DyeInfoHeader.Hash));
        return tag.GetData().ToStructure<DyeInfo>();
    }

    private static Dictionary<uint, string> ChannelNames = new Dictionary<uint, string>()
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

    private static List<string> ShaderDataNames = new List<string>()
    {
        "\"detail_diffuse_transform\"",
        "\"detail_normal_transform\"",
        "\"spec_aa_transform\"",
        "\"primary_albedo_tint\"",
        "\"primary_emissive_tint_color_and_intensity_bias\"",
        "\"primary_material_params\"",
        "\"primary_material_advanced_params\"",
        "\"primary_roughness_remap\"",
        "\"primary_worn_albedo_tint\"",
        "\"primary_wear_remap\"",
        "\"primary_worn_roughness_remap\"",
        "\"primary_worn_material_parameters\"",
        "\"secondary_albedo_tint\"",
        "\"secondary_emissive_tint_color_and_intensity_bias\"",
        "\"secondary_material_params\"",
        "\"secondary_material_advanced_params\"",
        "\"secondary_roughness_remap\"",
        "\"secondary_worn_albedo_tint\"",
        "\"secondary_wear_remap\"",
        "\"secondary_worn_roughness_remap\"",
        "\"secondary_worn_material_parameters\"",
    };
    
    public static string GetChannelName(DestinyHash channelHash)
    {
        return ChannelNames[channelHash];
    }

    public void ExportTextures(string savePath, ETextureFormat outputTextureFormat)
    {
        TextureExtractor.SetTextureFormat(outputTextureFormat);
        foreach (var entry in Header.DyeTextures)
        {
            TextureExtractor.SaveTextureToFile($"{savePath}/{entry.Texture.Hash}_{entry.TextureIndex}", entry.Texture.GetScratchImage());
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


[StructLayout(LayoutKind.Sequential, Size = 0x378)]
public struct D2Class_BA6D8080
{
    public long FileSize;
    [DestinyField(FieldType.RelativePointer)]
    public string DevName;
    public long Unk10;

    [DestinyOffset(0x48), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> DyeTextures;
    public DestinyHash Unk58;
    public DestinyHash Unk5C;
    
    [DestinyOffset(0x90), DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> DyeData;
    
    [DestinyOffset(0xB0)]
    public int UnkB0;
    [DestinyField(FieldType.TagHash)]
    public Tag DyeInfoHeader;
}