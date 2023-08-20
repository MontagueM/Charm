Texture2D RT0_Albedo : register( t0 );
Texture2D RT1_Normal : register( t1 );


//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer cbLightingChangesEveryFrame  : register( b0 )
{
    float4 LightDir[2];
    float4 LightColor[2];
}

//--------------------------------------------------------------------------------------

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float2 TextureUV : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureUV : TEXCOORD0;
};


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------

VertexShaderOutput VS( VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = input.Position;
    output.TextureUV = input.TextureUV;

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
float4 PS(VertexShaderOutput input) : SV_Target
{
    int3 sampleIndices = int3( input.Position.xy, 0 );
    float3 albedo = RT0_Albedo.Load(sampleIndices).xyz;
    float3 normal = RT1_Normal.Load(sampleIndices).xyz;
    //
    float4 finalColor = 0;
    //
    // //do NdotL lighting for 2 lights
    for(int i=0; i<2; i++)
    {
        finalColor += saturate(  dot( (float3)LightDir[i], normal) * LightColor[i] );
    }
    finalColor.a = 1;
    finalColor.xyz *= albedo.xyz;
    // finalColor.xyz = normal.xyz;
    return finalColor;
}

//--------------------------------------------------------------------------------------
// PSSolid - render a solid color
//--------------------------------------------------------------------------------------
float4 PSSolid(VertexShaderOutput input) : SV_Target
{
    // return float4(input.TextureUV.x, input.TextureUV.y, 0, 1);
    // return float4(1,0,1,1);
    int3 sampleIndices = int3( input.Position.xy, 0 );
    float3 albedo = RT0_Albedo.Load(sampleIndices).xyz;
    // return float4(1,0,1,1);
    return float4(albedo*4, 1);
}