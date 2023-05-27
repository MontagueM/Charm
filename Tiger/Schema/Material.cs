using System.Diagnostics;
using System.Text;

namespace Tiger.Schema;


// public class RawMaterial
// {
//     public VertexShader VSBytecode;
//     public PixelShader PSBytecode;
//     public List<Texture> VSTextures;
//     public List<Texture> PSTextures;
//     public List<ConstantBuffer> PSConstantBuffers;
//     public List<ConstantBuffer> VSConstantBuffers;
// }
//
public class Material : Tag<SMaterial>
{
    public static object _lock = new object();

    public Material(FileHash tagHash) : base(tagHash)
    {
    }

    public void SaveAllTextures(string saveDirectory)
    {
        foreach (var e in _tag.VSTextures)
        {
            if (e.Texture.Hash.IsInvalid())
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
        foreach (var e in _tag.PSTextures)
        {
            if (e.Texture.Hash.IsInvalid())
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

    public string Decompile(byte[] shaderBytecode, string? type = "ps")
    {
        // tried doing it via dll pinvoke but seemed to cause way too many problems so doing it via exe instead
        // string hlsl;
        // lock (_lock)
        // {
        //     GCHandle gcHandle = GCHandle.Alloc(shaderBytecode, GCHandleType.Pinned);
        //     IntPtr pShaderBytecode = gcHandle.AddrOfPinnedObject();
        //     IntPtr pHlslText = Marshal.AllocHGlobal(5000);
        //     int len;
        //     pHlslText = DecompileHLSL(pShaderBytecode, shaderBytecode.Length, out int pHlslTextLength);
        //     // len = Marshal.ReadInt32(pHlslTextLength);
        //     len = pHlslTextLength;
        //     hlsl = Marshal.PtrToStringUTF8(pHlslText);
        //     gcHandle.Free();
        // }
        // // Marshal.FreeHGlobal(pHlslText);
        // return hlsl;

        string directory = "hlsl_temp";
        string binPath = $"{directory}/{type}{Hash}.bin";
        string hlslPath = $"{directory}/{type}{Hash}.hlsl";



        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory("hlsl_temp/");
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

    public void SavePixelShader(string saveDirectory, bool isTerrain = false)
    {
        if (_tag.PixelShader != null)
        {
            string hlsl = Decompile(_tag.PixelShader.GetBytecode());
            string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
            string vfx = new VfxConverter().HlslToVfx(this, hlsl, false, isTerrain);

            Directory.CreateDirectory($"{saveDirectory}/Source2");
            Directory.CreateDirectory($"{saveDirectory}/Source2/materials");
            StringBuilder vmat = new StringBuilder();
            if (usf != String.Empty || vfx != String.Empty)
            {
                try
                {
                    if (!File.Exists($"{saveDirectory}/PS_{Hash}.usf"))
                    {
                        File.WriteAllText($"{saveDirectory}/PS_{Hash}.usf", usf);
                    }
                    if (!File.Exists($"{saveDirectory}/Source2/PS_{Hash}.shader"))
                    {
                        File.WriteAllText($"{saveDirectory}/Source2/PS_{Hash}.shader", vfx);
                    }

                    Console.WriteLine($"Saved pixel shader {Hash}");
                }
                catch (IOException)  // threading error
                {
                }
            }

            vmat.AppendLine("Layer0 \n{");

            //If the shader doesnt exist, just use the default complex.shader
            if (!File.Exists($"{saveDirectory}/Source2/PS_{Hash}.shader"))
            {
                vmat.AppendLine($"  shader \"complex.shader\"");

                //Use just the first texture for the diffuse
                if (_tag.PSTextures.Count > 0)
                {
                    vmat.AppendLine($"  TextureColor \"materials/Textures/{_tag.PSTextures[0].Texture.Hash}.png\"");
                }
            }
            else
            {
                vmat.AppendLine($"  shader \"ps_{Hash}.shader\"");
                vmat.AppendLine("   F_ALPHA_TEST 1");
            }

            foreach (var e in _tag.PSTextures)
            {
                if (e.Texture == null)
                {
                    continue;
                }

                vmat.AppendLine($"  TextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
            }

            // if(isTerrain)
            // {
            //     vmat.AppendLine($"  TextureT14 \"materials/Textures/{partEntry.Dyemap.Hash}.png\"");
            // }
            vmat.AppendLine("}");

            if (!File.Exists($"{saveDirectory}/Source2/materials/{Hash}.vmat"))
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
        if (_tag.VertexShader.Hash.IsValid() && !File.Exists($"{saveDirectory}/VS_{Hash}.usf"))
        {
            string hlsl = Decompile(_tag.VertexShader.GetBytecode(), "vs");
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
        if (_tag.ComputeShader != null && !File.Exists($"{saveDirectory}/CS_{Hash}.usf"))
        {
            string hlsl = Decompile(_tag.ComputeShader.GetBytecode(), "cs");
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
}
