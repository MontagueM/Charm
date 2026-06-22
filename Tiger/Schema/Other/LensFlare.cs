using Tiger.Exporters;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class LensFlare : Tag<SLensFlare>
{
    public MapTransform Transform { get; set; }
    public List<FileHash> Materials { get; set; }
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.LensFlares;

    public LensFlare(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene) // Not ideal
    {
        Exporter.Get().GetGlobalScene().AddToGlobalScene(this);
        Materials = new();
        using TigerReader reader = GetReader();
        for (int i = 0; i < _tag.Entries.Count; i++)
        {
            SLensFlareEntry entry = _tag.Entries.ElementAt(reader, i);
            if (entry.Material == null) continue;
            entry.Material.RenderStage = TfxRenderStage.LensFlares;
            scene.Materials.Add(new ExportMaterial(entry.Material));
            Materials.Add(entry.Material.Hash);
        }
    }
}

/// <summary>
/// Light Lens Flares
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806CBF, 0x18)] //BF6C8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808067B5, 0x1C)] //B5678080
public struct SMapLensFlareResource
{
    [SchemaField(0x10)]
    public LensFlare LensFlare; // S80806A78
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F68, 0x38)] //686F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A78, 0x38)] //786A8080
public struct SLensFlare
{
    public ulong FileSize;
    [SchemaField(0x18)]
    public Tag<S80806DA1> Unk18;
    [SchemaField(0x20)]
    public DynamicArrayUnloaded<SLensFlareEntry> Entries;
    public TigerHash Unk30;
}

/// <summary>
/// Unk data resource.
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F6D, 0xC)] //6D6F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806A7D, 0xC)] //7D6A8080
public struct SLensFlareEntry
{
    public Material Material;
    public Tag<S80806DA1> Unk04;
    public int Unk08;
}
