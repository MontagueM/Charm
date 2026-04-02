using Tiger.Exporters;
using Tiger.Schema.Static;

namespace Tiger.Schema;

public class StaticMesh : Tag<SStaticMesh>
{
    public StaticMesh(FileHash hash) : base(hash) { }

    public void SaveMaterialsFromParts(ExporterScene scene, List<StaticPart> parts)
    {
        foreach (StaticPart part in parts)
        {
            if (part.Material == null)
            {
                continue;
            }
            scene.Materials.Add(new ExportMaterial(part.Material));
        }
    }


    /// <summary>
    /// Loads both main parts and decal parts of the static mesh.
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <returns></returns>
    public List<StaticPart> Load(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        List<StaticPart> mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
    }

    public Task<List<StaticPart>> LoadAsync(ExportDetailLevel detailLevel)
    {
        return Task.Run(() => Load(detailLevel));
    }

    /// <summary>
    /// Loads just the main parts of the static mesh (excludes decals).
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <returns></returns>
    public List<StaticPart> LoadMainParts(ExportDetailLevel detailLevel)
    {
        List<StaticPart> decalParts = LoadDecals(detailLevel);
        List<StaticPart> mainParts = _tag.StaticData.Load(detailLevel, _tag);
        mainParts.AddRange(decalParts);
        return mainParts;
    }

    /// <summary>
    /// Loads just the decal parts of the static mesh.
    /// </summary>
    /// <param name="detailLevel"></param>
    /// <returns></returns>
    public List<StaticPart> LoadDecals(ExportDetailLevel detailLevel)
    {
        List<StaticPart> parts = new();
        foreach (SStaticMeshDecal decalPartEntry in _tag.Decals)
        {
            if (!Globals.Get().GetExportStages().Contains((TfxRenderStage)decalPartEntry.GetRenderStage()))
                continue;

            if (detailLevel == ExportDetailLevel.MostDetailed)
            {
                if (decalPartEntry.LODLevel is not 1 and not 2 and not 10)
                {
                    continue;
                }
            }
            else if (detailLevel == ExportDetailLevel.LeastDetailed)
            {
                if (decalPartEntry.LODLevel is 1 or 2 or 10)
                {
                    continue;
                }
            }
            StaticPart part = new(decalPartEntry);
            part.GetDecalData(decalPartEntry, _tag);
            if (decalPartEntry.Material is not null)
            {
                part.Material = decalPartEntry.Material;
                part.Material.RenderStage = (TfxRenderStage)decalPartEntry.GetRenderStage();
            }
            parts.Add(part);
        }

        return parts;
    }
}
