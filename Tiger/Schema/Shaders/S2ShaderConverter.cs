using System.Text;
using System.Text.RegularExpressions;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class S2ShaderConverter
{
    private StringReader hlsl;
    private StringBuilder vfx;
    private List<DXBCIOSignature> inputs = new();
    private List<DXBCIOSignature> outputs = new();
    private List<DXBCShaderResource> resources = new();

    private bool isTerrain = false;
    private bool bRT0 = true;
    private bool bTranslucent = false;
    private bool bFixRoughness = false;

    private bool bUsesNormalBuffer = false;
    private bool bUsesFrameBuffer = false;
    private bool bUsesDepthBuffer = false;

    public string vfxStructure =
$@"HEADER
{{
	Description = ""Charm Auto-Generated S&Box Shader"";
}}

MODES
{{
	VrForward();

	Depth();

	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( ""vr_tools_wireframe.shader"" );
	ToolsShadingComplexity( ""tools_shading_complexity.shader"" );

	//Reflection( S_MODE_REFLECTIONS );
}}

FEATURES
{{
    #include ""common/features.hlsl""
    //Feature( F_DYNAMIC_REFLECTIONS, 0..1, ""Rendering"" );
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
	float3 v3                : TEXCOORD15; //terrain specific
	float3 v4                : TEXCOORD16; //terrain specific
    float3 v5                : TEXCOORD17; //terrain specific
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

	PixelInput MainVs( VertexInput i )
	{{
		PixelInput o = ProcessVertex( i );
        float4 r0,r1,r2;
        o.vBlendValues = i.vColorBlendValues;
		o.vBlendValues.a = i.vColorBlendValues.a;

//vs_Function

		return FinalizeVertex( o );
	}}
}}

PS
{{
    #include ""common/pixel.hlsl""
    //#include ""raytracing/reflections.hlsl""
    #define CUSTOM_TEXTURE_FILTERING
    #define cmp -
    //RenderState

    //#if ( S_MODE_REFLECTIONS )
	//	#define FinalOutput ReflectionOutput
	//#else
	//	#define FinalOutput float4
	//#endif

//ps_samplers
//ps_CBuffers
//ps_Inputs

    float4 MainPs( PixelInput i ) : SV_Target0
    {{
//ps_Function

//ps_output
    }}
}}";

    public string HlslToVfx(IMaterial material, string pixel, string vertex, MaterialType type, bool bIsTerrain = false)
    {
        //Pixel Shader
        StringBuilder texSamples = new StringBuilder();
        hlsl = new StringReader(pixel);
        vfx = new StringBuilder();
        isTerrain = bIsTerrain;
        inputs = material.PixelShader.InputSignatures;
        outputs = material.PixelShader.OutputSignatures;
        resources = material.PixelShader.Resources;
        bTranslucent = type == MaterialType.Transparent || outputs.Count == 1;

        //ProcessHlslData();

        if (inputs.Any(input => input.Semantic == DXBCSemantic.SystemIsFrontFace))
            vfxStructure = vfxStructure.Replace("//frontface", "#define S_RENDER_BACKFACES 1");

        for (int i = 0; i < material.PS_Samplers.Count; i++)
        {
            if (material.PS_Samplers[i] is null)
                continue;

            var sampler = material.PS_Samplers[i].Sampler;
            texSamples.AppendLine($"\tSamplerState s_s{i + 1} < Filter({sampler.Filter}); AddressU({sampler.AddressU}); AddressV({sampler.AddressV}); AddressW({sampler.AddressW}); ComparisonFunc({sampler.ComparisonFunc}); MaxAniso({sampler.MaxAnisotropy}); >;");
        }

        vfxStructure = vfxStructure.Replace("//ps_samplers", texSamples.ToString());
        vfxStructure = vfxStructure.Replace("//ps_CBuffers", WriteCbuffers(material, false).ToString());

        hlsl = new StringReader(pixel);
        StringBuilder instructions = ConvertInstructions(material, false);
        if (instructions.ToString().Length == 0)
            return "";

        vfxStructure = vfxStructure.Replace("//ps_Function", instructions.ToString());
        vfxStructure = vfxStructure.Replace("//ps_Inputs", WriteFunctionDefinition(material, false).ToString());

        if (bTranslucent) //This way is stupid but it works
            vfxStructure = vfxStructure.Replace("//alpha", $"#ifndef S_ALPHA_TEST\r\n\t#define S_ALPHA_TEST 0\r\n\t#endif\r\n\t#ifndef S_TRANSLUCENT\r\n\t#define S_TRANSLUCENT 1\r\n\t#endif");

        vfxStructure = vfxStructure.Replace("//ps_output", AddOutput(material).ToString());

        if (isTerrain)
            vfxStructure = vfxStructure.Replace("//vs_Function", "// Terrain specific\r\n\t\tr1.xyz = float3(0,1,0) * i.vNormalOs.yzx;\r\n\t\tr1.xyz = i.vNormalOs.zxy * float3(0,0,1) + -r1.xyz;\r\n\t\tr0.z = dot(r1.yz, r1.yz);\r\n\t\tr0.z = rsqrt(r0.z);\r\n\t\tr1.xyz = r1.xyz * r0.zzz;\r\n\t\tr2.xyz = i.vNormalOs.zxy * r1.yzx;\r\n\t\tr2.xyz = i.vNormalOs.yzx * r1.zxy + -r2.xyz;\r\n\t\to.v4.xyz = r1.xyz;\r\n\t\tr0.z = dot(r2.xyz, r2.xyz);\r\n\t\tr0.z = rsqrt(r0.z);\r\n\t\to.v3.xyz = r2.xyz * r0.zzz;\r\n\t\tr1.xyz = abs(i.vNormalOs.xyz) * abs(i.vNormalOs.xyz);\r\n\t\tr1.xyz = r1.xyz * r1.xyz;\r\n\t\tr2.xyz = r1.xyz * r1.xyz;\r\n\t\tr2.xyz = r2.xyz * r2.xyz;\r\n\t\tr1.xyz = r2.xyz * r1.xyz;\r\n\t\tr0.z = dot(r1.xyz, float3(1,1,1));\r\n\t\to.v5.xyz = r1.xyz / r0.zzz;");

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

    //private void ProcessHlslData()
    //{
    //    string line = string.Empty;
    //    cbuffers.Clear();
    //    textures.Clear();
    //    inputs.Clear();
    //    bool bFindOpacity = false;
    //    do
    //    {
    //        line = hlsl.ReadLine();
    //        if (line != null)
    //        {
    //            if (line.Contains("r0,r1")) // at end of function definition
    //            {
    //                bFindOpacity = true;
    //            }

    //            if (bFindOpacity)
    //            {
    //                if (line.Contains("discard") || line.Contains("o0.w = r"))
    //                {
    //                    //bOpacityEnabled = true;
    //                    break;
    //                }
    //                continue;
    //            }

    //            if (line.Contains("Texture"))
    //            {
    //                TextureView texture = new TextureView();
    //                texture.Dimension = line.Split("<")[0];
    //                texture.Type = line.Split("<")[1].Split(">")[0];
    //                texture.Variable = line.Split("> ")[1].Split(" :")[0];
    //                texture.Index = Int32.TryParse(new string(texture.Variable.Skip(1).ToArray()), out int index) ? index : -1;
    //                textures.Add(texture);
    //            }
    //            else if (line.Contains("SamplerState"))
    //            {
    //                samplers.Add(line.Split("(")[1].Split(")")[0].Last() - 48);
    //            }
    //            else if (line.Contains("Buffer"))
    //            {
    //                Shaders.Buffer buffer = new Shaders.Buffer();
    //                buffer.Type = line.Split('<', '>')[1];
    //                buffer.Variable = line.Split(' ')[1];
    //                buffer.Index = Int32.TryParse(new string(buffer.Variable.Skip(1).ToArray()), out int index) ? index : -1;
    //                buffers.Add(buffer);
    //            }
    //            else if (line.Contains("cbuffer"))
    //            {
    //                hlsl.ReadLine();
    //                line = hlsl.ReadLine();
    //                Cbuffer cbuffer = new Cbuffer();
    //                cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
    //                cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
    //                cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
    //                cbuffer.Type = line.Split("cb")[0].Trim();
    //                cbuffers.Add(cbuffer);
    //            }
    //            else if (line.Contains(" v") && line.Contains(" : ") && !line.Contains("?"))
    //            {
    //                Input input = new Input();
    //                input.Variable = "v" + line.Split("v")[1].Split(" : ")[0];
    //                input.Index = Int32.TryParse(new string(input.Variable.Skip(1).ToArray()), out int index) ? index : -1;
    //                input.Semantic = line.Split(" : ")[1].Split(",")[0];
    //                if (input.Semantic == "SV_isFrontFace0")
    //                    bUsesFrontFace = true;
    //                input.Type = line.Split(" v")[0].Trim();
    //                inputs.Add(input);
    //            }
    //            else if (line.Contains("out") && line.Contains(" : "))
    //            {
    //                Output output = new Output();
    //                output.Variable = "o" + line.Split(" o")[2].Split(" : ")[0];
    //                output.Index = Int32.TryParse(new string(output.Variable.Skip(1).ToArray()), out int index) ? index : -1;
    //                output.Semantic = line.Split(" : ")[1].Split(",")[0];
    //                output.Type = line.Split("out ")[1].Split(" o")[0];
    //                outputs.Add(output);
    //            }
    //        }

    //    } while (line != null);
    //}

    private StringBuilder WriteCbuffers(IMaterial material, bool bIsVertexShader)
    {
        StringBuilder CBuffers = new();

        foreach (var resource in resources)
        {
            switch (resource.ResourceType)
            {
                case ResourceType.Buffer:
                    CBuffers.AppendLine($"\tBuffer<float4> b_t{resource.Index} : register(t{resource.Index});");
                    break;
                case ResourceType.CBuffer:
                    if (bIsVertexShader)
                    {
                        for (int i = 0; i < resource.Count; i++)
                        {
                            CBuffers.AppendLine($"\tfloat4 vs_cb{resource.Index}_{i} < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( \"vs_cb{resource.Index}/{i}\"); >;");
                        }
                    }
                    else
                    {
                        if (resource.Index != 12)
                        {
                            for (int i = 0; i < resource.Count; i++)
                            {
                                CBuffers.AppendLine($"\tfloat4 cb{resource.Index}_{i} < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( \"cb{resource.Index}/{i}\"); >;");
                            }
                        }
                    }
                    break;
            }
        }

        return CBuffers;
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
                funcDef.AppendLine($"\tCreateTexture2DWithoutSampler( g_t14 ) < Attribute( \"TerrainDyemap\" ); SrgbRead( false ); >;\r\n\n");
            }

            if (bUsesFrameBuffer)
            {
                funcDef.AppendLine($"\tBoolAttribute( bWantsFBCopyTexture, true );");
                funcDef.AppendLine($"\tCreateTexture2D( g_tFrameBufferCopyTexture ) < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;");
            }
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
                switch (i.Semantic)
                {
                    case DXBCSemantic.Position:
                        funcDef.AppendLine($"\t\tfloat4 v{i.BufferIndex} = float4(i.vPositionOs, 0); //{i.ToString()}");
                        break;
                    case DXBCSemantic.Tangent:
                        funcDef.AppendLine($"\t\tfloat4 v{i.BufferIndex} = i.vTangentUOs; //{i.ToString()}");
                        break;
                    case DXBCSemantic.Texcoord:
                        funcDef.AppendLine($"\t\tfloat4 v{i.BufferIndex} = float4(i.vTexCoord, 0, 0); //{i.ToString()}");
                        break;
                    case DXBCSemantic.Normal:
                        funcDef.AppendLine($"\t\tfloat4 v{i.BufferIndex} = i.vNormalOs; //{i.ToString()}");
                        break;
                    case DXBCSemantic.SystemVertexId:
                        funcDef.AppendLine($"\t\tuint v{i.BufferIndex} = i.vVertexID; //{i.ToString()}");
                        break;
                    case DXBCSemantic.SystemInstanceId:
                        funcDef.AppendLine($"\t\tuint v{i.BufferIndex} = i.vInstanceID; //{i.ToString()}");
                        break;
                    default:
                        funcDef.AppendLine($"\t\t{i.GetMaskType()} v{i.BufferIndex} = float4(1,1,1,1).{i.Mask.ToString().ToLower()}; //{i.Semantic}");
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

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= Tex2DS(g_t{texIndex}, s_s{sampleIndex}, {sampleUv}).{dotAfter}");
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
            //Need to divde by 39.37 to convert to meters
            funcDef.AppendLine("\t\tfloat3 vPositionWs = (i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz) / 39.37;");
            funcDef.AppendLine("\t\tfloat alpha = 1;");

            if (isTerrain) //variables are different for terrain for whatever reason, kinda have to guess
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {vPositionWs, 1};"); //Detail uv?
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTextureCoords, 1, 1};"); //Main uv?
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vNormalWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.v3,1};");
                funcDef.AppendLine("\t\tfloat4 v4 = {i.v4,1};");
                funcDef.AppendLine("\t\tfloat4 v5 = {i.v5,1};");
            }
            else
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {i.vNormalWs,1};"); //Mesh world normals
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTangentUWs,1};");
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vTangentVWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTextureCoords,0,0};"); //UVs
                funcDef.AppendLine("\t\tfloat4 v4 = {vPositionWs,0};"); //Don't really know, just guessing its world offset or something
                //funcDef.AppendLine("\t\tfloat4 v5 = i.vBlendValues;"); //Vertex color.
                //funcDef.AppendLine("uint v6 = 1;"); //Usually FrontFace but can also be v7
            }

            foreach (var i in inputs)
            {
                switch (i.GetMaskType())
                {
                    case "uint":
                        if (i.Semantic == DXBCSemantic.SystemIsFrontFace)
                            funcDef.AppendLine($"\t\tint v{i.RegisterIndex} = i.face;");
                        else
                            funcDef.AppendLine($"\t\tint v{i.RegisterIndex} = 1;");
                        break;
                    case "float4":
                        if (i.Semantic == DXBCSemantic.SystemPosition)
                            funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = i.vPositionSs;");
                        else if (i.RegisterIndex == 5 && i.Semantic == DXBCSemantic.Texcoord && !isTerrain)
                            funcDef.AppendLine($"\t\tfloat4 v5 = i.vBlendValues;");
                        //else
                        //    funcDef.AppendLine($"\t\tfloat4 {i.Variable} = float4(1,1,1,1);");
                        break;
                    case "float":
                        funcDef.AppendLine($"\t\tfloat v{i.RegisterIndex} = 1;");
                        break;
                }
            }
            funcDef.AppendLine("\t\tfloat4 o0 = float4(0,0,0,0);");
            funcDef.AppendLine("\t\tfloat4 o1 = float4(PackNormal3D(v0.xyz),1);");
            funcDef.AppendLine("\t\tfloat4 o2 = float4(0,0.5,0,0);");
            funcDef.AppendLine("\t\tfloat4 o3 = float4(0,0,0,0);\n");

            if (resources.Any(cbuffer => (cbuffer.ResourceType == ResourceType.CBuffer && cbuffer.Index == 12)))
            {
                funcDef.AppendLine(AddViewScope());
            }

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
                    bRT0 = false;
                line = hlsl.ReadLine();
            }
            do
            {
                //Doing these line replacements is really messy and needs to be replaced with a better method, someday
                line = hlsl.ReadLine();
                if (line != null)
                {
                    if (line.Contains("cb") && !line.Contains("Sample"))
                    {
                        string pattern = @"cb(\d+)\[(\d+)\]"; // Matches cb#[#]
                        string output = Regex.Replace(line, pattern, match =>
                        {
                            // Extract the values from the matched groups
                            string group1 = match.Groups[1].Value; // cbuffer index
                            string group2 = match.Groups[2].Value; // cbuffer array index

                            if (group1 != "12")
                            {
                                // Replace with the actual values of group1 and group2
                                return isVertexShader ? $"vs_cb{group1}_{group2}" : $"cb{group1}_{group2}";
                            }
                            else
                            {
                                // If group1 is "12", don't replace
                                return match.Value;
                            }
                        });

                        // Append the modified line to funcDef
                        funcDef.AppendLine($"\t\t{output.TrimStart()}");
                    }
                    else if (line.Contains("while (true)"))
                    {
                        funcDef.AppendLine($"\t\t{line.TrimStart().Replace("while (true)", "[loop] while (true)")}");
                    }
                    else if (line.Contains("return;"))
                    {
                        break;
                    }
                    else if (line.Contains("Sample"))
                    {
                        var equal = line.Split("=")[0];
                        var equal_post = line.Split("=")[1];
                        var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                        var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                        var sampleUv = line.Split(", ")[1].Split(").")[0];
                        var dotAfter = line.Split(").")[1];

                        if (sampleUv.Contains("cb")) //Rare case where a cbuffer value is used as a texcoord
                        {
                            string pattern = @"cb(\d+)\[(\d+)\]"; // Matches cb#[#]
                            sampleUv = Regex.Replace(sampleUv, pattern, match =>
                            {
                                // Extract the values from the matched groups
                                string group1 = match.Groups[1].Value; // cbuffer index
                                string group2 = match.Groups[2].Value; // cbuffer array index

                                if (group1 != "12")
                                {
                                    // Replace with the actual values of group1 and group2
                                    return isVertexShader ? $"vs_cb{group1}_{group2}" : $"cb{group1}_{group2}";
                                }
                                else
                                {
                                    // If group1 is "12", don't replace
                                    return match.Value;
                                }
                            });
                        }


                        if (texIndex == 14 && isTerrain) //THIS IS SO SO BAD
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()} = g_t{texIndex}.Sample(s_s{sampleIndex}, {sampleUv}).{dotAfter}");
                        }
                        else if (!material.EnumeratePSTextures().Any(texture => texture.TextureIndex == texIndex)) //Some kind of buffer texture
                        {
                            switch (texIndex)
                            {
                                case 10: //Depth
                                    bUsesDepthBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= Depth::Get({sampleUv}).xxxx; //{equal_post}");
                                    break;
                                case 11:
                                case 13:
                                case 23: //Usually uses SampleLevel but shouldnt be an issue?
                                    bUsesFrameBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_tFrameBufferCopyTexture.Sample(s_s{sampleIndex}, {sampleUv}).{dotAfter} //{equal_post}");
                                    break;
                                case 15:
                                case 20:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.3137,0.3137,0.3137,0.3137).{dotAfter} //{equal_post}");
                                    break;
                                case 0:
                                case 21:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.1882,0.1882,0.1882,0.1882).{dotAfter} //{equal_post}");
                                    break;
                                default:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.5,0.5,0.5,0.5).{dotAfter} //{equal_post}");
                                    break;
                            }
                        }
                        else
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.Sample(s_s{sampleIndex}, {sampleUv}).{dotAfter}");
                        }
                    }
                    else if (line.Contains("CalculateLevelOfDetail"))
                    {
                        var equal = line.Split("=")[0];
                        var texIndex = Int32.Parse(line.Split(".CalculateLevelOfDetail")[0].Split("t")[1]);
                        var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                        var sampleUv = line.Split(", ")[1].Split(")")[0];

                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.CalculateLevelOfDetail(s_s{sampleIndex}, {sampleUv});");
                    }
                    else if (line.Contains("Load"))
                    {
                        var equal = line.Split("=")[0];
                        var equal_post = line.Split("=")[1];
                        var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                        var sampleUv = line.Split("(")[1].Split(")")[0];
                        var dotAfter = line.Split(").")[1];

                        if (!material.EnumeratePSTextures().Any(texture => texture.TextureIndex == texIndex)) //Some kind of buffer texture
                        {
                            switch (texIndex)
                            {
                                case 2:
                                    bUsesNormalBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= v0.{dotAfter}");
                                    break;
                                case 10:
                                    bUsesDepthBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= Depth::Get({sampleUv}).xxxx; //{equal_post}");
                                    break;
                                case 11:
                                case 13:
                                case 23: //Usually uses SampleLevel but shouldnt be an issue?
                                    bUsesFrameBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_tFrameBufferCopyTexture.Load({sampleUv}).{dotAfter} //{equal_post}");
                                    break;
                                case 15:
                                case 20:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.3137,0.3137,0.3137,0.3137).{dotAfter} //{equal_post}");
                                    break;
                                case 0:
                                case 21:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.1882,0.1882,0.1882,0.1882).{dotAfter} //{equal_post}");
                                    break;
                                default:
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.5,0.5,0.5,0.5).{dotAfter} //{equal_post}");
                                    break;
                            }
                        }
                        else
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.Load({sampleUv}).{dotAfter}");
                        }
                    }
                    //else if (line.Contains("o0.w = r")) //o0.w = r(?)
                    //{
                    //    funcDef.AppendLine($"\t\t{line.TrimStart()}");
                    //    //funcDef.AppendLine($"\t\talpha = 1 - o0.w;");
                    //}
                    //else if (line.Contains("discard"))
                    //{
                    //    funcDef.AppendLine(line.Replace("discard", "\t\t{ alpha = 0; }"));
                    //}
                    else if (line.Contains("o1.xyzw = float4(0,0,0,0);"))
                    {
                        funcDef.AppendLine(line.Replace("o1.xyzw = float4(0,0,0,0);", "\t\to1.xyzw = float4(PackNormal3D(v0.xyz),0);")); //decals(?) have 0 normals sometimes, dont want that
                        bFixRoughness = true;
                    }
                    else if (line.Contains("GetDimensions")) //Uhhhh
                    {
                        funcDef.AppendLine($"\t\t//{line.TrimStart()}");
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

        if (!bRT0) //uses o1,o2
        {
            //this is fine...
            output.Append($"\t\t// Normal\r\n        " +
                $"float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);\r\n        " +
                $"float normal_length = length(biased_normal);\r\n        " +
                $"float3 normal_in_world_space = biased_normal / normal_length;\r\n\r\n        " +
                $"float smoothness = saturate(8 * (normal_length - 0.375));\r\n        \r\n        " +
                $"Material mat = Material::From(i,\r\n                    " +
                $"float4(o0.xyz, alpha), //albedo, alpha\r\n                    " +
                $"float4(0.5, 0.5, 1, 1), //Normal, gets set later\r\n                    " +
                $"float4(1 - smoothness, saturate(o2.x), saturate(o2.y * 2), 1), //rough, metal, ao\r\n                    " +
                $"float3(1.0f, 1.0f, 1.0f), //tint\r\n                    " +
                $"clamp((o2.y - 0.5) * 2 * 6 * o0.xyz, 0, 100)); //emission\r\n\r\n        " +
                $"mat.Transmission = o2.z;\r\n        " +
                $"mat.Normal = normal_in_world_space; //Normal is already in world space so no need to convert in Material::From");

            output.Append($"\n\t\treturn ShadingModelStandard::Shade(i, mat);");

            if (bFixRoughness)
                output = output.Replace("float smoothness = saturate(8 * (normal_length - 0.375));", "float smoothness = saturate(8 * (0 - 0.375));");

            if (bUsesNormalBuffer) //Cant get normal buffer in forward rendering so just use world normal ig...
                output = output.Replace("mat.Normal = normal_in_world_space;", $"mat.Normal = v0;");
        }
        else //only uses o0
        {
            bool a = bUsesNormalBuffer || bTranslucent || bUsesFrameBuffer || bUsesDepthBuffer;

            if (a) //??
            {
                //output.Append($"\t\treturn float4(o0.xyz, {(bUsesFrameBuffer ? "1" : "alpha")});");
                output.Append($"\t\treturn float4(o0.xyz, 1);");
            }
            else
            {
                output.AppendLine($"\t\tMaterial mat = Material::From(i, float4(o0.xyz, 1), float4(0.5, 0.5, 1, 1), float4(0.5, 0, 1, 1), float3(1.0f, 1.0f, 1.0f), 0);");
                output.AppendLine($"\t\treturn ShadingModelStandard::Shade(i, mat);");
            }
        }

        return output;
    }

    private string AddViewScope()
    {
        StringBuilder viewScope = new StringBuilder();
        viewScope.AppendLine($"\t\tfloat4 cb12[15] = {{");

        viewScope.AppendLine($"\t\tg_matWorldToProjection,"); //0

        viewScope.AppendLine($"\t\tfloat4(1,0,0,0),"); //4
        viewScope.AppendLine($"\t\tfloat4(0,1,0,0),");
        viewScope.AppendLine($"\t\tfloat4(0,0,1,0),");
        viewScope.AppendLine($"\t\tfloat4(g_vCameraPositionWs/39.37,1),");

        viewScope.AppendLine($"\t\tfloat4(0.5,0,0,0),"); //8
        viewScope.AppendLine($"\t\tfloat4(0,1,0,0),");
        viewScope.AppendLine($"\t\tfloat4(0,0,1,0),");
        viewScope.AppendLine($"\t\tfloat4(-100,-100,-100,1),");

        viewScope.AppendLine($"\t\tfloat4(g_vFrameBufferCopyInvSizeAndUvScale.xy, 1/g_vFrameBufferCopyInvSizeAndUvScale.xy),"); //12
        viewScope.AppendLine($"\t\tfloat4(1,0,0,0),"); //13
        viewScope.AppendLine($"\t\tfloat4(g_vCameraPositionWs/39.37,1)"); //14

        viewScope.AppendLine($"\t\t}};");

        return viewScope.ToString();
    }
}
