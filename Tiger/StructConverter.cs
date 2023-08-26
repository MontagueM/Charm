using System.Runtime.InteropServices;
using Tiger.Schema;

namespace Tiger;

public static class StructConverter
{
    public static T ToType<T>(this byte[] bytes)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T)); }
        finally { handle.Free(); }
    }

    public static dynamic ToType(this byte[] bytes, Type type)
    {
        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type); }
        finally { handle.Free(); }
    }

    public static T ReadType<T>(this BinaryReader stream)
    {
        return (T)ReadType(stream, typeof(T));
    }

    public static dynamic ReadType(this BinaryReader stream, Type type)
    {
        var buffer = new byte[Marshal.SizeOf(type)];
        stream.Read(buffer, 0, buffer.Length);
        return buffer.ToType(type);
    }

    public static void WriteStruct<T>(this BinaryWriter stream, T value) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf(typeof(T))];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            stream.Write(buffer, 0, buffer.Length);
        }
        finally { handle.Free(); }
    }

    public static byte[] FromType<T>(T value) where T : struct
    {
        var buffer = new byte[Marshal.SizeOf(typeof(T))];
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
        }
        finally { handle.Free(); }

        return buffer;
    }
}
