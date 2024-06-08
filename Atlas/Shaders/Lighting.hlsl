Texture2D RT0 : register( t0 );
Texture2D RT1 : register( t1 );
Texture2D RT2 : register( t2 );


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


struct VertexShaderInput2
{
    float4 Position : SV_POSITION;
};

struct VertexShaderOutput2
{
    float4 Position : SV_POSITION;
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

VertexShaderOutput2 VS2( VertexShaderInput2 input)
{
    VertexShaderOutput2 output = (VertexShaderOutput2)0;

    output.Position = input.Position;

    return output;
}

//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------
// Decode a packed normal (0.0-1.0 -> -1.0-1.0)
float3 DecodeNormal(float3 n) {
    return n * 2.0 - 1.0;
}

float4 PS(VertexShaderOutput input) : SV_Target
{
    int3 sampleIndices = int3( input.Position.xy, 0 );
    float4 rt0 = RT0.Load(sampleIndices);
    float4 rt1 = RT1.Load(sampleIndices);
    float4 rt2 = RT2.Load(sampleIndices);

    float3 albedo = rt0.xyz;
    float3 normal = DecodeNormal(rt1.xyz);

    float smoothness = length(normal) * 4 - 3;
    float roughness = 1.0 - saturate(smoothness);
    float metallic = rt2.x;
    float ao = rt2.y * 2.0;
    float emission = rt2.y * 2.0 - 1.0;

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
    float3 albedo = RT0.Load(sampleIndices).xyz;
    // return float4(1,0,1,1);
    return float4(albedo*4, 1);
}

float4 PS2(VertexShaderOutput2 input) : SV_Target
{
    int3 sampleIndices = int3( input.Position.xy, 0 );
    return float4(RT0.Load(sampleIndices).xyz, 1);
}
