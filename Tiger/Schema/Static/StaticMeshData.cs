
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static
{
    public interface IStaticMeshData : ISchema
    {
        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent);
    }
}

namespace Tiger.Schema.Static.DESTINY2_SHADOWKEEP_2601
{
    public class StaticMeshData : Tag<SStaticMeshData_SK>, IStaticMeshData
    {
        public StaticMeshData(FileHash hash) : base(hash)
        {
        }

        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = GetPartsOfDetailLevel(detailLevel);
            List<StaticPart> parts = GenerateParts(staticPartEntries, parent);
            return parts;
        }

        private List<StaticPart> GenerateParts(Dictionary<int, SStaticMeshPart> staticPartEntries, SStaticMesh parent)
        {
            List<StaticPart> parts = new();
            if (_tag.Buffers.Count == 0) return new List<StaticPart>();

            // Get material map
            int lowestDetail = 0xFF;
            foreach (var d2Class386D8080 in _tag.MaterialAssignments)
            {
                if (d2Class386D8080.DetailLevel < lowestDetail)
                {
                    lowestDetail = d2Class386D8080.DetailLevel;
                }
            }

            Dictionary<int, IMaterial> materialMap = new();
            for (var i = 0; i < _tag.MaterialAssignments.Count; i++)
            {
                var entry = _tag.MaterialAssignments[i];
                if (entry.DetailLevel == lowestDetail)
                {
                    materialMap.Add(entry.PartIndex, parent.Materials[i].Material);
                }
            }

            foreach (var (i, staticPartEntry) in staticPartEntries)
            {
                if (materialMap.ContainsKey(i))
                {
                    StaticPart part = new(staticPartEntry);
                    part.Material = materialMap[i];
                    part.GetAllData(_tag.Buffers[staticPartEntry.BufferIndex], parent);
                    parts.Add(part);
                }
            }

            return parts;
        }

        public Dictionary<int, SStaticMeshPart> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = new Dictionary<int, SStaticMeshPart>();

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
}

//TODO: collapse this into witch queen version?
namespace Tiger.Schema.Static.DESTINY2_BEYONDLIGHT_3402
{
    public class StaticMeshData : Tag<SStaticMeshData_BL>, IStaticMeshData
    {
        public StaticMeshData(FileHash hash) : base(hash)
        {
        }

        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = GetPartsOfDetailLevel(detailLevel);
            List<StaticPart> parts = GenerateParts(staticPartEntries, parent);
            return parts;
        }

        private List<StaticPart> GenerateParts(Dictionary<int, SStaticMeshPart> staticPartEntries, SStaticMesh parent)
        {
            List<StaticPart> parts = new();
            if (_tag.Meshes.Count > 1) throw new Exception("Multiple meshes not supported");
            if (_tag.Meshes.Count == 0) return new List<StaticPart>();
            SStaticMeshBuffers mesh = _tag.Meshes[0];

            // Get material map
            int lowestDetail = 0xFF;
            foreach (var d2Class386D8080 in _tag.MaterialAssignments)
            {
                if (d2Class386D8080.DetailLevel < lowestDetail)
                {
                    lowestDetail = d2Class386D8080.DetailLevel;
                }
            }

            Dictionary<int, IMaterial> materialMap = new();
            for (var i = 0; i < _tag.MaterialAssignments.Count; i++)
            {
                var entry = _tag.MaterialAssignments[i];
                if (entry.DetailLevel == lowestDetail)
                {
                    materialMap.Add(entry.PartIndex, parent.Materials[i].Material);
                }
            }

            foreach (var (i, staticPartEntry) in staticPartEntries)
            {
                if (materialMap.ContainsKey(i))
                {
                    StaticPart part = new StaticPart(staticPartEntry);
                    part.Material = materialMap[i];
                    part.GetAllData(mesh, parent);
                    parts.Add(part);
                }
            }
            return parts;
        }

        public Dictionary<int, SStaticMeshPart> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = new Dictionary<int, SStaticMeshPart>();

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
}



namespace Tiger.Schema.Static.DESTINY2_WITCHQUEEN_6307
{
    public class StaticMeshData : Tag<SStaticMeshData_BL>, IStaticMeshData
    {
        public StaticMeshData(FileHash hash) : base(hash)
        {
        }

        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = GetPartsOfDetailLevel(detailLevel);
            List<StaticPart> parts = GenerateParts(staticPartEntries, parent);
            return parts;
        }

        private List<StaticPart> GenerateParts(Dictionary<int, SStaticMeshPart> staticPartEntries, SStaticMesh parent)
        {
            List<StaticPart> parts = new();
            if (_tag.Meshes.Count > 1) throw new Exception("Multiple meshes not supported");
            if (_tag.Meshes.Count == 0) return new List<StaticPart>();
            SStaticMeshBuffers mesh = _tag.Meshes[0];

            // Get material map
            int lowestDetail = 0xFF;
            foreach (var d2Class386D8080 in _tag.MaterialAssignments)
            {
                if (d2Class386D8080.DetailLevel < lowestDetail)
                {
                    lowestDetail = d2Class386D8080.DetailLevel;
                }
            }

            Dictionary<int, IMaterial> materialMap = new();
            for (var i = 0; i < _tag.MaterialAssignments.Count; i++)
            {
                var entry = _tag.MaterialAssignments[i];
                if (entry.DetailLevel == lowestDetail)
                {
                    materialMap.Add(entry.PartIndex, parent.Materials[i].Material);
                }
            }

            foreach (var (i, staticPartEntry) in staticPartEntries)
            {
                if (materialMap.ContainsKey(i))
                {
                    StaticPart part = new StaticPart(staticPartEntry);
                    part.Material = materialMap[i];
                    part.GetAllData(mesh, parent);
                    parts.Add(part);
                }
            }
            return parts;
        }

        public Dictionary<int, SStaticMeshPart> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = new Dictionary<int, SStaticMeshPart>();

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
}
