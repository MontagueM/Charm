using Tiger.Exporters;

namespace Tiger.Schema;

public class ShadowingLights : Tag<SMapShadowingLight>
{
    public ShadowingLights(FileHash hash) : base(hash)
    {
    }

    public void LoadIntoExporter(ExporterScene scene, SMapDataEntry mapEntry)
    {
        var data = _tag.BufferData;
        if (data is null)
            return;

        // This sucks and is stupid
        var color = data.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).Count() != 0 ?
            data.TagData.Buffer1.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).FirstOrDefault().Vec :
            data.TagData.Buffer2.Where(x => (x.Vec.X != 0 && x.Vec.Y != 0 && x.Vec.Z != 0 && x.Vec.W != 0)).FirstOrDefault().Vec;

        Lights.LightData lightData = new()
        {
            Hash = data.Hash,
            LightType = Lights.LightType.Shadowing,
            Color = color,
            Size = new Vector2(_tag.HalfFOV * 2.0f, 1f),
            Range = _tag.Distance.W,
            Transform = new()
            {
                Position = mapEntry.Translation.ToVec3(),
                Quaternion = mapEntry.Rotation
            }
        };

        scene.AddMapLight(lightData);
    }
}
