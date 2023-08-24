using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class S2ShaderConverter
{
    private StringReader hlsl;
    private StringBuilder vfx;
    private readonly List<TextureView> textures = new List<TextureView>();
    private readonly List<int> samplers = new List<int>();
    private readonly List<Cbuffer> cbuffers = new List<Cbuffer>();
    private readonly List<Shaders.Buffer> buffers = new List<Shaders.Buffer>();
    private readonly List<Input> inputs = new List<Input>();
    private readonly List<Output> outputs = new List<Output>();
    private static bool isTerrain = false;
    private bool bOpacityEnabled = false;
    private bool bTranslucent = true;
    private bool bUsesFrontFace = false;
    private bool bFixRoughness = false;
    private bool bUsesNormalBuffer = false;

    public string vfxStructure =
$@"HEADER
{{
	Description = ""Charm Auto-Generated Source 2 Shader""; 
}}

MODES
{{
	VrForward();

	Depth(); 

	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( ""vr_tools_wireframe.shader"" );
	ToolsShadingComplexity( ""tools_shading_complexity.shader"" );

	Reflection( S_MODE_REFLECTIONS );
}}

FEATURES
{{
    #include ""common/features.hlsl""
    Feature( F_DYNAMIC_REFLECTIONS, 0..1, ""Rendering"" );
}}

COMMON
{{
    //alpha
    //frontface
	#include ""common/shared.hlsl""
    #define CUSTOM_MATERIAL_INPUTS
}}

struct VertexInput
{{
    float4 vColorBlendValues : Color0 < Semantic( Color ); >;
    //float4 vTangentUOs : TANGENT	< Semantic( TangentU_SignV ); >;
    //uint vVertexID : SV_VERTEXID < Semantic( VertexID ); >;
    //uint vInstanceID : SV_InstanceID < Semantic( None ); >;
	#include ""common/vertexinput.hlsl""
}};

struct PixelInput
{{
    float4 vBlendValues		 : TEXCOORD14;
    float3 o0                : TEXCOORD15;
	#include ""common/pixelinput.hlsl""
}};

VS
{{
	#include ""common/vertex.hlsl""
    #define CUSTOM_TEXTURE_FILTERING 
    #define cmp -

//vs_samplers  
//vs_CBuffers
//vs_Inputs

	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{{
		PixelInput o = ProcessVertex( i );
        float4 o0,o1,o2,o3,o4,o5,o6,o7,o8;
        o.vBlendValues = i.vColorBlendValues;
		o.vBlendValues.a = i.vColorBlendValues.a;

//vs_Function
        {(isTerrain ? "float4 r0;\r\n       r0.xyzw = (int4)float4(i.vPositionOs.xyz,0);\r\n        r0.xyzw = float4(0,0,0,0) + r0.xyzw;\r\n        r0.z = r0.w * 65536 + r0.z;\r\n        r0.xyw = float3(0.015625,0.015625,0.000122070313) * r0.xyz;\r\n        o.o0.xyz = r0.xyw;" : "")}
		return FinalizeVertex( o );
	}}
}}

PS
{{
    #include ""common/pixel.hlsl""
    #include ""raytracing/reflections.hlsl""
    #define CUSTOM_TEXTURE_FILTERING 
    #define cmp -
    //RenderState
    
    #if ( S_MODE_REFLECTIONS )
		#define FinalOutput ReflectionOutput
	#else
		#define FinalOutput float4
	#endif

    //Debugs, uncomment for use in shader baker
    //bool g_bDiffuse < Attribute( ""Debug_Diffuse"" ); >;
    //bool g_bRough < Attribute( ""Debug_Rough"" ); >;
    //bool g_bMetal < Attribute( ""Debug_Metal"" ); >;
    //bool g_bNorm < Attribute( ""Debug_Normal"" ); >;
    //bool g_bAO < Attribute( ""Debug_AO"" ); >;
    //bool g_bEmit < Attribute( ""Debug_Emit"" ); >;
    //bool g_bAlpha < Attribute( ""Debug_Alpha"" ); >;

//ps_samplers
//ps_CBuffers
//ps_Inputs
        
    FinalOutput MainPs( PixelInput i ) : SV_Target0
    {{
//ps_Function

        // Normal
        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);
        float normal_length = length(biased_normal);
        float3 normal_in_world_space = biased_normal / normal_length;

        float smoothness = saturate(8 * (normal_length - 0.375));
        
        Material mat = Material::From(i,
                    float4(o0.xyz, alpha), //albedo, alpha
                    float4(0.5, 0.5, 1, 1), //Normal, gets set later
                    float4(1 - smoothness, saturate(o2.x), saturate(o2.y * 2), 1), //rough, metal, ao
                    float3(1.0f, 1.0f, 1.0f), //tint
                    clamp((o2.y - 0.5) * 2 * 6 * o0.xyz, 0, 100)); //emission

        mat.Transmission = o2.z;
        mat.Normal = normal_in_world_space; //Normal is already in world space so no need to convert in Material::From

        //if(g_bDiffuse)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = o0.xyz;
        //}}
        //if(g_bRough)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = 1 - smoothness;
        //}}
        //if(g_bMetal)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = saturate(o2.x);
        //}}
        //if(g_bNorm)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = SrgbGammaToLinear(PackNormal3D(Vec3WsToTs(normal_in_world_space.xyz, i.vNormalWs.xyz, vTangentUWs.xyz, vTangentVWs.xyz)));
        //}}
        //if(g_bAO)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = saturate(o2.y * 2);
        //}}
        //if(g_bEmit)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = (o2.y - 0.5);
        //}}
        //if(g_bAlpha)
        //{{
        //    mat.Albedo = 0;
        //    mat.Emission = alpha;
        //}}

        #if ( S_MODE_REFLECTIONS )
		{{
			return Reflections::From( i, mat, SampleCountIntersection );
		}}
        #else
		{{
            return ShadingModelStandard::Shade(i, mat);
        }}
        #endif  
    }}
}}";

    public string HlslToVfx(IMaterial material, string pixel, string vertex, bool bIsTerrain = false)
    {
        //Pixel Shader
        StringBuilder texSamples = new StringBuilder();
        hlsl = new StringReader(pixel);
        vfx = new StringBuilder();
        isTerrain = bIsTerrain;

        ProcessHlslData();

        if (bUsesFrontFace)
        {
            vfxStructure = vfxStructure.Replace("//frontface", "#define S_RENDER_BACKFACES 1");
            vfxStructure = vfxStructure.Replace("//RenderState", "RenderState( CullMode, S_RENDER_BACKFACES ? NONE : DEFAULT );");
        }

        for (int i = 0; i < material.PS_Samplers.Count; i++)
        {
            if (material.PS_Samplers[i].Samplers is null)
                continue;

            var sampler = material.PS_Samplers[i].Samplers.Sampler;
            texSamples.AppendLine($"\tSamplerState g_s{i + 1} < Filter({sampler.Filter}); AddressU({sampler.AddressU}); AddressV({sampler.AddressV}); AddressW({sampler.AddressW}); ComparisonFunc({sampler.ComparisonFunc}); MaxAniso({sampler.MaxAnisotropy}); >;");
        }

        vfxStructure = vfxStructure.Replace("//ps_samplers", texSamples.ToString());
        vfxStructure = vfxStructure.Replace("//ps_CBuffers", WriteCbuffers(material, false).ToString());

        hlsl = new StringReader(pixel);
        StringBuilder instructions = ConvertInstructions(material, false);
        if (instructions.ToString().Length == 0)
        {
            return "";
        }
        vfxStructure = vfxStructure.Replace("//ps_Function", instructions.ToString());
        vfxStructure = vfxStructure.Replace("//ps_Inputs", WriteFunctionDefinition(material, false).ToString());

        if (bFixRoughness)
            vfxStructure = vfxStructure.Replace("float smoothness = saturate(8 * (normal_length - 0.375));", "float smoothness = saturate(8 * (0 - 0.375));");

        if (bOpacityEnabled || bTranslucent) //This way is stupid but it works
            vfxStructure = vfxStructure.Replace("//alpha", $"#ifndef S_ALPHA_TEST\r\n\t#define S_ALPHA_TEST {((bUsesNormalBuffer || bTranslucent) ? "0" : "1")}\r\n\t#endif\r\n\t#ifndef S_TRANSLUCENT\r\n\t#define S_TRANSLUCENT {((bUsesNormalBuffer || bTranslucent) ? "1" : "0")}\r\n\t#endif");

        if(bUsesNormalBuffer) //Cant get normal buffer in forward rendering so just use world normal ig...
            vfxStructure = vfxStructure.Replace("mat.Normal = normal_in_world_space;", $"mat.Normal = v0;");

        //------------------------------------------------------------------------------

        //Vertex Shader - Commented out for now
        //if(bIsTerrain) 
        //{
        //    texSamples = new StringBuilder();
        //    hlsl = new StringReader(vertex);

        //    ProcessHlslData();

        //    for (int i = 0; i < material.Header.VSSamplers.Count; i++)
        //    {
        //        if (material.Header.VSSamplers[i].Samplers is null)
        //            continue;

        //        var sampler = material.Header.VSSamplers[i].Samplers.Sampler;
        //        texSamples.AppendLine($"SamplerState g_s{i + 1} < Filter({sampler.Header.Filter}); AddressU({sampler.Header.AddressU}); AddressV({sampler.Header.AddressV}); AddressW({sampler.Header.AddressW}); ComparisonFunc({sampler.Header.ComparisonFunc}); MaxAniso({sampler.Header.MaxAnisotropy}); >;");
        //    }

        //    vfxStructure = vfxStructure.Replace("//vs_samplers", texSamples.ToString());

        //    vfxStructure = vfxStructure.Replace("//vs_Inputs", WriteFunctionDefinition(material, true).ToString());
        //    vfxStructure = vfxStructure.Replace("//vs_CBuffers", WriteCbuffers(material, true).ToString());

        //    hlsl = new StringReader(vertex);
        //    instructions = ConvertInstructions(true);
        //    vfxStructure = vfxStructure.Replace("//vs_Function", instructions.ToString());

        //}

        //--------------------------
        vfx.AppendLine(vfxStructure);
        return vfx.ToString();
    }

    private void ProcessHlslData()
    {
        string line = string.Empty;
        cbuffers.Clear();
        textures.Clear();
        inputs.Clear();
        bool bFindOpacity = false;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("r0,r1")) // at end of function definition
                {
                    bFindOpacity = true;
                }

                if (bFindOpacity)
                {
                    if (line.Contains("discard") || line.Contains("o0.w = r"))
                    {
                        bOpacityEnabled = true;
                        break;
                    }
                    continue;
                }

                if (line.Contains("Texture"))
                {
                    TextureView texture = new TextureView();
                    texture.Dimension = line.Split("<")[0];
                    texture.Type = line.Split("<")[1].Split(">")[0];
                    texture.Variable = line.Split("> ")[1].Split(" :")[0];
                    texture.Index = Int32.TryParse(new string(texture.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    textures.Add(texture);
                }
                else if (line.Contains("SamplerState"))
                {
                    samplers.Add(line.Split("(")[1].Split(")")[0].Last() - 48);
                }
                else if (line.Contains("Buffer"))
                {
                    Shaders.Buffer buffer = new Shaders.Buffer();
                    buffer.Type = line.Split('<', '>')[1];
                    buffer.Variable = line.Split(' ')[1];
                    buffer.Index = Int32.TryParse(new string(buffer.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    buffers.Add(buffer);
                }
                else if (line.Contains("cbuffer"))
                {
                    hlsl.ReadLine();
                    line = hlsl.ReadLine();
                    Cbuffer cbuffer = new Cbuffer();
                    cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                }
                else if (line.Contains(" v") && line.Contains(" : ") && !line.Contains("?"))
                {
                    Input input = new Input();
                    input.Variable = "v" + line.Split("v")[1].Split(" : ")[0];
                    input.Index = Int32.TryParse(new string(input.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    input.Semantic = line.Split(" : ")[1].Split(",")[0];
                    if (input.Semantic == "SV_isFrontFace0")
                        bUsesFrontFace = true;
                    input.Type = line.Split(" v")[0].Trim();
                    inputs.Add(input);
                }
                else if (line.Contains("out") && line.Contains(" : "))
                {
                    Output output = new Output();
                    output.Variable = "o" + line.Split(" o")[2].Split(" : ")[0];
                    output.Index = Int32.TryParse(new string(output.Variable.Skip(1).ToArray()), out int index) ? index : -1;
                    output.Semantic = line.Split(" : ")[1].Split(",")[0];
                    output.Type = line.Split("out ")[1].Split(" o")[0];
                    outputs.Add(output);
                }
            }

        } while (line != null);
    }

    private StringBuilder WriteCbuffers(IMaterial material, bool bIsVertexShader)
    {
        StringBuilder CBuffers = new();
        // Try to find matches, pixel shader has Unk2D0, Unk2E0, Unk300 available
        // Vertex has Unk90, UnkA0, UnkC0
        foreach (var buffer in buffers)
        {
            CBuffers.AppendLine($"\tBuffer<{buffer.Type}> b_{buffer.Variable} : register({buffer.Variable});");
        }

        foreach (var cbuffer in cbuffers)
        {
            CBuffers.AppendLine($"\tstatic {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("\t{");

            dynamic data = null;
            if (bIsVertexShader)
            {
                if (cbuffer.Count == material.Unk90.Count)
                {
                    data = material.Unk90;
                }
                else if (cbuffer.Count == material.UnkA0.Count)
                {
                    data = material.UnkA0;
                }
                else if (cbuffer.Count == material.UnkC0.Count)
                {
                    data = material.UnkC0;
                }
            }
            else
            {
                //if (cbuffer.Count == material.Header.Unk2D0.Count) Unk2D0 is byte, not float, so not a cbuffer?
                //{
                //    data = material.Header.Unk2D0;
                //}
                if (cbuffer.Count == material.Unk2E0.Count)
                {
                    data = material.Unk2E0;
                }
                else if (cbuffer.Count == material.Unk300.Count)
                {
                    data = material.Unk300;
                }
                else
                {
                    if (material.PSVector4Container.IsValid())
                    {
                        // Try the Vector4 storage file
                        TigerFile container = new(material.PSVector4Container.GetReferenceHash());
                        byte[] containerData = container.GetData();
                        int num = containerData.Length / 16;
                        if (cbuffer.Count == num)
                        {
                            List<Vector4> float4s = new();
                            for (int i = 0; i < containerData.Length / 16; i++)
                            {
                                float4s.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
                            }

                            data = float4s;
                        }
                    }

                }
            }

            for (int i = 0; i < cbuffer.Count; i++)
            {
                switch (cbuffer.Type)
                {
                    case "float4":
                        if (data == null) CBuffers.AppendLine("      float4(0.0, 0.0, 0.0, 0.0), //null" + i);
                        else
                        {
                            try
                            {
                                if (data[i] is Vec4)
                                {
                                    CBuffers.AppendLine($"\t\tfloat4({data[i].Vec.X}, {data[i].Vec.Y}, {data[i].Vec.Z}, {data[i].Vec.W}), //" + i);
                                }
                                else if (data[i] is Vector4)
                                {
                                    CBuffers.AppendLine($"\t\tfloat4({data[i].X}, {data[i].Y}, {data[i].Z}, {data[i].W}), //" + i);
                                }
                                else
                                {
                                    var x = data[i].Unk00.X; // really bad but required

                                    CBuffers.AppendLine($"\t\tfloat4({x}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W}), //" + i);
                                }
                            }
                            catch (Exception e)  // figure out whats up here, taniks breaks it
                            {
                                CBuffers.AppendLine("\t\tfloat4(0.0, 0.0, 0.0, 0.0), //Exception" + i);
                            }
                        }
                        break;
                    case "float3":
                        if (data == null) CBuffers.AppendLine("\t\tfloat3(0.0, 0.0, 0.0), //null" + i);
                        else CBuffers.AppendLine($"\t\tfloat3({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}), //" + i);
                        break;
                    case "float":
                        if (data == null) CBuffers.AppendLine("\t\tfloat(0.0), //null" + i);
                        else CBuffers.AppendLine($"\t\tfloat4({data[i].Unk00}), //" + i);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            CBuffers.AppendLine("\t};");
        }

        return CBuffers.Replace("∞", "1.#INF");
    }

    private StringBuilder WriteFunctionDefinition(IMaterial material, bool bIsVertexShader)
    {
        StringBuilder funcDef = new();
        if (bIsVertexShader)
        {
            foreach (var e in material.EnumerateVSTextures())
            {
                if (e.Texture != null)
                {
                    string type = e.Texture.IsSrgb() ? "Srgb" : "Linear";

                    funcDef.AppendLine($"\tCreateInputTexture2D( vTextureT{e.TextureIndex}, {type}, 8, \"\", \"\",  \"Textures,10/{e.TextureIndex}\", Default3( 1.0, 1.0, 1.0 ));");
                    funcDef.AppendLine($"\tCreateTexture2DWithoutSampler( g_vt{e.TextureIndex} )  < Channel( RGBA,  Box( vTextureT{e.TextureIndex} ), {type} ); OutputFormat( BC7 ); SrgbRead( {e.Texture.IsSrgb()} ); >; ");
                    funcDef.AppendLine($"\tTextureAttribute(g_vt{e.TextureIndex}, g_vt{e.TextureIndex});\n"); //Prevents some inputs not appearing for some reason
                }
            }
        }
        else
        {
            foreach (var e in material.EnumeratePSTextures())
            {
                if (e.Texture != null)
                {
                    string type = e.Texture.IsSrgb() ? "Srgb" : "Linear";
                    string dimension = e.Texture.GetDimension();

                    funcDef.AppendLine($"\tCreateInputTexture{dimension}( TextureT{e.TextureIndex}, {type}, 8, \"\", \"\",  \"Textures,10/{e.TextureIndex}\", Default3( 1.0, 1.0, 1.0 ));");
                    funcDef.AppendLine($"\tTexture{dimension} g_t{e.TextureIndex} < Channel( RGBA,  Box( TextureT{e.TextureIndex} ), {type} ); OutputFormat( BC7 ); SrgbRead( {e.Texture.IsSrgb()} ); >; ");
                    funcDef.AppendLine($"\tTextureAttribute(g_t{e.TextureIndex}, g_t{e.TextureIndex});\n"); //Prevents some inputs not appearing for some reason
                }
            }

            if (isTerrain) //Terrain has 4 dyemaps per shader, from what ive seen
            {
                funcDef.AppendLine($"\tCreateInputTexture2D( TextureT14_0, Linear, 8, \"\", \"\",  \"Textures,10/14\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture2D g_t14_0 < Channel( RGBA,  Box( TextureT14_0 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
                funcDef.AppendLine($"\tTextureAttribute(g_t14_0, g_t14_0);\n");

                funcDef.AppendLine($"\tCreateInputTexture2D( TextureT14_1, Linear, 8, \"\", \"\",  \"Textures,10/15\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture2D g_t14_1 < Channel( RGBA,  Box( TextureT14_1 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
                funcDef.AppendLine($"\tTextureAttribute(g_t14_1, g_t14_1);\n");

                funcDef.AppendLine($"\tCreateInputTexture2D( TextureT14_2, Linear, 8, \"\", \"\",  \"Textures,10/16\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture2D g_t14_2< Channel( RGBA,  Box( TextureT14_2 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
                funcDef.AppendLine($"\tTextureAttribute(g_t14_2, g_t14_2);\n");

                funcDef.AppendLine($"\tCreateInputTexture2D( TextureT14_3, Linear, 8, \"\", \"\",  \"Textures,10/17\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture2D( g_t14_3 )  < Channel( RGBA,  Box( TextureT14_3 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
                funcDef.AppendLine($"\tTextureAttribute(g_t14_3, g_t14_3);\n");
            }

            //if(bUsesNormalBuffer)
            //{
            //    funcDef.AppendLine($"\tBoolAttribute( bWantsFBCopyTexture, true );");
            //    funcDef.AppendLine($"\tCreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;");
            //}
        }
        return funcDef;
    }

    private StringBuilder ConvertInstructions(IMaterial material, bool isVertexShader)
    {
        StringBuilder funcDef = new();

        if (isVertexShader)
        {
            foreach (var i in inputs)
            {
                if (i.Semantic == "POSITION0")
                {
                    funcDef.AppendLine($"\t\tfloat4 {i.Variable} = float4(i.vPositionOs, 0); //{i.Semantic}");
                }
                else if (i.Semantic == "TANGENT0")
                {
                    funcDef.AppendLine($"\t\tfloat4 {i.Variable} = i.vTangentUOs; //{i.Semantic}");
                }
                else if (i.Semantic == "TEXCOORD0")
                {
                    funcDef.AppendLine($"\t\tfloat4 {i.Variable} = float4(i.vTexCoord, 0, 0); //{i.Semantic}");
                }
                else if (i.Semantic == "NORMAL0")
                {
                    funcDef.AppendLine($"\t\tfloat4 {i.Variable} = i.vNormalOs; //{i.Semantic}");
                }
                else if (i.Semantic == "SV_VERTEXID0")
                {
                    funcDef.AppendLine($"\t\tuint {i.Variable} = i.vVertexID; //{i.Semantic}");
                }
                else if (i.Semantic == "SV_InstanceID0")
                {
                    funcDef.AppendLine($"\t\tuint {i.Variable} = i.vInstanceID; //{i.Semantic}");
                }
                else
                {
                    funcDef.AppendLine($"\t\t{i.Type} {i.Variable} = {i.Variable}; //{i.Semantic}");
                }

                //if (i.Type == "uint")
                //{
                //    if (i.Semantic == "SV_isFrontFace0")
                //        funcDef.AppendLine($"       int {i.Variable} = i.face;");
                //    else
                //        funcDef.AppendLine($"       int {i.Variable} = {i.Variable};");
                //}
            }

            string line = hlsl.ReadLine();
            if (line == null)
            {
                return new StringBuilder();
            }
            while (!line.Contains("SV_POSITION0"))
            {
                line = hlsl.ReadLine();
                if (line == null)
                {
                    return new StringBuilder();
                }
            }
            hlsl.ReadLine();
            do
            {
                line = hlsl.ReadLine();
                if (line != null)
                {
                    if (line.Contains("return;"))
                    {
                        break;
                    }
                    else if (line.Contains("Sample"))
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                        var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                        var sampleUv = line.Split(", ")[1].Split(")")[0];
                        var dotAfter = line.Split(").")[1];
                        // todo add dimension

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= Tex2DS(g_t{texIndex}, g_s{sampleIndex}, {sampleUv}).{dotAfter}");
                    }
                    else if (line.Contains("Load"))
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                        var sampleUv = line.Split("(")[1].Split(")")[0];

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= b_t{texIndex}.Load({sampleUv});");
                    }
                    else
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart()}");
                    }
                    funcDef.Replace("∞", "1.#INF");
                }
            } while (line != null);
        }
        else //Pixel
        {
            funcDef.AppendLine("\t\tfloat3 vPositionWs = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;");
            funcDef.AppendLine("\t\tfloat3 vCameraToPositionDirWs = CalculateCameraToPositionDirWs( vPositionWs.xyz );");
            funcDef.AppendLine("\t\tfloat alpha = 1;");

            if (isTerrain) //variables are different for terrain for whatever reason, kinda have to guess
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {i.o0,1};"); //Detail uv?
                funcDef.AppendLine("\t\tfloat4 v1 = {i.o0*0.05,1};"); //Main uv?
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vNormalWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTangentUWs,1};");
                funcDef.AppendLine("\t\tfloat4 v4 = {i.vTangentVWs,1};");
                funcDef.AppendLine("\t\tfloat4 v5 = {0,0,0,0};");
            }
            else
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {i.vNormalWs,1};"); //Mesh world normals
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTangentUWs,1};");
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vTangentVWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTextureCoords,0,0};"); //UVs
                funcDef.AppendLine("\t\tfloat4 v4 = {(vPositionWs)/39.37,0};"); //Don't really know, just guessing its world offset or something
                funcDef.AppendLine("\t\tfloat4 v5 = i.vBlendValues;"); //Vertex color.
                //funcDef.AppendLine("uint v6 = 1;"); //Usually FrontFace but can also be v7
            }
            
            foreach (var i in inputs)
            {
                switch(i.Type)
                {
                    case "uint":
                        if (i.Semantic == "SV_isFrontFace0")
                            funcDef.AppendLine($"\t\tint {i.Variable} = i.face;");
                        else
                            funcDef.AppendLine($"\t\tint {i.Variable} = 1;");
                        break;
                    case "float4":
                        if (i.Semantic == "SV_POSITION0" && i.Variable != "v5")
                            funcDef.AppendLine($"\t\tfloat4 {i.Variable} = i.vPositionSs;");
                        break;
                }
            }
            funcDef.AppendLine("\t\tfloat4 o0 = float4(0,0,0,0);");
            funcDef.AppendLine("\t\tfloat4 o1 = float4(PackNormal3D(v0.xyz),1);");
            funcDef.AppendLine("\t\tfloat4 o2 = float4(0,0.5,0,0);\n");

            //if(cbuffers.Any(cbuffer => cbuffer.Index == 13 && cbuffer.Count == 2)) //Should be time but probably gets manipulated somehow
            //{
            //    funcDef.AppendLine("\t\tcb13[0] = g_flTime.xxxx;");
            //    funcDef.AppendLine("\t\tcb13[1] = g_flTime.xxxx;");
            //}
                
            string line = hlsl.ReadLine();
            if (line == null)
            {
                // its a broken pixel shader that uses some kind of memory textures
                return new StringBuilder();
            }
            while (!line.Contains("SV_TARGET0"))
            {
                line = hlsl.ReadLine();
                if (line == null)
                {
                    // its a broken pixel shader that uses some kind of memory textures
                    return new StringBuilder();
                }
            }
            while (!line.Contains("{"))
            {
                if (line.Contains("SV_TARGET2"))
                    bTranslucent = false;
                line = hlsl.ReadLine();
            }
            do
            {
                line = hlsl.ReadLine();
                if (line != null)
                {
                    if (line.Contains("cb12[7].xyz") || line.Contains("cb12[14].xyz")) //cb12 is view scope
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart().Replace("cb12[7].xyz", "vCameraToPositionDirWs")
                            .Replace("v4.xyz", "float3(0,0,0)")
                            .Replace("cb12[14].xyz", "vCameraToPositionDirWs")}");
                    }
                    else if (line.Contains("while (true)"))
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart().Replace("while (true)", "[unroll(20)] while (true)")}");
                    }
                    else if (line.Contains("return;"))
                    {
                        break;
                    }
                    else if (line.Contains("Sample"))
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                        var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                        var sampleUv = line.Split(", ")[1].Split(")")[0];
                        var dotAfter = line.Split(").")[1];
                        // todo add dimension

                        if (texIndex == 14 && isTerrain) //THIS IS SO SO BAD
                        {
                            funcDef.AppendLine($"\t\tbool red = i.vBlendValues.x > 0.5;\r\n" +
                                $"        bool green = i.vBlendValues.y > 0.5;\r\n" +
                                $"        bool blue = i.vBlendValues.z > 0.5;\r\n\r\n" +
                                $"        if (red && !green && !blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = Tex2DS(g_t14_0, TextureFiltering, {sampleUv}).{dotAfter};\r\n" +
                                $"        }}\r\n" +
                                $"        else if (!red && green && !blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = Tex2DS(g_t14_1, TextureFiltering, {sampleUv}).{dotAfter};\r\n" +
                                $"        }}\r\n" +
                                $"        else if (!red && !green && blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = Tex2DS(g_t14_2, TextureFiltering, {sampleUv}).{dotAfter};\r\n" +
                                $"        }}\r\n" +
                                $"        else if (red && green && blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = Tex2DS(g_t14_3, TextureFiltering, {sampleUv}).{dotAfter};\r\n" +
                                $"        }}");
                        }
                        else if(!material.EnumeratePSTextures().Any(texture => texture.TextureIndex == texIndex)) //Some kind of buffer texture
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,1,1,1).{dotAfter}"); 
                        }
                        else
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.Sample(g_s{sampleIndex}, {sampleUv}).{dotAfter}");
                        }
                    }
                    else if (line.Contains("CalculateLevelOfDetail"))
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".CalculateLevelOfDetail")[0].Split("t")[1]);
                        var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                        var sampleUv = line.Split(", ")[1].Split(")")[0];

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.CalculateLevelOfDetail(g_s{sampleIndex}, {sampleUv});");
                    }
                    else if (line.Contains("t2.Load")) //Pretty sure this is normal buffer, cant get/use in Forward Rendering...
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                        var sampleUv = line.Split("(")[1].Split(")")[0];
                        var dotAfter = line.Split(").")[1];
                        bUsesNormalBuffer = true;

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= v0.{dotAfter}");
                    }
                    else if (line.Contains("o0.w = r")) //o0.w = r(?)
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart()}");
                        funcDef.AppendLine($"\t\talpha = 1 - o0.w;");
                    }
                    else if (line.Contains("discard"))
                    {
                        funcDef.AppendLine(line.Replace("discard", "\t\t{ alpha = 0; }")); //sometimes o0.w is used for alpha instead on some shaders
                    }
                    else if (line.Contains("o1.xyzw = float4(0,0,0,0);"))
                    {
                        funcDef.AppendLine(line.Replace("o1.xyzw = float4(0,0,0,0);", "\t\to1.xyzw = float4(PackNormal3D(v0.xyz),0);")); //decals(?) have 0 normals sometimes, dont want that
                        bFixRoughness = true;
                    }
                    else
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart()}");
                    }
                    funcDef.Replace("∞", "1.#INF");
                }
            } while (line != null);
        }
        return funcDef;
    }
}
