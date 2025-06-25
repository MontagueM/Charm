using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class SkyObjects : Tag<SMapSkyObjects>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.SkyTransparent;
    public SkyObjects(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        var _config = ConfigSubsystem.Get();

        if (_tag.Entries is null)
            return;

        foreach ((int i, SA96A8080 element) in _tag.Entries.Select((value, index) => (index, value)))
        {
            if (element.Model.TagData.Model is null || (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && element.Unk70 == 5))
                continue;

            Matrix4x4 matrix = element.Transform;

            Vector3 scale = new();
            Vector4 trans = new();
            Vector4 quat = new();
            matrix.Decompose(out trans, out quat, out scale);

            scene.AddMapModel(element.Model.TagData.Model, new Transform
            {
                Position = trans.ToVec3(),
                Rotation = Vector4.QuaternionToEulerAngles(quat),
                Quaternion = quat,
                Scale = scale,
                Order = element.Unk68
            });

            foreach (DynamicMeshPart part in element.Model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null))
            {
                if (part.Material == null) continue;
                part.Material.RenderStage = TfxRenderStage.Transparents;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }
        }
    }
}

/// </summary>
/// Background entities/skybox resource
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DA1B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "916F8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A36A8080", 0x18)]
public struct SMapSkyObjectsResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public SkyObjects SkyObjects;  // A76A8080
}

/// <summary>
/// Background entities/skybox
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1F1C8080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "956F8080", 0x68)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A76A8080", 0x60)]
public struct SMapSkyObjects
{
    public long FileSize;
    public DynamicArray<SA96A8080> Entries;
    //public DynamicArray<SB3938080> Unk18;
    //public DynamicArray<SInt32> Unk28;
    [SchemaField(0x40)]
    public Vector4 Unk40;
    public Vector4 Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "1E1C8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "976F8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A96A8080", 0x90)]
public struct SA96A8080
{
    public Matrix4x4 Transform;
    public AABB Bounds;
    public Tag<SAE6A8080> Model;

    [SchemaField(0x60, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x64, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x68, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public float Unk68; // Ordering?

    [SchemaField(0x68, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x6C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public int Unk70; // if 5, skip the model??

    [SchemaField(0x78, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x7C, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<SC58A8080> Complex;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "4F068080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F08E8080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C58A8080", 0x1C)]
public struct SC58A8080
{
    public long FileSize;
    public int Unk08;
    public float Unk0C;
    public ResourcePointer Pointer; // 438B8080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "32048080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "728F8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "438B8080", 0x80)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "438B8080", 0x70)]
public struct S438B8080
{
    [SchemaField(0x10)]
    public DynamicArray<SInt16> Unk00;
    public DynamicArray<SInt16> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "3A1B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "9B6F8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "AE6A8080", 0x10)]
public struct SAE6A8080
{
    public long FileSize;
    public EntityModel Model;
}
