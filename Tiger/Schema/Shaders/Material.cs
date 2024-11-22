﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using Arithmic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tiger.Exporters;
using Tiger.Schema;
using static Tiger.Schema.DirectXSampler;
using static Tiger.Schema.RenderStates;

namespace Tiger.Schema.Shaders
{
    public interface IMaterial : ISchema
    {
        public FileHash FileHash { get; }
        public uint Unk08 { get; }
        public uint Unk10 { get; }
        public uint Unk0C { get; } //Seems to be backface culling
        public int UnkD4 { get; } // Determines if the vertex shader uses vertex animation..?

        // Vertex
        public ShaderBytecode? VertexShader { get; }
        public FileHash VSVector4Container { get; }
        public List<DirectXSampler> VS_Samplers { get; }
        public DynamicArray<D2Class_09008080> VS_TFX_Bytecode { get; }
        public DynamicArray<Vec4> VS_TFX_Bytecode_Constants { get; }
        public DynamicArray<Vec4> VS_CBuffers { get; }
        public IEnumerable<STextureTag> EnumerateVSTextures();

        // Pixel
        public ShaderBytecode? PixelShader { get; }
        public FileHash PSVector4Container { get; }
        public List<DirectXSampler> PS_Samplers { get; }
        public DynamicArray<D2Class_09008080> PS_TFX_Bytecode { get; }
        public DynamicArray<Vec4> PS_TFX_Bytecode_Constants { get; }
        public DynamicArray<Vec4> PS_CBuffers { get; }
        public IEnumerable<STextureTag> EnumeratePSTextures();

        // Compute
        public IEnumerable<STextureTag> EnumerateCSTextures();
        public ShaderBytecode? ComputeShader { get; }

        public IEnumerable<TfxScope> EnumerateScopes();
        public StateSelection RenderStates { get; }

        public static object _lock = new object();
        private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        public string Decompile(byte[] shaderBytecode, string name, string savePath = "hlsl_temp")
        {
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
                //Console.WriteLine(Path.GetFullPath(binPath));
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

        public void SavePixelShader(string saveDirectory, bool isTerrain = false)
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
                return;

            if (PixelShader != null && PixelShader.Hash.IsValid())
            {
                string pixel = Decompile(PixelShader.GetBytecode(), $"ps{PixelShader.Hash}");
                string vertex = Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}");
                string usf = _config.GetUnrealInteropEnabled() ? new UsfConverter().HlslToUsf(this, pixel, false) : "";
                string vfx = _config.GetS2ShaderExportEnabled() ? new S2ShaderConverter().HlslToVfx(this, pixel, vertex) : "";

                try
                {
                    if (usf != String.Empty && !File.Exists($"{saveDirectory}/Unreal/PS_{FileHash}.usf"))
                    {
                        Directory.CreateDirectory($"{saveDirectory}/Unreal");
                        File.WriteAllText($"{saveDirectory}/Unreal/PS_{FileHash}.usf", usf);
                    }
                    if (vfx != String.Empty)
                    {
                        Directory.CreateDirectory($"{saveDirectory}/Source2");
                        Directory.CreateDirectory($"{saveDirectory}/Source2/materials");

                        File.WriteAllText($"{saveDirectory}/Source2/PS_{PixelShader.Hash}.shader", vfx);
                        if (!isTerrain)
                            Source2Handler.SaveVMAT(saveDirectory, FileHash, this);
                    }
                }
                catch (IOException e)  // threading error
                {
                    Log.Error(e.Message);
                }
            }
        }

        // TODO: do this properly
        public void SaveVertexShader(string saveDirectory)
        {
            //if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            //    return;

            //Directory.CreateDirectory($"{saveDirectory}");
            //if (VertexShader != null && VertexShader.Hash.IsValid())
            //{
            //    string hlsl = Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}");
            //    string usf = _config.GetUnrealInteropEnabled() ? new UsfConverter().HlslToUsf(this, hlsl, true) : "";
            //    if (usf != String.Empty)
            //    {
            //        try
            //        {
            //            File.WriteAllText($"{saveDirectory}/VS_{FileHash}.usf", usf);
            //            //Console.WriteLine($"Saved vertex shader {FileHash}");
            //        }
            //        catch (IOException) // threading error
            //        {
            //        }
            //    }
            //}
        }

        // TODO: Remove material data from cfg and use this instead, cfg is too cluttered 
        public void SaveMaterial(string saveDirectory)
        {
            var hlslPath = $"{saveDirectory}/Shaders/Raw";
            var texturePath = $"{saveDirectory}/Textures";
            Directory.CreateDirectory(hlslPath);
            Directory.CreateDirectory(texturePath);

            JsonMaterial material = new()
            {
                Scopes = EnumerateScopes().ToList(),
                Externs = GetExterns().ToList(),
                RenderStates = RenderStates
            };
            if (PixelShader != null)
            {
                Decompile(PixelShader.GetBytecode(), $"ps{PixelShader.Hash}", hlslPath);
                SavePixelShader($"{saveDirectory}/Shaders/");

                ShaderDetails psCB = new ShaderDetails();
                psCB.CBuffers = GetCBuffer0();
                psCB.Bytecode = PS_TFX_Bytecode.Select(x => x.Value).ToList();
                psCB.Constants = PS_TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                psCB.Textures = new();
                foreach (var texture in EnumeratePSTextures())
                {
                    psCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.Texture.Hash,
                        Colorspace = texture.Texture.IsSrgb() ? "Srgb" : "Non-Color",
                        Dimension = texture.Texture.GetDimension().GetEnumDescription(),
                        Format = texture.Texture.TagData.GetFormat().ToString()
                    });
                }

                psCB.TileTextureDetails = new();
                psCB.Samplers = new();
                foreach (var item in PS_Samplers.Select((sampler, index) => new { sampler, index }))
                {
                    if (item.sampler.Hash.GetFileMetadata().Type != 34)
                    {
                        var tex = FileResourcer.Get().GetFile<Texture>(item.sampler.Hash);
                        psCB.TileTextureDetails.Add(new()
                        {
                            Hash = item.sampler.Hash,
                            Width = tex.TagData.Width,
                            Height = tex.TagData.Height,
                            Depth = tex.TagData.Depth,
                            ArraySize = tex.TagData.ArraySize,
                            TileCount = tex.TagData.TileCount,
                            TilingScaleOffset = tex.TagData.TilingScaleOffset
                        });
                    }
                    else
                    {
                        psCB.Samplers.TryAdd(item.index + 1, item.sampler.GetSampler());
                    }
                }

                material.Material.TryAdd(JsonMaterial.ShaderStage.Pixel, psCB);
            }

            if (VertexShader != null)
            {
                Decompile(VertexShader.GetBytecode(), $"vs{VertexShader.Hash}", hlslPath);
                SaveVertexShader($"{saveDirectory}/Shaders/");

                ShaderDetails vsCB = new ShaderDetails();
                vsCB.CBuffers = GetCBuffer0(true);
                vsCB.Bytecode = VS_TFX_Bytecode.Select(x => x.Value).ToList();
                vsCB.Constants = VS_TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                vsCB.Textures = new();
                foreach (var texture in EnumerateVSTextures())
                {
                    vsCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.Texture.Hash,
                        Colorspace = texture.Texture.IsSrgb() ? "Srgb" : "Non-Color",
                        Dimension = texture.Texture.GetDimension().GetEnumDescription(),
                        Format = texture.Texture.TagData.GetFormat().ToString()
                    });
                }

                material.Material.TryAdd(JsonMaterial.ShaderStage.Vertex, vsCB);
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


            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            File.WriteAllText($"{saveDirectory}/{FileHash}.json", JsonConvert.SerializeObject(material, jsonSettings));
        }

        public List<Vector4> GetVec4Container(bool vs = false)
        {
            List<Vector4> data = new();
            TigerFile container = new(vs ? VSVector4Container.GetReferenceHash() : PSVector4Container.GetReferenceHash());
            byte[] containerData = container.GetData();

            for (int i = 0; i < containerData.Length / 16; i++)
            {
                data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
            }

            return data;
        }

        public TfxBytecodeInterpreter GetPSBytecode()
        {
            return new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(PS_TFX_Bytecode));
        }

        public TfxBytecodeInterpreter GetVSBytecode()
        {
            return new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(VS_TFX_Bytecode));
        }

        public List<TfxExtern> GetExterns()
        {
            return Externs.GetExterns(this);
        }

        public List<Vector4> GetCBuffer0(bool bVS = false)
        {
            List<Vector4> data = new();
            if (bVS)
            {
                if (VSVector4Container.IsValid())
                {
                    data = GetVec4Container(true);
                }
                else
                {
                    foreach (var vec in VS_CBuffers)
                    {
                        data.Add(vec.Vec);
                    }
                }
            }
            else
            {
                if (PSVector4Container.IsValid())
                {
                    data = GetVec4Container();
                }
                else
                {
                    foreach (var vec in PS_CBuffers)
                    {
                        data.Add(vec.Vec);
                    }
                }
            }
            return data;
        }

        private struct JsonMaterial
        {
            public JsonMaterial() { }

            public List<TfxScope> Scopes { get; set; } = new();
            public List<TfxExtern> Externs { get; set; } = new();
            public StateSelection RenderStates { get; set; } = new();
            public Dictionary<ShaderStage, ShaderDetails> Material { get; set; } = new();

            public enum ShaderStage
            {
                Vertex,
                Pixel
            }
        }

        private struct ShaderDetails
        {
            public ShaderDetails() { }

            public Dictionary<int, TextureDetails> Textures { get; set; } = new();
            public List<Vector4> CBuffers { get; set; } = new();
            public List<byte> Bytecode { get; set; } = new();
            public List<Vector4> Constants { get; set; } = new();
            public Dictionary<int, D3D11_SAMPLER_DESC> Samplers { get; set; } = new();
            public List<TileTextureDetails> TileTextureDetails { get; set; } = new();
        }

        private struct TextureDetails
        {
            public string Hash;
            public string Dimension;
            public string Format;
            public string Colorspace;
        }

        private struct TileTextureDetails
        {
            public string Hash;
            public int Width;
            public int Height;
            public int Depth;
            public int ArraySize;
            public int TileCount;
            public Vector4 TilingScaleOffset;
        }
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct StateSelection
{
    private int inner;

    public BungieBlendDesc? Blend => BlendState() != -1 ? RenderStates.BlendStates[BlendState()] : null;
    public BungieRasterizerDesc? Rasterizer => RasterizerState() != -1 ? RenderStates.RasterizerStates[RasterizerState()] : null;
    public BungieDepthBiasDesc? DepthBias => DepthBiasState() != -1 ? RenderStates.DepthBiasStates[DepthBiasState()] : null;

    public StateSelection(int value)
    {
        this.inner = value;
    }

    public int BlendState()
    {
        if ((inner & 0x80) != 0)
        {
            return (inner & 0x7F);
        }
        return -1;
    }

    public int DepthStencilState()
    {
        int v = (inner >> 8) & 0xFF;
        if ((v & 0x80) != 0)
        {
            return (v & 0x7F);
        }
        return -1;
    }

    public int RasterizerState()
    {
        int v = (inner >> 16) & 0xFF;
        if ((v & 0x80) != 0)
        {
            return (v & 0x7F);
        }
        return -1;
    }

    public int DepthBiasState()
    {
        int v = (inner >> 24) & 0xFF;
        if ((v & 0x80) != 0)
        {
            return (v & 0x7F);
        }
        return -1;
    }

    public override string ToString()
    {
        if (BlendState() != -1)
        {
            var blendState = RenderStates.BlendStates[BlendState()];
            return $"Blend State {BlendState()}:\n" +
                $"\tIsBlendEnabled: {blendState.BlendDesc.IsBlendEnabled}\n" +
                $"\tSourceBlend: {blendState.BlendDesc.SourceBlend}\n" +
                $"\tDestinationBlend: {blendState.BlendDesc.DestinationBlend}\n" +
                $"\tBlendOperation: {blendState.BlendDesc.BlendOperation}\n" +
                $"\tSourceAlphaBlend: {blendState.BlendDesc.SourceAlphaBlend}\n" +
                $"\tDestinationAlphaBlend: {blendState.BlendDesc.DestinationAlphaBlend}\n" +
                $"\tAlphaBlendOperation: {blendState.BlendDesc.AlphaBlendOperation}\n" +
                $"\tRenderTargetWriteMask: {blendState.BlendDesc.RenderTargetWriteMask}\n";
        }
        return "";
    }
}

namespace Tiger.Schema.Shaders.DESTINY1_RISE_OF_IRON
{
    public class Material : Tag<SMaterial_ROI>, IMaterial
    {
        public FileHash FileHash => Hash;
        public uint Unk08 => _tag.Unk08;
        public uint Unk10 => _tag.Unk10;
        public uint Unk0C => _tag.Unk0C;
        public int UnkD4 => 0;

        // Leaving shaders null until they (if ever) can be decompiled to hlsl
        public ShaderBytecode VertexShader => _tag.VertexShader; // null;
        public ShaderBytecode PixelShader => _tag.PixelShader; // null;
        public ShaderBytecode ComputeShader => null;
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
        public StateSelection RenderStates => _tag.RenderStates;

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
            yield return new();
        }

        public IEnumerable<TfxScope> EnumerateScopes()
        {
            foreach (Enum scopeBit in EnumExtensions.GetFlags(_tag.UsedScopes))
            {
                if (Enum.TryParse(scopeBit.ToString(), out TfxScope scope))
                    yield return scope;
                else
                    throw new Exception($"Unknown scope value {scope.ToString()}");
            }
        }

        public Material(FileHash fileHash) : base(fileHash)
        {
        }
    }
}

namespace Tiger.Schema.Shaders.DESTINY2_SHADOWKEEP_2601
{
    public class Material : Tag<SMaterial_SK>, IMaterial
    {
        public FileHash FileHash => Hash;
        public uint Unk08 => _tag.Unk08;
        public uint Unk10 => _tag.Unk10;
        public uint Unk0C => _tag.Unk0C;
        public int UnkD4 => _tag.UnkBC;

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
        public StateSelection RenderStates => _tag.RenderStates;

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

        public IEnumerable<TfxScope> EnumerateScopes()
        {
            foreach (Enum scopeBit in EnumExtensions.GetFlags(_tag.UsedScopes))
            {
                if (Enum.TryParse(scopeBit.ToString(), out TfxScope scope))
                    yield return scope;
                else
                    throw new Exception($"Unknown scope value {scope.ToString()}");
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
        public int UnkD4 => _tag.UnkD4;

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
        public StateSelection RenderStates => _tag.RenderStates;

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

        public IEnumerable<TfxScope> EnumerateScopes()
        {
            foreach (Enum scopeBit in EnumExtensions.GetFlags(_tag.UsedScopes))
            {
                if (Enum.TryParse(scopeBit.ToString(), out TfxScope scope))
                    yield return scope;
                else
                    throw new Exception($"Unknown scope value {scope.ToString()}");
            }
        }

        public Material(FileHash fileHash) : base(fileHash)
        {
        }
    }
}
