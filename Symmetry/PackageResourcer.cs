using System.Collections.Concurrent;

namespace Symmetry;

/// <summary>
/// Handles everything to do with package .pkg files. Creates handles for them, and manages them to prioritise
/// speed above everything.
/// If the Destiny 2 cache_phr is filled, we will use that as it is the most reliable source of information.
/// We can't cache the pkgs in memory as way too much storage, so we maintain handles for open pkgs and never close them.
/// We maintain a minimal API so the only thing you can do is ask for the byte array of a file or its metadata.
/// This is a "singleton" class as well but is written as a static class instead so we don't have to handle instances.
/// There are no Destiny references here so this can be used for any game on Tiger engine.
/// File can be anything, Tag is specifically the name of files with Bungie structs inside.
/// </summary>
public class PackageResourcer
{
    struct PackageQueueItem
    {
        private int PkgId;
        
    }

    public struct FileMetadata
    {
        public int PkgId;
        public int FileId;
        public TigerHash Hash;
        public int Size;
    }
    
    private ConcurrentQueue<PackageQueueItem> _packageQueue = new ConcurrentQueue<PackageQueueItem>();

    public static byte[] GetFileBytes(string path)
    {
        throw new NotImplementedException();
    }
    
    public static FileMetadata GetFileMetadata(string path)
    {
        throw new NotImplementedException();
    }
}