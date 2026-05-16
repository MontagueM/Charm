// Just the default S&Box shading model with my own tweak
// that allows certain materials (Taken for example) to always be emissive, ignoring lighting.
// Will probably iterate on in the future for more things if needed.

#ifndef COMMON_PIXEL_H
#define COMMON_PIXEL_H
#include "sbox_pixel.fxc"
#include "common/material.hlsl"
#endif // COMMON_PIXEL_H

#ifndef COMMON_PIXEL_SHADING_H
#define COMMON_PIXEL_SHADING_H

#include "common/material.hlsl"
#include "common/GBuffer.hlsl"
#include "common/classes/Decals.hlsl"

float4 DoAtmospherics(float3 vPositionWs, float2 vPositionSs, float4 vColor, bool bAdditiveBlending = false)
{
    vPositionWs = vPositionWs.xyz;
    float3 vPositionToCameraWs = vPositionWs.xyz - g_vCameraPositionWs.xyz;

    if (g_bFogEnabled)
    {
        if (bAdditiveBlending)
        {
			//
			// When blending additively, dest pixel contains the fog term.
			// Therefore, we want to scale alpha with fog amount in front of us,
			// rather than double-adding the fog
			//
            if (g_bGradientFogEnabled)
                vColor.a *= 1.0 - CalculateGradientFog(vPositionWs, vPositionToCameraWs).a;

            if (g_bCubemapFogEnabled)
                vColor.a *= 1.0 - CalculateCubemapFog(vPositionWs, vPositionToCameraWs).a;

            if (g_bVolumetricFogEnabled)
                vColor.a *= CalculateVolumetricFog(vPositionWs.xyz, vPositionSs.xy).a;
        }
        else
        {
            vColor.rgb = ApplyGradientFog(vColor.rgb, vPositionWs.xyz, vPositionToCameraWs.xyz);
            vColor.rgb = ApplyCubemapFog(vColor.rgb, vPositionWs.xyz, vPositionToCameraWs.xyz);
            vColor.rgb = ApplyVolumetricFog(vColor.rgb, vPositionWs.xyz, vPositionSs.xy);
        }
    }

    return vColor;
}

float4 DoPostProcessing(const Material material, float4 color)
{
    // Remove alpha if we are not transparent, might be shit but screenshots are being written
    // with alpha in some shaders
    #ifndef CUSTOM_MATERIAL_INPUTS
        #if ( !S_ALPHA_TEST && !S_TRANSLUCENT && !TRANSLUCENT )
        {
        color.a = 1.0f;
    }
#endif
#endif

    return color;
}

void AdjustAlphaToCoverage(inout Material m)
{
#if ( S_ALPHA_TEST )
    {
        float eps = 1.0f/255.0f;
        
        // Clip first to try to kill the wave if we're in an area of all zero
        clip(m.Opacity - eps);

        m.Opacity = AdjustOpacityForAlphaToCoverage( m.Opacity, g_flAlphaTestReference, g_flAntiAliasedEdgeStrength, m.TextureCoords.xy );

        if (g_nMSAASampleCount == 1)
            OpaqueFadeDepth((m.Opacity + 0.5f + eps) * 0.5f, m.ScreenPosition.xy);
        else
            clip(m.Opacity - 0.000001); // Second clipping pass after alpha to coverage adjustment
    }
#endif
}

class D2ShadingModelStandard
{
    //
    // Converts our Material struct to the CombinerInput structure used by Valve's lighting model.
    //
    static CombinerInput MaterialToCombinerInput(Material m)
    {
        CombinerInput o = PS_InitFinalCombiner();
      
        // Position and geometric data
        o.vPositionWithOffsetWs = m.WorldPositionWithOffset;
        o.vPositionWs = m.WorldPosition;
        o.vPositionSs = m.ScreenPosition;

        // Normal and tangent space
        o.vNormalWs = m.Normal;
        o.vNormalTs = NormalWorldToTangent(m.Normal, m.WorldTangentU, m.WorldTangentV);
        o.vTangentUWs = m.WorldTangentU;
        o.vTangentVWs = m.WorldTangentV;

        // Material properties
        o.vRoughness = m.Roughness.xx;
        o.vEmissive = m.Emission;
        o.flAmbientOcclusion = m.AmbientOcclusion;
        o.vTransmissiveMask = m.Transmission;
        o.flOpacity = m.Opacity;

        // Lighting and UV coordinates
        o.vLightmapUV = m.LightmapUV;
        o.vTextureCoords = m.TextureCoords;

        // Adjustments
        {
            // Convert Metalness to Internal Specular
            o = CalculateDiffuseAndSpecularFromAlbedoAndMetalness(o, m.Albedo.rgb, m.Metalness);

            // Roughness adjusted to fix geometric specular aliasing
            o.vRoughness.xy = AdjustRoughnessByGeometricNormal(o.vRoughness.xy, o.vNormalWs.xyz);
        }
        
        return o;
    }


    static float4 Shade(Material m, float o0w)
    {
		
        // Want it right before the lighting
        Decals::Apply(m.WorldPosition, m);
        
        // Do our magic alpha to coverage adjustment
        AdjustAlphaToCoverage(m);

        LightingTerms_t lightingTerms = InitLightingTerms();
        CombinerInput combinerInput = MaterialToCombinerInput(m);

        // Calculate lighting
        {
            ComputeDirectLighting(lightingTerms, combinerInput);
            CalculateIndirectLighting(lightingTerms, combinerInput);
        }

        // Composite lighting terms, apply adjustments 
        float4 color;
        {
		
            float3 vDiffuseAO = CalculateDiffuseAmbientOcclusion(combinerInput, lightingTerms);
            lightingTerms.vIndirectDiffuse.rgb *= vDiffuseAO.rgb;
            lightingTerms.vDiffuse.rgb *= lerp(float3(1.0, 1.0, 1.0), vDiffuseAO.rgb, combinerInput.flAmbientOcclusionDirectDiffuse);
            
            float3 vSpecularAO = CalculateSpecularAmbientOcclusion(combinerInput, lightingTerms);
            lightingTerms.vIndirectSpecular.rgb *= vSpecularAO.rgb;
            lightingTerms.vSpecular.rgb *= lerp(float3(1.0, 1.0, 1.0), vSpecularAO.rgb, combinerInput.flAmbientOcclusionDirectSpecular);
            
            float3 vDiffuse = ((lightingTerms.vDiffuse.rgb + lightingTerms.vIndirectDiffuse.rgb) * combinerInput.vDiffuseColor.rgb) + combinerInput.vEmissive.rgb;
            float3 vSpecular = lightingTerms.vSpecular.rgb + lightingTerms.vIndirectSpecular.rgb;
            
            color = float4(o0w >= 0.995000005 ? combinerInput.vEmissive.rgb : vDiffuse + vSpecular, m.Opacity);
        }

        //
        if (DepthNormals::WantsDepthNormals())
            return DepthNormals::Output(m.Normal, m.Roughness, color.a);

        if (ToolsVis::WantsToolsVis())
            return DoToolsVis(color, m, lightingTerms);

        // Composite atmospherics after lighting
        color = DoAtmospherics(m.WorldPosition, m.ScreenPosition.xy, color);
        
        return color;
    }

    /// <summary>
    /// Tools visualization for the standard shading model.
    /// </summary>
    static float4 DoToolsVis(inout float4 color, Material m, LightingTerms_t lightingTerms)
    {
        ToolsVis toolVis = ToolsVis::Init(color, lightingTerms.vDiffuse.rgb, lightingTerms.vSpecular.rgb, lightingTerms.vIndirectDiffuse.rgb, lightingTerms.vIndirectSpecular.rgb, lightingTerms.vTransmissive.rgb);

        toolVis.HandleFlatOverlayColor(m.Albedo, color);
        toolVis.HandleFullbright(color, m.Albedo, m.WorldPosition, m.Normal);
        toolVis.HandleDiffuseLighting(color);
        toolVis.HandleSpecularLighting(color);
        toolVis.HandleTransmissiveLighting(color);
        toolVis.HandleLightingComplexity(color, m.WorldPosition, m.ScreenPosition, m.Normal);
        toolVis.HandleAlbedo(color, m.Albedo);
        toolVis.HandleReflectivity(color, m.Albedo);
        toolVis.HandleRoughness(color, float2(m.Roughness, m.Roughness));
        toolVis.HandleDiffuseAmbientOcclusion(color, min(m.AmbientOcclusion, min(lightingTerms.flBakedAmbientOcclusion, lightingTerms.flDynamicAmbientOcclusion)));
        toolVis.HandleSpecularAmbientOcclusion(color, min(m.AmbientOcclusion, min(lightingTerms.flBakedAmbientOcclusion, lightingTerms.flDynamicAmbientOcclusion)));
        toolVis.HandleShaderIDColor(color);
        toolVis.HandleCubemapReflections(color, m.WorldPosition, m.ScreenPosition, m.Normal);
        toolVis.HandleNormalTs(color, m.TangentNormal);
        toolVis.HandleNormalWs(color, m.Normal);
        toolVis.HandleTangentUWs(color, m.WorldTangentU);
        toolVis.HandleTangentVWs(color, m.WorldTangentV);
        toolVis.HandleBentNormalWs(color, float3(0, 0, 0));
        toolVis.HandleGeometricRoughness(color, m.Normal);
        toolVis.HandleCurvature(color, 0);
        toolVis.HandleTiledRenderingColors(color, m.Albedo, m.ScreenPosition);

// What the fuck
//#ifdef g_tColor
        //Texture2D representativeTexture = g_tColor;
        //toolVis.ShowUVs(color, m.Albedo, representativeTexture, m.TextureCoords);
        //toolVis.ShowMipUtilization(color, m.Albedo, representativeTexture, m.TextureCoords);
//#endif
        // Disable bloom
        color.rgb = saturate(color.rgb);

        return color;
    }
};

#endif // COMMON_PIXEL_SHADING_H
