
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using Field.Strings;

namespace Field.General;

public class PackageHandler
{
    private static ConcurrentDictionary<uint, dynamic> Cache = new ConcurrentDictionary<uint, dynamic>();
    private static ConcurrentBag<StringContainer> GlobalStringContainerCache = new ConcurrentBag<StringContainer>();
    private static Dictionary<TagHash, string> ActivityNames = new Dictionary<TagHash, string>();
    public static ConcurrentDictionary<TagHash, byte[]> BytesCache = new ConcurrentDictionary<TagHash, byte[]>();

    public static IntPtr GetExecutionDirectoryPtr()
    {
        // todo make this const or smth, it only needs to be calculated once at beginning
        return Marshal.StringToHGlobalAnsi(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Replace("\\", "/") + "/");
    }
    
    public static void Initialise()
    {
        DllInit(GetExecutionDirectoryPtr());
    }
    
    public static uint MakeHash(int pkgId, int entryId)
    {
        return (uint) (0x80800000 + (pkgId << 0xD) + entryId);
    }

    // public static dynamic GetTag(Type type, ulong hash)
    // {
    //     return GetTag(type, TagHash64Handler.GetTagHash64(hash));
    // }
    //
    // public static dynamic GetTag(Type type, uint hash)
    // {
    //     return GetTag(type, new TagHash(hash));
    // }
    
    public static Tag<T> GetTag<T>(ulong hash, bool disableLoad=false) where T : struct
    {
        return GetTag<T>(TagHash64Handler.GetTagHash64(hash), disableLoad);
    }
    
    public static Tag<T> GetTag<T>(TagHash hash, bool disableLoad=false) where T : struct
    {
        return GetTag(typeof(T), hash, disableLoad);
    }
    
    public static Tag<T> GetTag<T>(uint hash, bool disableLoad=false) where T : struct
    {
        return GetTag(typeof(T), new TagHash(hash), disableLoad);
    }

    // public static dynamic GetTag(Type type, string hash)
    // {
    //     return GetTag(type, new TagHash(hash));
    // }
    
    public static void CacheHashDataList(uint[] hashes)
    {
        GCHandle handle = GCHandle.Alloc(hashes, GCHandleType.Pinned);
        DestinyFile.UnmanagedDictionary ud = DllGetDataMany(new DestinyFile.UnmanagedData {dataPtr = handle.AddrOfPinnedObject(), dataSize = hashes.Length}, GetExecutionDirectoryPtr());
        uint[] keys = new uint[ud.Keys.dataSize];
        Copy(ud.Keys.dataPtr, keys, 0, ud.Keys.dataSize);
        DestinyFile.UnmanagedData[] vals = new DestinyFile.UnmanagedData[ud.Values.dataSize];
        Copy(ud.Values.dataPtr, vals, 0, ud.Values.dataSize);
        // Dictionary<uint, byte[]> data = new Dictionary<uint, byte[]>(keys.Length);
        for (int i = 0; i < keys.Length; i++)
        {        
            byte[] managedArray = new byte[vals[i].dataSize];
            Copy(vals[i].dataPtr, managedArray, 0, vals[i].dataSize);
            BytesCache[new TagHash(keys[i])] = managedArray;
        }
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetDataMany", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedDictionary DllGetDataMany(DestinyFile.UnmanagedData pHashes, IntPtr executionDirectoryPtr);

    
    public static dynamic GetTag(Type type, TagHash hash, bool disableLoad=false)
    {
        if (!hash.IsValid())
        {
            return null;
        }
            // Check if tag exists already in the cache and return if it does exist
        if (Cache.ContainsKey(hash.Hash) && BytesCache.ContainsKey(hash))
        {   
            if (Cache[hash.Hash].GetType() == type)
                return Cache[hash.Hash];
            if (type.IsValueType)
            {
                if (Cache[hash.Hash].GetType().GenericTypeArguments.Length > 0 && Cache[hash.Hash].GetType().GenericTypeArguments[0].UnderlyingSystemType == type)
                {
                    return Cache[hash.Hash];
                }
            }

            dynamic r;
            Cache.TryRemove(hash.Hash, out r);
        }
        // Create a new tag and add it to the cache as it doesn't exist
        if (type.IsValueType)  // checking its a struct
        {
            type = typeof(Tag<>).MakeGenericType(type);
        }

        dynamic tag;
        if (disableLoad)
            tag = Activator.CreateInstance(type, hash, disableLoad);
        else
            tag = Activator.CreateInstance(type, hash);
        Cache.TryAdd(hash, tag);
        return tag;
    }
    
    public static void GenerateGlobalStringContainerCache()
    {
        List<DestinyHash> hashes = new List<DestinyHash>();
        // Iterate over all the 02218080 files and their string containers inside
        var vals = GetAllEntriesOfReference(0x0172, 0x80802102); // 045EAE80 to speed it up
        CacheHashDataList(vals.ToArray());
        var valsActivities = GetAllTagsWithReference(0x80808e8b); // to get the activity location names etc
        CacheHashDataList(valsActivities.Select(x => x.Hash).ToArray());

        // global strings
        Parallel.ForEach(vals, val =>
        {
            TagHash hash = new TagHash(val);
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
        });
        
        // activity strings
        Parallel.ForEach(valsActivities, hash =>
        {
        Tag<D2Class_8B8E8080> f = new Tag<D2Class_8B8E8080>(hash);
        if (f.Header.StringContainer != null)
        GlobalStringContainerCache.Add(f.Header.StringContainer);
        });
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
    
    [DllImport("Symmetry.dll", EntryPoint = "DllInit", CallingConvention = CallingConvention.StdCall)]
    public extern static void DllInit(IntPtr executionDirectoryPtr);
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllEntriesOfReference", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetAllEntriesOfReference(uint hash, uint reference, IntPtr executionDirectoryPtr);

    public static List<uint> GetAllEntriesOfReference(int pkgId, uint reference)
    {
        var pAllEntries = DllGetAllEntriesOfReference(MakeHash(pkgId, 0), reference, GetExecutionDirectoryPtr());
        int[] vals = new int[pAllEntries.dataSize];
        Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        return vals.Select(x => MakeHash(pkgId, x)).ToList();
    }

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryReference", CallingConvention = CallingConvention.StdCall)]
    public extern static uint DllGetEntryReference(uint hash, IntPtr executionDirectoryPtr);

    [DllImport("Symmetry.dll", EntryPoint = "DllGetEntryTypes", CallingConvention = CallingConvention.StdCall)]
    public extern static int DllGetEntryTypes(uint hash, IntPtr executionDirectoryPtr);
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllActivityNames", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetAllActivityNames(IntPtr executionDirectoryPtr);

    
    public static TagHash GetEntryReference(TagHash hash)
    {
        uint refHash = DllGetEntryReference(hash.Hash, GetExecutionDirectoryPtr());
        return new TagHash(refHash);
    }

    public static void GetEntryTypes(uint hash, out int hType, out int hSubtype)
    {
        int combinedTypes = DllGetEntryTypes(hash, GetExecutionDirectoryPtr());
        hType = (combinedTypes >> 16) & 0xFFFF;
        hSubtype = combinedTypes & 0xFFFF;
    }
    
    public static void GetAllActivityNames()
    {
        DestinyFile.UnmanagedData unmanagedData = DllGetAllActivityNames(GetExecutionDirectoryPtr());
        D2Class_C59E8080_Out[] managedArray = new D2Class_C59E8080_Out[unmanagedData.dataSize];
        Copy(unmanagedData.dataPtr, managedArray, 0, unmanagedData.dataSize);
        for (var i = 0; i < managedArray.Length; i++)
        {
            var th = new TagHash(managedArray[i].TagHash);
            string activityName = Marshal.PtrToStringAnsi(managedArray[i].ActivityName);
            if (ActivityNames.ContainsKey(th))
            {
                // Take the longer
                if (activityName.Length > ActivityNames[th].Length)
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
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllTagsWithReference", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetAllTagsWithReference(uint reference, IntPtr executionDirectoryPtr);

    public static List<TagHash> GetAllTagsWithReference(uint reference)
    {
        DestinyFile.UnmanagedData pAllEntries = DllGetAllTagsWithReference(reference, GetExecutionDirectoryPtr());
        uint[] vals = new uint[pAllEntries.dataSize];
        Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        return vals.Select(x => new TagHash(x)).ToList();
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllTagsWithTypes", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetAllTagsWithTypes(int type, int subType, IntPtr executionDirectoryPtr);

    public static List<TagHash> GetAllTagsWithTypes(int type, int subType)
    {
        DestinyFile.UnmanagedData pAllEntries = DllGetAllTagsWithTypes(type, subType, GetExecutionDirectoryPtr());
        uint[] vals = new uint[pAllEntries.dataSize];
        Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        return vals.Select(x => new TagHash(x)).ToList();
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetTagsWithTypes", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedData DllGetTagsWithTypes(int pkgId, int type, int subType, IntPtr executionDirectoryPtr);

    public static List<TagHash> GetTagsWithTypes(int pkgId, int type, int subType)
    {
        DestinyFile.UnmanagedData pAllEntries = DllGetTagsWithTypes(pkgId, type, subType, GetExecutionDirectoryPtr());
        uint[] vals = new uint[pAllEntries.dataSize];
        Copy(pAllEntries.dataPtr, vals, 0, pAllEntries.dataSize);
        return vals.Select(x => new TagHash(x)).ToList();
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

    [DllImport("Symmetry.dll", EntryPoint = "DllGetPackageName", CallingConvention = CallingConvention.StdCall)]
    public extern static IntPtr DllGetPackageName(int packageId, IntPtr executionDirectoryPtr);

    public static string GetPackageName(int pkgId)
    {
        IntPtr pStr = DllGetPackageName(pkgId, GetExecutionDirectoryPtr());
        return Marshal.PtrToStringAnsi(pStr);
    }
}