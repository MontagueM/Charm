using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Static;


public class Decorator : Tag<SDecorator>
{
    public Decorator(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene scene, string saveDirectory)
    {
        var models = _tag.DecoratorModels;
        // Model transform offsets
        List<Vector4> data = new();
        TigerFile container = new(_tag.BufferData.TagData.Unk14.Hash);
        byte[] containerData = container.GetData();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }

        using TigerReader reader = _tag.BufferData.TagData.InstanceBuffer.GetReferenceReader();
        for (int i = 0; i < _tag.InstanceRanges.Count - 1; i++)
        {
            var start = _tag.InstanceRanges[i].Unk00;
            var end = _tag.InstanceRanges[i + 1].Unk00;
            var count = end - start;

            var dynID = models.Count == 1 ? i : 0;
            var model = models[models.Count == 1 ? 0 : i].DecoratorModel;
            if (model.TagData.SpeedTreeData != null)
                continue; // TODO: Trees, skip for now

            var parts = GenerateParts(model.TagData.Model); //.Load(ExportDetailLevel.MostDetailed, null);
            foreach (DynamicMeshPart part in parts)
            {
                if (part.Material == null) continue;
                scene.Materials.Add(new ExportMaterial(part.Material));

            }

            for (int j = 0; j < count; j++)
            {
                reader.BaseStream.Seek((start + j) * 0x10, SeekOrigin.Begin);
                var a = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                var b = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                //Console.WriteLine($"Model {model.Hash} - External ID {dynID} - Instance {j}: Pos {data[0] * a + data[1]} | Rot {data[2] * b + data[3]}");

                Transform transform = new Transform
                {
                    Position = (data[0] * a + data[1]).ToVec3(),
                    Quaternion = data[2] * b + data[3],
                    Rotation = Vector4.QuaternionToEulerAngles(data[2] * b + data[3]),
                    Scale = new((data[0] * a + data[1]).W)
                };
                scene.AddMapModelParts($"{model.Hash}_{dynID}", parts.Where(x => x.GroupIndex == dynID).ToList(), transform);
            }
        }
    }

    // Should just use EntityModel.Load but we need to get just the first mesh entry in Meshes since the rest are LODss
    private List<DynamicMeshPart> GenerateParts(EntityModel model)
    {
        var dynamicParts = GetPartsOfDetailLevel(model, ExportDetailLevel.MostDetailed);
        List<DynamicMeshPart> parts = new();
        List<int> exportPartRange = new();
        if (model.TagData.Meshes.Count == 0) return parts;

        var mesh = model.TagData.Meshes[model.GetReader(), 0];
        exportPartRange = EntityModel.GetExportRanges(mesh);

        foreach ((int i, D2Class_CB6E8080 part) in dynamicParts[0])
        {
            if (!exportPartRange.Contains(i))
                continue;

            DynamicMeshPart dynamicMeshPart = new(part, null)
            {
                Index = i,
                GroupIndex = part.ExternalIdentifier,
                LodCategory = part.LodCategory,
                bAlphaClip = (part.GetFlags() & 0x8) != 0,
                VertexLayoutIndex = Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ?
                mesh.GetInputLayoutForStage(0) : mesh.GetInputLayoutForStage(0) / 4
            };

            if (Strategy.CurrentStrategy > TigerStrategy.DESTINY1_RISE_OF_IRON)
            {
                if (dynamicMeshPart.Material is null ||
                dynamicMeshPart.Material.VertexShader is null ||
                dynamicMeshPart.Material.PixelShader is null)
                    continue;
            }
            else
            {
                if (dynamicMeshPart.Material is null ||
                dynamicMeshPart.Material.VertexShader is null ||
                dynamicMeshPart.Material.PixelShader is null)
                    continue;
            }

            dynamicMeshPart.GetAllData(mesh, model.TagData);
            parts.Add(dynamicMeshPart);
        }

        return parts;
    }

    private Dictionary<int, Dictionary<int, D2Class_CB6E8080>> GetPartsOfDetailLevel(EntityModel model, ExportDetailLevel eDetailLevel)
    {
        Dictionary<int, Dictionary<int, D2Class_CB6E8080>> parts = new();
        using TigerReader reader = model.GetReader();

        int meshIndex = 0;
        int partIndex = 0;
        var mesh = model.TagData.Meshes[reader, 0];

        parts.Add(meshIndex, new Dictionary<int, D2Class_CB6E8080>());
        for (int i = 0; i < mesh.Parts.Count; i++)
        {
            D2Class_CB6E8080 part = mesh.Parts[reader, i];
            if (eDetailLevel == ExportDetailLevel.AllLevels)
            {
                parts[meshIndex].Add(partIndex, part);
            }
            else
            {
                if (eDetailLevel == ExportDetailLevel.MostDetailed && part.LodCategory is ELodCategory.MainGeom0 or ELodCategory.GripStock0 or ELodCategory.Stickers0 or ELodCategory.InternalGeom0 or ELodCategory.Detail0)
                {
                    parts[meshIndex].Add(partIndex, part);
                }
                else if (eDetailLevel == ExportDetailLevel.LeastDetailed && part.LodCategory == ELodCategory.LowPolyGeom3)
                {
                    parts[meshIndex].Add(partIndex, part);
                }
            }
            partIndex++;
        }



        return parts;
    }
}
