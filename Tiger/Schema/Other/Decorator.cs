using System.Numerics;
using Tiger.Exporters;
using Tiger.Schema.Entity;

namespace Tiger.Schema;

public class Decorator : Tag<SDecorator>
{
    public TfxFeatureRenderer FeatureType = TfxFeatureRenderer.SpeedtreeTrees;
    public Decorator(FileHash hash) : base(hash)
    {

    }

    public void LoadIntoExporter(ExporterScene decorsScene, ExporterScene treesScene, string saveDirectory)
    {
        if (_tag.BufferData is null)
            return;

        DynamicArray<SB16C8080> models = _tag.DecoratorModels;
        // Model transform offsets
        List<Vector4> SpeedtreePlacements = new(); //{ Vector4.Zero, Vector4.Zero.WithW(1) };

        TigerFile container = new(_tag.BufferData.TagData.Unk14.Hash);
        byte[] containerData = container.GetData();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            SpeedtreePlacements.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }
        SpeedtreePlacements.Add(Vector4.Zero);
        SpeedtreePlacements.Add(Vector4.Zero.WithW(1));

        using TigerReader reader = _tag.BufferData.TagData.InstanceBuffer.GetReferenceReader();

        for (int i = 0; i < _tag.InstanceRanges.Count - 1; i++)
        {
            int start = _tag.InstanceRanges[i].Value;
            int end = _tag.InstanceRanges[i + 1].Value;
            int count = end - start;

            int dynID = models.Count == 1 ? i : 0;
            Tag<SB26C8080> model = models[models.Count == 1 ? 0 : i].DecoratorModel;
            var isSpeedTree = model.TagData.SpeedTreeData != null;

            List<DynamicMeshPart> parts = model.TagData.Model.Load(ExportDetailLevel.MostDetailed, null).Where(x => x.IndexOffset == 0).SkipLast(1).ToList(); //GenerateParts(model.TagData.Model); //.Load(ExportDetailLevel.MostDetailed, null);
            foreach (DynamicMeshPart part in parts)
            {
                if (part.Material == null) continue;

                if (isSpeedTree)
                {
                    treesScene.Materials.Add(new ExportMaterial(part.Material));

                    var vecs = model.TagData.SpeedTreeData.TagData.Unk08[part.MeshIndex];
                    var scale = vecs.Unk00;
                    var offset = vecs.Unk10;
                    var uvTransform = vecs.Unk20;

                    for (int k = 0; k < part.VertexPositions.Count; k++)
                    {
                        part.VertexPositions[k] = new Vector4(
                            part.VertexPositions[k].X * scale.X + offset.X,
                            part.VertexPositions[k].Y * scale.Y + offset.Y,
                            part.VertexPositions[k].Z * scale.Z + offset.Z,
                            part.VertexPositions[k].W
                        );
                    }

                    for (int k = 0; k < part.VertexTexcoords0.Count; k++)
                    {
                        part.VertexTexcoords0[k] = new Vector2(
                            part.VertexTexcoords0[k].X * uvTransform.X + uvTransform.Z,
                            part.VertexTexcoords0[k].Y * -uvTransform.Y + 1 - uvTransform.W
                        );
                    }
                }
                else
                    decorsScene.Materials.Add(new ExportMaterial(part.Material));
            }

            for (int j = 0; j < count; j++)
            {
                reader.BaseStream.Seek((start + j) * 0x10, SeekOrigin.Begin);
                var pos = new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()); // v5?
                var rot = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()); // v6?
                var v7 = new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()); // v7?

                Vector4 inst = SpeedtreePlacements[0] * pos + SpeedtreePlacements[1];
                Vector4 q = SpeedtreePlacements[2] * rot + SpeedtreePlacements[3];
                Vector4 unk = SpeedtreePlacements[4] * v7 + SpeedtreePlacements[5];

                if (isSpeedTree)
                {
                    System.Numerics.Vector3 r2 = q.ToVec3();
                    System.Numerics.Vector3 r1 = unk.ToVec3();
                    System.Numerics.Vector3 r3 = System.Numerics.Vector3.Cross(r1, r2);

                    System.Numerics.Matrix4x4 rotationMatrix = new System.Numerics.Matrix4x4(
                        r2.X, r3.X, r1.X, 0,
                        r2.Y, r3.Y, r1.Y, 0,
                        r2.Z, r3.Z, r1.Z, 0,
                        0, 0, 0, 1
                    );

                    var quat = Quaternion.CreateFromRotationMatrix(rotationMatrix);
                    q = new(quat.X, quat.Y, quat.Z, -quat.W);
                }
                else
                    parts = parts.Where(x => x.MeshIndex == 0).ToList();

                Transform transform = new()
                {
                    Position = inst.ToVec3(),
                    Quaternion = q,
                    Scale = new(inst.W)
                };

                if (isSpeedTree)
                    treesScene.AddMapModelParts($"{model.Hash}_{i}", parts, transform);
                else
                    decorsScene.AddMapModelParts($"{model.Hash}_{i}", parts, transform);
            }
        }
    }
}

#region Decorator structs
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "361C8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AD718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "C36C8080", 0x18)]
public struct SDecoratorMapResource
{
    [SchemaField(0xC, TigerStrategy.DESTINY1_RISE_OF_IRON), NoLoad]
    [SchemaField(0x10, TigerStrategy.DESTINY2_SHADOWKEEP_2601), NoLoad]
    public Decorator Decorator;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CE1A8080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "64718080", 0xA8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "986C8080", 0xA8)]
public struct SDecorator
{
    public ulong Size;
    public DynamicArray<SB16C8080> DecoratorModels;
    public DynamicArray<SInt32> InstanceRanges;
    public DynamicArray<SInt32> Unk28;
    public DynamicArray<SInt32> Unk38;
    public Tag<SA46C8080> BufferData;
    public Tag<SOcclusionBounds> OcculusionBounds;
    public DynamicArray<SInt32> Unk50;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "17488080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7D718080", 0x4)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B16C8080", 0x4)]
public struct SB16C8080
{
    public Tag<SB26C8080> DecoratorModel;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "221C8080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "7E718080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B26C8080", 0xD8)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "B26C8080", 0x100)]
public struct SB26C8080
{
    public long FileSize;
    public EntityModel Model;
    public int UnkC;

    //public AABB BoundingBox; not in pre-bl, dont really care about it tho
    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag Unk30;  // SB46C8080

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x34, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public Tag<SB86C8080> SpeedTreeData; // Used for actual trees
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D81B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "84718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "B86C8080", 0x18)]
public struct SB86C8080
{
    [SchemaField(0x8)]
    public DynamicArray<SBA6C8080> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "9A1A8080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "86718080", 0x50)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "BA6C8080", 0x50)]
public struct SBA6C8080
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
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A46C8080", 0x20)]
public struct SA46C8080
{
    public ulong Size;
    public TigerHash Unk08;
    public TigerHash UnkC;
    public int Unk10;
    public Tag<S9F6C8080> Unk14;
    public VertexBuffer InstanceBuffer;
    [NoLoad]
    public Tag<SDecoratorInstanceData> InstanceData; // Same as InstanceBuffer
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "321B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "73718080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A76C8080", 0x18)]
public struct SDecoratorInstanceData
{
    [SchemaField(0x8)]
    public DynamicArrayUnloaded<SA96C8080> InstanceElement;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "291B8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "75718080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A96C8080", 0x10)]
public struct SA96C8080
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
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "9F6C8080", 0x60)]
public struct S9F6C8080
{
    // SpeedtreePlacements[0-5]?
    public Vector4 Unk00;
    public Vector4 Unk10;
    public Vector4 Unk20;
    public Vector4 Unk30;
    public Vector4 Unk40;
    public Vector4 Unk50;
}
#endregion
