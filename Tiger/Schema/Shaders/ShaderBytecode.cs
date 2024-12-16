using System.Diagnostics;
using System.Runtime.InteropServices;
using Arithmic;

namespace Tiger.Schema;

public class ShaderBytecode : TigerReferenceFile<SShaderBytecode>
{
    public ShaderBytecode(FileHash hash) : base(hash)
    {
    }

    private string? _decompiled = null;

    private List<DXBCIOSignature>? _inputSignatures;
    public List<DXBCIOSignature> InputSignatures
    {
        get
        {
            if (Strategy.IsD1())
                return new();

            if (_inputSignatures != null)
            {
                return _inputSignatures;
            }

            _inputSignatures = GetInputSignatures();
            Log.Debug($"Input signatures for shader {Hash} ({_inputSignatures.Count}):");
            foreach (DXBCIOSignature inputSignature in _inputSignatures)
            {
                Log.Debug(inputSignature.DebugString());
            }
            return _inputSignatures;
        }
    }

    private List<DXBCIOSignature>? _outputSignatures;
    public List<DXBCIOSignature> OutputSignatures
    {
        get
        {
            if (Strategy.IsD1())
                return new();

            if (_outputSignatures != null)
            {
                return _outputSignatures;
            }

            _outputSignatures = GetOutputSignatures();
            Log.Debug($"Output signatures for shader {Hash} ({_outputSignatures.Count}):");
            foreach (DXBCIOSignature outputSignature in _outputSignatures)
            {
                Log.Debug(outputSignature.DebugString());
            }
            return _outputSignatures;
        }
    }

    private List<DXBCShaderResource> _resources;
    public List<DXBCShaderResource> Resources
    {
        get
        {
            if (Strategy.IsD1())
                return new();

            if (_resources != null)
            {
                return _resources;
            }

            _resources = GetShaderResources();
            Log.Debug($"Shader Resources for shader {Hash} ({_resources.Count}):");
            foreach (DXBCShaderResource resource in _resources)
            {
                Log.Debug(resource.DebugString());
            }
            return _resources;
        }
    }

    public byte[] GetBytecode()
    {
        if (Strategy.IsD1())
            return Array.Empty<byte>();

        using TigerReader reader = GetReferenceReader();
        return reader.ReadBytes((int)_tag.BytecodeSize);
    }

    private static object _lock = new object();
    public string Decompile(string name, string savePath = "hlsl_temp")
    {
        if (_decompiled is not null)
            return _decompiled;

        var shaderBytecode = GetBytecode();
        if (Strategy.IsD1() || shaderBytecode.Length == 0)
            return "";

        string binPath = $"{savePath}/{name}.bin";
        string hlslPath = $"{savePath}/{name}.hlsl";

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory($"{savePath}/");
        }

        lock (_lock)
        {
            if (!File.Exists(binPath))
            {
                File.WriteAllBytes(binPath, shaderBytecode);
            }
        }

        if (!File.Exists(hlslPath))
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "ThirdParty/3dmigoto_shader_decomp.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-D \"{binPath}\"";

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            if (!File.Exists(hlslPath))
            {
                throw new FileNotFoundException($"Decompilation failed for {name}");
            }
        }

        string hlsl = "";
        lock (_lock)
        {
            while (hlsl == "")
            {
                try  // needed for slow machines
                {
                    hlsl = File.ReadAllText(hlslPath);
                    _decompiled = hlsl;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
        return hlsl;
    }

    //These are kinda messy and can probably be simplified
    public List<DXBCIOSignature> GetInputSignatures()
    {
        using TigerReader reader = GetReferenceReader();

        reader.Seek(0x34, SeekOrigin.Begin);
        uint inputSignatureCount = reader.ReadUInt32();
        reader.Seek(0x4, SeekOrigin.Current);
        List<DXBCIOSignature> inputSignatures = new();
        for (int i = 0; i < inputSignatureCount; i++)
        {
            DXBCIOElement signature = reader.ReadType<DXBCIOElement>();
            inputSignatures.Add(new DXBCIOSignature(reader, 0x34, signature));
        }

        return inputSignatures;
    }

    public List<DXBCIOSignature> GetOutputSignatures()
    {
        using TigerReader reader = GetReferenceReader();

        reader.Seek(0x30, SeekOrigin.Begin);
        uint chunkStart = reader.ReadUInt32();
        reader.Seek(chunkStart + 0x8, SeekOrigin.Current);

        uint outputSignatureCount = reader.ReadUInt32();
        reader.Seek(0x4, SeekOrigin.Current);
        List<DXBCIOSignature> outputSignatures = new();
        for (int i = 0; i < outputSignatureCount; i++)
        {
            DXBCIOElement signature = reader.ReadType<DXBCIOElement>();
            outputSignatures.Add(new DXBCIOSignature(reader, chunkStart + 0x8 + 0x34, signature));
        }

        return outputSignatures;
    }

    public List<DXBCShaderResource> GetShaderResources()
    {
        using TigerReader reader = GetReferenceReader();
        // Go to ISGN chunk
        reader.Seek(0x30, SeekOrigin.Begin);
        // Get the chunk size of ISGN
        uint sizeISGN = reader.ReadUInt32();

        // Go to OSGN chunk
        reader.Seek(sizeISGN + 0x4, SeekOrigin.Current);
        // Get the chunk size of OSGN
        uint sizeOSGN = reader.ReadUInt32();

        // Go to SHEX chunk
        reader.Seek(sizeOSGN + 0x14, SeekOrigin.Current);

        //uint a = reader.ReadUInt32();
        //Debug.Assert(a == 1480935507);

        List<DXBCShaderResource> shaderResources = new();
        ResourceType type = (ResourceType)reader.ReadUInt32();
        try
        {
            do
            {
                switch (type)
                {
                    case ResourceType.CBuffer:
                    case ResourceType.CBuffer1: //Dynamically Linked or some shit, i forgot the name
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.CBuffer,
                            Index = reader.ReadUInt32(),
                            Count = reader.ReadUInt32()
                        });
                        break;
                    case ResourceType.Buffer:
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.Buffer,
                            Index = reader.ReadUInt32(),
                            Count = 0
                        });
                        reader.Seek(0x4, SeekOrigin.Current);
                        break;
                    case ResourceType.Texture2D:
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.Texture2D,
                            Index = reader.ReadUInt32(),
                            Count = 0
                        });
                        reader.Seek(0x4, SeekOrigin.Current);
                        break;
                    case ResourceType.Texture3D:
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.Texture3D,
                            Index = reader.ReadUInt32(),
                            Count = 0
                        });
                        reader.Seek(0x4, SeekOrigin.Current);
                        break;
                    case ResourceType.TextureCube:
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.TextureCube,
                            Index = reader.ReadUInt32(),
                            Count = 0
                        });
                        reader.Seek(0x4, SeekOrigin.Current);
                        break;
                    case ResourceType.SamplerState:
                        reader.Seek(0x4, SeekOrigin.Current);
                        shaderResources.Add(new DXBCShaderResource
                        {
                            ResourceType = ResourceType.SamplerState,
                            Index = reader.ReadUInt32(),
                            Count = 0
                        });
                        break;
                        //default:
                        //    throw new NotSupportedException($"Unknown Type {type}");
                }

                type = (ResourceType)reader.ReadUInt32();

            }
            while (type != ResourceType.None
                    && type != ResourceType.Output
                    && type != ResourceType.PSInput
                    && type != ResourceType.VSInput);
        }
        catch (Exception ex)
        {
            Log.Error($"{Hash}: {ex}");
        }

        return shaderResources;
    }
}

[SchemaStruct(0x28)]
public struct SShaderBytecode
{
    public ulong FileSize;
    public ulong BytecodeSize;
    public TigerHash Unk0C;
}

public enum DXBCSemantic
{
    None,
    Position,
    Texcoord,
    Normal,
    Binormal,
    Tangent,
    BlendIndices,
    BlendWeight,
    Colour,

    //System semantics
    SystemVertexId,
    SystemInstanceId,
    SystemTarget,
    SystemPosition,
    SystemIsFrontFace
}


public struct DXBCIOSignature
{
    public DXBCSemantic Semantic;
    public uint SemanticIndex;
    public RegisterComponentType ComponentType;
    public ComponentMask Mask;
    public uint RegisterIndex;
    public int BufferIndex = -1; // gets set in a decorator

    public DXBCIOSignature(TigerReader reader, long chunkStart, DXBCIOElement inputSignature)
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
                Semantic = DXBCSemantic.Position;
                break;
            case "TEXCOORD":
                Semantic = DXBCSemantic.Texcoord;
                break;
            case "NORMAL":
                Semantic = DXBCSemantic.Normal;
                break;
            case "BINORMAL":
                Semantic = DXBCSemantic.Binormal;
                break;
            case "TANGENT":
                Semantic = DXBCSemantic.Tangent;
                break;
            case "BLENDINDICES":
                Semantic = DXBCSemantic.BlendIndices;
                break;
            case "BLENDWEIGHT":
                Semantic = DXBCSemantic.BlendWeight;
                break;
            case "COLOR":
                Semantic = DXBCSemantic.Colour;
                break;

            //System
            case "SV_POSITION":
                Semantic = DXBCSemantic.SystemPosition;
                break;
            case "SV_isFrontFace":
                Semantic = DXBCSemantic.SystemIsFrontFace;
                break;
            case "SV_VertexID": //Does case matter here?
            case "SV_VERTEXID":
                Semantic = DXBCSemantic.SystemVertexId;
                break;
            case "SV_InstanceID":
                Semantic = DXBCSemantic.SystemInstanceId;
                break;
            case "SV_Target":
            case "SV_TARGET":
                Semantic = DXBCSemantic.SystemTarget;
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

    public string GetMaskType()
    {
        switch (GetNumberOfComponents())
        {
            case 1:
                return "uint";
            case 2:
                return "float2";
            case 3:
                return "float3";
            case 4:
                return "float4";
            default:
                return "float4";
        }
    }

    public override string ToString()
    {
        switch (Semantic)
        {
            case DXBCSemantic.Position:
                return "POSITION";
            case DXBCSemantic.Texcoord:
                return "TEXCOORD";
            case DXBCSemantic.Normal:
                return "NORMAL";
            case DXBCSemantic.Binormal:
                return "BINORMAL";
            case DXBCSemantic.Tangent:
                return "TANGENT";
            case DXBCSemantic.BlendIndices:
                return "BLENDINDICES";
            case DXBCSemantic.BlendWeight:
                return "BLENDWEIGHT";
            case DXBCSemantic.Colour:
                return "COLOR";

            case DXBCSemantic.SystemPosition:
                return "SV_POSITION";
            case DXBCSemantic.SystemIsFrontFace:
                return "SV_ISFRONTFACE";
            case DXBCSemantic.SystemVertexId:
                return "SV_VERTEXID";
            case DXBCSemantic.SystemInstanceId:
                return "SV_INSTANCEID";
            case DXBCSemantic.SystemTarget:
                return "SV_TARGET";
            default:
                throw new NotImplementedException($"Unknown Semantic {Semantic}");
        }
    }
}

public struct DXBCShaderResource
{
    public ResourceType ResourceType;
    public uint Index;
    public uint Count;
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct DXBCIOElement
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

public enum ResourceType
{
    CBuffer = 0x04000059,
    CBuffer1 = 0x04000859,
    Buffer = 0x04000858,
    SamplerState = 0x0300005A,
    Texture2D = 0x04001858,
    Texture3D = 0x04002858,
    TextureCube = 0x04003058,

    // Can already get through IOSignatures
    // But need to know where to stop reading
    None = 0x02000068,
    VSInput = 0x0300005F,
    PSInput = 0x03001062,
    Output = 0x03000065
}
