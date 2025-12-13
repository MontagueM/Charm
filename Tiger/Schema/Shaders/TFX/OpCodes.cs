using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

public static class TfxBytecodeOp
{
    public enum BytecodeType
    {
        Expression = 0,
        Sequencer = 1,
        Particle = 2
    }

    public static List<TfxData> ParseAll(DynamicArray<SUInt8> bytecode, BytecodeType type = BytecodeType.Expression)
    {
        byte[] data = new byte[bytecode.Count];
        for (int i = 0; i < bytecode.Count; i++)
        {
            data[i] = bytecode[i].Value;
        }

        List<TfxData> opcodes = new();
        using (MemoryStream stream = new(data))
        {
            using (BinaryReader reader = new(stream))
            {
                while (stream.Position < data.Length)
                {
                    TfxData op = ReadTfxBytecodeOp(reader, type);
                    opcodes.Add(op);
                }
            }
        }

        return opcodes;
    }

    public static TfxBytecode RemapOp(byte value)
    {
        string name =
            Strategy.IsLatest() ? ((TfxBytecode_EoF)value).ToString() :

            Strategy.IsD1() ? ((TfxBytecode_D1)value).ToString() :
            Strategy.IsPreBL() || Strategy.IsBL() ? ((TfxBytecode_BL)value).ToString() :
            Strategy.IsPostBL() ? ((TfxBytecode_TFS)value).ToString() :
            ((TfxBytecode)value).ToString();

        if (Enum.TryParse(name, out TfxBytecode result))
            return result;

        throw new InvalidCastException($"Couldn't cast TfxBytecode value {value} ({name}) for {Strategy.CurrentStrategy}");
    }

    public static TfxData ReadTfxBytecodeOp(BinaryReader reader, BytecodeType type)
    {
        TfxData tfxData = new()
        {
            op = RemapOp(reader.ReadByte()),
            data = null
        };
        if (type == BytecodeType.Sequencer && tfxData.op == TfxBytecode.PushExternInputMat4)
            tfxData.op = TfxBytecode.PopOutput;

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

                case TfxBytecode.LerpConstantSaturated:
                    LerpConstantData LerpConstantSatData = new();
                    LerpConstantSatData.constant_start = reader.ReadByte();
                    tfxData.data = LerpConstantSatData;
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

                case TfxBytecode.Spline8ConstChain:
                    Spline8ConstChainData Unk39Data = new();
                    Unk39Data.constant_index = reader.ReadByte();
                    tfxData.data = Unk39Data;
                    break;

                case TfxBytecode.Gradient4Const:
                    Gradient4ConstData Unk3aData = new();
                    Unk3aData.constant_index = reader.ReadByte();
                    tfxData.data = Unk3aData;
                    break;

                case TfxBytecode.Gradient8Const:
                    Gradient8ConstData Gradient8ConstData = new();
                    Gradient8ConstData.constant_index = reader.ReadByte();
                    tfxData.data = Gradient8ConstData;
                    break;

                case TfxBytecode.PushExternInputFloat:
                    PushExternInputFloatData PushExternInputFloatData = new();
                    if (type != BytecodeType.Sequencer)
                        PushExternInputFloatData.extern_ = Externs.GetExtern(reader.ReadByte());

                    PushExternInputFloatData.element = reader.ReadByte();
                    tfxData.data = PushExternInputFloatData;
                    break;

                case TfxBytecode.PushExternInputVec4:
                    PushExternInputVec4Data PushExternInputVec4Data = new();
                    PushExternInputVec4Data.extern_ = Externs.GetExtern(reader.ReadByte());
                    PushExternInputVec4Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputVec4Data;
                    break;

                case TfxBytecode.PushExternInputMat4:
                    PushExternInputMat4Data PushExternInputMat4Data = new();
                    PushExternInputMat4Data.extern_ = Externs.GetExtern(reader.ReadByte());
                    PushExternInputMat4Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputMat4Data;
                    break;

                case TfxBytecode.PushExternInputTextureView:
                    PushExternInputTextureViewData Unk3fData = new();
                    Unk3fData.extern_ = Externs.GetExtern(reader.ReadByte());
                    Unk3fData.element = reader.ReadByte();
                    tfxData.data = Unk3fData;
                    break;

                case TfxBytecode.PushExternInputU32:
                    PushExternInputU32Data PushExternInputU32Data = new();
                    PushExternInputU32Data.extern_ = Externs.GetExtern(reader.ReadByte());
                    PushExternInputU32Data.element = reader.ReadByte();
                    tfxData.data = PushExternInputU32Data;
                    break;

                case TfxBytecode.PushExternInputUav:
                    PushExternInputUavData Unk41Data = new();
                    Unk41Data.extern_ = Externs.GetExtern(reader.ReadByte());
                    Unk41Data.element = reader.ReadByte();
                    tfxData.data = Unk41Data;
                    break;

                case TfxBytecode.PushFromOutput:
                    PushFromOutputData Unk43Data = new();
                    Unk43Data.element = reader.ReadByte();
                    tfxData.data = Unk43Data;
                    break;

                case TfxBytecode.PopOutput:
                    PopOutputData PopOutputData = new();
                    PopOutputData.slot = reader.ReadByte();
                    tfxData.data = PopOutputData;
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
                    PushObjectChannelVector.hash = Strategy.IsD1() ? reader.ReadByte() : Endian.SwapU32(reader.ReadUInt32());
                    tfxData.data = PushObjectChannelVector;
                    break;

                case TfxBytecode.PushGlobalChannelVector:
                    PushGlobalChannelVectorData PushGlobalChannelVector = new();
                    PushGlobalChannelVector.Index = reader.ReadByte();
                    tfxData.data = PushGlobalChannelVector;
                    break;

                case TfxBytecode.Unk50:
                    Unk50Data Unk50Data = new();
                    Unk50Data.unk1 = reader.ReadByte();
                    tfxData.data = Unk50Data;
                    break;

                case TfxBytecode.PushTexDimensions:
                    PushTexDimensionsData Unk52Data = new();
                    Unk52Data.index = reader.ReadByte();
                    Unk52Data.fields = reader.ReadByte();
                    tfxData.data = Unk52Data;
                    break;

                case TfxBytecode.PushTexTileParams:
                    PushTexTileParamsData Unk53Data = new();
                    Unk53Data.index = reader.ReadByte();
                    Unk53Data.fields = reader.ReadByte();
                    tfxData.data = Unk53Data;
                    break;

                case TfxBytecode.PushTexTileCount:
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

    public static string TfxToString(TfxData tfxData, DynamicArray<Vec4> constants, Material? material = null)
    {
        string output = "";
        byte index = 0;
        switch (tfxData.data)
        {
            case PermuteData:
                output = $"{DecodePermuteParam(((PermuteData)tfxData.data).fields).ToUpper()}";
                break;
            case PushConstantVec4Data:
                output = $"{constants[((PushConstantVec4Data)tfxData.data).constant_index].Vec.ToString().Replace("Infinity", "1.#INF")}";
                break;
            case LerpConstantData:
                output = $"From: {constants[((LerpConstantData)tfxData.data).constant_start].Vec}: To: {constants[((LerpConstantData)tfxData.data).constant_start + 1].Vec}";
                break;
            case Spline4ConstData:
                index = ((Spline4ConstData)tfxData.data).constant_index;
                string C3 = $"{constants[index].Vec}";
                string C2 = $"{constants[index + 1].Vec}";
                string C1 = $"{constants[index + 2].Vec}";
                string C0 = $"{constants[index + 3].Vec}";
                string threshold = $"{constants[index + 4].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {C3}" +
                    $"\n\tC2: {C2}" +
                    $"\n\tC1: {C1}" +
                    $"\n\tC0: {C0}" +
                    $"\n\tThreshold: {threshold}";
                break;

            case Spline8ConstData:
                index = ((Spline8ConstData)tfxData.data).constant_index;
                string s8_C3 = $"{constants[index].Vec}";
                string s8_C2 = $"{constants[index + 1].Vec}";
                string s8_C1 = $"{constants[index + 2].Vec}";
                string s8_C0 = $"{constants[index + 3].Vec}";
                string s8_D3 = $"{constants[index + 4].Vec}";
                string s8_D2 = $"{constants[index + 5].Vec}";
                string s8_D1 = $"{constants[index + 6].Vec}";
                string s8_D0 = $"{constants[index + 7].Vec}";
                string C_thresholds = $"{constants[index + 8].Vec}";
                string D_thresholds = $"{constants[index + 9].Vec}";

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

            case Spline8ConstChainData:
                index = ((Spline8ConstChainData)tfxData.data).constant_index;
                string s8c_C3 = $"{constants[index].Vec}";
                string s8c_C2 = $"{constants[index + 1].Vec}";
                string s8c_C1 = $"{constants[index + 2].Vec}";
                string s8c_C0 = $"{constants[index + 3].Vec}";
                string s8c_D3 = $"{constants[index + 4].Vec}";
                string s8c_D2 = $"{constants[index + 5].Vec}";
                string s8c_D1 = $"{constants[index + 6].Vec}";
                string s8c_D0 = $"{constants[index + 7].Vec}";
                string C1_thresholds = $"{constants[index + 8].Vec}";
                string D1_thresholds = $"{constants[index + 9].Vec}";

                output = $"Index {index}:" +
                    $"\n\tC3: {s8c_C3}" +
                    $"\n\tC2: {s8c_C2}" +
                    $"\n\tC1: {s8c_C1}" +
                    $"\n\tC0: {s8c_C0}" +
                    $"\n\tD3: {s8c_D3}" +
                    $"\n\tD2: {s8c_D2}" +
                    $"\n\tD1: {s8c_D1}" +
                    $"\n\tD0: {s8c_D0}" +
                    $"\n\tC_thresholds: {C1_thresholds}" +
                    $"\n\tD_thresholds: {D1_thresholds}";
                break;

            case Gradient4ConstData: // Gradient4Const
                index = ((Gradient4ConstData)tfxData.data).constant_index;
                string BaseColor = $"{constants[index].Vec}";
                string Cred = $"{constants[index + 1].Vec}";
                string Cgreen = $"{constants[index + 2].Vec}";
                string Cblue = $"{constants[index + 3].Vec}";
                string Calpha = $"{constants[index + 4].Vec}";
                string Cthresholds = $"{constants[index + 5].Vec}";

                output = $"Index {index}:" +
                    $"\n\tBaseColor: {BaseColor}" +
                    $"\n\tCred: {Cred}" +
                    $"\n\tCgreen: {Cgreen}" +
                    $"\n\tCblue: {Cblue}" +
                    $"\n\tCalpha: {Calpha}" +
                    $"\n\tCthresholds: {Cthresholds}";
                break;

            case Gradient8ConstData:
                index = ((Gradient8ConstData)tfxData.data).constant_index;
                BaseColor = $"{constants[index].Vec}";
                Cred = $"{constants[index + 1].Vec}";
                Cgreen = $"{constants[index + 2].Vec}";
                Cblue = $"{constants[index + 3].Vec}";
                Calpha = $"{constants[index + 4].Vec}";
                string Dred = $"{constants[index + 5].Vec}";
                string Dgreen = $"{constants[index + 6].Vec}";
                string Dblue = $"{constants[index + 7].Vec}";
                string Dalpha = $"{constants[index + 8].Vec}";
                Cthresholds = $"{constants[index + 9].Vec}";
                string Dthresholds = $"{constants[index + 10].Vec}";

                output = $"Index {index}:" +
                    $"\n\tBaseColor: {BaseColor}" +
                    $"\n\tCred: {Cred}" +
                    $"\n\tCgreen: {Cgreen}" +
                    $"\n\tCblue: {Cblue}" +
                    $"\n\tCalpha: {Calpha}" +
                    $"\n\tDred: {Dred}" +
                    $"\n\tDgreen: {Dgreen}" +
                    $"\n\tDblue: {Dblue}" +
                    $"\n\tDalpha: {Dalpha}" +
                    $"\n\tCthresholds: {Cthresholds}" +
                    $"\n\tDthresholds: {Dthresholds}";
                break;

            case PushExternInputFloatData:
                byte pFloat = ((PushExternInputFloatData)tfxData.data).element;
                var _extern = ((PushExternInputFloatData)tfxData.data).extern_;
                output = $"extern {_extern}, element {pFloat} (0x{(pFloat * 4):X})";
                if (_extern == TfxExtern.Frame && pFloat == 0)
                    output += " (Time)";
                break;
            case PushExternInputVec4Data:
                byte pVec = ((PushExternInputVec4Data)tfxData.data).element;
                output = $"extern {((PushExternInputVec4Data)tfxData.data).extern_}, element {pVec} (0x{(pVec * 16):X})";
                break;
            case PushExternInputMat4Data:
                byte pMat = ((PushExternInputMat4Data)tfxData.data).element;
                output = $"extern {((PushExternInputMat4Data)tfxData.data).extern_}, element {pMat} (0x{(pMat * 16):X})";
                break;
            case PushExternInputTextureViewData:
                byte pTex = ((PushExternInputTextureViewData)tfxData.data).element;
                output = $"extern {((PushExternInputTextureViewData)tfxData.data).extern_}, element {pTex} (0x{(pTex * 8):X})";
                break;
            case PushExternInputU32Data:
                byte pU32 = ((PushExternInputU32Data)tfxData.data).element;
                output = $"extern {((PushExternInputU32Data)tfxData.data).extern_}, element {pU32} (0x{(pU32 * 4):X})";
                break;
            case PushExternInputUavData:
                byte pUav = ((PushExternInputUavData)tfxData.data).element;
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
                byte texSlot = ((SetShaderTextureData)tfxData.data).value;
                output = $"Texture Slot {texSlot & 0x1F}";
                break;
            case Unk49Data:
                output = $"unk1 {((Unk49Data)tfxData.data).unk1}";
                break;
            case SetShaderSamplerData:
                byte sampSlot = ((SetShaderSamplerData)tfxData.data).value;
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
                var hash = new StringHash(((PushObjectChannelVectorData)tfxData.data).hash);
                output = $"hash {GlobalStrings.Get().GetString(hash)}";
                break;
            case PushGlobalChannelVectorData:
                index = ((PushGlobalChannelVectorData)tfxData.data).Index;
                output = $"index {index}, default {GlobalChannels.GetDefault(index)}";
                break;
            case Unk50Data:
                output = $"unk1 {((Unk50Data)tfxData.data).unk1}";
                break;

            case PushTexDimensionsData:
                var ptd = ((PushTexDimensionsData)tfxData.data);
                Texture tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[ptd.index].Hash);

                output = $"Tex {tex.Hash} ({ptd.index}): {DecodePermuteParam(ptd.fields).ToUpper()}: " +
                    $"({tex.TagData.Width}, {tex.TagData.Height}, {tex.TagData.Depth}, {tex.TagData.ArraySize})";
                break;

            case PushTexTileParamsData:
                var ptt = ((PushTexTileParamsData)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[ptt.index].Hash);

                output = $"Tex {tex.Hash} ({ptt.index}): {DecodePermuteParam(ptt.fields).ToUpper()}: " +
                    $"{tex.TagData.TilingScaleOffset}";
                break;

            case PushTexTileCountData:
                var pttc = ((PushTexTileCountData)tfxData.data);
                tex = FileResourcer.Get().GetFile<Texture>(material.PSSamplers[pttc.index].Hash);

                output = $"Tex {tex.Hash} ({pttc.index}): {DecodePermuteParam(pttc.fields).ToUpper()}: " +
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

public enum TfxBytecode : byte // Not ordered by value, different versions get mapped to this
{
    Add = 0x1,
    Subtract,
    Multiply,
    Divide,
    Multiply2,
    Add2,
    IsZero,
    Min,
    Max,
    LessThan,
    Dot,
    Merge_1_3,
    Merge_2_2,
    Merge_3_1,
    Cubic,
    Lerp,
    LerpSaturated,
    MultiplyAdd,
    Clamp,
    Unk14, // SmoothStep?
    Abs,
    Sign,
    Floor,
    Ceil,
    Round,
    Frac,
    Unk1b, // Normalize()?
    Unk1c, // Maybe also normalize, but slightly different?
    Negate,
    VecRotSin,
    VecRotCos,
    VecRotSinCos,
    PermuteAllX,
    Permute,
    Saturate,
    Unk25,
    Unk26, // Also normalize apparently
    Triangle,
    Jitter,
    Wander,
    Rand,
    RandSmooth,
    Unk2c,
    Unk2d,
    TransformVec4,
    PushConstantVec4,// Shifted to 0x3B in EOF, 0x34 => 0x3B
    LerpConstant,
    LerpConstantSaturated,
    Spline4Const,
    Spline8Const,
    Spline8ConstChain,
    Gradient4Const,
    Gradient8Const,
    PushExternInputFloat,
    PushExternInputVec4,
    PushExternInputMat4,
    PushExternInputTextureView,
    PushExternInputU32,
    PushExternInputUav,
    Unk42, // Not in Pre-BL
    PushFromOutput,
    PopOutput,
    PopOutputMat4,
    PushTemp,
    PopTemp,
    SetShaderTexture,
    Unk49, //{ unk1: u8 }
    SetShaderSampler,
    SetShaderUav,
    Unk4c, //{ unk1: u8 }
    PushSampler,
    PushObjectChannelVector,
    PushGlobalChannelVector,
    Unk50,
    Unk51,
    PushTexDimensions,
    PushTexTileParams,
    PushTexTileCount,
    Unk55,
    Unk56,
    Unk57,
    Unk58,

    // Added in EoF
    Unk34_EoF,
    Unk35_EoF,
    Unk36_EoF,
    Unk37_EoF,
    Unk38_EoF,
    Unk39_EoF,
    Unk3A_EoF,
}

// D1 RoI
public enum TfxBytecode_D1 : byte
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
    Unk14 = 0x14,
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
    Spline8ConstChain = 0x39,
    Gradient4Const = 0x3a,
    Gradient8Const = 0x3b,
    PushExternInputFloat = 0x3c,
    PushExternInputVec4 = 0x3d,
    PushExternInputMat4 = 0x3e,
    PushExternInputTextureView = 0x3f,
    PushExternInputU32 = 0x40,
    PushExternInputUav = 0x41, // idk if this is in D1, F3487E81 uses this but doesnt seem right
    PushFromOutput = 0x41,
    PopOutput = 0x42,
    PopOutputMat4 = 0x43,
    PushTemp = 0x44,
    PopTemp = 0x45,
    SetShaderTexture = 0x46,
    SetShaderSampler = 0x47,
    PushSampler = 0x49,
    PushObjectChannelVector = 0x4A,
    PushGlobalChannelVector = 0x4B,
    Unk50 = 0x4E,
    PushTexDimensions = 0x50,
    PushTexTileParams = 0x51,
    PushTexTileCount = 0x52,
    //Unk55 = 0x54, // No idea if these are in D1
    //Unk56 = 0x55,
    //Unk57 = 0x56,
    //Unk58 = 0x57,
}

// SK to BL
public enum TfxBytecode_BL : byte
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
    Unk14 = 0x14,
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
    Spline8ConstChain = 0x39,
    Gradient4Const = 0x3a,
    Gradient8Const = 0x3b,
    PushExternInputFloat = 0x3c,
    PushExternInputVec4 = 0x3d,
    PushExternInputMat4 = 0x3e,
    PushExternInputTextureView = 0x3f,
    PushExternInputU32 = 0x40,
    PushExternInputUav = 0x41,
    PushFromOutput = 0x42,
    PopOutput = 0x43,
    PopOutputMat4 = 0x44,
    PushTemp = 0x45,
    PopTemp = 0x46,
    SetShaderTexture = 0x47,
    Unk49 = 0x48,
    SetShaderSampler = 0x49,
    SetShaderUav = 0x4A,
    Unk4c = 0x4B,
    PushSampler = 0x4C,
    PushObjectChannelVector = 0x4D,
    PushGlobalChannelVector = 0x4E,
    Unk50 = 0x4F,
    Unk51 = 0x50,
    PushTexDimensions = 0x51,
    PushTexTileParams = 0x52,
    PushTexTileCount = 0x53,
    Unk55 = 0x54,
    Unk56 = 0x55,
    Unk57 = 0x56,
    Unk58 = 0x57,
}

// WQ to TFS
public enum TfxBytecode_TFS : byte
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
    Unk14 = 0x14,
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
    Spline8ConstChain = 0x39,
    Gradient4Const = 0x3a,
    Gradient8Const = 0x3b,
    PushExternInputFloat = 0x3c,
    PushExternInputVec4 = 0x3d,
    PushExternInputMat4 = 0x3e,
    PushExternInputTextureView = 0x3f,
    PushExternInputU32 = 0x40,
    PushExternInputUav = 0x41,
    Unk42 = 0x42,
    PushFromOutput = 0x43,
    PopOutput = 0x44,
    PopOutputMat4 = 0x45,
    PushTemp = 0x46,
    PopTemp = 0x47,
    SetShaderTexture = 0x48,
    Unk49 = 0x49,
    SetShaderSampler = 0x4a,
    SetShaderUav = 0x4b,
    Unk4c = 0x4c,
    PushSampler = 0x4d,
    PushObjectChannelVector = 0x4e,
    PushGlobalChannelVector = 0x4f,
    Unk50 = 0x50,
    Unk51 = 0x51,
    PushTexDimensions = 0x52,
    PushTexTileParams = 0x53,
    PushTexTileCount = 0x54,
    Unk55 = 0x55,
    Unk56 = 0x56,
    Unk57 = 0x57,
    Unk58 = 0x58,
}

// EoF
public enum TfxBytecode_EoF : byte
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
    MultiplyAdd = 0x15,
    Clamp = 0x16,
    Unk14 = 0x17,
    Abs = 0x18,
    Sign = 0x19,
    Floor = 0x1A,
    Ceil = 0x1B,
    Round = 0x1C,
    Frac = 0x1D,
    Unk1b = 0x1E, // Normalize?
    Unk1c = 0x1F, // Maybe also normalize, but slightly different?
    Negate = 0x20,
    VecRotSin = 0x21,
    VecRotCos = 0x22,
    VecRotSinCos = 0x23,
    PermuteAllX = 0x28,
    Permute = 0x29,
    Saturate = 0x2A,
    Unk25 = 0x2C,
    Unk26 = 0x2D,
    Triangle = 0x2E,
    Jitter = 0x2F,
    Wander = 0x30,
    Rand = 0x31,
    RandSmooth = 0x32,
    Unk2c = 0x33,
    Unk2d = 0x34,
    TransformVec4 = 0x35,

    Unk34_EoF = 0x3B, // No clue
    Unk35_EoF = 0x3C,
    Unk36_EoF = 0x3D,
    Unk37_EoF = 0x3E,
    Unk38_EoF = 0x3F,
    Unk39_EoF = 0x40,
    Unk3A_EoF = 0x41,

    PushConstantVec4 = 0x42, // 0x34 => 0x3B EOF, 0x3B => 0x42 Renegades
    LerpConstant = 0x43,
    LerpConstantSaturated = 0x44,
    Spline4Const = 0x45,
    Spline8Const = 0x46,
    Spline8ConstChain = 0x47,
    Gradient4Const = 0x48,
    Gradient8Const = 0x49,
    PushExternInputFloat = 0x4A,
    PushExternInputVec4 = 0x4B,
    PushExternInputMat4 = 0x4C,
    PushExternInputTextureView = 0x4D,
    PushExternInputU32 = 0x4E,
    PushExternInputUav = 0x4F,
    Unk42 = 0x50,
    PushFromOutput = 0x51,
    PopOutput = 0x52,
    PopOutputMat4 = 0x53,
    PushTemp = 0x54,
    PopTemp = 0x55,
    SetShaderTexture = 0x56,
    Unk49 = 0x57,
    SetShaderSampler = 0x58,
    SetShaderUav = 0x59,
    Unk4c = 0x5A,
    PushSampler = 0x5B,
    PushObjectChannelVector = 0x5C,
    PushGlobalChannelVector = 0x5D,
    Unk50 = 0x5E,
    Unk51 = 0x5F,
    PushTexDimensions = 0x60,
    PushTexTileParams = 0x61,
    PushTexTileCount = 0x62,
    Unk55 = 0x63,
    Unk56 = 0x64,
    Unk57 = 0x65,
    Unk58 = 0x66,
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

public struct Spline8ConstChainData
{
    public byte constant_index;
}

public struct Gradient4ConstData
{
    public byte constant_index;
}

public struct Gradient8ConstData
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
    public byte Index;
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

