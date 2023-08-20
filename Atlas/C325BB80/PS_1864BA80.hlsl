// ---- Created with 3Dmigoto v1.3.16 on Wed Feb 15 23:57:46 2023
Texture2D<float4> t8 : register(t8);

Texture2D<float4> t7 : register(t7);

Texture2D<float4> t6 : register(t6);

Texture2D<float4> t5 : register(t5);

Texture2D<float4> t4 : register(t4);

Texture2D<float4> t3 : register(t3);

Texture2D<float4> t2 : register(t2);

Texture3D<float4> t1 : register(t1);

Texture2D<float4> t0 : register(t0);

SamplerState s3_s : register(s3);

SamplerState s2_s : register(s2);

SamplerState s1_s : register(s1);

cbuffer cb12 : register(b12)
{
  float4 cb12[8];
}

cbuffer cb0 : register(b0)
{
  float4 cb0[86];
}




// 3Dmigoto declarations
#define cmp -


void main(
  float4 v0 : TEXCOORD0,
  float4 v1 : TEXCOORD1,
  float4 v2 : TEXCOORD2,
  float4 v3 : TEXCOORD3,
  float4 v4 : TEXCOORD4,
  float4 v5 : TEXCOORD8,
  float4 v6 : SV_POSITION0,
  uint v7 : SV_isFrontFace0,
  out float4 o0 : SV_TARGET0,
  out float4 o1 : SV_TARGET1,
  out float4 o2 : SV_TARGET2)
{
  float4 r0,r1,r2,r3,r4,r5,r6,r7,r8,r9;
  uint4 bitmask, uiDest;
  float4 fDest;

  r0.xyz = v4.xyz * cb0[0].xyz + cb0[1].xyz;
  r0.x = t1.Sample(s2_s, r0.xyz).x;
  r0.x = saturate(cb0[2].y * r0.x + cb0[2].x);
  r0.yz = v4.xy * cb0[4].xy + cb0[4].zw;
  r0.w = t2.Sample(s2_s, r0.yz).w;
  r1.x = cb0[41].x + 1;
  r1.yz = v3.xy * cb0[38].xy + cb0[38].zw;
  r1.w = t6.Sample(s3_s, r1.yz).x;
  r2.xyz = t8.Sample(s2_s, r1.yz).xyz;
  r1.y = cb0[40].y * r1.w + cb0[40].x;
  r1.y = cb0[41].x * r1.y;
  r1.z = saturate(4 * v5.w);
  r2.w = saturate(-0.25 + v5.w);
  r1.z = r1.w * r1.z + r2.w;
  r3.y = saturate(cb0[39].y * r1.z + cb0[39].x);
  r3.xz = float2(0,0);
  r1.zw = t5.Sample(s1_s, r3.xy).xw;
  r1.x = saturate(r1.w * r1.x + -r1.y);
  r1.y = -r1.x * cb0[42].x + 1;
  r3.xy = cb0[44].xy * r1.xx;
  r3.xy = r2.xy * r3.xx + r3.yy;
  r1.xy = cb0[43].xy * r1.yy;
  r4.xyz = t7.Sample(s2_s, v3.xy).xyz;
  r1.xy = r4.xy * r1.xx + r1.yy;
  r2.w = saturate(cb0[43].z + r4.z);
  r1.xy = r1.xy + r3.xy;
  r4.xyz = cb12[7].xyz + -v4.xyz;
  r3.x = dot(r4.xyz, r4.xyz);
  r3.x = sqrt(r3.x);
  r3.x = saturate(r3.x * cb0[45].x + cb0[45].y);
  r3.y = 1 + -r3.x;
  r4.xyz = t4.Sample(s2_s, v3.xy).xyz;
  r4.w = saturate(cb0[46].y * r4.z + cb0[46].x);
  r5.xy = t3.Sample(s2_s, v3.xy).yx;
  r5.z = cb0[47].y * r5.x + cb0[47].x;
  r4.w = r5.z * r4.w;
  r3.y = r4.w * r3.y;
  r3.x = r4.w * r3.x;
  r5.zw = cb0[50].xy * r3.xx;
  r3.xy = cb0[48].xy * r3.yy;
  r2.xy = r2.xy * r3.xx + r3.yy;
  r1.xy = r2.xy + r1.xy;
  r2.xy = v3.xy * cb0[49].xy + cb0[49].zw;
  r6.xyz = t8.Sample(s2_s, r2.xy).xyz;
  r2.xy = r6.xy * r5.zz + r5.ww;
  r3.x = saturate(cb0[50].z + r6.z);
  r1.xy = r2.xy + r1.xy;
  r2.x = dot(r1.xy, r1.xy);
  r2.x = 1 + -r2.x;
  r2.x = max(0, r2.x);
  r2.x = sqrt(r2.x);
  r6.xyz = v2.xyz * r1.yyy;
  r6.xyz = v1.xyz * r1.xxx + r6.xyz;
  r6.xyz = v0.xyz * r2.xxx + r6.xyz;
  r1.x = dot(r6.xyz, r6.xyz);
  r1.x = rsqrt(r1.x);
  r6.xyz = r6.xyz * r1.xxx;
  r7.xyz = log2(abs(r6.xyz));
  r7.xyz = cb0[3].xxx * r7.xyz;
  r7.xyz = exp2(r7.xyz);
  r1.x = dot(r7.xyz, float3(1,1,1));
  r7.xyz = r7.xyz / r1.xxx;
  r8.xyzw = v4.yzxz * cb0[4].xyxy + cb0[4].zwzw;
  r0.y = t2.Sample(s2_s, r8.xy).y;
  r0.z = t2.Sample(s2_s, r8.zw).y;
  r0.y = dot(r0.yzw, r7.xyz);
  r0.x = r0.x * r0.y;
  r3.w = cb0[5].y * r0.x + cb0[5].x;
  r0.x = cb0[32].y * r0.x + cb0[32].x;
  r0.x = max(cb0[30].x, r0.x);
  r0.x = min(cb0[31].x, r0.x);
  r0.x = cb0[35].y * r0.x + cb0[35].x;
  r0.x = max(cb0[33].x, r0.x);
  r0.x = min(cb0[34].x, r0.x);
  r7.xyw = t0.Sample(s1_s, r3.zw).yzx;
  r7.xyw = saturate(r7.xyw);
  r0.y = cmp(r7.x < r7.y);
  r8.xy = r7.yx;
  r9.xy = r8.yx;
  r8.zw = float2(-1,0.666666687);
  r9.zw = float2(0,-0.333333343);
  r8.xyzw = r0.yyyy ? r8.xyzw : r9.xyzw;
  r0.y = cmp(r7.w < r8.x);
  r7.xyz = r8.xyw;
  r8.xyw = r7.wyx;
  r7.xyzw = r0.yyyy ? r7.xyzw : r8.xyzw;
  r0.y = min(r7.w, r7.y);
  r0.y = r7.x + -r0.y;
  r0.z = r0.y * 6 + 1.00000001e-007;
  r0.w = r7.w + -r7.y;
  r0.z = r0.w / r0.z;
  r0.z = r7.z + r0.z;
  r0.z = saturate(cb0[6].x + abs(r0.z));
  r3.yzw = float3(1,0.666666687,0.333333343) + r0.zzz;
  r3.yzw = frac(r3.yzw);
  r3.yzw = r3.yzw * float3(6,6,6) + float3(-3,-3,-3);
  r3.yzw = saturate(float3(-1,-1,-1) + abs(r3.yzw));
  r3.yzw = float3(-1,-1,-1) + r3.yzw;
  r0.z = 1.00000001e-007 + r7.x;
  r0.w = saturate(cb0[8].x + r7.x);
  r0.y = r0.y / r0.z;
  r0.y = saturate(cb0[7].x + r0.y);
  r3.yzw = r0.yyy * r3.yzw + float3(1,1,1);
  r0.y = -cb0[14].x + cb0[12].x;
  r0.z = -cb0[18].x + r5.y;
  r0.z = saturate(cb0[18].y * r0.z);
  r0.y = r0.z * r0.y + cb0[14].x;
  r0.z = cmp(r5.y < cb0[16].x);
  r0.y = r0.z ? r0.y : 0;
  r0.z = cb0[14].x + -cb0[12].x;
  r1.x = -cb0[19].x + r5.y;
  r1.x = saturate(cb0[19].y * r1.x);
  r0.z = r1.x * r0.z + cb0[12].x;
  r1.x = cmp(r5.y >= cb0[16].x);
  r0.y = r1.x ? r0.z : r0.y;
  r0.z = cmp(1 < v3.y);
  r0.z = r0.z ? cb0[12].x : cb0[11].x;
  r0.z = cb0[13].x * r0.z;
  r1.x = cb0[13].x + 1;
  r0.z = saturate(r5.y * r1.x + -r0.z);
  r7.xyz = cb0[10].xyz + -cb0[9].xyz;
  r7.xyz = saturate(r0.zzz * r7.xyz + cb0[9].xyz);
  r8.xyz = r7.xyz * r4.xyz;
  r7.xyz = -r7.xyz * r4.xyz + r4.xyz;
  r7.xyz = r0.yyy * r7.xyz + r8.xyz;
  r0.y = r4.x * r1.z;
  r0.z = cb0[83].y * r4.y + cb0[83].x;
  r0.y = cb0[20].x * r0.y;
  r0.y = saturate(cb0[21].y * r0.y + cb0[21].x);
  r1.x = -cb0[28].x + r0.y;
  r1.x = saturate(cb0[28].y * r1.x);
  r4.xyz = cb0[23].xyz + -cb0[22].xyz;
  r1.xyz = r1.xxx * r4.xyz + cb0[22].xyz;
  r2.x = cmp(r0.y < cb0[26].x);
  r1.xyz = r2.xxx ? r1.xyz : 0;
  r2.x = -cb0[29].x + r0.y;
  r0.y = cmp(r0.y >= cb0[26].x);
  r2.x = saturate(cb0[29].y * r2.x);
  r4.xyz = cb0[24].xyz + -cb0[23].xyz;
  r4.xyz = r2.xxx * r4.xyz + cb0[23].xyz;
  r1.xyz = saturate(r0.yyy ? r4.xyz : r1.xyz);
  r1.xyz = r1.xyz + -r7.xyz;
  r1.xyz = r1.www * r1.xyz + r7.xyz;
  r0.y = cb0[82].y * r1.w + cb0[82].x;
  r0.y = saturate(r0.y * r0.z);
  r1.xyz = -r0.www * r3.yzw + r1.xyz;
  r3.yzw = r3.yzw * r0.www;
  r0.z = max(cb0[36].x, r0.x);
  r0.z = min(cb0[37].x, r0.z);
  o0.xyz = r0.zzz * r1.xyz + r3.yzw;
  o0.w = 0;
  r0.z = saturate(cb0[48].z + r2.z);
  r0.w = saturate(cb0[44].z + r2.z);
  r0.z = min(r0.z, r3.x);
  r0.z = min(r0.w, r0.z);
  r0.z = min(r2.w, r0.z);
  r0.w = -cb0[81].x + cb0[12].x;
  r0.y = r0.y * r0.w + cb0[81].x;
  r0.y = -cb0[80].x + r0.y;
  r0.x = r0.x * r0.y + cb0[80].x;
  r0.x = r5.y * r0.x;
  r0.x = max(cb0[84].x, r0.x);
  r0.x = saturate(min(cb0[85].x, r0.x));
  r0.x = min(r0.x, r0.z);
  r0.x = r0.x * 0.125 + 0.375;
  o1.xyz = saturate(r6.xyz * r0.xxx + float3(0.5,0.5,0.5));
  o1.w = 0;
  r0.x = cb0[77].y * r5.y + cb0[77].x;
  r5.x = saturate(r5.x);
  o2.y = 0.5 * r5.x;
  r0.x = max(cb0[78].x, r0.x);
  o2.x = min(cb0[79].x, r0.x);
  o2.z = 0;
  o2.w = v0.w;
  return;
}