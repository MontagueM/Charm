namespace Tiger.Schema;

public class StaticMeshData : Tag<SStaticMeshData>
{
    public StaticMeshData(FileHash hash) : base(hash)
    {
    }

    public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent)
    {
        Dictionary<int, D2Class_376D8080> staticPartEntries = GetPartsOfDetailLevel(detailLevel);
        List<StaticPart> parts = GenerateParts(staticPartEntries, parent);
        return parts;
    }

    private List<StaticPart> GenerateParts(Dictionary<int, D2Class_376D8080> staticPartEntries, SStaticMesh parent)
    {
        List<StaticPart> parts = new();
        if (_tag.Meshes.Count > 1) throw new Exception("Multiple meshes not supported");
        if (_tag.Meshes.Count == 0) return new List<StaticPart>();
        D2Class_366D8080 mesh = _tag.Meshes[0];

        // Get material map
        int lowestDetail = 0xFF;
        foreach (var d2Class386D8080 in _tag.MaterialAssignments)
        {
            if (d2Class386D8080.DetailLevel < lowestDetail)
            {
                lowestDetail = d2Class386D8080.DetailLevel;
            }
        }

        Dictionary<int, Material> materialMap = new();
        for (var i = 0; i < _tag.MaterialAssignments.Count; i++)
        {
            var entry = _tag.MaterialAssignments[i];
            if (entry.DetailLevel == lowestDetail)
            {
                materialMap.Add(entry.PartIndex, parent.Materials[i].MaterialHash);
            }
        }

        foreach (var (i, staticPartEntry) in staticPartEntries)
        {
            if (materialMap.ContainsKey(i))
            {
                StaticPart part = new StaticPart(staticPartEntry);
                part.GetAllData(mesh, parent);
                part.Material = materialMap[i];
                parts.Add(part);
            }
        }
        return parts;
    }

    public Dictionary<int, D2Class_376D8080> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
    {
        Dictionary<int, D2Class_376D8080> staticPartEntries = new Dictionary<int, D2Class_376D8080>();

        if (detailLevel == ExportDetailLevel.MostDetailed)
        {
            for (int i = 0; i < _tag.Parts.Count; i++)
            {
                var staticPartEntry = _tag.Parts[i];
                if (staticPartEntry.DetailLevel == 1 || staticPartEntry.DetailLevel == 2 || staticPartEntry.DetailLevel == 10)
                {
                    staticPartEntries.Add(i, staticPartEntry);
                }
            }
        }
        else if (detailLevel == ExportDetailLevel.LeastDetailed)
        {
            for (int i = 0; i < _tag.Parts.Count; i++)
            {
                var staticPartEntry = _tag.Parts[i];
                if (staticPartEntry.DetailLevel != 1 && staticPartEntry.DetailLevel != 2 && staticPartEntry.DetailLevel != 10)
                {
                    staticPartEntries.Add(i, staticPartEntry);
                }
            }
        }
        else
        {
            for (int i = 0; i < _tag.Parts.Count; i++)
            {
                var staticPartEntry = _tag.Parts[i];
                staticPartEntries.Add(i, staticPartEntry);
            }
        }

        return staticPartEntries;
    }
}
