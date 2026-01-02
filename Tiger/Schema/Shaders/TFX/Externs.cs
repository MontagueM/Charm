using Arithmic;

namespace Tiger;

// Contains both D1 and D2 externs
public enum TfxExtern : byte
{
    None,
    Frame,
    View,
    Deferred,
    DeferredLight,
    DeferredUberLight,
    DeferredShadow,
    Atmosphere,
    RigidModel,
    EditorMesh,
    EditorMeshMaterial,
    EditorDecal,
    EditorTerrain,
    EditorTerrainPatch,
    EditorTerrainDebug,
    SimpleGeometry,
    UiFont,
    CuiView,
    CuiObject,
    CuiBitmap,
    CuiVideo,
    CuiStandard,
    CuiHud,
    CuiScreenspaceBoxes,
    TextureVisualizer,
    Generic,
    Particle,
    ParticleDebug,
    GearDyeVisualizationMode,
    ScreenArea,
    Mlaa,
    Msaa,
    Hdao,
    DownsampleTextureGeneric,
    DownsampleDepth,
    Ssao,
    VolumetricObscurance,
    Postprocess,
    TextureSet,
    Transparent,
    Vignette,
    GlobalLighting,
    ShadowMask,
    ObjectEffect,
    Decal,
    DecalSetTransform,
    DynamicDecal,
    DecoratorWind,
    TextureCameraLighting,
    VolumeFog,
    Fxaa,
    Smaa,
    Letterbox,
    DepthOfField,
    PostprocessInitialDownsample,
    CopyDepth,
    DisplacementMotionBlur,
    DebugShader,
    MinmaxDepth,
    SdsmBiasAndScale,
    SdsmBiasAndScaleTextures,
    ComputeShadowMapData,
    ComputeLocalLightShadowMapData,
    BilateralUpsample,
    HealthOverlay,
    LightProbeDominantLight,
    LightProbeLightInstance,
    Water,
    LensFlare,
    ScreenShader,
    Scaler,
    GammaControl,
    SpeedtreePlacements,
    Reticle,
    Distortion,
    WaterDebug,
    ScreenAreaInput,
    WaterDepthPrepass,
    OverheadVisibilityMap,
    ParticleCompute,
    CubemapFiltering,
    ParticleFastpath,
    VolumetricsPass,
    TemporalReprojection,
    FxaaCompute,
    VbCopyCompute,
    UberDepth,
    GearDye,
    Cubemaps,
    ShadowBlendWithPrevious,
    DebugShadingOutput,
    Ssao3d,
    WaterDisplacement,
    PatternBlending,
    UiHdrTransform,
    PlayerCenteredCascadedGrid,
    SoftDeform,

    //D1 only
    GearPlatedTextures,
    GearDye0,
    GearDye1,
    GearDye2,
    GearDecalDye,
    ChunkModel,
    Eqaa,
    Evsm,
    DeferredMultiSampled,

    // Added in EoF
    CuiDrawingShader,
    ParticleMeshEmissionCompute
}

public static class Externs
{
    private enum TfxExternD2_EoF : byte
    {
        None,
        Frame,
        View,
        Deferred,
        DeferredLight,
        DeferredUberLight,
        DeferredShadow,
        Atmosphere,
        RigidModel,
        EditorMesh,
        EditorMeshMaterial,
        EditorDecal,
        EditorTerrain,
        EditorTerrainPatch,
        EditorTerrainDebug,
        SimpleGeometry,
        UiFont,
        CuiView,
        CuiObject,
        CuiBitmap,
        CuiVideo,
        CuiStandard,
        CuiHud,
        CuiScreenspaceBoxes,
        CuiDrawingShader,
        TextureVisualizer,
        Generic,
        Particle,
        ParticleDebug,
        GearDyeVisualizationMode,
        ScreenArea,
        Mlaa,
        Msaa,
        Hdao,
        DownsampleTextureGeneric,
        DownsampleDepth,
        Ssao,
        VolumetricObscurance,
        Postprocess,
        TextureSet,
        Transparent,
        Vignette,
        GlobalLighting,
        ShadowMask,
        ObjectEffect,
        Decal,
        DecalSetTransform,
        DynamicDecal,
        DecoratorWind,
        TextureCameraLighting,
        VolumeFog,
        Fxaa,
        Smaa,
        Letterbox,
        DepthOfField,
        PostprocessInitialDownsample,
        CopyDepth,
        DisplacementMotionBlur,
        DebugShader,
        MinmaxDepth,
        SdsmBiasAndScale,
        SdsmBiasAndScaleTextures,
        ComputeShadowMapData,
        ComputeLocalLightShadowMapData,
        BilateralUpsample,
        HealthOverlay,
        LightProbeDominantLight,
        LightProbeLightInstance,
        Water,
        LensFlare,
        ScreenShader,
        Scaler,
        GammaControl,
        SpeedtreePlacements,
        Reticle,
        Distortion,
        WaterDebug,
        ScreenAreaInput,
        WaterDepthPrepass,
        OverheadVisibilityMap,
        ParticleCompute,
        CubemapFiltering,
        ParticleFastpath,
        VolumetricsPass,
        TemporalReprojection,
        FxaaCompute,
        VbCopyCompute,
        UberDepth,
        GearDye,
        Cubemaps,
        ShadowBlendWithPrevious,
        DebugShadingOutput,
        Ssao3d,
        WaterDisplacement,
        PatternBlending,
        UiHdrTransform,
        PlayerCenteredCascadedGrid,
        SoftDeform,
        ParticleMeshEmissionCompute
    }

    private enum TfxExternD2 : byte
    {
        None = 0,
        Frame = 1,
        View = 2,
        Deferred = 3,
        DeferredLight = 4,
        DeferredUberLight = 5,
        DeferredShadow = 6,
        Atmosphere = 7,
        RigidModel = 8,
        EditorMesh = 9,
        EditorMeshMaterial = 10,
        EditorDecal = 11,
        EditorTerrain = 12,
        EditorTerrainPatch = 13,
        EditorTerrainDebug = 14,
        SimpleGeometry = 15,
        UiFont = 16,
        CuiView = 17,
        CuiObject = 18,
        CuiBitmap = 19,
        CuiVideo = 20,
        CuiStandard = 21,
        CuiHud = 22,
        CuiScreenspaceBoxes = 23,
        TextureVisualizer = 24,
        Generic = 25,
        Particle = 26,
        ParticleDebug = 27,
        GearDyeVisualizationMode = 28,
        ScreenArea = 29,
        Mlaa = 30,
        Msaa = 31,
        Hdao = 32,
        DownsampleTextureGeneric = 33,
        DownsampleDepth = 34,
        Ssao = 35,
        VolumetricObscurance = 36,
        Postprocess = 37,
        TextureSet = 38,
        Transparent = 39,
        Vignette = 40,
        GlobalLighting = 41,
        ShadowMask = 42,
        ObjectEffect = 43,
        Decal = 44,
        DecalSetTransform = 45,
        DynamicDecal = 46,
        DecoratorWind = 47,
        TextureCameraLighting = 48,
        VolumeFog = 49,
        Fxaa = 50,
        Smaa = 51,
        Letterbox = 52,
        DepthOfField = 53,
        PostprocessInitialDownsample = 54,
        CopyDepth = 55,
        DisplacementMotionBlur = 56,
        DebugShader = 57,
        MinmaxDepth = 58,
        SdsmBiasAndScale = 59,
        SdsmBiasAndScaleTextures = 60,
        ComputeShadowMapData = 61,
        ComputeLocalLightShadowMapData = 62,
        BilateralUpsample = 63,
        HealthOverlay = 64,
        LightProbeDominantLight = 65,
        LightProbeLightInstance = 66,
        Water = 67,
        LensFlare = 68,
        ScreenShader = 69,
        Scaler = 70,
        GammaControl = 71,
        SpeedtreePlacements = 72,
        Reticle = 73,
        Distortion = 74,
        WaterDebug = 75,
        ScreenAreaInput = 76,
        WaterDepthPrepass = 77,
        OverheadVisibilityMap = 78,
        ParticleCompute = 79,
        CubemapFiltering = 80,
        ParticleFastpath = 81,
        VolumetricsPass = 82,
        TemporalReprojection = 83,
        FxaaCompute = 84,
        VbCopyCompute = 85,
        UberDepth = 86,
        GearDye = 87,
        Cubemaps = 88,
        ShadowBlendWithPrevious = 89,
        DebugShadingOutput = 90,
        Ssao3d = 91,
        WaterDisplacement = 92,
        PatternBlending = 93,
        UiHdrTransform = 94,
        PlayerCenteredCascadedGrid = 95,
        SoftDeform = 96,
    }

    private enum TfxExternD1 : byte
    {
        None = 0,
        Frame = 1,
        View = 2,
        Deferred = 3,
        DeferredLight = 4,
        DeferredUberLight = 5,
        DeferredShadow = 6,
        Atmosphere = 7,
        RigidModel = 8,
        EditorMesh = 9,
        EditorMeshMaterial = 10,
        EditorDecal = 11,
        EditorTerrain = 12,
        EditorTerrainPatch = 13,
        SimpleGeometry = 14,
        UiFont = 15,
        CuiView = 16,
        CuiObject = 17,
        CuiBitmap = 18,
        CuiVideo = 19,
        CuiStandard = 20,
        CuiHud = 21,
        CuiScreenspaceBoxes = 22,
        TextureVisualizer = 23,
        Generic = 24,
        GearPlatedTextures = 25,
        Particle = 26,
        GearDye0 = 27,
        GearDye1 = 28,
        GearDye2 = 29,
        GearDecalDye = 30,
        GearDyeVisualizationMode = 31,
        ScreenArea = 32,
        Mlaa = 33,
        Hdao = 34,
        DownsampleTextureGeneric = 35,
        DownsampleDepth = 36,
        Ssao = 37,
        VolumetricObscurance = 38,
        Postprocess = 39,
        TextureSet = 40,
        Transparent = 41,
        Vignette = 42,
        GlobalLighting = 43,
        ShadowMask = 44,
        ObjectEffect = 45,
        ChunkModel = 46,
        Decal = 47,
        DynamicDecal = 48,
        DecoratorWind = 49,
        TextureCameraLighting = 50,
        VolumeFog = 51,
        Fxaa = 52,
        Eqaa = 53,
        Letterbox = 54,
        DepthOfField = 55,
        PostprocessInitialDownsample = 56,
        CopyDepth = 57,
        DisplacementMotionBlur = 58,
        DebugShader = 59,
        MinmaxDepth = 60,
        SdsmBiasAndScale = 61,
        SdsmBiasAndScaleTextures = 62,
        ComputeShadowMapData = 63,
        ComputeLocalLightShadowMapData = 64,
        Evsm = 65,
        BilateralUpsample = 66,
        HealthOverlay = 67,
        LightProbeDominantLight = 68,
        LightProbeLightInstance = 69,
        Water = 70,
        LensFlare = 71,
        ScreenShader = 72,
        Scaler = 73,
        SpeedtreePlacements = 74,
        Reticle = 75,
        Distortion = 76,
        DeferredMultiSampled = 77,
        WaterDebug = 78,
        ScreenAreaInput = 79,
        WaterDepthPrepass = 80,
    }

    private static readonly TfxExtern[] _externMapD1 = Build<TfxExternD1>();
    private static readonly TfxExtern[] _externMapD2 = Build<TfxExternD2>();
    private static readonly TfxExtern[] _externMapD2EoF = Build<TfxExternD2_EoF>();

    private static TfxExtern[] Build<TEnum>() where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        var map = new TfxExtern[256];

        foreach (var v in values)
        {
            var name = v.ToString();
            if (Enum.TryParse(name, out TfxExtern ext))
                map[Convert.ToByte(v)] = ext;
        }

        return map;
    }

    /// <summary>
    /// Remaps the given byte value to the correct Tfx extern depending on the current version.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    public static TfxExtern GetExtern(byte value)
    {
        return Strategy.CurrentStrategy switch
        {
            TigerStrategy.DESTINY1_RISE_OF_IRON => _externMapD1[value],
            TigerStrategy.DESTINY2_LATEST => _externMapD2EoF[value],
            _ => _externMapD2[value]
        };

        //throw new InvalidCastException($"Couldn't cast extern value {value} ({name}) for {Strategy.CurrentStrategy}");
    }

    // TODO s&box, remove InlineOrDefault since they are defined in the shader now since the removal of dynamic expressions
    public static string GetExternFloat(TfxExtern extern_, int element, bool bInline = false)
    {
        string InlineOrDefault(string name, string defaultValue) =>
                    bInline ? $"float4({name}.xxxx)" : $"(exists({name}) ? ({name}) : ({defaultValue}))";

        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        switch (extern_)
        {
            case TfxExtern.Frame:
                return element switch
                {
                    // Not using inline method for time since its an engine provided value
                    0 => bInline ? $"float4(CurrentTime.xxxx)" : "(Time)", // game_time
                    0x04 => bInline ? $"float4(CurrentTime.xxxx)" : "(Time)", // render_time
                    0x10 => InlineOrDefault("FrameTimeOfDay", "0.5"),
                    0x14 => "float4(0.05.xxxx)", // delta_game_time
                    0x18 => "float4(0.016.xxxx)", // exposure_time
                    0x1C => InlineOrDefault("ExposureScale", "0.65"), // exposure_scale
                    0x28 => InlineOrDefault("ExposureIllumRelative", "1"), // exposure_illum_relative
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {
                    0x70 => InlineOrDefault("AtmosTimeOfDay", "0.5"),
                    0x74 => InlineOrDefault("AtmosUnk74", "0"),
                    0x78 => InlineOrDefault("AtmosUnk78", "0"),
                    0x150 => InlineOrDefault("AtmosUnk150", "0"),
                    0x154 => InlineOrDefault("AtmosUnk154", "0"),
                    0x160 => InlineOrDefault("AtmosFogIntensity", "0"),
                    0x164 => InlineOrDefault("AtmosUnk164", "0"),
                    0x168 => InlineOrDefault("AtmosUnk168", "0"),
                    0x16c => InlineOrDefault("AtmosUnk16C", "0"),
                    0x170 => InlineOrDefault("AtmosUnk170", "0.0001"),
                    0x190 => InlineOrDefault("AtmosUnk190", "0"),
                    0x194 => InlineOrDefault("AtmosUnk194", "0"),
                    0x198 => InlineOrDefault("AtmosUnk198", "0.0001"),
                    0x1b4 => InlineOrDefault("AtmosRotation", "0"),
                    0x1b8 => InlineOrDefault("AtmosIntensity", "1"),
                    0x1bc => InlineOrDefault("AtmosUnk1BC", "0.5"),
                    0x1c0 => InlineOrDefault("AtmosUnk1C0", "0"),
                    0x1c4 => InlineOrDefault("AtmosUnk1C4", "0"),
                    0x1e0 => InlineOrDefault("AtmosUnk1E0", "0"),
                    0x1e4 => InlineOrDefault("AtmosSunIntensity", "0.05923"),
                    0x1e8 => InlineOrDefault("AtmosUnk1E8", "0"),
                    0x1ec => InlineOrDefault("AtmosUnk1EC", "0"),
                    0x1f8 => InlineOrDefault("AtmosUnk1F8", "0"),
                    0x1fc => InlineOrDefault("AtmosUnk1FC", "0"),
                    0x208 => InlineOrDefault("AtmosUnk208", "0"),

                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1,1,1,1)";
        }
    }

    public static string GetExternVec4(TfxExtern extern_, int element, bool bInline = false)
    {
        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        string InlineOrDefault(string name, string defaultValue) =>
            bInline ? $"{name}" : $"(exists({name}) ? ({name}) : ({defaultValue}))";

        switch (extern_)
        {
            case TfxExtern.Deferred:
                return element switch
                {
                    0x0 => "float4((1 / g_flFarPlane)*TO_INCHES, ((g_flFarPlane - g_flNearPlane) / (g_flFarPlane * g_flNearPlane))*TO_INCHES,0,0)",
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Frame:
                return element switch
                {
                    0x1A0 => "float4(0, 0, 0, 0)",
                    0x1C0 => "float4(1, 1, 0, 1)",
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {
                    0x90 => InlineOrDefault("AtmosRTDimensions", "float4(480.0, 270.0, 0.00208, 0.0037)"),
                    0xD0 => "float4(512.0, 512.0, 1.0 / 512.0, 1.0 / 512.0)", // depth_angle_density_lookup_resolution
                    0x110 => InlineOrDefault("AtmosSunDir", "float4(-0.30372, -0.59835, 0.74144, 0.0)"),
                    0x140 => InlineOrDefault("AtmosSunColor", "float4(1.0, 0.95, 0.85, 1.0)"), // actually fog color..?
                    0x180 => InlineOrDefault("AtmosUnk180", "float4(0,0,0,0)"),
                    0x1D0 => InlineOrDefault("AtmosUnk1D0", "float4(0,0,0,0)"),
                    0x210 => InlineOrDefault("AtmosUnk210", "float4(0,0,0,0)"),
                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Decal:
                return element switch
                {
                    0x10 => "float4((1 / g_flFarPlane)*TO_INCHES, ((g_flFarPlane - g_flNearPlane) / (g_flFarPlane * g_flNearPlane))*TO_INCHES,0,0)",
                    0x20 => "float4(0.03, 0, 0, 0)",
                    _ => HandleUnknownElement(element, extern_)
                };
            case TfxExtern.DecalSetTransform:
                return element switch
                {
                    0x0 => "float4(0, 0, 0, 1)",
                    0x10 => "float4(0, 0, 0, 1)",
                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1, 1, 1, 1)";
        }
    }

    // TODO, make this return just the texture attribute name maybe?
    public static string GetExternTexture(TfxExtern extern_, int element, bool bInline = false)
    {
        static string HandleUnknownElement(int element, TfxExtern extern_)
        {
            Log.Warning($"Unimplemented element {element} (0x{element:X}) for extern {extern_}");
            return "float4(1,1,1,1)";
        }

        switch (extern_)
        {
            case TfxExtern.Deferred:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Frame:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Atmosphere:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            case TfxExtern.Decal:
                return element switch
                {

                    _ => HandleUnknownElement(element, extern_)
                };

            default:
                Log.Warning($"Unimplemented extern {extern_}[{element} (0x{(element):X})]");
                return $"float4(1, 1, 1, 1)";
        }
    }
}
