using System.Text;
using System.Text.RegularExpressions;
using SharpDX.Direct3D11;
using Tiger.Schema.Shaders;

namespace Tiger.Schema;

public class S2ShaderConverter
{
    private Material Material;
    private StringReader hlsl;
    private StringBuilder vfx;
    private List<TfxScope> Scopes = new();
    private List<TfxExtern> Externs = new();
    private List<STextureTag> Textures = new();
    private List<DXBCIOSignature> Inputs = new();
    private List<DXBCIOSignature> Outputs = new();
    private List<DXBCShaderResource> Resources = new();
    private List<int> ExternTextureSlots = new();

    private bool isTerrain = false;
    private bool isDecorator = false;
    private bool bRT0 = true;
    private bool bTranslucent = false;

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
}}

FEATURES
{{
    #include ""common/features.hlsl""
}}

COMMON
{{
    //alpha
    //frontface
    #define TO_INCHES 39.3700787
	#include ""common/shared.hlsl""
    #define CUSTOM_MATERIAL_INPUTS
}}

struct VertexInput
{{
    float4 vColor : Color0 < Semantic( Color ); >;
    uint vVertexID : SV_VERTEXID < Semantic( VertexID ); >;
    uint vInstanceID : SV_InstanceID < Semantic( None ); >;
	#include ""common/vertexinput.hlsl""
}};

struct PixelInput
{{
    float4 vColor		 : TEXCOORD14;
	float3 v3                : TEXCOORD15; //terrain specific
	float3 v4                : TEXCOORD16; //terrain specific
    float3 v5                : TEXCOORD17; //terrain specific

    float3 vPositionOs : TEXCOORD18;
	float3 vNormalOs : TEXCOORD19;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;

	#include ""common/pixelinput.hlsl""
}};

//Vertex Shader

PS
{{
    #include ""common/pixel.hlsl""
    #define CUSTOM_TEXTURE_FILTERING
    #define cmp -

//ps_samplers
//ps_CBuffers
//ps_Inputs
//ps_additional

    float4 MainPs( PixelInput i ) : SV_Target0
    {{
//ps_Function

//ps_output
    }}
}}";

    public string HlslToVfx(Material material)
    {
        Material = material;
        //Pixel Shader
        StringBuilder texSamples = new StringBuilder();
        hlsl = new StringReader(material.Pixel.Shader.Decompile($"ps{material.Pixel.Shader.Hash}"));
        vfx = new StringBuilder();

        Scopes = material.EnumerateScopes().ToList();
        Externs = material.GetExterns();
        Inputs = material.Pixel.Shader.InputSignatures;
        Outputs = material.Pixel.Shader.OutputSignatures;
        Resources = material.Pixel.Shader.Resources;
        Textures = material.Pixel.EnumerateTextures().ToList();

        bTranslucent = Outputs.Count == 1 || Scopes.Contains(TfxScope.TRANSPARENT) || Scopes.Contains(TfxScope.TRANSPARENT_ADVANCED) || Scopes.Contains(TfxScope.DECAL);
        isTerrain = Scopes.Contains(TfxScope.TERRAIN);
        isDecorator = Scopes.Contains(TfxScope.INSTANCES);

        if (Inputs.Exists(input => input.Semantic == DXBCSemantic.SystemIsFrontFace))
            vfxStructure = vfxStructure.Replace("//frontface", "#define S_RENDER_BACKFACES 1");

        for (int i = 0; i < material.PSSamplers.Count; i++)
        {
            if (material.PSSamplers[i] is null)
                continue;

            var sampler = material.PSSamplers[i].Sampler;
            texSamples.AppendLine($"\tSamplerState s{i + 1}_s < Filter({sampler.Filter}); AddressU({sampler.AddressU}); AddressV({sampler.AddressV}); AddressW({sampler.AddressW}); ComparisonFunc({sampler.ComparisonFunc}); MaxAniso({sampler.MaxAnisotropy}); >;");
        }

        if (bTranslucent) //This way is stupid but it works
        {
            vfxStructure = vfxStructure.Replace("//alpha", $"#ifndef S_ALPHA_TEST\r\n\t#define S_ALPHA_TEST 0\r\n\t#endif\r\n\t#ifndef S_TRANSLUCENT\r\n\t#define S_TRANSLUCENT 1\r\n\t#endif");
            vfxStructure = vfxStructure.Replace("Depth();", "//Depth();"); // ikd if this even does anything
        }

        vfxStructure = vfxStructure.Replace("//ps_samplers", texSamples.ToString());
        vfxStructure = vfxStructure.Replace("//ps_CBuffers", WriteCbuffers(material, false).ToString());
        vfxStructure = vfxStructure.Replace("//ps_Inputs", WriteTexInputs(material, false).ToString());

        if (Resources.Exists(cbuffer => cbuffer.ResourceType == ResourceType.CBuffer && cbuffer.Index == 12))
            vfxStructure = vfxStructure.Replace("//ps_additional", AddTPToProj());

        StringBuilder instructions = ConvertInstructions(material, false);
        if (instructions.ToString().Length == 0)
            return "";

        vfxStructure = vfxStructure.Replace("//ps_Function", instructions.ToString());
        vfxStructure = vfxStructure.Replace("//ps_output", AddOutput().ToString());

        //------------------------------Vertex Shader-----------------------------------

        string vertex = material.Vertex.Shader.Decompile($"vs{material.Vertex.Shader.Hash}");

        Inputs = material.Vertex.Shader.InputSignatures;
        Outputs = material.Vertex.Shader.OutputSignatures;
        Resources = material.Vertex.Shader.Resources;
        Textures = material.Vertex.EnumerateTextures().ToList();

        vfxStructure = vfxStructure.Replace("//Vertex Shader", AddVertexShader());

        if (isTerrain)
            vfxStructure = vfxStructure.Replace("//vs_Function", "\t\tfloat4 r0,r1,r2,r3,r4,r5;\r\n\t\t// Terrain specific\r\n\t\tr1.xyz = float3(0,1,0) * i.vNormalOs.yzx;\r\n\t\tr1.xyz = i.vNormalOs.zxy * float3(0,0,1) + -r1.xyz;\r\n\t\tr0.z = dot(r1.yz, r1.yz);\r\n\t\tr0.z = rsqrt(r0.z);\r\n\t\tr1.xyz = r1.xyz * r0.zzz;\r\n\t\tr2.xyz = i.vNormalOs.zxy * r1.yzx;\r\n\t\tr2.xyz = i.vNormalOs.yzx * r1.zxy + -r2.xyz;\r\n\t\to.v4.xyz = r1.xyz;\r\n\t\tr0.z = dot(r2.xyz, r2.xyz);\r\n\t\tr0.z = rsqrt(r0.z);\r\n\t\to.v3.xyz = r2.xyz * r0.zzz;\r\n\t\tr1.xyz = abs(i.vNormalOs.xyz) * abs(i.vNormalOs.xyz);\r\n\t\tr1.xyz = r1.xyz * r1.xyz;\r\n\t\tr2.xyz = r1.xyz * r1.xyz;\r\n\t\tr2.xyz = r2.xyz * r2.xyz;\r\n\t\tr1.xyz = r2.xyz * r1.xyz;\r\n\t\tr0.z = dot(r1.xyz, float3(1,1,1));\r\n\t\to.v5.xyz = r1.xyz / r0.zzz;");

        // Only gonna add vertex shaders for basic statics/entities with vertex animation
        if (material.Vertex.Unk64 != 0 && !isDecorator && !isTerrain && (Scopes.Contains(TfxScope.RIGID_MODEL) || Scopes.Contains(TfxScope.CHUNK_MODEL)))
        {
            texSamples = new StringBuilder();
            hlsl = new StringReader(vertex);

            for (int i = 0; i < material.Vertex.Samplers.Count; i++)
            {
                if (material.VSSamplers[i] is null)
                    continue;

                var sampler = material.VSSamplers[i].Sampler;
                texSamples.AppendLine($"\tSamplerState s{i + 1}_s < Filter({sampler.Filter}); AddressU({sampler.AddressU}); AddressV({sampler.AddressV}); AddressW({sampler.AddressW}); ComparisonFunc({sampler.ComparisonFunc}); MaxAniso({sampler.MaxAnisotropy}); >;");
            }

            vfxStructure = vfxStructure.Replace("//vs_samplers", texSamples.ToString());
            vfxStructure = vfxStructure.Replace("//vs_CBuffers", WriteCbuffers(material, true).ToString());

            instructions = ConvertInstructions(material, true);
            if (instructions.ToString().Length == 0)
                return "";

            vfxStructure = vfxStructure.Replace("//vs_Function", instructions.ToString());
            vfxStructure = vfxStructure.Replace("//vs_Inputs", WriteTexInputs(material, true).ToString());

            vfxStructure = vfxStructure.Replace("//vs_output", AddOutput(true).ToString());
        }

        //--------------------------
        vfx.AppendLine(vfxStructure);
        return vfx.ToString();
    }

    private StringBuilder WriteCbuffers(Material material, bool isVertexShader)
    {
        StringBuilder CBuffers = new();

        foreach (var resource in Resources)
        {
            switch (resource.ResourceType)
            {
                //case ResourceType.Buffer:
                //    CBuffers.AppendLine($"\tBuffer<float4> b_t{resource.Index} : register(t{resource.Index});");
                //    break;
                case ResourceType.CBuffer:
                    if (resource.Index != 12 && (!isVertexShader || resource.Index != 1))
                    {
                        string cbType = isVertexShader ? "vs_cb" : "cb";
                        for (int i = 0; i < resource.Count; i++)
                        {
                            CBuffers.AppendLine($"\tfloat4 {cbType}{resource.Index}_{i} < Default4( 0.0f, 0.0f, 0.0f, 0.0f ); UiGroup( \"{cbType}{resource.Index}/{i}\"); >;");
                        }
                    }
                    break;
            }
        }

        return CBuffers;
    }

    private StringBuilder WriteTexInputs(Material material, bool isVertexShader)
    {
        StringBuilder funcDef = new();
        bool bAlreadyUsingFB = false;

        if (isVertexShader)
            funcDef.AppendLine($"\tDynamicComboRule( Allow0( D_COMPRESSED_NORMALS_AND_TANGENTS ) );");
        else
            funcDef.AppendLine($"{AddRenderStates()}");

        foreach (var e in Textures)
        {
            if (e.GetTexture() != null)
            {
                string type = isVertexShader ? "VS" : "PS";
                string colSpace = e.GetTexture().IsSrgb() ? "Srgb" : "Linear";
                string dimension = "2D";

                switch (e.GetTexture().GetDimension())
                {
                    case TextureDimension.CUBE:
                        dimension = "Cube";
                        break;
                    case TextureDimension.D3:
                        dimension = "2D";
                        break;
                }

                string tex = isVertexShader ? $"g_vt{e.TextureIndex}" : $"g_t{e.TextureIndex}";
                funcDef.AppendLine($"\tCreateInputTexture{dimension}( {type}_TextureT{e.TextureIndex}, {colSpace}, 8, \"\", \"\",  \"{type} Textures,10/{e.TextureIndex}\", Default3( 1.0, 1.0, 1.0 ));");
                funcDef.AppendLine($"\tTexture{dimension} {tex} < Channel( RGBA,  Box( {type}_TextureT{e.TextureIndex} ), {colSpace} ); OutputFormat( BC7 ); SrgbRead( {e.GetTexture().IsSrgb()} ); >; ");
                //funcDef.AppendLine($"\tTextureAttribute(g_t{e.TextureIndex}, g_t{e.TextureIndex});\n"); //Prevents some inputs not appearing for some reason
            }
        }

        if (isVertexShader)
            return funcDef;

        if (isTerrain)
        {
            funcDef.AppendLine($"\tTexture2D g_t14 < Attribute( \"TerrainDyemap\" ); SrgbRead( false ); >;\r\n\n");
        }

        var opcodes = material.Pixel.GetBytecode().Opcodes;
        foreach ((int i, var op) in opcodes.Select((value, index) => (index, value)))
        {
            switch (op.op)
            {
                case TfxBytecode.PushExternInputTextureView:
                    var data = (PushExternInputTextureViewData)op.data;
                    var slot = ((SetShaderTextureData)opcodes[i + 1].data).value & 0x1F;
                    var index = data.element * 8;
                    switch (data.extern_)
                    {
                        case TfxExtern.Frame:
                            switch (index)
                            {
                                case 0xB8: // SGlobalTextures SpecularTintLookup
                                    ExternTextureSlots.Add(slot);
                                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_tSpecularTintLookup, Srgb, 8, \"\", \"\",  \"PS Textures,10/{slot}\", Default4( 1.0, 1.0, 1.0, 1.0 ));");
                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Channel( RGBA,  Box( PS_tSpecularTintLookup ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                                    break;
                            }
                            break;

                        case TfxExtern.Deferred:
                            switch (index)
                            {
                                case 0x48:
                                    ExternTextureSlots.Add(slot);
                                    if (!bAlreadyUsingFB)
                                        funcDef.AppendLine($"\tBoolAttribute( bWantsFBCopyTexture, true );");

                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;");
                                    bAlreadyUsingFB = true;
                                    break;
                                case 0x98: // Generated sky hemisphere
                                    ExternTextureSlots.Add(slot);
                                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT{slot}, Srgb, 8, \"\", \"\",  \"PS Textures,10/{slot}\", Default4( 1.0, 1.0, 1.0, 1.0 ));");
                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Channel( RGBA,  Box( PS_TextureT{slot} ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                                    break;
                            }
                            break;

                        case TfxExtern.Atmosphere:
                            switch (index)
                            {
                                case 0x80: // SMapAtmosphere Lookup4
                                    ExternTextureSlots.Add(slot);
                                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT{slot}, Srgb, 8, \"\", \"\",  \"PS Textures,10/{slot}\", Default4( 1.0, 1.0, 1.0, 1.0 ));");
                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Channel( RGBA,  Box( PS_TextureT{slot} ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                                    break;
                                case 0xE0: // SMapAtmosphere T11
                                    ExternTextureSlots.Add(slot);
                                    break;
                                case 0xF0: // SMapAtmosphere T13
                                    ExternTextureSlots.Add(slot);
                                    break;
                            }
                            break;

                        case TfxExtern.Water:
                            switch (index)
                            {
                                case 0x30: // temp
                                case 0x0:
                                    ExternTextureSlots.Add(slot);
                                    if (!bAlreadyUsingFB)
                                        funcDef.AppendLine($"\tBoolAttribute( bWantsFBCopyTexture, true );");

                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;");
                                    bAlreadyUsingFB = true;
                                    break;

                                case 0x28:
                                    ExternTextureSlots.Add(slot);
                                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT{slot}, Srgb, 8, \"\", \"\",  \"PS Textures,10/{slot}\", Default4( 0.5, 0.5, 0.0, 1.0 ));");
                                    funcDef.AppendLine($"\tTexture2D g_t{slot} < Channel( RGBA,  Box( PS_TextureT{slot} ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }

        foreach (var scope in Scopes)
        {
            switch (scope)
            {
                // Gonna use input texture for these since "Default4" works on it, wont be the magenta checkerboard if missing, idk if this is bad or not
                case TfxScope.TRANSPARENT:
                    //funcDef.AppendLine($"\tTexture2D g_t11 < Attribute( \"AtmosFar\" ); >;\n");
                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT11, Srgb, 8, \"\", \"\",  \"PS Textures,10/11\", Default4( 0.0, 0.0, 0.0, 1.0 ));");
                    funcDef.AppendLine($"\tTexture2D g_t11 < Channel( RGBA,  Box( PS_TextureT11 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");

                    //funcDef.AppendLine($"\tTexture2D g_t13 < Attribute( \"AtmosNear\" ); >;\n");
                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT13, Srgb, 8, \"\", \"\",  \"PS Textures,10/13\", Default4( 0.0, 0.0, 0.0, 1.0 ));");
                    funcDef.AppendLine($"\tTexture2D g_t13 < Channel( RGBA,  Box( PS_TextureT13 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");

                    funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT15, Srgb, 8, \"\", \"\",  \"PS Textures,10/15\", Default4( 1.0, 1.0, 1.0, 1.0 ));");
                    funcDef.AppendLine($"\tTexture2D g_t15 < Channel( RGBA,  Box( PS_TextureT15 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");

                    if (Resources.Any(x => x.ResourceType == ResourceType.Texture2D && x.Index == 23))
                    {
                        ExternTextureSlots.Add(23);
                        if (!bAlreadyUsingFB)
                            funcDef.AppendLine($"\tBoolAttribute( bWantsFBCopyTexture, true );");

                        funcDef.AppendLine($"\tTexture2D g_t23 < Attribute( \"FrameBufferCopyTexture\" ); SrgbRead( true ); Filter( MIN_MAG_MIP_LINEAR ); AddressU( CLAMP ); AddressV( CLAMP ); >;");
                        bAlreadyUsingFB = true;
                    }

                    break;
            }
        }

        foreach (var extern_ in Externs)
        {
            foreach (var resource in Resources)
            {
                switch (extern_)
                {
                    case TfxExtern.Atmosphere when ((resource.ResourceType == ResourceType.Texture2D && resource.Index == 1) && !Textures.Exists(texture => texture.TextureIndex == 1 && texture.GetTexture() is not null)):
                        //funcDef.AppendLine($"\tTexture2D g_t1 < Attribute( \"AtmosFar\" ); >;\r\n\n");
                        funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT1, Srgb, 8, \"\", \"\",  \"PS Textures,10/1\", Default4( 0.0, 0.0, 0.0, 0.0 ));");
                        funcDef.AppendLine($"\tTexture2D g_t1 < Channel( RGBA,  Box( PS_TextureT1 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                        break;
                    case TfxExtern.Atmosphere when ((resource.ResourceType == ResourceType.Texture2D && resource.Index == 2) && !Textures.Exists(texture => texture.TextureIndex == 2 && texture.GetTexture() is not null)):
                        //funcDef.AppendLine($"\tTexture2D g_t2 < Attribute( \"AtmosNear\" ); >;\r\n\n");
                        funcDef.AppendLine($"\tCreateInputTexture2D( PS_TextureT2, Srgb, 8, \"\", \"\",  \"PS Textures,10/2\", Default4( 0.0, 0.0, 0.0, 0.0 ));");
                        funcDef.AppendLine($"\tTexture2D g_t2 < Channel( RGBA,  Box( PS_TextureT2 ), Srgb ); OutputFormat( RGBA8888 ); SrgbRead( True ); >;");
                        break;
                }
            }
        }

        return funcDef;
    }

    private StringBuilder ConvertInstructions(Material material, bool isVertexShader)
    {
        StringBuilder funcDef = new();

        if (isVertexShader)
        {
            funcDef.AppendLine("\t\tfloat3 vCameraPos = g_vCameraPositionWs/TO_INCHES;");
            funcDef.AppendLine(AddViewScope(true));
            funcDef.AppendLine(AddCB1());

            foreach (var i in Inputs)
            {
                switch (i.Semantic)
                {
                    case DXBCSemantic.Position:
                        funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = float4(i.vPositionOs/TO_INCHES, 0); //{i.ToString()}");
                        break;
                    case DXBCSemantic.Tangent:
                        funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = i.vTangentUOs_flTangentVSign; //{i.ToString()}");
                        break;
                    case DXBCSemantic.Texcoord when i.SemanticIndex == 0:
                        funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = float4(i.vTexCoord, 0, 0); //{i.ToString()}");
                        break;
                    case DXBCSemantic.Normal:
                        funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = i.vNormalOs; //{i.ToString()}");
                        break;
                    case DXBCSemantic.SystemVertexId:
                        funcDef.AppendLine($"\t\tuint v{i.RegisterIndex} = i.vVertexID; //{i.ToString()}");
                        break;
                    case DXBCSemantic.SystemInstanceId:
                        funcDef.AppendLine($"\t\tuint v{i.RegisterIndex} = 0; //{i.ToString()}");
                        break;
                    default:
                        funcDef.AppendLine($"\t\t{i.GetMaskType()} v{i.RegisterIndex} = float4(1,1,1,1).{i.Mask.ToString().ToLower()}; //{i.Semantic}");
                        break;
                }
            }
        }
        else //Pixel
        {
            //Need to divde by TO_INCHES to convert to meters
            funcDef.AppendLine("\t\tfloat3 vCameraPos = g_vCameraPositionWs/TO_INCHES;");
            funcDef.AppendLine("\t\tfloat3 vPositionWs = (i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz) / TO_INCHES;");
            funcDef.AppendLine("\t\tfloat alpha = 1;");

            if (isTerrain) // Input variables are different for terrain
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {vPositionWs, 1};"); // World Pos
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTextureCoords.xy, 1, 1};"); // UVs
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vNormalWs,1};"); // Mesh world normals
                funcDef.AppendLine("\t\tfloat4 v3 = {i.v3,1};"); // From VS, Used for normals
                funcDef.AppendLine("\t\tfloat4 v4 = {i.v4,1};"); // From VS, Used for normals
                funcDef.AppendLine("\t\tfloat4 v5 = {i.v5,1};"); // From VS, Used for tri-planar mapping? Mainly seen on vertical terrain
            }
            else if (isDecorator)
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {i.vTextureCoords.xy,0,0};");
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vNormalWs,1};");
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vTangentUWs,1};");
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTangentVWs,1};");
                funcDef.AppendLine("\t\tfloat4 v4 = {0,0,0,0};"); // Unsure
                funcDef.AppendLine("\t\tfloat4 v5 = {vPositionWs,0};");
            }
            else // statics, normal entities
            {
                funcDef.AppendLine("\t\tfloat4 v0 = {i.vNormalWs,1};"); // Mesh world normals
                funcDef.AppendLine("\t\tfloat4 v1 = {i.vTangentUWs,1};"); // Tangent U
                funcDef.AppendLine("\t\tfloat4 v2 = {i.vTangentVWs,1};"); // Tangent V
                funcDef.AppendLine("\t\tfloat4 v3 = {i.vTextureCoords.xy,0,0};"); // UVs
                funcDef.AppendLine("\t\tfloat4 v4 = {vPositionWs,0};"); // World Pos
            }

            foreach (var i in Inputs)
            {
                switch (i.GetMaskType())
                {
                    case "uint":
                        if (i.Semantic == DXBCSemantic.SystemIsFrontFace)
                            funcDef.AppendLine($"\t\tint v{i.RegisterIndex} = i.face;");
                        else
                        {
                            if (isDecorator)
                                funcDef.AppendLine($"\t\tint w{i.RegisterIndex} = 1; //{i.Semantic}{i.SemanticIndex}");
                            else
                                funcDef.AppendLine($"\t\tint v{i.RegisterIndex} = 1; //{i.Semantic}{i.SemanticIndex}");
                        }
                        break;

                    case "float4":
                        if (i.Semantic == DXBCSemantic.SystemPosition)
                            funcDef.AppendLine($"\t\tfloat4 v{i.RegisterIndex} = i.vPositionSs;");
                        else if (i.RegisterIndex == 5 && i.Semantic == DXBCSemantic.Texcoord && !isTerrain && !isDecorator)
                            funcDef.AppendLine($"\t\tfloat4 v5 = i.vColor; //{i.Semantic}{i.SemanticIndex}");
                        break;

                    case "float":
                        if (isDecorator)
                            funcDef.AppendLine($"\t\tfloat w{i.RegisterIndex} = 1; //{i.Semantic}{i.SemanticIndex}");
                        else
                            funcDef.AppendLine($"\t\tfloat v{i.RegisterIndex} = 1; //{i.Semantic}{i.SemanticIndex}");
                        break;
                }
            }
            funcDef.AppendLine("\t\tfloat4 o0 = float4(0,0,0,1);");
            funcDef.AppendLine("\t\tfloat4 o1 = float4(PackNormal3D(v0.xyz),1);");
            funcDef.AppendLine("\t\tfloat4 o2 = float4(0,0.5,0,0);");
            funcDef.AppendLine("\t\tfloat4 o3 = float4(0,0,0,0);\n");

            if (Resources.Exists(cbuffer => (cbuffer.ResourceType == ResourceType.CBuffer && cbuffer.Index == 12)))
            {
                funcDef.AppendLine(AddViewScope());
            }
        }


        string line = hlsl.ReadLine();
        if (line == null)
        {
            return new StringBuilder();
        }
        if (!isVertexShader)
        {
            while (!line.Contains("SV_TARGET0"))
            {
                line = hlsl.ReadLine();
                if (line == null)
                {
                    return new StringBuilder();
                }
            }
            while (!line.Contains("{"))
            {
                if (line.Contains("SV_TARGET2"))
                    bRT0 = false;
                line = hlsl.ReadLine();
            }
        }
        else
        {
            while (!line.Contains("SV_POSITION0"))
            {
                line = hlsl.ReadLine();
                if (line == null)
                {
                    return new StringBuilder();
                }
            }
            while (!line.Contains("{"))
            {
                line = hlsl.ReadLine();
            }
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

                        if (group1 != "12" && (!isVertexShader || group1 != "1"))
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
                    var equal_tex_post = equal_post.Substring(equal_post.IndexOf(".") + 1);

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

                            if (group1 != "12" || (isVertexShader && group1 != "1"))
                            {
                                equal_tex_post = equal_tex_post.Replace($"cb{group1}[{group2}]", $"cb{group1}_{group2}");
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


                    if (texIndex == 14 && isTerrain) // Terrain dyemap, not defined in the material itself
                    {
                        funcDef.AppendLine($"\t\t{equal.TrimStart()} = g_t{texIndex}.Sample(s{sampleIndex}_s, {sampleUv}).{dotAfter}");
                    }
                    else if ((!Textures.Exists(texture => texture.TextureIndex == texIndex && texture.GetTexture() is not null)) && !ExternTextureSlots.Contains(texIndex)) // Some kind of buffer texture or not defined in the material
                    {
                        switch (texIndex)
                        {
                            case 1 when Externs.Contains(TfxExtern.Atmosphere) && !isVertexShader:
                            case 2 when Externs.Contains(TfxExtern.Atmosphere) && !isVertexShader:
                            case 11 when Scopes.Contains(TfxScope.TRANSPARENT): // Atmosphere textures
                            case 13 when Scopes.Contains(TfxScope.TRANSPARENT):
                            case 15 when Scopes.Contains(TfxScope.TRANSPARENT): // Atmosphere Density
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.{equal_tex_post}");
                                break;

                            case 10: // Depth buffer
                                bUsesDepthBuffer = true;
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= Depth::GetNormalized({sampleUv})*0.3937.xxxx; //{equal_post}");
                                break;

                            case 23 when Scopes.Contains(TfxScope.TRANSPARENT): // Unsure                    
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t23.{equal_tex_post}");//.SampleLevel(s_s{sampleIndex}, {sampleUv}).{dotAfter} //{equal_post}");
                                break;

                            case 7 when Externs.Contains(TfxExtern.Water):
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,0,0,1).{dotAfter} //{equal_post}");
                                break;

                            case 1: // Unknown
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.21191,0.21191,0.21191,0.49804).{dotAfter} //{equal_post}");
                                break;

                            case 16: // Specular related..?
                            case 17:
                            case 18:
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.1,0.1,0.1,1).{dotAfter} //{equal_post}");
                                break;
                            case 19:
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,0,0,1).{dotAfter} //{equal_post}");
                                break;

                            case 0: // Unknown
                            case 20: // Some kind of modified depth?
                            case 21: // Volumetric lighting?
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0,0,0,0).{dotAfter} //{equal_post}");
                                break;

                            case 22: // Unknown
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.55078,0.55078,0.55078,0.49804).{dotAfter} //{equal_post}");
                                break;

                            case 6 when Externs.Contains(TfxExtern.Deferred): // Hemisphere/Sky texture?
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.01,0.01,0.02,1).{dotAfter} //{equal_post}");
                                break;

                            default: // Unknown
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0,0,0,0).{dotAfter} //{equal_post}");
                                break;
                        }
                    }
                    else // Textures defined by the material
                    {
                        string tex = isVertexShader ? $"g_vt{texIndex}" : $"g_t{texIndex}";
                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= {tex}.{equal_tex_post}");
                    }
                }
                else if (line.Contains("CalculateLevelOfDetail"))
                {
                    var equal = line.Split("=")[0];
                    var texIndex = Int32.Parse(line.Split(".CalculateLevelOfDetail")[0].Split("t")[1]);
                    var sampleIndex = Int32.Parse(line.Split("(s")[1].Split("_s,")[0]);
                    var sampleUv = line.Split(", ")[1].Split(")")[0];

                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.CalculateLevelOfDetail(s{sampleIndex}_s, {sampleUv});");
                }
                else if (line.Contains("Load"))
                {
                    var equal = line.Split("=")[0];
                    var equal_post = line.Split("=")[1];
                    var texIndex = Int32.Parse(line.Split(".Load")[0].Split("t")[1]);
                    var sampleUv = line.Split("(")[1].Split(")")[0];
                    var dotAfter = line.Split(").")[1];

                    if (!isVertexShader)
                    {
                        if (!Textures.Exists(texture => texture.TextureIndex == texIndex && texture.GetTexture() is not null)) //Some kind of buffer texture
                        {
                            switch (texIndex)
                            {
                                case 2:
                                    if (Scopes.Contains(TfxScope.DECAL))
                                    {
                                        bUsesNormalBuffer = true;
                                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= v0.{dotAfter}");
                                    }
                                    else
                                        funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0.5,0.5,0.5,0.5).{dotAfter} //{equal_post}");

                                    break;
                                case 10:
                                    bUsesDepthBuffer = true;
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= Depth::GetNormalized({sampleUv})*0.3937.xxxx; //{equal_post}");
                                    break;
                                case 11:
                                case 13:
                                case 23: //Usually uses SampleLevel but shouldnt be an issue?
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
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0,0,0,0).{dotAfter} //{equal_post}");
                                    break;
                            }
                        }
                        else
                        {
                            funcDef.AppendLine($"\t\t{equal.TrimStart()}= g_t{texIndex}.Load({sampleUv}).{dotAfter}");
                        }
                    }
                    else
                    {
                        switch (texIndex)
                        {
                            case 0: // Vertex Color
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= i.vColor.{dotAfter}");
                                break;
                            case 1: // Vertex AO
                                funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(1,1,1,1).{dotAfter}");
                                break;
                            default:
                                if (!Textures.Exists(texture => texture.TextureIndex == texIndex && texture.GetTexture() is not null)) //Some kind of buffer texture
                                {
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= float4(0,0,0,0).{dotAfter} //{equal_post}");
                                }
                                else
                                {
                                    string tex = isVertexShader ? $"g_vt{texIndex}" : $"g_t{texIndex}";
                                    funcDef.AppendLine($"\t\t{equal.TrimStart()}= {tex}.Load({sampleUv}).{dotAfter}");
                                }
                                break;
                        }
                    }

                }
                else if (line.Contains("o1.xyzw = float4(0,0,0,0);") && !isVertexShader)
                {
                    funcDef.AppendLine(line.Replace("o1.xyzw = float4(0,0,0,0);", "\t\to1.xyzw = float4(PackNormal3D(v0.xyz),0);")); //decals(?) have 0 normals sometimes, dont want that
                }
                else if (line.Contains("GetDimensions")) // Uhhhh
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

        return funcDef;
    }

    private StringBuilder AddOutput(bool isVertexShader = false)
    {
        StringBuilder output = new StringBuilder();

        if (isVertexShader)
        {
            output.AppendLine($"\t\to.vPositionWs.xyz = o4 * TO_INCHES;");
            output.AppendLine($"\t\to.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs );");
        }
        else
        {
            if (!bRT0) //uses o1,o2
            {
                //this is fine...
                output.Append($@"
        // Normal
		float3 normal = o1.xyz * float3(2,2,2) + float3(-1,-1,-1);
		float length = sqrt(dot(normal.xyz, normal.xyz));
		normal = normal.xyz / length;
        
		// Roughness
		r0.x = length * 4 + -3;
		r0.y = saturate(-0.5 * r0.x);
		r0.z = r0.x * r0.x;
		r0.x = cmp(r0.x < -0.0105999997);
		float roughness = r0.x ? r0.y : r0.z;

		// Emission
		r0.x = o2.y;
		r0.x = saturate(r0.x * 2 + -1.00784314);
		r0.x = r0.x * 13 + -7;
		r0.x = exp2(r0.x);
		r0.x = -0.0078125 + r0.x;
		r0.y = 1;
		r0.x = r0.y * r0.x;
		float3 emission = r0.xxx * o0.xyz;

		Material mat = Material::Init();
        mat.Albedo = o0.xyz;
        mat.Normal = {(bUsesNormalBuffer ? "i.vNormalWs.xyz" : "normal")};
        mat.Roughness = 1 - roughness;
        mat.Metalness = saturate(o2.x);
        mat.AmbientOcclusion = saturate(o2.y * 2);
        mat.TintMask = 1;
        mat.Opacity = {(bTranslucent || Material.RenderStates.BlendState() != -1 ? "o0.w" : "1")};
        mat.Emission = emission;       
        mat.Transmission = o2.z;

        // Misc
        mat.WorldPosition = i.vPositionWithOffsetWs + g_vHighPrecisionLightingOffsetWs.xyz;
        mat.WorldPositionWithOffset = i.vPositionWithOffsetWs;
        mat.ScreenPosition = i.vPositionSs;
		mat.WorldTangentU = i.vTangentUWs;
		mat.WorldTangentV = i.vTangentVWs;
        mat.TextureCoords = i.vTextureCoords.xy;

		return ShadingModelStandard::Shade(i, mat);");

            }
            else //only uses o0
            {
                bool a = bUsesNormalBuffer || bTranslucent || bUsesFrameBuffer || bUsesDepthBuffer;

                if (a) //??
                {
                    //output.Append($"\t\treturn float4(o0.xyz, {(bUsesFrameBuffer ? "1" : "alpha")});");
                    output.Append($"\t\treturn float4(o0.xyz, o0.w);");
                }
                else
                {
                    output.AppendLine($"\t\tMaterial mat = Material::From(i, float4(o0.xyz, 1), float4(0.5, 0.5, 1, 1), float4(0.5, 0, 1, 1), float3(1.0f, 1.0f, 1.0f), 0);");
                    output.AppendLine($"\t\treturn ShadingModelStandard::Shade(i, mat);");
                }
            }
        }

        return output;
    }

    private string AddVertexShader() // I hate this
    {
        if (isDecorator) // Surely this is fine...
            return $"VS\r\n{{\r\n\t#include \"common/vertex.hlsl\"\r\n\r\n    float g_flEdgeFrequency < Default( 0.17 ); Range( 0.0, 1.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n    float g_flEdgeAmplitude < Default( 0.15 ); Range( 0.0, 1.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n    float g_flBranchFrequency < Default( 0.17 ); Range( 0.0, 1.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n    float g_flBranchAmplitude < Default( 0.15 ); Range( 0.0, 1.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n    float g_flTrunkDeflection < Default( 0.0 ); Range( 0.0, 1.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n    float g_flTrunkDeflectionStart < Default( 0.0 ); Range( 0.0, 1000.0 ); UiGroup( \"Foliage Animation\" ); >;\r\n\r\n    float4 SmoothCurve( float4 x )\r\n    {{  \r\n        return x * x * ( 3.0 - 2.0 * x );  \r\n    }}  \r\n\r\n    float4 TriangleWave( float4 x )\r\n    {{\r\n        return abs( frac( x + 0.5 ) * 2.0 - 1.0 );  \r\n    }}  \r\n\r\n    float4 SmoothTriangleWave( float4 x )\r\n    {{  \r\n        return SmoothCurve( TriangleWave( x ) );  \r\n    }}\r\n\r\n    // High-frequency displacement used in Unity's TerrainEngine; based on \"Vegetation Procedural Animation and Shading in Crysis\"\r\n    // http://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch16.html\r\n    void FoliageDetailBending( inout float3 vPositionOs, float3 vNormalOs, float3 vVertexColor, float3x4 matObjectToWorld, float3 vWind )\r\n    {{\r\n        // 1.975, 0.793, 0.375, 0.193 are good frequencies   \r\n        const float4 vFoliageFreqs = float4( 1.975, 0.793, 0.375, 0.193 );\r\n\r\n        // Attenuation and phase offset is encoded in the vertex color\r\n        const float flEdgeAtten = vVertexColor.r;\r\n        const float flBranchAtten = vVertexColor.b;\r\n        const float flDetailPhase = vVertexColor.g;\r\n\r\n        // Material defined frequency and amplitude\r\n        const float2 vTime = g_flTime.xx * float2( g_flEdgeFrequency, g_flBranchFrequency );\r\n        const float flEdgeAmp = g_flEdgeAmplitude;\r\n        const float flBranchAmp = g_flBranchAmplitude;\r\n\r\n        // Phases\r\n        float flObjPhase = dot( mul( matObjectToWorld, float4( 0, 0, 0, 1 ) ), 1 );\r\n        float flBranchPhase = flDetailPhase + flObjPhase;\r\n        float flVtxPhase = dot( vPositionOs.xyz, flDetailPhase + flBranchPhase );\r\n\r\n        // fmod max phase avoid imprecision at high phases\r\n        const float maxPhase = 50000.0f;\r\n\r\n        // x is used for edges; y is used for branches\r\n        float2 vWavesIn = ( vTime.xy + fmod( float2( flVtxPhase, flBranchPhase ), maxPhase ) ) * length( vWind );\r\n        \r\n        float4 vWaves = ( frac( vWavesIn.xxyy * vFoliageFreqs ) * 2.0 - 1.0 );\r\n        vWaves = SmoothTriangleWave( vWaves );\r\n        float2 vWavesSum = vWaves.xz + vWaves.yw;\r\n\r\n        // Edge (xy) and branch bending (z)\r\n        float flBranchWindBend = 1.0f - abs( dot( normalize( vWind.xyz ), normalize( float3( vPositionOs.xy, 0.0f ) ) ) );\r\n        flBranchWindBend *= flBranchWindBend;\r\n\r\n        vPositionOs.xyz += vWavesSum.x * flEdgeAtten * flEdgeAmp * vNormalOs.xyz;\r\n        vPositionOs.xyz += vWavesSum.y * flBranchAtten * flBranchAmp * float3( 0.0f, 0.0f, 1.0f );\r\n        vPositionOs.xyz += vWavesSum.y * flBranchAtten * flBranchAmp * flBranchWindBend * vWind.xyz;\r\n    }}\r\n\r\n    // Main vegetation bending - displace verticies' xy positions along the wind direction\r\n    // using normalized height to scale the amount of deformation.\r\n    void FoliageMainBending( inout float3 vPositionOs, float3 vWind )\r\n    {{\r\n        if ( g_flTrunkDeflection <= 0.0 ) return;\r\n\r\n        float flLength = length( vPositionOs.xyz );\r\n        // Bend factor - Wind variation is done on the CPU.  \r\n        float flBF = 0.01f * max( vPositionOs.z - g_flTrunkDeflectionStart, 0 ) * g_flTrunkDeflection;  \r\n        // Smooth bending factor and increase its nearby height limit.  \r\n        flBF += 1.0f;\r\n        flBF *= flBF;\r\n        flBF = flBF * flBF - flBF;\r\n\r\n        // Back and forth\r\n        float flBend = pow( max( 0.0f, length( vWind ) - 1.0f ) / 4.0f, 2 ) * 4.0f;\r\n        flBend = flBend + 0.7f * sqrt( flBend ) * sin( g_flTime );\r\n        flBF *= flBend;\r\n\r\n        // Displace position  \r\n        float3 vNewPos = vPositionOs;\r\n        vNewPos.xy += vWind.xy * flBF;\r\n\r\n        // Rescale (reduces stretch)\r\n        vPositionOs.xyz = normalize( vNewPos.xyz ) * flLength;\r\n    }}\r\n\r\n\t//\r\n\t// Main\r\n\t//\r\n\tPixelInput MainVs( VertexInput i )\r\n\t{{\r\n\t\tPixelInput o = ProcessVertex( i );\r\n\r\n        //o.vColor = i.vColor;\r\n\t\to.vColor = i.vColor;\r\n\t\to.vColor.a = i.vColor.a;\r\n\r\n        float3 vNormalOs;\r\n        float4 vTangentUOs_flTangentVSign;\r\n\r\n        VS_DecodeObjectSpaceNormalAndTangent( i, vNormalOs, vTangentUOs_flTangentVSign );\r\n\t\t\r\n        float3 vPositionOs = i.vPositionOs.xyz;\r\n        float3x4 matObjectToWorld = CalculateInstancingObjectToWorldMatrix( i );\r\n\r\n\t\tif(!all(i.vColor.xyz == float3(1, 1, 1)))\r\n\t\t{{\r\n\t\t\tfloat3 vWind = float3(1,1,0.1) * 4.0f; //g_vWindDirection.xyz * g_vWindStrengthFreqMulHighStrength.x;\r\n\t\t\tFoliageDetailBending( vPositionOs, vNormalOs, i.vColor.xyz, matObjectToWorld, vWind );\r\n\t\t\tFoliageMainBending( vPositionOs, vWind );\r\n\t\t}}\r\n\r\n        o.vPositionWs = mul( matObjectToWorld, float4( vPositionOs.xyz, 1.0 ) );\r\n\t    o.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs.xyz );\r\n\r\n\t\t// Add your vertex manipulation functions here\r\n\t\treturn FinalizeVertex( o );\r\n\t}}\r\n}}";
        else // Basic vertex shader
            return $"VS\r\n{{\r\n\t#include \"common/vertex.hlsl\"\r\n    #define CUSTOM_TEXTURE_FILTERING\r\n    #define cmp -\r\n\r\n//vs_samplers\r\n//vs_CBuffers\r\n//vs_Inputs\r\n\r\n\tPixelInput MainVs( VertexInput i )\r\n\t{{\r\n\t\tPixelInput o = ProcessVertex( i );\r\n        float4 o0,o1,o2,o3,o4,o5,o6,o7,o8;\r\n        o.vColor = i.vColor;\r\n\t\to.vColor.a = i.vColor.a;\r\n        o.vPositionOs = i.vPositionOs.xyz;\r\n        VS_DecodeObjectSpaceNormalAndTangent( i, o.vNormalOs, o.vTangentUOs_flTangentVSign );\r\n\r\n//vs_Function\r\n//vs_output\r\n\t\treturn FinalizeVertex( o );\r\n\t}}\r\n}}";
    }


    private string AddViewScope(bool isVertexShader = false)
    {
        StringBuilder viewScope = new StringBuilder();
        viewScope.AppendLine($"\t\tfloat4 cb12[{(isVertexShader ? "16" : "15")}] = {{");

        viewScope.AppendLine($"\t\t\ttranspose(g_matWorldToProjection),"); //0-3 World To Proj

        viewScope.AppendLine($"\t\t\tfloat4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),0),"); //4-7 Camera To World
        viewScope.AppendLine($"\t\t\tfloat4(g_vCameraUpDirWs,0),");
        viewScope.AppendLine($"\t\t\tfloat4(-g_vCameraDirWs,0),");
        viewScope.AppendLine($"\t\t\tfloat4(vCameraPos,1),");

        if (isVertexShader)
        {
            viewScope.AppendLine($"\t\t\tfloat4(g_vViewportSize, g_vInvViewportSize),"); //VS 8 Target
            viewScope.AppendLine($"\t\t\tfloat4(1,0,0,0),"); //VS 9 View Misc
            viewScope.AppendLine($"\t\t\tfloat4(vCameraPos,1),"); //VS 10 Position
            viewScope.AppendLine($"\t\t\tg_matViewToProjection,"); //VS 11-14 Camera To Proj
            viewScope.AppendLine($"\t\t\tfloat4(0,0,1,0) - transpose(g_matWorldToProjection)[3],"); //VS 15 Unk
        }
        else
        {
            viewScope.AppendLine($"\t\t\tg_matProjectionToView * TargetPixelToProjective( g_vViewportSize ),"); //PS 8-11 TargetPixelToProjective
            viewScope.AppendLine($"\t\t\tfloat4(g_vViewportSize, g_vInvViewportSize),"); //PS 12 Target
            viewScope.AppendLine($"\t\t\tfloat4(1,0,0,0),"); //PS 13 View Misc
            viewScope.AppendLine($"\t\t\tfloat4(vCameraPos,1)"); //PS 14
        }


        viewScope.AppendLine($"\t\t}};");

        return viewScope.ToString();
    }

    private string AddCB1()
    {
        StringBuilder cb1 = new StringBuilder();
        cb1.AppendLine($"\t\tfloat3x4 matObjectToWorld = CalculateInstancingObjectToWorldMatrix( i );");
        cb1.AppendLine($"\t\tmatObjectToWorld[0].w /= TO_INCHES;");
        cb1.AppendLine($"\t\tmatObjectToWorld[1].w /= TO_INCHES;");
        cb1.AppendLine($"\t\tmatObjectToWorld[2].w /= TO_INCHES;");

        cb1.AppendLine($"\t\tfloat4 cb1[{(Scopes.Contains(TfxScope.RIGID_MODEL) ? "8" : "6")}] = {{");

        if (Scopes.Contains(TfxScope.RIGID_MODEL)) // Entities
        {
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[0].x, matObjectToWorld[1].x, matObjectToWorld[2].x, 0),"); // 0-3 Mesh To World
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[0].y, matObjectToWorld[1].y, matObjectToWorld[2].y, 0),");
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[0].z, matObjectToWorld[1].z, matObjectToWorld[2].z, 0),");
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[0].w, matObjectToWorld[1].w, matObjectToWorld[2].w, 1),");

            cb1.AppendLine($"\t\t\tfloat4(1,1,1,1),"); // 4 Scale
            cb1.AppendLine($"\t\t\tfloat4(0,0,0,0),"); // 5 Offset
            cb1.AppendLine($"\t\t\tfloat4(1,1,0,0),"); // 6 TexCoord Scale/Offset
            cb1.AppendLine($"\t\t\tfloat4(1,1,1,1),"); // 7 dynamic_sh_ao_values
        }
        else // Statics
        {
            cb1.AppendLine($"\t\t\tfloat4(0,0,0,1),"); // 0 Offset/Scale
            cb1.AppendLine($"\t\t\tfloat4(1,0,0,1),"); // 1 TexCoord Scale/Offset, max_color_index
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[0]),"); // 2-5 Transforms
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[1]),");
            cb1.AppendLine($"\t\t\tfloat4(matObjectToWorld[2]),");
            cb1.AppendLine($"\t\t\tfloat4(1,1,1,9.40395E-38)");
        }

        cb1.AppendLine($"\t\t}};");

        return cb1.ToString();
    }

    private string AddTPToProj()
    {
        StringBuilder tp = new StringBuilder();
        tp.AppendLine("\tfloat4x4 TargetPixelToProjective(float2 size)\n\t{");
        tp.AppendLine("\t\treturn float4x4(");
        tp.AppendLine("\t\t\t2.0f / size.x,  0.0f,          0.0f, \t0.0f,");
        tp.AppendLine("\t\t\t0.0f,          -2.0f / size.y, 0.0f, \t0.0f,");
        tp.AppendLine("\t\t\t0.0f,           0.0f,          1.0f,\t0.0f,");
        tp.AppendLine("\t\t\t-1.0f,          1.0f,          0.0f, \t1.0f");
        tp.AppendLine("\t\t);\n\t}");

        return tp.ToString();
    }

    private string AddRenderStates()
    {
        StringBuilder renderStates = new();
        if (Material.RenderStates.BlendState() != -1)
        {
            var blendState = RenderStates.BlendStates[Material.RenderStates.BlendState()];
            renderStates.AppendLine($"\tRenderState(AlphaToCoverageEnable, {blendState.AlphaToCoverageEnable.ToString().ToLower()})");
            renderStates.AppendLine($"\tRenderState(IndependentBlendEnable, {blendState.IndependentBlendEnable.ToString().ToLower()})");
            renderStates.AppendLine($"\tRenderState(BlendEnable, {blendState.BlendDesc.IsBlendEnabled.ToString().ToLower()})");
            renderStates.AppendLine($"\tRenderState(SrcBlend, {BlendOptionString(blendState.BlendDesc.SourceBlend)})");
            renderStates.AppendLine($"\tRenderState(DstBlend, {BlendOptionString(blendState.BlendDesc.DestinationBlend)})");
            renderStates.AppendLine($"\tRenderState(BlendOp, {BlendOpString(blendState.BlendDesc.BlendOperation)})");
            renderStates.AppendLine($"\tRenderState(SrcBlendAlpha, {BlendOptionString(blendState.BlendDesc.SourceAlphaBlend)})");
            renderStates.AppendLine($"\tRenderState(DstBlendAlpha, {BlendOptionString(blendState.BlendDesc.DestinationAlphaBlend)})");
            renderStates.AppendLine($"\tRenderState(BlendOpAlpha, {BlendOpString(blendState.BlendDesc.AlphaBlendOperation)})\n");
        }

        if (Material.RenderStates.RasterizerState() != -1)
        {
            var rasState = RenderStates.RasterizerStates[Material.RenderStates.RasterizerState()];
            renderStates.AppendLine($"\tRenderState(FillMode, {rasState.FillMode.ToString().ToUpper()})");
            renderStates.AppendLine($"\tRenderState(CullMode, {rasState.CullMode.ToString().ToUpper()})");
            renderStates.AppendLine($"\tRenderState(DepthClipEnable, {rasState.DepthClipEnable.ToString().ToLower()})\n");
        }

        if (Material.RenderStates.DepthBiasState() != -1)
        {
            var depthState = RenderStates.DepthBiasStates[Material.RenderStates.DepthBiasState()];
            renderStates.AppendLine($"\tRenderState(DepthBias, {depthState.DepthBias.ToString("F1")})");
            renderStates.AppendLine($"\tRenderState(SlopeScaleDepthBias, {depthState.SlopeScaledDepthBias.ToString("F1")})");
            renderStates.AppendLine($"\tRenderState(DepthBiasClamp, {depthState.DepthBiasClamp.ToString("F1")})\n");
        }

        if (Material.RenderStates.DepthStencilState() != -1)
        {
            var depthStencilState = RenderStates.DepthStencilStates[Material.RenderStates.DepthStencilState()];
            renderStates.AppendLine($"\tRenderState(DepthEnable, {depthStencilState.Depth.Enable.ToString().ToLower()})");
            renderStates.AppendLine($"\tRenderState(DepthWriteEnable, {(depthStencilState.Depth.WriteMask == 0 ? "false" : "true")})");
            renderStates.AppendLine($"\tRenderState(DepthFunc, {CompareFuncString(depthStencilState.Depth.Func)})\n");

            // TODO: Need correct StencilRef 
            //renderStates.AppendLine($"\tRenderState(StencilEnable, {depthStencilState.Stencil.StencilEnable.ToString().ToLower()})");
            //renderStates.AppendLine($"\tRenderState(StencilRef, 36)");
            //renderStates.AppendLine($"\tRenderState(StencilReadMask, {(byte)depthStencilState.Stencil.StencilReadMask})");
            //renderStates.AppendLine($"\tRenderState(StencilWriteMask, {(byte)depthStencilState.Stencil.StencilWriteMask})");

            //renderStates.AppendLine($"\tRenderState(StencilFailOp, {StencilOpString(depthStencilState.Stencil.FrontFace.FailOp)})");
            //renderStates.AppendLine($"\tRenderState(StencilDepthFailOp, {StencilOpString(depthStencilState.Stencil.FrontFace.DepthFailOp)})");
            //renderStates.AppendLine($"\tRenderState(StencilPassOp, {StencilOpString(depthStencilState.Stencil.FrontFace.PassOp)})");
            //renderStates.AppendLine($"\tRenderState(StencilFunc, {CompareFuncString(depthStencilState.Stencil.FrontFace.Func)})");

            //renderStates.AppendLine($"\tRenderState(BackStencilFailOp, {StencilOpString(depthStencilState.Stencil.BackFace.FailOp)})");
            //renderStates.AppendLine($"\tRenderState(BackStencilDepthFailOp, {StencilOpString(depthStencilState.Stencil.BackFace.DepthFailOp)})");
            //renderStates.AppendLine($"\tRenderState(BackStencilPassOp, {StencilOpString(depthStencilState.Stencil.BackFace.PassOp)})");
            //renderStates.AppendLine($"\tRenderState(BackStencilFunc, {CompareFuncString(depthStencilState.Stencil.BackFace.Func)})");
        }

        return renderStates.ToString();
    }

    private string StencilOpString(StencilOperation op)
    {
        switch (op)
        {
            case (StencilOperation.Keep):
                return "KEEP";
            case (StencilOperation.Zero):
                return "ZERO";
            case (StencilOperation.Replace):
                return "REPLACE";
            case (StencilOperation.IncrementAndClamp):
                return "INCR_SAT";
            case (StencilOperation.DecrementAndClamp):
                return "DECR_SAT";
            case (StencilOperation.Invert):
                return "INVERT";
            case (StencilOperation.Increment):
                return "INCR";
            case (StencilOperation.Decrement):
                return "DECR";
            default:
                return "KEEP";
        }
    }

    private string CompareFuncString(Comparison comparison)
    {
        switch (comparison)
        {
            case (Comparison.Never):
                return "NEVER";
            case (Comparison.Less):
                return "LESS";
            case (Comparison.Equal):
                return "EQUAL";
            case (Comparison.LessEqual):
                return "LESS_EQUAL";
            case (Comparison.Greater):
                return "GREATER";
            case (Comparison.GreaterEqual):
                return "GREATER_EQUAL";
            case (Comparison.NotEqual):
                return "NOT_EQUAL";
            case (Comparison.Always):
                return "ALWAYS";
            default:
                return "ALWAYS";
        }
    }

    private string BlendOptionString(BlendOption blendOption)
    {
        switch (blendOption)
        {
            case (BlendOption.Zero):
                return "ZERO";
            case (BlendOption.One):
                return "ONE";
            case (BlendOption.SourceColor):
                return "SRC_COLOR";
            case (BlendOption.InverseSourceColor):
                return "INV_SRC_COLOR";
            case (BlendOption.SourceAlpha):
                return "SRC_ALPHA";
            case (BlendOption.InverseSourceAlpha):
                return "INV_SRC_ALPHA";
            case (BlendOption.DestinationAlpha):
                return "DEST_ALPHA";
            case (BlendOption.InverseDestinationAlpha):
                return "INV_DEST_ALPHA";
            case (BlendOption.DestinationColor):
                return "DEST_COLOR";
            case (BlendOption.InverseDestinationColor):
                return "INV_DEST_COLOR";
            case (BlendOption.SourceAlphaSaturate):
                return "SRC_ALPHA_SAT";
            case (BlendOption.BlendFactor):
                return "BLEND_FACTOR";
            case (BlendOption.SecondarySourceColor):
                return "SRC1_COLOR";
            case (BlendOption.InverseSecondarySourceColor):
                return "INV_SRC1_COLOR";
            case (BlendOption.SecondarySourceAlpha):
                return "SRC1_ALPHA";
            case (BlendOption.InverseSecondarySourceAlpha):
                return "INV_SRC1_ALPHA";
            default:
                return "ONE";
        }
    }

    private string BlendOpString(BlendOperation blendOp)
    {
        switch (blendOp)
        {
            case (BlendOperation.Add):
                return "ADD";
            case (BlendOperation.Subtract):
                return "SUBTRACT";
            case (BlendOperation.ReverseSubtract):
                return "REV_SUBTRACT";
            case (BlendOperation.Minimum):
                return "MIN";
            case (BlendOperation.Maximum):
                return "MAX";
            default:
                return "ADD";
        }
    }
}
