Texture2D txDiffuse : register( t0 );
SamplerState samLinear : register( s0 );

//--------------------------------------------------------------------------------------
// Constant Buffer Variables
//--------------------------------------------------------------------------------------

cbuffer cbChangeOnResize : register( b0 )
{
    matrix Projection;
};

cbuffer cbGeometryChangesEveryFrame  : register( b1 )
{
    matrix View;
    matrix World;
    float4 MeshColor;
}

//--------------------------------------------------------------------------------------
struct VS_INPUT
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
};

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
};

struct PS_OUTPUT
{
    float4 RT0_Albedo : SV_TARGET0; // albedo xyz
    float4 RT1_Normal : SV_TARGET1; // normal xyz
};

//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------
PS_INPUT VS( VS_INPUT input )
{
    PS_INPUT output = (PS_INPUT)0;
    output.Position = mul( float4(input.Position, 1), World );
    output.Position = mul( output.Position, View );
    output.Position = mul( output.Position, Projection );
    output.Normal = mul( float4( input.Normal, 1 ), World ).xyz;
    output.TexCoord = input.TexCoord;
    return output;
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
PS_OUTPUT PSTexture(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;
    output.RT0_Albedo.xyz = txDiffuse.Sample( samLinear, input.TexCoord ).xyz;
    output.RT1_Normal.xyz = input.Normal.xyz;
    return output;
}

//--------------------------------------------------------------------------------------
// PSSolid - render a solid color
//--------------------------------------------------------------------------------------
PS_OUTPUT PSSolid(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;
    output.RT0_Albedo.xyz = MeshColor.xyz;
    output.RT1_Normal.xyz = input.Normal.xyz;
    return output;
}