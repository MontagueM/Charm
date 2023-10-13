using System.Diagnostics;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Model;


public class VertexBuffer : TigerReferenceFile<SVertexHeader>
{
    public VertexBuffer(FileHash hash) : base(hash)
    {
    }

    /// <summary>
    /// Acquires raw memory and vertex layout description of the vertex buffer.
    /// </summary>
    /// <param name="part"></param>
    public void ReadRawData(RawMeshPart part, uint offset, uint count)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Parses a vertex buffer from a tag, and returns a list of vertices, only parsing the vertices that are actually used.
    /// </summary>
    /// <param name="part">The parent part to set the changes to.</param>
    /// <param name="uniqueVertexIndices">All the vertex indices that will be acquired.</param>
    public void ReadVertexData(MeshPart part, HashSet<uint> uniqueVertexIndices, int bufferIndex = -1,
        int otherStride = -1, bool isTerrain = false)
    {
        using var handle = GetReferenceReader();
        foreach (var vertexIndex in uniqueVertexIndices)
        {
            ReadVertexData(handle, part, vertexIndex, bufferIndex, otherStride, isTerrain);
        }
    }

    private void ReadVertexData(TigerReader handle, MeshPart part, uint vertexIndex, int bufferIndex = -1,
        int otherStride = -1, bool isTerrain = false)
    {
        handle.BaseStream.Seek(vertexIndex * _tag.Stride, SeekOrigin.Begin);
        bool status = false;
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            switch (bufferIndex)
            {
                case 0:
                    switch (_tag.Stride)
                    {
                        case 0x8:
                            Vector4 v;
                            if (isTerrain)
                            {
                                v = new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadUInt16(),
                                    handle.ReadInt16(), true);
                                if (v.W > 0)
                                {
                                    v.Z += 2 * v.W; // terrain uses a z precision extension.
                                }
                            }
                            else
                            {
                                v = new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), true);
                            }

                            part.VertexPositions.Add(v);
                            break;
                        case 0xC:
                            part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            // theres no way to do this "correctly" without using the DXBC info, which i dont want to do
                            if (otherStride == 0x10 || part is not DynamicMeshPart)
                            {
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            }
                            else
                            {
                                VertexWeight vw2 = new()
                                {
                                    // 0xFE designates no bone weight assigned.
                                    WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0xFE, 0xFE),
                                    WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0),
                                };
                                (part as DynamicMeshPart).VertexWeights.Add(vw2);
                            }

                            break;
                        case 0x10:
                            part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            VertexWeight vw = new()
                            {
                                WeightValues =
                                    new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                        handle.ReadByte()),
                                WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(),
                                    handle.ReadByte(), handle.ReadByte()),
                            };
                            (part as DynamicMeshPart).VertexWeights.Add(vw);
                            break;
                        case 0x1C:
                            part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        case 0x20:
                            part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                handle.ReadByte()));
                            break;
                        default:
                            break;
                    }

                    break;
                case 1:
                    switch (_tag.Stride)
                    {
                        case 0x08:
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        case 0x0C:
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        case 0x10:
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        case 0x14:
                            if (otherStride is 0x08 or 0x10 ||
                                otherStride == 0x0C && part is DynamicMeshPart) // 12 and 16 is for entity
                            {
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                            }
                            else
                            {
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(),
                                    handle.ReadByte(), handle.ReadByte()));
                            }

                            break;
                        case 0x18: // normal and tangent euler
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                handle.ReadByte()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        default:
                            break;
                    }

                    break;
                default:
                    break;
            }

            status = true;
        }
        else
        {
            switch (_tag.Type)
            {
                case 0:
                    status = ReadVertexDataType0(handle, part);
                    break;
                case 1:
                    status = ReadVertexDataType1(handle, part);
                    break;
                case 5:
                    status = ReadVertexDataType5(handle, part);
                    break;
                case 6:
                    status = ReadVertexDataType6(handle, part as DynamicMeshPart, vertexIndex);
                    break;
                default:
                    throw new NotImplementedException($"Vertex type {_tag.Type} is not implemented.");
                    break;
            }
        }

        if (!status)
        {
            throw new NotImplementedException($"Vertex stride {_tag.Stride} for type {_tag.Type} is not implemented.");
        }
    }

    private bool ReadVertexDataType0(TigerReader handle, MeshPart part)
    {
        switch (_tag.Stride)
        {
            case 0x4:
                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                break;
            case 0x8: // all terrain-specific
                var v = new Vector4(handle.ReadUInt16(), handle.ReadUInt16(), handle.ReadInt16(), handle.ReadUInt16(),
                    true);
                if (v.W > 0 && Math.Abs(v.W - 0x7F_FF) > 0.1f)
                {
                    v.Z += 2 * v.W; // terrain uses a z precision extension.
                }

                part.VertexPositions.Add(v);

                break;
            case 0xC:
                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                break;
            case 0x10:
                part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                // Quaternion normal
                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16()));
                break;
            case 0x18: // normal and tangent euler
                part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                break;
            default:
                return false;
        }

        return true;
    }

    private bool ReadVertexDataType1(TigerReader handle, MeshPart part)
    {
        switch (_tag.Stride)
        {
            case 0x4:
                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                break;
            case 0x8:
                VertexWeight vw = new()
                {
                    WeightValues =
                        new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
                    WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                        handle.ReadByte()),
                };
                (part as DynamicMeshPart).VertexWeights.Add(vw);
                break;
            case 0x18: // normals and tangents are euler
                short w;
                part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    w = handle.ReadInt16(), true));
                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                    handle.ReadInt16(), true));
                AddVertexColourSlotInfo(part as DynamicMeshPart, w);
                break;
            case 0x30: // physics, normals and tangents are euler
                part.VertexPositions.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(),
                    handle.ReadSingle()));
                part.VertexNormals.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(),
                    handle.ReadSingle()));
                part.VertexTangents.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(),
                    handle.ReadSingle()));
                break;
            default:
                return false;
        }

        return true;
    }

    public static void AddVertexColourSlotInfo(DynamicMeshPart dynamicPart, short w)
    {
        Vector4 vc = Vector4.Zero;
        switch (w & 0x7)
        {
            case 0:
                vc.X = 0.333f;
                break;
            case 1:
                vc.X = 0.666f;
                break;
            case 2:
                vc.X = 0.999f;
                break;
            case 3:
                vc.Y = 0.333f;
                break;
            case 4:
                vc.Y = 0.666f;
                break;
            case 5:
                vc.Y = 0.999f;
                break;
        }

        if (dynamicPart.bAlphaClip)
        {
            vc.Z = 0.25f;
        }

        dynamicPart.VertexColourSlots.Add(vc);
    }

    private bool ReadVertexDataType5(TigerReader handle, MeshPart part)
    {
        switch (_tag.Stride)
        {
            case 4:
                // it can be longer here, its not broken i think
                if (handle.BaseStream.Length <= handle.BaseStream.Position)
                {
                    part.VertexColours.Add(new Vector4(0, 0, 0, 0));
                }
                else
                {
                    part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                        handle.ReadByte()));
                }

                break;
            default:
                return false;
        }

        return true;
    }

    private bool ReadVertexDataType6(TigerReader handle, DynamicMeshPart dynamicPart, uint vertexIndex)
    {
        switch (_tag.Stride)
        {
            case 0x4:
                /* How SPSB works:
                 * - stores a signed short value in the W of the position data
                 * - if value has no flags (<0x800), W is just the bone index that vertex is fully weighted to (one bone only)
                 * - the value can also have flags which indicate the number of weights present (2-4) and the location in the file
                 * - the location value can sometimes not change, which means to just continue reading
                 * - there can also be a header, idk what it means but it disrupts the order
                 * - Flags:
                 * - if first bit is set (ie negative signed), the vertex has up to 4 weights and its index is defined by the other 15 bits
                 * - else if value >= 0x800, the value of w is the offset and has up to 2 weights
                 * - The SPSB file itself:
                 * - goes weight indices | weight values for 2 weights, opposite for 4
                 * - if theres 3 weights, the weight value is garbage and the weight index is zero
                 * - the file is separated into chunks, which are found by the index in the W value
                 * - chunks are of size 0x20
                 * - if there are 2 weights per vertex, there are 8 weights per chunk
                 * - if there are 4 weights per vertex, there are 4 weights per chunk
                 * - you can see the chunk effect by the duplication of index access in the W value
                 * - if there is an inteface between 2 and 4 (eg 2 2 4 4 in one chunk) it pads by 2 extra 2s after the 2 2 of all zeros
                 * - this gives evidence for a correct approach to be to use vertex index to map the file
                 */

                // new code vvv
                VertexWeight vw = new();
                short w = (short)dynamicPart.VertexPositions[dynamicPart.VertexIndexMap[vertexIndex]].W;
                if (w >= 0 && w < 0x800)
                {
                    vw.WeightIndices = new IntVector4(w, 0, 0, 0);
                    vw.WeightValues = new IntVector4(255, 0, 0, 0);
                    dynamicPart.VertexWeights.Add(vw);
                    return true;
                }

                int chunkIndex, weightCount;
                if (w < 0)
                {
                    chunkIndex = Math.Abs(w) - 0x800; // take absolute value to remove flag
                    weightCount = 4;
                }
                else
                {
                    chunkIndex = w - 0x800; // remove the flag
                    weightCount = 2;
                }

                // To get the correct offset, we also need the vertex index to find offset within the chunk
                // If 2 weights per vertex, we know its vertexIndex % 8, if 4 vertexIndex % 4
                if (weightCount == 2)
                {
                    handle.BaseStream.Seek(chunkIndex * 0x20 + (vertexIndex % 8) * 4, SeekOrigin.Begin);
                    vw.WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
                    vw.WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
                    Debug.Assert(vw.WeightValues.X + vw.WeightValues.Y == 255);
                    dynamicPart.VertexWeights.Add(vw);
                }
                else
                {
                    handle.BaseStream.Seek(chunkIndex * 0x20 + (vertexIndex % 4) * 8, SeekOrigin.Begin);
                    vw.WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                        handle.ReadByte());
                    if (vw.WeightValues.X + vw.WeightValues.Y + vw.WeightValues.Z == 255)
                    {
                        vw.WeightValues.W = 0;
                    }

                    vw.WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                        handle.ReadByte());
                    // Debug.Assert(vw.WeightValues.X + vw.WeightValues.Y + vw.WeightValues.Z + vw.WeightValues.W == 255);
                    dynamicPart.VertexWeights.Add(vw);
                }

                break;
            default:
                return false;
        }

        return true;
    }

    public void ReadVertexDataSignatures(MeshPart part, HashSet<uint> uniqueVertexIndices,
        List<InputSignature> inputSignatures, bool isTerrain = false)
    {
        using var reader = GetReferenceReader();
        foreach (uint vertexIndex in uniqueVertexIndices)
        {
            ReadVertexDataSignature(reader, part, vertexIndex, inputSignatures, isTerrain);
        }
    }

    private void ReadVertexDataSignature(TigerReader reader, MeshPart part, uint vertexIndex,
        List<InputSignature> inputSignatures, bool isTerrain = false)
    {
        reader.Seek(vertexIndex * _tag.Stride, SeekOrigin.Begin);

        foreach (InputSignature inputSignature in inputSignatures)
        {
            switch (inputSignature.Semantic)
            {
                case InputSemantic.Position:
                    if(isTerrain) //has to be a float
                    {
                        part.VertexPositions.Add(new Vector4((float)reader.ReadInt16(), (float)reader.ReadInt16(), (float)reader.ReadInt16(),
                            (float)reader.ReadInt16()));
                    }
                    else
                        part.VertexPositions.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                            reader.ReadInt16()));
                    break;
                case InputSemantic.Texcoord:
                    switch (inputSignature.Mask)
                    {
                        case ComponentMask.XY:
                            part.VertexTexcoords0.Add(new Vector2(reader.ReadInt16(), reader.ReadInt16()));
                            break;
                        case ComponentMask.XYZW:
                            part.VertexTexcoords0.Add(new Vector2(reader.ReadInt16(), reader.ReadInt16()));
                            part.VertexColours.Add(new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                                reader.ReadByte()));
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    break;
                case InputSemantic.Normal:
                    switch (inputSignature.Mask)
                    {
                        // euler
                        case ComponentMask.XYZ:
                            part.VertexNormals.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(),
                                reader.ReadInt16(), reader.ReadInt16(), true));
                            break;
                        // quaternion
                        case ComponentMask.XYZW:
                            part.VertexNormals.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(),
                                reader.ReadInt16(), reader.ReadInt16()));
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    break;
                case InputSemantic.Tangent:
                    switch (inputSignature.Mask)
                    {
                        // euler
                        case ComponentMask.XYZ:
                            part.VertexTangents.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(),
                                reader.ReadInt16(), reader.ReadInt16(), true));
                            break;
                        // quaternion
                        case ComponentMask.XYZW:
                            part.VertexTangents.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(),
                                reader.ReadInt16(), reader.ReadInt16()));
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    break;
                case InputSemantic.Colour:
                    part.VertexColours.Add(new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte()));
                    break;
                case InputSemantic.BlendIndices:
                    //Indices get set in BlendWeight
                    break;
                case InputSemantic.BlendWeight:
                    //VertexWeight vw = new()
                    //{
                    //    WeightIndices = new IntVector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    //    WeightValues = new IntVector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                    //};
                    //(part as DynamicMeshPart).VertexWeights.Add(vw);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}


[SchemaStruct(0xC)]
public struct SVertexHeader
{
    public uint DataSize;
    public short Stride;
    public short Type;
    public int Deadbeef;
}
