using System.Diagnostics;
using System.Text;

namespace Tiger.Schema.Shaders
{
    public interface IMaterial : ISchema
    {
        public FileHash FileHash { get; }
        public IEnumerable<STextureTag> EnumerateVSTextures();
        public IEnumerable<STextureTag> EnumeratePSTextures();
        public ShaderBytecode VertexShader { get; }
        public ShaderBytecode PixelShader { get; }
        public FileHash PSVector4Container { get; }
        public DynamicArray<D2Class_09008080> Unk90 { get; }
        public DynamicArray<Vec4> UnkA0 { get; }
        public DynamicArray<Vec4> UnkC0 { get; }
        public DynamicArray<D2Class_09008080> Unk2D0 { get; }
        public DynamicArray<Vec4> Unk2E0 { get; }
        public DynamicArray<Vec4> Unk300 { get; }
        public static object _lock = new object();

        public void SaveAllTextures(string saveDirectory)
        {
            foreach (var e in EnumerateVSTextures())
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
            foreach (var e in EnumeratePSTextures())
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

        public string Decompile(byte[] shaderBytecode, string? type = "ps")
        {
            string directory = "hlsl_temp";
            string binPath = $"{directory}/{type}{FileHash}.bin";
            string hlslPath = $"{directory}/{type}{FileHash}.hlsl";

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
                startInfo.FileName = "ThirdParty/3dmigoto_shader_decomp.exe";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.Arguments = $"-D {binPath}";
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }

                if (!File.Exists(hlslPath))
                {
                    throw new FileNotFoundException($"Decompilation failed for {FileHash}");
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
            if (PixelShader != null)
            {
                string hlsl = Decompile(PixelShader.GetBytecode());
                string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
                string vfx = new VfxConverter().HlslToVfx(this, hlsl, false, isTerrain);

                Directory.CreateDirectory($"{saveDirectory}/Source2");
                Directory.CreateDirectory($"{saveDirectory}/Source2/materials");
                StringBuilder vmat = new StringBuilder();
                if (usf != String.Empty || vfx != String.Empty)
                {
                    try
                    {
                        if (!File.Exists($"{saveDirectory}/PS_{FileHash}.usf"))
                        {
                            File.WriteAllText($"{saveDirectory}/PS_{FileHash}.usf", usf);
                        }
                        if (!File.Exists($"{saveDirectory}/Source2/PS_{FileHash}.shader"))
                        {
                            File.WriteAllText($"{saveDirectory}/Source2/PS_{FileHash}.shader", vfx);
                        }

                        Console.WriteLine($"Saved pixel shader {FileHash}");
                    }
                    catch (IOException)  // threading error
                    {
                    }
                }

                vmat.AppendLine("Layer0 \n{");

                //If the shader doesnt exist, just use the default complex.shader
                if (!File.Exists($"{saveDirectory}/Source2/PS_{FileHash}.shader"))
                {
                    vmat.AppendLine($"  shader \"complex.shader\"");

                    //Use just the first texture for the diffuse
                    if (EnumeratePSTextures().Any())
                    {
                        vmat.AppendLine($"  TextureColor \"materials/Textures/{EnumeratePSTextures().First().Texture.Hash}.png\"");
                    }
                }
                else
                {
                    vmat.AppendLine($"  shader \"ps_{FileHash}.shader\"");
                    vmat.AppendLine("   F_ALPHA_TEST 1");
                }

                foreach (var e in EnumeratePSTextures())
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

                if (!File.Exists($"{saveDirectory}/Source2/materials/{FileHash}.vmat"))
                {
                    try
                    {
                        File.WriteAllText($"{saveDirectory}/Source2/materials/{FileHash}.vmat", vmat.ToString());
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
            if (VertexShader.Hash.IsValid() && !File.Exists($"{saveDirectory}/VS_{FileHash}.usf"))
            {
                string hlsl = Decompile(VertexShader.GetBytecode(), "vs");
                string usf = new UsfConverter().HlslToUsf(this, hlsl, true);
                if (usf != String.Empty)
                {
                    try
                    {
                        File.WriteAllText($"{saveDirectory}/VS_{FileHash}.usf", usf);
                        Console.WriteLine($"Saved vertex shader {FileHash}");
                    }
                    catch (IOException) // threading error
                    {
                    }
                }
            }
        }
    }
}


// public class RawMaterial
// {
//     public VertexShader VSBytecode;
//     public PixelShader PSBytecode;
//     public List<Texture> VSTextures;
//     public List<Texture> PSTextures;
//     public List<ConstantBuffer> PSConstantBuffers;
//     public List<ConstantBuffer> VSConstantBuffers;
// }



namespace Tiger.Schema.Shaders.DESTINY2_SHADOWKEEP_2601
{
    public class Material : Tag<SMaterial_SK>, IMaterial
    {
        public FileHash FileHash => Hash;
        public ShaderBytecode VertexShader => _tag.VertexShader;
        public ShaderBytecode PixelShader => _tag.PixelShader;
        public FileHash PSVector4Container => _tag.PSVector4Container;
        public DynamicArray<D2Class_09008080> Unk90 => _tag.Unk68;
        public DynamicArray<Vec4> UnkA0 => _tag.Unk78;
        public DynamicArray<Vec4> UnkC0 => _tag.Unk98;
        public DynamicArray<D2Class_09008080> Unk2D0 => _tag.Unk2E8;
        public DynamicArray<Vec4> Unk2E0 => _tag.Unk2F8;
        public DynamicArray<Vec4> Unk300 => _tag.Unk310;

        public IEnumerable<STextureTag> EnumerateVSTextures()
        {
            foreach (STextureTag texture in _tag.VSTextures)
            {
                yield return texture;
            }
        }

        public IEnumerable<STextureTag> EnumeratePSTextures()
        {
            foreach (STextureTag texture in _tag.PSTextures)
            {
                yield return texture;
            }
        }

        public Material(FileHash fileHash) : base(fileHash)
        {
        }
    }
}

namespace Tiger.Schema.Shaders.DESTINY2_WITCHQUEEN_6307
{
    public class Material : Tag<SMaterial_WQ>, IMaterial
    {
        public FileHash FileHash => Hash;
        public ShaderBytecode VertexShader => _tag.VertexShader;
        public ShaderBytecode PixelShader => _tag.PixelShader;
        public FileHash PSVector4Container => _tag.PSVector4Container;
        public DynamicArray<D2Class_09008080> Unk90 => _tag.Unk90;
        public DynamicArray<Vec4> UnkA0 => _tag.UnkA0;
        public DynamicArray<Vec4> UnkC0 => _tag.UnkC0;
        public DynamicArray<D2Class_09008080> Unk2D0 => _tag.Unk2D0;
        public DynamicArray<Vec4> Unk2E0 => _tag.Unk2E0;
        public DynamicArray<Vec4> Unk300 => _tag.Unk300;

        public IEnumerable<STextureTag> EnumerateVSTextures()
        {
            foreach (STextureTag64 texture in _tag.VSTextures)
            {
                yield return texture;
            }
        }

        public IEnumerable<STextureTag> EnumeratePSTextures()
        {
            foreach (STextureTag64 texture in _tag.PSTextures)
            {
                yield return texture;
            }
        }

        public Material(FileHash fileHash) : base(fileHash)
        {
        }

        // public void SaveComputeShader(string saveDirectory)
        // {
        //     Directory.CreateDirectory($"{saveDirectory}");
        //     if (_tag.ComputeShader != null && !File.Exists($"{saveDirectory}/CS_{Hash}.usf"))
        //     {
        //         string hlsl = Decompile(_tag.ComputeShader.GetBytecode(), "cs");
        //         string usf = new UsfConverter().HlslToUsf(this, hlsl, false);
        //         if (usf != String.Empty)
        //         {
        //             try
        //             {
        //                 File.WriteAllText($"{saveDirectory}/CS_{Hash}.usf", usf);
        //                 Console.WriteLine($"Saved compute shader {Hash}");
        //             }
        //             catch (IOException)  // threading error
        //             {
        //             }
        //         }
        //     }
        // }
    }
}
