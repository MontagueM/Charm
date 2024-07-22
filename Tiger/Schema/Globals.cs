using DirectXTexNet;
using SharpDX.Direct3D11;

namespace Tiger.Schema.Model;

[InitializeAfter(typeof(PackageResourcer))]
public class Globals : Strategy.StrategistSingleton<Globals>
{
    private List<TigerInputLayout> _inputLayouts = new();

    public Globals(TigerStrategy strategy, StrategyConfiguration strategyConfiguration) : base(strategy)
    {
    }

    public class TigerInputLayout
    {
        public List<TigerInputLayoutElement> Elements { get; set; }
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

    protected override void Initialise()
    {
        FillVertexInputLayouts();
    }

    protected override void Reset()
    {
        _inputLayouts.Clear();
    }

    public List<TigerInputLayout> GetInputLayouts()
    {
        return _inputLayouts;
    }

    private void FillVertexInputLayouts()
    {
        _inputLayouts.AddRange(BaseInputLayouts);

        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON) // D1 has an extra base layout, so just gonna reuse the last entry (suuurely its fine)
            _inputLayouts.Add(BaseInputLayouts[BaseInputLayouts.Count - 1]);

        bool PackageFilterFunc(string packagePath) =>
            Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ?
            packagePath.Contains("client_startup") : packagePath.Contains("globals");

        //PackageResourcer.Get().GetAllHashes<SClientBootstrap>(PackageFilterFunc).First();
        FileHash hash = Strategy.CurrentStrategy switch
        {
            TigerStrategy.DESTINY1_RISE_OF_IRON => new FileHash("0020AF80"),
            _ when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402 => PackageResourcer.Get().GetNamedTag("render_globals"),
            _ => PackageResourcer.Get().GetNamedTag("client_bootstrap_patchable")
        };

        var pkg = FileResourcer.Get().GetSchemaTag<SClientBootstrap>(hash);

        var ElementSet = pkg.TagData.RenderGlobals.TagData.InputLayouts.TagData.Elements2.TagData.Sets;
        var Mappings = pkg.TagData.RenderGlobals.TagData.InputLayouts.TagData.ElementMappings.TagData.Layouts;

        foreach (var layout in Mappings)
        {
            List<TigerInputLayoutElement> layoutElements = new List<TigerInputLayoutElement>();
            foreach (var (bufferIndex, elementIndex) in new int[] { layout.Element0, layout.Element1, layout.Element2, layout.Element3 }.Select((value, index) => (index, value)))
            {
                if (elementIndex == -1)
                    continue;

                foreach (var e in ElementSet[elementIndex].Elements)
                {
                    var semantic = InputSemantics[e.Semantic];
                    var format = GetInputFormats()[e.Format];
                    layoutElements.Add(new TigerInputLayoutElement
                    {
                        HlslType = format.HlslType,
                        Format = format.Format,
                        Stride = format.Stride,
                        SemanticName = semantic,
                        SemanticIndex = e.SemanticIndex,
                        BufferIndex = (uint)bufferIndex,
                        IsInstanceData = false
                    });
                }
            }
            _inputLayouts.Add(new TigerInputLayout
            {
                Elements = layoutElements
            });
        }

        //Console.WriteLine($"Input Layouts for {Strategy.CurrentStrategy}: {_inputLayouts.Count}");
        //for (int i = 0; i < _inputLayouts.Count; i++)
        //{
        //    var layout = _inputLayouts[i];
        //    Console.WriteLine($"Layout {i}");
        //    for (int j = 0; j < layout.Elements.Count; j++)
        //    {
        //        var element = layout.Elements[j];
        //        Console.WriteLine($"\tElement {j}");
        //        Console.WriteLine($"\t\tHlslType {element.HlslType}");
        //        Console.WriteLine($"\t\tFormat {element.Format}");
        //        Console.WriteLine($"\t\tStride {element.Stride}");
        //        Console.WriteLine($"\t\tSemanticName {element.SemanticName}");
        //        Console.WriteLine($"\t\tSemanticIndex {element.SemanticIndex}");
        //        Console.WriteLine($"\t\tBufferIndex {element.BufferIndex}");
        //        Console.WriteLine($"\t\tIsInstanceData {element.IsInstanceData}");
        //    }
        //}
    }

    public List<TfxRenderStage> ExportRenderStages = new List<TfxRenderStage>
    {
        TfxRenderStage.GenerateGbuffer,
        TfxRenderStage.Decals,
        TfxRenderStage.InvestmentDecals,
        TfxRenderStage.DecalsAdditive,
        TfxRenderStage.Transparents,
        TfxRenderStage.Distortion
        //TfxRenderStage.Reticle
    };

    public TfxRenderStage[] GetRenderStages()
    {
        return (TfxRenderStage[])Enum.GetValues(typeof(TfxRenderStage));
    }

    private static readonly string[] InputSemantics =
    {
        "POSITION",
        "BLENDWEIGHT",
        "BLENDINDICES",
        "NORMAL",
        "PSIZE",
        "TEXCOORD",
        "TANGENT",
        "BINORMAL",
        "COLOR"
    };

    private List<TigerInputLayoutElement> InputFormats = new List<TigerInputLayoutElement>
    {
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 0
        new TigerInputLayoutElement { HlslType = "float", Stride = 4, Format = DXGI_FORMAT.R32_FLOAT }, // 1
        new TigerInputLayoutElement { HlslType = "float2", Stride = 8, Format = DXGI_FORMAT.R32G32_FLOAT }, // 2
        new TigerInputLayoutElement { HlslType = "float3", Stride = 12, Format = DXGI_FORMAT.R32G32B32_FLOAT }, // 3
        new TigerInputLayoutElement { HlslType = "float4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_FLOAT }, // 4
        new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UNORM }, // 5
        new TigerInputLayoutElement { HlslType = "uint4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UINT }, // 6
        new TigerInputLayoutElement { HlslType = "int2", Stride = 4, Format = DXGI_FORMAT.R16G16_SINT }, // 7
        new TigerInputLayoutElement { HlslType = "int4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SINT }, // 8
        new TigerInputLayoutElement { HlslType = "uint4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_UINT }, // 9
        new TigerInputLayoutElement { HlslType = "float2", Stride = 4, Format = DXGI_FORMAT.R16G16_SNORM }, // 10
        new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SNORM }, // 11
        new TigerInputLayoutElement { HlslType = "float2", Stride = 4, Format = DXGI_FORMAT.R16G16_FLOAT }, // 12
        new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_FLOAT }, // 13
        new TigerInputLayoutElement { HlslType = "int4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_SINT }, // 14
        new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_SNORM }, // 15
        new TigerInputLayoutElement { HlslType = "uint4", Stride = 4, Format = DXGI_FORMAT.R10G10B10A2_UINT }, // 16
        new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R10G10B10A2_UNORM }, // 17
        new TigerInputLayoutElement { HlslType = "int", Stride = 4, Format = DXGI_FORMAT.R32_SINT }, // 18
        new TigerInputLayoutElement { HlslType = "int2", Stride = 8, Format = DXGI_FORMAT.R32G32_SINT }, // 19
        new TigerInputLayoutElement { HlslType = "int4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_SINT }, // 20
        new TigerInputLayoutElement { HlslType = "int", Stride = 4, Format = DXGI_FORMAT.R32_UINT }, // 21
        new TigerInputLayoutElement { HlslType = "int2", Stride = 8, Format = DXGI_FORMAT.R32G32_UINT }, // 22
        new TigerInputLayoutElement { HlslType = "int4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_UINT }, // 23
        new TigerInputLayoutElement { HlslType = "int", Stride = 2, Format = DXGI_FORMAT.R16_SINT }, // 24
        new TigerInputLayoutElement { HlslType = "float", Stride = 1, Format = DXGI_FORMAT.R8_UNORM }, // 25
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 26
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 27
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 28
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 29
        new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 30
        new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UNORM_SRGB }, // 31
        new TigerInputLayoutElement { HlslType = "float3", Stride = 4, Format = DXGI_FORMAT.R11G11B10_FLOAT }, // 32
        new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SNORM }, // 33
    };

    private List<TigerInputLayoutElement> GetInputFormats()
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            InputFormats = new List<TigerInputLayoutElement>
            {
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 0
                new TigerInputLayoutElement { HlslType = "float", Stride = 4, Format = DXGI_FORMAT.R32_FLOAT }, // 1
                new TigerInputLayoutElement { HlslType = "float2", Stride = 8, Format = DXGI_FORMAT.R32G32_FLOAT }, // 2
                new TigerInputLayoutElement { HlslType = "float3", Stride = 12, Format = DXGI_FORMAT.R32G32B32_FLOAT }, // 3
                new TigerInputLayoutElement { HlslType = "float4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_FLOAT }, // 4
                new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UNORM }, // 5
                new TigerInputLayoutElement { HlslType = "uint4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UINT }, // 6
                new TigerInputLayoutElement { HlslType = "int2", Stride = 4, Format = DXGI_FORMAT.R16G16_SINT }, // 7
                new TigerInputLayoutElement { HlslType = "int4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SINT }, // 8
                new TigerInputLayoutElement { HlslType = "float2", Stride = 4, Format = DXGI_FORMAT.R16G16_SNORM }, // 9
                new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SNORM }, // 10
                new TigerInputLayoutElement { HlslType = "float2", Stride = 4, Format = DXGI_FORMAT.R16G16_FLOAT }, // 11
                new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_FLOAT }, // 12
                new TigerInputLayoutElement { HlslType = "int4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_SINT }, // 13
                new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_SNORM }, // 14
                new TigerInputLayoutElement { HlslType = "uint4", Stride = 4, Format = DXGI_FORMAT.R10G10B10A2_UINT }, // 15
                new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R10G10B10A2_UNORM }, // 16
                new TigerInputLayoutElement { HlslType = "int", Stride = 4, Format = DXGI_FORMAT.R32_SINT }, // 17
                new TigerInputLayoutElement { HlslType = "int2", Stride = 8, Format = DXGI_FORMAT.R32G32_SINT }, // 18
                new TigerInputLayoutElement { HlslType = "int4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_SINT }, // 19
                new TigerInputLayoutElement { HlslType = "int", Stride = 4, Format = DXGI_FORMAT.R32_UINT }, // 20
                new TigerInputLayoutElement { HlslType = "int2", Stride = 8, Format = DXGI_FORMAT.R32G32_UINT }, // 21
                new TigerInputLayoutElement { HlslType = "int4", Stride = 16, Format = DXGI_FORMAT.R32G32B32A32_UINT }, // 22
                new TigerInputLayoutElement { HlslType = "int", Stride = 2, Format = DXGI_FORMAT.R16_SINT }, // 23
                new TigerInputLayoutElement { HlslType = "float", Stride = 1, Format = DXGI_FORMAT.R8_UNORM }, // 24
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 25
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 26
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 27
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 28
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 29
                new TigerInputLayoutElement { HlslType = "float4", Stride = 4, Format = DXGI_FORMAT.R8G8B8A8_UNORM_SRGB }, // 30
                new TigerInputLayoutElement { HlslType = "float3", Stride = 4, Format = DXGI_FORMAT.R11G11B10_FLOAT }, // 31
                new TigerInputLayoutElement { HlslType = "float4", Stride = 8, Format = DXGI_FORMAT.R16G16B16A16_SNORM }, // 32
                new TigerInputLayoutElement { HlslType = "", Stride = 0, Format = DXGI_FORMAT.UNKNOWN }, // 33
            };
        }

        return InputFormats;

    }

    private static readonly List<TigerInputLayout> BaseInputLayouts = new List<TigerInputLayout> {
        // Layout 0
        new TigerInputLayout {
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
            Elements = new List<TigerInputLayoutElement> {
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
        }
    };
}


//public static class VertexLayouts
//{
//    public class TigerInputLayout
//    {
//        public List<TigerInputLayoutElement> Elements { get; set; }
//    }

//    public class TigerInputLayoutElement
//    {
//        public string HlslType { get; set; }
//        public DXGI_FORMAT Format { get; set; }
//        public uint Stride { get; set; }
//        public string SemanticName { get; set; }
//        public uint SemanticIndex { get; set; }
//        public uint BufferIndex { get; set; }
//        public bool IsInstanceData { get; set; }
//    }

//    // yoinked from Alkahest (credit to Cohae obviously)
//    public static List<TigerInputLayout> InputLayouts = new List<TigerInputLayout> {
//        // Layout 0
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 1
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 2
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 3
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 4
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 5
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 6
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 7
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 8
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 9
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 10
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 11
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 12
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 13
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "BLENDWEIGHT",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "uint4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UINT,
//                    Stride = 4,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 14
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 15
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "BINORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 16
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 17
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 18
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 19
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "BINORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 20
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "BINORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 21
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 22
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "int4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SINT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_FLOAT,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 23
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "int4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SINT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 24
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "BINORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 25
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "BINORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 2,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 26
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R16G16_SNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R16G16B16A16_SNORM,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = true
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "BLENDINDICES",
//                    SemanticIndex = 0,
//                    BufferIndex = 3,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 27
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 28
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 29
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 30
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 31
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 32
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 33
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 34
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 35
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 36
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 37
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 38
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 39
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 40
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 41
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 42
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 15,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 43
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 44
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 45
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 46
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 47
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 48
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 49
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 50
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 51
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 52
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 53
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 54
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 55
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 56
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 57
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 58
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 15,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 59
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 60
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 61
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 62
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 63
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 64
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 65
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 66
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 67
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 68
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 69
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 70
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 71
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 72
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 73
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 74
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 1,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 2,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 3,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 9,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 10,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 11,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 12,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 13,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 14,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 15,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 1,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 2,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 75
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//            }
//        },
//            // Layout 76
//        new TigerInputLayout {
//            Elements = new List<TigerInputLayoutElement> {
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "POSITION",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float3",
//                    Format = DXGI_FORMAT.R32G32B32_FLOAT,
//                    Stride = 12,
//                    SemanticName = "NORMAL",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TANGENT",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float2",
//                    Format = DXGI_FORMAT.R32G32_FLOAT,
//                    Stride = 8,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R8G8B8A8_UNORM,
//                    Stride = 4,
//                    SemanticName = "COLOR",
//                    SemanticIndex = 0,
//                    BufferIndex = 0,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 4,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 5,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 6,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 7,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//                new TigerInputLayoutElement {
//                    HlslType = "float4",
//                    Format = DXGI_FORMAT.R32G32B32A32_FLOAT,
//                    Stride = 16,
//                    SemanticName = "TEXCOORD",
//                    SemanticIndex = 8,
//                    BufferIndex = 1,
//                    IsInstanceData = false
//                },
//            }
//        },
//    };
//}

public static class RenderStates
{
    public class BungieBlendDesc
    {
        public bool AlphaToCoverageEnable;
        public bool IndependentBlendEnable;
        public RenderTargetBlendDescription BlendDesc;
    }

    public class BungieRasterizerDesc
    {
        public FillMode FillMode;
        public CullMode CullMode;
        public bool FrontCounterClockwise;
        public bool DepthClipEnable;
        public bool ScissorEnable;
    }

    public class BungieDepthBiasDesc
    {
        public int DepthBias;
        public float SlopeScaledDepthBias;
        public float DepthBiasClamp;
    }


    public static BungieBlendDesc[] BlendStates = new BungieBlendDesc[]
    {
	    // Blend State 0
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 1
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 2
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 3
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationColor,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.DestinationAlpha,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 4
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationColor,
                DestinationBlend = BlendOption.SourceColor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.DestinationAlpha,
                DestinationAlphaBlend = BlendOption.SourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 5
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.SourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 6
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.DestinationAlpha,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 7
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.DestinationAlpha,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 8
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 9
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Minimum,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Minimum,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 10
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Maximum,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Maximum,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 11
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationColor,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.DestinationAlpha,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 12
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.SourceAlpha,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 13
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.BlendFactor,
                DestinationBlend = BlendOption.InverseBlendFactor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.BlendFactor,
                DestinationAlphaBlend = BlendOption.InverseBlendFactor,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 14
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.InverseSourceAlpha,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.InverseSourceAlpha,
                DestinationAlphaBlend = BlendOption.SourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 15
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationAlpha,
                DestinationBlend = BlendOption.InverseDestinationAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 16
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 17
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 18
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 19
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 20
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 21
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 22
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 23
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 24
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 25
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 26
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 27
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 28
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 29
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 30
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 31
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 32
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 33
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 34
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 35
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.SourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 36
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 37
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 38
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 39
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 40
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 41
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 42
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 43
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 44
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 45
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 46
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 47
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 48
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 49
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 50
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 51
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 52
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 53
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 54
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 55
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 56
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 57
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 58
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 59
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 60
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 61
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 62
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 63
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 64
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 65
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 66
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 67
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 68
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 69
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 70
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 71
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 72
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 73
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 74
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 75
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 76
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationColor,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 77
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationColor,
                DestinationBlend = BlendOption.SourceColor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 78
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = false,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.Zero,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 79
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.DestinationAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 80
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.InverseDestinationAlpha,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 81
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.DestinationAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 82
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.InverseDestinationAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 83
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 84
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 85
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 86
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SecondarySourceColor,
                DestinationBlend = BlendOption.InverseSecondarySourceColor,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.SecondarySourceAlpha,
                DestinationAlphaBlend = BlendOption.InverseSecondarySourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 87
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 88
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.Zero,
                DestinationAlphaBlend = BlendOption.InverseSourceAlpha,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
	    // Blend State 89
	    new BungieBlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = true,
            BlendDesc = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.One,
                DestinationBlend = BlendOption.One,
                BlendOperation = BlendOperation.ReverseSubtract,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.One,
                AlphaBlendOperation = BlendOperation.ReverseSubtract,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            }
        },
    };

    public static BungieRasterizerDesc[] RasterizerStates = new BungieRasterizerDesc[]
    {
	    // Rasterizer State 0
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 1
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 2
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 3
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Front,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 4
	    new BungieRasterizerDesc {
            FillMode = FillMode.Wireframe,
            CullMode = CullMode.Back,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 5
	    new BungieRasterizerDesc {
            FillMode = FillMode.Wireframe,
            CullMode = CullMode.None,
            FrontCounterClockwise = true,
            DepthClipEnable = true,
            ScissorEnable = false
        },
	    // Rasterizer State 6
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            FrontCounterClockwise = true,
            DepthClipEnable = false,
            ScissorEnable = false
        },
	    // Rasterizer State 7
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            FrontCounterClockwise = true,
            DepthClipEnable = false,
            ScissorEnable = false
        },
	    // Rasterizer State 8
	    new BungieRasterizerDesc {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Front,
            FrontCounterClockwise = true,
            DepthClipEnable = false,
            ScissorEnable = false
        },
    };

    public static BungieDepthBiasDesc[] DepthBiasStates = new BungieDepthBiasDesc[]
    {
	    // DepthBias 0
	    new BungieDepthBiasDesc
        {
            DepthBias =  0,
            SlopeScaledDepthBias =  0.0f,
            DepthBiasClamp =  0.0f,
        },
	    // DepthBias 1
	    new BungieDepthBiasDesc
        {
            DepthBias =  0,
            SlopeScaledDepthBias =  0.0f,
            DepthBiasClamp =  0.0f,
        },
	    // DepthBias 2
	    new BungieDepthBiasDesc
        {
            DepthBias =  5,
            SlopeScaledDepthBias =  2.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 3
	    new BungieDepthBiasDesc
        {
            DepthBias =  10,
            SlopeScaledDepthBias =  4.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 4
	    new BungieDepthBiasDesc
        {
            DepthBias =  15,
            SlopeScaledDepthBias =  6.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 5
	    new BungieDepthBiasDesc
        {
            DepthBias =  20,
            SlopeScaledDepthBias =  8.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 6
	    new BungieDepthBiasDesc
        {
            DepthBias =  2,
            SlopeScaledDepthBias =  2.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 7
	    new BungieDepthBiasDesc
        {
            DepthBias =  -1,
            SlopeScaledDepthBias =  -2.0f,
            DepthBiasClamp =  10000000000.0f,
        },
	    // DepthBias 8
	    new BungieDepthBiasDesc
        {
            DepthBias =  51,
            SlopeScaledDepthBias =  2.0f,
            DepthBiasClamp =  10000000000.0f,
        },
    };
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "41038080", 0x40)] // reference from shared_manifest
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "80978080", 0x5C)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "8C978080", 0x5C)]
public struct SClientBootstrap
{
    [SchemaField(0x3C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x4C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)] // is a Tag64 in Post-BL but its '32 bit tag, 01" so it doesn't matter
    public Tag<SRenderGlobals> RenderGlobals;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "B01B8080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B16C8080", 0x40)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "A8678080", 0x40)]
public struct SRenderGlobals
{
    public long FileSize;
    public Tag<SVertexInputLayouts> InputLayouts;
    //[SchemaField(0x10, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    //public DynamicArrayUnloaded<SRenderGlobalScopes> Scopes;
    //public DynamicArrayUnloaded<SRenderGlobalPipelines> Pipelines;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "631B8080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A6728080", 0x30)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "786D8080", 0x38)]
public struct SVertexInputLayouts
{
    public long FileSize;
    [SchemaField(0xC)]
    public Tag<SVertexInputElementSets> Elements1;
    public Tag<SVertexInputElementSets> Elements2;

    //[SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    //[SchemaField(0x2C, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    //public Tag<SVertexInputElementSets> ElementsLast;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<SVertexInputLayoutMapping> ElementMappings;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "DE1B8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AD728080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "7F6D8080", 0x18)]
public struct SVertexInputElementSets
{
    public long FileSize;
    public DynamicArray<SVertexInputElementSet> Sets;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "981A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AF728080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "816D8080", 0x10)]
public struct SVertexInputElementSet
{
    public DynamicArray<SVertexInputElement> Elements;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D71B8080", 0x03)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "B2728080", 0x03)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "846D8080", 0x03)]
public struct SVertexInputElement
{
    public byte Semantic;
    public byte SemanticIndex;
    public byte Format;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "C71A8080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "A9728080", 0x18)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "7B6D8080", 0x18)]
public struct SVertexInputLayoutMapping
{
    public long FileSize;
    public DynamicArray<SVertexLayout> Layouts;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "891A8080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "AC728080", 0x1C)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "846D8080", 0x1C)]
public struct SVertexLayout
{
    public short Index;
    [SchemaField(0x8)]
    public int Element0;
    public int Element1;
    public int Element2;
    public int Element3;
}
