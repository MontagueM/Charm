using Arithmic;
using Tiger;
using Tiger.Schema;

public class TfxBytecodeOp
{
    public static List<TfxData> ParseAll(DynamicArray<D2Class_09008080> bytecode)
    {
        byte[] data = new byte[bytecode.Count];
        for (int i = 0; i < bytecode.Count; i++)
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

        try
        {
            switch (tfxData.op)
            {
                case TfxBytecode.Permute:
                    PermuteData PermuteData = new();
                    PermuteData.fields = reader.ReadByte();
                    tfxData.data = PermuteData;
                    break;
                case TfxBytecode.PushConstantVec4:
                    PushConstantVec4Data PushConstantVec4Data = new();
                    PushConstantVec4Data.constant_index = reader.ReadByte();
                    tfxData.data = PushConstantVec4Data;
                    break;
                case TfxBytecode.LerpConstant:
                    LerpConstantData LerpConstantData = new();
                    LerpConstantData.constant_start = reader.ReadByte();
                    tfxData.data = LerpConstantData;
                    break;
                case TfxBytecode.Spline4Const:
                    Spline4ConstData Spline4ConstData = new();
                    Spline4ConstData.constant_index = reader.ReadByte();
                    tfxData.data = Spline4ConstData;
                    break;
                case TfxBytecode.Spline8Const:
                    Spline8ConstData Spline8ConstData = new();
                    Spline8ConstData.constant_index = reader.ReadByte();
                    tfxData.data = Spline8ConstData;
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
                case TfxBytecode.PushExternInputFloat:
                    PushExternInputFloatData PushExternInputFloatData = new();
                    PushExternInputFloatData.extern_ = (TfxExtern)reader.ReadByte();
                    PushExternInputFloatData.element = reader.ReadByte();
                    tfxData.data = PushExternInputFloatData;
                    break;
                case TfxBytecode.PushExternInputVec4:
                    PushExternInputVec4Data PushExternInputVec4Data = new();
                    PushExternInputVec4Data.extern_ = (TfxExtern)reader.ReadByte();
                    PushExternInputVec4Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputVec4Data;
                    break;
                case TfxBytecode.PushExternInputMat4:
                    PushExternInputMat4Data PushExternInputMat4Data = new();
                    PushExternInputMat4Data.extern_ = (TfxExtern)reader.ReadByte();
                    PushExternInputMat4Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputMat4Data;
                    break;
                case TfxBytecode.PushExternInputTextureView:
                    PushExternInputTextureViewData Unk3fData = new();
                    Unk3fData.extern_ = (TfxExtern)reader.ReadByte();
                    Unk3fData.element = reader.ReadByte();
                    tfxData.data = Unk3fData;
                    break;
                case TfxBytecode.PushExternInputU32:
                    PushExternInputU32Data PushExternInputU32Data = new();
                    PushExternInputU32Data.extern_ = (TfxExtern)reader.ReadByte();
                    PushExternInputU32Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputU32Data;
                    break;
                case TfxBytecode.PushExternInputU64Unknown:
                    PushExternInputU64UnknownData Unk41Data = new();
                    Unk41Data.extern_ = (TfxExtern)reader.ReadByte();
                    Unk41Data.element = reader.ReadByte();
                    tfxData.data = Unk41Data;
                    break;
                case TfxBytecode.PopOutput:
                    PopOutputData PopOutputData = new();
                    PopOutputData.slot = reader.ReadByte();
                    tfxData.data = PopOutputData;
                    break;
                case TfxBytecode.PushFromOutput:
                    PushFromOutputData Unk43Data = new();
                    Unk43Data.element = reader.ReadByte();
                    tfxData.data = Unk43Data;
                    break;
                case TfxBytecode.PopOutputMat4:
                    PopOutputMat4Data Unk45Data = new();
                    Unk45Data.slot = reader.ReadByte();
                    tfxData.data = Unk45Data;
                    break;
                case TfxBytecode.PushTemp:
                    PushTempData PushTempData = new();
                    PushTempData.slot = reader.ReadByte();
                    tfxData.data = PushTempData;
                    break;
                case TfxBytecode.PopTemp:
                    PopTempData PopTempData = new();
                    PopTempData.slot = reader.ReadByte();
                    tfxData.data = PopTempData;
                    break;
                case TfxBytecode.SetShaderTexture:
                    SetShaderTextureData Unk48Data = new();
                    Unk48Data.value = reader.ReadByte();
                    tfxData.data = Unk48Data;
                    break;
                case TfxBytecode.Unk49:
                    Unk49Data Unk49 = new();
                    Unk49.unk1 = reader.ReadByte();
                    tfxData.data = Unk49;
                    break;
                case TfxBytecode.SetShaderSampler:
                    SetShaderSamplerData Unk4aData = new();
                    Unk4aData.value = reader.ReadByte();
                    tfxData.data = Unk4aData;
                    break;
                case TfxBytecode.SetShaderUav:
                    SetShaderUavData Unk4bData = new();
                    Unk4bData.value = reader.ReadByte();
                    tfxData.data = Unk4bData;
                    break;
                case TfxBytecode.Unk4c:
                    Unk4cData Unk4cData = new();
                    Unk4cData.unk1 = reader.ReadByte();
                    tfxData.data = Unk4cData;
                    break;
                case TfxBytecode.PushSampler:
                    PushSamplerData PushSampler = new();
                    PushSampler.unk1 = reader.ReadByte();
                    tfxData.data = PushSampler;
                    break;
                case TfxBytecode.PushObjectChannelVector:
                    PushObjectChannelVectorData PushObjectChannelVector = new();
                    PushObjectChannelVector.unk1 = reader.ReadInt32();
                    tfxData.data = PushObjectChannelVector;
                    break;
                case TfxBytecode.PushGlobalChannelVector:
                    PushGlobalChannelVectorData PushGlobalChannelVector = new();
                    PushGlobalChannelVector.unk1 = reader.ReadByte();
                    tfxData.data = PushGlobalChannelVector;
                    break;
                case TfxBytecode.Unk50:
                    Unk50Data Unk50Data = new();
                    Unk50Data.unk1 = reader.ReadByte();
                    tfxData.data = Unk50Data;
                    break;
                case TfxBytecode.Unk52:
                    Unk52Data Unk52Data = new();
                    Unk52Data.unk1 = reader.ReadByte();
                    Unk52Data.unk2 = reader.ReadByte();
                    tfxData.data = Unk52Data;
                    break;
                case TfxBytecode.Unk53:
                    Unk53Data Unk53Data = new();
                    Unk53Data.unk1 = reader.ReadByte();
                    Unk53Data.unk2 = reader.ReadByte();
                    tfxData.data = Unk53Data;
                    break;
                case TfxBytecode.Unk54:
                    Unk54Data Unk54Data = new();
                    Unk54Data.unk1 = reader.ReadByte();
                    Unk54Data.unk2 = reader.ReadByte();
                    tfxData.data = Unk54Data;
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
        return tfxData;
    }

    public static string TfxToString(TfxData tfxData, DynamicArray<Vec4> constants)
    {
        string output = "";
        switch (tfxData.data)
        {
            case PermuteData:
                output = $"permute({DecodePermuteParam(((PermuteData)tfxData.data).fields)}), fields {((PermuteData)tfxData.data).fields}";
                break;
            case PushConstantVec4Data:
                output = $"constant_index {((PushConstantVec4Data)tfxData.data).constant_index}: Constant value: {constants[((PushConstantVec4Data)tfxData.data).constant_index].Vec.ToString()}";
                break;
            case LerpConstantData:
                output = $"constant_start {((LerpConstantData)tfxData.data).constant_start}: Constant 1: {constants[((LerpConstantData)tfxData.data).constant_start].Vec}: Constant 2: {constants[((LerpConstantData)tfxData.data).constant_start + 1].Vec}";
                break;
            case Spline4ConstData:
                output = $"unk1 {((Spline4ConstData)tfxData.data).constant_index}";
                break;
            case Spline8ConstData:
                output = $"unk1 {((Spline8ConstData)tfxData.data).constant_index}";
                break;
            case Unk39Data:
                output = $"unk1 {((Unk39Data)tfxData.data).unk1}";
                break;
            case Unk3aData:
                output = $"unk1 {((Unk3aData)tfxData.data).unk1}";
                break;
            case UnkLoadConstantData:
                output = $"constant_index {((UnkLoadConstantData)tfxData.data).constant_index}: Constant value: {constants[((UnkLoadConstantData)tfxData.data).constant_index].Vec}";
                break;
            case PushExternInputFloatData:
                output = $"extern {((PushExternInputFloatData)tfxData.data).extern_}, element {((PushExternInputFloatData)tfxData.data).element}";
                break;
            case PushExternInputVec4Data:
                output = $"extern {((PushExternInputVec4Data)tfxData.data).extern_}, element {((PushExternInputVec4Data)tfxData.data).element}";
                break;
            case PushExternInputMat4Data:
                output = $"extern {((PushExternInputMat4Data)tfxData.data).extern_}, element {((PushExternInputMat4Data)tfxData.data).element}";
                break;
            case PushExternInputTextureViewData:
                output = $"extern {((PushExternInputTextureViewData)tfxData.data).extern_}, element {((PushExternInputTextureViewData)tfxData.data).element}";
                break;
            case PushExternInputU32Data:
                output = $"extern {((PushExternInputU32Data)tfxData.data).extern_}, element {((PushExternInputU32Data)tfxData.data).element}";
                break;
            case PushExternInputU64UnknownData:
                output = $"extern {((PushExternInputU64UnknownData)tfxData.data).extern_}, element {((PushExternInputU64UnknownData)tfxData.data).element}";
                break;
            case PopOutputData:
                output = $"slot {((PopOutputData)tfxData.data).slot}";
                break;
            case PushFromOutputData:
                output = $"element {((PushFromOutputData)tfxData.data).element}";
                break;
            case StoreToBufferData:
                output = $"element {((StoreToBufferData)tfxData.data).element}";
                break;
            case PushTempData:
                output = $"unk1 {((PushTempData)tfxData.data).slot}";
                break;
            case PopTempData:
                output = $"unk1 {((PopTempData)tfxData.data).slot}";
                break;
            case Unk47Data:
                output = $"unk1 {((Unk47Data)tfxData.data).unk1}";
                break;
            case SetShaderTextureData:
                output = $"value {((SetShaderTextureData)tfxData.data).value}";
                break;
            case Unk49Data:
                output = $"unk1 {((Unk49Data)tfxData.data).unk1}";
                break;
            case SetShaderSamplerData:
                output = $"value {((SetShaderSamplerData)tfxData.data).value}";
                break;
            case SetShaderUavData:
                output = $"value {((SetShaderUavData)tfxData.data).value}";
                break;
            case Unk4cData:
                output = $"unk1 {((Unk4cData)tfxData.data).unk1}";
                break;
            case PushSamplerData:
                output = $"unk1 {((PushSamplerData)tfxData.data).unk1}";
                break;
            case PushObjectChannelVectorData:
                output = $"unk1 {((PushObjectChannelVectorData)tfxData.data).unk1}";
                break;
            case PushGlobalChannelVectorData:
                var index = ((PushGlobalChannelVectorData)tfxData.data).unk1;
                output = $"value {index} {GlobalChannelDefaults.GetGlobalChannelDefaults()[index]}";
                break;
            case Unk50Data:
                output = $"unk1 {((Unk50Data)tfxData.data).unk1}";
                break;
            case Unk52Data:
                output = $"unk1 {((Unk52Data)tfxData.data).unk1}";
                break;
            case Unk53Data:
                output = $"unk1 {((Unk53Data)tfxData.data).unk1}";
                break;
            case Unk54Data:
                output = $"unk1 {((Unk54Data)tfxData.data).unk1}";
                break;
        }

        return output;
    }

    public static string DecodePermuteParam(byte param)
    {
        char[] dims = { 'x', 'y', 'z', 'w' };
        int s0 = (param >> 6) & 0b11;
        int s1 = (param >> 4) & 0b11;
        int s2 = (param >> 2) & 0b11;
        int s3 = param & 0b11;

        return $".{dims[s0]}{dims[s1]}{dims[s2]}{dims[s3]}";
    }
}

public enum TfxBytecode : byte
{
    Add = 0x01,
    Subtract = 0x02,
    Multiply = 0x03,
    Divide = 0x04,
    Multiply2 = 0x05,
    Add2 = 0x06,
    IsZero = 0x07,
    Min = 0x08,
    Max = 0x09,
    LessThan = 0x0a,
    Dot = 0x0b,
    Merge_1_3 = 0x0c,
    Merge_2_2 = 0x0d,
    Merge_3_1 = 0x0e,
    Unk0f = 0x0f,
    Lerp = 0x10,
    Unk11 = 0x11,
    MultiplyAdd = 0x12,
    Clamp = 0x13,
    Abs = 0x15,
    Sign = 0x16,
    Floor = 0x17,
    Ceil = 0x18,
    Round = 0x19,
    Frac = 0x1a,
    Unk1b = 0x1b,
    Unk1c = 0x1c,
    Negate = 0x1d,
    VecRotSin = 0x1e,
    VecRotCos = 0x1f,
    VecRotSinCos = 0x20,
    PermuteAllX = 0x21,
    Permute = 0x22, //{ fields: u8 }
    Saturate = 0x23,
    Unk25 = 0x25,
    Unk26 = 0x26,
    Triangle = 0x27,
    Jitter = 0x28,
    Wander = 0x29,
    Rand = 0x2a,
    RandSmooth = 0x2b,
    Unk2c = 0x2c,
    Unk2d = 0x2d,
    TransformVec4 = 0x2e,
    PushConstantVec4 = 0x34, //{ constant_index: u8 }
    LerpConstant = 0x35, //{ unk1: u8 }
    Spline4Const = 0x37, //{ unk1: u8 }
    Spline8Const = 0x38, //{ unk1: u8 }
    Unk39 = 0x39, //{ unk1: u8 }
    Unk3a = 0x3a, //{ unk1: u8 }
    UnkLoadConstant = 0x3b, //{ constant_index: u8 }
    PushExternInputFloat = 0x3c, //{ extern_: TfxExtern, element: u8 }
    PushExternInputVec4 = 0x3d, //{ extern_: TfxExtern, unk2: u8 }
    PushExternInputMat4 = 0x3e, //{ extern_: TfxExtern, unk2: u8 }
    PushExternInputTextureView = 0x3f, //{ extern_: TfxExtern, unk2: u8 }
    PushExternInputU32 = 0x40, //{ extern_: TfxExtern, unk2: u8 }
    PushExternInputU64Unknown = 0x41, //{ extern_: TfxExtern, unk2: u8 }
    Unk42 = 0x42,
    PushFromOutput = 0x43, //{ unk1: u8 }
    PopOutput = 0x44, //{ element: u8 }
    PopOutputMat4 = 0x45, //{ slot: u8 }
    PushTemp = 0x46, //{ slot: u8 }
    PopTemp = 0x47, //{ unk1: u8 }
    SetShaderTexture = 0x48, //{ unk1: u8 }
    Unk49 = 0x49, //{ unk1: u8 }
    SetShaderSampler = 0x4a, //{ unk1: u8 }
    SetShaderUav = 0x4b, //{ unk1: u8 }
    Unk4c = 0x4c, //{ unk1: u8 }
    PushSampler = 0x4d, //{ unk1: u8 }
    PushObjectChannelVector = 0x4e, //{ unk1: u8, unk2: u8, unk3: u8, unk4: u8 }
    PushGlobalChannelVector = 0x4f, //{ unk1: u8 }
    Unk50 = 0x50, //{ unk1: u8 }
    Unk51 = 0x51,
    Unk52 = 0x52, //{ unk1: u8, unk2: u8 }
    Unk53 = 0x53, //{ unk1: u8, unk2: u8 }
    Unk54 = 0x54, //{ unk1: u8, unk2: u8 }
    Unk55 = 0x55,
    Unk56 = 0x56,
    Unk57 = 0x57,
    Unk58 = 0x58,
}

public struct TfxData
{
    public TfxBytecode op;
    public dynamic? data;
}

public struct PermuteData
{
    public byte fields;
}

public struct PushConstantVec4Data
{
    public byte constant_index;
}

public struct LerpConstantData
{
    public byte constant_start;
}

public struct Spline4ConstData
{
    public byte constant_index;
}

public struct Spline8ConstData
{
    public byte constant_index;
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

public struct PushExternInputFloatData
{
    public TfxExtern extern_;
    public byte element;
}

public struct PushExternInputVec4Data
{
    public TfxExtern extern_;
    public byte element;
}

public struct PushExternInputMat4Data
{
    public TfxExtern extern_;
    public byte element;
}

public struct PushExternInputTextureViewData
{
    public TfxExtern extern_;
    public byte element;
}

public struct PushExternInputU32Data
{
    public TfxExtern extern_;
    public byte element;
}

public struct PushExternInputU64UnknownData
{
    public TfxExtern extern_;
    public byte element;
}

public struct PopOutputData
{
    public byte slot;
}

public struct StoreToBufferData
{
    public byte element;
}

public struct PushFromOutputData
{
    public byte element;
}

public struct PopOutputMat4Data
{
    public byte slot;
}

public struct PushTempData
{
    public byte slot;
}

public struct PopTempData
{
    public byte slot;
}

public struct Unk47Data
{
    public byte unk1;
}

public struct SetShaderTextureData
{
    public byte value;
}

public struct Unk49Data
{
    public byte unk1;
}

public struct SetShaderSamplerData
{
    public byte value;
}

public struct SetShaderUavData
{
    public byte value;
}

public struct Unk4cData
{
    public byte unk1;
}

public struct PushSamplerData
{
    public byte unk1;
}

public struct PushObjectChannelVectorData
{
    public int unk1;
}

public struct PushGlobalChannelVectorData
{
    public byte unk1;
}

public struct Unk50Data
{
    public byte unk1;
}

public struct Unk52Data
{
    public byte unk1;
    public byte unk2;
}

public struct Unk53Data
{
    public byte unk1;
    public byte unk2;
}

public struct Unk54Data
{
    public byte unk1;
    public byte unk2;
}

