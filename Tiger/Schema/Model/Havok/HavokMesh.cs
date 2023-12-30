using System.Runtime.InteropServices;

namespace Tiger.Schema.Havok;

public unsafe class DestinyHavok
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3
    {
        public float X, Y, Z;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CArray<T> where T : struct
    {
        public T* Data;
        public ulong Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CShape
    {
        public CArray<Vector3> Vertices;
        public CArray<ushort> Indices;
    }

    [DllImport("ThirdParty/Alkahest/destiny_havok.dll", EntryPoint = "destinyhavok_read_shape_collection")]
    private static extern CArray<CShape>* ReadShapeCollection(IntPtr data, ulong length);

    [DllImport("ThirdParty/Alkahest/destiny_havok.dll", EntryPoint = "destinyhavok_free_shape_collection")]
    private static extern void FreeShapeCollection(CArray<CShape>* p);

    public struct HavokShape
    {
        public Vector3[] Vertices;
        public ushort[] Indices;
    }

    public static HavokShape[] ReadShapeCollection(byte[] data)
    {
        var bufferPtr = Marshal.AllocCoTaskMem(data.Length);
        Marshal.Copy(data, 0, bufferPtr, data.Length);

        var shapeCollectionPtr = ReadShapeCollection(bufferPtr, (ulong)data.Length);
        if (shapeCollectionPtr == null)
            return null; //throw new Exception("Failed to read shape collection, destiny-havok returned null.");

        var shapeCollection = *shapeCollectionPtr;

        var shapes = new HavokShape[shapeCollection.Length];
        for (ulong i = 0; i < shapeCollection.Length; i++)
        {
            var shape = shapeCollection.Data[i];
            var vertices = new Vector3[shape.Vertices.Length];
            var indices = new ushort[shape.Indices.Length];

            for (ulong j = 0; j < shape.Vertices.Length; j++)
            {
                vertices[j] = shape.Vertices.Data[j];
            }

            for (ulong j = 0; j < shape.Indices.Length; j++)
            {
                indices[j] = shape.Indices.Data[j];
            }

            shapes[i] = new HavokShape
            {
                Vertices = vertices,
                Indices = indices
            };
        }

        FreeShapeCollection(shapeCollectionPtr);
        return shapes;
    }
}
