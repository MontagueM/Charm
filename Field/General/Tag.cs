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
    public FieldType Value
    {
        get { return _val; }
    }


    public DestinyFieldAttribute(FieldType d2FieldType)
    {
        _val = d2FieldType;
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
    ResourceInTablePointer,  // a pointer that points to a resource in a specified table
    String, // u32 container + u32 key
    String64, // u64 container + u32 key
    StringNoContainer, // u32 key no container
}
    
public class Tag : File
{
    public Type HeaderType;

    public Tag(string hash) : base(hash)
    {
        Parse();
    }

    public Tag(TagHash hash) : base(hash.GetString())
    {
        Parse();
    }

    public virtual object GetHeader()
    {
        return null;
    }

    protected virtual T ReadHeader<T>() where T : struct
    {
        HeaderType = typeof(T);
        if (!IsStructureValid(typeof(T).FullName)) throw new Exception("Structure failed to validate, likely some changes to the game.");
        return ReadStruct(typeof(T), Handle);
    }

    public static dynamic ReadStruct(Type T, BinaryReader handle)
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
                    field.SetValue(result, ReadStruct(field.FieldType, handle));
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
                if (fieldType == FieldType.TablePointer)
                {
                    Type genericType = typeof(List<>).MakeGenericType(field.FieldType.GenericTypeArguments[0].UnderlyingSystemType);
                    var list = Activator.CreateInstance(genericType);
                    var tableSize = handle.ReadInt64();
                    var tablePointer = handle.ReadInt64();
                    var tableOffset = fieldOffset + tablePointer+24;
                    for (int j = 0; j < tableSize; j++)
                    {
                        dynamic value;
                        var type = field.FieldType.GenericTypeArguments[0];
                        if (field.FieldType.FullName.Contains("D2Class_")) // array of classes, overriding arrays if pure class hashes
                        {
                            handle.BaseStream.Seek(tableOffset + j*type.StructLayoutAttribute.Size, SeekOrigin.Begin);
                            value = ReadStruct(type, handle);
                        }
                        else
                        {
                            handle.BaseStream.Seek(tableOffset + j*4, SeekOrigin.Begin);
                            uint hash = handle.ReadUInt32();
                            if (type == typeof(DestinyHash))
                            {
                                value = new DestinyHash(hash);
                            }
                            else
                            {
                                value = Activator.CreateInstance(type, new TagHash(hash));
                            }
                        }
                        // dynamic value = StructConverter.ToStructure(handle.ReadBytes(Marshal.SizeOf(field.FieldType.GenericTypeArguments[0])), field.FieldType.GenericTypeArguments[0]);
                        ((IList) list).Add(value);
                    }
                    handle.BaseStream.Seek(fieldOffset + 0x10, SeekOrigin.Begin);
                    field.SetValue(result, list);
                }
                // else if (field.FieldType == typeof(RelativePointer))
                // {
                //     throw new NotImplementedException();
                //     object b = result;
                //     RelativePointer pointer = (RelativePointer)field.GetValue(b);
                //     // pointer.SetOriginal();
                //     pointer.Add(startOffset + Marshal.OffsetOf(typeof(T), field.Name).ToInt64());
                //     field.SetValue(b, pointer);
                //     result = (T)b;
                // }
                // else if (field.FieldType == typeof(Resource))
                // {
                //     throw new NotImplementedException();
                //     object b = result;
                //     Resource resource = (Resource)field.GetValue(b);
                //     
                //     // Check what the class is, if we don't know it we don't bother reading it
                //     resource.Pointer.Add(startOffset + Marshal.OffsetOf(typeof(T), field.Name).ToInt64());
                //
                //     handle.BaseStream.Seek(resource.Pointer.Get() - 4, SeekOrigin.Begin);
                //     uint tagClass = handle.ReadUInt32();
                //     switch (tagClass)
                //     {
                //         case 0x80806d8f: // Stores the hash of the EntityModel type
                //             // resource.Read<EntityResource_8F6D8080>(handle, entityResourceType.EntityModelHeader);
                //             break;
                //         default:
                //             break;
                //     }
                //     var a = 0;
                //
                //     // pointer.SetOriginal();
                //     
                //     
                //     field.SetValue(b, resource);
                //     result = (T)b;
                // }
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
                            if (tagHash.Hash != 0x811c9dc5 && tagHash.Hash != 0xffffffff && CheckTagHashValid(tagHash.GetString()))
                            {
                                // dynamic tag = Activator.CreateInstance(field.FieldType.GetElementType(), tagHash);
                                // tag.Parse();
                                dynamic tag = PackageHandler.GetTag(field.FieldType.GetElementType(), tagHash);
                                tagArray[i] = tag;
                            }
                        }
                        field.SetValue(result, tagArray);
                    }
                    else
                    {
                        TagHash tagHash = new TagHash(handle.ReadUInt32());
                        if (tagHash.Hash != 0x811c9dc5 && tagHash.Hash != 0xffffffff)
                        {
                            // dynamic tag = Activator.CreateInstance(field.FieldType, tagHash);
                            // tag.Parse();
                            dynamic tag = PackageHandler.GetTag(field.FieldType, tagHash);
                            field.SetValue(result, tag);
                        }
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
                            if (T == typeof(D2Class_069B8080))  // If entity resource, don't bother reading if we don't know it (too many types)
                            {
                                return result;
                            }
                            throw new NotImplementedException($"Unable to find type for resource class {Endian.U32ToString(resourceClass)}");
                        }
                        dynamic resource = ReadStruct(innerType, handle);
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
                        throw new NotImplementedException($"Unable to find type for resource class {Endian.U32ToString(resourceClass)}");
                    }
                    dynamic resource = ReadStruct(innerType, handle);
                    field.SetValue(result, resource);
                }
                else if (fieldType == FieldType.ResourcePointerWithClass)
                {
                    long resourcePointer = handle.ReadInt64();
                    uint resourceClass = handle.ReadUInt32();
                    if (resourcePointer > 0)
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (fieldType == FieldType.ResourcePointer)
                {
                    long resourcePointer = handle.ReadInt64();
                    handle.BaseStream.Seek(resourcePointer-8, SeekOrigin.Current);
                    Type innerType = field.FieldType;
                    dynamic resource = ReadStruct(innerType, handle);
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
                    int bIs32Bit = (handle.ReadInt32() >> 31) & 0x1;
                    var u64 = handle.ReadUInt64();
                    if (bIs32Bit == 0)
                    {
                        tagHash = new TagHash(u64);
                    }
                    if (tagHash.Hash != 0x811c9dc5 && tagHash.Hash != 0xffffffff && (tagHash.Hash != 0 && bIs32Bit == 0))
                    {
                        // dynamic tag = Activator.CreateInstance(field.FieldType, tagHash);
                        // tag.Parse();
                        dynamic tag = PackageHandler.GetTag(field.FieldType, tagHash);
                        field.SetValue(result, tag);
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
                    if ((tagHash.Hash != 0x811c9dc5 && tagHash.Hash != 0xffffffff) || (tagHash.Hash != 0 && bIs32Bit == 0))
                    {
                        StringContainer tag = PackageHandler.GetTag(typeof(StringContainer), tagHash);
                        DestinyHash key = new DestinyHash(handle.ReadUInt32());
                        string resStr = tag.GetStringFromHash(ELanguage.English, key);
                        field.SetValue(result, resStr);
                    }
                    else
                    {
                        field.SetValue(result, "%%FAIL%%");
                    }
                }
                else if (fieldType == FieldType.StringNoContainer)
                {
                    // We need to use the cached 02218080 string classes
                    DestinyHash key = new DestinyHash(handle.ReadUInt32());
                    string globalString = PackageHandler.GetGlobalString(key);
                    field.SetValue(result, globalString);
                }
                else
                {
                    throw new NotImplementedException();
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
    public extern static int VerifyFieldLocation([MarshalAs(UnmanagedType.LPStr)] string structureType, long fieldOffset);
        
    [DllImport("Symmetry.dll", EntryPoint = "DllIsStructureValid", CallingConvention = CallingConvention.StdCall)]
    public extern static bool IsStructureValid([MarshalAs(UnmanagedType.LPStr)] string structureType);
        
    // protected object ReadResource()
    // {
    //     // A resource is from a pointer in header; if that pointer - 4 is a class hash, then it's a resource
    //     // There are also global pointer things but they can't be pointed to by the header
    //     var resourceType = ReadStruct<ResourceType>();
    //     var resource = ReadResource(resourceType);
    //     return resource;
    // }
        
    protected void Parse()
    {
        GetHandle();
        ParseStructs();
        ParseData();
        CloseHandle();
    }
        
    protected virtual void ParseStructs()
    {
    }
        
    protected virtual void ParseData()
    {
    }
        
    public static bool CheckTagHashValid(string hash)
    {
        if (hash.Length != 8 && hash.Length != 16)
        {
            return false;
        }

        if (hash.Length == 8)
        { 
            // Quick verify
            if (!hash.EndsWith("80") || hash.EndsWith("8080"))
            {
                return false;
            }

            return true;
        }
        if (hash.Length == 16)
        {
            // Quick verify
            if (!hash.StartsWith("0000"))
            {
                return false;
            }

            return true;
        }
        return false;
    }
}

public class Tag<T> : Tag where T : struct
{
    public T Header;

    public Tag(TagHash tagHash) : base(tagHash)
    {
    }
        
    public Tag(string tagHash) : base(tagHash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<T>();
    }
}