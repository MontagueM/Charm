using System.Runtime.InteropServices;

namespace Tiger.Schema;

public enum PrimitiveType  // name comes from bungie
{
    Triangles = 3,
    TriangleStrip = 5,
}

public enum ELodCategory : byte
{
    MainGeom0 = 0, // main geometry lod0
    GripStock0 = 1,  // grip/stock lod0
    Stickers0 = 2,  // stickers lod0
    InternalGeom0 = 3,  // internal geom lod0
    LowPolyGeom1 = 4,  // low poly geom lod1
    LowPolyGeom2 = 7,  // low poly geom lod2
    GripStockScope2 = 8,  // grip/stock/scope lod2
    LowPolyGeom3 = 9,  // low poly geom lod3
    Detail0 = 10 // detail lod0
}

[StructLayout(LayoutKind.Sequential)]
public struct ELod
{
    public ELodCategory DetailLevel;

    public bool IsHighestLevel()
    {
        return DetailLevel == ELodCategory.MainGeom0 ||
               DetailLevel == ELodCategory.GripStock0 ||
               DetailLevel == ELodCategory.Stickers0 ||
               DetailLevel == ELodCategory.InternalGeom0 ||
               DetailLevel == ELodCategory.Detail0;
    }
}
