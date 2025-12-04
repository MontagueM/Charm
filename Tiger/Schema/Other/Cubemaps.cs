using Tiger.Exporters;

namespace Tiger.Schema;

public class Cubemap
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.Cubemaps;
    public SMapCubemapResource CubemapEntry;
    public MapTransform CubemapTransform { get; set; }
    public CubemapShape Shape { get; set; } = CubemapShape.Box;

    public Cubemap(SMapCubemapResource cubemap)
    {
        CubemapEntry = cubemap;

        if (cubemap.Unk70.W > 0)
            Shape = CubemapShape.Sphere;
    }

    public void LoadIntoExporter()
    {
        Exporter.Get().GetGlobalScene().AddToGlobalScene(this);
    }

    public enum CubemapShape
    {
        Box = 0,
        Sphere = 1
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "7F6B8080", 0x1C0)] // Map cubemaps dont exist in D1 but this needs to exist
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7F6B8080", 0x1C0)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "95668080", 0x1E0)]
[SchemaStruct(TigerStrategy.DESTINY2_FINAL_SHAPE_8264, "95668080", 0x1D0)]
public struct SMapCubemapResource // Dataresource for cubemaps
{
    // Theres a lot more data in here thats used for determining fade, relighting, parallax, etc but its not important in this context

    [SchemaField(0x20)]
    public Vector4 CubemapSize; // XYZ, no W
    public Vector4 CubemapCenter; // Visual center of the cubemap, not its actual position

    [SchemaField(0x70)]
    public Vector4 Unk70;

    [SchemaField(0xB0)]
    public long WorldID; // Same as the ID in the datatable entry

    [SchemaField(0x140, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x140, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x100, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Vector4 CubemapRotation; // Unsure if this is always the same as the map datatable entry

    [SchemaField(0x190, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x190, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1B0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1B0, TigerStrategy.DESTINY2_FINAL_SHAPE_8264, Obsolete = true)] // :(
    public StringPointer CubemapName;

    [SchemaField(0x198, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x198, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1B8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1AC, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Texture CubemapTexture;

    [SchemaField(0x1A0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x1A0, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x1C0, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x1B4, TigerStrategy.DESTINY2_FINAL_SHAPE_8264)]
    public Texture CubemapIBLTexture;
}
