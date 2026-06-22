using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class WaterDecals
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.Water;
    public SMapWaterDecal Water;
    public MapTransform Transform;
    public WaterDecals(SMapWaterDecal entry)
    {
        Water = entry;
    }

    public void LoadIntoExporter(ExporterScene scene)
    {
        Transform transform = new()
        {
            Position = Transform.Translation.ToVec3(),
            Quaternion = Transform.Rotation,
            Rotation = Vector4.QuaternionToEulerAngles(Transform.Rotation),
            Scale = new(Transform.Translation.W)
        };

        List<DynamicMeshPart> parts = Water.Model.Load(ExportDetailLevel.MostDetailed, null);

        scene.AddMapModelParts($"{Water.Model.Hash}", parts.Where(x => x.RenderStage != TfxRenderStage.WaterReflection).ToList(), transform);
        scene.AddMapModelParts($"{Water.Model.Hash}_Reflection", parts.Where(x => x.RenderStage == TfxRenderStage.WaterReflection).ToList(), transform);

        foreach (DynamicMeshPart part in parts)
        {
            if (part.Material == null) continue;
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }
}

/// <summary>
/// Usually a flat plane for screen-space reflected water
/// </summary>
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80801A7E, 0x60)] //7E1A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80806DE0, 0x50)] //E06D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808068D4, 0x70)] //D4688080
public struct SMapWaterDecal
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public EntityModel Model;
}
