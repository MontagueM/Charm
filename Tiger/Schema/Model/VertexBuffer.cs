using System.Diagnostics;
using DirectXTexNet;
using Tiger.Schema.Entity;
using static Tiger.Schema.Globals;

namespace Tiger.Schema.Model;


public class VertexBuffer : TigerReferenceFile<SVertexHeader>
{
    public VertexBuffer(FileHash hash) : base(hash)
    {
    }

    public bool _uvExists = false;

    /// <summary>
    /// Parses a vertex buffer from a tag, and returns a list of vertices, only parsing the vertices that are actually used.
    /// </summary>
    /// <param name="part">The parent part to set the changes to.</param>
    /// <param name="uniqueVertexIndices">All the vertex indices that will be acquired.</param>
    public void ReadVertexData(MeshPart part, HashSet<uint> uniqueVertexIndices, int bufferIndex = -1, int otherStride = -1, bool isTerrain = false)
    {
        var _strategy = Strategy.CurrentStrategy;
        _uvExists = part.VertexTexcoords0.Count > 0;

        using var handle = GetReferenceReader();
        foreach (var vertexIndex in uniqueVertexIndices)
        {
            //if (_strategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            //    ReadD1VertexData(handle, part, vertexIndex, bufferIndex, otherStride, isTerrain);
            //else
            ReadVertexData(handle, part, vertexIndex, bufferIndex, otherStride, isTerrain);
        }
    }

    // Everything here is pretty straight forward, for Beyond Light onwards anyways...
    // Pre-BL is that one cousin no one likes and complicates everything for some unknown reason
    // but we just have to deal with them or else they'll just cause problems.
    public void ReadVertexDataFromLayout(MeshPart part, HashSet<uint> uniqueVertexIndices, int bufferIndex = -1)
    {
        var vertexLayout = Globals.Get().GetInputLayouts()[part.VertexLayoutIndex];
        //Console.WriteLine($"{this.Hash}: Mat {part.Material.FileHash}, Vertex Layout {part.VertexLayoutIndex} (Current Buffer Index {bufferIndex}, Part index {part.Index})");

        using var handle = GetReferenceReader();
        foreach (var vertexIndex in uniqueVertexIndices)
        {
            bool HasWeights = false;
            IntVector4 WeightValue = new();
            IntVector4 WeightIndex = new();

            handle.BaseStream.Seek(vertexIndex * _tag.Stride, SeekOrigin.Begin);

            Debug.Assert(handle.BaseStream.Length >= handle.BaseStream.Position);
            if (handle.BaseStream.Length <= handle.BaseStream.Position)
                handle.BaseStream.Position = handle.BaseStream.Length - _tag.Stride;

            foreach (var element in vertexLayout.Elements)
            {
                if (element.BufferIndex != bufferIndex || element.IsInstanceData || element.Format == DXGI_FORMAT.UNKNOWN)
                    continue;

                switch (element.SemanticName)
                {
                    case "POSITION":
                        Vector4 pos = (Vector4)ReadVertexDataFromElement(handle, element);
                        part.VertexPositions.Add(pos);

                        // Pre-BL liked to store bone indicies in the position W for mesh that were fully weighted to said bone
                        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999 && part is DynamicMeshPart)
                        {
                            short w = (short)((part as DynamicMeshPart).VertexPositions[(part as DynamicMeshPart).VertexIndexMap[vertexIndex]].W * 32_767.0f);
                            if (w >= 0 && w < 0x800)
                            {
                                HasWeights = true;
                                WeightIndex = new IntVector4(w, 0, 0, 0);
                                WeightValue = new IntVector4(255, 0, 0, 0);
                            }
                        }
                        break;
                    case "NORMAL":
                        Vector4 norm = (Vector4)ReadVertexDataFromElement(handle, element);
                        part.VertexNormals.Add(norm);
                        if (part is DynamicMeshPart)
                            AddVertexColourSlotInfo(part as DynamicMeshPart, (short)(norm.W * 32_767.0f));
                        break;
                    case "TANGENT":
                        part.VertexTangents.Add((Vector4)ReadVertexDataFromElement(handle, element));
                        break;
                    case "TEXCOORD":
                        var texcoord = ReadVertexDataFromElement(handle, element);
                        if (texcoord is Vector2)
                        {
                            if (element.SemanticIndex == 0 || element.SemanticIndex == 1)
                                part.VertexTexcoords0.Add((Vector2)texcoord);
                        }
                        else
                        {
                            if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999 && element.SemanticIndex == 0)
                            {
                                var tex = (Vector4)texcoord;
                                part.VertexTexcoords0.Add(new Vector2(tex.X, tex.Y));
                                part.VertexColours.Add(new Vector4(tex.Z, tex.W, 0, 1)); // Unsure?
                            }
                            else
                            {
                                if (!part.VertexExtraData.ContainsKey((int)element.SemanticIndex))
                                    part.VertexExtraData.TryAdd((int)element.SemanticIndex, new());

                                part.VertexExtraData[(int)element.SemanticIndex].Add((Vector4)texcoord);
                            }
                        }
                        break;
                    case "COLOR":
                        part.VertexColours.Add((Vector4)ReadVertexDataFromElement(handle, element));
                        break;
                    case "BLENDINDICES": // can be R16G16B16A16_UINT or R8G8B8A8_UINT
                        var indices = (Vector4)ReadVertexDataFromElement(handle, element);
                        WeightIndex = new IntVector4((int)indices.X, (int)indices.Y, (int)indices.Z, (int)indices.W);

                        // Pre-BL also liked to use just BLENDINDICES to assign weights, instead of being normal and using BLENDWEIGHT :)
                        if ((part as DynamicMeshPart).HasSkeleton && !vertexLayout.Elements.Exists(x => x.SemanticName == "BLENDWEIGHT"))
                        {
                            VertexWeight vw2 = new()
                            {
                                // 0xFE designates no bone weight assigned.
                                WeightIndices = new IntVector4(WeightIndex.X, WeightIndex.Y, 0xFE, 0xFE),
                                WeightValues = new IntVector4(WeightIndex.Z, WeightIndex.W, 0, 0),
                            };
                            (part as DynamicMeshPart).VertexWeights.Add(vw2);
                        }
                        break;
                    case "BLENDWEIGHT": // always R8G8B8A8_UNORM
                        HasWeights = true;
                        WeightValue = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte());
                        break;
                    default:
                        throw new NotImplementedException($"Implement parsing for {element.SemanticName} ({element.HlslType}, {element.Format}, size 0x{element.Stride:X2})");
                }
            }

            if (HasWeights)
            {
                VertexWeight vw = new()
                {
                    WeightIndices = WeightIndex,
                    WeightValues = WeightValue,
                };
                (part as DynamicMeshPart).VertexWeights.Add(vw);
            }

            if (Strategy.IsD1() && part is DynamicMeshPart)
                DynamicMeshPart.AddVertexColourSlotInfo(part as DynamicMeshPart, (part as DynamicMeshPart).GearDyeChangeColorIndex);
        }
    }

    private dynamic ReadVertexDataFromElement(TigerReader handle, TigerInputLayoutElement element)
    {
        switch (element.Stride)
        {
            case 0x4: // 4
                switch (element.Format)
                {
                    case DXGI_FORMAT.R8G8B8A8_UNORM:
                        return new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte());
                    case DXGI_FORMAT.R16G16_SNORM:
                        return new Vector2(handle.ReadInt16(), handle.ReadInt16());
                    case DXGI_FORMAT.R8G8B8A8_UINT:
                        return new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), true);
                    case DXGI_FORMAT.R16G16_FLOAT:
                        return new Vector2(handle.ReadHalf(), handle.ReadHalf());
                    default:
                        return new Vector2(handle.ReadInt16(), handle.ReadInt16());
                }
            case 0x8: // 8
                switch (element.Format)
                {
                    case DXGI_FORMAT.R32G32_FLOAT:
                        return new Vector2(handle.ReadSingle(), handle.ReadSingle());
                    case DXGI_FORMAT.R16G16B16A16_SNORM:
                        return new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16());
                    case DXGI_FORMAT.R16G16B16A16_FLOAT:
                        return new Vector4(handle.ReadHalf(), handle.ReadHalf(), handle.ReadHalf(), handle.ReadHalf());
                    case DXGI_FORMAT.R16G16B16A16_SINT:
                        return new Vector4((float)handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16()); // cast as float so values arent divided
                    case DXGI_FORMAT.R16G16B16A16_UINT:
                        return new Vector4((float)handle.ReadUInt16(), handle.ReadUInt16(), handle.ReadUInt16(), handle.ReadUInt16());
                    default:
                        return new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16());
                }
            case 0xC: // 12
                switch (element.Format)
                {
                    case DXGI_FORMAT.R32G32B32_FLOAT:
                        return new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), 0);
                    default:
                        return new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), 0);
                }
            case 0x10: // 16
                switch (element.Format)
                {
                    case DXGI_FORMAT.R32G32B32A32_FLOAT:
                        return new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle());
                    default:
                        return new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle());
                }
            default:
                throw new NotImplementedException($"Cannot read element {element.SemanticName} ({element.HlslType}, {element.Format}, size {element.Stride:X2})");
        }
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

    #region Old-Methods
    // TODO: Start phasing this out, things like vertex color and weights will still need this though
    private void ReadVertexData(TigerReader handle, MeshPart part, uint vertexIndex, int bufferIndex = -1, int otherStride = -1, bool isTerrain = false)
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
                                v = new Vector4((float)handle.ReadInt16(), (float)handle.ReadInt16(), (float)handle.ReadInt16(),
                                        (float)handle.ReadInt16());
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
                        case 0x30: // wtf
                            part.VertexPositions.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                                handle.ReadSingle(), handle.ReadSingle()));
                            part.VertexNormals.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                                handle.ReadSingle(), handle.ReadSingle()));
                            part.VertexTangents.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                                handle.ReadSingle(), handle.ReadSingle()));
                            break;
                        default:
                            break;
                    }

                    break;
                case 1:
                    switch (_tag.Stride)
                    {
                        case 0x04:
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            break;
                        case 0x08:
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                            break;
                        case 0x0C:
                            if (isTerrain)
                            {
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), true));
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadHalf(), handle.ReadHalf()));
                            }
                            else
                            {
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), true));
                            }
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
                    status = ReadVertexDataType5(handle, part, vertexIndex);
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

    private bool ReadVertexDataType5(TigerReader handle, MeshPart part, uint vertexIndex)
    {
        switch (_tag.Stride)
        {
            case 4:
                if (part.MaxVertexColorIndex != -1)
                {
                    var index = Math.Min((uint)part.MaxVertexColorIndex, vertexIndex);
                    handle.BaseStream.Seek(index * _tag.Stride, SeekOrigin.Begin);

                    part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                       handle.ReadByte()));
                }
                // it can be longer here, its not broken i think
                else if (handle.BaseStream.Length <= handle.BaseStream.Position)
                {
                    handle.BaseStream.Position = handle.BaseStream.Length - 4;
                    part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                        handle.ReadByte()));
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
                VertexWeight vw = new VertexWeight();
                short w = (short)(dynamicPart.VertexPositions[dynamicPart.VertexIndexMap[vertexIndex]].W * 32_767.0f);
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
                    chunkIndex = w - 0x800;  // remove the flag
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
                    // always the first two weights valid then the second group can be one or two
                    handle.BaseStream.Seek(chunkIndex * 0x20 + (vertexIndex % 4) * 8, SeekOrigin.Begin);
                    vw.WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
                    vw.WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
                    // if (vw.WeightValues.X + vw.WeightValues.Y + vw.WeightValues.Z == 255)
                    // {
                    //     vw.WeightValues.W = 0;
                    // }
                    vw.WeightIndices.Z = handle.ReadByte();
                    vw.WeightIndices.W = handle.ReadByte();
                    vw.WeightValues.Z = handle.ReadByte();
                    if (vw.WeightIndices.Z == vw.WeightIndices.W)
                    {
                        vw.WeightIndices.W = 0;
                    }
                    else
                    {
                        vw.WeightValues.W = handle.ReadByte();
                    }
                    dynamicPart.VertexWeights.Add(vw);
                }

                break;
            default:
                return false;
        }

        return true;
    }

    // Should probably keep for Pre-BL
    public void ReadVertexDataSignatures(MeshPart part, HashSet<uint> uniqueVertexIndices, List<DXBCIOSignature> inputSignatures, bool isTerrain = false)
    {
        using var reader = GetReferenceReader();
        foreach (uint vertexIndex in uniqueVertexIndices)
        {
            ReadVertexDataSignature(reader, part, vertexIndex, inputSignatures, isTerrain);
        }
    }

    private void ReadVertexDataSignature(TigerReader reader, MeshPart part, uint vertexIndex, List<DXBCIOSignature> inputSignatures, bool isTerrain = false)
    {
        reader.Seek(vertexIndex * _tag.Stride, SeekOrigin.Begin);

        bool HasWeights = false;
        IntVector4 WeightValue = new();
        IntVector4 WeightIndex = new();

        foreach (DXBCIOSignature inputSignature in inputSignatures)
        {
            switch (inputSignature.Semantic)
            {
                case DXBCSemantic.Position:
                    if (isTerrain) //has to be a float
                    {
                        part.VertexPositions.Add(new Vector4((float)reader.ReadInt16(), (float)reader.ReadInt16(), (float)reader.ReadInt16(),
                            (float)reader.ReadInt16()));
                    }
                    else
                    {
                        if (Strategy.CurrentStrategy > TigerStrategy.DESTINY2_SHADOWKEEP_2999)
                        {
                            part.VertexPositions.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                           reader.ReadInt16()));
                        }
                        else // Pre-BL has bone indices for unweighted rigging in the vertex position W (Thanks BIOS!)
                        {
                            part.VertexPositions.Add(new Vector4(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(),
                            reader.ReadInt16(), true));

                            if (part is DynamicMeshPart)
                            {
                                short w = (short)(part as DynamicMeshPart).VertexPositions[(part as DynamicMeshPart).VertexIndexMap[vertexIndex]].W;
                                if (w >= 0)
                                {
                                    HasWeights = true;
                                    WeightIndex = new IntVector4(w, 0, 0, 0);
                                    WeightValue = new IntVector4(255, 0, 0, 0);
                                }
                            }

                        }
                    }

                    break;
                case DXBCSemantic.Texcoord:
                    switch (inputSignature.Mask)
                    {
                        case ComponentMask.XY:
                            if (isTerrain)
                                part.VertexTexcoords0.Add(new Vector2(reader.ReadHalf(), reader.ReadHalf()));
                            else
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
                case DXBCSemantic.Normal:
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
                case DXBCSemantic.Tangent:
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
                case DXBCSemantic.Colour:
                    part.VertexColours.Add(new Vector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
                        reader.ReadByte()));
                    break;
                case DXBCSemantic.BlendIndices:
                    HasWeights = true;
                    WeightIndex = new IntVector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                case DXBCSemantic.BlendWeight:
                    HasWeights = true;
                    WeightValue = new IntVector4(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                    break;
                default:
                    //throw new NotImplementedException($"Semantic {inputSignature.Semantic} Not Implemented");
                    break;
            }
        }

        if (HasWeights)
        {
            //Console.WriteLine($"{WeightValue.X}, {WeightValue.Y}, {WeightValue.Z}, {WeightValue.W}");
            VertexWeight vw = new()
            {
                WeightIndices = WeightIndex,
                WeightValues = WeightValue,
            };
            (part as DynamicMeshPart).VertexWeights.Add(vw);
        }
    }

    private void ReadD1VertexData(TigerReader handle, MeshPart part, uint vertexIndex, int bufferIndex = -1, int otherStride = -1, bool isTerrain = false)
    {
        handle.BaseStream.Seek(vertexIndex * _tag.Stride, SeekOrigin.Begin);
        bool status = false;

        // this shouldn't happen but yet it can (only case was on Oryx)
        Debug.Assert(handle.BaseStream.Length >= handle.BaseStream.Position);
        if (handle.BaseStream.Length <= handle.BaseStream.Position)
            handle.BaseStream.Position = handle.BaseStream.Length - _tag.Stride;

        switch (bufferIndex)
        {
            case 0:
                switch (_tag.Stride)
                {
                    case 0x8:
                        Vector4 v;
                        if (isTerrain)
                        {
                            v = new Vector4((float)handle.ReadInt16(), (float)handle.ReadInt16(), (float)handle.ReadInt16(),
                                    (float)handle.ReadInt16());
                        }
                        else
                        {
                            v = new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), true);
                        }

                        part.VertexPositions.Add(v);
                        if (part is DynamicMeshPart)
                        {
                            short w = (short)(part as DynamicMeshPart).VertexPositions[(part as DynamicMeshPart).VertexIndexMap[vertexIndex]].W;
                            if (w >= 0 && w != 32767)
                            {
                                VertexWeight vw2 = new()
                                {
                                    // 0xFE designates no bone weight assigned.
                                    WeightIndices = new IntVector4(w, 0, 0, 0),
                                    WeightValues = new IntVector4(255, 0, 0, 0),
                                };
                                (part as DynamicMeshPart).VertexWeights.Add(vw2);
                            }
                        }
                        break;
                    case 0xC:

                        part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                        handle.ReadInt16(), handle.ReadInt16(), true));

                        if (part is DynamicMeshPart) // (otherStride == 0x18 || otherStride == 0x14 || otherStride == 0x10)
                        {
                            // Check Pos W
                            handle.BaseStream.Position -= 0x2;
                            var check = handle.ReadInt16();
                            if (check == 32767 || check == -32767)
                            {
                                // Check if UVs or weights
                                handle.BaseStream.Position += 0x2;
                                byte w1 = handle.ReadByte();
                                byte w2 = handle.ReadByte();
                                handle.BaseStream.Position -= 0x4;

                                // stupid stupid stupid stupid stupid stupid
                                if ((part as DynamicMeshPart).HasSkeleton || (w1 + w2 == 255 && (part as DynamicMeshPart).VertexWeights.Count == part.VertexPositions.Count - 1))
                                {
                                    VertexWeight vw2 = new()
                                    {
                                        // 0xFE designates no bone weight assigned.
                                        WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0xFE, 0xFE),
                                        WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0),
                                    };
                                    (part as DynamicMeshPart).VertexWeights.Add(vw2);
                                }
                                else
                                {
                                    part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                                }
                            }
                            else
                            {
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));

                                short w = (short)(part as DynamicMeshPart).VertexPositions[(part as DynamicMeshPart).VertexIndexMap[vertexIndex]].W;
                                if (w >= 0 && w != 32767)
                                {
                                    VertexWeight vw3 = new()
                                    {
                                        WeightIndices = new IntVector4(w, 0, 0, 0),
                                        WeightValues = new IntVector4(255, 0, 0, 0),
                                    };
                                    (part as DynamicMeshPart).VertexWeights.Add(vw3);
                                }
                            }
                        }
                        else
                        {
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                        }
                        break;

                    case 0x10:
                        part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        VertexWeight vw = new()
                        {
                            WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
                            WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
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

                    case 0x30: // wtf
                        part.VertexPositions.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                            handle.ReadSingle(), handle.ReadSingle()));
                        part.VertexNormals.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                            handle.ReadSingle(), handle.ReadSingle()));
                        part.VertexTangents.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(),
                            handle.ReadSingle(), handle.ReadSingle()));
                        break;

                    default:
                        throw new NotImplementedException($"Vertex stride {_tag.Stride} (Buffer Index {bufferIndex}) for type {_tag.Type} is not implemented.");
                        break;
                }
                break;

            case 1:
                switch (_tag.Stride)
                {
                    case 0x04:
                        part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                        break;
                    case 0x08:
                        part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        break;
                    case 0x0C:
                        if (isTerrain)
                        {
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), true));
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadHalf(), handle.ReadHalf()));
                        }
                        else
                        {
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), true));
                        }
                        break;
                    case 0x10:
                        part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        break;
                    case 0x14:
                        if (_uvExists)
                        {
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                            part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                            handle.ReadByte()));
                        }
                        else
                        {
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                            part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), true));
                            part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        }
                        break;
                    case 0x18:
                        if (otherStride != 0xC)
                        {
                            part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));

                            // This is really stupid
                            handle.Seek(0xA, SeekOrigin.Current); // Check for Normal W (should always be 0?)
                            var check = handle.ReadInt16();
                            handle.Seek(0x6, SeekOrigin.Current);
                            var check2 = handle.ReadInt16();  // Check for Tangent W
                            handle.BaseStream.Position -= 0x14;

                            // I thought tangents were euler? so why can it be 32767 or -32767?
                            if (check == 0 && (check2 == 32767 || check2 == -32767))
                            {
                                part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                handle.ReadByte()));
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
                                part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                handle.ReadByte()));
                            }
                            break;
                        }
                        else
                        {
                            if (_uvExists)
                            {
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexColours.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16())); // ??
                            }
                            else
                            {
                                part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                                part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                    handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                                handle.ReadInt16(), handle.ReadInt16(), true));
                                part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                                    handle.ReadByte()));
                            }
                            break;
                        }
                        break;

                    case 0x1C:
                        part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                        part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                            handle.ReadInt16(), handle.ReadInt16(), true));
                        part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(),
                        handle.ReadInt16(), handle.ReadInt16(), true));
                        part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(),
                            handle.ReadByte()));
                        break;

                    default:
                        throw new NotImplementedException($"Vertex stride {_tag.Stride} (Buffer Index {bufferIndex}) for type {_tag.Type} is not implemented.");
                        break;
                }
                break;
            case 2:
                switch (_tag.Stride)
                {
                    case 0x04:
                        part.VertexTexcoords0.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
                        break;
                    case 0x08:
                        VertexWeight vw = new()
                        {
                            WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
                            WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
                        };
                        (part as DynamicMeshPart).VertexWeights.Add(vw);
                        break;
                }
                break;
            default:
                break;
        }

        if (part is DynamicMeshPart)
            DynamicMeshPart.AddVertexColourSlotInfo(part as DynamicMeshPart, (part as DynamicMeshPart).GearDyeChangeColorIndex);

        status = true;
        if (!status)
        {
            throw new NotImplementedException($"Vertex stride {_tag.Stride} for type {_tag.Type} is not implemented.");
        }
    }
    #endregion
}

public enum BufferType
{
    Default,
    Color,
    Weight,
    Skinning,
    Instance,
}

[SchemaStruct(0xC)]
public struct SVertexHeader
{
    public uint DataSize;
    public short Stride;
    public short Type;
    public int Deadbeef;
}
