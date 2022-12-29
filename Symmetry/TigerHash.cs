namespace Symmetry;


/// <summary>
/// A TigerHash represents any hash that is used to identify something within Tiger.
/// See "StringHash", "FileHash", "File64Hash", and "TagClassHash" for children of this class.
/// </summary>
public abstract class TigerHash
{
    
}

/// <summary>
/// Represents string that has been hashed via the FNV1-32 algorithm.
/// </summary>
public class StringHash : TigerHash
{
    
}

/// <summary>
/// Represents a package hash, which is a combination of the EntryId and PkgId e.g. ABCD5680.
/// </summary>
public class FileHash : TigerHash
{
    
}

/// <summary>
/// Same as FileHash, but represents a 64-bit version that is used as a hard reference to a tag. Helps to keep
/// files more similar as FileHash's can change, but the 64-bit version will always be the same.
/// </summary>
public class File64Hash : FileHash
{
    
}

/// <summary>
/// Represents the type a tag can be. These can exist as package references which stores the type of the tag file,
/// or within the tag file itself which represents the resource types and other tags inside e.g. ABCD8080.
/// </summary>
public class TagClassHash : TigerHash
{
    
}