// ---- Created with 3Dmigoto v1.3.16 on Fri Feb 10 22:49:48 2023
Buffer<float4> t1 : register(t1);

cbuffer cb1 : register(b1)
{
  float4 cb1[80];
}

cbuffer cb12 : register(b12)
{
  float4 cb12[15];
}




// 3Dmigoto declarations
#define cmp -


void main(
  float4 v0 : POSITION0,
  float4 v1 : TANGENT0,
  float2 v2 : TEXCOORD0,
  uint v3 : SV_VERTEXID0,
  uint v4 : SV_InstanceID0,
  out float4 o0 : TEXCOORD0,
  out float4 o1 : TEXCOORD1,
  out float4 o2 : TEXCOORD2,
  out float4 o3 : TEXCOORD3,
  out float3 o4 : TEXCOORD4,
  out float4 o5 : SV_POSITION0)
{
// Needs manual fix for instruction:
// unknown dcl_: dcl_input_sgv v3.x, vertex_id
// Needs manual fix for instruction:
// unknown dcl_: dcl_input_sgv v4.x, instance_id
  float4 r0,r1,r2,r3;
  uint4 bitmask, uiDest;
  float4 fDest;

  bitmask.x = ((~(-1 << 30)) << 2) & 0xffffffff;  r0.x = (((uint)v4.x << 2) & bitmask.x) | ((uint)1 & ~bitmask.x);
  bitmask.y = ((~(-1 << 30)) << 2) & 0xffffffff;  r0.y = (((uint)v4.x << 2) & bitmask.y) | ((uint)2 & ~bitmask.y);
  bitmask.z = ((~(-1 << 30)) << 2) & 0xffffffff;  r0.z = (((uint)v4.x << 2) & bitmask.z) | ((uint)3 & ~bitmask.z);
  r0.w = 0x02000000 & asint(cb1[r0.z+2].w);
  r0.w = cmp(1 < (uint)r0.w);
  r0.w = r0.w ? 0 : v3.x;
  bitmask.z = ((~(-1 << 25)) << 2) & 0xffffffff;  r0.z = (((uint)cb1[r0.z+2].w << 2) & bitmask.z) | ((uint)0 & ~bitmask.z);
  r0.z = (int)r0.w + (int)r0.z;
  r0.z = t1.Load(r0.z).x;
  o0.w = r0.z;
  r0.z = (uint)v4.x << 2;
  r1.xyzw = v1.zyxz + v1.zyxz;
  r0.w = v1.w * r1.y;
  r2.z = r1.z * v1.z + -r0.w;
  r0.w = -r1.x * v1.z + 1;
  r2.x = -r1.y * v1.y + r0.w;
  r3.y = -r1.z * v1.x + r0.w;
  r1.xw = v1.yw * r1.zw;
  r2.y = r1.x + r1.w;
  r3.x = r1.z * v1.y + -r1.w;
  r3.z = dot(r1.yz, v1.zw);
  r1.x = dot(cb1[r0.z+2].xyz, r2.xyz);
  r1.y = dot(cb1[r0.x+2].xyz, r2.xyz);
  r1.z = dot(cb1[r0.y+2].xyz, r2.xyz);
  r0.w = dot(r1.xyz, r1.xyz);
  r0.w = rsqrt(r0.w);
  r1.xyz = r1.xyz * r0.www;
  o0.xyz = r1.xyz;
  r2.x = dot(cb1[r0.z+2].xyz, r3.xyz);
  r2.y = dot(cb1[r0.x+2].xyz, r3.xyz);
  r2.z = dot(cb1[r0.y+2].xyz, r3.xyz);
  r2.xyzw = r2.xyzz * r0.wwww;
  o1.xyzw = r2.xyzw;
  r3.xyz = r2.ywx * r1.zxy;
  r1.xyz = r1.yzx * r2.wxy + -r3.xyz;
  r0.w = cmp(v1.w >= 0);
  r0.w = r0.w ? 1 : -1;
  o2.xyz = r1.xyz * r0.www;
  o3.xy = v2.xy * cb1[1].xx + cb1[1].yz;
  r1.w = cb1[r0.z+2].w + -cb12[7].x;
  r1.xyz = cb1[r0.z+2].xyz;
  r2.xyz = v0.xyz * cb1[0].www + cb1[0].xyz;
  r2.w = 1;
  r1.x = dot(r1.xyzw, r2.xyzw);
  r3.w = cb1[r0.x+2].w + -cb12[7].y;
  r3.xyz = cb1[r0.x+2].xyz;
  r1.y = dot(r3.xyzw, r2.xyzw);
  r3.w = cb1[r0.y+2].w + -cb12[7].z;
  r3.xyz = cb1[r0.y+2].xyz;
  r1.z = dot(r3.xyzw, r2.xyzw);
  o4.xyz = cb12[7].xyz + r1.xyz;
  r0.xyzw = cb12[1].xyzw * r1.yyyy;
  r0.xyzw = cb12[0].xyzw * r1.xxxx + r0.xyzw;
  r0.xyzw = cb12[2].xyzw * r1.zzzz + r0.xyzw;
  o5.xyzw = cb12[14].xyzw + r0.xyzw;
  return;
}