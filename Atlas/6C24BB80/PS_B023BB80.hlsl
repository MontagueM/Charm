// ---- Created with 3Dmigoto v1.3.16 on Tue Feb 14 20:40:25 2023
Texture2D<float4> t6 : register(t6);

Texture2D<float4> t5 : register(t5);

Texture2D<float4> t4 : register(t4);

Texture2D<float4> t3 : register(t3);

Texture2D<float4> t2 : register(t2);

Texture2D<float4> t1 : register(t1);

Texture2D<float4> t0 : register(t0);

SamplerState s3_s : register(s3);

SamplerState s2_s : register(s2);

SamplerState s1_s : register(s1);

cbuffer cb0 : register(b0)
{
  float4 cb0[72];
}




// 3Dmigoto declarations
#define cmp -


void main(
  float4 v0 : TEXCOORD0,
  float4 v1 : TEXCOORD1,
  float4 v2 : TEXCOORD2,
  float4 v3 : TEXCOORD3,
  float3 v4 : TEXCOORD4,
  float4 v5 : SV_POSITION0,
  uint v6 : SV_isFrontFace0,
  out float4 o0 : SV_TARGET0,
  out float4 o1 : SV_TARGET1,
  out float4 o2 : SV_TARGET2)
{
  float4 r0,r1,r2,r3;
  uint4 bitmask, uiDest;
  float4 fDest;

  r0.zw = float2(-1,0.666666687);
  r1.zw = float2(0,-0.333333343);
  r2.x = cmp(1 < v3.y);
  r2.x = saturate(r2.x ? cb0[1].x : cb0[0].x);
  r2.xyz = cb0[3].xyz * r2.xxx + cb0[2].xyz;
  r3.xyw = saturate(r2.yzx);
  r2.w = cmp(r3.x < r3.y);
  r0.xy = r3.yx;
  r1.xy = r0.yx;
  r0.xyzw = r2.wwww ? r0.xyzw : r1.xyzw;
  r1.x = cmp(r3.w < r0.x);
  r3.xyz = r0.xyw;
  r0.xyw = r3.wyx;
  r0.xyzw = r1.xxxx ? r3.xyzw : r0.xyzw;
  r1.x = min(r0.w, r0.y);
  r1.x = -r1.x + r0.x;
  r1.y = r1.x * 6 + 1.00000001e-007;
  r0.y = r0.w + -r0.y;
  r0.y = r0.y / r1.y;
  r0.y = r0.z + r0.y;
  r0.y = saturate(cb0[4].x + abs(r0.y));
  r0.yzw = float3(1,0.666666687,0.333333343) + r0.yyy;
  r0.yzw = frac(r0.yzw);
  r0.yzw = r0.yzw * float3(6,6,6) + float3(-3,-3,-3);
  r0.yzw = saturate(float3(-1,-1,-1) + abs(r0.yzw));
  r0.yzw = float3(-1,-1,-1) + r0.yzw;
  r1.y = 1.00000001e-007 + r0.x;
  r0.x = saturate(cb0[6].x + r0.x);
  r1.x = r1.x / r1.y;
  r1.x = saturate(cb0[5].x + r1.x);
  r0.yzw = r1.xxx * r0.yzw + float3(1,1,1);
  r0.xyz = r0.xxx * r0.yzw + -r2.xyz;
  r1.xy = v3.xy * cb0[18].xy + cb0[18].zw;
  r1.xy = t2.Sample(s1_s, r1.xy).xz;
  r0.w = cb0[9].y * r1.x + cb0[9].x;
  r0.w = max(cb0[7].x, r0.w);
  r0.w = min(cb0[8].x, r0.w);
  r3.xyzw = t0.Sample(s1_s, v3.xy).xyzw;
  r1.z = cb0[12].y * r3.y + cb0[12].x;
  r1.z = max(cb0[10].x, r1.z);
  r1.z = min(cb0[11].x, r1.z);
  r0.w = saturate(r1.z * r0.w);
  r0.xyz = r0.www * r0.xyz + r2.xyz;
  r2.xyz = cb0[13].xyz + -r0.xyz;
  r0.w = saturate(cb0[14].y * r3.y + cb0[14].x);
  r0.xyz = r0.www * r2.xyz + r0.xyz;
  r1.z = dot(v0.xyz, v0.xyz);
  r1.z = rsqrt(r1.z);
  r2.xyz = v0.xyz * r1.zzz;
  r1.z = saturate(dot(cb0[21].xyz, r2.xyz));
  r1.z = saturate(r1.z * cb0[22].x + cb0[22].y);
  r1.w = saturate(dot(r3.xyzw, cb0[23].xyzw));
  r1.z = r1.z + r1.w;
  r1.z = min(1, r1.z);
  r2.xy = v4.xy * cb0[19].xy + cb0[19].zw;
  r1.w = t4.Sample(s1_s, r2.xy).x;
  r1.w = saturate(cb0[20].y * r1.w + cb0[20].x);
  r2.x = r1.w * r1.z;
  r1.z = r1.w * r1.z + -0.25;
  r1.z = max(0, r1.z);
  r1.w = 4 * r2.x;
  r1.w = min(1, r1.w);
  r2.xy = v3.xy * cb0[16].xy + cb0[16].zw;
  r2.x = t3.Sample(s3_s, r2.xy).x;
  r1.z = r2.x * r1.w + r1.z;
  r1.w = saturate(cb0[24].y * r1.z + cb0[24].x);
  r2.w = saturate(cb0[31].y * r1.z + cb0[31].x);
  r2.y = r1.x * r1.w;
  r1.x = cb0[63].y * r1.y + cb0[63].x;
  r2.xz = float2(0,0);
  r1.yzw = t1.Sample(s2_s, r2.xy).xyz;
  r2.x = t1.Sample(s2_s, r2.zw).w;
  r2.x = saturate(cb0[32].y * r2.x + cb0[32].x);
  r2.yzw = cb0[25].xyz * r1.yzw;
  r1.yzw = r1.yzw * cb0[26].xyz + -r2.yzw;
  r3.x = r3.y * cb0[27].x + r3.x;
  r3.x = cb0[30].y * r3.x + cb0[30].x;
  r3.x = max(cb0[28].x, r3.x);
  r3.x = min(cb0[29].x, r3.x);
  r1.yzw = r3.xxx * r1.yzw + r2.yzw;
  r2.yzw = r1.yzw + -r0.xyz;
  r0.xyz = r2.xxx * r2.yzw + r0.xyz;
  o0.xyz = cb0[15].xyz * r0.xyz;
  r0.x = r1.y * cb0[67].x + cb0[67].y;
  r0.x = -cb0[0].x * r1.x + r0.x;
  o0.w = -1;
  r0.y = cb0[0].x * r1.x;
  r0.x = r2.x * r0.x + r0.y;
  r0.x = max(cb0[69].x, r0.x);
  r0.x = saturate(min(cb0[70].x, r0.x));
  r0.y = saturate(cb0[71].x);
  r0.y = r0.y * -2 + -r0.x;
  r0.x = r2.x * r0.y + r0.x;
  r1.yzw = t5.Sample(s1_s, v3.xy).xyz;
  r0.y = saturate(cb0[35].z + r1.w);
  r2.yz = v3.xy * cb0[17].xy + cb0[17].zw;
  r3.xzw = t6.Sample(s1_s, r2.yz).xyz;
  r0.z = t3.Sample(s1_s, r2.yz).w;
  r0.z = saturate(cb0[33].y * r0.z + cb0[33].x);
  r0.z = r0.z + -r3.y;
  r0.z = saturate(r2.x * r0.z + r3.y);
  o2.y = 0.5 * r0.z;
  r0.z = saturate(cb0[36].z + r3.w);
  r0.y = min(r0.y, r0.z);
  r0.x = min(r0.x, r0.y);
  r0.x = r0.x * 0.125 + 0.375;
  r0.y = -r2.x * cb0[1].x + 1;
  r0.yz = cb0[35].xy * r0.yy;
  r0.yz = r1.yz * r0.yy + r0.zz;
  r1.yz = cb0[36].xy * r2.xx;
  r1.yz = r3.xz * r1.yy + r1.zz;
  r0.yz = r1.yz + r0.yz;
  r1.yzw = v2.xyz * r0.zzz;
  r1.yzw = v1.xyz * r0.yyy + r1.yzw;
  r0.y = dot(r0.yz, r0.yz);
  r0.y = 1 + -r0.y;
  r0.y = max(0, r0.y);
  r0.y = sqrt(r0.y);
  r1.yzw = v0.xyz * r0.yyy + r1.yzw;
  r0.y = dot(r1.yzw, r1.yzw);
  r0.y = rsqrt(r0.y);
  r1.yzw = r1.yzw * r0.yyy;
  o1.xyz = saturate(r1.yzw * r0.xxx + float3(0.5,0.5,0.5));
  o1.w = 0;
  r0.x = cb0[65].x + -cb0[64].x;
  r0.x = r2.x * r0.x + cb0[64].x;
  r0.x = saturate(r1.x + r0.x);
  r0.x = r0.w * r0.x;
  o2.x = cb0[66].y * r0.x + cb0[66].x;
  o2.w = cb0[34].y * v0.w + cb0[34].x;
  o2.z = 0;
  return;
}