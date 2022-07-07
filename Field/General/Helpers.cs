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