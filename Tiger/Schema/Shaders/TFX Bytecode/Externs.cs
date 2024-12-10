﻿using Tiger.Schema;

namespace Tiger;

public enum TfxExtern : byte
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

public static class GlobalChannels
{
    private static Vector4[] Channels = null;

    public static Vector4 Get(int index)
    {
        if (Channels == null)
            Fill();

        return Channels[index];
    }

    public static Vector4[] Fill()
    {
        Channels = new Vector4[256];

        for (int i = 0; i < Channels.Length; i++)
        {
            Channels[i] = Vector4.One;
        }


        Channels[10] = Vector4.One;
        Channels[25] = new Vector4(40.0f);
        Channels[26] = new Vector4(0.90f); // Atmos intensity but a channel?
        Channels[27] = Vector4.One; // specular tint intensity
        Channels[28] = Vector4.One; // specular tint
        Channels[31] = Vector4.One; // diffuse tint 1
        Channels[32] = Vector4.One; // diffuse tint 1 intensity
        Channels[33] = Vector4.One; // diffuse tint 2
        Channels[34] = Vector4.One; // diffuse tint 2 intensity
        Channels[35] = new Vector4(0.55f);
        Channels[37] = new Vector4(50.0f, 0.0f, 0.0f, 0.0f); // Fog start
        Channels[40] = Vector4.Zero;
        Channels[41] = new Vector4(50.0f, 0.0f, 0.0f, 0.0f); // Fog falloff
        Channels[43] = Vector4.Zero;
        Channels[82] = Vector4.Zero;
        Channels[83] = Vector4.Zero;
        Channels[93] = new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
        Channels[97] = Vector4.Zero;
        Channels[98] = Vector4.Zero;
        Channels[100] = new Vector4(0.41105f, 0.71309f, 0.56793f, 0.56793f);
        Channels[113] = Vector4.Zero;
        Channels[127] = Vector4.Zero;
        Channels[131] = new Vector4(0.5f, 0.0f, 0.3f, 0.0f); // Seems related to line lights

        return Channels;
    }
}
