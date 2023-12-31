using System.Runtime.InteropServices;
using Arithmic;
using System.Text;
using System.Numerics;

namespace Tiger.Schema.Havok;

public unsafe class DestinyHavok
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HavokVector3
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
        public CArray<HavokVector3> Vertices;
        public CArray<ushort> Indices;
    }

    [DllImport("ThirdParty/Alkahest/destiny_havok.dll", EntryPoint = "destinyhavok_read_shape_collection")]
    private static extern CArray<CShape>* ReadShapeCollection(IntPtr data, ulong length);

    [DllImport("ThirdParty/Alkahest/destiny_havok.dll", EntryPoint = "destinyhavok_free_shape_collection")]
    private static extern void FreeShapeCollection(CArray<CShape>* p);

    public struct HavokShape
    {
        public HavokVector3[] Vertices;
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
            var vertices = new HavokVector3[shape.Vertices.Length];
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

    public static void SaveHavokShape(FileHash hash, string name, Vector4 transforms, Vector4 quat)
    {
        var shapeCollection = DestinyHavok.ReadShapeCollection(FileResourcer.Get().GetFile(hash).GetData());
        if (shapeCollection is null)
        {
            Log.Error("Havok shape collection is null");
            return;
        }

        Directory.CreateDirectory($"{ConfigSubsystem.Get().GetExportSavePath()}/HavokShapes");
        int i = 0;
        foreach (var shape in shapeCollection)
        {
            var vertices = shape.Vertices;
            var indices = shape.Indices;

            var sb = new StringBuilder();
            foreach (var vertex in vertices)
            {
                System.Numerics.Vector3 rotatedVertex = RotateVertex(vertex, new Quaternion(quat.X, quat.Y, quat.Z, quat.W));
                sb.AppendLine($"v {(rotatedVertex.X + transforms.X) * transforms.W} {(rotatedVertex.Y + transforms.Y) * transforms.W} {(rotatedVertex.Z + transforms.Z) * transforms.W}");
            }
            foreach (var index in indices.Chunk(3))
            {
                sb.AppendLine($"f {index[0] + 1} {index[1] + 1} {index[2] + 1}");
            }

            Console.WriteLine($"Writing 'HavokShapes/{hash}_{i}.obj'");
            File.WriteAllText($"{ConfigSubsystem.Get().GetExportSavePath()}/HavokShapes/{name}_{hash}_{i++}.obj", sb.ToString());
        }
    }

    private static System.Numerics.Vector3 RotateVertex(HavokVector3 vertex, Quaternion rotationQuaternion)
    {
        // Ensure the quaternion is normalized
        rotationQuaternion = Quaternion.Normalize(rotationQuaternion);

        // Convert the vertex to a quaternion
        Quaternion vertexQuaternion = new Quaternion(vertex.X, vertex.Y, vertex.Z, 0.0f);

        // Rotate the vertex
        Quaternion rotatedVertexQuaternion = Quaternion.Multiply(rotationQuaternion, vertexQuaternion);
        rotatedVertexQuaternion = Quaternion.Multiply(rotatedVertexQuaternion, Quaternion.Conjugate(rotationQuaternion));

        // Extract the rotated vertex
        System.Numerics.Vector3 rotatedVertex = new System.Numerics.Vector3(rotatedVertexQuaternion.X, rotatedVertexQuaternion.Y, rotatedVertexQuaternion.Z);

        return rotatedVertex;
    }
}
