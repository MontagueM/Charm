using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using ConcurrentCollections;

namespace Tiger;

public struct TypeIdentifier
{
    public uint ClassHash { get; set; }
    public int Type { get; set; }
    public HashSet<int> SubTypes { get; set; }

    public bool HasClassHash()
    {
        return ClassHash != TigerHash.InvalidHash32;
    }

    public bool HasTypeSubType()
    {
        return Type != -1 && SubTypes.Count > 0;
    }
}

public struct TypeSubType
{
    public int Type { get; set; }
    public HashSet<int> SubTypes { get; set; }
}

public class SchemaDeserializer : Strategy.StrategistSingleton<SchemaDeserializer>
{
    // todo separate into different class?
    // todo maybe coalesce into one or two dictionaries?

    // todo source generation

    // Stores both SchemaStruct and SchemaType sizes
    private readonly ConcurrentDictionary<Type, int> _schemaSerializedSizeMap = new();

    // First maps the struct schema type, then the field name
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, int>> _schemaFieldOffsetMap = new();

    // 0x8080.... class hash to type
    private readonly ConcurrentDictionary<uint, Type> _schemaHashTypeMap = new();

    // type to 0x8080.... class hash
    private readonly ConcurrentDictionary<Type, uint> _schemaTypeHashMap = new();

    // type to type and subtype class hash
    private readonly ConcurrentDictionary<Type, TypeSubType> _nonSchemaTypeMap = new();

    // Stores SchemaType FieldInfo[] array
    private readonly ConcurrentDictionary<Type, FieldInfo[]> _schemaTypeFieldsMap = new();

    // Stores all ITagDeserialize types
    private readonly ConcurrentHashSet<Type> _tagDeserializeTypes = new();

    public SchemaDeserializer(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Initialise()
    {
        FillSchemaCaches();
    }

    protected override void Reset()
    {
        // we don't need to reset anything as irrelevant to any configuration changes
    }

    public bool TryGetSchemaType(uint classHash, out Type schemaType)
    {
        return _schemaHashTypeMap.TryGetValue(classHash, out schemaType);
    }

    // public bool TryGetNonSchemaType(int type, int subType, out Type schemaType)
    // {
    //     return _schemaHashTypeMap.TryGetValue(classHash, out schemaType);
    // }
    //
    // public bool TryGetNonSchemaType(int type, out Type schemaType)
    // {
    //     return _schemaHashTypeMap.TryGetValue(classHash, out schemaType);
    // }

    public bool TryGetSchemaTypeIdentifier(Type schemaType, out TypeIdentifier typeIdentifier)
    {
        _schemaTypeHashMap.TryGetValue(schemaType, out uint classHash);
        _nonSchemaTypeMap.TryGetValue(schemaType, out TypeSubType typeSubType);

        typeIdentifier = new TypeIdentifier
        {
            ClassHash = classHash != 0 ? classHash : TigerHash.InvalidHash32,
            Type = typeSubType.Type != 0 ? typeSubType.Type : -1,
            SubTypes = typeSubType.SubTypes != null ? typeSubType.SubTypes : new HashSet<int>()
        };

        return typeIdentifier.ClassHash != TigerHash.InvalidHash32 || typeIdentifier.Type != -1 || typeIdentifier.SubTypes.Any();
    }

    private void FillSchemaCaches()
    {
        FillSchemaTypeCaches();
        FillTypeFieldOffsetMap();
    }

    private void FillSchemaTypeCaches()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes());

        Parallel.ForEach(types, type =>
        {
            bool isTigerDeserialize = type.FindInterfaces((t, _) => t == typeof(ITigerDeserialize), null).Length > 0;
            if (isTigerDeserialize)
            {
                _tagDeserializeTypes.Add(type);
            }

            // only first level of inheritance
            if (type.IsSubclassOf(typeof(TigerFile)) && type.BaseType.IsGenericType)
            {
                // Type? schemaType = type.GetNestedType("Schema", BindingFlags.NonPublic);
                Type schemaType = type.BaseType.GenericTypeArguments.First();

                SchemaStructAttribute? schemaStructAttr = GetAttribute<SchemaStructAttribute>(schemaType);
                if (schemaStructAttr != null && !string.IsNullOrEmpty(schemaStructAttr.ClassHash))
                {
                    _schemaTypeHashMap.TryAdd(type, new FileHash(schemaStructAttr.ClassHash).Hash32);
                }

                NonSchemaStructAttribute? nonSchemaStructAttr = GetAttribute<NonSchemaStructAttribute>(schemaType);
                if (nonSchemaStructAttr != null)
                {
                    _schemaSerializedSizeMap.TryAdd(type, nonSchemaStructAttr.SerializedSize);
                    _nonSchemaTypeMap.TryAdd(type, new TypeSubType{ Type = nonSchemaStructAttr.Type, SubTypes = nonSchemaStructAttr.SubTypes});
                    _schemaTypeFieldsMap.TryAdd(type, type.GetFields());
                    return;
                }
            }

            SchemaStructAttribute? schemaStructAttribute = GetAttribute<SchemaStructAttribute>(type);
            if (schemaStructAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, schemaStructAttribute.SerializedSize);
                _schemaHashTypeMap.TryAdd(new FileHash(schemaStructAttribute.ClassHash).Hash32, type);
                _schemaTypeHashMap.TryAdd(type, new FileHash(schemaStructAttribute.ClassHash).Hash32);
                _schemaTypeFieldsMap.TryAdd(type, type.GetFields());
                return;
            }

            NonSchemaStructAttribute? nonSchemaStructAttribute = GetAttribute<NonSchemaStructAttribute>(type);
            if (nonSchemaStructAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, nonSchemaStructAttribute.SerializedSize);
                _nonSchemaTypeMap.TryAdd(type, new TypeSubType{ Type = nonSchemaStructAttribute.Type, SubTypes = nonSchemaStructAttribute.SubTypes});
                _schemaTypeFieldsMap.TryAdd(type, type.GetFields());
                return;
            }

            NonSchemaTypeAttribute? nonSchemaTypeAttr = GetFirstAttribute<NonSchemaTypeAttribute>(type);
            if (nonSchemaTypeAttr != null)
            {
                _nonSchemaTypeMap.TryAdd(type, new TypeSubType{ Type = nonSchemaTypeAttr.Type, SubTypes = nonSchemaTypeAttr.SubTypes});
                return;
            }

            SchemaTypeAttribute? schemaTypeAttribute = (SchemaTypeAttribute?)type.GetCustomAttribute(typeof(SchemaTypeAttribute), true);
            if (schemaTypeAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, schemaTypeAttribute.SerializedSize);
                return;
            }
        });
    }

    // side effect also adds some more types to the serialized size map
    private void FillTypeFieldOffsetMap()
    {
        Parallel.ForEach(_schemaSerializedSizeMap
            .Keys
            .Where(t => GetAttribute<SchemaStructAttribute>(t) != null || GetAttribute<NonSchemaStructAttribute>(t) != null),
            type =>
        {
            _schemaFieldOffsetMap.TryAdd(type, new ConcurrentDictionary<string, int>());

            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                SchemaFieldAttribute? schemaFieldAttribute = GetAttribute<SchemaFieldAttribute>(fieldInfo);
                if (schemaFieldAttribute != null)
                {
                    _schemaFieldOffsetMap[type].TryAdd(fieldInfo.Name, schemaFieldAttribute.Offset);
                }
                // else
                // {
                //     FieldOffsetAttribute? fieldOffsetAttribute = (FieldOffsetAttribute?)fieldInfo.GetCustomAttribute(typeof(FieldOffsetAttribute), false);
                //     if (fieldOffsetAttribute != null)
                //     {
                //         _schemaFieldOffsetMap[type].TryAdd(fieldInfo.Name, fieldOffsetAttribute.Value);
                //     }
                // }

                // Also add any other types we need e.g. uint, ushort, etc.
                if (fieldInfo.FieldType.IsEnum)
                {
                    _schemaSerializedSizeMap.TryAdd(fieldInfo.FieldType, Marshal.SizeOf(Enum.GetUnderlyingType(fieldInfo.FieldType)));
                    continue;
                }
                if (fieldInfo.FieldType is {IsGenericType: false, IsArray: false} && !_schemaSerializedSizeMap.ContainsKey(fieldInfo.FieldType))
                {
                    _schemaSerializedSizeMap.TryAdd(fieldInfo.FieldType, Marshal.SizeOf(fieldInfo.FieldType));
                    continue;
                }
            }
        });
    }

    public static T DeserializeTag64<T>(TigerReader reader) where T : TigerFile
    {
        return (T)DeserializeTag64(reader, typeof(T));
    }

    public static TigerFile DeserializeTag64(TigerReader reader, Type fieldType)
    {
        uint u32 = reader.ReadUInt32();
        int bIs32Bit = reader.ReadInt32();
        ulong u64 = reader.ReadUInt64();
        if (bIs32Bit == 1)
        {
            return FileResourcer.Get().GetFile(fieldType, new FileHash(u32));
        }
        else if (bIs32Bit == 0)
        {
            return FileResourcer.Get().GetFile(fieldType, new FileHash64(u64));
        }
        else
        {
            throw new Exception("Invalid bIs32Bit value");
        }
    }

    public T DeserializeSchema<T>(TigerReader reader) where T : struct
    {
        return DeserializeSchema(reader, typeof(T));
    }

    public dynamic DeserializeSchema(TigerReader reader, Type schemaType)
    {
        object resource = Activator.CreateInstance(schemaType);

        FieldInfo[] fields = GetSchemaFields(schemaType);
        long startOffset = reader.Position;
        long fieldOffset = 0;
        foreach (FieldInfo fieldInfo in fields)
        {
            if (GetSchemaFieldOffset(schemaType, fieldInfo, out int offset))
            {
                fieldOffset = offset;
            }

            reader.Seek(startOffset + fieldOffset, SeekOrigin.Begin);

            long fieldSize = -1;
            dynamic? fieldValue;
            if (IsTigerDeserializeType(fieldInfo.FieldType))
            {
                fieldValue = DeserializeTigerType(reader, fieldInfo.FieldType);
                fieldSize = GetSchemaTypeSize(fieldInfo.FieldType);
            }
            else if (IsTigerFile64Type(fieldInfo))
            {
                fieldValue = DeserializeTag64(reader, fieldInfo.FieldType);
                fieldSize = 0x10;
            }
            else if (IsTigerFileType(fieldInfo.FieldType))
            {
                FileHash fileHash = new(reader.ReadUInt32());
                fieldValue = FileResourcer.Get().GetFile(fieldInfo.FieldType, fileHash);
                fieldSize = GetSchemaTypeSize(fieldInfo.FieldType);
            }
            else if (fieldInfo.FieldType.IsEnum)
            {
                fieldValue = Enum.ToObject(fieldInfo.FieldType, reader.ReadByte());
                fieldSize = 1;
            }
            else if (fieldInfo.FieldType.IsArray)
            {
                int arraySize = ((MarshalAsAttribute)fieldInfo.GetCustomAttribute(typeof(MarshalAsAttribute), true)).SizeConst;
                fieldValue = Activator.CreateInstance(fieldInfo.FieldType, arraySize);
                for (int i = 0; i < arraySize; i++)
                {
                    dynamic value;
                    if (IsTigerDeserializeType(fieldInfo.FieldType))
                    {
                        value =  DeserializeTigerType(reader, fieldInfo.FieldType.GetElementType());
                    }
                    else
                    {
                        value =  DeserializeMarshalType(reader, fieldInfo.FieldType.GetElementType());
                    }
                    // assume it's a tiger deserialize type
                    fieldValue[i] = value;
                }

                if (IsTigerDeserializeType(fieldInfo.FieldType))
                {
                    fieldSize = GetSchemaTypeSize(fieldInfo.FieldType.GetElementType()) * arraySize;
                }
                else
                {
                    fieldSize = Marshal.SizeOf(fieldInfo.FieldType.GetElementType()) * arraySize;
                }
            }
            else
            {
                fieldValue = DeserializeMarshalType(reader, fieldInfo.FieldType);
                fieldSize = Marshal.SizeOf(fieldInfo.FieldType);
            }
            fieldInfo.SetValue(resource, fieldValue); // todo check if this set is required, might be a reference type?

            if (fieldSize == -1)
            {
                throw new Exception($"Failed to get field size for field {fieldInfo.Name} of type {fieldInfo.FieldType}");
            }
            fieldOffset += fieldSize;
        }


        return resource;
    }

    private FieldInfo[] GetSchemaFields(Type schemaType)
    {
        if (_schemaTypeFieldsMap.TryGetValue(schemaType, out FieldInfo[]? fields))
        {
            return fields;
        }

        throw new Exception($"Failed to get schema fields for type {schemaType}");
    }

    public bool IsTigerDeserializeType(Type type)
    {
        if (type.IsGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }
        return _tagDeserializeTypes.Contains(type);
    }

    public bool IsTigerFileType(Type fieldType)
    {
        return fieldType == typeof(TigerFile) || fieldType.IsSubclassOf(typeof(TigerFile));
    }

    private bool IsTigerFile64Type(FieldInfo fieldInfo)
    {
        return GetFirstAttribute<Tag64Attribute>(fieldInfo) != null;
    }

    private dynamic DeserializeTigerType(TigerReader reader, Type fieldType)
    {
        ITigerDeserialize? fieldValue = (ITigerDeserialize?)Activator.CreateInstance(fieldType);
        if (fieldValue == null)
        {
            throw new Exception($"Failed to create schema field instance of type {fieldType}");
        }

        fieldValue.Deserialize(reader);
        return fieldValue;
    }

    private dynamic DeserializeMarshalType(TigerReader reader, Type fieldType)
    {
        return reader.ReadType(fieldType);
    }

    public int GetSchemaStructSize<T>()
    {
        return GetSchemaStructSize(typeof(T));
    }

    private int GetSchemaStructSize(Type type)
    {
        if (_schemaSerializedSizeMap.TryGetValue(type, out int size))
        {
            return size;
        }
        else
        {
            // todo make this recursive
            if (_schemaSerializedSizeMap.TryGetValue(type.BaseType, out size))
            {
                return size;
            }
            else
            {
                return Marshal.SizeOf(type);
            }
        }
    }

    private int GetSchemaTypeSize(Type type)
    {
        if (type.IsGenericType)
        {
            type = type.GetGenericTypeDefinition();
        }
        if (_schemaSerializedSizeMap.TryGetValue(type, out int size))
        {
            return size;
        }
        else
        {
            throw new Exception($"Failed to get schema type size for type {type} as it has no schema type attribute");
        }
    }

    private bool GetSchemaFieldOffset(Type schemaType, FieldInfo fieldInfo, out int offset)
    {
        if (!_schemaFieldOffsetMap.ContainsKey(schemaType))
        {
            offset = 0;
            return false;
        }
        return _schemaFieldOffsetMap[schemaType].TryGetValue(fieldInfo.Name, out offset);
        //
        // offset = 0;
        // SchemaFieldAttribute? attribute = GetAttribute<SchemaFieldAttribute>(fieldInfo);
        // if (attribute == null)
        // {
        //     return GetFieldOffset(fieldInfo, out offset);
        // }
        // offset = attribute.Offset;
        // return true;
    }

    // private bool GetFieldOffset(FieldInfo fieldInfo, out int offset)
    // {
    //     offset = 0;
    //     FieldOffsetAttribute? attribute = (FieldOffsetAttribute?)fieldInfo.GetCustomAttribute(typeof(FieldOffsetAttribute), false);
    //     if (attribute == null)
    //     {
    //         return false;
    //     }
    //     offset = attribute.Value;
    //     return true;
    // }

    private T? GetFirstAttribute<T>(ICustomAttributeProvider var) where T : Attribute
    {
        T[] attributes = var.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        if (!attributes.Any())
        {
            // we still want to be able to get size of non-schema types
            return null;
        }
        return attributes.First();
    }

    private T? GetAttribute<T>(ICustomAttributeProvider var) where T : StrategyAttribute
    {
        T[] attributes = var.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        if (!attributes.Any())
        {
            // we still want to be able to get size of non-schema types
            return null;
        }
        // if there's only one attribute, presume it applies to every strategy
        if (attributes.Length == 1)
        {
            return attributes.First();
        }

        // otherwise we need to find the one that matches the current strategy
        Dictionary<TigerStrategy, T> map = attributes.ToDictionary(a => a.Strategy, a => a);
        map.GetFullStrategyMap();
        if (map.TryGetValue(_strategy, out T attribute))
        {
            return attribute;
        }
        else
        {
            throw new Exception($"Failed to get schema struct size for type {var} as it has multiple schema struct attributes but none match the current strategy {_strategy}");
        }
    }
}
