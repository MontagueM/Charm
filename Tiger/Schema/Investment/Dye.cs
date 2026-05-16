using System.ComponentModel;
using System.Runtime.InteropServices;
using Tiger.Exporters;

namespace Tiger.Schema;


public class Dye : Tag<SScope>
{
    public Dye(FileHash hash) : base(hash) { }

    public DyeInfo GetDyeInfo()
    {
        // Bungie stopped using the DyeInfo file? Gotta do it the messy way I guess
        //TigerFile tag = FileResourcer.Get().GetFile(_tag.DyeInfoHeader.GetReferenceHash());
        //return tag.GetData().ToType<DyeInfo>();

        List<Vector4> values = _tag.Pixel.Value.GetCBuffer();
        DyeInfo dyeInfo = new();

        dyeInfo.DetailDiffuseTransform = values[0];
        dyeInfo.DetailNormalTransform = values[1];
        dyeInfo.SpecAaTransform = values[2];
        dyeInfo.PrimaryAlbedoTint = values[3];
        dyeInfo.PrimaryEmissiveTintColorAndIntensityBias = values[4];
        dyeInfo.PrimaryMaterialParams = values[5];
        dyeInfo.PrimaryMaterialAdvancedParams = values[6];
        dyeInfo.PrimaryRoughnessRemap = values[7];
        dyeInfo.PrimaryWornAlbedoTint = values[8];
        dyeInfo.PrimaryWearRemap = values[9];
        dyeInfo.PrimaryWornRoughnessRemap = values[10];
        dyeInfo.PrimaryWornMaterialParameters = values[11];
        dyeInfo.SecondaryAlbedoTint = values[12];
        dyeInfo.SecondaryEmissiveTintColorAndIntensityBias = values[13];
        dyeInfo.SecondaryMaterialParams = values[14];
        dyeInfo.SecondaryMaterialAdvancedParams = values[15];
        dyeInfo.SecondaryRoughnessRemap = values[16];
        dyeInfo.SecondaryWornAlbedoTint = values[17];
        dyeInfo.SecondaryWearRemap = values[18];
        dyeInfo.SecondaryWornRoughnessRemap = values[19];
        dyeInfo.SecondaryWornMaterialParameters = values[20];

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
        TextureExporter.SetTextureFormat(outputTextureFormat);
        foreach (STextureTag entry in _tag.Pixel.Value.EnumerateTextures())
        {
            TextureExporter.SaveTextureToFile($"{savePath}/{entry.Texture.Hash}", entry.Texture.GetScratchImage());
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
    public DynamicStruct<SScopeStage> Pixel;
    public DynamicStruct<SScopeStage> Vertex;
}

[NonSchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x88)]
public struct SScopeStage
{
    public DynamicArray<STextureTag> Textures;
    public TigerHash Unk58;
    public TigerHash Unk5C;

    public DynamicArray<SUInt8> TFX_Bytecode;
    public DynamicArray<Vec4> TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTag> Samplers;
    public DynamicArray<Vec4> CBuffers; // Fallback if Vector4Container doesn't exist, I guess..?

    [SchemaField(0x68)]
    public int CBufferSlot;
    public FileHash Vector4Container; // Is just empty sometimes for some reason

    public IEnumerable<STextureTag> EnumerateTextures()
    {
        foreach (STextureTag texture in Textures)
        {
            yield return texture;
        }
    }

    public IEnumerable<DirectXSampler> EnumerateSamplers()
    {
        foreach (SDirectXSamplerTag sampler in Samplers)
        {
            yield return sampler.GetSampler();
        }
    }

    public List<Vector4> GetCBuffer()
    {
        List<Vector4> data = new();
        if (Vector4Container.IsValid() && !GetVec4Container().All(x => x == Vector4.Zero))
        {
            data = GetVec4Container();
        }
        else
        {
            foreach (Vec4 vec in CBuffers)
            {
                data.Add(vec.Vec);
            }
        }
        return data;
    }

    public List<Vector4> GetVec4Container()
    {
        List<Vector4> data = new();
        TigerFile container = new(Vector4Container.GetReferenceHash());
        byte[] containerData = container.GetData();

        for (int i = 0; i < containerData.Length / 16; i++)
        {
            data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }

        return data;
    }

    public TfxBytecodeInterpreterHLSL GetBytecode()
    {
        return new TfxBytecodeInterpreterHLSL(TfxBytecodeOp.ParseAll(TFX_Bytecode));
    }
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
        TextureExporter.SetTextureFormat(outputTextureFormat);
        if (_tag.DetailDiffuse is not null)
            TextureExporter.SaveTextureToFile($"{savePath}/{_tag.DetailDiffuse.Hash}", _tag.DetailDiffuse.GetScratchImage());
        if (_tag.DetailNormal is not null)
            TextureExporter.SaveTextureToFile($"{savePath}/{_tag.DetailNormal.Hash}", _tag.DetailNormal.GetScratchImage());
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


