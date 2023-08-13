using System.Diagnostics;
using System.Runtime.InteropServices;
using Arithmic;

namespace Tiger.Schema;

public class ShaderBytecode : TigerReferenceFile<SShaderBytecode>
{
    private List<InputSignature>? _inputSignatures;
    public List<InputSignature> InputSignatures
    {
        get
        {
            if (_inputSignatures != null)
            {
                return _inputSignatures;
            }

            _inputSignatures = GetInputSignatures();
            Log.Debug($"Input signatures for shader {Hash} ({_inputSignatures.Count}):");
            foreach (InputSignature inputSignature in _inputSignatures)
            {
                Log.Debug(inputSignature.DebugString());
            }
            return _inputSignatures;
        }
    }

    public ShaderBytecode(FileHash hash) : base(hash)
    {
    }

    public byte[] GetBytecode()
    {
        using TigerReader reader = GetReferenceReader();
        return reader.ReadBytes((int)_tag.BytecodeSize);
    }

    public List<InputSignature> GetInputSignatures()
    {
        using TigerReader reader = GetReferenceReader();
#if DEBUG
        reader.Seek(0x2C, SeekOrigin.Begin);
        uint inputSignatureCC = reader.ReadUInt32();
        Debug.Assert(inputSignatureCC == 1313297225);
        uint chunkSize = reader.ReadUInt32();
#endif
        reader.Seek(0x34, SeekOrigin.Begin);
        uint inputSignatureCount = reader.ReadUInt32();
        reader.Seek(0x4, SeekOrigin.Current);
        List<InputSignature> inputSignatures = new();
        for (int i = 0; i < inputSignatureCount; i++)
        {
            DXBCInputSignature signature = reader.ReadType<DXBCInputSignature>();
            if (signature.SystemValueType == 0)  // we only want non-system value inputs
            {
                inputSignatures.Add(new InputSignature(reader, 0x34, signature));
            }
        }

        return inputSignatures;
    }
}

[SchemaStruct(0x28)]
public struct SShaderBytecode
{
    public ulong FileSize;
    public ulong BytecodeSize;
    public TigerHash Unk0C;
}

public enum InputSemantic
{
    Position,
    Texcoord,
    Normal,
    Tangent,
    BlendIndices,
    BlendWeight,
    Colour
}

public struct InputSignature
{
    public InputSemantic Semantic;
    public RegisterComponentType ComponentType;
    public ComponentMask Mask;
    public uint RegisterIndex;

    public InputSignature(TigerReader reader, long chunkStart, DXBCInputSignature inputSignature)
    {
        ComponentType = inputSignature.ComponentType;
        Mask = inputSignature.Mask;
        RegisterIndex = inputSignature.Register;

        long offset = reader.Position;
        reader.Seek(chunkStart + inputSignature.SemanticNameOffset, SeekOrigin.Begin);
        string semanticName = reader.ReadNullTerminatedString();
        switch (semanticName)
        {
            case "POSITION":
                Semantic = InputSemantic.Position;
                break;
            case "TEXCOORD":
                Semantic = InputSemantic.Texcoord;
                break;
            case "NORMAL":
                Semantic = InputSemantic.Normal;
                break;
            case "TANGENT":
                Semantic = InputSemantic.Tangent;
                break;
            case "BLENDINDICES":
                Semantic = InputSemantic.BlendIndices;
                break;
            case "BLENDWEIGHT":
                Semantic = InputSemantic.BlendWeight;
                break;
            case "COLOR":
                Semantic = InputSemantic.Colour;
                break;
            default:
                throw new NotImplementedException($"Unknown semantic {semanticName}");
        }

        reader.Seek(offset, SeekOrigin.Begin);
    }

    public int GetNumberOfComponents()
    {
        return Mask switch
        {
            ComponentMask.X => 1,
            ComponentMask.Y => 1,
            ComponentMask.Z => 1,
            ComponentMask.W => 1,
            ComponentMask.XY => 2,
            ComponentMask.XYZ => 4,  // XYZ/3 is always padded to 4
            ComponentMask.XYZW => 4,
            _ => throw new NotImplementedException($"Unknown component mask {Mask}")
        };
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct DXBCInputSignature
{
    public int SemanticNameOffset;
    public uint SemanticIndex;
    public uint SystemValueType;
    public RegisterComponentType ComponentType;
    public uint Register;
    public ComponentMask Mask;
    public ComponentMask ReadWriteMask;
}

public enum RegisterComponentType : uint
{
    Unknown = 0,
    UInt32 = 1,
    SInt32 = 2,
    Float32 = 3,
}

[Flags]
public enum ComponentMask : byte
{
    None = 0,

    X = 1,
    Y = 2,
    Z = 4,
    W = 8,

    XY = X | Y,
    XYZ = X | Y | Z,

    XYZW = X | Y | Z | W
}
