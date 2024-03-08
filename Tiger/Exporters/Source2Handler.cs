using System.Text;
using System.Text.Json;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Texture = Tiger.Schema.Texture;

namespace Tiger.Exporters;

public class SBoxHandler
{
    public static void SaveStaticVMDL(string savePath, ExporterMesh mesh)
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
                text = text.Replace("%FILENAME%", $"Models/Statics/{mesh.Hash}.fbx");
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

    public static void SaveEntityVMDL(string savePath, ExporterEntity entity)
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
                text = text.Replace("%FILENAME%", $"Models/Entities/{entity.Mesh.Hash}.fbx");
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

    public static void SaveVMAT(string savePath, string hash, IMaterial materialHeader, bool isTerrain = false, List<Texture> terrainDyemaps = null)
    {
        string path;
        if (isTerrain)
            path = $"{savePath}/Materials/Terrain";
        else
            path = $"{savePath}/Materials";

        Directory.CreateDirectory(path);
        StringBuilder vmat = new StringBuilder();

        vmat.AppendLine("Layer0\n{");

        //If the shader doesnt exist, just use the default complex.shader
        //if (!File.Exists($"{savePath}/Shaders/PS_{materialHeader.PixelShader?.Hash}.shader"))
        //{
        //    vmat.AppendLine($"  shader \"complex.shader\"");

        //    //Use just the first texture for the diffuse
        //    if (materialHeader.EnumeratePSTextures().Any())
        //    {
        //        if (materialHeader.EnumeratePSTextures().ElementAt(0).Texture is not null)
        //            vmat.AppendLine($"  TextureColor \"Textures/{materialHeader.EnumeratePSTextures().ElementAt(0).Texture.Hash}.png\"");
        //    }
        //}

        //Material parameters
        vmat.AppendLine($"\tshader \"ps_{materialHeader.PixelShader.Hash}.shader\"");
        vmat.AppendLine($"\tF_ALPHA_TEST 1");
        vmat.AppendLine($"\tF_ADDITIVE_BLEND 1");

        if (materialHeader.Unk0C != 0)
            vmat.AppendLine($"\tF_RENDER_BACKFACES 1");

        //Textures
        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"\tTextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
        }

        //vmat.AppendLine(PopulateCBuffers(materialHeader.Decompile(materialHeader.VertexShader.GetBytecode(), $"vs{materialHeader.VertexShader.Hash}"), materialHeader, true).ToString());
        vmat.AppendLine(PopulateCBuffers(materialHeader).ToString());

        //Dynamic expressions
        TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(materialHeader.PS_TFX_Bytecode));
        var bytecode_hlsl = bytecode.Evaluate(materialHeader.PS_TFX_Bytecode_Constants);

        vmat.AppendLine($"\tDynamicParams\r\n\t{{");
        foreach (var entry in bytecode_hlsl)
        {
            vmat.AppendLine($"\t\tcb0_{entry.Key} \"{entry.Value}\"");
        }

        foreach (var resource in materialHeader.PixelShader.Resources)
        {
            if (resource.ResourceType == Schema.ResourceType.CBuffer)
            {
                switch (resource.Index)
                {
                    case 2: //Transparent scope
                        for (int i = 0; i < resource.Count; i++)
                        {
                            vmat.AppendLine($"\t\tcb2_{i} \"float4(0,1,1,1)\"");
                        }
                        break;
                    case 8: //??? scope
                        for (int i = 0; i < resource.Count; i++)
                        {
                            if (i < 5)
                                vmat.AppendLine($"\t\tcb8_{i} \"float4(0,0,0,0)\"");
                            else
                                vmat.AppendLine($"\t\tcb8_{i} \"float4(1,1,1,1)\"");
                        }
                        break;
                    case 13: //Frame scope
                        vmat.AppendLine($"\t\tcb13_0 \"Time\"");
                        vmat.AppendLine($"\t\tcb13_1 \"float4(0.25,1,1,1)\"");
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

    public static void SaveDecalVMAT(string savePath, string hash, IMaterial materialHeader) //Testing
    {
        StringBuilder vmat = new StringBuilder();
        vmat.AppendLine("Layer0 \n{");

        vmat.AppendLine($"  shader \"projected_decals.shader\"");

        //Use just the first texture for the diffuse
        if (materialHeader.EnumeratePSTextures().Any())
        {
            if (materialHeader.EnumeratePSTextures().ElementAt(0).Texture is not null)
                vmat.AppendLine($"  TextureColor \"Textures/{materialHeader.EnumeratePSTextures().ElementAt(0).Texture.Hash}.png\"");
        }

        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"  TextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
        }

        vmat.AppendLine("}");

        try
        {
            Directory.CreateDirectory($"{savePath}/materials/");
            File.WriteAllText($"{savePath}/materials/{hash}_decal.vmat", vmat.ToString());
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
                data = materialHeader.GetVec4Container(materialHeader.VSVector4Container.GetReferenceHash());
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
                data = materialHeader.GetVec4Container(materialHeader.PSVector4Container.GetReferenceHash());
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

        var file = TextureFile.CreateDefault(tex, tex.IsCubemap() ? ImageDimension.CUBEARRAY : ImageDimension._2D);
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
