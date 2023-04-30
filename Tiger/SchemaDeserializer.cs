using System.Reflection;
using System.Runtime.InteropServices;

namespace Tiger;

public class SchemaDeserializer : Strategy.StrategistSingleton<SchemaDeserializer>
{
    public SchemaDeserializer(TigerStrategy strategy) : base(strategy)
    {
    }
    
    public T DeserializeSchema<T>(TigerReader reader) where T : struct
    {
        return DeserializeSchema(reader, typeof(T));
    }

    public dynamic DeserializeSchema(TigerReader reader, Type schemaType)
    {
        object resource = Activator.CreateInstance(schemaType);

        FieldInfo[] fields = schemaType.GetFields();
        long startOffset = reader.Position;
        long fieldOffset = 0;
        foreach (FieldInfo fieldInfo in fields)
        {
            if (GetSchemaFieldOffset(fieldInfo, out int offset))
            {
                fieldOffset = offset;
            }

            reader.Seek(startOffset + fieldOffset, SeekOrigin.Begin);

            long fieldSize = -1;
            dynamic? fieldValue;
            if (IsTigerDeserializeType(fieldInfo))
            {
                fieldValue = DeserializeTigerType(reader, fieldInfo.FieldType);
                fieldSize = GetSchemaTypeSize(fieldInfo.FieldType);
            }
            else if (IsTigerFile64Type(fieldInfo))
            {
                uint u32 = reader.ReadUInt32();
                int bIs32Bit = reader.ReadInt32();
                ulong u64 = reader.ReadUInt64();
                if (bIs32Bit == 1)
                {
                    fieldValue = FileResourcer.Get().GetFile(fieldInfo.FieldType, new FileHash(u32));
                }
                else if (bIs32Bit == 0)
                {
                    fieldValue = FileResourcer.Get().GetFile(fieldInfo.FieldType, new File64Hash(u64));
                }
                else
                {
                    throw new Exception("Invalid bIs32Bit value");
                }

                fieldSize = GetSchemaTypeSize(fieldInfo.FieldType);
            }
            else if (IsTigerFileType(fieldInfo))
            {
                FileHash fileHash = new FileHash(reader.ReadUInt32());
                fieldValue = FileResourcer.Get().GetFile(fieldInfo.FieldType, fileHash);
                fieldSize = GetSchemaTypeSize(fieldInfo.FieldType);
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
    
    private bool IsTigerDeserializeType(FieldInfo fieldInfo)
    {
        return fieldInfo.FieldType.FindInterfaces((type, _) => type == typeof(ITigerDeserialize), null).Length > 0;
    }
    
    private bool IsTigerFileType(FieldInfo fieldInfo)
    {
        return fieldInfo.FieldType.IsSubclassOf(typeof(TigerFile));
    }
    
    private bool IsTigerFile64Type(FieldInfo fieldInfo)
    {
        return fieldInfo.FieldType.IsSubclassOf(typeof(Tag64<>));
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

    public int GetSchemaStructSize<T>() where T : struct
    {
        return GetSchemaStructSize(typeof(T));
    }
    
    private int GetSchemaStructSize(Type type)
    {
        SchemaStructAttribute? attribute = GetAttribute<SchemaStructAttribute>(type);
        if (attribute == null)
        {
            return Marshal.SizeOf(type);
        }
        return attribute.SerializedSize;
    }
    
    private T? GetAttribute<T>(ICustomAttributeProvider var) where T : StrategyAttribute
    {
        T[] attributes = var.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        if (!attributes.Any())
        {
            // we still want to be able to get size of non-schema types
            return null;
        }
        if (attributes.Length == 1)
        {
            return attributes.First();
        }
        T? attribute = (T?) var.GetCustomAttributes(typeof(T), false).FirstOrDefault(a => ((T)a).Strategy == _strategy);
        if (attribute == null)
        {
            throw new Exception($"Failed to get schema struct size for type {var} as it has multiple schema struct attributes but none match the current strategy {_strategy}");
        }
        return attribute;
    }

    private int GetSchemaTypeSize<T>() where T : struct
    {
        return GetSchemaTypeSize(typeof(T));
    }
    
    private int GetSchemaTypeSize(Type type)
    {
        SchemaTypeAttribute? attribute = (SchemaTypeAttribute?) type.GetCustomAttribute(typeof(SchemaTypeAttribute), true);
        if (attribute == null)
        {
            throw new Exception($"Failed to get schema type size for type {type} as it has no schema type attribute");
        }
        return attribute.SerializedSize;
    }
    
    private bool GetSchemaFieldOffset(FieldInfo fieldInfo, out int offset)
    {
        offset = 0;
        SchemaFieldAttribute? attribute = GetAttribute<SchemaFieldAttribute>(fieldInfo);
        if (attribute == null)
        {
            return GetFieldOffset(fieldInfo, out offset);
        }
        offset = attribute.Offset;
        return true;
    }
    
    private bool GetFieldOffset(FieldInfo fieldInfo, out int offset)
    {
        offset = 0;
        FieldOffsetAttribute? attribute = (FieldOffsetAttribute?) fieldInfo.GetCustomAttribute(typeof(FieldOffsetAttribute), false);
        if (attribute == null)
        {
            return false;
        }
        offset = attribute.Value;
        return true;
    }
}