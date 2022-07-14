using System.IO;
using System.Runtime.InteropServices;

namespace Field.General;
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
    
    public static T ReadStructure<T>(BinaryReaderBE br) where T : struct
    {
        var bytes = br.ReadBytes(Marshal.SizeOf<T>());
        AdjustEndianness(typeof(T), bytes);

        GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T)); }
        finally { handle.Free(); }
    }
    
    private static void AdjustEndianness(Type type, byte[] data, int startOffset = 0)
    {
        foreach (var field in type.GetFields())
        {
            var fieldType = field.FieldType;
            if (field.IsStatic)
                // don't process static fields
                continue;

            if (fieldType == typeof(string) || fieldType.IsArray || fieldType == typeof(sbyte))
                // don't swap bytes for strings or arrays or single bytes
                continue;

            var offset = Marshal.OffsetOf(type, field.Name).ToInt32();

            // handle enums
            if (fieldType.IsEnum)
                fieldType = Enum.GetUnderlyingType(fieldType);

            // check for sub-fields to recurse if necessary
            var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

            var effectiveOffset = startOffset + offset;

            if (subFields.Length == 0)
            {
                Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
            }
            else
            {
                // recurse
                AdjustEndianness(fieldType, data, effectiveOffset);
            }
        }
    }
}

public class BinaryReaderBE : BinaryReader { 
    public BinaryReaderBE(System.IO.Stream stream)  : base(stream) { }

    public override int ReadInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToInt32(data, 0);
    }

    public override Int16 ReadInt16()
    {
        var data = base.ReadBytes(2);
        Array.Reverse(data);
        return BitConverter.ToInt16(data, 0);
    }

    public override Int64 ReadInt64()
    {
        var data = base.ReadBytes(8);
        Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }

    public override UInt32 ReadUInt32()
    {
        var data = base.ReadBytes(4);
        Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

}