
using System.Runtime.InteropServices;
using Field.Strings;

namespace Field.General;

public class PackageHandler
{
    private static Dictionary<uint, dynamic> Cache = new Dictionary<uint, dynamic>();
    private static List<StringContainer> GlobalStringContainerCache = new List<StringContainer>();

    private static bool AddToCache(uint hash, dynamic tag)
    {
        return Cache.TryAdd(hash, tag);
    }
    
    public static dynamic GetTag(Type type, ulong hash)
    {
        return GetTag(type, TagHash64Handler.GetTagHash64(hash));
    }

    public static dynamic GetTag(Type type, uint hash)
    {
        return GetTag(type, new TagHash(hash));
    }
    
    public static dynamic GetTag(Type type, string hash)
    {
        return GetTag(type, new TagHash(hash));
    }
    
    public static dynamic GetTag(Type type, TagHash hash)
    {
        // Check if tag exists already in the cache and return if it does exist
        if (Cache.ContainsKey(hash.Hash))
        {
            return Cache[hash.Hash];
        }
        // Create a new tag and add it to the cache as it doesn't exist
        dynamic tag = Activator.CreateInstance(type, hash);
        bool status = AddToCache(hash.Hash, tag);
        if (!status)
        {
            Environment.Exit(77);
            throw new Exception($"Failed to add tag with hash {hash} to PackageHandler cache.");
        }
        return tag;
    }
    
    public static void GenerateGlobalStringContainerCache()
    {
        List<string> hashes = new List<string>();
        // Iterate over all the 02218080 files and their string containers inside
        File.UnmanagedData pAllEntries = GetAllEntriesOfReference("045EAE80", "02218080"); // 045EAE80 to speed it up
        int[] vals = new int[pAllEntries.dataSize];
        Marshal.Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        foreach (int i in vals)
        {
            string hash = Endian.U32ToString((uint) (0x80800000 + (0x172 << 0xD) + i));
            Tag<D2Class_02218080> f = new Tag<D2Class_02218080>(hash);
            foreach (var q in f.Header.Unk28)
            {
                if (q.Unk10 == null) continue;
                if (hashes.Contains(q.Unk10.Hash))
                {
                    continue;
                }
                hashes.Add(q.Unk10.Hash);
                GlobalStringContainerCache.Add(q.Unk10);
            }
        }
    }
    
    public static string GetGlobalString(DestinyHash key)
    {
        foreach (var container in GlobalStringContainerCache)
        {
            var index = container.Header.StringHashTable.BinarySearch(key);
            if (index >= 0)
            {
                return container.GetStringFromHash(ELanguage.English, key);
            }
        }
    
        return $"%%NOGLOBALSTRING[{Endian.U32ToString(key.Hash)}]%%";
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllEntriesOfReference", CallingConvention = CallingConvention.StdCall)]
    public extern static File.UnmanagedData GetAllEntriesOfReference([MarshalAs(UnmanagedType.LPStr)] string hash, [MarshalAs(UnmanagedType.LPStr)] string reference);

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryReference", CallingConvention = CallingConvention.StdCall)]
    public extern static IntPtr DllGetEntryReference([MarshalAs(UnmanagedType.LPStr)] string hash);

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryTypes", CallingConvention = CallingConvention.StdCall)]
    public extern static int DllGetEntryTypes([MarshalAs(UnmanagedType.LPStr)] string hash);

    
    public static string GetEntryReference(string hash)
    {
        IntPtr pRef = DllGetEntryReference(hash);
        return Marshal.PtrToStringAnsi(pRef);
    }

    public static void GetEntryTypes(string hash, out int hType, out int hSubtype)
    {
        int combinedTypes = DllGetEntryTypes(hash);
        hType = (combinedTypes >> 16) & 0xFFFF;
        hSubtype = combinedTypes & 0xFFFF;
    }
}