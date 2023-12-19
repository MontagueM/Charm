using System.Diagnostics;
using System.Text;
using Tiger.Exporters;

namespace Tiger.Schema.Shaders
{
    public struct TextureView
    {
        public string Dimension;
        public string Type;
        public string Variable;
        public int Index;
    }

    public struct Buffer
    {
        public string Variable;
        public string Type;
        public int Index;
    }

    public struct Cbuffer
    {
        public string Variable;
        public string Type;
        public int Count;
        public int Index;
    }

    public struct Input
    {
        public string Variable;
        public string Type;
        public int Index;
        public string Semantic;
    }

    public struct Output
    {
        public string Variable;
        public string Type;
        public int Index;
        public string Semantic;
    }

    public interface IMaterial : ISchema
    {
        public FileHash FileHash { get; }
        public uint Unk08 { get; }
        public uint Unk10 { get; }
        public uint Unk0C { get; } //Seems to be backface culling
        public IEnumerable<STextureTag> EnumerateVSTextures();
        public IEnumerable<STextureTag> EnumeratePSTextures();
        public IEnumerable<STextureTag> EnumerateCSTextures();
        public ShaderBytecode? VertexShader { get; }
        public ShaderBytecode? PixelShader { get; }
        public ShaderBytecode? ComputeShader { get; }
        public FileHash PSVector4Container { get; }
        public FileHash VSVector4Container { get; }
        public List<DirectXSampler> PS_Samplers { get; }
        public List<DirectXSampler> VS_Samplers { get; }
        public DynamicArray<D2Class_09008080> VS_TFX_Bytecode { get; }
        public DynamicArray<Vec4> VS_TFX_Bytecode_Constants { get; }
        public DynamicArray<Vec4> VS_CBuffers { get; }
        public DynamicArray<D2Class_09008080> PS_TFX_Bytecode { get; }
        public DynamicArray<Vec4> PS_TFX_Bytecode_Constants { get; }
        public DynamicArray<Vec4> PS_CBuffers { get; }
        public static object _lock = new object();
        private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        public string Decompile(byte[] shaderBytecode, string name, string savePath = "hlsl_temp")
        {
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
                Console.WriteLine(Path.GetFullPath(binPath));
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
                    }
                    catch (IOException)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            return hlsl;
        }

        public void SaveShaders(string saveDirectory, bool isTerrain = false)
        {
            Directory.CreateDirectory($"{saveDirectory}/Shaders");
            if (PixelShader != null && PixelShader.Hash.IsValid())
            {
                string pixel = Decompile(PixelShader.GetBytecode(), $"ps{PixelShader.Hash}");
                string vertex = Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}");
                string vfx = new S2ShaderConverter().HlslToVfx(this, pixel, vertex, isTerrain);

                try
                {
                    if (vfx != String.Empty)
                    {
                        File.WriteAllText($"{saveDirectory}/Shaders/PS_{PixelShader.Hash}.shader", vfx);
                        SBoxHandler.SaveVMAT(saveDirectory, FileHash, this, isTerrain);
                    }
                }
                catch (IOException)  // threading error
                {
                }
            }
        }

        public void SaveVertexShader(string saveDirectory)
        {
            //Directory.CreateDirectory($"{saveDirectory}");
            //if (VertexShader != null && VertexShader.Hash.IsValid())
            //{
            //    string hlsl = Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}");
            //    if (usf != String.Empty)
            //    {
            //        try
            //        {
            //            File.WriteAllText($"{saveDirectory}/VS_{FileHash}.usf", usf);
            //            Console.WriteLine($"Saved vertex shader {FileHash}");
            //        }
            //        catch (IOException) // threading error
            //        {
            //        }
            //    }
            //}
        }

        //Only useful for saving single material from DevView or MaterialView, better control for output compared to scene system
        public void SaveMaterial(string saveDirectory)
        {
            var hlslPath = $"{saveDirectory}/Raw_Shaders";
            var texturePath = $"{saveDirectory}/Textures";
            Directory.CreateDirectory(hlslPath);
            Directory.CreateDirectory(texturePath);

            if (PixelShader != null)
            {
                Decompile(PixelShader.GetBytecode(), $"ps{PixelShader.Hash}", hlslPath);
                SaveShaders($"{saveDirectory}");
            }
            if (VertexShader != null)
            {
                Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}", hlslPath);
                SaveVertexShader($"{saveDirectory}");
            }

            foreach (STextureTag texture in EnumerateVSTextures())
            {
                if (texture.Texture == null)
                    continue;

                texture.Texture.SavetoFile($"{saveDirectory}/Textures/{texture.Texture.Hash}");
            }
            foreach (STextureTag texture in EnumeratePSTextures())
            {
                if (texture.Texture == null)
                    continue;

                texture.Texture.SavetoFile($"{saveDirectory}/Textures/{texture.Texture.Hash}");
            }
        }

        public List<Vector4> GetVec4Container(FileHash containerHash)
        {
            List<Vector4> data = new();
            TigerFile container = new(containerHash);
            byte[] containerData = container.GetData();

            for (int i = 0; i < containerData.Length / 16; i++)
            {
                data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
            }

            return data;
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
        public uint Unk08 => _tag.Unk08;
        public uint Unk10 => _tag.Unk10;
        public uint Unk0C => _tag.Unk0C;
        public ShaderBytecode VertexShader => _tag.VertexShader;
        public ShaderBytecode PixelShader => _tag.PixelShader;
        public ShaderBytecode ComputeShader => _tag.ComputeShader;
        public FileHash PSVector4Container => _tag.PSVector4Container;
        public FileHash VSVector4Container => _tag.VSVector4Container;
        public DynamicArray<D2Class_09008080> VS_TFX_Bytecode => _tag.VS_TFX_Bytecode;
        public DynamicArray<Vec4> VS_TFX_Bytecode_Constants => _tag.VS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> VS_CBuffers => _tag.VS_CBuffers;
        public DynamicArray<D2Class_09008080> PS_TFX_Bytecode => _tag.PS_TFX_Bytecode;
        public DynamicArray<Vec4> PS_TFX_Bytecode_Constants => _tag.PS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> PS_CBuffers => _tag.PS_CBuffers;
        public List<DirectXSampler> VS_Samplers => _tag.VS_Samplers.Select(x => x.Samplers).ToList();
        public List<DirectXSampler> PS_Samplers => _tag.PS_Samplers.Select(x => x.Samplers).ToList();

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

        public IEnumerable<STextureTag> EnumerateCSTextures()
        {
            foreach (STextureTag texture in _tag.CSTextures)
            {
                yield return texture;
            }
        }

        public Material(FileHash fileHash) : base(fileHash)
        {
        }
    }
}

namespace Tiger.Schema.Shaders.DESTINY2_BEYONDLIGHT_3402
{
    public class Material : Tag<SMaterial_BL>, IMaterial
    {
        public FileHash FileHash => Hash;
        public uint Unk08 => _tag.Unk08;
        public uint Unk10 => _tag.Unk10;
        public uint Unk0C => _tag.Unk0C;
        public ShaderBytecode VertexShader => _tag.VertexShader;
        public ShaderBytecode PixelShader => _tag.PixelShader;
        public ShaderBytecode ComputeShader => _tag.ComputeShader;
        public FileHash PSVector4Container => _tag.PSVector4Container;
        public FileHash VSVector4Container => _tag.VSVector4Container;
        public DynamicArray<D2Class_09008080> VS_TFX_Bytecode => _tag.VS_TFX_Bytecode;
        public DynamicArray<Vec4> VS_TFX_Bytecode_Constants => _tag.VS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> VS_CBuffers => _tag.VS_CBuffers;
        public DynamicArray<D2Class_09008080> PS_TFX_Bytecode => _tag.PS_TFX_Bytecode;
        public DynamicArray<Vec4> PS_TFX_Bytecode_Constants => _tag.PS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> PS_CBuffers => _tag.PS_CBuffers;
        public List<DirectXSampler> VS_Samplers => _tag.VS_Samplers.Select(s => s.Samplers).ToList();
        public List<DirectXSampler> PS_Samplers => _tag.PS_Samplers.Select(s => s.Samplers).ToList();

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

        public IEnumerable<STextureTag> EnumerateCSTextures()
        {
            foreach (STextureTag64 texture in _tag.CSTextures)
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
        public uint Unk08 => _tag.Unk08;
        public uint Unk10 => _tag.Unk10;
        public uint Unk0C => _tag.Unk0C;
        public ShaderBytecode VertexShader => _tag.VertexShader;
        public ShaderBytecode PixelShader => _tag.PixelShader;
        public ShaderBytecode ComputeShader => _tag.ComputeShader;
        public FileHash PSVector4Container => _tag.PSVector4Container;
        public FileHash VSVector4Container => _tag.VSVector4Container;
        public DynamicArray<D2Class_09008080> VS_TFX_Bytecode => _tag.VS_TFX_Bytecode;
        public DynamicArray<Vec4> VS_TFX_Bytecode_Constants => _tag.VS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> VS_CBuffers => _tag.VS_CBuffers;
        public DynamicArray<D2Class_09008080> PS_TFX_Bytecode => _tag.PS_TFX_Bytecode;
        public DynamicArray<Vec4> PS_TFX_Bytecode_Constants => _tag.PS_TFX_Bytecode_Constants;
        public DynamicArray<Vec4> PS_CBuffers => _tag.PS_CBuffers;
        public List<DirectXSampler> VS_Samplers => _tag.VS_Samplers.Select(x => x.Samplers).ToList();
        public List<DirectXSampler> PS_Samplers => _tag.PS_Samplers.Select(x => x.Samplers).ToList();

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

        public IEnumerable<STextureTag> EnumerateCSTextures()
        {
            foreach (STextureTag64 texture in _tag.CSTextures)
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
