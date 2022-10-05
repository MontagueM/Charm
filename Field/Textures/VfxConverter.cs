using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Field.General;
using Field.Models;
//using Field;

namespace Field;

public class VfxConverter
{
    private StringReader hlsl;
    private StringBuilder vfx;
    private bool bOpacityEnabled = false;
    private List<Texture> textures = new List<Texture>();
    private List<int> samplers = new List<int>();
    private List<Cbuffer> cbuffers = new List<Cbuffer>();
    private List<Input> inputs = new List<Input>();
    private List<Output> outputs = new List<Output>();

    private readonly string[] sampleStates = {
        "SamplerState g_sWrap < Filter( ANISOTROPIC ); AddressU( WRAP ); AddressV( WRAP ); >;",
        "SamplerState g_sClamp < Filter( ANISOTROPIC ); AddressU( CLAMP ); AddressV( CLAMP ); >;",
        "SamplerState g_sMirror < Filter( ANISOTROPIC ); AddressU( MIRROR ); AddressV( MIRROR ); >;",
        "SamplerState g_sBorder < Filter( ANISOTROPIC ); AddressU( BORDER ); AddressV( BORDER ); >;}",
    };
    

    public string vfxStructure = @"HEADER
{
	Description = ""Charm Auto-Generated Source 2 Shader""; 
}

FEATURES
{
    #include ""common/features.hlsl""
    //alphatest
}

MODES
{
    VrForward();													
    Depth( ""vr_depth_only.vfx"" );
    ToolsVis( S_MODE_TOOLS_VIS ); 									
    ToolsWireframe( ""vr_tools_wireframe.vfx"" );
	ToolsShadingComplexity( ""vr_tools_shading_complexity.vfx"" );
}

COMMON
{
	#include ""common/shared.hlsl""
    //translucent
}

struct VertexInput
{
    float4 vColorBlendValues : TEXCOORD4 < Semantic( color ); >;
	#include ""common/vertexinput.hlsl""
};

struct PixelInput
{
    float4 vBlendValues		 : TEXCOORD14;
	#include ""common/pixelinput.hlsl""
};

VS
{
	#include ""common/vertex.hlsl""
	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )
	{
		PixelInput o = ProcessVertex( i );
        o.vBlendValues = i.vColorBlendValues;
		o.vBlendValues.a = i.vColorBlendValues.a;
		return FinalizeVertex( o );
	}
}

PS
{
    #include ""common/pixel.hlsl""
    #define cmp -
    //#define CUSTOM_TEXTURE_FILTERING // uncomment to use custom texture filtering
    //SamplerState g_sWrap < Filter( ANISOTROPIC ); AddressU( WRAP ); AddressV( WRAP ); >;
    //SamplerState g_sClamp < Filter( ANISOTROPIC ); AddressU( CLAMP ); AddressV( CLAMP ); >;
    //SamplerState g_sMirror < Filter( ANISOTROPIC ); AddressU( MIRROR ); AddressV( MIRROR ); >;
    //SamplerState g_sBorder < Filter( ANISOTROPIC ); AddressU( BORDER ); AddressV( BORDER ); >;

    RenderState(CullMode, F_RENDER_BACKFACES? NONE : DEFAULT );";


    public string HlslToVfx(Material material, string hlslText, bool bIsVertexShader)
    {
        //Console.WriteLine("Material: " + material.Hash);
        hlsl = new StringReader(hlslText);
        vfx = new StringBuilder();
       
        bOpacityEnabled = false;
        ProcessHlslData();
        if (bOpacityEnabled)
        {
            vfx.AppendLine("// masked");
            StringBuilder replace = new StringBuilder();
            replace.AppendLine(@"   Feature( F_ALPHA_TEST, 0..1, ""Rendering"" );").ToString();
            replace.AppendLine(@"   Feature( F_PREPASS_ALPHA_TEST, 0..1, ""Rendering"" );").ToString();
        
            vfxStructure = vfxStructure.Replace("//alphatest", replace.ToString());
            //vfxStructure = vfxStructure.Replace("//translucent", "#define S_TRANSLUCENT 1");
            //Turns out I dont need S_TRANSLUCENT to use alpha test
        }
        vfx.AppendLine(vfxStructure);

        WriteCbuffers(material, bIsVertexShader);
        WriteFunctionDefinition(material, bIsVertexShader);
        hlsl = new StringReader(hlslText);
        bool success = ConvertInstructions();
        if (!success)
        {
            return "";
        }

        if (!bIsVertexShader)
        {
            AddOutputs();
        }

        //WriteFooter(bIsVertexShader);
        return vfx.ToString();
    }

    private void ProcessHlslData()
    {
        string line = string.Empty;
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
                    if (line.Contains("discard"))
                    {
                        bOpacityEnabled = true;
                        break;
                    }
                    continue;
                }

                if (line.Contains("Texture"))
                {
                    Texture texture = new Texture();
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

    private void WriteCbuffers(Material material, bool bIsVertexShader)
    {
        // Try to find matches, pixel shader has Unk2D0 Unk2E0 Unk2F0 Unk300 available
        foreach (var cbuffer in cbuffers)
        {
            vfx.AppendLine($"   static {cbuffer.Type} {cbuffer.Variable}[{cbuffer.Count}] = ").AppendLine(" {");
            
            dynamic data = null;
            if (bIsVertexShader)
            {
                if (cbuffer.Count == material.Header.Unk90.Count)
                {
                    data = material.Header.Unk90;
                }
                else if (cbuffer.Count == material.Header.UnkA0.Count)
                {
                    data = material.Header.UnkA0;
                }
                else if (cbuffer.Count == material.Header.UnkB0.Count)
                {
                    data = material.Header.UnkB0;
                }
                else if (cbuffer.Count == material.Header.UnkC0.Count)
                {
                    data = material.Header.UnkC0;
                }
            }
            else
            {
                if (cbuffer.Count == material.Header.Unk2D0.Count)
                {
                    data = material.Header.Unk2D0;
                }
                else if (cbuffer.Count == material.Header.Unk2E0.Count)
                {
                    data = material.Header.Unk2E0;
                }
                else if (cbuffer.Count == material.Header.Unk2F0.Count)
                {
                    data = material.Header.Unk2F0;
                }
                else if (cbuffer.Count == material.Header.Unk300.Count)
                {
                    data = material.Header.Unk300;
                }
                else
                {
                    if (material.Header.PSVector4Container.Hash != 0xffff_ffff)
                    {
                        // Try the Vector4 storage file
                        DestinyFile container = new DestinyFile(PackageHandler.GetEntryReference(material.Header.PSVector4Container));
                        byte[] containerData = container.GetData();
                        int num = containerData.Length / 16;
                        if (cbuffer.Count == num)
                        {
                            List<Vector4> float4s = new List<Vector4>();
                            for (int i = 0; i < containerData.Length / 16; i++)
                            {
                                float4s.Add(StructConverter.ToStructure<Vector4>(containerData.Skip(i*16).Take(16).ToArray()));
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
                        if (data == null) vfx.AppendLine("      float4(0.0, 0.0, 0.0, 0.0),");
                        else
                        {
                            try
                            {
                                if (data[i] is Vector4)
                                {
                                    vfx.AppendLine($"       float4({data[i].X}, {data[i].Y}, {data[i].Z}, {data[i].W}),");
                                }
                                else
                                {
                                    var x = data[i].Unk00.X; // really bad but required
                                    vfx.AppendLine($"       float4({x}, {data[i].Unk00.Y}, {data[i].Unk00.Z}, {data[i].Unk00.W}),");
                                }
                            }
                            catch (Exception e)  // figure out whats up here, taniks breaks it
                            {
                                vfx.AppendLine("        float4(0.0, 0.0, 0.0, 0.0),");
                            }
                        }
                        break;
                    case "float3":
                        if (data == null) vfx.AppendLine("      float3(0.0, 0.0, 0.0),");
                        else vfx.AppendLine($"      float3({data[i].Unk00.X}, {data[i].Unk00.Y}, {data[i].Unk00.Z}),");
                        break;
                    case "float":
                        if (data == null) vfx.AppendLine("    float(0.0),");
                        else vfx.AppendLine($"      float4({data[i].Unk00}),");
                        break;
                    default:
                        throw new NotImplementedException();
                }  
            }

            vfx.AppendLine("    };");
        }
    }
    
    private void WriteFunctionDefinition(Material material, bool bIsVertexShader)
    {
        if (!bIsVertexShader)
        {
            foreach (var i in inputs)
            {
                if (i.Type == "float4")
                {
                    vfx.AppendLine($"   static {i.Type} {i.Variable} = " + "{1, 1, 1, 1};\n");
                }
                else if (i.Type == "float3")
                {
                    vfx.AppendLine($"   static {i.Type} {i.Variable} = " + "{1, 1, 1};\n");
                }
                else if (i.Type == "uint")
                {
                    vfx.AppendLine($"   static {i.Type} {i.Variable} = " + "1;\n");
                }
            }
        }

        if (bIsVertexShader)
        {
            foreach (var output in outputs)
            {
                vfx.AppendLine($"{output.Type} {output.Variable};");
            }

            //vfx.AppendLine().AppendLine("PixelInput MainVs( INSTANCED_SHADER_PARAMS( VS_INPUT i ) )");
        }
        else
        {
            foreach (var e in material.Header.PSTextures)
            {
                string type = "Srgb";
                if (e.Texture != null)
                {
                    if (e.Texture.IsSrgb())
                    {
                        type = "Srgb";
                    }
                    else
                    {
                        type = "Linear";
                    }

                    //vfx.AppendLine($"   {texture.Type} {texture.Variable},");
                    vfx.AppendLine($"   CreateInputTexture2D( TextureT{e.TextureIndex}, {type}, 8, \"\", \"\",  \"Textures,10/{e.TextureIndex}\", Default3( 1.0, 1.0, 1.0 ));");
                    vfx.AppendLine($"   CreateTexture2DWithoutSampler( g_t{e.TextureIndex} )  < Channel( RGBA,  Box( TextureT{e.TextureIndex} ), {type} ); OutputFormat( BC7 ); SrgbRead( {e.Texture.IsSrgb()} ); >; \n");
                }
            }

            if(bOpacityEnabled)
            {
                 vfx.AppendLine("    StaticCombo( S_ALPHA_TEST, F_ALPHA_TEST, Sys( PC ) );");
	             vfx.AppendLine("    StaticCombo( S_PREPASS_ALPHA_TEST, F_PREPASS_ALPHA_TEST, Sys( PC ) );");
            }

            vfx.AppendLine("    float4 MainPs( PixelInput i ) : SV_Target0");
            vfx.AppendLine("    {");

            // Output render targets, todo support vertex shader
            vfx.AppendLine("        float4 o0,o1,o2;");
            vfx.AppendLine("        float alpha = 1;");
            vfx.AppendLine("        float4 tx = float4(i.vTextureCoords, 1, 1);");

            vfx.AppendLine("        float4 v0 = {1,1,1,1};"); //Seems to only be used for normals.
            vfx.AppendLine("        float4 v1 = {i.vNormalWs, 1};"); //Pretty sure this is mesh normals
            vfx.AppendLine("        float4 v2 = {i.vTangentUWs, 1};"); //Tangent? Seems to only be used for normals.
            vfx.AppendLine("        float4 v3 = {i.vTextureCoords, 1,1};"); //seems only used as texture coords
            vfx.AppendLine("        float4 v4 = i.vBlendValues;"); //Not sure if this is VC or not
            vfx.AppendLine("        float4 v5 = i.vBlendValues;"); //seems like this is always the same as v4/only used if shader uses VC alpha
            //vfx.AppendLine("        uint v6 = 1;"); //no idea

            foreach (var i in inputs)
            {
                if (i.Type == "uint")
                {
                    vfx.AppendLine($"       {i.Variable}.x = {i.Variable}.x * tx.x;");
                }
            }
            // vfx.Replace("v0.xyzw = v0.xyzw * tx.xyzw;", "v0.xyzw = v0.xyzw;");
            // vfx.Replace("v1.xyzw = v1.xyzw * tx.xyzw;", "v1.xyzw = v1.xyzw;");
            // vfx.Replace("v2.xyzw = v2.xyzw * tx.xyzw;", "v2.xyzw = v2.xyzw;");
            // vfx.Replace("v5.xyzw = v5.xyzw * tx.xyzw;", "v5.xyzw = v5.xyzw;");
        }
    }

    private bool ConvertInstructions()
    {
        Dictionary<int, Texture> texDict = new Dictionary<int, Texture>();

        foreach (var texture in textures)
        {
            texDict.Add(texture.Index, texture);
        }
        List<int> sortedIndices = texDict.Keys.OrderBy(x => x).ToList();
        List<Texture> sortedTextures = texDict.Values.OrderBy(x => x.Variable).ToList();
        string line = hlsl.ReadLine();
        if (line == null)
        {
            // its a broken pixel shader that uses some kind of memory textures
            return false;
        }
        while (!line.Contains("SV_TARGET2"))
        {
            line = hlsl.ReadLine();
            if (line == null)
            {
                // its a broken pixel shader that uses some kind of memory textures
                return false;
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
                if (line.Contains("Sample"))
                {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".Sample")[0].Split("t")[1]);
                    var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                    var sampleUv = line.Split(", ")[1].Split(")")[0];
                    var dotAfter = line.Split(").")[1];
                    // todo add dimension
                    vfx.AppendLine($"       {equal}= Tex2DS(g_t{texIndex}, TextureFiltering, {sampleUv}).{dotAfter}");
                }
                // todo add load, levelofdetail, o0.w, discard
                else if (line.Contains("discard"))
                {
                    vfx.AppendLine(line.Replace("discard", "        { alpha = 0; }"));
                }
                else
                {
                    vfx.AppendLine($"       {line}");
                }
                vfx.Replace("∞", "1.#INF");
            }
        } while (line != null);

        return true;
    }

    private void AddOutputs()
    {
        Dictionary<int, Texture> texDict = new Dictionary<int, Texture>();

        foreach (var texture in textures)
        {
            texDict.Add(texture.Index, texture);
        }
        List<Texture> sortedTextures = texDict.Values.OrderBy(x => x.Variable).ToList();

        string outputString = @"

        // Normal
        float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);
        float normal_length = length(biased_normal);
        float3 normal_in_world_space = biased_normal / normal_length;
 
        float4 normal = float4(normal_in_world_space,1);
        normal.y = 1 - normal.y;
        normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
        
        float smoothness = saturate(8 * (normal_length - 0.375));
        
        Material mat = ToMaterial(i, float4(o0.xyz, 1), saturate(normal), float4(1 - smoothness, saturate(o2.x), saturate(o2.y * 2), 1));
        mat.Opacity = alpha;
        mat.Emission = clamp((o2.y - 0.5) * 2 * 8 * mat.Albedo, 0, 100);
        
        ShadingModelValveStandard sm;
		
        return FinalizePixelMaterial( i, mat, sm );
    }
}";
 
        if(bOpacityEnabled)
        {
            vfx.Replace("float3 biased_normal = o1.xyz - float3(0.5, 0.5, 0.5);", "float3 biased_normal = o1.xyz;");
        }
        vfx.AppendLine(outputString);
    }
}