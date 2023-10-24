using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiger.Schema;
using Tiger;

public class TfxBytecodeOp
{
    public static List<TfxData> ParseAll(DynamicArray<D2Class_09008080> bytecode)
    {
        byte[] data = new byte[bytecode.Count];
        for(int i = 0; i < bytecode.Count; i++)
        {
            data[i] = bytecode[i].Value;
        }

        List<TfxData> opcodes = new();
        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (stream.Position < data.Length)
                {
                    TfxData op = ReadTfxBytecodeOp(reader);
                    opcodes.Add(op);
                }
            }
        }

        return opcodes;
    }

    public static TfxData ReadTfxBytecodeOp(BinaryReader reader)
    {
        TfxData tfxData = new()
        {
            op = (TfxBytecode)reader.ReadByte(),
            data = null
        };

        switch (tfxData.op)
        {
            case TfxBytecode.Unk0b:
                Unk0bData Unk0bData = new();
                Unk0bData.unk1 = reader.ReadByte();
                Unk0bData.unk2 = reader.ReadByte();
                tfxData.data = Unk0bData;
                break;
            case TfxBytecode.Unk22:
                Unk22Data Unk22Data = new();
                Unk22Data.unk1 = reader.ReadByte();
                tfxData.data = Unk22Data;
                break;
            case TfxBytecode.UnkLoadConstant2:
                UnkLoadConstant2Data UnkLoadConstant2Data = new();
                UnkLoadConstant2Data.constant_index = reader.ReadByte();
                tfxData.data = UnkLoadConstant2Data;
                break;
            case TfxBytecode.Unk35:
                Unk35Data Unk35Data = new();
                Unk35Data.unk1 = reader.ReadByte();
                tfxData.data = Unk35Data;
                break;
            case TfxBytecode.Unk37:
                Unk37Data Unk37Data = new();
                Unk37Data.unk1 = reader.ReadByte();
                tfxData.data = Unk37Data;
                break;
            case TfxBytecode.Unk38:
                Unk38Data Unk38Data = new();
                Unk38Data.unk1 = reader.ReadByte();
                tfxData.data = Unk38Data;
                break;
            case TfxBytecode.Unk39:
                Unk39Data Unk39Data = new();
                Unk39Data.unk1 = reader.ReadByte();
                tfxData.data = Unk39Data;
                break;
            case TfxBytecode.Unk3a:
                Unk3aData Unk3aData = new();
                Unk3aData.unk1 = reader.ReadByte();
                tfxData.data = Unk3aData;
                break;
            case TfxBytecode.UnkLoadConstant:
                UnkLoadConstantData UnkLoadConstantData = new();
                UnkLoadConstantData.constant_index = reader.ReadByte();
                tfxData.data = UnkLoadConstantData;
                break;
            case TfxBytecode.LoadExtern:
                LoadExternData LoadExternData = new();
                LoadExternData.extern_ = (TfxExtern)reader.ReadByte();
                LoadExternData.element = reader.ReadByte();
                tfxData.data = LoadExternData;
                break;
            case TfxBytecode.Unk3d:
                Unk3dData Unk3dData = new();
                Unk3dData.unk1 = reader.ReadByte();
                Unk3dData.unk2 = reader.ReadByte();
                tfxData.data = Unk3dData;
                break;
            case TfxBytecode.Unk3e:
                Unk3eData Unk3eData = new();
                Unk3eData.unk1 = reader.ReadByte();
                tfxData.data = Unk3eData;
                break;
            case TfxBytecode.Unk3f:
                Unk3fData Unk3fData = new();
                Unk3fData.unk1 = reader.ReadByte();
                tfxData.data = Unk3fData;
                break;
            case TfxBytecode.Unk42:
                Unk42Data Unk42Data = new();
                Unk42Data.unk1 = reader.ReadByte();
                Unk42Data.unk2 = reader.ReadByte();
                tfxData.data = Unk42Data;
                break;
            case TfxBytecode.Unk43:
                Unk43Data Unk43Data = new();
                Unk43Data.unk1 = reader.ReadByte();
                tfxData.data = Unk43Data;
                break;
            case TfxBytecode.StoreToBuffer:
                StoreToBufferData StoreToBufferData = new();
                StoreToBufferData.element = reader.ReadByte();
                tfxData.data = StoreToBufferData;
                break;
            case TfxBytecode.Unk45:
                Unk45Data Unk45Data = new();
                Unk45Data.unk1 = reader.ReadByte();
                tfxData.data = Unk45Data;
                break;
            case TfxBytecode.Unk46:
                Unk46Data Unk46Data = new();
                Unk46Data.unk1 = reader.ReadByte();
                tfxData.data = Unk46Data;
                break;
            case TfxBytecode.Unk47:
                Unk47Data Unk47Data = new();
                Unk47Data.unk1 = reader.ReadByte();
                tfxData.data = Unk47Data;
                break;
            case TfxBytecode.Unk48:
                Unk48Data Unk48Data = new();
                Unk48Data.unk1 = reader.ReadByte();
                tfxData.data = Unk48Data;
                break;
            case TfxBytecode.Unk49:
                Unk49Data Unk49 = new();
                Unk49.unk1 = reader.ReadByte();
                tfxData.data = Unk49;
                break;
            case TfxBytecode.Unk4a:
                Unk4aData Unk4aData = new();
                Unk4aData.unk1 = reader.ReadByte();
                tfxData.data = Unk4aData;
                break;
            case TfxBytecode.Unk4c:
                Unk4cData Unk4cData = new();
                Unk4cData.unk1 = reader.ReadByte();
                tfxData.data = Unk4cData;
                break;
            case TfxBytecode.Unk4d:
                Unk4dData Unk4dData = new();
                Unk4dData.unk1 = reader.ReadByte();
                tfxData.data = Unk4dData;
                break;
            case TfxBytecode.Unk4e:
                Unk4eData Unk4eData = new();
                Unk4eData.unk1 = reader.ReadByte();
                Unk4eData.unk2 = reader.ReadByte();
                tfxData.data = Unk4eData;
                break;
            case TfxBytecode.Unk4f:
                Unk4fData Unk4fData = new();
                Unk4fData.unk1 = reader.ReadByte();
                tfxData.data = Unk4fData;
                break;
            case TfxBytecode.Unk52:
                Unk52Data Unk52Data = new();
                Unk52Data.unk1 = reader.ReadByte();
                Unk52Data.unk2 = reader.ReadByte();
                tfxData.data = Unk52Data;
                break;
        }

        return tfxData;
    }

    public static string TfxToString(TfxData tfxData)
    {
        string output = "";
        switch (tfxData.data)
        {
            case Unk0bData:
                output = $"unk1 {((Unk0bData)tfxData.data).unk1}, unk2 {((Unk0bData)tfxData.data).unk2}";
                break;
            case Unk22Data:
                output = $"unk1 {((Unk22Data)tfxData.data).unk1}";
                break;
            case UnkLoadConstant2Data:
                output = $"constant_index {((UnkLoadConstant2Data)tfxData.data).constant_index}";
                break;
            case Unk35Data:
                output = $"unk1 {((Unk35Data)tfxData.data).unk1}";
                break;
            case Unk37Data:
                output = $"unk1 {((Unk37Data)tfxData.data).unk1}";
                break;
            case Unk38Data:
                output = $"unk1 {((Unk38Data)tfxData.data).unk1}";
                break;
            case Unk39Data:
                output = $"unk1 {((Unk39Data)tfxData.data).unk1}";
                break;
            case Unk3aData:
                output = $"unk1 {((Unk3aData)tfxData.data).unk1}";
                break;
            case UnkLoadConstantData:
                output = $"constant_index {((UnkLoadConstantData)tfxData.data).constant_index}";
                break;
            case LoadExternData:
                output = $"extern {((LoadExternData)tfxData.data).extern_}, element {((LoadExternData)tfxData.data).element}";
                break;
            case Unk3dData:
                output = $"unk1 {((Unk3dData)tfxData.data).unk1}, unk2 {((Unk3dData)tfxData.data).unk2}";
                break;
            case Unk3eData:
                output = $"unk1 {((Unk3eData)tfxData.data).unk1}";
                break;
            case Unk3fData:
                output = $"unk1 {((Unk3fData)tfxData.data).unk1}, unk2 {((Unk3fData)tfxData.data).unk2}";
                break;
            case Unk42Data:
                output = $"unk1 {((Unk42Data)tfxData.data).unk1}, unk2 {((Unk42Data)tfxData.data).unk2}";
                break;
            case Unk43Data:
                output = $"unk1 {((Unk43Data)tfxData.data).unk1}";
                break;
            case StoreToBufferData:
                output = $"element {((StoreToBufferData)tfxData.data).element}";
                break;
            case Unk45Data:
                output = $"unk1 {((Unk45Data)tfxData.data).unk1}";
                break;
            case Unk46Data:
                output = $"unk1 {((Unk46Data)tfxData.data).unk1}";
                break;
            case Unk47Data:
                output = $"unk1 {((Unk47Data)tfxData.data).unk1}";
                break;
            case Unk48Data:
                output = $"unk1 {((Unk48Data)tfxData.data).unk1}";
                break;
            case Unk49Data:
                output = $"unk1 {((Unk49Data)tfxData.data).unk1}";
                break;
            case Unk4aData:
                output = $"unk1 {((Unk4aData)tfxData.data).unk1}";
                break;
            case Unk4cData:
                output = $"unk1 {((Unk4cData)tfxData.data).unk1}";
                break;
            case Unk4dData:
                output = $"unk1 {((Unk4dData)tfxData.data).unk1}";
                break;
            case Unk4eData:
                output = $"unk1 {((Unk4eData)tfxData.data).unk1}, unk2 {((Unk4eData)tfxData.data).unk2}";
                break;
            case Unk4fData:
                output = $"unk1 {((Unk4fData)tfxData.data).unk1}";
                break;
            case Unk52Data:
                output = $"unk1 {((Unk52Data)tfxData.data).unk1}, unk2 {((Unk52Data)tfxData.data).unk2}";
                break;
        }

        return output;
    }
}

public enum TfxBytecode : byte
{
    Unk01 = 0x01,
    Unk02 = 0x02,
    Unk03 = 0x03,
    Unk04 = 0x04,
    Unk07 = 0x07,
    Unk08 = 0x08,
    Unk09 = 0x09,
    Unk0b = 0x0b, // Variant with associated data
    Unk0c = 0x0c,
    Unk0d = 0x0d,
    Unk0e = 0x0e,
    Unk0f = 0x0f,
    Unk10 = 0x10,
    Unk11 = 0x11,
    Unk12 = 0x12,
    Unk13 = 0x13,
    Unk15 = 0x15,
    Unk16 = 0x16,
    Unk17 = 0x17,
    Unk1a = 0x1a,
    Unk1b = 0x1b,
    Unk1c = 0x1c,
    Unk1d = 0x1d,
    Unk1f = 0x1f,
    Unk20 = 0x20,
    Unk21 = 0x21,
    Unk22 = 0x22, // Variant with associated data
    Unk23 = 0x23,
    Unk25 = 0x25,
    Unk26 = 0x26,
    Unk27 = 0x27,
    Unk28 = 0x28,
    Unk29 = 0x29,
    Unk2a = 0x2a,
    Unk2e = 0x2e,
    UnkLoadConstant2 = 0x34, // Variant with associated data
    Unk35 = 0x35, // Variant with associated data
    Unk37 = 0x37, // Variant with associated data
    Unk38 = 0x38, // Variant with associated data
    Unk39 = 0x39, // Variant with associated data
    Unk3a = 0x3a, // Variant with associated data
    UnkLoadConstant = 0x3b, // Variant with associated data
    LoadExtern = 0x3c, // Variant with associated data
    Unk3d = 0x3d, // Variant with associated data
    Unk3e = 0x3e, // Variant with associated data
    Unk3f = 0x3f, // Variant with associated data
    Unk42 = 0x42, // Variant with associated data
    Unk43 = 0x43, // Variant with associated data
    StoreToBuffer = 0x44, // Variant with associated data
    Unk45 = 0x45, // Variant with associated data
    Unk46 = 0x46, // Variant with associated data
    Unk47 = 0x47, // Variant with associated data
    Unk48 = 0x48, // Variant with associated data
    Unk49 = 0x49, // Variant with associated data
    Unk4a = 0x4a, // Variant with associated data
    Unk4c = 0x4c, // Variant with associated data
    Unk4d = 0x4d, // Variant with associated data
    Unk4e = 0x4e, // Variant with associated data
    Unk4f = 0x4f, // Variant with associated data
    Unk52 = 0x52, // Variant with associated data
}

public struct TfxData
{
    public TfxBytecode op;
    public dynamic? data;
}

public struct Unk0bData
{
    public byte unk1;
    public byte unk2;
}

public struct Unk22Data
{
    public byte unk1;
}

public struct UnkLoadConstant2Data
{
    public byte constant_index;
}

public struct Unk35Data
{
    public byte unk1;
}

public struct Unk37Data
{
    public byte unk1;
}

public struct Unk38Data
{
    public byte unk1;
}

public struct Unk39Data
{
    public byte unk1;
}

public struct Unk3aData
{
    public byte unk1;
}

public struct UnkLoadConstantData
{
    public byte constant_index;
}

public struct LoadExternData
{
    public TfxExtern extern_;
    public byte element;
}

public struct Unk3dData
{
    public byte unk1;
    public byte unk2;
}

public struct Unk3eData
{
    public byte unk1;
}

public struct Unk3fData
{
    public byte unk1;
    public byte unk2;
}

public struct Unk42Data
{
    public byte unk1;
    public byte unk2;
}

public struct Unk43Data
{
    public byte unk1;
}

public struct StoreToBufferData
{
    public byte element;
}

public struct Unk45Data
{
    public byte unk1;
}

public struct Unk46Data
{
    public byte unk1;
}

public struct Unk47Data
{
    public byte unk1;
}

public struct Unk48Data
{
    public byte unk1;
}

public struct Unk49Data
{
    public byte unk1;
}

public struct Unk4aData
{
    public byte unk1;
}

public struct Unk4cData
{
    public byte unk1;
}

public struct Unk4dData
{
    public byte unk1;
}

public struct Unk4eData
{
    public byte unk1;
    public byte unk2;
}

public struct Unk4fData
{
    public byte unk1;
}

public struct Unk52Data
{
    public byte unk1;
    public byte unk2;
}

// Define associated data structs for other variants with associated data.

