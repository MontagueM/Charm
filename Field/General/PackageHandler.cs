
using System.Runtime.InteropServices;
using Field.Strings;

namespace Field.General;

public class PackageHandler
{
    private static Dictionary<uint, dynamic> Cache = new Dictionary<uint, dynamic>();
    private static List<StringContainer> GlobalStringContainerCache = new List<StringContainer>();
    private static Dictionary<TagHash, string> ActivityNames = new Dictionary<TagHash, string>();

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
        if (!hash.IsValid())
        {
            return null;
        }
            // Check if tag exists already in the cache and return if it does exist
        if (Cache.ContainsKey(hash.Hash))
        {
            return Cache[hash.Hash];
        }
        // Create a new tag and add it to the cache as it doesn't exist
        dynamic tag = Activator.CreateInstance(type, hash);
        bool status = AddToCache(hash.Hash, tag);
        if (!status)  // already exists in cache, caused by threading
        {
            // Environment.Exit(77);
            // throw new Exception($"Failed to add tag with hash {hash} to PackageHandler cache.");
        }
        return tag;
    }
    
    public static void GenerateGlobalStringContainerCache()
    {
        List<DestinyHash> hashes = new List<DestinyHash>();
        // Iterate over all the 02218080 files and their string containers inside
        DestinyFile.UnmanagedData pAllEntries = GetAllEntriesOfReference(new DestinyHash("045EAE80").Hash, new DestinyHash("02218080").Hash); // 045EAE80 to speed it up
        int[] vals = new int[pAllEntries.dataSize];
        Marshal.Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        foreach (int i in vals)
        {
            TagHash hash = new TagHash((uint) (0x80800000 + (0x172 << 0xD) + i));
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
    public extern static DestinyFile.UnmanagedData GetAllEntriesOfReference(uint hash, uint reference);

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryReference", CallingConvention = CallingConvention.StdCall)]
    public extern static uint DllGetEntryReference(uint hash);

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryTypes", CallingConvention = CallingConvention.StdCall)]
    public extern static int DllGetEntryTypes(uint hash);
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllActivityNames", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetAllActivityNames();

    
    public static TagHash GetEntryReference(TagHash hash)
    {
        uint refHash = DllGetEntryReference(hash.Hash);
        return new TagHash(refHash);
    }

    public static void GetEntryTypes(uint hash, out int hType, out int hSubtype)
    {
        int combinedTypes = DllGetEntryTypes(hash);
        hType = (combinedTypes >> 16) & 0xFFFF;
        hSubtype = combinedTypes & 0xFFFF;
    }
    
    public static void GetAllActivityNames()
    {
        DestinyFile.UnmanagedData unmanagedData = DllGetAllActivityNames();
        D2Class_C59E8080_Out[] managedArray = new D2Class_C59E8080_Out[unmanagedData.dataSize];
        Copy(unmanagedData.dataPtr, managedArray, 0, unmanagedData.dataSize);
        for (var i = 0; i < managedArray.Length; i++)
        {
            var th = new TagHash(managedArray[i].TagHash);
            string activityName = Marshal.PtrToStringAnsi(managedArray[i].ActivityName);
            if (ActivityNames.ContainsKey(th))
            {
                // Take the shorter
                if (ActivityNames[th].Length > activityName.Length)
                {
                    ActivityNames[th] = activityName;
                }
            }
            else
            {
                ActivityNames.Add(th, activityName);
            }
        }
    }

    public static string GetActivityName(TagHash hash)
    {
        if (ActivityNames.ContainsKey(hash))
        {
            return ActivityNames[hash];
        }
        return "";
    }
    
    public struct D2Class_C59E8080
    {
        public TagHash TagHash;
        public DestinyHash TagClass;
        public string ActivityName;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct D2Class_C59E8080_Out
    {
        public uint TagHash;
        public uint TagClass;
        public IntPtr ActivityName;
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