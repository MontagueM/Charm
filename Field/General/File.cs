using System.Runtime.InteropServices;

namespace Field.General;

public class File
{
    public string Hash;
    protected BinaryReader Handle;
    
    public File(string hash)
    {
        Hash = hash;
    }
    
    public BinaryReader GetHandle()
    {
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
    public static extern UnmanagedData DllGetData([MarshalAs(UnmanagedType.LPStr)] string hash);

    public void CloseHandle()
    {
        Handle.Close();
    }
}