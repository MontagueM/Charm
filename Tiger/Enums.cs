namespace Tiger;

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

public enum TigerLanguage
{
    English = 1,
    French = 2,
    Italian = 3,
    German = 4,
    Spanish = 5,
    Japanese = 6,
    Portuguese = 7,
    Russian = 8,
    Polish = 9,
    Simplified_Chinese = 10,
    Traditional_Chinese = 11,
    Latin_American_Spanish = 12,
    Korean = 13,
}
