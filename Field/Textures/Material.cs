using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Field.Entities;
using Field.General;
using File = System.IO.File;

namespace Field;

public class Material : Tag
{
    public D2Class_AA6D8080 Header;
    public static object _lock = new object();

    
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
            if (e.Texture == null)
            {
                continue;
            }
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/VS_{e.TextureIndex}_{e.Texture.Hash}";
            if (!File.Exists(path))
            {
                e.Texture.SavetoFile(path); 
            }
        }
        foreach (var e in Header.PSTextures)
        {
            if (e.Texture == null)
            {
                continue;
            }
            // todo change to 64 bit hash?
            string path = $"{saveDirectory}/{e.Texture.Hash}";
            if (!File.Exists(path + ".dds") && !File.Exists(path + ".png") && !File.Exists(path + ".tga"))
            {
                e.Texture.SavetoFile(path); 
            }
        }
    }
    
    // [DllImport("HLSLDecompiler.dll", EntryPoint = "DecompileHLSL", CallingConvention = CallingConvention.Cdecl)]
    // public static extern IntPtr DecompileHLSL(
    //     IntPtr pShaderBytecode,
    //     int BytecodeLength,
    //     out int pHlslTextLength
    // );

    public string Decompile(byte[] shaderBytecode, string? type = "ps", string directory = "hlsl_temp")
    {
        string binPath = $"{directory}/{type}{Hash}.bin";
        string hlslPath = $"{directory}/{type}{Hash}.hlsl";

      

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
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
            startInfo.FileName = "3dmigoto_shader_decomp.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = $"-D {binPath}";
            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }

            if (!File.Exists(hlslPath))
            {
                throw new FileNotFoundException($"Decompilation failed for {Hash}");
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
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                }
            }
        }
        return hlsl;
    }
    
    public static void Decompile(byte[] shaderBytecode, TagHash hash, string? type = "ps", string directory = "hlsl_temp")
    {
        string binPath = $"{directory}/{type}{hash}.bin";
        string hlslPath = $"{directory}/{type}{hash}.hlsl";

        if (File.Exists(hlslPath) || File.Exists(binPath))
        {
            return;
        }

        File.WriteAllBytes(binPath, shaderBytecode);

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.CreateNoWindow = false;
        startInfo.UseShellExecute = false;
        startInfo.FileName = "3dmigoto_shader_decomp.exe";
        startInfo.WindowStyle = ProcessWindowStyle.Hidden;
        startInfo.Arguments = $"-D {binPath}";
        startInfo.RedirectStandardOutput = false;
        using (Process exeProcess = Process.Start(startInfo))
        {
            exeProcess.WaitForExit();
        }

        if (!File.Exists(hlslPath))
        {
            throw new FileNotFoundException($"Decompilation failed for {hash}");
        }
        
        File.Delete(binPath);
    }

    public void SavePixelShader(string saveDirectory)
    {
        if (Header.PixelShader != null && !File.Exists($"{saveDirectory}/PS_{Hash}.usf") && !File.Exists($"{saveDirectory}/PS_{Hash}.hlsl"))
        {
            string hlsl = Decompile(Header.PixelShader.GetBytecode());
            File.WriteAllText($"{saveDirectory}/PS_{Hash}.hlsl", hlsl);
            string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
            string vfx = new VfxConverter().HlslToVfx(this, hlsl, false);
            
            Directory.CreateDirectory($"{saveDirectory}/Source2");
            Directory.CreateDirectory($"{saveDirectory}/Source2/materials");
            StringBuilder vmat = new StringBuilder();
            if (usf != String.Empty || vfx != String.Empty)
            {
                try
                {
                    File.WriteAllText($"{saveDirectory}/PS_{Hash}.usf", usf);
                    File.WriteAllText($"{saveDirectory}/Source2/PS_{Hash}.vfx", vfx);
                    Console.WriteLine($"Saved pixel shader {Hash}");
                }
                catch (IOException)  // threading error
                {
                }
            }
            
            vmat.AppendLine("Layer0 \n{");
            vmat.AppendLine($"  shader \"ps_{Hash}.vfx\"");
            vmat.AppendLine("   F_ALPHA_TEST 1");
            foreach (var e in Header.PSTextures)
            {
                if (e.Texture == null)
                {
                    continue;
                }
                //Console.WriteLine("Saving texture " + e.Texture.Hash + " " + e.TextureIndex + " " + e.Texture.IsSrgb().ToString());
                vmat.AppendLine($"  TextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
            }
            vmat.AppendLine("}");
            
            if(!File.Exists($"{saveDirectory}/Source2/materials/{Hash}.vmat"))
            {
                try
                {
                    File.WriteAllText($"{saveDirectory}/Source2/materials/{Hash}.vmat", vmat.ToString());
                }
                catch (IOException)  
                {
                }
            }
        }
    }
    
    public void SaveVertexShader(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}");
        if (Header.VertexShader != null && !File.Exists($"{saveDirectory}/VS_{Hash}.usf") && !File.Exists($"{saveDirectory}/VS_{Hash}.hlsl"))
        {
            string hlsl = Decompile(Header.VertexShader.GetBytecode(), "vs");
            File.WriteAllText($"{saveDirectory}/VS_{Hash}.hlsl", hlsl);
            string usf = new UsfConverter().HlslToUsf(this, hlsl, true);
            if (usf != String.Empty)
            {
                try
                {
                    File.WriteAllText($"{saveDirectory}/VS_{Hash}.usf", usf);
                    Console.WriteLine($"Saved vertex shader {Hash}");
                }
                catch (IOException)  // threading error
                {
                }
            }
        }
    }

    public void SaveComputeShader(string saveDirectory)
    {
        Directory.CreateDirectory($"{saveDirectory}");
        if (Header.ComputeShader != null && !File.Exists($"{saveDirectory}/CS_{Hash}.usf"))
        {
            string hlsl = Decompile(Header.ComputeShader.GetBytecode(), "cs");
            File.WriteAllText($"{saveDirectory}/CS_{Hash}.hlsl", hlsl);
            string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
            if (usf != String.Empty)
            {
                try
                {
                    File.WriteAllText($"{saveDirectory}/CS_{Hash}.usf", usf);
                    Console.WriteLine($"Saved compute shader {Hash}");
                }
                catch (IOException)  // threading error
                {
                }
            }
        }
    }

    public static List<Vector4> GetDataFromContainer(TagHash containerHash)
    {
        DestinyFile container = new DestinyFile(PackageHandler.GetEntryReference(containerHash));
        byte[] containerData = container.GetData();
        int num = containerData.Length / 16;
        
        List<Vector4> float4s = new List<Vector4>();
        for (int i = 0; i < containerData.Length / 16; i++)
        {
            float4s.Add(StructConverter.ToStructure<Vector4>(containerData.Skip(i*16).Take(16).ToArray()));
        }

        return float4s;
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
    [DestinyOffset(0xE4)] 
    public TagHash VSVector4Container;
    
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