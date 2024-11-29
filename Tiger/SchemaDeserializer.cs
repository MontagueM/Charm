using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;
using Arithmic;
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

    // Stores all interface types as a map between the interface and the implementing type
    private readonly ConcurrentDictionary<Type, Type> _interfaceImplementingTypeMap = new();

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
                    _nonSchemaTypeMap.TryAdd(type, new TypeSubType { Type = nonSchemaStructAttr.Type, SubTypes = nonSchemaStructAttr.SubTypes });
                    _schemaTypeFieldsMap.TryAdd(type, GetStrategyFields(type.GetFields()));
                }
            }

            SchemaStructAttribute? schemaStructAttribute = GetAttribute<SchemaStructAttribute>(type);
            if (schemaStructAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, schemaStructAttribute.SerializedSize);
                _schemaHashTypeMap.TryAdd(new FileHash(schemaStructAttribute.ClassHash).Hash32, type);
                _schemaTypeHashMap.TryAdd(type, new FileHash(schemaStructAttribute.ClassHash).Hash32);
                _schemaTypeFieldsMap.TryAdd(type, GetStrategyFields(type.GetFields()));
                return;
            }

            NonSchemaStructAttribute? nonSchemaStructAttribute = GetAttribute<NonSchemaStructAttribute>(type);
            if (nonSchemaStructAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, nonSchemaStructAttribute.SerializedSize);
                _nonSchemaTypeMap.TryAdd(type, new TypeSubType { Type = nonSchemaStructAttribute.Type, SubTypes = nonSchemaStructAttribute.SubTypes });
                _schemaTypeFieldsMap.TryAdd(type, GetStrategyFields(type.GetFields()));
                return;
            }

            NonSchemaTypeAttribute? nonSchemaTypeAttr = GetAttribute<NonSchemaTypeAttribute>(type);
            if (nonSchemaTypeAttr != null)
            {
                _nonSchemaTypeMap.TryAdd(type, new TypeSubType { Type = nonSchemaTypeAttr.Type, SubTypes = nonSchemaTypeAttr.SubTypes });
                return;
            }

            SchemaTypeAttribute? schemaTypeAttribute = (SchemaTypeAttribute?)type.GetCustomAttribute(typeof(SchemaTypeAttribute), true);
            if (schemaTypeAttribute != null)
            {
                _schemaSerializedSizeMap.TryAdd(type, schemaTypeAttribute.SerializedSize);
                return;
            }

            bool isSchemaInterface = type.GetInterfaces().Contains(typeof(ISchema));
            if (isSchemaInterface)
            {
                // ConcurrentDictionary<TigerStrategy, Type> interfaceMap = new();
                // find the types that implement this interface, then find the strategy from their namespaces
                foreach (Type interfaceType in types.Where(t => t.GetInterfaces().Contains(type)))
                {
                    TigerStrategy strategy = GetStrategyFromNamespace(interfaceType.Namespace);
                    if (strategy == TigerStrategy.NONE)
                    {
                        throw new Exception($"Could not find strategy for interface {interfaceType}");
                    }
                    if (strategy > _strategy)
                    {
                        continue;
                    }
                    // interfaceMap.TryAdd(strategy, interfaceType);

                    SchemaStructAttribute? intSchemaStructAttribute = GetAttribute<SchemaStructAttribute>(interfaceType.BaseType.GenericTypeArguments.First());
                    _schemaTypeHashMap.TryAdd(type, new FileHash(intSchemaStructAttribute.ClassHash).Hash32);
                    _interfaceImplementingTypeMap.TryAdd(type, interfaceType);
                }
            }
        });
    }

    private FieldInfo[] GetStrategyFields(FieldInfo[] getFields)
    {
#if DEBUG
        foreach (var field in getFields)
        {
            var attributes = field.GetCustomAttributes<SchemaFieldAttribute>().ToArray();
            // Check if attributes array is null or empty
            if (attributes == null || attributes.Length == 0)
                continue;

            var attribute = GetAttribute<SchemaFieldAttribute>(field);

            // Check if attribute is null
            if (attribute == null)
                Console.WriteLine($"Attribute for field {field.Name} is null. ({field.FieldType}, {field.ReflectedType.Name})");
        }
#endif
        // don't include fields that have a strategy assigned but are larger than us
        return getFields.Where(f => !f.GetCustomAttributes<SchemaFieldAttribute>().Any() || (_strategy >= GetAttribute<SchemaFieldAttribute>(f).Strategy && !GetAttribute<SchemaFieldAttribute>(f).Obsolete)).ToArray();
    }

    private TigerStrategy GetStrategyFromNamespace(string namespaceString)
    {
        string strategyString = namespaceString.Split(".").Last();
        if (Enum.TryParse(strategyString, out TigerStrategy strategy))
        {
            return strategy;
        }

        Log.Error($"Could not parse strategy from namespace {namespaceString}");
        return TigerStrategy.NONE;
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

            foreach (FieldInfo fieldInfo in GetStrategyFields(type.GetFields()))
            {
                if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(ISchema)))
                {
                    continue;
                }

                SchemaFieldAttribute? schemaFieldAttribute = GetAttribute<SchemaFieldAttribute>(fieldInfo);
                if (schemaFieldAttribute != null && schemaFieldAttribute.Offset != -1)
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
                    // todo figure out why this doesnt work wrt to the deserializer also needing its own enum parsing
                    _schemaSerializedSizeMap.TryAdd(fieldInfo.FieldType, Marshal.SizeOf(Enum.GetUnderlyingType(fieldInfo.FieldType)));
                    continue;
                }
                if (fieldInfo.FieldType is { IsGenericType: false, IsArray: false } && !_schemaSerializedSizeMap.ContainsKey(fieldInfo.FieldType))
                {
                    _schemaSerializedSizeMap.TryAdd(fieldInfo.FieldType, Marshal.SizeOf(fieldInfo.FieldType));
                    continue;
                }
            }
        });
    }

    public static T DeserializeTag64<T>(TigerReader reader, bool shouldLoad = true) where T : TigerFile
    {
        return (T)DeserializeTag64(reader, typeof(T), shouldLoad);
    }

    public static FileHash GetFileHashFrom64(TigerReader reader)
    {
        uint u32 = reader.ReadUInt32();
        int bIs32Bit = reader.ReadInt32();
        ulong u64 = reader.ReadUInt64();
        //Console.WriteLine($"{u32:X} : {bIs32Bit:X} : {u64:X}");
        if (bIs32Bit == 1 || bIs32Bit == 2) // TFS can have 2 instead of 1?
        {
            return new FileHash(u32);
        }
        else if (bIs32Bit == 0)
        {
            return new FileHash64(u64);
        }
        else
        {
            reader.DumpToFile();
            throw new Exception($"Invalid bIs32Bit value ({bIs32Bit}) at Position {reader.Position - 0x8 - 0x4:X} in {reader.Hash:X}");
        }
    }

    public static TigerFile DeserializeTag64(TigerReader reader, Type fieldType, bool shouldLoad = true)
    {
        return FileResourcer.Get().GetFile(fieldType, GetFileHashFrom64(reader), shouldLoad);
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
            Type fieldType = fieldInfo.FieldType;
            if (IsSchemaInterfaceType(fieldType))
            {
                fieldType = GetSchemaInterfaceType(fieldType);
            }

            if (GetSchemaFieldOffset(schemaType, fieldInfo, out int offset))
            {
                fieldOffset = offset;
            }

            reader.Seek(startOffset + fieldOffset, SeekOrigin.Begin);

            long fieldSize = -1;
            dynamic? fieldValue;
            if (IsTigerFile64Type(fieldInfo))
            {
                // hardcode allow for 64 bit file but instead just the hash as FileHash
                if (fieldType == typeof(FileHash))
                {
                    fieldValue = GetFileHashFrom64(reader);
                }
                else
                {
                    bool shouldLoad = !HasNoLoadAttribute(fieldInfo);
                    fieldValue = DeserializeTag64(reader, fieldType, shouldLoad);
                }
                fieldSize = 0x10;
            }
            else if (IsTigerDeserializeType(fieldType))
            {
                fieldValue = DeserializeTigerType(reader, fieldType);

                // fieldSize for DynamicStruct has to be the size of the type given, this seems to work fine, but idk if its optimal or not
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(DynamicStruct<>))
                    fieldSize = GetSchemaTypeSize(fieldType.GetGenericArguments()[0]); // Needs to be the type inside DynamicStruct<T>
                else
                    fieldSize = GetSchemaTypeSize(fieldType);
            }
            else if (IsTigerFileType(fieldType))
            {
                FileHash fileHash = new(reader.ReadUInt32());
                bool shouldLoad = !HasNoLoadAttribute(fieldInfo);
                fieldValue = FileResourcer.Get().GetFile(fieldType, fileHash, shouldLoad);
                fieldSize = GetSchemaTypeSize(fieldType);
            }
            else if (fieldType.IsEnum)
            {
                // Seems to work fine? Mont, yell at me if this isnt okay
                fieldValue = Enum.ToObject(fieldType, reader.ReadType(Enum.GetUnderlyingType(fieldInfo.FieldType)));
                fieldSize = GetSchemaTypeSize(fieldType);
            }
            else if (fieldType.IsArray)
            {
                var attr = GetAttribute<SchemaFieldAttribute>(fieldInfo);
                if (attr == null)
                {
                    throw new Exception($"Array type must have SchemaFieldAttribute to define array size. ({reader.Hash:X})");
                }
                int arraySize = attr.ArraySizeConst;
                fieldValue = Activator.CreateInstance(fieldType, arraySize);
                for (int i = 0; i < arraySize; i++)
                {
                    dynamic value;
                    if (IsTigerDeserializeType(fieldType))
                    {
                        value = DeserializeTigerType(reader, fieldType.GetElementType());
                    }
                    else
                    {
                        value = DeserializeMarshalType(reader, fieldType.GetElementType());
                    }
                    // assume it's a tiger deserialize type
                    fieldValue[i] = value;
                }

                if (IsTigerDeserializeType(fieldType))
                {
                    fieldSize = GetSchemaTypeSize(fieldType.GetElementType()) * arraySize;
                }
                else
                {
                    fieldSize = Marshal.SizeOf(fieldType.GetElementType()) * arraySize;
                }
            }
            else
            {
                fieldValue = DeserializeMarshalType(reader, fieldType);
                fieldSize = Marshal.SizeOf(fieldType);
            }
            fieldInfo.SetValue(resource, fieldValue); // todo check if this set is required, might be a reference type?

            if (fieldSize == -1)
            {
                throw new Exception($"Failed to get field size for field {fieldInfo.Name} of type {fieldType} ({reader.Hash:X})");
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

    public bool IsSchemaInterfaceType(Type fieldType)
    {
        return _interfaceImplementingTypeMap.ContainsKey(fieldType);
    }

    public Type GetSchemaInterfaceType(Type fieldType)
    {
        return _interfaceImplementingTypeMap[fieldType];
    }

    private bool IsTigerFile64Type(FieldInfo fieldInfo)
    {
        return GetFirstAttribute<Tag64Attribute>(fieldInfo) != null;
    }

    private bool HasNoLoadAttribute(FieldInfo fieldInfo)
    {
        return GetFirstAttribute<NoLoadAttribute>(fieldInfo) != null;
    }

    private dynamic DeserializeTigerType(TigerReader reader, Type fieldType)
    {
        ITigerDeserialize? fieldValue = (ITigerDeserialize?)Activator.CreateInstance(fieldType);
        if (fieldValue == null)
        {
            throw new Exception($"Failed to create schema field instance of type {fieldType} ({reader.Hash:X})");
        }

        fieldValue.Deserialize(reader);
        return fieldValue;
    }

    public T DeserializeTigerType<T>(TigerReader reader) where T : ITigerDeserialize, new()
    {
        T fieldValue = Activator.CreateInstance<T>();
        if (fieldValue == null)
        {
            throw new Exception($"Failed to create schema field instance of type {typeof(T)} ({reader.Hash:X})");
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
        // todo all this stuff should be cached, reflection is slow
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
            Log.Error($"Failed to get schema struct size for type {var} as it has multiple schema struct attributes but none match the current strategy {_strategy}");
            return null;
        }
    }
}
