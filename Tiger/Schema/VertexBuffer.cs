namespace Tiger.Schema;

// public class VertexBuffer : Tag
// {
//     private D2Class_VertexHeader header;
//
//     public VertexBuffer(TagHash hash, VertexHeader parent) : base(hash)
//     {
//         header = parent.Header;
//     }
//
//     /// <summary>
//     /// Parses a vertex buffer from a tag, and returns a list of vertices, only parsing the vertices that are actually used.
//     /// </summary>
//     /// <param name="dynamicPart">The parent part to set the changes to.</param>
//     /// <param name="uniqueVertexIndices">All the vertex indices that will be acquired.</param>
//     public void ParseBuffer(Part part, HashSet<uint> uniqueVertexIndices)
//     {
//         using (var handle = GetHandle())
//         {
//             foreach (var vertexIndex in uniqueVertexIndices)
//             {
//                 ReadVertexData(part, vertexIndex, handle);
//             }
//         }
//     }
//
//     private void ReadVertexData(Part part, uint vertexIndex, BinaryReader handle)
//     {
//         handle.BaseStream.Seek(vertexIndex * header.Stride, SeekOrigin.Begin);
//         bool status = false;
//         switch (header.Type)
//         {
//             case 0:
//                 status = ReadVertexDataType0(part, handle);
//                 break;
//             case 1:
//                 status = ReadVertexDataType1(part, handle);
//                 break;
//             case 5:
//                 status = ReadVertexDataType5(part, handle);
//                 break;
//             case 6:
//                 status = ReadVertexDataType6(part, vertexIndex, handle);
//                 break;
//             default:
//                 throw new NotImplementedException($"Vertex type {header.Type} is not implemented.");
//                 break;
//         }
//         if (!status)
//         {
//             throw new NotImplementedException($"Vertex stride {header.Stride} for type {header.Type} is not implemented.");
//         }
//     }
//
//     private bool ReadVertexDataType0(Part part, BinaryReader handle)
//     {
//         switch (header.Stride)
//         {
//             case 0x4:
//                 part.VertexTexcoords.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
//                 break;
//             case 0x8:  // all terrain-specific
//                 var v = new Vector4(handle.ReadUInt16(), handle.ReadUInt16(), handle.ReadInt16(), handle.ReadUInt16(), true);
//                 if (v.W > 0)
//                 {
//                     v.Z += 2 * v.W;  // terrain uses a z precision extension.
//                 }
//
//                 part.VertexPositions.Add(v);
//
//                 break;
//             case 0xC:
//                 part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 part.VertexTexcoords.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
//                 break;
//             // case 0x10:
//             //     part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//             //     // Quaternion normal
//             //     part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16()));
//             //     break;
//             case 0x18:  // normal and tangent euler
//                 part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 break;
//             default:
//                 return false;
//         }
//         return true;
//     }
//
//     private bool ReadVertexDataType1(Part part, BinaryReader handle)
//     {
//         switch (header.Stride)
//         {
//             case 0x4:
//                 part.VertexTexcoords.Add(new Vector2(handle.ReadInt16(), handle.ReadInt16()));
//                 break;
//             case 0x8:
//                 VertexWeight vw = new VertexWeight
//                 {
//                     WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
//                     WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()),
//                 };
//                 (part as DynamicPart).VertexWeights.Add(vw);
//                 break;
//             case 0x18:  // normals and tangents are euler
//                 short w;
//                 part.VertexPositions.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 part.VertexNormals.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), w = handle.ReadInt16(), true));
//                 part.VertexTangents.Add(new Vector4(handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), handle.ReadInt16(), true));
//                 AddSlotInfo(part, w);
//                 break;
//             case 0x30: // physics, normals and tangents are euler
//                 part.VertexPositions.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle()));
//                 part.VertexNormals.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle()));
//                 part.VertexTangents.Add(new Vector4(handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle(), handle.ReadSingle()));
//                 break;
//             default:
//                 return false;
//         }
//         return true;
//     }
//
//     public static void AddSlotInfo(Part part, short w)
//     {
//         // slots
//         Vector4 vc = Vector4.Zero;
//         switch (w & 0x7)
//         {
//             case 0:
//                 vc.X = 0.333f;
//                 break;
//             case 1:
//                 vc.X = 0.666f;
//                 break;
//             case 2:
//                 vc.X = 0.999f;
//                 break;
//             case 3:
//                 vc.Y = 0.333f;
//                 break;
//             case 4:
//                 vc.Y = 0.666f;
//                 break;
//             case 5:
//                 vc.Y = 0.999f;
//                 break;
//         }
//         if (part.bAlphaClip)
//             vc.Z = 0.25f;
//         part.VertexColourSlots.Add(vc);
//     }
//
//     private bool ReadVertexDataType5(Part part, BinaryReader handle)
//     {
//         switch (header.Stride)
//         {
//             case 4:
//                 // it can be longer here, its not broken i think
//                 if (handle.BaseStream.Length <= handle.BaseStream.Position)
//                 {
//                     part.VertexColours.Add(new Vector4(0, 0, 0, 0));
//                 }
//                 else
//                 {
//                     part.VertexColours.Add(new Vector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte()));
//                 }
//                 break;
//             default:
//                 return false;
//         }
//         return true;
//     }
//
//     private bool ReadVertexDataType6(Part part, uint vertexIndex, BinaryReader handle)
//     {
//         DynamicPart dynamicPart = (DynamicPart) part;
//         switch (header.Stride)
//         {
//             case 0x4:
//                 /* How SPSB works:
//                  * - stores a signed short value in the W of the position data
//                  * - if value has no flags (<0x800), W is just the bone index that vertex is fully weighted to (one bone only)
//                  * - the value can also have flags which indicate the number of weights present (2-4) and the location in the file
//                  * - the location value can sometimes not change, which means to just continue reading
//                  * - there can also be a header, idk what it means but it disrupts the order
//                  * - Flags:
//                  * - if first bit is set (ie negative signed), the vertex has up to 4 weights and its index is defined by the other 15 bits
//                  * - else if value >= 0x800, the value of w is the offset and has up to 2 weights
//                  * - The SPSB file itself:
//                  * - goes weight indices | weight values for 2 weights, opposite for 4
//                  * - if theres 3 weights, the weight value is garbage and the weight index is zero
//                  * - the file is separated into chunks, which are found by the index in the W value
//                  * - chunks are of size 0x20
//                  * - if there are 2 weights per vertex, there are 8 weights per chunk
//                  * - if there are 4 weights per vertex, there are 4 weights per chunk
//                  * - you can see the chunk effect by the duplication of index access in the W value
//                  * - if there is an inteface between 2 and 4 (eg 2 2 4 4 in one chunk) it pads by 2 extra 2s after the 2 2 of all zeros
//                  * - this gives evidence for a correct approach to be to use vertex index to map the file
//                  */
//
//                 // new code vvv
//                 VertexWeight vw = new VertexWeight();
//                 short w = (short)part.VertexPositions[part.VertexIndices.IndexOf(vertexIndex)].W;
//                 if (w >= 0 && w < 0x800)
//                 {
//                     vw.WeightIndices = new IntVector4(w, 0, 0, 0);
//                     vw.WeightValues = new IntVector4(255, 0, 0, 0);
//                     dynamicPart.VertexWeights.Add(vw);
//                     return true;
//                 }
//
//                 int chunkIndex, weightCount;
//                 if (w < 0)
//                 {
//                     chunkIndex = Math.Abs(w)- 0x800; // take absolute value to remove flag
//                     weightCount = 4;
//                 }
//                 else
//                 {
//                     chunkIndex = w - 0x800;  // remove the flag
//                     weightCount = 2;
//                 }
//
//                 // To get the correct offset, we also need the vertex index to find offset within the chunk
//                 // If 2 weights per vertex, we know its vertexIndex % 8, if 4 vertexIndex % 4
//                 if (weightCount == 2)
//                 {
//                     handle.BaseStream.Seek(chunkIndex * 0x20 + (vertexIndex % 8) * 4, SeekOrigin.Begin);
//                     vw.WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
//                     vw.WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), 0, 0);
//                     dynamicPart.VertexWeights.Add(vw);
//                 }
//                 else
//                 {
//                     handle.BaseStream.Seek(chunkIndex * 0x20 + (vertexIndex % 4) * 8, SeekOrigin.Begin);
//                     vw.WeightValues = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte());
//                     if (vw.WeightValues.X + vw.WeightValues.Y + vw.WeightValues.Z == 255)
//                     {
//                         vw.WeightValues.W = 0;
//                     }
//                     vw.WeightIndices = new IntVector4(handle.ReadByte(), handle.ReadByte(), handle.ReadByte(), handle.ReadByte());
//                     dynamicPart.VertexWeights.Add(vw);
//                 }
//                 break;
//             default:
//                 return false;
//         }
//         return true;
//     }
// }
