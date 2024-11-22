using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

public static class TfxBytecodeOp
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
        var _strat = Strategy.CurrentStrategy;
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
                case TfxBytecode.Gradient4Const: // Gradient4Const
                    Gradient4ConstData Unk3aData = new();
                    Unk3aData.index_start = reader.ReadByte();
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
                case TfxBytecode.PushExternInputUav when !Strategy.IsD1():
                    PushExternInputUavData Unk41Data = new();
                    Unk41Data.extern_ = (TfxExtern)reader.ReadByte();
                    Unk41Data.element = reader.ReadByte();
                    tfxData.data = Unk41Data;
                    break;

                // From here forward, SK and BL is op-1, D1 is all over the place....so its gonna get ugly

                case TfxBytecode.PushFromOutput - 2 when Strategy.IsD1():
                case TfxBytecode.PushFromOutput - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushFromOutput when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushFromOutput;

                    PushFromOutputData Unk43Data = new();
                    Unk43Data.element = reader.ReadByte();
                    tfxData.data = Unk43Data;
                    break;
                case TfxBytecode.PopOutput - 2 when Strategy.IsD1():
                case TfxBytecode.PopOutput - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PopOutput when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PopOutput;

                    PopOutputData PopOutputData = new();
                    PopOutputData.slot = reader.ReadByte();
                    tfxData.data = PopOutputData;
                    break;
                case TfxBytecode.PopOutputMat4 - 2 when Strategy.IsD1():
                case TfxBytecode.PopOutputMat4 - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PopOutputMat4 when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PopOutputMat4;

                    PopOutputMat4Data Unk45Data = new();
                    Unk45Data.slot = reader.ReadByte();
                    tfxData.data = Unk45Data;
                    break;

                case TfxBytecode.PushTemp - 2 when Strategy.IsD1():
                case TfxBytecode.PushTemp - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushTemp when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushTemp;

                    PushTempData PushTempData = new();
                    PushTempData.slot = reader.ReadByte();
                    tfxData.data = PushTempData;
                    break;

                case TfxBytecode.PopTemp - 2 when Strategy.IsD1():
                case TfxBytecode.PopTemp - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PopTemp when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PopTemp;

                    PopTempData PopTempData = new();
                    PopTempData.slot = reader.ReadByte();
                    tfxData.data = PopTempData;
                    break;

                case TfxBytecode.SetShaderTexture - 2 when Strategy.IsD1():
                case TfxBytecode.SetShaderTexture - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.SetShaderTexture when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.SetShaderTexture;

                    SetShaderTextureData Unk48Data = new();
                    Unk48Data.value = reader.ReadByte();
                    tfxData.data = Unk48Data;
                    break;

                //case TfxBytecode.Unk49 - 2 when Strategy.IsD1():
                case TfxBytecode.Unk49 - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.Unk49 when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.Unk49;

                    Unk49Data Unk49 = new();
                    Unk49.unk1 = reader.ReadByte();
                    tfxData.data = Unk49;
                    break;

                case TfxBytecode.SetShaderSampler - 3 when Strategy.IsD1(): // 0x47
                case TfxBytecode.SetShaderSampler - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.SetShaderSampler when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.SetShaderSampler;

                    SetShaderSamplerData Unk4aData = new();
                    Unk4aData.value = reader.ReadByte();
                    tfxData.data = Unk4aData;
                    break;

                //case TfxBytecode.SetShaderUav - 2 when Strategy.IsD1():
                case TfxBytecode.SetShaderUav - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.SetShaderUav when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.SetShaderUav;

                    SetShaderUavData Unk4bData = new();
                    Unk4bData.value = reader.ReadByte();
                    tfxData.data = Unk4bData;
                    break;

                //case TfxBytecode.Unk4c - 2 when Strategy.IsD1():
                case TfxBytecode.Unk4c - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.Unk4c when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.Unk4c;

                    Unk4cData Unk4cData = new();
                    Unk4cData.unk1 = reader.ReadByte();
                    tfxData.data = Unk4cData;
                    break;

                case TfxBytecode.PushSampler - 4 when Strategy.IsD1(): // 0x49
                case TfxBytecode.PushSampler - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushSampler when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushSampler;

                    PushSamplerData PushSampler = new();
                    PushSampler.unk1 = reader.ReadByte();
                    tfxData.data = PushSampler;
                    break;

                case TfxBytecode.PushObjectChannelVector - 4 when Strategy.IsD1(): // I think...?? Uses an index instead of hash?
                case TfxBytecode.PushObjectChannelVector - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushObjectChannelVector when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushObjectChannelVector;

                    PushObjectChannelVectorData PushObjectChannelVector = new();
                    PushObjectChannelVector.hash = Strategy.IsD1() ? reader.ReadByte() : reader.ReadUInt32();
                    tfxData.data = PushObjectChannelVector;
                    break;

                case TfxBytecode.PushGlobalChannelVector - 4 when Strategy.IsD1():
                case TfxBytecode.PushGlobalChannelVector - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushGlobalChannelVector when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushGlobalChannelVector;

                    PushGlobalChannelVectorData PushGlobalChannelVector = new();
                    PushGlobalChannelVector.unk1 = reader.ReadByte();
                    tfxData.data = PushGlobalChannelVector;
                    break;

                case TfxBytecode.Unk50 - 2 when Strategy.IsD1():
                case TfxBytecode.Unk50 - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.Unk50 when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.Unk50;

                    Unk50Data Unk50Data = new();
                    Unk50Data.unk1 = reader.ReadByte();
                    tfxData.data = Unk50Data;
                    break;

                case TfxBytecode.PushTexDimensions - 2 when Strategy.IsD1():
                case TfxBytecode.PushTexDimensions - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushTexDimensions when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushTexDimensions;

                    PushTexDimensionsData Unk52Data = new();
                    Unk52Data.index = reader.ReadByte();
                    Unk52Data.fields = reader.ReadByte();
                    tfxData.data = Unk52Data;
                    break;

                case TfxBytecode.PushTexTileParams - 2 when Strategy.IsD1():
                case TfxBytecode.PushTexTileParams - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushTexTileParams when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushTexTileParams;

                    PushTexTileParamsData Unk53Data = new();
                    Unk53Data.index = reader.ReadByte();
                    Unk53Data.fields = reader.ReadByte();
                    tfxData.data = Unk53Data;
                    break;

                case TfxBytecode.PushTexTileCount - 2 when Strategy.IsD1():
                case TfxBytecode.PushTexTileCount - 1 when Strategy.IsPreBL() || Strategy.IsBL():
                case TfxBytecode.PushTexTileCount when Strategy.IsPostBL():
                    tfxData.op = TfxBytecode.PushTexTileCount;

                    PushTexTileCountData Unk54Data = new();
                    Unk54Data.index = reader.ReadByte();
                    Unk54Data.fields = reader.ReadByte();
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

    public static string TfxToString(TfxData tfxData, DynamicArray<Vec4> constants, IMaterial? material = null)
    {
        string output = "";
        byte index = 0;
        switch (tfxData.data)
        {
            case PermuteData:
                output = $"{DecodePermuteParam(((PermuteData)tfxData.data).fields).ToUpper()}";
                break;
            case PushConstantVec4Data:
                output = $"{constants[((PushConstantVec4Data)tfxData.data).constant_index].Vec.ToString()}";
                break;
            case LerpConstantData:
                output = $"A: {constants[((LerpConstantData)tfxData.data).constant_start].Vec}: B: {constants[((LerpConstantData)tfxData.data).constant_start + 1].Vec}";
                break;
            case Spline4ConstData:
                index = ((Spline4ConstData)tfxData.data).constant_index;
                var C3 = $"{constants[index].Vec}";
                var C2 = $"{constants[index + 1].Vec}";
                var C1 = $"{constants[index + 2].Vec}";
                var C0 = $"{constants[index + 3].Vec}";
                var threshold = $"{constants[index + 4].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {C3}" +
                    $"\n\tC2: {C2}" +
                    $"\n\tC1: {C1}" +
                    $"\n\tC0: {C0}" +
                    $"\n\tThreshold: {threshold}";
                break;

            case Spline8ConstData:
                index = ((Spline8ConstData)tfxData.data).constant_index;
                var s8_C3 = $"{constants[index].Vec}";
                var s8_C2 = $"{constants[index + 1].Vec}";
                var s8_C1 = $"{constants[index + 2].Vec}";
                var s8_C0 = $"{constants[index + 3].Vec}";
                var s8_D3 = $"{constants[index + 4].Vec}";
                var s8_D2 = $"{constants[index + 5].Vec}";
                var s8_D1 = $"{constants[index + 6].Vec}";
                var s8_D0 = $"{constants[index + 7].Vec}";
                var C_thresholds = $"{constants[index + 8].Vec}";
                var D_thresholds = $"{constants[index + 9].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {s8_C3}" +
                    $"\n\tC2: {s8_C2}" +
                    $"\n\tC1: {s8_C1}" +
                    $"\n\tC0: {s8_C0}" +
                    $"\n\tD3: {s8_D3}" +
                    $"\n\tD2: {s8_D2}" +
                    $"\n\tD1: {s8_D1}" +
                    $"\n\tD0: {s8_D0}" +
                    $"\n\tC_thresholds: {C_thresholds}" +
                    $"\n\tD_thresholds: {D_thresholds}";
                break;

            case Unk39Data:
                output = $"unk1 {((Unk39Data)tfxData.data).unk1}";
                break;
            case Gradient4ConstData: // Gradient4Const
                index = ((Gradient4ConstData)tfxData.data).index_start;
                var BaseColor = $"{constants[index].Vec}";
                var Cred = $"{constants[index + 1].Vec}";
                var Cgreen = $"{constants[index + 2].Vec}";
                var Cblue = $"{constants[index + 3].Vec}";
                var Calpha = $"{constants[index + 4].Vec}";
                var Cthresholds = $"{constants[index + 5].Vec}";

                output = $"Index {index}:" +
                    $"\n\tBaseColor: {BaseColor}" +
                    $"\n\tCred: {Cred}" +
                    $"\n\tCgreen: {Cgreen}" +
                    $"\n\tCblue: {Cblue}" +
                    $"\n\tCalpha: {Calpha}" +
                    $"\n\tCthresholds: {Cthresholds}";
                break;

            case UnkLoadConstantData:
                output = $"constant_index {((UnkLoadConstantData)tfxData.data).constant_index}: Constant value: {constants[((UnkLoadConstantData)tfxData.data).constant_index].Vec}";
                break;

            case PushExternInputFloatData:
                var pFloat = ((PushExternInputFloatData)tfxData.data).element;
                output = $"extern {((PushExternInputFloatData)tfxData.data).extern_}, element {pFloat} (0x{(pFloat * 4):X})";
                break;
            case PushExternInputVec4Data:
                var pVec = ((PushExternInputVec4Data)tfxData.data).element;
                output = $"extern {((PushExternInputVec4Data)tfxData.data).extern_}, element {pVec} (0x{(pVec * 16):X})";
                break;
            case PushExternInputMat4Data:
                var pMat = ((PushExternInputMat4Data)tfxData.data).element;
                output = $"extern {((PushExternInputMat4Data)tfxData.data).extern_}, element {pMat} (0x{(pMat * 16):X})";
                break;
            case PushExternInputTextureViewData:
                var pTex = ((PushExternInputTextureViewData)tfxData.data).element;
                output = $"extern {((PushExternInputTextureViewData)tfxData.data).extern_}, element {pTex} (0x{(pTex * 8):X})";
                break;
            case PushExternInputU32Data:
                var pU32 = ((PushExternInputU32Data)tfxData.data).element;
                output = $"extern {((PushExternInputU32Data)tfxData.data).extern_}, element {pU32} (0x{(pU32 * 4):X})";
                break;
            case PushExternInputUavData:
                var pUav = ((PushExternInputUavData)tfxData.data).element;
                output = $"extern {((PushExternInputUavData)tfxData.data).extern_}, element {pUav} (0x{(pUav * 8):X})";
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
                output = $"index {((PushTempData)tfxData.data).slot}";
                break;
            case PopTempData:
                output = $"index {((PopTempData)tfxData.data).slot}";
                break;
            case Unk47Data:
                output = $"unk1 {((Unk47Data)tfxData.data).unk1}";
                break;
            case SetShaderTextureData:
                var texSlot = ((SetShaderTextureData)tfxData.data).value;
                output = $"Texture Slot {texSlot & 0x1F}";
                break;
            case Unk49Data:
                output = $"unk1 {((Unk49Data)tfxData.data).unk1}";
                break;
            case SetShaderSamplerData:
                var sampSlot = ((SetShaderSamplerData)tfxData.data).value;
                output = $"Sampler Slot {sampSlot & 0x1F}";
                break;
            case SetShaderUavData:
                output = $"value {((SetShaderUavData)tfxData.data).value}";
                break;
            case Unk4cData:
                output = $"unk1 {((Unk4cData)tfxData.data).unk1}";
                break;
            case PushSamplerData:
                output = $"index {((PushSamplerData)tfxData.data).unk1}";
                break;
            case PushObjectChannelVectorData:
                output = $"hash {((PushObjectChannelVectorData)tfxData.data).hash:X}";
                break;
            case PushGlobalChannelVectorData:
                index = ((PushGlobalChannelVectorData)tfxData.data).unk1;
                output = $"index {index} {GlobalChannelDefaults.GetGlobalChannelDefaults()[index]}";
                break;
            case Unk50Data:
                output = $"unk1 {((Unk50Data)tfxData.data).unk1}";
                break;

            case PushTexDimensionsData:
                var ptd = ((PushTexDimensionsData)tfxData.data);
                Texture tex = FileResourcer.Get().GetFile<Texture>(material.PS_Samplers[ptd.index].Hash);

                output = $"{DecodePermuteParam(ptd.fields).ToUpper()}: " +
                    $"({tex.TagData.Width}, {tex.TagData.Height}, {tex.TagData.Depth}, {tex.TagData.ArraySize})";
                break;

            case PushTexTileParamsData:
                var ptt = ((PushTexTileParamsData)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PS_Samplers[ptt.index].Hash);

                output = $"{DecodePermuteParam(ptt.fields).ToUpper()}: " +
                    $"{tex.TagData.TilingScaleOffset}";
                break;

            case PushTexTileCountData:
                var pttc = ((PushTexTileCountData)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PS_Samplers[pttc.index].Hash);

                output = $"{DecodePermuteParam(pttc.fields).ToUpper()}: " +
                    $"({tex.TagData.TileCount}, {tex.TagData.ArraySize}, 0, 0)"; break;
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
    Cubic = 0x0f,
    Lerp = 0x10,
    LerpSaturated = 0x11,
    MultiplyAdd = 0x12,
    Clamp = 0x13,
    Abs = 0x15,
    Sign = 0x16,
    Floor = 0x17,
    Ceil = 0x18,
    Round = 0x19,
    Frac = 0x1a,
    Unk1b = 0x1b, // Normalize()?
    Unk1c = 0x1c, // Maybe also normalize, but slightly different?
    Negate = 0x1d,
    VecRotSin = 0x1e,
    VecRotCos = 0x1f,
    VecRotSinCos = 0x20,
    PermuteAllX = 0x21,
    Permute = 0x22,
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
    PushConstantVec4 = 0x34,
    LerpConstant = 0x35,
    LerpConstantSaturated = 0x36,
    Spline4Const = 0x37,
    Spline8Const = 0x38,
    Unk39 = 0x39, // Spline8ConstChain?
    Gradient4Const = 0x3a,
    UnkLoadConstant = 0x3b, //{ constant_index: u8 }
    PushExternInputFloat = 0x3c,
    PushExternInputVec4 = 0x3d,
    PushExternInputMat4 = 0x3e,
    PushExternInputTextureView = 0x3f,
    PushExternInputU32 = 0x40,
    PushExternInputUav = 0x41,

    Unk42 = 0x42, // Not in Pre-BL, everything further down is shifted - 1
    PushFromOutput = 0x43,
    PopOutput = 0x44,
    PopOutputMat4 = 0x45,
    PushTemp = 0x46,
    PopTemp = 0x47,
    SetShaderTexture = 0x48,
    Unk49 = 0x49, //{ unk1: u8 }
    SetShaderSampler = 0x4a,
    SetShaderUav = 0x4b,
    Unk4c = 0x4c, //{ unk1: u8 }
    PushSampler = 0x4d,
    PushObjectChannelVector = 0x4e,
    PushGlobalChannelVector = 0x4f,
    Unk50 = 0x50, //{ unk1: u8 }
    Unk51 = 0x51,
    PushTexDimensions = 0x52, //{ unk1: u8, unk2: u8 }
    PushTexTileParams = 0x53, //{ unk1: u8, unk2: u8 }
    PushTexTileCount = 0x54, //{ unk1: u8, unk2: u8 }
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

public struct Gradient4ConstData
{
    public byte index_start;
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

public struct PushExternInputUavData
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
    public uint hash;
}

public struct PushGlobalChannelVectorData
{
    public byte unk1;
}

public struct Unk50Data
{
    public byte unk1;
}

public struct PushTexDimensionsData // Width, Height, Depth, Mip count
{
    public byte index; // Index in the Samplers array
    public byte fields; // Swizzle
}

public struct PushTexTileParamsData // Vec4 at 0x10 in texture header
{
    public byte index;
    public byte fields;
}

public struct PushTexTileCountData // 0x28 (Array Size), 0x2A (Tile Count) in texture header
{
    public byte index;
    public byte fields;
}

