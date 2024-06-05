using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Model;

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
        List<Vector4> SpeedtreePlacements = new() { Vector4.Zero, Vector4.Zero.WithW(1) };

        TigerFile container = new(_tag.BufferData.TagData.Unk14.Hash);
        byte[] containerData = container.GetData();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            SpeedtreePlacements.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
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

            var parts = GenerateParts(model.TagData.Model, model.TagData.SpeedTreeData != null ? 19 : 26); //.Load(ExportDetailLevel.MostDetailed, null);
            foreach (DynamicMeshPart part in parts)
            {
                if (part.Material == null) continue;
                scene.Materials.Add(new ExportMaterial(part.Material));
            }

            for (int j = 0; j < count; j++)
            {
                reader.BaseStream.Seek((start + j) * 0x10, SeekOrigin.Begin);
                var pos = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
                var rot = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

                Transform transform = new Transform
                {
                    Position = (SpeedtreePlacements[2] * pos + SpeedtreePlacements[3]).ToVec3(),
                    Quaternion = (SpeedtreePlacements[4] * rot + SpeedtreePlacements[5]),
                    Rotation = Vector4.QuaternionToEulerAngles((SpeedtreePlacements[4] * rot + SpeedtreePlacements[5])),
                    Scale = new((SpeedtreePlacements[2] * pos + SpeedtreePlacements[3]).W)
                };

                scene.AddMapModelParts($"{model.Hash}_{dynID}", parts.Where(x => x.GroupIndex == dynID).ToList(), transform);
            }

            // Trees need(?) their vertex shader to transform correctly...
            //if (model.TagData.SpeedTreeData != null)
            //{
            //    var scale = model.TagData.SpeedTreeData.TagData.Unk08[0].Unk00;
            //    var offset = model.TagData.SpeedTreeData.TagData.Unk08[0].Unk10;
            //    foreach (var part in parts)
            //    {
            //        for (int k = 0; k < part.VertexPositions.Count; k++)
            //        {
            //            part.VertexPositions[k] = new Vector4(
            //                part.VertexPositions[k].X * scale.X + offset.X,
            //                part.VertexPositions[k].Y * scale.Y + offset.Y,
            //                part.VertexPositions[k].Z * scale.Z + offset.Z,
            //                part.VertexPositions[k].W
            //            );
            //        }
            //    }

            //    var uvTransform = model.TagData.SpeedTreeData.TagData.Unk08[0].Unk20;
            //    foreach (var part in parts)
            //    {
            //        for (int k = 0; k < part.VertexTexcoords0.Count; k++)
            //        {
            //            part.VertexTexcoords0[k] = new Vector2(
            //                part.VertexTexcoords0[k].X * uvTransform.X + uvTransform.Z,
            //                part.VertexTexcoords0[k].Y * -uvTransform.Y + 1 - uvTransform.W
            //            );
            //        }
            //    }
            //}
        }
    }

    // Should just use EntityModel.Load but we need to get just the first mesh entry in Meshes since the rest are LODs
    private List<DynamicMeshPart> GenerateParts(EntityModel model, int layoutIndex)
    {
        var dynamicParts = GetPartsOfDetailLevel(model);
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
                mesh.GetInputLayoutForStage(0) : layoutIndex
            };

            if (dynamicMeshPart.Material is null ||
            dynamicMeshPart.Material.VertexShader is null ||
            dynamicMeshPart.Material.PixelShader is null)
                continue;

            dynamicMeshPart.GetAllData(mesh, model.TagData);
            parts.Add(dynamicMeshPart);
        }

        return parts;
    }

    private Dictionary<int, Dictionary<int, D2Class_CB6E8080>> GetPartsOfDetailLevel(EntityModel model)
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
            if (part.LodCategory is ELodCategory.MainGeom0 or ELodCategory.GripStock0 or ELodCategory.Stickers0 or ELodCategory.InternalGeom0 or ELodCategory.Detail0)
                parts[meshIndex].Add(partIndex, part);

            partIndex++;
        }

        return parts;
    }

}

#region Decorator structs
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "361C8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AD718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "C36C8080", 0x18)]
public struct SDecoratorMapResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Decorator Decorator;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CE1A8080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "64718080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "986C8080", 0xA8)]
public struct SDecorator
{
    public ulong Size;
    public DynamicArray<D2Class_B16C8080> DecoratorModels;
    public DynamicArray<D2Class_07008080> InstanceRanges;
    public DynamicArray<D2Class_07008080> Unk28;
    public DynamicArray<D2Class_07008080> Unk38;
    public Tag<D2Class_A46C8080> BufferData;
    public Tag<SOcclusionBounds> OcculusionBounds;
    public DynamicArray<D2Class_07008080> Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "17488080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7D718080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B16C8080", 0x4)]
public struct D2Class_B16C8080
{
    public Tag<D2Class_B26C8080> DecoratorModel;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "221C8080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7E718080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B26C8080", 0x100)]
public struct D2Class_B26C8080
{
    public long FileSize;
    public EntityModel Model;
    public int UnkC;
    //public AABB BoundingBox; not in pre-bl, dont really care about it tho
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag Unk30;  // D2Class_B46C8080
    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x34, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<D2Class_B86C8080> SpeedTreeData; // Used for actual trees
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D81B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "84718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B86C8080", 0x18)]
public struct D2Class_B86C8080
{
    [SchemaField(0x8)]
    public DynamicArray<D2Class_BA6C8080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "9A1A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "86718080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "BA6C8080", 0x50)]
public struct D2Class_BA6C8080
{
    // part of Speedtree cbuffer (cb10)
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CB1A8080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "70718080", 0x20)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A46C8080", 0x20)]
public struct D2Class_A46C8080
{
    public ulong Size;
    public TigerHash Unk08;
    public TigerHash UnkC;
    public int Unk10;
    public Tag<D2Class_9F6C8080> Unk14;
    public VertexBuffer InstanceBuffer;
    public Tag<SDecoratorInstanceData> InstanceData;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "321B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A76C8080", 0x18)]
public struct SDecoratorInstanceData
{
    [SchemaField(0x8)]
    public DynamicArray<D2Class_A96C8080> InstanceElement;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "291B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "75718080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "A96C8080", 0x10)]
public struct D2Class_A96C8080
{
    // Normalized position
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public ushort[] Position;
    // Rotation represented as an 8-bit quaternion
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public byte[] Rotation;
    // RGBA color
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 4)]
    public byte[] Color;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "721A8080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "6B718080", 0x60)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "9F6C8080", 0x60)]
public struct D2Class_9F6C8080
{
    // SpeedtreePlacements[2-7]
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
}
#endregion
