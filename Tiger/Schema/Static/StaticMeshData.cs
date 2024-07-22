using System.Diagnostics;
using System.Runtime.InteropServices;
using Tiger.Schema.Model;

namespace Tiger.Schema.Static
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferGroup
    {
        public Blob IndexBuffer;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public Blob[] VertexBuffers;
        public uint IndexOffset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StaticMeshInfo
    {
        public Vector4 MeshTransform;
        public float TexcoordScale;
        public Vector2 TexcoordTranslation;
        public uint Unk;
    }

    public interface IStaticMeshData : ISchema
    {
        public List<StaticPart> Load(ExportDetailLevel detailLevel, SStaticMesh parent);
        public List<BufferGroup> GetBuffers();
        List<int> GetStrides();
        Blob GetTransformsBlob();
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

        public Blob GetTransformsBlob() => throw new NotImplementedException();

        private List<StaticPart> GenerateParts(Dictionary<int, SStaticMeshPart> staticPartEntries, SStaticMesh parent)
        {
            List<StaticPart> parts = new();
            if (_tag.Buffers.Count == 0) return new List<StaticPart>();

            foreach (var (i, staticPartEntry) in staticPartEntries)
            {
                var material = parent.Materials[i].Material;
                if (material is null)// || material.Unk08 != 1)
                    continue;

                StaticPart part = new StaticPart(staticPartEntry);
                part.VertexLayoutIndex = _tag.MaterialAssignments[i].VertexLayoutIndex;
                part.Material = material;
                part.GetAllData(_tag.Buffers[staticPartEntry.BufferIndex], parent);
                parts.Add(part);
            }

            return parts;
        }

        public Dictionary<int, SStaticMeshPart> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = new Dictionary<int, SStaticMeshPart>();

            for (int i = 0; i < _tag.MaterialAssignments.Count; i++)
            {
                var mat = _tag.MaterialAssignments[i];
                var part = _tag.Parts[mat.PartIndex];

                if (!Globals.Get().ExportRenderStages.Contains((TfxRenderStage)mat.RenderStage))
                    continue;

                switch (detailLevel)
                {
                    case ExportDetailLevel.MostDetailed:
                        if (part.DetailLevel == 1 || part.DetailLevel == 2 || part.DetailLevel == 10)
                        {
                            staticPartEntries.Add(i, part);
                        }
                        break;
                    case ExportDetailLevel.LeastDetailed:
                        if (part.DetailLevel != 1 && part.DetailLevel != 2 && part.DetailLevel != 10)
                        {
                            staticPartEntries.Add(i, part);
                        }
                        break;
                    default:
                        staticPartEntries.Add(i, part);
                        break;
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
            Debug.Assert(_tag.MaterialAssignments.Count == parent.Materials.Count);
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
                if (buffers.VertexColor != null)
                {
                    bufferGroup.VertexBuffers[2] = buffers.VertexColor.ToBlob();
                }
                bufferGroup.IndexOffset = buffers.UnkOffset;
                bufferGroups.Add(bufferGroup);
            }

            return bufferGroups;
        }

        public List<int> GetStrides()
        {
            List<int> strides = new();
            if (_tag.Meshes.Count() == 0) return strides;
            if (_tag.Meshes[0].Vertices0 != null) strides.Add(_tag.Meshes[0].Vertices0.TagData.Stride);
            if (_tag.Meshes[0].Vertices1 != null) strides.Add(_tag.Meshes[0].Vertices1.TagData.Stride);
            if (_tag.Meshes[0].VertexColor != null) strides.Add(_tag.Meshes[0].VertexColor.TagData.Stride);
            return strides;
        }

        public Blob GetTransformsBlob()
        {
            using TigerReader reader = GetReader();
            reader.Seek(0x40, SeekOrigin.Begin);
            return new Blob(reader.ReadBytes(0x20));
        }

        private List<StaticPart> GenerateParts(Dictionary<int, SStaticMeshPart> staticPartEntries, SStaticMesh parent)
        {
            List<StaticPart> parts = new();
            if (_tag.Meshes.Count > 1) throw new Exception("Multiple meshes not supported");
            if (_tag.Meshes.Count == 0) return new List<StaticPart>();
            SStaticMeshBuffers mesh = _tag.Meshes[0];

            foreach (var (i, staticPartEntry) in staticPartEntries)
            {
                var material = parent.Materials[i].Material;
                if (material is null)// || material.Unk08 != 1)
                    continue;

                StaticPart part = new StaticPart(staticPartEntry);
                part.VertexLayoutIndex = _tag.MaterialAssignments[i].VertexLayoutIndex;
                part.Material = material;
                part.MaxVertexColorIndex = (int)_tag.MaxVertexColorIndex;
                part.GetAllData(mesh, parent);
                parts.Add(part);
            }
            return parts;
        }

        public Dictionary<int, SStaticMeshPart> GetPartsOfDetailLevel(ExportDetailLevel detailLevel)
        {
            Dictionary<int, SStaticMeshPart> staticPartEntries = new Dictionary<int, SStaticMeshPart>();

            for (int i = 0; i < _tag.MaterialAssignments.Count; i++)
            {
                var mat = _tag.MaterialAssignments[i];
                var part = _tag.Parts[mat.PartIndex];

                if (!Globals.Get().ExportRenderStages.Contains((TfxRenderStage)mat.RenderStage))
                    continue;

                Debug.Assert(part.BufferIndex == 0, $"{Hash} has part with buffer index {part.BufferIndex}");
                if (part.BufferIndex == 0)
                {
                    switch (detailLevel)
                    {
                        case ExportDetailLevel.MostDetailed:
                            if (part.DetailLevel == 1 || part.DetailLevel == 2 || part.DetailLevel == 10)
                            {
                                staticPartEntries.Add(i, part);
                            }
                            break;
                        case ExportDetailLevel.LeastDetailed:
                            if (part.DetailLevel != 1 && part.DetailLevel != 2 && part.DetailLevel != 10)
                            {
                                staticPartEntries.Add(i, part);
                            }
                            break;
                        default:
                            staticPartEntries.Add(i, part);
                            break;
                    }
                }
            }
            return staticPartEntries;
        }
    }
}
