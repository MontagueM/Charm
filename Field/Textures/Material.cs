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

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_AA6D8080>();
    }

    public void SaveAllTextures(string saveDirectory)
    {
        foreach (var e in Header.VSTextures)
        {
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/VS_{e.TextureIndex}_{e.Texture.Hash}.dds";
            if (!File.Exists(path))
            {
                e.Texture.SaveToDDSFile(path); 
            }
        }
        foreach (var e in Header.PSTextures)
        {
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/PS_{e.TextureIndex}_{e.Texture.Hash}.dds";
            if (!File.Exists(path))
            {
                e.Texture.SaveToDDSFile(path); 
            }
        }
    }
    
    [DllImport("HLSLDecompiler.dll", EntryPoint = "DecompileHLSL", CallingConvention = CallingConvention.Cdecl)]
    public static extern int DecompileHLSL(
        IntPtr pShaderBytecode,
        int BytecodeLength,
        out IntPtr pHlslText
    );

    public string Decompile(byte[] shaderBytecode)
    {
        GCHandle gcHandle = GCHandle.Alloc(shaderBytecode, GCHandleType.Pinned);
        IntPtr pShaderBytecode = gcHandle.AddrOfPinnedObject();
        IntPtr pHlslText = Marshal.AllocHGlobal(50000);
        int result = DecompileHLSL(pShaderBytecode, shaderBytecode.Length, out pHlslText);
        string hlsl = Marshal.PtrToStringUTF8(pHlslText);
        // Marshal.FreeHGlobal(pHlslText);
        gcHandle.Free();
        return hlsl;
    }

    public void SavePixelShader(string saveDirectory)
    {
        if (Header.PixelShader != null)
        {
            string hlsl = Decompile(Header.PixelShader.GetBytecode());
            File.WriteAllText($"{saveDirectory}/{Hash}_PS.hlsl", hlsl);
            string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
            if (usf != String.Empty)
            {
                File.WriteAllText($"{saveDirectory}/PS_{Hash}.usf", usf);
            }
        }
    }
    
    public void SaveVertexShader(string saveDirectory)
    {
        if (Header.VertexShader != null)
        {
            string hlsl = Decompile(Header.VertexShader.GetBytecode());
            File.WriteAllText($"{saveDirectory}/VS_{Hash}.hlsl", hlsl);
            string usf = new UsfConverter().HlslToUsf(this, hlsl, true);
            if (usf != String.Empty)
            {
                File.WriteAllText($"{saveDirectory}/VS_{Hash}.usf", usf);
            }
        }
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
    [DestinyOffset(0x324)] 
    public TagHash PSVector4Container;
    
    [DestinyOffset(0x340), DestinyField(FieldType.TagHash)]
    public ShaderHeader ComputeShader;
    [DestinyOffset(0x348), DestinyField(FieldType.TablePointer)]
    public List<D2Class_CF6D8080> CSTextures;
    [DestinyOffset(0x360), DestinyField(FieldType.TablePointer)]
    public List<D2Class_09008080> Unk360;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk370;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_3F018080> Unk380;
    [DestinyField(FieldType.TablePointer)]
    public List<D2Class_90008080> Unk390;
    
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
