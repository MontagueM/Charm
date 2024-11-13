using System.Text;
using Arithmic;
using Newtonsoft.Json;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Texture = Tiger.Schema.Texture;

namespace Tiger.Exporters;

public static class Source2Handler
{
    private static void SaveVMDL(string name, string savePath, string fbxPath, List<MeshPart> parts)
    {
        try
        {
            if (!File.Exists($"{savePath}/{name}.vmdl"))
            {
                File.Copy("Exporters/template.vmdl", $"{savePath}/{name}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{name}.vmdl");

                StringBuilder mats = new StringBuilder();

                int i = 0;
                foreach (var part in parts)
                {
                    mats.AppendLine("{");
                    if (part.Material == null)
                    {
                        mats.AppendLine($"    from = \"{name}_Group{part.GroupIndex}_Index{part.Index}_{i}_{part.LodCategory}.vmat\"");
                        mats.AppendLine($"    to = \"materials/black_matte.vmat\"");
                    }
                    else
                    {
                        mats.AppendLine($"    from = \"{part.Material.FileHash}.vmat\"");
                        mats.AppendLine($"    to = \"Shaders/Source2/materials/{part.Material.FileHash}.vmat\"");
                    }
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"{fbxPath}/{name}.fbx");
                text = text.Replace("%MESHNAME%", name);

                File.WriteAllText($"{savePath}/{name}.vmdl", text);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    public static void SaveStaticVMDL(string savePath, string fbxPath, ExporterMesh mesh)
    {
        SaveVMDL(mesh.Hash, savePath, fbxPath, mesh.Parts.Select(x => x.MeshPart).ToList());
    }

    public static void SaveEntityVMDL(string savePath, string fbxPath, ExporterEntity entity)
    {
        SaveVMDL(entity.Mesh.Hash, savePath, fbxPath, entity.Mesh.Parts.Select(x => x.MeshPart).ToList());
    }

    public static void SaveTerrainVMDL(string name, string savePath, List<StaticPart> parts)
    {
        Directory.CreateDirectory($"{savePath}/Models/Terrain/");
        string fbxPath = $"Models/Terrain";
        SaveVMDL(name, $"{savePath}/Models/Terrain", fbxPath, parts.ToList<MeshPart>());
    }

    public static void SaveVMAT(string savePath, string hash, IMaterial material, List<Texture> terrainDyemaps = null)
    {
        string path;
        if (material.EnumerateScopes().Contains(TfxScope.TERRAIN)) // TODO: Fix this shit
            path = $"{savePath}/Shaders/Source2/Materials";
        else
            path = $"{savePath}/Source2/Materials";

        Directory.CreateDirectory(path);
        StringBuilder vmat = new StringBuilder();

        vmat.AppendLine("Layer0\n{");

        //Material parameters
        vmat.AppendLine($"\tshader \"Shaders/Source2/ps_{material.PixelShader.Hash}.shader\"");

        if ((material.EnumerateScopes().Contains(TfxScope.TRANSPARENT) || material.EnumerateScopes().Contains(TfxScope.TRANSPARENT_ADVANCED)) && material.RenderStates.BlendState() == -1)
            vmat.AppendLine($"\tF_ADDITIVE_BLEND 1");

        //Textures
        foreach (var e in material.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"\tPS_TextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
        }

        if (material.UnkD4 != 0) // Vertex animation?
        {
            foreach (var e in material.EnumerateVSTextures())
            {
                if (e.Texture == null)
                    continue;

                vmat.AppendLine($"\tVS_TextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
            }
        }

        vmat.AppendLine(PopulateCBuffers(material).ToString()); // PS
        vmat.AppendLine(PopulateCBuffers(material, true).ToString()); // VS

        //PS Dynamic expressions
        TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(material.PS_TFX_Bytecode));
        var bytecode_hlsl = bytecode.Evaluate(material.PS_TFX_Bytecode_Constants, false, material);

        vmat.AppendLine($"\tDynamicParams\r\n\t{{");
        string temp_time_fix = $"CurTime = exists(CurrentTime) ? CurrentTime : Time;";
        foreach (var entry in bytecode_hlsl)
        {
            var expression = entry.Value.Contains("Time") ? $"{temp_time_fix} return {entry.Value.Replace("Time", "CurTime")};" : entry.Value;
            vmat.AppendLine($"\t\tcb0_{entry.Key} \"{expression}\"");
        }

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

                            vmat.AppendLine($"\t\tPS_TextureT11 \"AtmosFar\"");
                            vmat.AppendLine($"\t\tPS_TextureT13 \"AtmosNear\"");
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
                            vmat.AppendLine($"\t\tcb8_6 \"float4(0.05, 0.05, 0.05, 0.5)\""); // Main Tint? // float4(0.55, 0.41091052, 0.22670946, 0.50381273)
                            vmat.AppendLine($"\t\tcb8_7 \"float4(1.0, 1.0, 1.0, 0.9997778)\""); // Cubemap Reflection Tint?
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
                        vmat.AppendLine($"\t\tcb13_0 \"float4(Time, Time, 0.05, 0.016)\"");
                        vmat.AppendLine($"\t\tcb13_1 \"float4(1,16,0.5,1.5)\"");
                        break;
                }
            }
        }

        if (material.UnkD4 != 0) // Vertex animation?
        {
            //VS Dynamic expressions
            bytecode = new(TfxBytecodeOp.ParseAll(material.VS_TFX_Bytecode));
            bytecode_hlsl = bytecode.Evaluate(material.VS_TFX_Bytecode_Constants, false, material);

            foreach (var entry in bytecode_hlsl)
            {
                var expression = entry.Value.Contains("Time") ? $"{temp_time_fix} return {entry.Value.Replace("Time", "CurTime")};" : entry.Value;
                vmat.AppendLine($"\t\tvs_cb0_{entry.Key} \"{expression}\"");
            }

            foreach (var resource in material.VertexShader.Resources)
            {
                if (resource.ResourceType == Schema.ResourceType.CBuffer)
                {
                    switch (resource.Index)
                    {
                        case 13: // Frame
                            vmat.AppendLine($"\t\tvs_cb13_0 \"float4(Time, Time, 0.05, 0.016)\"");
                            vmat.AppendLine($"\t\tvs_cb13_1 \"float4(1,16,0.5,1.5)\"");
                            break;
                    }
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

    public static void SaveVTEX(Texture tex, string savePath, string vtexPath = "")
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        var file = VTEX.Create(tex, tex.GetDimension(), vtexPath);
        var json = JsonConvert.SerializeObject(file, Formatting.Indented);
        try
        {
            File.WriteAllText($"{savePath}/{tex.Hash}.vtex", json);
        }
        catch
        {
        }
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

            var diff = dye.TagData.Textures[0];
            text = text.Replace($"DiffMap{dyeIndex}", $"{diff.Texture.Hash}.{TextureExtractor.GetExtension(outputTextureFormat)}");
            var norm = dye.TagData.Textures[1];
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

    public static VTEX Create(Texture texture, TextureDimension dimension, string vtexPath = "")
    {
        if (vtexPath != "" && !vtexPath.EndsWith('/'))
            vtexPath = $"{vtexPath}/";
        return new VTEX
        {
            Images = new List<string> { $"textures/{vtexPath}{texture.Hash}.png" },
            OutputFormat = dimension == TextureDimension.CUBE
            ? ImageFormatType.BC6H.ToString() : ImageFormatType.RGBA8888.ToString(),
            OutputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
            InputColorSpace = (texture.IsSrgb() ? GammaType.SRGB : GammaType.Linear).ToString(),
            OutputTypeString = dimension == TextureDimension.CUBE ? "CUBE" : "2D"
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
    RGBA16161616,
    RGBA16161616F,
    BC7,
    BC6H,
}
