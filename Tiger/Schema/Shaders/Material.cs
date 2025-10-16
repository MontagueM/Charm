using System.Runtime.InteropServices;
using System.Text;
using Arithmic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tiger.Exporters;
using Tiger.Schema;
using static Tiger.Schema.DirectXSampler;
using static Tiger.Schema.RenderStates;

namespace Tiger.Schema.Shaders
{
    public class Material : Tag<SMaterial>
    {
        public Material(FileHash fileHash) : base(fileHash)
        {
        }

        // Currently used for shader/material conversion purposes
        public TfxRenderStage RenderStage { get; set; } = TfxRenderStage.GenerateGbuffer;

        public StateSelection RenderStates => _tag.RenderStates;

        public SMaterialShader Pixel => _tag.Pixel.Value;
        public List<DirectXSampler> PSSamplers => _tag.Pixel.Value.EnumerateSamplers().ToList();

        public SMaterialShader Vertex => _tag.Vertex.Value;
        public List<DirectXSampler> VSSamplers => _tag.Vertex.Value.EnumerateSamplers().ToList();

        public SMaterialShader Compute => _tag.Compute.Value;

        public IEnumerable<TfxScope> EnumerateScopes()
        {
            foreach (Enum scopeBit in EnumExtensions.GetFlags(_tag.GetScopeBits()))
            {
                if (Enum.TryParse(scopeBit.ToString(), out TfxScope scope))
                    yield return scope;
                else
                    throw new Exception($"Unknown scope value {scope.ToString()}");
            }
        }

        private static ConfigSubsystem _config = TigerInstance.GetSubsystem<ConfigSubsystem>();

        public void SavePixelShader(string saveDirectory, bool fromMaterialViewer = false)
        {
            if (Strategy.IsD1())
                return;

            // Dont export the hlsl if none of the shader related settings are enabled
            // but force export if we're saving from the material viewer
            if (!_config.GetSaveShaderHLSL() && !_config.GetSBoxExportEnabled() && !fromMaterialViewer)
                return;

            if (Pixel.Shader != null && Pixel.Shader.Hash.IsValid())
            {
                try
                {
                    string pixel = Pixel.Shader.Decompile($"ps{Pixel.Shader.Hash}");
                    Directory.CreateDirectory($"{saveDirectory}/Shaders/HLSL");
                    File.WriteAllText($"{saveDirectory}/Shaders/HLSL/PS_{Pixel.Shader.Hash}.hlsl", pixel);

                    if (_config.GetUnrealInteropEnabled())
                    {
                        string usf = new UsfConverter().HlslToUsf(this, false);
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Unreal");
                        File.WriteAllText($"{saveDirectory}/Shaders/Unreal/PS_{Pixel.Shader.Hash}.usf", usf);
                    }

                    if (_config.GetSBoxExportEnabled())
                    {
                        string vfx = new S2ShaderConverter().HlslToVfx(this);
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Source2");
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Source2/materials");

                        if (!File.Exists($"{saveDirectory}/Shaders/TFXFunctions.hlsl"))
                            File.Copy("./Exporters/Shaders/TFXFunctions.hlsl", $"{saveDirectory}/Shaders/TFXFunctions.hlsl");

                        FileHash hash = (Pixel.GetBytecode().CanInlineBytecode() || RenderStage == TfxRenderStage.WaterReflection) ? Hash : Pixel.Shader.Hash;
                        File.WriteAllText($"{saveDirectory}/Shaders/Source2/PS_{hash}.shader", vfx);
                        Source2Handler.SaveVMAT(saveDirectory, Hash, this);
                    }
                }
                catch (IOException e)  // threading error
                {
                    Log.Error(e.Message);
                }
            }
        }

        // TODO: do this properly
        public void SaveVertexShader(string saveDirectory, bool fromMaterialViewer = false)
        {
            if (Strategy.IsD1())
                return;

            // Dont export the hlsl if none of the shader related settings are enabled
            // but force export if we're saving from the material viewer
            if (!_config.GetSaveShaderHLSL() && !_config.GetSBoxExportEnabled() && !fromMaterialViewer)
                return;

            if (Vertex.Shader != null && Vertex.Shader.Hash.IsValid())
            {
                try
                {
                    string vertex = Vertex.Shader.Decompile($"vs{Vertex.Shader.Hash}");
                    Directory.CreateDirectory($"{saveDirectory}/HLSL");
                    File.WriteAllText($"{saveDirectory}/HLSL/VS_{Vertex.Shader.Hash}.hlsl", vertex);
                }
                catch (IOException e)  // threading error
                {
                    Log.Error(e.Message);
                }
            }
        }

        public void Export(string saveDirectory, bool fromMaterialViewer = false)
        {
            string texturePath = $"{saveDirectory}/Textures";
            string materialPath = $"{saveDirectory}/Materials";
            Directory.CreateDirectory(texturePath);
            Directory.CreateDirectory(materialPath);

            JsonMaterial material = new()
            {
                Hash = Hash,
                Scopes = EnumerateScopes().ToList(),
                Externs = GetExterns(),
                RenderStates = RenderStates
            };

            if (Pixel.Shader != null)
            {
                SavePixelShader($"{saveDirectory}", fromMaterialViewer);

                ShaderDetails psCB = new();
                psCB.Hash = Pixel.Shader.Hash;
                psCB.CBuffers = Pixel.GetCBuffer0();
                psCB.Bytecode = Pixel.TFX_Bytecode.Select(x => x.Value).ToList();
                psCB.Constants = Pixel.TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                var bytecode = new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(Pixel.TFX_Bytecode));
                foreach (var objectChannel in bytecode.Opcodes.Where(x => x.op == TfxBytecode.PushObjectChannelVector))
                {
                    var hash = new StringHash(((PushObjectChannelVectorData)objectChannel.data).hash);
                    material.UsedChannelNames.TryAdd(hash, GlobalStrings.Get().GetString(hash));
                }

                psCB.Textures = new();
                foreach (STextureTag texture in Pixel.EnumerateTextures())
                {
                    if (texture.Texture is null)
                        continue;

                    psCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.Texture.Hash,
                        Colorspace = texture.Texture.IsSrgb() ? "sRGB" : "Non-Color",
                        Dimension = texture.Texture.GetDimension().GetEnumDescription(),
                        Format = texture.Texture.TagData.GetFormat().ToString()
                    });

                    string savePath = $"{saveDirectory}/Textures/{texture.Texture.Hash}";
                    if (File.Exists($"{savePath}.{TextureExtractor.GetExtension(_config.GetOutputTextureFormat())}"))
                        continue;

                    texture.Texture.SavetoFile(savePath);
                }

                psCB.TileTextureDetails = new();
                psCB.Samplers = new();
                foreach (var item in Pixel.Samplers.Select((sampler, index) => new { sampler, index }))
                {
                    DirectXSampler? sampler = item.sampler.GetSampler();
                    if (sampler is null)
                        continue;

                    if (sampler.Hash.GetFileMetadata().Type != 34)
                    {
                        Texture? tex = FileResourcer.Get().GetFile<Texture>(sampler.Hash);
                        if (tex is null)
                            continue;

                        psCB.TileTextureDetails.TryAdd(item.index, new()
                        {
                            Hash = sampler.Hash,
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
                        psCB.Samplers.TryAdd(item.index + 1, sampler.Sampler);
                    }
                }

                material.Material.TryAdd(JsonMaterial.ShaderStage.Pixel, psCB);
            }

            if (Vertex.Shader != null)
            {
                SaveVertexShader($"{saveDirectory}/Shaders/", fromMaterialViewer);

                ShaderDetails vsCB = new();
                vsCB.Hash = Vertex.Shader.Hash;
                vsCB.CBuffers = Vertex.GetCBuffer0();
                vsCB.Bytecode = Vertex.TFX_Bytecode.Select(x => x.Value).ToList();
                vsCB.Constants = Vertex.TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                var bytecode = new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(Vertex.TFX_Bytecode));
                foreach (var objectChannel in bytecode.Opcodes.Where(x => x.op == TfxBytecode.PushObjectChannelVector))
                {
                    var hash = new StringHash(((PushObjectChannelVectorData)objectChannel.data).hash);
                    material.UsedChannelNames.TryAdd(hash, GlobalStrings.Get().GetString(hash));
                }

                vsCB.Textures = new();
                foreach (STextureTag texture in Vertex.EnumerateTextures())
                {
                    if (texture.Texture is null)
                        continue;

                    vsCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.Texture.Hash,
                        Colorspace = texture.Texture.IsSrgb() ? "Srgb" : "Non-Color",
                        Dimension = texture.Texture.GetDimension().GetEnumDescription(),
                        Format = texture.Texture.TagData.GetFormat().ToString()
                    });

                    string savePath = $"{saveDirectory}/Textures/{texture.Texture.Hash}";
                    if (File.Exists($"{savePath}.{TextureExtractor.GetExtension(_config.GetOutputTextureFormat())}"))
                        continue;

                    texture.Texture.SavetoFile($"{saveDirectory}/Textures/{texture.Texture.Hash}");
                }

                vsCB.TileTextureDetails = new();
                vsCB.Samplers = new();
                foreach (var item in Vertex.Samplers.Select((sampler, index) => new { sampler, index }))
                {
                    DirectXSampler? sampler = item.sampler.GetSampler();
                    if (sampler is null)
                        continue;

                    if (sampler.Hash.GetFileMetadata().Type != 34)
                    {
                        Texture? tex = FileResourcer.Get().GetFile<Texture>(sampler.Hash);
                        if (tex is null)
                            continue;

                        vsCB.TileTextureDetails.TryAdd(item.index, new()
                        {
                            Hash = sampler.Hash,
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
                        vsCB.Samplers.TryAdd(item.index + 1, sampler.Sampler);
                    }
                }

                material.Material.TryAdd(JsonMaterial.ShaderStage.Vertex, vsCB);
            }

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            File.WriteAllText($"{materialPath}/{Hash}.json", JsonConvert.SerializeObject(material, jsonSettings));
        }

        public List<TfxExtern> GetExterns()
        {
            List<TfxData> opcodes = Pixel.GetBytecode().Opcodes;
            opcodes.AddRange(Vertex.GetBytecode().Opcodes);

            var list = new List<TfxExtern>();
            foreach (TfxData op in opcodes.Where(x => x.op.ToString().Contains("Extern")))
            {
                if (!list.Contains(op.data.extern_))
                    list.Add(op.data.extern_);
            }

            return list;
        }

        private struct JsonMaterial
        {
            public JsonMaterial() { }

            public string Hash { get; set; }
            public List<TfxScope> Scopes { get; set; } = new();
            public List<TfxExtern> Externs { get; set; } = new();
            public StateSelection RenderStates { get; set; } = new();
            public Dictionary<ShaderStage, ShaderDetails> Material { get; set; } = new();
            public Dictionary<uint, string> UsedChannelNames { get; set; } = new();

            public enum ShaderStage
            {
                Vertex,
                Pixel
            }
        }

        private struct ShaderDetails
        {
            public ShaderDetails() { }

            public string Hash { get; set; }
            public Dictionary<int, TextureDetails> Textures { get; set; } = new();
            public List<Vector4> CBuffers { get; set; } = new();
            public List<byte> Bytecode { get; set; } = new();
            public List<Vector4> Constants { get; set; } = new();
            public Dictionary<int, D3D11_SAMPLER_DESC> Samplers { get; set; } = new();
            public Dictionary<int, TileTextureDetails> TileTextureDetails { get; set; } = new();
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
        StringBuilder states = new();
        if (BlendState() != -1)
        {
            BungieBlendDesc blendState = RenderStates.BlendStates[BlendState()];
            states.AppendLine($"Blend State {BlendState()}:\n {blendState.ToString()}");
        }
        if (DepthStencilState() != -1)
        {
            BungieDepthStencilDesc dsState = RenderStates.DepthStencilStates[DepthStencilState()];
            states.AppendLine($"Depth Stencil State {DepthStencilState()}:\n {dsState.ToString()}");
        }
        if (RasterizerState() != -1)
        {
            BungieRasterizerDesc rasterizer = RenderStates.RasterizerStates[RasterizerState()];
            states.AppendLine($"Rasterizer State {RasterizerState()}:\n {rasterizer.ToString()}");
        }
        if (DepthBiasState() != -1)
        {
            BungieDepthBiasDesc depthBias = RenderStates.DepthBiasStates[DepthBiasState()];
            states.AppendLine($"Depth Bias State {DepthBiasState()}:\n {depthBias.ToString()}");
        }

        return states.ToString();
    }
}
