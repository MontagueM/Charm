// ---- Created with 3Dmigoto v1.3.16 on Sun Apr 20 19:33:40 2025

cbuffer procedural_spinner_constant_buffer : register(b0)
{
  float4 inv_resolution_time : packoffset(c0);
}



// 3Dmigoto declarations
#define cmp -


void main(
  uint v0 : SV_VERTEXID0,
  out float4 o0 : SV_POSITION0,
  out float4 o1 : TEXCOORD0)
{
// Needs manual fix for instruction:
// unknown dcl_: dcl_input_sgv v0.x, vertex_id
  float4 r0;
  uint4 bitmask, uiDest;
  float4 fDest;

  r0.x = (uint)v0.x;
  r0.xy = r0.xx * float2(0.25,0.5) + float2(0.125,0.25);
  r0.xy = frac(r0.xy);
  r0.xy = cmp(r0.xy >= float2(0.5,0.5));
  r0.xy = r0.xy ? float2(1,1) : 0;
  o0.xy = r0.xy * float2(2,2) + float2(-1,-1);
  o0.zw = float2(0,1);
  o1.xyzw = inv_resolution_time.xyzw;
  return;
}
