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
        List<InputSignature> inputSignatures = new();
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            using TigerReader reader1 = GetReader();
            reader1.Seek(0x38, SeekOrigin.Begin);
            var semanticCount = reader1.ReadByte();
            reader1.Seek(0x54, SeekOrigin.Begin);

            Console.WriteLine($"Semantic Count {semanticCount}");
            for (int i = 0; i < semanticCount; i++)
            {
                Console.WriteLine($"--------------------");
                ORBISInputSignature signature = reader1.ReadType<ORBISInputSignature>();

                Console.WriteLine($"-SemanticIndex {signature.SemanticIndex}");
                Console.WriteLine($"-SemanticPosition? {signature.SemanticPosition}");
                Console.WriteLine($"-SemanticComponents {signature.SemanticComponents}");
                Console.WriteLine($"-SystemValueType {signature.SystemValueType}");

                if (signature.SystemValueType >= 1)
                    break;

            }

            return inputSignatures;
        }

        using TigerReader reader = GetReferenceReader();
        reader.Seek(0x34, SeekOrigin.Begin);
        uint inputSignatureCount = reader.ReadUInt32();
        reader.Seek(0x4, SeekOrigin.Current);
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
    None,
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
    public uint SemanticIndex;
    public RegisterComponentType ComponentType;
    public ComponentMask Mask;
    public uint RegisterIndex;
    public int BufferIndex = -1; // gets set in a decorator

    public InputSignature(TigerReader reader, long chunkStart, DXBCInputSignature inputSignature)
    {
        SemanticIndex = inputSignature.SemanticIndex;
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

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct ORBISInputSignature
{
    // Byte index, byte semantic or postion to read to, byte # components, byte system value

    // Signatures start at around 0x50?
    // 0F001001 onwards are outputs?

    // Semantics aren't clearly defined so its kind of a guess based on the number of components and/or position to read to?

    // Position is (at least should be) always first
    // 00040300: 00 being index, 04 being POSITION or meaning to read to 0x4, 03 being the number of components (XYZ), 00 meaning non-system?
    // 01080400: 01 index, 08 being TEXCOORD0?, 04 (can be 02) number of components (XYZW) (XY UVs, ZW Vertex Color if 4?)
    // 020C0300: 02, NORMAL, XYZ
    // 03100400: 03, TANGENT, XYZW
    // 04140400: 04, Unknown, XYZW (ive seen 01 though)

    // Vertex buffer BDF24681 has position, texcoord, normal, tangent and color
    // Vertex shader header for the material has, in order, 00040300 (Pos XYZ), 01080200 (texcoord, XY),
    // 020C0300 (Norm, XYZ), 03100400 (Tangent, XYZW), then maybe 04140400 is Vertex color XYZW

    public byte SemanticIndex;
    public byte SemanticPosition; // ??
    public byte SemanticComponents;
    public byte SystemValueType;
}

//public enum ORBISSemantic : byte
//{
//    Position = 0x4,
//    Texcoord = 0x8,
//    Normal = 0xC,
//    Tangent = 0x10,
//    Color = 0x14,
//}
