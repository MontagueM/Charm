using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    float3 v5                : TEXCOORD15; //terrain specific
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
        float4 r0,r1,r2;
        o.vBlendValues = i.vColorBlendValues;
		o.vBlendValues.a = i.vColorBlendValues.a;

//vs_Function
        {(isTerrain ? "  r1.xyz = abs(i.vNormalOs.xyz) * abs(i.vNormalOs.xyz);\r\n  r1.xyz = r1.xyz * r1.xyz;\r\n  r2.xyz = r1.xyz * r1.xyz;\r\n  r2.xyz = r2.xyz * r2.xyz;\r\n  r1.xyz = r2.xyz * r1.xyz;\r\n  r0.z = dot(r1.xyz, float3(1,1,1));\r\n  o.v5.xyz = r1.xyz / r0.zzz;" : "")}
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

//ps_samplers
//ps_CBuffers
//ps_Inputs

    FinalOutput MainPs( PixelInput i ) : SV_Target0
    {{
//ps_Function

//ps_output
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
        }

        for (int i = 0; i < material.PS_Samplers.Count; i++)
        {
            if (material.PS_Samplers[i] is null)
                continue;

            var sampler = material.PS_Samplers[i].Sampler;
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

        if (bOpacityEnabled || bTranslucent) //This way is stupid but it works
        {
            vfxStructure = vfxStructure.Replace("//alpha", $"#ifndef S_ALPHA_TEST\r\n\t#define S_ALPHA_TEST {((bUsesNormalBuffer || bTranslucent) ? "0" : "1")}\r\n\t#endif\r\n\t#ifndef S_TRANSLUCENT\r\n\t#define S_TRANSLUCENT {((bUsesNormalBuffer || bTranslucent) ? "1" : "0")}\r\n\t#endif");
            if (bTranslucent)
                vfxStructure = vfxStructure.Replace("FinalOutput MainPs", "float4 MainPs");
        }

        vfxStructure = vfxStructure.Replace("//ps_output", AddOutput(material).ToString());

        //------------------------------------------------------------------------------

        //Vertex Shader - Commented out for now
        //texSamples = new StringBuilder();
        //hlsl = new StringReader(vertex);

        //ProcessHlslData();

        //for (int i = 0; i < material.VS_Samplers.Count; i++)
        //{
        //    if (material.VS_Samplers[i] is null)
        //        continue;

        //    var sampler = material.VS_Samplers[i].Sampler;
        //    texSamples.AppendLine($"SamplerState g_s{i + 1} < Filter({sampler.Filter}); AddressU({sampler.AddressU}); AddressV({sampler.AddressV}); AddressW({sampler.AddressW}); ComparisonFunc({sampler.ComparisonFunc}); MaxAniso({sampler.MaxAnisotropy}); >;");
        //}

        //vfxStructure = vfxStructure.Replace("//vs_samplers", texSamples.ToString());

        //vfxStructure = vfxStructure.Replace("//vs_Inputs", WriteFunctionDefinition(material, true).ToString());
        //vfxStructure = vfxStructure.Replace("//vs_CBuffers", WriteCbuffers(material, true).ToString());

        //hlsl = new StringReader(vertex);
        //instructions = ConvertInstructions(material, true);
        //vfxStructure = vfxStructure.Replace("//vs_Function", instructions.ToString());

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

        foreach (var buffer in buffers)
        {
            CBuffers.AppendLine($"\tBuffer<{buffer.Type}> b_{buffer.Variable} : register({buffer.Variable});");
        }

        foreach (var cbuffer in cbuffers)
        {
            //CBuffers.AppendLine($"\tstatic {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine("\t{");

            dynamic data = null;
            if (bIsVertexShader)
            {
                for (int i = 0; i < cbuffer.Count; i++)
                {
                    CBuffers.AppendLine($"\tfloat4 vs_cb{cbuffer.Index}_{i} < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( \"vs_cb{cbuffer.Index}/{i}\"); >;");
                }
            }
            else
            {
                for (int i = 0; i < cbuffer.Count; i++)
                {
                    CBuffers.AppendLine($"\tfloat4 cb{cbuffer.Index}_{i} < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( \"cb{cbuffer.Index}/{i}\"); >;");
                }
            }
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
                funcDef.AppendLine($"\tTexture2D g_t14_2 < Channel( RGBA,  Box( TextureT14_2 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
                funcDef.AppendLine($"\tTextureAttribute(g_t14_2, g_t14_2);\n");

                funcDef.AppendLine($"\tCreateInputTexture2D( TextureT14_3, Linear, 8, \"\", \"\",  \"Textures,10/17\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture2D g_t14_3 < Channel( RGBA,  Box( TextureT14_3 ), Linear ); OutputFormat( RGBA8888 ); SrgbRead( False ); >; ");
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
                switch(i.Semantic)
                {
                    case "POSITION0":
                        funcDef.AppendLine($"\t\tfloat4 {i.Variable} = float4(i.vPositionOs, 0); //{i.Semantic}");
                        break;
                    case "TANGENT0":
                        funcDef.AppendLine($"\t\tfloat4 {i.Variable} = i.vTangentUOs; //{i.Semantic}");
                        break;
                    case "TEXCOORD0":
                        funcDef.AppendLine($"\t\tfloat4 {i.Variable} = float4(i.vTexCoord, 0, 0); //{i.Semantic}");
                        break;
                    case "NORMAL0":
                        funcDef.AppendLine($"\t\tfloat4 {i.Variable} = i.vNormalOs; //{i.Semantic}");
                        break;
                    case "SV_VERTEXID0":
                        funcDef.AppendLine($"\t\tuint {i.Variable} = i.vVertexID; //{i.Semantic}");
                        break;
                    case "SV_InstanceID0":
                        funcDef.AppendLine($"\t\tuint {i.Variable} = i.vInstanceID; //{i.Semantic}");
                        break;
                    default:
                        funcDef.AppendLine($"\t\t{i.Type} {i.Variable} = {i.Variable}; //{i.Semantic}");
                        break;
                }
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
                funcDef.AppendLine("\t\tfloat4 v0 = {vPositionWs/39.37, 1};"); //Detail uv?
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTextureCoords, 1, 1};"); //Main uv?
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vNormalWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTangentUWs,1};");
                funcDef.AppendLine("\t\tfloat4 v4 = {i.vTangentVWs,1};");
                funcDef.AppendLine("\t\tfloat4 v5 = {i.v5,1};");
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

            //if (cbuffers.Any(cbuffer => cbuffer.Index == 13 && cbuffer.Count == 2)) //Should be time (g_flTime) but probably gets manipulated somehow
            //{
            //    funcDef.AppendLine("\t\tcb13[0] = 1;");
            //    funcDef.AppendLine("\t\tcb13[1] = 1;");
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
                    else if (line.Contains("cb"))
                    {
                        string pattern = @"cb(\d+)\[(\d+)\]"; // Matches cb#[#]
                        string output = Regex.Replace(line, pattern, isVertexShader ? "vs_cb$1_$2" : "cb$1_$2");

                        funcDef.AppendLine($"\t\t{output.TrimStart()}");
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
                                $"            {equal} = g_t{texIndex}_0.Sample(g_s{sampleIndex}, {sampleUv}).{dotAfter}\r\n" +
                                $"        }}\r\n" +
                                $"        else if (!red && green && !blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = g_t{texIndex}_1.Sample(g_s{sampleIndex}, {sampleUv}).{dotAfter}\r\n" +
                                $"        }}\r\n" +
                                $"        else if (!red && !green && blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = g_t{texIndex}_2.Sample(g_s{sampleIndex}, {sampleUv}).{dotAfter}\r\n" +
                                $"        }}\r\n" +
                                $"        else if (red && green && blue)\r\n" +
                                $"        {{\r\n" +
                                $"            {equal} = g_t{texIndex}_3.Sample(g_s{sampleIndex}, {sampleUv}).{dotAfter}\r\n" +
                                $"        }}");
                        }
                        else if (!material.EnumeratePSTextures().Any(texture => texture.TextureIndex == texIndex)) //Some kind of buffer texture
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,1,1,1).{dotAfter} //t{texIndex}");
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
                    else if (line.Contains("Load")) 
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                        var sampleUv = line.Split("(")[1].Split(")")[0];
                        var dotAfter = line.Split(").")[1];
                        
                        if (texIndex == 2) //Pretty sure this is normal buffer, cant get/use in Forward Rendering...
                        {
                            bUsesNormalBuffer = true;
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= v0.{dotAfter}");
                        }
                        else if (!material.EnumeratePSTextures().Any(texture => texture.TextureIndex == texIndex)) //Some kind of buffer texture
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,1,1,1).{dotAfter} //t{texIndex}.Load");
                        }
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

    private StringBuilder AddOutput(IMaterial material)
    {
        StringBuilder output = new StringBuilder();

        if(!bTranslucent) //uses o1,o2
        {
            //this is fine...
            output.Append($"\t\t// Normal\r\n        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);\r\n        float normal_length = length(biased_normal);\r\n        float3 normal_in_world_space = biased_normal / normal_length;\r\n\r\n        float smoothness = saturate(8 * (normal_length - 0.375));\r\n        \r\n        Material mat = Material::From(i,\r\n                    float4(o0.xyz, alpha), //albedo, alpha\r\n                    float4(0.5, 0.5, 1, 1), //Normal, gets set later\r\n                    float4(1 - smoothness, saturate(o2.x), saturate(o2.y * 2), 1), //rough, metal, ao\r\n                    float3(1.0f, 1.0f, 1.0f), //tint\r\n                    clamp((o2.y - 0.5) * 2 * 6 * o0.xyz, 0, 100)); //emission\r\n\r\n        mat.Transmission = o2.z;\r\n        mat.Normal = normal_in_world_space; //Normal is already in world space so no need to convert in Material::From\r\n\r\n        #if ( S_MODE_REFLECTIONS )\r\n\t\t{{\r\n\t\t\treturn Reflections::From( i, mat, SampleCountIntersection );\r\n\t\t}}\r\n        #else\r\n\t\t{{\r\n            return ShadingModelStandard::Shade(i, mat);\r\n        }}\r\n        #endif");

            if (bFixRoughness)
                output = output.Replace("float smoothness = saturate(8 * (normal_length - 0.375));", "float smoothness = saturate(8 * (0 - 0.375));");

            if (bUsesNormalBuffer) //Cant get normal buffer in forward rendering so just use world normal ig...
                output = output.Replace("mat.Normal = normal_in_world_space;", $"mat.Normal = v0;");
        }
        else //only uses o0
        {
            output.AppendLine($"\t\tMaterial mat = Material::From(i, float4(o0.xyz, alpha), float4(0.5, 0.5, 1, 1), float4(0.5, 0, 1, 1), float3(1.0f, 1.0f, 1.0f), 0);");
            output.AppendLine($"\t\treturn ShadingModelStandard::Shade(i, mat);");

            //if (material.Unk0C == 2) //Unlit?
            //{
            //    output.Append($"\t\treturn float4(o0.xyz, alpha);");
            //}
            //else
            //{
            //    output.AppendLine($"\t\tMaterial mat = Material::From(i, float4(o0.xyz, alpha), float4(0.5, 0.5, 1, 1), float4(0.5, 0, 1, 1), float3(1.0f, 1.0f, 1.0f), 0);");
            //    output.AppendLine($"\t\treturn ShadingModelStandard::Shade(i, mat);");
            //}
        }

        return output;
    }
}
