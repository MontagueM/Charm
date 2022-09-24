using System.Runtime.InteropServices;

namespace Field.General;

/// <summary>
/// Represents the file behind the Tag, handles all the file-based processing including reading data.
/// To make this thread-safe (particularly the MemoryStream handle) we ensure the byte[] data is cached and only ever
/// read once, then generate a new handle each time GetHandle is called. This should only really ever be used
/// in a dispose-oriented system, as otherwise you will have many leftover streams causing leakage.
/// We store the data in this file instead of in PackageHandler BytesCache as we expect one DestinyFile to be made
/// per tag, never more (as long as PackageHandler GetTag is used).
/// </summary>
public class DestinyFile
{
    public TagHash Hash;
    private byte[] _data = null;
    
    public DestinyFile(TagHash hash)
    {
        Hash = hash;
    }
    
    public BinaryReader GetHandle()
    {
        return new BinaryReader(GetStream());
    }

    public MemoryStream GetStream()
    {
        return new MemoryStream(GetData());
    }

    public struct UnmanagedData
    {
        public IntPtr dataPtr;
        public int dataSize;
    }
    
    public struct UnmanagedDictionary
    {
        public UnmanagedData Keys;
        public UnmanagedData Values;
    }
        
    public byte[] GetData()
    {
        if (_data == null)
        {
            if (PackageHandler.BytesCache.ContainsKey(Hash))
            {
                _data = PackageHandler.BytesCache[Hash];
            }
            else
            {
                UnmanagedData unmanagedData = DllGetData(Hash, PackageHandler.GetExecutionDirectoryPtr());
                byte[] managedArray = new byte[unmanagedData.dataSize];
                PackageHandler.Copy(unmanagedData.dataPtr, managedArray, 0, unmanagedData.dataSize);
                PackageHandler.BytesCache.TryAdd(Hash, managedArray);
                _data = managedArray;  
            }
        }

        return _data;
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetData", CallingConvention = CallingConvention.StdCall)]
    public static extern UnmanagedData DllGetData(uint hash, IntPtr executionDirectoryPtr);
}