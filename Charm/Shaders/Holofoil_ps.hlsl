cbuffer InvResTime : register(b0)
{
    //float ScreenWidth;
    //float ScreenHeight;
    float Time;
};

#define cmp -

float4 main(float2 v0 : TEXCOORD) : SV_Target
{
	float4 o0,r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12;
    float2 v1 = v0;// /float2(ScreenHeight, ScreenWidth);
	//v0.xy = v0 / float2(ScreenHeight, ScreenWidth);
	
	float4 cb0[74] = {
    float4(3.0, 0.0, 0.0, 0.0), 
    float4(5.0, 5.0, 0.0, 0.0), 
    float4(9.0, 9.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.2, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), // 6
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.2, 0.0, 0.0, 0.0), 
    float4(0.01, 0.0, 0.0, 0.0), 
    float4(251.0, 0.0, 0.0, 0.0), 
    float4(7.0, 0.0, 0.0, 0.0), 
    float4(15.0, 0.0, 0.0, 0.0), 
    float4(0.112876266, 0.0416148, 0.15684614, 1.0), 
    float4(0.02777657, 0.0065044165, 0.037007883, 1.0), 
    float4(0.03543072, 0.0075743333, 0.039055157, 1.0), 
    float4(0.09883968, 0.033104762, 0.12477182, 1.0), 
    float4(0.114625804, 0.07109033, 0.1214127, 1.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.25, 0.0, 0.0, 0.0), 
    float4(0.5, 0.0, 0.0, 0.0), 
    float4(0.75, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 4.0, 0.0, 0.0), 
    float4(0.25, 4.0, 0.0, 0.0), 
    float4(0.5, 4.0, 0.0, 0.0), 
    float4(0.75, 4.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.25, 0.0, 0.0, 0.0), 
    float4(0.5, 0.0, 0.0, 0.0), 
    float4(0.75, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 4.0, 0.0, 0.0), 
    float4(0.25, 4.0, 0.0, 0.0), 
    float4(0.5, 4.0, 0.0, 0.0), 
    float4(0.75, 4.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.25, 0.0, 0.0, 0.0), 
    float4(0.5, 0.0, 0.0, 0.0), 
    float4(0.75, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 4.0, 0.0, 0.0), 
    float4(0.25, 4.0, 0.0, 0.0), 
    float4(0.5, 4.0, 0.0, 0.0), 
    float4(0.75, 4.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.25, 0.0, 0.0, 0.0), 
    float4(0.5, 0.0, 0.0, 0.0), 
    float4(0.75, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 4.0, 0.0, 0.0), 
    float4(0.25, 4.0, 0.0, 0.0), 
    float4(0.5, 4.0, 0.0, 0.0), 
    float4(0.75, 4.0, 0.0, 0.0), 
    float4(1.0, 1.0, 1.0, 0.0), 
    float4(1.0, 0.0, 0.0, 0.0), 
    float4(42.0, 0.0, 0.0, 0.0), 
    float4(0.0001, 0.0, 0.0, 0.0), 
    float4(1.25, 0.0, 0.0, 0.0), 
    float4(101.0, 0.0, 0.0, 0.0), 
    float4(0.01, 0.19, 0.0, 0.0), 
    float4(0.5, 0.5, 0.5, 0.0), 
    float4(0.05, 0.2, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(0.0, 0.0, 0.0, 0.0), 
    float4(1.0, 1.0, 1.0, 0.9), 
  };

  cb0[6] = cb0[10] = float4(94.00, 72.00, 0.00, 0.00);
  cb0[13] = Time;
  cb0[69] = float4(0.30588, 0.20, 0.38824, 1.00);

  r0.x = cb0[10].y + cb0[6].x;
  r0.y = cmp(cb0[62].x >= 0);
  r0.y = r0.y ? 4.99999987e-005 : -4.99999987e-005;
  r0.y = cb0[62].x + r0.y;
  r0.zw = v1.yx * cb0[0].xx + cb0[1].yx;
  r0.zw = -cb0[2].yx + r0.zw;
  r1.x = dot(r0.zw, r0.zw);
  r1.x = sqrt(r1.x);
  r1.y = min(abs(r0.z), abs(r0.w));
  r1.z = max(abs(r0.z), abs(r0.w));
  r1.z = 1 / r1.z;
  r1.y = r1.y * r1.z;
  r1.z = r1.y * r1.y;
  r1.w = r1.z * 0.0208350997 + -0.0851330012;
  r1.w = r1.z * r1.w + 0.180141002;
  r1.w = r1.z * r1.w + -0.330299497;
  r1.z = r1.z * r1.w + 0.999866009;
  r1.w = r1.y * r1.z;
  r2.x = cmp(abs(r0.w) < abs(r0.z));
  r1.w = r1.w * -2 + 1.57079637;
  r1.w = r2.x ? r1.w : 0;
  r1.y = r1.y * r1.z + r1.w;
  r1.z = cmp(r0.w < -r0.w);
  r1.z = r1.z ? -3.141593 : 0;
  r1.y = r1.y + r1.z;
  r1.z = min(r0.z, r0.w);
  r0.z = max(r0.z, r0.w);
  r0.w = cmp(r1.z < -r1.z);
  r0.z = cmp(r0.z >= -r0.z);
  r0.z = r0.z ? r0.w : 0;
  r0.z = r0.z ? -r1.y : r1.y;
  r0.z = cb0[3].x * cb0[4].x + r0.z;
  sincos(r0.z, r2.x, r3.x);
  r3.y = r2.x;
  r0.zw = r3.xy * r1.xx + cb0[2].xy;
  r1.x = cb0[14].x * cb0[13].x;
  r1.x = r0.x * cb0[15].x + r1.x;
  r1.yz = floor(r0.zw);
  r0.zw = frac(r0.zw);
  r2.xy = float2(0,10000);
  r1.w = -1;
  while (true) {
    r2.z = cmp(1 < (int)r1.w);
    if (r2.z != 0) break;
    r3.z = (int)r1.w;
    r2.zw = r2.yx;
    r3.x = -1;
    while (true) {
      r3.w = cmp(1 < (int)r3.x);
      if (r3.w != 0) break;
      r3.y = (int)r3.x;
      r4.xy = r3.yz + -r0.zw;
      r5.yz = r3.yz + r1.yz;
      r3.yw = r5.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r3.yw = frac(r3.yw);
      r3.yw = float2(17,17) * r3.yw;
      r4.z = r3.y * r3.w;
      r3.y = r3.y + r3.w;
      r3.y = r4.z * r3.y;
      r3.y = frac(r3.y);
      r3.w = dot(r3.yy, r3.yy);
      r3.w = sqrt(r3.w);
      r4.z = 0.5 * r3.y;
      r3.y = r3.y * 20 + r1.x;
      sincos(r3.y, r6.x, r7.x);
      r7.y = r6.x;
      r3.yw = r7.xy * r3.ww;
      r3.yw = r3.yw * float2(0.5,0.5) + r4.zz;
      r3.yw = r4.xy + r3.yw;
      r5.x = dot(r3.yw, r3.yw);
      r3.y = cmp(r5.x < r2.z);
      r2.zw = r3.yy ? r5.xy : r2.zw;
      r3.x = (int)r3.x + 1;
    }
    r2.xy = r2.wz;
    r1.w = (int)r1.w + 1;
  }
  r2.yz = float2(0,10000);
  r1.w = -1;
  while (true) {
    r2.w = cmp(1 < (int)r1.w);
    if (r2.w != 0) break;
    r3.z = (int)r1.w;
    r3.xw = r2.zy;
    r2.w = -1;
    while (true) {
      r4.x = cmp(1 < (int)r2.w);
      if (r4.x != 0) break;
      r3.y = (int)r2.w;
      r4.xy = r3.yz + -r0.zw;
      r5.yz = r3.yz + r1.yz;
      r4.zw = r5.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r4.zw = frac(r4.zw);
      r4.zw = float2(17,17) * r4.zw;
      r3.y = r4.z * r4.w;
      r4.z = r4.z + r4.w;
      r3.y = r4.z * r3.y;
      r3.y = frac(r3.y);
      r4.z = dot(r3.yy, r3.yy);
      r4.z = sqrt(r4.z);
      r4.w = 0.5 * r3.y;
      r3.y = r3.y * 20 + r1.x;
      sincos(r3.y, r6.x, r7.x);
      r7.y = r6.x;
      r5.yw = r7.xy * r4.zz;
      r4.zw = r5.yw * float2(0.5,0.5) + r4.ww;
      r4.xy = r4.xy + r4.zw;
      r5.x = dot(r4.xy, r4.xy);
      r3.y = cmp(r5.x < r3.x);
      r3.xw = r3.yy ? r5.xz : r3.xw;
      r2.w = (int)r2.w + 1;
    }
    r2.yz = r3.wx;
    r1.w = (int)r1.w + 1;
  }
  r1.w = cb0[17].x * r2.y;
  r1.w = r2.x * cb0[16].x + r1.w;
  r1.w = r1.w / cb0[62].x;
  r2.x = cmp(r1.w >= -r1.w);
  r1.w = frac(abs(r1.w));
  r1.w = r2.x ? r1.w : -r1.w;
  r1.w = cb0[62].x * r1.w;
  r1.w = r1.w / r0.y;
  r1.w = r0.x * cb0[63].x + r1.w;
  r2.x = cb0[64].x * cb0[13].x;
  r1.w = r1.w * cb0[65].x + r2.x;
  r1.w = sin(r1.w);
  r1.w = max(0, r1.w);
  r1.w = saturate(cb0[68].y * r1.w + cb0[68].x);
  r3.xyz = cb0[69].xyz * cb0[67].xyz;
  r3.w = cb0[69].w * r1.w;
  r1.w = cmp(cb0[18].x >= 0);
  r1.w = r1.w ? 4.99999987e-005 : -4.99999987e-005;
  r1.w = cb0[18].x + r1.w;
  r2.yzw = float3(0,10000,-1);
  while (true) {
    r4.x = cmp(1 < (int)r2.w);
    if (r4.x != 0) break;
    r4.z = (int)r2.w;
    r4.xw = r2.zy;
    r5.x = -1;
    while (true) {
      r5.y = cmp(1 < (int)r5.x);
      if (r5.y != 0) break;
      r4.y = (int)r5.x;
      r5.yz = r4.yz + -r0.zw;
      r6.yz = r4.yz + r1.yz;
      r6.zw = r6.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r6.zw = frac(r6.zw);
      r6.zw = float2(17,17) * r6.zw;
      r4.y = r6.z * r6.w;
      r5.w = r6.z + r6.w;
      r4.y = r5.w * r4.y;
      r4.y = frac(r4.y);
      r5.w = dot(r4.yy, r4.yy);
      r5.w = sqrt(r5.w);
      r6.z = 0.5 * r4.y;
      r4.y = r4.y * 20 + r1.x;
      sincos(r4.y, r7.x, r8.x);
      r8.y = r7.x;
      r7.xy = r8.xy * r5.ww;
      r6.zw = r7.xy * float2(0.5,0.5) + r6.zz;
      r5.yz = r6.zw + r5.yz;
      r6.x = dot(r5.yz, r5.yz);
      r4.y = cmp(r6.x < r4.x);
      r4.xw = r4.yy ? r6.xy : r4.xw;
      r5.x = (int)r5.x + 1;
    }
    r2.yz = r4.wx;
    r2.w = (int)r2.w + 1;
  }
  r2.zw = float2(0,10000);
  r4.x = -1;
  while (true) {
    r4.y = cmp(1 < (int)r4.x);
    if (r4.y != 0) break;
    r4.z = (int)r4.x;
    r5.xy = r2.wz;
    r4.w = -1;
    while (true) {
      r5.z = cmp(1 < (int)r4.w);
      if (r5.z != 0) break;
      r4.y = (int)r4.w;
      r5.zw = r4.yz + -r0.zw;
      r6.yz = r4.yz + r1.yz;
      r6.yw = r6.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r6.yw = frac(r6.yw);
      r6.yw = float2(17,17) * r6.yw;
      r4.y = r6.y * r6.w;
      r6.y = r6.y + r6.w;
      r4.y = r6.y * r4.y;
      r4.y = frac(r4.y);
      r6.y = dot(r4.yy, r4.yy);
      r6.y = sqrt(r6.y);
      r6.w = 0.5 * r4.y;
      r4.y = r4.y * 20 + r1.x;
      sincos(r4.y, r7.x, r8.x);
      r8.y = r7.x;
      r7.xy = r8.xy * r6.yy;
      r6.yw = r7.xy * float2(0.5,0.5) + r6.ww;
      r5.zw = r6.yw + r5.zw;
      r6.x = dot(r5.zw, r5.zw);
      r4.y = cmp(r6.x < r5.x);
      r5.xy = r4.yy ? r6.xz : r5.xy;
      r4.w = (int)r4.w + 1;
    }
    r2.zw = r5.yx;
    r4.x = (int)r4.x + 1;
  }
  r2.z = cb0[17].x * r2.z;
  r2.y = r2.y * cb0[16].x + r2.z;
  r2.y = r2.y / cb0[18].x;
  r2.z = cmp(r2.y >= -r2.y);
  r2.y = frac(abs(r2.y));
  r2.y = r2.z ? r2.y : -r2.y;
  r2.y = cb0[18].x * r2.y;
  r2.y = r2.y / r1.w;
  r2.z = cmp(r2.y < cb0[25].x);
  r2.w = cmp(r2.y < cb0[26].x);
  r4.x = cmp(r2.y >= cb0[25].x);
  r2.w = r2.w ? r4.x : 0;
  r4.x = cmp(r2.y < cb0[27].x);
  r4.y = cmp(r2.y >= cb0[26].x);
  r4.x = r4.y ? r4.x : 0;
  r4.y = cmp(r2.y >= cb0[27].x);
  r4.z = -cb0[29].x + r2.y;
  r4.z = saturate(cb0[29].y * r4.z);
  r5.xyzw = cb0[20].xyzw + -cb0[19].xyzw;
  r4.z = r4.z * r5.x + cb0[19].x;
  r4.w = -cb0[30].x + r2.y;
  r4.w = saturate(cb0[30].y * r4.w);
  r6.xyzw = cb0[21].xyzw + -cb0[20].xyzw;
  r4.w = r4.w * r6.x + cb0[20].x;
  r5.x = -cb0[31].x + r2.y;
  r5.x = saturate(cb0[31].y * r5.x);
  r7.xyzw = cb0[22].xyzw + -cb0[21].xyzw;
  r5.x = r5.x * r7.x + cb0[21].x;
  r2.y = -cb0[32].x + r2.y;
  r2.y = saturate(cb0[32].y * r2.y);
  r8.xyzw = cb0[23].xyzw + -cb0[22].xyzw;
  r2.y = r2.y * r8.x + cb0[22].x;
  r2.z = r2.z ? r4.z : 0;
  r2.z = r2.w ? r4.w : r2.z;
  r2.z = r4.x ? r5.x : r2.z;
  r4.x = r4.y ? r2.y : r2.z;
  r2.yzw = float3(0,10000,-1);
  while (true) {
    r4.w = cmp(1 < (int)r2.w);
    if (r4.w != 0) break;
    r9.z = (int)r2.w;
    r9.xw = r2.zy;
    r4.w = -1;
    while (true) {
      r5.x = cmp(1 < (int)r4.w);
      if (r5.x != 0) break;
      r9.y = (int)r4.w;
      r10.xy = r9.yz + -r0.zw;
      r11.yz = r9.yz + r1.yz;
      r10.zw = r11.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r10.zw = frac(r10.zw);
      r10.zw = float2(17,17) * r10.zw;
      r5.x = r10.z * r10.w;
      r6.x = r10.z + r10.w;
      r5.x = r6.x * r5.x;
      r5.x = frac(r5.x);
      r6.x = dot(r5.xx, r5.xx);
      r6.x = sqrt(r6.x);
      r7.x = 0.5 * r5.x;
      r5.x = r5.x * 20 + r1.x;
      sincos(r5.x, r5.x, r12.x);
      r12.y = r5.x;
      r10.zw = r12.xy * r6.xx;
      r10.zw = r10.zw * float2(0.5,0.5) + r7.xx;
      r10.xy = r10.xy + r10.zw;
      r11.x = dot(r10.xy, r10.xy);
      r5.x = cmp(r11.x < r9.x);
      r9.xw = r5.xx ? r11.xy : r9.xw;
      r4.w = (int)r4.w + 1;
    }
    r2.yz = r9.wx;
    r2.w = (int)r2.w + 1;
  }
  r2.zw = float2(0,10000);
  r4.w = -1;
  while (true) {
    r5.x = cmp(1 < (int)r4.w);
    if (r5.x != 0) break;
    r9.z = (int)r4.w;
    r9.xw = r2.wz;
    r5.x = -1;
    while (true) {
      r6.x = cmp(1 < (int)r5.x);
      if (r6.x != 0) break;
      r9.y = (int)r5.x;
      r10.xy = r9.yz + -r0.zw;
      r11.yz = r9.yz + r1.yz;
      r10.zw = r11.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r10.zw = frac(r10.zw);
      r10.zw = float2(17,17) * r10.zw;
      r6.x = r10.z * r10.w;
      r7.x = r10.z + r10.w;
      r6.x = r7.x * r6.x;
      r6.x = frac(r6.x);
      r7.x = dot(r6.xx, r6.xx);
      r7.x = sqrt(r7.x);
      r8.x = 0.5 * r6.x;
      r6.x = r6.x * 20 + r1.x;
      sincos(r6.x, r6.x, r12.x);
      r12.y = r6.x;
      r10.zw = r12.xy * r7.xx;
      r10.zw = r10.zw * float2(0.5,0.5) + r8.xx;
      r10.xy = r10.xy + r10.zw;
      r11.x = dot(r10.xy, r10.xy);
      r6.x = cmp(r11.x < r9.x);
      r9.xw = r6.xx ? r11.xz : r9.xw;
      r5.x = (int)r5.x + 1;
    }
    r2.zw = r9.wx;
    r4.w = (int)r4.w + 1;
  }
  r2.z = cb0[17].x * r2.z;
  r2.y = r2.y * cb0[16].x + r2.z;
  r2.y = r2.y / cb0[18].x;
  r2.z = cmp(r2.y >= -r2.y);
  r2.y = frac(abs(r2.y));
  r2.y = r2.z ? r2.y : -r2.y;
  r2.y = cb0[18].x * r2.y;
  r2.y = r2.y / r1.w;
  r2.z = cmp(r2.y < cb0[34].x);
  r2.w = cmp(r2.y < cb0[35].x);
  r4.w = cmp(r2.y >= cb0[34].x);
  r2.w = r2.w ? r4.w : 0;
  r4.w = cmp(r2.y < cb0[36].x);
  r5.x = cmp(r2.y >= cb0[35].x);
  r4.w = r4.w ? r5.x : 0;
  r5.x = cmp(r2.y >= cb0[36].x);
  r6.x = -cb0[38].x + r2.y;
  r6.x = saturate(cb0[38].y * r6.x);
  r5.y = r6.x * r5.y + cb0[19].y;
  r6.x = -cb0[39].x + r2.y;
  r6.x = saturate(cb0[39].y * r6.x);
  r6.x = r6.x * r6.y + cb0[20].y;
  r6.y = -cb0[40].x + r2.y;
  r6.y = saturate(cb0[40].y * r6.y);
  r6.y = r6.y * r7.y + cb0[21].y;
  r2.y = -cb0[41].x + r2.y;
  r2.y = saturate(cb0[41].y * r2.y);
  r2.y = r2.y * r8.y + cb0[22].y;
  r2.z = r2.z ? r5.y : 0;
  r2.z = r2.w ? r6.x : r2.z;
  r2.z = r4.w ? r6.y : r2.z;
  r4.y = r5.x ? r2.y : r2.z;
  r2.yzw = float3(0,10000,-1);
  while (true) {
    r4.w = cmp(1 < (int)r2.w);
    if (r4.w != 0) break;
    r9.z = (int)r2.w;
    r5.xy = r2.zy;
    r4.w = -1;
    while (true) {
      r6.x = cmp(1 < (int)r4.w);
      if (r6.x != 0) break;
      r9.y = (int)r4.w;
      r6.xy = r9.yz + -r0.zw;
      r10.yz = r9.yz + r1.yz;
      r7.xy = r10.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r7.xy = frac(r7.xy);
      r7.xy = float2(17,17) * r7.xy;
      r8.x = r7.x * r7.y;
      r7.x = r7.x + r7.y;
      r7.x = r8.x * r7.x;
      r7.x = frac(r7.x);
      r7.y = dot(r7.xx, r7.xx);
      r7.y = sqrt(r7.y);
      r8.x = 0.5 * r7.x;
      r7.x = r7.x * 20 + r1.x;
      sincos(r7.x, r7.x, r9.x);
      r9.y = r7.x;
      r7.xy = r9.xy * r7.yy;
      r7.xy = r7.xy * float2(0.5,0.5) + r8.xx;
      r6.xy = r7.xy + r6.xy;
      r10.x = dot(r6.xy, r6.xy);
      r6.x = cmp(r10.x < r5.x);
      r5.xy = r6.xx ? r10.xy : r5.xy;
      r4.w = (int)r4.w + 1;
    }
    r2.yz = r5.yx;
    r2.w = (int)r2.w + 1;
  }
  r2.zw = float2(0,10000);
  r4.w = -1;
  while (true) {
    r5.x = cmp(1 < (int)r4.w);
    if (r5.x != 0) break;
    r9.z = (int)r4.w;
    r5.xy = r2.wz;
    r6.x = -1;
    while (true) {
      r6.y = cmp(1 < (int)r6.x);
      if (r6.y != 0) break;
      r9.y = (int)r6.x;
      r7.xy = r9.yz + -r0.zw;
      r10.yz = r9.yz + r1.yz;
      r8.xy = r10.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r8.xy = frac(r8.xy);
      r8.xy = float2(17,17) * r8.xy;
      r6.y = r8.x * r8.y;
      r8.x = r8.x + r8.y;
      r6.y = r8.x * r6.y;
      r6.y = frac(r6.y);
      r8.x = dot(r6.yy, r6.yy);
      r8.x = sqrt(r8.x);
      r8.y = 0.5 * r6.y;
      r6.y = r6.y * 20 + r1.x;
      sincos(r6.y, r9.x, r11.x);
      r11.y = r9.x;
      r9.xy = r11.xy * r8.xx;
      r8.xy = r9.xy * float2(0.5,0.5) + r8.yy;
      r7.xy = r8.xy + r7.xy;
      r10.x = dot(r7.xy, r7.xy);
      r6.y = cmp(r10.x < r5.x);
      r5.xy = r6.yy ? r10.xz : r5.xy;
      r6.x = (int)r6.x + 1;
    }
    r2.zw = r5.yx;
    r4.w = (int)r4.w + 1;
  }
  r2.z = cb0[17].x * r2.z;
  r2.y = r2.y * cb0[16].x + r2.z;
  r2.y = r2.y / cb0[18].x;
  r2.z = cmp(r2.y >= -r2.y);
  r2.y = frac(abs(r2.y));
  r2.y = r2.z ? r2.y : -r2.y;
  r2.y = cb0[18].x * r2.y;
  r2.y = r2.y / r1.w;
  r2.z = cmp(r2.y < cb0[43].x);
  r2.w = cmp(r2.y < cb0[44].x);
  r4.w = cmp(r2.y >= cb0[43].x);
  r2.w = r2.w ? r4.w : 0;
  r4.w = cmp(r2.y < cb0[45].x);
  r5.x = cmp(r2.y >= cb0[44].x);
  r4.w = r4.w ? r5.x : 0;
  r5.x = cmp(r2.y >= cb0[45].x);
  r5.y = -cb0[47].x + r2.y;
  r5.y = saturate(cb0[47].y * r5.y);
  r5.y = r5.y * r5.z + cb0[19].z;
  r5.z = -cb0[48].x + r2.y;
  r5.z = saturate(cb0[48].y * r5.z);
  r5.z = r5.z * r6.z + cb0[20].z;
  r6.x = -cb0[49].x + r2.y;
  r6.x = saturate(cb0[49].y * r6.x);
  r6.x = r6.x * r7.z + cb0[21].z;
  r2.y = -cb0[50].x + r2.y;
  r2.y = saturate(cb0[50].y * r2.y);
  r2.y = r2.y * r8.z + cb0[22].z;
  r2.z = r2.z ? r5.y : 0;
  r2.z = r2.w ? r5.z : r2.z;
  r2.z = r4.w ? r6.x : r2.z;
  r4.z = r5.x ? r2.y : r2.z;
  r2.yzw = float3(0,10000,-1);
  while (true) {
    r4.w = cmp(1 < (int)r2.w);
    if (r4.w != 0) break;
    r5.z = (int)r2.w;
    r6.xy = r2.zy;
    r4.w = -1;
    while (true) {
      r5.x = cmp(1 < (int)r4.w);
      if (r5.x != 0) break;
      r5.y = (int)r4.w;
      r7.xy = r5.yz + -r0.zw;
      r8.yz = r5.yz + r1.yz;
      r5.xy = r8.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r5.xy = frac(r5.xy);
      r5.xy = float2(17,17) * r5.xy;
      r6.z = r5.x * r5.y;
      r5.x = r5.x + r5.y;
      r5.x = r6.z * r5.x;
      r5.x = frac(r5.x);
      r5.y = dot(r5.xx, r5.xx);
      r5.y = sqrt(r5.y);
      r6.z = 0.5 * r5.x;
      r5.x = r5.x * 20 + r1.x;
      sincos(r5.x, r5.x, r9.x);
      r9.y = r5.x;
      r5.xy = r9.xy * r5.yy;
      r5.xy = r5.xy * float2(0.5,0.5) + r6.zz;
      r5.xy = r7.xy + r5.xy;
      r8.x = dot(r5.xy, r5.xy);
      r5.x = cmp(r8.x < r6.x);
      r6.xy = r5.xx ? r8.xy : r6.xy;
      r4.w = (int)r4.w + 1;
    }
    r2.yz = r6.yx;
    r2.w = (int)r2.w + 1;
  }
  r2.zw = float2(0,10000);
  r4.w = -1;
  while (true) {
    r5.x = cmp(1 < (int)r4.w);
    if (r5.x != 0) break;
    r5.z = (int)r4.w;
    r6.xy = r2.wz;
    r5.x = -1;
    while (true) {
      r6.z = cmp(1 < (int)r5.x);
      if (r6.z != 0) break;
      r5.y = (int)r5.x;
      r7.xy = r5.yz + -r0.zw;
      r8.yz = r5.yz + r1.yz;
      r9.xy = r8.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r9.xy = frac(r9.xy);
      r9.xy = float2(17,17) * r9.xy;
      r5.y = r9.x * r9.y;
      r6.z = r9.x + r9.y;
      r5.y = r6.z * r5.y;
      r5.y = frac(r5.y);
      r6.z = dot(r5.yy, r5.yy);
      r6.z = sqrt(r6.z);
      r7.z = 0.5 * r5.y;
      r5.y = r5.y * 20 + r1.x;
      sincos(r5.y, r9.x, r10.x);
      r10.y = r9.x;
      r9.xy = r10.xy * r6.zz;
      r9.xy = r9.xy * float2(0.5,0.5) + r7.zz;
      r7.xy = r9.xy + r7.xy;
      r8.x = dot(r7.xy, r7.xy);
      r5.y = cmp(r8.x < r6.x);
      r6.xy = r5.yy ? r8.xz : r6.xy;
      r5.x = (int)r5.x + 1;
    }
    r2.zw = r6.yx;
    r4.w = (int)r4.w + 1;
  }
  r2.z = cb0[17].x * r2.z;
  r2.y = r2.y * cb0[16].x + r2.z;
  r2.y = r2.y / cb0[18].x;
  r2.z = cmp(r2.y >= -r2.y);
  r2.y = frac(abs(r2.y));
  r2.y = r2.z ? r2.y : -r2.y;
  r2.y = cb0[18].x * r2.y;
  r1.w = r2.y / r1.w;
  r2.y = cmp(r1.w < cb0[52].x);
  r2.z = cmp(r1.w < cb0[53].x);
  r2.w = cmp(r1.w >= cb0[52].x);
  r2.z = r2.w ? r2.z : 0;
  r2.w = cmp(r1.w < cb0[54].x);
  r4.w = cmp(r1.w >= cb0[53].x);
  r2.w = r2.w ? r4.w : 0;
  r4.w = cmp(r1.w >= cb0[54].x);
  r5.x = -cb0[56].x + r1.w;
  r5.x = saturate(cb0[56].y * r5.x);
  r5.x = r5.x * r5.w + cb0[19].w;
  r5.y = -cb0[57].x + r1.w;
  r5.y = saturate(cb0[57].y * r5.y);
  r5.y = r5.y * r6.w + cb0[20].w;
  r5.z = -cb0[58].x + r1.w;
  r5.z = saturate(cb0[58].y * r5.z);
  r5.z = r5.z * r7.w + cb0[21].w;
  r1.w = -cb0[59].x + r1.w;
  r1.w = saturate(cb0[59].y * r1.w);
  r1.w = r1.w * r8.w + cb0[22].w;
  r2.y = r2.y ? r5.x : 0;
  r2.y = r2.z ? r5.y : r2.y;
  r2.y = r2.w ? r5.z : r2.y;
  r1.w = r4.w ? r1.w : r2.y;
  r2.y = 1.0; // t0.Sample(s1_s, v1.xy).w;
  r2.y = saturate(cb0[61].x * r2.y);
  r2.zw = float2(0,10000);
  r4.w = -1;
  while (true) {
    r5.x = cmp(1 < (int)r4.w);
    if (r5.x != 0) break;
    r5.z = (int)r4.w;
    r5.xw = r2.wz;
    r6.x = -1;
    while (true) {
      r6.y = cmp(1 < (int)r6.x);
      if (r6.y != 0) break;
      r5.y = (int)r6.x;
      r6.yz = r5.yz + -r0.zw;
      r7.yz = r5.yz + r1.yz;
      r7.zw = r7.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r7.zw = frac(r7.zw);
      r7.zw = float2(17,17) * r7.zw;
      r5.y = r7.z * r7.w;
      r6.w = r7.z + r7.w;
      r5.y = r6.w * r5.y;
      r5.y = frac(r5.y);
      r6.w = dot(r5.yy, r5.yy);
      r6.w = sqrt(r6.w);
      r7.z = 0.5 * r5.y;
      r5.y = r5.y * 20 + r1.x;
      sincos(r5.y, r8.x, r9.x);
      r9.y = r8.x;
      r8.xy = r9.xy * r6.ww;
      r7.zw = r8.xy * float2(0.5,0.5) + r7.zz;
      r6.yz = r7.zw + r6.yz;
      r7.x = dot(r6.yz, r6.yz);
      r5.y = cmp(r7.x < r5.x);
      r5.xw = r5.yy ? r7.xy : r5.xw;
      r6.x = (int)r6.x + 1;
    }
    r2.zw = r5.wx;
    r4.w = (int)r4.w + 1;
  }
  r5.xy = float2(0,10000);
  r2.w = -1;
  while (true) {
    r4.w = cmp(1 < (int)r2.w);
    if (r4.w != 0) break;
    r6.z = (int)r2.w;
    r5.zw = r5.yx;
    r4.w = -1;
    while (true) {
      r6.x = cmp(1 < (int)r4.w);
      if (r6.x != 0) break;
      r6.y = (int)r4.w;
      r6.xw = r6.yz + -r0.zw;
      r7.yz = r6.yz + r1.yz;
      r7.yw = r7.yz * float2(0.318309903,0.318309903) + float2(0.100000001,0.100000001);
      r7.yw = frac(r7.yw);
      r7.yw = float2(17,17) * r7.yw;
      r6.y = r7.y * r7.w;
      r7.y = r7.y + r7.w;
      r6.y = r7.y * r6.y;
      r6.y = frac(r6.y);
      r7.y = dot(r6.yy, r6.yy);
      r7.y = sqrt(r7.y);
      r7.w = 0.5 * r6.y;
      r6.y = r6.y * 20 + r1.x;
      sincos(r6.y, r8.x, r9.x);
      r9.y = r8.x;
      r8.xy = r9.xy * r7.yy;
      r7.yw = r8.xy * float2(0.5,0.5) + r7.ww;
      r6.xy = r7.yw + r6.xw;
      r7.x = dot(r6.xy, r6.xy);
      r6.x = cmp(r7.x < r5.z);
      r5.zw = r6.xx ? r7.xz : r5.zw;
      r4.w = (int)r4.w + 1;
    }
    r5.xy = r5.wz;
    r2.w = (int)r2.w + 1;
  }
  r0.z = cb0[17].x * r5.x;
  r0.z = r2.z * cb0[16].x + r0.z;
  r0.z = r0.z / cb0[62].x;
  r0.w = cmp(r0.z >= -r0.z);
  r0.z = frac(abs(r0.z));
  r0.z = r0.w ? r0.z : -r0.z;
  r0.z = cb0[62].x * r0.z;
  r0.y = r0.z / r0.y;
  r0.x = r0.x * cb0[63].x + r0.y;
  r0.x = r0.x * cb0[65].x + r2.x;
  r0.x = sin(r0.x);
  r0.x = max(0, r0.x);
  r0.x = saturate(cb0[66].y * r0.x + cb0[66].x);
  r0.x = r2.y + r0.x;
  r0.x = min(1, r0.x);
  r2.xyz = cb0[60].xyz * r4.xyz;
  r2.w = r1.w * r0.x;
  r0.xyzw = saturate(float4(18.3791599,18.3791599,18.3791599,18.3791599) * r3.xyzw);
  r1.xyzw = saturate(r3.xyzw * float4(4.59478998,4.59478998,4.59478998,4.59478998) + float4(-0.25,-0.25,-0.25,-0.25));
  r0.xyzw = r2.xyzw * r0.xyzw + r1.xyzw;
  r0.xyzw = cb0[73].xyzw * r0.xyzw;
  // r0.xyzw = cb11[0].xyzw * r0.xyzw;
  o0.xyz = r0.xyz * r0.www;
  o0.w = r0.w;
  return o0;
}