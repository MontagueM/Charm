using System;
using System.Collections.Generic;
using System.IO;

namespace Tiger.Schema;

public class IndexBuffer : TigerReferenceFile<SIndexHeader>
{
    public IndexBuffer(FileHash hash) : base(hash)
    {
    }

    public List<UIntVector3> GetIndexData(PrimitiveType indexFormat, uint offset, uint count)
    {
        using (TigerReader handle = GetReferenceReader())
        {
            if (indexFormat == PrimitiveType.Triangles)
            {
                return ReadTriangles(handle, offset, count);
            }
            else if (indexFormat == PrimitiveType.TriangleStrip)
            {
                return ReadTriangleStrip(handle, offset, count);
            }
            else
            {
                throw new NotImplementedException($"Unknown index format {indexFormat}");
            }
        }
    }

    private List<UIntVector3> ReadTriangles(TigerReader handle, uint offset, uint count)
    {
        List<UIntVector3> indices = new();

        if (_tag.Is32Bit)
        {
            handle.BaseStream.Seek(offset * 4, SeekOrigin.Begin);
            for (uint i = 0; i < count; i += 3)
            {
                indices.Add(new UIntVector3(handle.ReadUInt32(), handle.ReadUInt32(), handle.ReadUInt32()));
            }
        }
        else
        {
            handle.BaseStream.Seek(offset * 2, SeekOrigin.Begin);
            for (uint i = 0; i < count; i += 3)
            {
                indices.Add(new UIntVector3(handle.ReadUInt16(), handle.ReadUInt16(), handle.ReadUInt16()));
            }
        }

        return indices;
    }

    private List<UIntVector3> ReadTriangleStrip(TigerReader handle, uint offset, uint count)
    {
        List<UIntVector3> indices = new();

        int triCount = 0;
        if (_tag.Is32Bit)
        {
            handle.BaseStream.Seek(offset * 4, SeekOrigin.Begin);
            while (true)
            {
                uint i1 = handle.ReadUInt32();
                uint i2 = handle.ReadUInt32();
                uint i3 = handle.ReadUInt32();
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
                handle.BaseStream.Seek(-8, SeekOrigin.Current);
                triCount++;
                if (indices.Count == count)
                {
                    break;
                }
            }
        }
        else
        {
            handle.BaseStream.Seek(offset * 2, SeekOrigin.Begin);
            long start = handle.BaseStream.Position;
            while (handle.BaseStream.Position + 4 - start < count * 2)  // + 4 from reading the first two previous
            {
                uint i1 = handle.ReadUInt16();
                uint i2 = handle.ReadUInt16();
                uint i3 = handle.ReadUInt16();
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
                handle.BaseStream.Seek(-4, SeekOrigin.Current);
                triCount++;
            }
        }

        return indices;
    }
}

[SchemaStruct(0x18)]
public struct SIndexHeader
{
    public sbyte Unk00;
    public bool Is32Bit;
    public short Unk02;
    public int Zeros04;
    public long DataSize;
    public int Deadbeef;
    public int Zeros14;
}
