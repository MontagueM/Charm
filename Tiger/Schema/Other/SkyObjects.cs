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

        int i = 0;
        foreach (S80806AA9 element in _tag.Entries)
        {
            if (element.Model.TagData.Model is null || (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && element.Unk70 == 5))
                continue;

            //Console.WriteLine($"{i} Hash {element.Model?.TagData.Model?.Hash}: Unk64 {element.Unk64}, Unk68 {element.Unk68}");

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
                Order = i, //element.Unk64 I guess the order is just the index? Idk
            });

            foreach (DynamicMeshPart part in element.Model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null))
            {
                if (part.Material == null) continue;
                part.Material.RenderStage = TfxRenderStage.Transparents;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }
            i++;
        }
    }
}

/// </summary>
/// Background entities/skybox resource
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801BDA, 0x10)] //DA1B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F91, 0x18)] //916F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806AA3, 0x18)] //A36A8080
public struct SMapSkyObjectsResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public SkyObjects SkyObjects;  // A76A8080
}

/// <summary>
/// Background entities/skybox
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801C1F, 0x68)] //1F1C8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F95, 0x68)] //956F8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80806AA7, 0x60)] //A76A8080
public struct SMapSkyObjects
{
    public long FileSize;
    public DynamicArray<S80806AA9> Entries;
    //public DynamicArray<S808093B3> Unk18;
    //public DynamicArray<SInt32> Unk28;
    [SchemaField(0x40)]
    public Vector4 Unk40;
    public Vector4 Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801C1E, 0x80)] //1E1C8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F97, 0x80)] //976F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806AA9, 0x90)] //A96A8080
public struct S80806AA9
{
    public Matrix4x4 Transform;
    public AABB Bounds;
    public Tag<S80806AAE> Model;
    //public float Unk64;

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
    public Tag<S80808AC5> Complex;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080064F, 0x1C)] //4F068080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808EF0, 0x1C)] //F08E8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808AC5, 0x1C)] //C58A8080
public struct S80808AC5
{
    public long FileSize;
    public int Unk08;
    public float Unk0C;
    public ResourcePointer Pointer; // 438B8080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800432, 0x80)] //32048080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F72, 0x80)] //728F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808B43, 0x80)] //438B8080
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, 0x80808B43, 0x70)] //438B8080
public struct S80808B43
{
    [SchemaField(0x10)]
    public DynamicArray<SInt16> Unk00;
    public DynamicArray<SInt16> Unk10;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801B3A, 0x10)] //3A1B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806F9B, 0x10)] //9B6F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80806AAE, 0x10)] //AE6A8080
public struct S80806AAE
{
    public long FileSize;
    public EntityModel Model;
}
