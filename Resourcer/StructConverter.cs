using System.Runtime.InteropServices;

namespace Resourcer;

public static class StructConverter
{
    public static T ToStructure<T>(this byte[] bytes) where T : struct
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T)); }
        finally { handle.Free(); }
    }
        
    public static dynamic ToStructure(this byte[] bytes, Type type)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
        finally { handle.Free(); }
    }

    public static T ToClass<T>(this byte[] bytes) where T : class
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T)); }
        finally { handle.Free(); }
    }
}