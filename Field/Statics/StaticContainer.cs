using System.Diagnostics;
using System.Runtime.InteropServices;
using Field.General;
using Field.Models;
using File = Field.General.File;

namespace Field.Statics;

public class StaticContainer
{
    private uint _hash;
    public StaticContainer(TagHash hash)
    {
        _hash = hash.Hash;
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllLoadStaticContainer", CallingConvention = CallingConvention.StdCall)]
    public extern static File.UnmanagedData DllLoadStaticContainer(uint staticContainerHash, ELOD detailLevel);

    public List<Part> Load(ELOD detailLevel)
    {
        File.UnmanagedData unmanagedData = DllLoadStaticContainer(_hash, detailLevel);
        List<Part> outPart = new List<Part>();
        outPart.EnsureCapacity(unmanagedData.dataSize);
        for (int i = 0; i < unmanagedData.dataSize; i++)
        {
            PartUnmanaged partUnmanaged = Marshal.PtrToStructure<PartUnmanaged>(unmanagedData.dataPtr + i*Marshal.SizeOf<PartUnmanaged>());
            outPart.Add(partUnmanaged.Decode());
        }

        return outPart;
    }
}

public struct PartUnmanaged
{
    public uint IndexOffset;
    public uint IndexCount;
    public sbyte PrimitiveType;
    public File.UnmanagedData Indices;
    public File.UnmanagedData VertexIndices;
    public File.UnmanagedData VertexPositions;
    public File.UnmanagedData VertexTexcoords;
    public File.UnmanagedData VertexNormals;
    public File.UnmanagedData VertexColours;

    public Part Decode()
    {
        Part outPart = new Part();
        outPart.IndexOffset = IndexOffset;
        outPart.IndexCount = IndexCount;
        outPart.PrimitiveType = (EPrimitiveType)PrimitiveType;
        
        // Indices
        UIntVector3[] indices = new UIntVector3[Indices.dataSize];
        Copy(Indices.dataPtr, indices, 0, Indices.dataSize);
        outPart.Indices = indices.ToList();

        // VertexIndices
        uint[] vertexIndices = new uint[VertexIndices.dataSize];
        Copy(VertexIndices.dataPtr, vertexIndices, 0, VertexIndices.dataSize);
        outPart.VertexIndices = vertexIndices.ToList();
        
        // VertexPositions
        Vector4[] vertexPositions = new Vector4[VertexPositions.dataSize];
        Copy(VertexPositions.dataPtr, vertexPositions, 0, VertexPositions.dataSize);
        outPart.VertexPositions = vertexPositions.ToList();
        
        // VertexTexcoords
        Vector2[] vertexTexcoords = new Vector2[VertexTexcoords.dataSize];
        Copy(VertexTexcoords.dataPtr, vertexTexcoords, 0, VertexTexcoords.dataSize);
        outPart.VertexTexcoords = vertexTexcoords.ToList();
        
        // VertexNormals
        Vector4[] vertexNormals = new Vector4[VertexNormals.dataSize];
        Copy(VertexNormals.dataPtr, vertexNormals, 0, VertexNormals.dataSize);
        outPart.VertexNormals = vertexNormals.ToList();
        
        // VertexColours
        Vector4[] vertexColours = new Vector4[VertexColours.dataSize];
        Copy(VertexColours.dataPtr, vertexColours, 0, VertexColours.dataSize);
        outPart.VertexColours = vertexColours.ToList();
        
        return outPart;
    }
    
    [DllImport("kernel32.dll", EntryPoint = "RtlCopyMemory", SetLastError = false)]
    static extern void CopyMemory(IntPtr destination, IntPtr source, UIntPtr length);

    public static void Copy<T>(IntPtr source, T[] destination, int startIndex, int length)
        where T : struct
    {
        var gch = GCHandle.Alloc(destination, GCHandleType.Pinned);
        try
        {
            var targetPtr = Marshal.UnsafeAddrOfPinnedArrayElement(destination, startIndex);
            var bytesToCopy = Marshal.SizeOf(typeof(T)) * length;

            CopyMemory(targetPtr, source, (UIntPtr)bytesToCopy);
        }
        finally
        {
            gch.Free();
        }
    }
}