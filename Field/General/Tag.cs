using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Field.Entities;
using Field.Strings;

/// DO NOT TOUCH ANYTHING IN THIS FILE
/// IF YOU DO THE WHOLE THING WILL EXPLODE

namespace Field.General;


public enum ELanguage
{
    English = 1,
    French = 2,
    Italian = 3,
    German = 4,
    Spanish = 5,
    Japanese = 6,
    Portuguese = 7,
    Russian = 8,
    Polish = 9,
    Simplified_Chinese = 10,
    Traditional_Chinese = 11,
    Latin_American_Spanish = 12,
    Korean = 13,
}
    
[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DestinyFieldAttribute : Attribute
{
    internal FieldType _val;
    internal bool _disableLoad;
    
    public FieldType Value
    {
        get { return _val; }
    }
    
    public bool DisableLoad
    {
        get { return _disableLoad; }
    }


    public DestinyFieldAttribute(FieldType d2FieldType)
    {
        _val = d2FieldType;
    }
    
    public DestinyFieldAttribute(FieldType d2FieldType, bool disableLoad)
    {
        _val = d2FieldType;
        _disableLoad = disableLoad;
    }
}

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class DestinyOffsetAttribute : Attribute
{
    internal int _val;
    public int Offset
    {
        get { return _val; }
    }


    public DestinyOffsetAttribute(int d2Offset)
    {
        _val = d2Offset;
    }
}

public enum FieldType
{
    ResourcePointer,  // a relative pointer that points to a resource
    ResourcePointerWithClass,  // a relative pointer that points to a resource, and the class of resource is specified
    TablePointer,  // a relative pointer that points to a table, 0x0 is table count 0x8 is table relative pointer
    RelativePointer,  // a relative pointer that points to a place within the same file
    ResourceInTag,  // a pointer that points to a resource in a specified tag
    ResourceInTagWeird, // the weird one of above
    TagHash,  // a tag hash that points to a tag
    Resource, // resource, no pointer just the resource class right there
    TagHash64,  // taghash but in the 64bit format
    ResourceInTablePointer,  // a pointer that points to a resource in a specified table so has a constant type
    String, // u32 container + u32 key
    String64, // u64 container + u32 key
    // StringNoContainer, // u32 key no container (deprecated, moved to DestinyHash)
}

public class Tag : DestinyFile
{
    public Type HeaderType;
    public new TagHash Hash;

    public Tag(TagHash tagHash) : base(tagHash)
    {
        Hash = tagHash;
        if (tagHash.IsValid())
        {
            Parse();
        }
    }
    
    public Tag(TagHash tagHash, bool disableLoad) : base(tagHash)
    {
        Hash = tagHash;
        if (tagHash.IsValid() && !disableLoad)
        {
            Parse();
        }
    }

    public virtual object GetHeader()
    {
        return null;
    }

    protected virtual T ReadHeader<T>(StringContainer? sc = null) where T : struct
    {
        HeaderType = typeof(T);
        if (!IsStructureValid(typeof(T).FullName, PackageHandler.GetExecutionDirectoryPtr())) throw new Exception("Structure failed to validate, likely some changes to the game.");
        dynamic ret;
        using (var handle = GetHandle())
        {
            ret = ReadStruct(typeof(T), handle, sc);
        }
        return ret;
    }

    public dynamic ReadStruct(Type T, BinaryReader handle, StringContainer? sc = null)
    {
        long startOffset = handle.BaseStream.Position;
        object result = Activator.CreateInstance(T);
        foreach (var field in T.GetFields())
        {
            long fieldOffset = handle.BaseStream.Position;
            // int verifiedLocation = VerifyFieldLocation(T.FullName, fieldOffset);
            // if (verifiedLocation != fieldOffset) throw new Exception("Field offset failed to validate, likely some changes to the game.");
            var attOffset = ((DestinyOffsetAttribute) field.GetCustomAttribute(typeof(DestinyOffsetAttribute), true));
            if (attOffset != null)  // force offset change
            {
                fieldOffset = startOffset + attOffset.Offset;
                handle.BaseStream.Seek(fieldOffset, SeekOrigin.Begin);
            }
            var attField = ((DestinyFieldAttribute) field.GetCustomAttribute(typeof(DestinyFieldAttribute), true));
            if (attField == null)  // read as normal
            {
                if (field.FieldType.IsArray)
                {
                    int arraySize = ((MarshalAsAttribute)field.GetCustomAttribute(typeof(MarshalAsAttribute), true)).SizeConst;
                    dynamic array = Activator.CreateInstance(field.FieldType, arraySize);
                    for (int i = 0; i < arraySize; i++)
                    {
                        dynamic value = StructConverter.ToStructure(handle.ReadBytes(Marshal.SizeOf(field.FieldType.GetElementType())), field.FieldType.GetElementType());
                        array[i] = value;
                    }
                    field.SetValue(result, array);
                }
                else if (field.FieldType.FullName.Contains("D2Class_"))
                {
                    field.SetValue(result, ReadStruct(field.FieldType, handle, sc));
                }
                else if (field.FieldType == typeof(DestinyHash))
                {
                    field.SetValue(result, new DestinyHash(handle.ReadUInt32(), sc));
                }
                else if (field.FieldType == typeof(TagHash))
                {
                    field.SetValue(result, new TagHash(handle.ReadUInt32()));
                }
                else if (field.FieldType.BaseType == typeof(Enum))
                {
                    field.SetValue(result, Enum.ToObject(field.FieldType, handle.ReadByte()));
                }
                else
                {
                    dynamic value = StructConverter.ToStructure(handle.ReadBytes(Marshal.SizeOf(field.FieldType)), field.FieldType);
                    field.SetValue(result, value);
                }
            }
            else  // custom read
            { 
                FieldType fieldType = attField.Value;
                bool disableLoad = attField.DisableLoad;
                if (fieldType == FieldType.TablePointer)
                {
                    var tableSize = handle.ReadInt64();
                    var tablePointer = handle.ReadInt64();
                    var tableOffset = fieldOffset + tablePointer+24;
                    if (field.FieldType.FullName.Contains("IndexAccessList"))
                    {
                        Type genericType = typeof(IndexAccessList<>).MakeGenericType(field.FieldType.GenericTypeArguments[0].UnderlyingSystemType);
                        dynamic? list = Activator.CreateInstance(genericType);
                        list.Count = tableSize;
                        list.Offset = tableOffset;
                        list.ParentTag = this;
                        field.SetValue(result, list);
                    }
                    else
                    {
                        Type genericType = typeof(List<>).MakeGenericType(field.FieldType.GenericTypeArguments[0].UnderlyingSystemType);
                        var list = Activator.CreateInstance(genericType);

                        for (int j = 0; j < tableSize; j++)
                        {
                            dynamic value;
                            var type = field.FieldType.GenericTypeArguments[0];
                            if (field.FieldType.FullName.Contains("D2Class_")) // array of classes, overriding arrays if pure class hashes
                            {
                                handle.BaseStream.Seek(tableOffset + j*type.StructLayoutAttribute.Size, SeekOrigin.Begin);
                                value = ReadStruct(type, handle, sc);
                            }
                            else
                            {
                                handle.BaseStream.Seek(tableOffset + j*4, SeekOrigin.Begin);
                                uint hash = handle.ReadUInt32();
                                if (type == typeof(DestinyHash))
                                {
                                    value = new DestinyHash(hash, sc);
                                }
                                else
                                {
                                    // todo should prob use GetTag instead
                                    value = Activator.CreateInstance(type, new TagHash(hash));
                                }
                            }
                            // dynamic value = StructConverter.ToStructure(handle.ReadBytes(Marshal.SizeOf(field.FieldType.GenericTypeArguments[0])), field.FieldType.GenericTypeArguments[0]);
                            ((IList) list).Add(value);
                        }
                        field.SetValue(result, list);  
                    }

                    handle.BaseStream.Seek(fieldOffset + 0x10, SeekOrigin.Begin);

                }
                else if (fieldType == FieldType.ResourceInTag)
                {
                    handle.BaseStream.Seek(0x10, SeekOrigin.Current);
                    continue;
                }
                else if (fieldType == FieldType.ResourceInTagWeird)
                {
                    handle.BaseStream.Seek(0x18, SeekOrigin.Current);
                    continue;
                }
                else if (fieldType == FieldType.TagHash)
                {
                    if (field.FieldType.IsArray)
                    {
                        int arraySize = ((MarshalAsAttribute)field.GetCustomAttribute(typeof(MarshalAsAttribute), true)).SizeConst;
                        dynamic tagArray = Activator.CreateInstance(field.FieldType, arraySize);
                        for (int i = 0; i < arraySize; i++)
                        {
                            TagHash tagHash = new TagHash(handle.ReadUInt32());
                            dynamic tag = PackageHandler.GetTag(field.FieldType.GetElementType(), tagHash, disableLoad);
                            tagArray[i] = tag;
                        }
                        field.SetValue(result, tagArray);
                    }
                    else
                    {
                        TagHash tagHash = new TagHash(handle.ReadUInt32());
                        dynamic tag = PackageHandler.GetTag(field.FieldType, tagHash, disableLoad);
                        field.SetValue(result, tag);
                    }
                }
                else if (fieldType == FieldType.ResourcePointer)
                {
                    long resourcePointer = handle.ReadInt64();
                    if (resourcePointer > 0)  // dont read zeros or negatives as cause infinite loops
                    {
                        handle.BaseStream.Seek(resourcePointer-12, SeekOrigin.Current);
                        uint resourceClass = handle.ReadUInt32();
                        Type innerType = Type.GetType($"Field.D2Class_{Endian.U32ToString(resourceClass)}, Field, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                        if (innerType == null)
                        {
                            if (T == typeof(D2Class_069B8080) || T == typeof(D2Class_85988080) || T == typeof(D2Class_F1918080))  // If entity resource or map data resource, don't bother reading if we don't know it (too many types)
                            {
                                handle.BaseStream.Seek(fieldOffset + 8, SeekOrigin.Begin);  // return back to keep reading
                                return result;
                            }
                            continue;
                            //throw new NotImplementedException($"[RESOURCE POINTER] Unable to find type for resource class {Endian.U32ToString(resourceClass)}. Parent is {T.FullName}, in tag {Hash}, field is {field.Name}");
                        }
                        dynamic resource = ReadStruct(innerType, handle, sc);
                        handle.BaseStream.Seek(fieldOffset + 8, SeekOrigin.Begin);  // return back to keep reading
                        field.SetValue(result, resource);
                    }
                }
                else if (fieldType == FieldType.Resource)
                {
                    uint resourceClass = handle.ReadUInt32();
                    Type innerType = Type.GetType($"Field.D2Class_{Endian.U32ToString(resourceClass)}, Field, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                    if (innerType == null)
                    {
                        continue;
                        //throw new NotImplementedException($"[RESOURCE] Unable to find type for resource class {Endian.U32ToString(resourceClass)}. Parent is {T.FullName}, in tag {Hash}, field is {field.Name}");
                    }
                    dynamic resource = ReadStruct(innerType, handle, sc);
                    field.SetValue(result, resource);
                }
                else if (fieldType == FieldType.ResourcePointerWithClass)
                {
                    long resourcePointer = handle.ReadInt64();
                    uint resourceClass = handle.ReadUInt32();
                    if (resourcePointer > 0)
                    {
                        continue;
                        //throw new NotImplementedException();
                    }
                }
                else if (fieldType == FieldType.ResourceInTablePointer)
                {
                    long resourcePointer = handle.ReadInt64();
                    handle.BaseStream.Seek(resourcePointer-8, SeekOrigin.Current);
                    Type innerType = field.FieldType;
                    dynamic resource = ReadStruct(innerType, handle, sc);
                    handle.BaseStream.Seek(fieldOffset + 8, SeekOrigin.Begin);  // return back to keep reading
                    field.SetValue(result, resource);
                }
                else if (fieldType == FieldType.RelativePointer)
                {
                    if (field.FieldType == typeof(string))
                    {
                        StringBuilder sb = new StringBuilder();
                        long relativePointer = handle.ReadInt64();
                        handle.BaseStream.Seek(relativePointer-8, SeekOrigin.Current);
                        while (true)
                        {
                            char c = handle.ReadChar();
                            if (c == 0)
                            {
                                break;
                            }
                            sb.Append(c);
                        }
                        field.SetValue(result, sb.ToString());
                        handle.BaseStream.Seek(fieldOffset + 8, SeekOrigin.Begin);  // return back to keep reading
                    }
                    else
                    {
                        // Just sends back to some place, often a tag?
                        long relativePointer = handle.ReadInt64();
                        field.SetValue(result, relativePointer+handle.BaseStream.Position-8);
                    }
                }
                else if (fieldType == FieldType.TagHash64)
                {
                    TagHash tagHash = new TagHash(handle.ReadUInt32());
                    int bIs32Bit = handle.ReadInt32();
                    var u64 = handle.ReadUInt64();
                    if (bIs32Bit == 0)
                    {
                        tagHash = new TagHash(u64);
                    }
                    if (tagHash.IsValid())// && bIs32Bit == 0)
                    {
                        if (field.FieldType == typeof(TagHash))
                        {
                            field.SetValue(result, tagHash);
                        }
                        else
                        {
                            dynamic tag = PackageHandler.GetTag(field.FieldType, tagHash, disableLoad);
                            field.SetValue(result, tag); 
                        }
                    }
                }
                else if (fieldType == FieldType.String)
                {
                    uint indexOrHash = handle.ReadUInt32();
                    TagHash tagHash = new TagHash(indexOrHash);
                    DestinyHash key = new DestinyHash(handle.ReadUInt32(), sc);
                    if (tagHash.IsValid())
                    {
                        StringContainer tag = PackageHandler.GetTag(typeof(StringContainer), tagHash, disableLoad);
                        string resStr = tag.GetStringFromHash(ELanguage.English, key);
                        field.SetValue(result, resStr);
                    }
                    else if (indexOrHash != 0xFF_FF)
                    {
                        StringContainer tag = PackageHandler.GetTag(typeof(StringContainer), InvestmentHandler.GetStringContainerFromIndex(indexOrHash), disableLoad);
                        string resStr = tag.GetStringFromHash(ELanguage.English, key);
                        field.SetValue(result, resStr);
                    }
                    else  // uses an index system instead
                    {
                        field.SetValue(result, string.Empty);
                    }
                }
                else if (fieldType == FieldType.String64)
                {
                    TagHash tagHash = new TagHash(handle.ReadUInt32());
                    int bIs32Bit = handle.ReadInt32();
                    var u64 = handle.ReadUInt64();
                    if (bIs32Bit == 0)
                    {
                        tagHash = new TagHash(u64);
                    }
                    if (tagHash.IsValid())
                    {
                        StringContainer tag = PackageHandler.GetTag(typeof(StringContainer), tagHash, disableLoad);
                        DestinyHash key = new DestinyHash(handle.ReadUInt32(), sc);
                        string resStr = tag.GetStringFromHash(ELanguage.English, key);
                        field.SetValue(result, resStr);
                    }
                    else
                    {
                        field.SetValue(result, "%%FAIL%%");
                    }
                }
                // else if (fieldType == FieldType.StringNoContainer)
                // {
                //     // We need to use the cached 02218080 string classes
                //     DestinyHash key = new DestinyHash(handle.ReadUInt32(), sc);
                //     string globalString = PackageHandler.GetGlobalString(key);
                //     field.SetValue(result, globalString);
                // }
                else
                {
                    continue;
                    //throw new NotImplementedException();
                }
            }

        }
        // move on one resource size
        if (T.FullName.Contains("D2Class_"))
        {
            handle.BaseStream.Seek(startOffset + T.StructLayoutAttribute.Size, SeekOrigin.Begin);
        }
        return result;
    }
        
                
    [DllImport("Symmetry.dll", EntryPoint = "DllVerifyFieldLocation", CallingConvention = CallingConvention.StdCall)]
    public extern static int VerifyFieldLocation([MarshalAs(UnmanagedType.LPStr)] string structureType, long fieldOffset, IntPtr executionDirectoryPtr);
        
    [DllImport("Symmetry.dll", EntryPoint = "DllIsStructureValid", CallingConvention = CallingConvention.StdCall)]
    public extern static bool IsStructureValid([MarshalAs(UnmanagedType.LPStr)] string structureType, IntPtr executionDirectoryPtr);
        
    // protected object ReadResource()
    // {
    //     // A resource is from a pointer in header; if that pointer - 4 is a class hash, then it's a resource
    //     // There are also global pointer things but they can't be pointed to by the header
    //     var resourceType = ReadStruct<ResourceType>();
    //     var resource = ReadResource(resourceType);
    //     return resource;
    // }
        
    protected virtual void Parse()
    {
        ParseStructs();
        ParseData();
    }
        
    protected virtual void ParseStructs()
    {
    }
        
    protected virtual void ParseData()
    {
    }
}

public class Tag<T> : Tag where T : struct
{
    public T Header;

    public Tag(TagHash tagHash) : base(tagHash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<T>();
    }
}

/// <summary>
/// Used like a normal table list, but is not loaded by the Tag parser.
/// Structures can be accessed by index like a List.
/// Best used for extremely large tables that cause performance issues when using a normal List.
/// </summary>
/// <typeparam name="T">The structure of that list</typeparam>
public class IndexAccessList<T> : IEnumerable<T> where T : struct
{
    public long Count;
    public long Offset;
    public Tag ParentTag = null;
    
    public IEnumerator<T> GetEnumerator()
    {
        throw new NotSupportedException();
        // foreach (Guest guest in _guestList)
        // {
        //     yield return guest;
        // }
    }

    public T ElementAt(int index)
    {
        if (index >= Count || index < 0)
        {
            throw new IndexOutOfRangeException();
        }

        using (var handle = ParentTag.GetHandle())
        {
            handle.BaseStream.Seek(Offset + index * typeof(T).StructLayoutAttribute.Size, SeekOrigin.Begin);
            T structure = ParentTag.ReadStruct(typeof(T), handle);
            return structure; 
        }
    }
    
    public T ElementAt(int index, BinaryReader handle)
    {
        if (index >= Count || index < 0)
        {
            throw new IndexOutOfRangeException();
        }
        
        handle.BaseStream.Seek(Offset + index * typeof(T).StructLayoutAttribute.Size, SeekOrigin.Begin);
        T structure = ParentTag.ReadStruct(typeof(T), handle);
        return structure;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// We assume the list is sorted via a position 0 DestinyHash uint32.
    /// </summary>
    /// <param name="hash">Hash to find if it exists.</param>
    /// <returns>The entry of the list if found, otherwise null.</returns>
    public T? BinarySearch(DestinyHash hash)
    {
        using (var handle = ParentTag.GetHandle())
        {
            uint compareValue = hash.Hash;
            int min = 0;
            int max = (int)Count - 1;
            int structureSize = typeof(T).StructLayoutAttribute.Size;
            while (min <= max)
            {
                int mid = (min + max) / 2;
                handle.BaseStream.Seek(Offset + mid * structureSize, SeekOrigin.Begin);
                uint midValue = handle.ReadUInt32();
                if (midValue == compareValue)
                {
                    return ElementAt(mid, handle);
                }
                if (midValue < compareValue)
                {
                    min = mid + 1;
                }
                else
                {
                    max = mid - 1;
                }
            }
        }

        return null;
    }
}

// public class ConcurrentBinaryReader
// {
//     private PipeReader _pr;
//
//     public ConcurrentBinaryReader(MemoryStream ms)
//     {
//         _pr = PipeReader.Create(ms);
//     }
//
//     public long Seek(long offset, SeekOrigin origin)
//     {
//         return _pr.AsStream().Seek(offset, origin);
//     }
//     
//     public void ReadUInt32(long offset, SeekOrigin origin)
//     {
//         _pr.ReadAsync();
//     }
// }