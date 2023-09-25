cbuffer cb12_View : register(b12)
{
    matrix WorldToProjective;    // cb12[0-3]
    matrix CameraToWorld;        // cb12[4-7]
    float4 Target;               // cb12[8], viewport dimensions 0/1 is width/height, 2/3 is 1/width and 1/height
    float4 Unk09;
    float4 CameraPosition;
    matrix Unk11;                // idk why but cb12[14].z must not be zero ever
    float4 ViewMiscellaneous;    // cb12[15]; cb12[10] is camera position
}

struct VertexShaderInput
{
    float3 Position : POSITION;
	float4 Colour : COLOR;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
	float4 Colour : COLOR;
};


//--------------------------------------------------------------------------------------
// Vertex Shader
//--------------------------------------------------------------------------------------

VertexShaderOutput VS( VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 pos = float4(input.Position.xyz, 1);

    pos = mul(WorldToProjective, pos);

    output.Position = pos;
	output.Colour = input.Colour;


    return output;
}


//--------------------------------------------------------------------------------------
// Pixel Shader
//--------------------------------------------------------------------------------------

void PS(
    VertexShaderOutput input,
    out float4 o0 : SV_TARGET0,
    out float4 o1 : SV_TARGET1,
    out float4 o2 : SV_TARGET2)
{
    o0 = input.Colour;
    o1 = float4(0, 0, 0, 0);
    o2 = float4(0, 0, 0, 0);
}
