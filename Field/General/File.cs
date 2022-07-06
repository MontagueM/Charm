using System.Runtime.InteropServices;

namespace Field.General;

public class DestinyFile
{
    public TagHash Hash;
    protected BinaryReader Handle;
    
    public DestinyFile(TagHash hash)
    {
        Hash = hash;
    }
    
    public BinaryReader GetHandle()
    {
        if (Handle != null)
        {
            if (Handle.BaseStream.CanRead)
            {
                return Handle;
            }
        }
        Handle = new BinaryReader(GetStream());
        return Handle;
    }

    public Stream GetStream()
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
        UnmanagedData unmanagedData = DllGetData(Hash);
        byte[] managedArray = new byte[unmanagedData.dataSize];
        Marshal.Copy(unmanagedData.dataPtr, managedArray, 0, unmanagedData.dataSize);
        return managedArray;
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetData", CallingConvention = CallingConvention.StdCall)]
    public static extern UnmanagedData DllGetData(uint hash);

    public void CloseHandle()
    {
        Handle.Close();
    }
}