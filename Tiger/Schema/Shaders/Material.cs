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

        private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        public void SavePixelShader(string saveDirectory, bool isTerrain = false)
        {
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
                return;

            if (Pixel.Shader != null && Pixel.Shader.Hash.IsValid())
            {
                try
                {
                    if (_config.GetUnrealInteropEnabled())
                    {
                        string usf = new UsfConverter().HlslToUsf(this, false);
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Unreal");
                        File.WriteAllText($"{saveDirectory}/Shaders/Unreal/PS_{Hash}.usf", usf);
                    }

                    if (_config.GetS2ShaderExportEnabled())
                    {
                        string vfx = new S2ShaderConverter().HlslToVfx(this);
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Source2");
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/Source2/materials");

                        File.WriteAllText($"{saveDirectory}/Shaders/Source2/PS_{Pixel.Shader.Hash}.shader", vfx);
                        if (!isTerrain)
                            Source2Handler.SaveVMAT(saveDirectory, Hash, this);
                    }

                    if (_config.GetExportHLSL())
                    {
                        string pixel = Pixel.Shader.Decompile($"ps{Pixel.Shader.Hash}");
                        Directory.CreateDirectory($"{saveDirectory}/Shaders/HLSL");
                        File.WriteAllText($"{saveDirectory}/Shaders/HLSL/PS_{Hash}.hlsl", pixel);
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
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
                return;

            if (Vertex.Shader != null && Vertex.Shader.Hash.IsValid())
            {
                try
                {
                    if (_config.GetExportHLSL())
                    {
                        string vertex = Vertex.Shader.Decompile($"vs{Vertex.Shader.Hash}");
                        Directory.CreateDirectory($"{saveDirectory}/HLSL");
                        File.WriteAllText($"{saveDirectory}/HLSL/VS_{Hash}.hlsl", vertex);
                    }
                }
                catch (IOException e)  // threading error
                {
                    Log.Error(e.Message);
                }
            }
        }

        // TODO: Remove material data from cfg and use this instead, cfg is too cluttered 
        public void Export(string saveDirectory)
        {
            var hlslPath = $"{saveDirectory}/Shaders/Raw";
            var texturePath = $"{saveDirectory}/Textures";
            Directory.CreateDirectory(hlslPath);
            Directory.CreateDirectory(texturePath);

            JsonMaterial material = new()
            {
                Hash = Hash,
                Scopes = EnumerateScopes().ToList(),
                Externs = GetExterns().ToList(),
                RenderStates = RenderStates
            };
            if (Pixel.Shader != null)
            {
                Pixel.Shader.Decompile($"ps{Pixel.Shader.Hash}", hlslPath);
                SavePixelShader($"{saveDirectory}");

                ShaderDetails psCB = new ShaderDetails();
                psCB.CBuffers = Pixel.GetCBuffer0();
                psCB.Bytecode = Pixel.TFX_Bytecode.Select(x => x.Value).ToList();
                psCB.Constants = Pixel.TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                psCB.Textures = new();
                foreach (var texture in Pixel.EnumerateTextures())
                {
                    psCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.GetTexture().Hash,
                        Colorspace = texture.GetTexture().IsSrgb() ? "Srgb" : "Non-Color",
                        Dimension = texture.GetTexture().GetDimension().GetEnumDescription(),
                        Format = texture.GetTexture().TagData.GetFormat().ToString()
                    });
                }

                psCB.TileTextureDetails = new();
                psCB.Samplers = new();
                foreach (var item in Pixel.Samplers.Select((sampler, index) => new { sampler, index }))
                {
                    var sampler = item.sampler.GetSampler();
                    if (sampler.Hash.GetFileMetadata().Type != 34)
                    {
                        var tex = FileResourcer.Get().GetFile<Texture>(sampler.Hash);
                        psCB.TileTextureDetails.Add(new()
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
                Vertex.Shader.Decompile($"vs{Vertex.Shader.Hash}", hlslPath);
                SaveVertexShader($"{saveDirectory}/Shaders/");

                ShaderDetails vsCB = new ShaderDetails();
                vsCB.CBuffers = Vertex.GetCBuffer0();
                vsCB.Bytecode = Vertex.TFX_Bytecode.Select(x => x.Value).ToList();
                vsCB.Constants = Vertex.TFX_Bytecode_Constants.Select(x => x.Vec).ToList();

                vsCB.Textures = new();
                foreach (var texture in Vertex.EnumerateTextures())
                {
                    vsCB.Textures.TryAdd((int)texture.TextureIndex, new()
                    {
                        Hash = texture.GetTexture().Hash,
                        Colorspace = texture.GetTexture().IsSrgb() ? "Srgb" : "Non-Color",
                        Dimension = texture.GetTexture().GetDimension().GetEnumDescription(),
                        Format = texture.GetTexture().TagData.GetFormat().ToString()
                    });
                }

                material.Material.TryAdd(JsonMaterial.ShaderStage.Vertex, vsCB);
            }

            foreach (STextureTag texture in Vertex.EnumerateTextures())
            {
                if (texture.GetTexture() == null)
                    continue;

                texture.GetTexture().SavetoFile($"{saveDirectory}/Textures/{texture.GetTexture().Hash}");
            }
            foreach (STextureTag texture in Pixel.EnumerateTextures())
            {
                if (texture.GetTexture() == null)
                    continue;

                texture.GetTexture().SavetoFile($"{saveDirectory}/Textures/{texture.GetTexture().Hash}");
            }


            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };
            File.WriteAllText($"{saveDirectory}/{Hash}.json", JsonConvert.SerializeObject(material, jsonSettings));
        }

        public List<TfxExtern> GetExterns()
        {
            var opcodes = Pixel.GetBytecode().Opcodes;
            opcodes.AddRange(Vertex.GetBytecode().Opcodes);

            var list = new List<TfxExtern>();
            foreach (var op in opcodes.Where(x => x.op.ToString().Contains("Extern")))
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
        StringBuilder states = new();
        if (BlendState() != -1)
        {
            var blendState = RenderStates.BlendStates[BlendState()];
            states.AppendLine($"Blend State {BlendState()}:\n {blendState.ToString()}");
        }
        if (DepthStencilState() != -1)
        {
            var dsState = RenderStates.DepthStencilStates[DepthStencilState()];
            states.AppendLine($"Depth Stencil State {DepthStencilState()}:\n {dsState.ToString()}");
        }

        return states.ToString();
    }
}
