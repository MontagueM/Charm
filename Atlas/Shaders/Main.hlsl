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

struct VS_INPUTENTITY
{
    float4 Position : POSITION;
    float4 Tangent : TANGENT;
    float2 TexCoord : TEXCOORD0;
};

struct VS_INPUTCAR
{
    float4 Position : POSITION;
    float4 Tangent : TANGENT;
    float2 TexCoord : TEXCOORD0;
};


struct PS_INPUTCAR
{
    float4 v0 : TEXCOORD0;
    float4 v1 : TEXCOORD1;
    float4 v2 : TEXCOORD2;
    float4 TexCoord : TEXCOORD3;
    float3 v4 : TEXCOORD4;
    float4 Position : SV_POSITION0;
};

struct PS_INPUTPOLE
{
    float4 v0 : TEXCOORD0;
    float4 v1 : TEXCOORD1;
    float4 v2 : TEXCOORD2;
    float4 TexCoord : TEXCOORD3;
    float3 v4 : TEXCOORD4;
  	float4 v5 : TEXCOORD8;
    float4 Position : SV_POSITION0;
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

// first thing to try is to use real PS but fake VS

PS_INPUT VSEntity( VS_INPUTENTITY input )
{
    PS_INPUT output = (PS_INPUT)0;
    input.Position.xyz = input.Position.xzy;
    input.Position.x = -input.Position.x;
    // output.Position = input.Position;
    output.Position = mul( input.Position, World );
    output.Position = mul( output.Position, View );
    output.Position = mul( output.Position, Projection );
    // output.Normal = mul( input.Tangent, World ).xyz; // wrong
    //output.TexCoord.x = (input.TexCoord.x * 5.56146240234375) + -3.03619384765625;
    //output.TexCoord.y = (input.TexCoord.y * 5.56146240234375) - (1 - 5.20550537109375);
    output.TexCoord.x = (input.TexCoord.x * 5.56146240234375) + -3.03619384765625;
    output.TexCoord.y = (input.TexCoord.y * 5.56146240234375) + 5.20550537109375;
    return output;
}

PS_INPUTCAR VSCar( VS_INPUTCAR input )
{
    PS_INPUTCAR output = (PS_INPUTCAR)0;
    input.Position.xyz = input.Position.xzy;
    input.Position.x = -input.Position.x;
    // output.Position = input.Position;
    output.Position = mul( input.Position, World );
    output.Position = mul( output.Position, View );
    output.Position = mul( output.Position, Projection );
    // output.Normal = mul( input.Tangent, World ).xyz; // wrong
    //output.TexCoord.x = (input.TexCoord.x * 5.56146240234375) + -3.03619384765625;
    //output.TexCoord.y = (input.TexCoord.y * 5.56146240234375) - (1 - 5.20550537109375);
    output.v0 = float4(0,0,1,0);
    output.v1 = float4(1,1,1,1);
    output.v2 = float4(1,1,1,1);
    output.v4 = float4(1,1,1,1);
    output.TexCoord.x = (input.TexCoord.x * 5.56146240234375) + -3.03619384765625;
    output.TexCoord.y = (input.TexCoord.y * 5.56146240234375) + 5.20550537109375;
    return output;
}

PS_INPUTPOLE VSPole( VS_INPUTCAR input )
{
    PS_INPUTPOLE output = (PS_INPUTPOLE)0;
    input.Position.xyz = input.Position.xzy;
    input.Position.x = -input.Position.x;
    // output.Position = input.Position;
    output.Position = mul( input.Position, World );
    output.Position = mul( output.Position, View );
    output.Position = mul( output.Position, Projection );
    // output.Normal = mul( input.Tangent, World ).xyz; // wrong
    //output.TexCoord.x = (input.TexCoord.x * 5.56146240234375) + -3.03619384765625;
    //output.TexCoord.y = (input.TexCoord.y * 5.56146240234375) - (1 - 5.20550537109375);
    output.v0 = float4(0,0,1,0);
    output.v1 = float4(1,1,1,1);
    output.v2 = float4(1,1,1,1);
    output.v4 = float4(1,1,1,1);
    output.v5 = float4(1,1,1,1);
    output.TexCoord.x = (input.TexCoord.x * 9.33603668212891) + 2.78580379486084;
    output.TexCoord.y = (input.TexCoord.y * 9.33603668212891) + 8.54026317596436;
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