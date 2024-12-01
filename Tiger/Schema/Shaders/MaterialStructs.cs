namespace Tiger.Schema;

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "D71A8080", 0x488)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "E8718080", 0x400)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "AA6D8080", 0x3B0)]
[SchemaStruct(TigerStrategy.DESTINY2_WITCHQUEEN_6307, "AA6D8080", 0x3D0)]
public struct SMaterial // Errm Ackchyually its called "technique" 🤓
{
    public long FileSize;
    public uint Unk08;
    public uint Unk0C;
    public uint Unk10;

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    public ScopeBitsD1 UsedScopesD1;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x18, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public ScopeBitsSK UsedScopesSK;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public ScopeBitsBL UsedScopesBL;

    //public ScopeBitsD1 CompatibleScopesD1; // Not really important, but they are there after each UsedScopes
    //public ScopeBitsSK CompatibleScopesSK;
    //public ScopeBitsBL CompatibleScopesBL;

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public StateSelection RenderStates;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x58, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicStruct<SMaterialShader> Vertex;

    [SchemaField(0x2A8, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x2C8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x298, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x2B0, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicStruct<SMaterialShader> Pixel;

    [SchemaField(0x348, TigerStrategy.DESTINY1_RISE_OF_IRON)] // Unsure, everything else has 6 shader stages, D1 has 7? (Doesnt matter anyways)
    [SchemaField(0x368, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x328, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    [SchemaField(0x340, TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
    public DynamicStruct<SMaterialShader> Compute;

    public dynamic GetScopeBits()
    {
        if (Strategy.IsD1())
            return UsedScopesD1;
        else if (Strategy.IsPreBL())
            return UsedScopesSK;
        else
            return UsedScopesBL;
    }
}

[NonSchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0xA0)]
[NonSchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x90)]
public struct SMaterialShader
{
    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    public ShaderBytecode Shader;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<STextureTag> Textures;

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    public DynamicArray<D2Class_09008080> TFX_Bytecode;
    public DynamicArray<Vec4> TFX_Bytecode_Constants;
    public DynamicArray<SDirectXSamplerTag> Samplers;
    public DynamicArray<Vec4> CBuffers; // Fallback if Vector4Container doesn't exist, I guess..?

    [SchemaField(0x7C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x74, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x64, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public int Unk64;

    [SchemaField(0x80, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x70, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public int BufferSlot;
    public FileHash Vector4Container;

    public IEnumerable<STextureTag> EnumerateTextures()
    {
        foreach (STextureTag texture in Textures)
        {
            yield return texture;
        }
    }

    public IEnumerable<DirectXSampler> EnumerateSamplers()
    {
        foreach (SDirectXSamplerTag sampler in Samplers)
        {
            yield return sampler.GetSampler();
        }
    }

    public List<Vector4> GetCBuffer0()
    {
        List<Vector4> data = new();
        if (Vector4Container.IsValid())
        {
            data = GetVec4Container();
        }
        else
        {
            foreach (var vec in CBuffers)
            {
                data.Add(vec.Vec);
            }
        }
        return data;
    }

    public List<Vector4> GetVec4Container()
    {
        List<Vector4> data = new();
        TigerFile container = new(Vector4Container.GetReferenceHash());
        byte[] containerData = container.GetData();

        for (int i = 0; i < containerData.Length / 16; i++)
        {
            data.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
        }

        return data;
    }

    public TfxBytecodeInterpreter GetBytecode()
    {
        return new TfxBytecodeInterpreter(TfxBytecodeOp.ParseAll(TFX_Bytecode));
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "281B8080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "11728080", 0x8)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "CF6D8080", 0x18)]
public struct STextureTag
{
    public uint TextureIndex;
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public Texture TextureSK;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402), Tag64]
    public Texture TextureBL;

    public Texture GetTexture()
    {
        if (Strategy.IsPreBL() || Strategy.IsD1())
            return TextureSK;
        else
            return TextureBL;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, "CC1A8080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, "F3738080", 0x10)]
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, "3F018080", 0x10)]
public struct SDirectXSamplerTag
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Obsolete = true)]
    public DirectXSampler SamplerSK;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402), Tag64]
    public DirectXSampler SamplerBL;

    public DirectXSampler GetSampler()
    {
        if (Strategy.IsD1())
            return null;
        else if (Strategy.IsPreBL())
            return SamplerSK;
        else
            return SamplerBL;
    }
}


[SchemaStruct("09008080", 1)]
public struct D2Class_09008080
{
    public byte Value;
}

[SchemaStruct("90008080", 0x10)]
public struct Vec4
{
    public Vector4 Vec;
}

[Flags]
public enum ScopeBitsBL : ulong
{
    FRAME = 1UL << 0,
    VIEW = 1UL << 1,
    RIGID_MODEL = 1UL << 2,
    EDITOR_MESH = 1UL << 3,
    EDITOR_TERRAIN = 1UL << 4,
    CUI_VIEW = 1UL << 5,
    CUI_OBJECT = 1UL << 6,
    SKINNING = 1UL << 7,
    SPEEDTREE = 1UL << 8,
    CHUNK_MODEL = 1UL << 9,
    DECAL = 1UL << 10,
    INSTANCES = 1UL << 11,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1UL << 12,
    TRANSPARENT = 1UL << 13,
    TRANSPARENT_ADVANCED = 1UL << 14,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1UL << 15,
    TERRAIN = 1UL << 16,
    POSTPROCESS = 1UL << 17,
    CUI_BITMAP = 1UL << 18,
    CUI_STANDARD = 1UL << 19,
    UI_FONT = 1UL << 20,
    CUI_HUD = 1UL << 21,
    PARTICLE_TRANSFORMS = 1UL << 22,
    PARTICLE_LOCATION_METADATA = 1UL << 23,
    CUBEMAP_VOLUME = 1UL << 24,
    GEAR_PLATED_TEXTURES = 1UL << 25,
    GEAR_DYE_0 = 1UL << 26,
    GEAR_DYE_1 = 1UL << 27,
    GEAR_DYE_2 = 1UL << 28,
    GEAR_DYE_DECAL = 1UL << 29,
    GENERIC_ARRAY = 1UL << 30,
    GEAR_DYE_SKIN = 1UL << 31,
    GEAR_DYE_LIPS = 1UL << 32,
    GEAR_DYE_HAIR = 1UL << 33,
    GEAR_DYE_FACIAL_LAYER_0_MASK = 1UL << 34,
    GEAR_DYE_FACIAL_LAYER_0_MATERIAL = 1UL << 35,
    GEAR_DYE_FACIAL_LAYER_1_MASK = 1UL << 36,
    GEAR_DYE_FACIAL_LAYER_1_MATERIAL = 1UL << 37,
    PLAYER_CENTERED_CASCADED_GRID = 1UL << 38,
    GEAR_DYE_012 = 1UL << 39,
    COLOR_GRADING_UBERSHADER = 1UL << 40,
}

[Flags]
public enum ScopeBitsSK : uint
{
    FRAME = 1U << 0,
    VIEW = 1U << 1,
    RIGID_MODEL = 1U << 2,
    EDITOR_MESH = 1U << 3,
    EDITOR_TERRAIN = 1U << 4,
    CUI_VIEW = 1U << 5,
    CUI_OBJECT = 1U << 6,
    SKINNING = 1U << 7,
    SPEEDTREE = 1U << 8,
    CHUNK_MODEL = 1U << 9,
    DECAL = 1U << 10,
    INSTANCES = 1U << 11,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1U << 12,
    TRANSPARENT = 1U << 13,
    TRANSPARENT_ADVANCED = 1U << 14,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1U << 15,
    TERRAIN = 1U << 16,
    POSTPROCESS = 1U << 17,
    CUI_BITMAP = 1U << 18,
    CUI_STANDARD = 1U << 19,
    UI_FONT = 1U << 20,
    CUI_HUD = 1U << 21,
    PARTICLE_TRANSFORMS = 1U << 22,
    PARTICLE_LOCATION_METADATA = 1U << 23,
    CUBEMAP_VOLUME = 1U << 24,
    GEAR_PLATED_TEXTURES = 1U << 25,
    GEAR_DYE_0 = 1U << 26,
    GEAR_DYE_1 = 1U << 27,
    GEAR_DYE_2 = 1U << 28,
    GEAR_DYE_DECAL = 1U << 29,
    GENERIC_ARRAY = 1U << 30,
    WEATHER = 1U << 31
}

[Flags]
public enum ScopeBitsD1 : uint
{
    FRAME = 1U << 0,
    VIEW = 1U << 1,
    RIGID_MODEL = 1U << 2,
    EDITOR_MESH = 1U << 3,
    EDITOR_TERRAIN = 1U << 4,
    CUI_VIEW = 1U << 5,
    CUI_OBJECT = 1U << 6,
    SKINNING = 1U << 7,
    SPEEDTREE = 1U << 8,
    CHUNK_MODEL = 1U << 9,
    DECAL = 1U << 10,
    INSTANCES = 1U << 11,
    SPEEDTREE_INSTANCE_DATA = 1U << 12,
    SPEEDTREE_LOD_DRAWCALL_DATA = 1U << 13,
    TRANSPARENT = 1U << 14,
    TRANSPARENT_ADVANCED = 1U << 15,
    SDSM_BIAS_AND_SCALE_TEXTURES = 1U << 16,
    TERRAIN = 1U << 17,
    POSTPROCESS = 1U << 18,
    CUI_BITMAP = 1U << 19,
    CUI_STANDARD = 1U << 20,
    UI_FONT = 1U << 21,
    CUI_HUD = 1U << 22,
}
