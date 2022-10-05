using System.Diagnostics;
using System.Runtime.InteropServices;
using Field.General;
using Field.Models;

namespace Field.Statics;

public class StaticContainer
{
    public TagHash Hash;
    public StaticContainer(TagHash hash)
    {
        Hash = hash;
    }
    
    public void SaveMaterialsFromParts(string saveDirectory, List<Part> parts, bool bSaveShaders)
    {
        Directory.CreateDirectory($"{saveDirectory}/Textures");
        Directory.CreateDirectory($"{saveDirectory}/Shaders");
        foreach (var part in parts)
        {
            if (part.Material == null || !part.Material.Hash.IsValid()) continue;
            part.Material.SaveAllTextures($"{saveDirectory}/Textures");
            if (bSaveShaders)
            {
                part.Material.SavePixelShader($"{saveDirectory}/Shaders");
                part.Material.SaveVertexShader($"{saveDirectory}/Shaders");
                part.Material.SaveComputeShader($"{saveDirectory}/Shaders");
            }
        }
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllLoadStaticContainer", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllLoadStaticContainer(uint staticContainerHash, ELOD detailLevel, IntPtr executionDirectoryPtr);

    public List<Part> Load(ELOD detailLevel)
    {
        DestinyFile.UnmanagedData unmanagedData = DllLoadStaticContainer(Hash.Hash, detailLevel, PackageHandler.GetExecutionDirectoryPtr());
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
    public DestinyFile.UnmanagedData Indices;
    public DestinyFile.UnmanagedData VertexIndices;
    public DestinyFile.UnmanagedData VertexPositions;
    public DestinyFile.UnmanagedData VertexTexcoords;
    public DestinyFile.UnmanagedData VertexNormals;
    public DestinyFile.UnmanagedData VertexTangents;
    public DestinyFile.UnmanagedData VertexColours;
    public uint MaterialHash;
    public int DetailLevel;

    public Part Decode()
    {
        Part outPart = new Part();
        outPart.LodCategory = (ELodCategory)DetailLevel;
        outPart.IndexOffset = IndexOffset;
        outPart.IndexCount = IndexCount;
        outPart.PrimitiveType = (EPrimitiveType)PrimitiveType;
        outPart.Material = new Material(new TagHash(MaterialHash));
        // Indices
        UIntVector3[] indices = new UIntVector3[Indices.dataSize];
        PackageHandler.Copy(Indices.dataPtr, indices, 0, Indices.dataSize);
        outPart.Indices = indices.ToList();
        Marshal.FreeHGlobal(Indices.dataPtr);

        // VertexIndices
        uint[] vertexIndices = new uint[VertexIndices.dataSize];
        PackageHandler.Copy(VertexIndices.dataPtr, vertexIndices, 0, VertexIndices.dataSize);
        outPart.VertexIndices = vertexIndices.ToList();
        Marshal.FreeHGlobal(VertexIndices.dataPtr);

        // VertexPositions
        Vector4[] vertexPositions = new Vector4[VertexPositions.dataSize];
        PackageHandler.Copy(VertexPositions.dataPtr, vertexPositions, 0, VertexPositions.dataSize);
        outPart.VertexPositions = vertexPositions.ToList();
        Marshal.FreeHGlobal(VertexPositions.dataPtr);

        // VertexTexcoords
        Vector2[] vertexTexcoords = new Vector2[VertexTexcoords.dataSize];
        PackageHandler.Copy(VertexTexcoords.dataPtr, vertexTexcoords, 0, VertexTexcoords.dataSize);
        outPart.VertexTexcoords = vertexTexcoords.ToList();
        Marshal.FreeHGlobal(VertexTexcoords.dataPtr);

        // VertexNormals
        Vector4[] vertexNormals = new Vector4[VertexNormals.dataSize];
        PackageHandler.Copy(VertexNormals.dataPtr, vertexNormals, 0, VertexNormals.dataSize);
        outPart.VertexNormals = vertexNormals.ToList();
        Marshal.FreeHGlobal(VertexNormals.dataPtr);

        // VertexTangents
        Vector4[] vertexTangents = new Vector4[VertexTangents.dataSize];
        PackageHandler.Copy(VertexTangents.dataPtr, vertexTangents, 0, VertexTangents.dataSize);
        outPart.VertexTangents = vertexTangents.ToList();
        Marshal.FreeHGlobal(VertexTangents.dataPtr);

        // VertexColours
        Vector4[] vertexColours = new Vector4[VertexColours.dataSize];
        PackageHandler.Copy(VertexColours.dataPtr, vertexColours, 0, VertexColours.dataSize);
        outPart.VertexColours = vertexColours.ToList();
        Marshal.FreeHGlobal(VertexColours.dataPtr);

        return outPart;
    }
}