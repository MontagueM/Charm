using System.Runtime.InteropServices;
using System.Text;
using Field.Entities;
using Field.General;
using File = System.IO.File;

namespace Field.Textures;

public class Material : Tag
{
    public D2Class_AA6D8080 Header;

    
    public Material(TagHash hash) : base(hash)
    {
    }
    public Material(string hash) : base(hash)
    {
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AA6D8080>();
    }

    public void SaveAllTextures(string saveDirectory)
    {
        
    }
    
    [DllImport("HLSLDecompiler.dll", EntryPoint = "DecompileHLSL", CallingConvention = CallingConvention.StdCall)]
    public static extern int DecompileHLSL(
        IntPtr pShaderBytecode,
        int BytecodeLength,
        out IntPtr pHlslText,
        out IntPtr pShaderModel
    );

    private string Decompile(byte[] shaderBytecode)
    {
        GCHandle gcHandle = GCHandle.Alloc(shaderBytecode, GCHandleType.Pinned);
        IntPtr pShaderBytecode = gcHandle.AddrOfPinnedObject();
        IntPtr pHlslText = new IntPtr();
        IntPtr pShaderModel = new IntPtr();
        int result = DecompileHLSL(pShaderBytecode, shaderBytecode.Length, out pHlslText, out pShaderBytecode);
        string hlsl = Marshal.PtrToStringUTF8(pHlslText);
        return hlsl;
    }

    public void SavePixelShader(string saveDirectory)
    {
        if (Header.PixelShader != null)
        {
            string hlsl = Decompile(Header.PixelShader.GetBytecode());
            string usf = HlslToUsf(hlsl);
            File.WriteAllText($"{saveDirectory}/{Hash}_PS.hlsl", usf);
        }
    }

    private string HlslToUsf(string hlsl)
    {
        StringBuilder usf = new StringBuilder(hlsl);
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x3D0)]
public struct D2Class_AA6D8080
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [DestinyOffset(0x70), DestinyField(FieldType.TagHash)]
    public ShaderHeader VertexShader;
    [DestinyOffset(0x78), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> VSTextures;
    [DestinyOffset(0x90), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk90;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkA0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> UnkB0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> UnkC0;
    
    [DestinyOffset(0x2B0), DestinyField(FieldType.TagHash)]
    public ShaderHeader PixelShader;
    [DestinyOffset(0x2B8), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> PSTextures;
    [DestinyOffset(0x2D0), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk2D0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk2E0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> Unk2F0;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk300;
    
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public struct D2Class_CF6D8080
{
    public long TextureIndex;
    [DestinyField(FieldType.TagHash64)]
    public TextureHeader Texture;
}

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
public struct D2Class_3F018080
{
    [DestinyField(FieldType.TagHash64)]
    public Tag Unk00;
}
