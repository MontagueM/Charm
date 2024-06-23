using DirectXTexNet;

namespace Tiger.Schema.Model;

public static class VertexLayouts
{
    public class TigerInputLayout
    {
        public TigerInputLayoutElement[] Elements { get; set; }
    }

    public class TigerInputLayoutElement
    {
        public string HlslType { get; set; }
        public DXGI_FORMAT Format { get; set; }
        public uint Stride { get; set; }
        public string SemanticName { get; set; }
        public uint SemanticIndex { get; set; }
        public uint BufferIndex { get; set; }
        public bool IsInstanceData { get; set; }
    }

    public static List<TfxRenderStage> ExportRenderStages = new List<TfxRenderStage>
    {
        TfxRenderStage.GenerateGbuffer,
        TfxRenderStage.Decals,
        TfxRenderStage.InvestmentDecals,
        TfxRenderStage.DecalsAdditive,
        TfxRenderStage.Transparents,
        TfxRenderStage.Distortion
        //TfxRenderStage.Reticle
    };

    public static TfxRenderStage[] GetRenderStages()
    {
        return (TfxRenderStage[])Enum.GetValues(typeof(TfxRenderStage));
    }

    // yoinked from Alkahest (credit to Cohae obviously)
    public static TigerInputLayout[] InputLayouts = new TigerInputLayout[] {
        // Layout 0
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 1
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 2
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 3
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 4
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 5
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 6
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 7
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 8
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 9
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 10
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 11
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 12
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 13
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "BLENDWEIGHT",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "uint4",
                    Format = DXGI_FORMAT.R8G8B8A8_UINT,
                    Stride = 4,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
            }
        },
            // Layout 14
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
            }
        },
            // Layout 15
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "BINORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 16
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 17
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 18
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 19
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "BINORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 20
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "BINORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 21
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 22
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "int4",
                    Format = DXGI_FORMAT.R16G16B16A16_SINT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_FLOAT,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 23
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "int4",
                    Format = DXGI_FORMAT.R16G16B16A16_SINT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 24
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "BINORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 25
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "BINORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 2,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 26
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R16G16_SNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = true
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "BLENDINDICES",
                    SemanticIndex = 0,
                    BufferIndex = 3,
                    IsInstanceData = false
                },
            }
        },
            // Layout 27
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 28
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 29
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 30
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 31
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 32
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 33
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 34
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 35
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 36
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 37
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 38
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 39
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 40
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 41
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 42
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 15,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 43
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 44
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 45
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 46
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 47
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 48
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 49
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 50
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 51
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 52
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 53
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 54
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 55
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 56
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 57
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 58
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 15,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 59
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 60
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 61
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 62
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 63
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 64
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 65
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 66
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 67
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 68
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 69
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 70
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 71
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 72
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 73
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 74
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 1,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 2,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 3,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 9,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 10,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 11,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 12,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 13,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 14,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 15,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "POSITION",
                    SemanticIndex = 1,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 2,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
            // Layout 75
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
            }
        },
            // Layout 76
        new TigerInputLayout {
            Elements = new TigerInputLayoutElement[] {
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "POSITION",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float3",
                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
                    Stride = 12,
                    SemanticName = "NORMAL",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TANGENT",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float2",
                    Format = DXGI_FORMAT.R32G32_FLOAT,
                    Stride = 8,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
                    Stride = 4,
                    SemanticName = "COLOR",
                    SemanticIndex = 0,
                    BufferIndex = 0,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 4,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 5,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 6,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 7,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
                new TigerInputLayoutElement {
                    HlslType = "float4",
                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
                    Stride = 16,
                    SemanticName = "TEXCOORD",
                    SemanticIndex = 8,
                    BufferIndex = 1,
                    IsInstanceData = false
                },
            }
        },
    };
}

