
using System.Runtime.InteropServices;
using Tiger.Schema.Model;
using Tiger.Schema.Shaders;

namespace Tiger.Schema.Static
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferGroup
    {
        public Blob IndexBuffer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
        public Blob[] VertexBuffers;
        public uint IndexOffset;
    }

    public interface IStaticMeshData : ISchema
    {
        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent);
        public List<BufferGroup> GetBuffers();
        List<int> GetStrides();
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

        public List<BufferGroup> GetBuffers() => throw new NotImplementedException();

        public List<int> GetStrides() => throw new NotImplementedException();

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

        public List<BufferGroup> GetBuffers()
        {
            List<BufferGroup> bufferGroups = new();
            foreach (SStaticMeshBuffers buffers in _tag.Meshes)
            {
                BufferGroup bufferGroup = new();
                bufferGroup.IndexBuffer = buffers.Indices.ToBlob();
                bufferGroup.VertexBuffers = new Blob[3];
                bufferGroup.VertexBuffers[0] = buffers.Vertices0.ToBlob();
                if (buffers.Vertices1 != null)
                {
                    bufferGroup.VertexBuffers[1] = buffers.Vertices1.ToBlob();
                }
                if (buffers.Vertices2 != null)
                {
                    bufferGroup.VertexBuffers[2] = buffers.Vertices2.ToBlob();
                }
                bufferGroup.IndexOffset = buffers.UnkOffset;
                bufferGroups.Add(bufferGroup);
            }

            return bufferGroups;
        }

        public List<int> GetStrides()
        {
            List<int> strides = new();
            if (_tag.Meshes[0].Vertices0 != null) strides.Add(_tag.Meshes[0].Vertices0.TagData.Stride);
            if (_tag.Meshes[0].Vertices1 != null) strides.Add(_tag.Meshes[0].Vertices1.TagData.Stride);
            if (_tag.Meshes[0].Vertices2 != null) strides.Add(_tag.Meshes[0].Vertices2.TagData.Stride);
            return strides;
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
