using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Texture = Tiger.Schema.Texture;

namespace Tiger.Exporters;

public class Source2Handler
{
    public static void SaveStaticVMDL(string savePath, string fbxPath, ExporterMesh mesh)
    {
        try
        {
            if (!File.Exists($"{savePath}/{mesh.Hash}.vmdl"))
            {
                File.Copy("Exporters/template.vmdl", $"{savePath}/{mesh.Hash}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{mesh.Hash}.vmdl");

                StringBuilder mats = new StringBuilder();

                int i = 0;
                foreach (var part in mesh.Parts)
                {
                    mats.AppendLine("{");
                    if (part.Material == null)
                    {
                        mats.AppendLine($"    from = \"{mesh.Hash}_Group{part.MeshPart.GroupIndex}_Index{part.Index}_{i}_{part.MeshPart.LodCategory}.vmat\"");
                        mats.AppendLine($"    to = \"materials/black_matte.vmat\"");
                    }
                    else
                    {
                        mats.AppendLine($"    from = \"{part.Material.FileHash}.vmat\"");
                        mats.AppendLine($"    to = \"materials/{part.Material.FileHash}.vmat\"");
                    }
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"{fbxPath}/{mesh.Hash}.fbx");
                text = text.Replace("%MESHNAME%", mesh.Hash);

                File.WriteAllText($"{savePath}/{mesh.Hash}.vmdl", text);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    //public static void SaveEntityVMDL(string savePath, Entity entity)
    //{
    //    var parts = entity.Load(ExportDetailLevel.MostDetailed);
    //    SaveEntityVMDL(savePath, entity.Hash, parts);
    //}

    public static void SaveEntityVMDL(string savePath, string fbxPath, ExporterEntity entity)
    {
        try
        {
            if (!File.Exists($"{savePath}/{entity.Mesh.Hash}.vmdl"))
            {
                File.Copy("Exporters/template.vmdl", $"{savePath}/{entity.Mesh.Hash}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{entity.Mesh.Hash}.vmdl");

                StringBuilder mats = new StringBuilder();

                int i = 0;
                foreach (var part in entity.Mesh.Parts)
                {
                    mats.AppendLine("{");
                    if (part.Material == null)
                    {
                        mats.AppendLine($"    from = \"{entity.Mesh.Hash}_Group{part.MeshPart.GroupIndex}_Index{part.MeshPart.Index}_{i}_{part.MeshPart.LodCategory}.vmat\"");
                        mats.AppendLine($"    to = \"materials/black_matte.vmat\"");
                    }
                    else
                    {
                        mats.AppendLine($"    from = \"{part.Material.FileHash}.vmat\"");
                        mats.AppendLine($"    to = \"materials/{part.Material.FileHash}.vmat\"");
                    }
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"{fbxPath}/{entity.Mesh.Hash}.fbx");
                text = text.Replace("%MESHNAME%", entity.Mesh.Hash);

                File.WriteAllText($"{savePath}/{entity.Mesh.Hash}.vmdl", text);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    public static void SaveTerrainVMDL(string name, string savePath, List<StaticPart> parts)
    {
        Directory.CreateDirectory($"{savePath}/Models/Terrain/");
        File.Copy("Exporters/template.vmdl", $"{savePath}/Models/Terrain/{name}.vmdl", true);
        if (File.Exists($"{savePath}/Models/Terrain/{name}.vmdl"))
        {
            string text = File.ReadAllText($"{savePath}/Models/Terrain/{name}.vmdl");

            StringBuilder mats = new StringBuilder();

            int i = 0;
            foreach (var staticpart in parts)
            {
                mats.AppendLine("{");
                mats.AppendLine($"    from = \"{staticpart.Material.FileHash}.vmat\"");
                mats.AppendLine($"    to = \"materials/Terrain/{staticpart.Material.FileHash}.vmat\"");
                mats.AppendLine("},\n");
                i++;
            }

            text = text.Replace("%MATERIALS%", mats.ToString());
            text = text.Replace("%FILENAME%", $"Models/Terrain/{name}.fbx");
            text = text.Replace("%MESHNAME%", name);

            File.WriteAllText($"{savePath}/Models/Terrain/{name}.vmdl", text);
        }
    }

    public static void SaveVMAT(string savePath, string hash, IMaterial material, List<Texture> terrainDyemaps = null)
    {
        string path;
        if (material.EnumerateScopes().Contains(TfxScope.TERRAIN))
            path = $"{savePath}/Source2/Materials/Terrain";
        else
            path = $"{savePath}/Source2/Materials";

        Directory.CreateDirectory(path);
        StringBuilder vmat = new StringBuilder();

        vmat.AppendLine("Layer0\n{");

        //Material parameters
        vmat.AppendLine($"\tshader \"shaders/ps_{material.PixelShader.Hash}.shader\"");

        if ((material.EnumerateScopes().Contains(TfxScope.TRANSPARENT) || material.EnumerateScopes().Contains(TfxScope.TRANSPARENT_ADVANCED)) && material.RenderStates.BlendState() == -1)
            vmat.AppendLine($"\tF_ADDITIVE_BLEND 1");

        if (material.Unk0C != 0 && (material.EnumerateScopes().Contains(TfxScope.TRANSPARENT) || material.EnumerateScopes().Contains(TfxScope.TRANSPARENT_ADVANCED)))
            vmat.AppendLine($"\tF_RENDER_BACKFACES 1");

        //Textures
        foreach (var e in material.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"\tTextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
        }

        vmat.AppendLine(PopulateCBuffers(material).ToString()); // PS
        //vmat.AppendLine(PopulateCBuffers(material, true).ToString()); // VS

        //PS Dynamic expressions
        TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(material.PS_TFX_Bytecode));
        var bytecode_hlsl = bytecode.Evaluate(material.PS_TFX_Bytecode_Constants);

        vmat.AppendLine($"\tDynamicParams\r\n\t{{");
        foreach (var entry in bytecode_hlsl)
        {
            vmat.AppendLine($"\t\tcb0_{entry.Key} \"{entry.Value}\"");
        }

        //VS Dynamic expressions
        //bytecode = new(TfxBytecodeOp.ParseAll(material.VS_TFX_Bytecode));
        //bytecode_hlsl = bytecode.Evaluate(material.VS_TFX_Bytecode_Constants);

        //foreach (var entry in bytecode_hlsl)
        //{
        //    vmat.AppendLine($"\t\tvs_cb0_{entry.Key} \"{entry.Value}\"");
        //}

        foreach (var resource in material.PixelShader.Resources)
        {
            if (resource.ResourceType == Schema.ResourceType.CBuffer)
            {
                switch (resource.Index)
                {
                    case 2: // Transparent
                        if (material.EnumerateScopes().Contains(TfxScope.TRANSPARENT))
                        {
                            for (int i = 0; i < resource.Count; i++)
                            {
                                if (i == 0)
                                    vmat.AppendLine($"\t\tcb2_{i} \"float4(0,100,0,0)\"");
                                else
                                    vmat.AppendLine($"\t\tcb2_{i} \"float4(1,1,1,1)\"");
                            }
                        }
                        break;
                    case 8: // Transparent_Advanced
                        if (material.EnumerateScopes().Contains(TfxScope.TRANSPARENT_ADVANCED))
                        {
                            vmat.AppendLine($"\t\tcb8_0 \"float4(0.0009849314,0.0019836868,0.0007783567,0.0015586712)\"");
                            vmat.AppendLine($"\t\tcb8_1 \"float4(0.00098604,0.002085914,0.0009838239,0.0018864698)\"");
                            vmat.AppendLine($"\t\tcb8_2 \"float4(0.0011860824,0.0024346288,0.0009468408,0.001850187)\"");
                            vmat.AppendLine($"\t\tcb8_3 \"float4(0.7903466, 0.7319064, 0.56213695, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_4 \"float4(0.0, 1.0, 0.109375, 0.046875)\"");
                            vmat.AppendLine($"\t\tcb8_5 \"float4(0.0, 0.0, 0.0, 0.00086945295)\"");
                            vmat.AppendLine($"\t\tcb8_6 \"float4(0.55, 0.41091052, 0.22670946, 0.50381273)\"");
                            vmat.AppendLine($"\t\tcb8_7 \"float4(1.0, 1.0, 1.0, 0.9997778)\"");
                            vmat.AppendLine($"\t\tcb8_8 \"float4(132.92885, 66.40444, 56.853416, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_9 \"float4(132.92885, 66.40444, 1000.0, 0.0001)\"");
                            vmat.AppendLine($"\t\tcb8_10 \"float4(131.92885, 65.40444, 55.853416, 0.6784314)\"");
                            vmat.AppendLine($"\t\tcb8_11 \"float4(131.92885, 65.40444, 999.0, 5.5)\"");
                            vmat.AppendLine($"\t\tcb8_12 \"float4(0.0, 0.5, 25.575994, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_13 \"float4(0.0, 0.0, 0.0, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_14 \"float4(0.025, 10000.0, -9999.0, 1.0)\"");
                            vmat.AppendLine($"\t\tcb8_15 \"float4(1.0, 1.0, 1.0, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_16 \"float4(0.0, 0.0, 0.0, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_17 \"float4(10.979255, 7.1482353, 6.3034935, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_18 \"float4(0.0037614072, 0.0, 0.0, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_19 \"float4(0.0, 0.0075296126, 0.0, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_20 \"float4(0.0, 0.0, 0.017589089, 0.0)\"");
                            vmat.AppendLine($"\t\tcb8_21 \"float4(0.27266484, -0.31473818, -0.15603681, 1.0)\"");
                            vmat.AppendLine($"\t\tcb8_36 \"float4(1.0, 0.0, 0.0, 0.0)\"");

                            //for (int i = 0; i < resource.Count; i++)
                            //{
                            //    vmat.AppendLine($"\t\tcb8_{i} \"float4(0,0,0,0)\"");
                            //}
                        }
                        break;
                    case 13: // Frame
                        vmat.AppendLine($"\t\tcb13_0 \"float4(Time, Time, 0.05, 1)\"");
                        vmat.AppendLine($"\t\tcb13_1 \"float4(0.4,8,1,1)\"");
                        break;
                }
            }
        }

        vmat.AppendLine($"\t}}");
        vmat.AppendLine("}");

        if (terrainDyemaps is not null)
            foreach (var tex in terrainDyemaps)
            {
                SaveVTEX(tex, $"{savePath}/Textures");
            }

        try
        {
            File.WriteAllText($"{path}/{hash}.vmat", vmat.ToString());
        }
        catch (IOException)
        {
        }
    }

    public static StringBuilder PopulateCBuffers(IMaterial materialHeader, bool isVertexShader = false)
    {
        StringBuilder cbuffers = new();

        List<Vector4> data = new();
        string cbType = isVertexShader ? "vs_cb0" : "cb0";

        if (isVertexShader)
        {
            if (materialHeader.VSVector4Container.IsValid())
            {
                data = materialHeader.GetVec4Container(true);
            }
            else
            {
                foreach (var vec in materialHeader.VS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
            }
        }
        else
        {
            if (materialHeader.PSVector4Container.IsValid())
            {
                data = materialHeader.GetVec4Container();
            }
            else
            {
                foreach (var vec in materialHeader.PS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
            }
        }

        for (int i = 0; i < data.Count; i++)
        {
            cbuffers.AppendLine($"\t\"{cbType}_{i}\" \"[{data[i].X} {data[i].Y} {data[i].Z} {data[i].W}]\"");
        }

        return cbuffers;
    }

    public static void SaveVTEX(Texture tex, string savePath)
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        var file = VTEX.Create(tex, tex.IsCubemap() ? ImageDimension.CUBEARRAY : ImageDimension._2D);
        var json = JsonSerializer.Serialize(file, JsonSerializerOptions.Default);
        File.WriteAllText($"{savePath}/{tex.Hash}.vtex", json);
    }

    public static void SaveGearVMAT(string saveDirectory, string meshName, TextureExportFormat outputTextureFormat, List<Dye> dyes, string fileSuffix = "")
    {
        File.Copy($"Exporters/template.vmat", $"{saveDirectory}/{meshName}{fileSuffix}.vmat", true);
        string text = File.ReadAllText($"{saveDirectory}/{meshName}{fileSuffix}.vmat");

        string[] components = { "X", "Y", "Z", "W" };

        int dyeIndex = 1;
        foreach (var dye in dyes)
        {
            var dyeInfo = dye.GetDyeInfo();
            foreach (var fieldInfo in dyeInfo.GetType().GetFields())
            {
                Vector4 value = (Vector4)fieldInfo.GetValue(dyeInfo);
                if (!fieldInfo.CustomAttributes.Any())
                    continue;
                string valueName = fieldInfo.CustomAttributes.First().ConstructorArguments[0].Value.ToString();
                for (int i = 0; i < 4; i++)
                {
                    text = text.Replace($"{valueName}{dyeIndex}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                }
            }

            var diff = dye.TagData.DyeTextures[0];
            text = text.Replace($"DiffMap{dyeIndex}", $"{diff.Texture.Hash}.{TextureExtractor.GetExtension(outputTextureFormat)}");
            var norm = dye.TagData.DyeTextures[1];
            text = text.Replace($"NormMap{dyeIndex}", $"{norm.Texture.Hash}.{TextureExtractor.GetExtension(outputTextureFormat)}");
            dyeIndex++;
        }

        text = text.Replace("OUTPUTPATH", $"Textures");
        text = text.Replace("SHADERNAMEENUM", $"{meshName}{fileSuffix}");
        File.WriteAllText($"{saveDirectory}/{meshName}{fileSuffix}.vmat", text);
    }
}

public class VTEX
{
    public List<string> Images { get; set; }

    public string InputColorSpace { get; set; }

    public string OutputFormat { get; set; }

    public string OutputColorSpace { get; set; }

    public string OutputTypeString { get; set; }

    public static VTEX Create(Texture texture, ImageDimension dimension)
    {
        return new VTEX
        {
            Images = new List<string> { $"textures/{texture.Hash}.png" },
            OutputFormat = ImageFormatType.RGBA8888.ToString(),
            OutputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
            InputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
            OutputTypeString = EnumExtensions.GetEnumDescription(dimension)
        };
    }
}

public enum GammaType
{
    Linear,
    SRGB,
}

public enum ImageFormatType
{
    DXT5,
    DXT1,
    RGBA8888,
    BC7,
    BC6H,
}

public enum ImageDimension
{
    [Description("1D")]
    _1D,
    [Description("2D")]
    _2D,
    [Description("3D")]
    _3D,
    [Description("1DArray")]
    _1DARRAY,
    [Description("2DArray")]
    _2DARRAY,
    [Description("3DArray")]
    _3DARRAY,
    [Description("CUBE")]
    CUBE,
    [Description("CUBEARRAY")]
    CUBEARRAY
}
