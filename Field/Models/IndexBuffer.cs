using System;
using System.Collections.Generic;
using System.IO;
using Field.General;

namespace Field.Models;

public class IndexBuffer : Tag
{
    private D2Class_IndexHeader header;

    
    public IndexBuffer(TagHash hash, IndexHeader parent) : base(hash)
    {
        header = parent.Header;
    }

    public List<UIntVector3> ParseBuffer(EPrimitiveType indexFormat, uint offset, uint count)
    {
        BinaryReader reader = GetHandle();
        List<UIntVector3> indices = new List<UIntVector3>();
        // indices.Capacity = count;
        if (indexFormat == EPrimitiveType.Triangles)
        {
            if (header.Is32Bit)
            {
                reader.BaseStream.Seek(offset * 4, SeekOrigin.Begin);
                for (uint i = 0; i < count; i+= 3)
                {
                    indices.Add(new UIntVector3(reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32()));
                }   
            }
            else
            {
                reader.BaseStream.Seek(offset * 2, SeekOrigin.Begin);
                for (uint i = 0; i < count; i+= 3)
                {
                    indices.Add(new UIntVector3(reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()));
                }   
            }
        }
        else if (indexFormat == EPrimitiveType.TriangleStrip)
        {
            int triCount = 0;
            if (header.Is32Bit)
            {
                reader.BaseStream.Seek(offset * 4, SeekOrigin.Begin);
                while (true)
                {
                    uint i1 = reader.ReadUInt32();
                    uint i2 = reader.ReadUInt32();
                    uint i3 = reader.ReadUInt32();
                    if (i3 == 0xFF_FF_FF_FF)
                    {
                        triCount = 0;
                        continue;
                    }
                    if (triCount % 2 == 0)
                    {
                        indices.Add(new UIntVector3(i1, i2, i3));
                    }
                    else
                    {
                        indices.Add(new UIntVector3(i2, i1, i3));
                    }
                    reader.BaseStream.Seek(-8, SeekOrigin.Current);
                    triCount++;
                    if (indices.Count == count)
                    {
                        break;
                    }
                }   
            }
            else
            {
                reader.BaseStream.Seek(offset * 2, SeekOrigin.Begin);
                long start = reader.BaseStream.Position;
                while (reader.BaseStream.Position + 4 - start < count * 2)  // + 4 from reading the first two previous
                {
                    uint i1 = reader.ReadUInt16();
                    uint i2 = reader.ReadUInt16();
                    uint i3 = reader.ReadUInt16();
                    if (i3 == 0xFF_FF)
                    {
                        triCount = 0;
                        continue;
                    }
                    if (triCount % 2 == 0)
                    {
                        indices.Add(new UIntVector3(i1, i2, i3));
                    }
                    else
                    {
                        indices.Add(new UIntVector3(i2, i1, i3));
                    }
                    reader.BaseStream.Seek(-4, SeekOrigin.Current);
                    triCount++;
                }    
            }
        }
        CloseHandle();
        return indices;
    }
}