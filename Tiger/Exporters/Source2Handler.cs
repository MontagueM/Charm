using System.Text;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

namespace Tiger.Exporters;

public class Source2Handler
{
    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    public static bool source2Shaders = _config.GetS2ShaderExportEnabled();
    public static bool source2Models = _config.GetS2VMDLExportEnabled();
    public static bool source2Materials = _config.GetS2VMATExportEnabled();

    public static void SaveStaticVMDL(string savePath, string staticMeshName, List<StaticPart> staticMesh)
    {
        try
        {
            if (!File.Exists($"{savePath}/{staticMeshName}.vmdl"))
            {
                //Source 2 shit
                File.Copy("Exporters/template.vmdl", $"{savePath}/{staticMeshName}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{staticMeshName}.vmdl");

                StringBuilder mats = new StringBuilder();

                int i = 0;
                foreach (MeshPart staticpart in staticMesh)
                {
                    if (staticpart.Material == null)
                        continue;
                    mats.AppendLine("{");
                    mats.AppendLine($"    from = \"{staticpart.Material.FileHash}.vmat\"");
                    mats.AppendLine($"    to = \"materials/{staticpart.Material.FileHash}.vmat\"");
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"models/{staticMeshName}.fbx");
                text = text.Replace("%MESHNAME%", staticMeshName);

                File.WriteAllText($"{savePath}/{staticMeshName}.vmdl", text);
            }
        }
        catch (Exception ex)
        {
        }
    }

    public static void SaveEntityVMDL(string savePath, Entity entity)
    {
        var parts = entity.Load(ExportDetailLevel.MostDetailed);
        SaveEntityVMDL(savePath, entity.Hash, parts);
    }

    public static void SaveEntityVMDL(string savePath, string hash, List<DynamicMeshPart> parts)
    {
        try
        {
            if (!File.Exists($"{savePath}/{hash}.vmdl"))
            {
                File.Copy("Exporters/template.vmdl", $"{savePath}/{hash}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{hash}.vmdl");

                StringBuilder mats = new StringBuilder();

                int i = 0;
                foreach (var part in parts)
                {
                    if (part.Material == null)
                        continue;

                    if (!part.Material.EnumeratePSTextures().Any())
                        continue;

                    mats.AppendLine("{");
                    mats.AppendLine($"    from = \"{part.Material.FileHash}.vmat\"");
                    mats.AppendLine($"    to = \"materials/{part.Material.FileHash}.vmat\"");
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"models/{hash}.fbx");
                text = text.Replace("%MESHNAME%", hash);

                File.WriteAllText($"{savePath}/{hash}.vmdl", text);
            }
        }
        catch(Exception e)
        {

        }
    }

    public static void SaveTerrainVMDL(string savePath, string hash, List<StaticPart> parts, STerrain terrainHeader)
    {
        Directory.CreateDirectory($"{savePath}/Statics/");
        File.Copy("Exporters/template.vmdl", $"{savePath}/Statics/{hash}_Terrain.vmdl", true);
        if (File.Exists($"{savePath}/Statics/{hash}_Terrain.vmdl"))
        {
            string text = File.ReadAllText($"{savePath}/Statics/{hash}_Terrain.vmdl");

            StringBuilder mats = new StringBuilder();

            int i = 0;
            foreach (var staticpart in parts)
            {
                if (terrainHeader.MeshGroups[staticpart.GroupIndex].Dyemap != null)
                {
                    mats.AppendLine("{");
                    mats.AppendLine($"    from = \"{staticpart.Material.FileHash}.vmat\"");
                    mats.AppendLine($"    to = \"materials/Terrain/{hash}_{staticpart.Material.FileHash}.vmat\"");
                    mats.AppendLine("},\n");
                    i++;
                }
            }

            text = text.Replace("%MATERIALS%", mats.ToString());
            text = text.Replace("%FILENAME%", $"models/{hash}_Terrain.fbx");
            text = text.Replace("%MESHNAME%", hash);

            File.WriteAllText($"{savePath}/Statics/{hash}_Terrain.vmdl", text);
        }
    }

    public static void SaveVMAT(string savePath, string hash, IMaterial materialHeader, bool isTerrain = false)
    {
        StringBuilder vmat = new StringBuilder();
        vmat.AppendLine("Layer0 \n{");

        //If the shader doesnt exist, just use the default complex.shader
        if (!File.Exists($"{savePath}/Source2/PS_{materialHeader.PixelShader?.Hash}.shader"))
        {
            vmat.AppendLine($"  shader \"complex.shader\"");

            //Use just the first texture for the diffuse
            if (materialHeader.EnumeratePSTextures().Any())
            {
                if (materialHeader.EnumeratePSTextures().ElementAt(0).Texture is not null)
                    vmat.AppendLine($"  TextureColor \"materials/Textures/{materialHeader.EnumeratePSTextures().ElementAt(0).Texture.Hash}.png\"");
            }
        }
        else
        {
            vmat.AppendLine($"\tshader \"ps_{materialHeader.PixelShader.Hash}.shader\"");
            vmat.AppendLine($"\tF_ALPHA_TEST 1");
            vmat.AppendLine($"\tF_ADDITIVE_BLEND 1");

            if(materialHeader.Unk0C != 0)
            {
                vmat.AppendLine($"\tF_RENDER_BACKFACES 1");
            }

            TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(materialHeader.PS_TFX_Bytecode));
            var bytecode_hlsl = bytecode.Evaluate(materialHeader.PS_TFX_Bytecode_Constants);

            if (bytecode_hlsl.Count > 0)
            {
                vmat.AppendLine($"\tDynamicParams\r\n\t{{");
                foreach (var entry in bytecode_hlsl)
                {
                    vmat.AppendLine($"\t\tcb0_{entry.Key} \"{entry.Value}\"");
                }
                vmat.AppendLine($"\t}}");
            }
        }

        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"\tTextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
        }

        //vmat.AppendLine(PopulateCBuffers(materialHeader.Decompile(materialHeader.VertexShader.GetBytecode(), $"vs{materialHeader.VertexShader.Hash}"), materialHeader, true).ToString());
        vmat.AppendLine(PopulateCBuffers(materialHeader.Decompile(materialHeader.PixelShader.GetBytecode(), $"ps{materialHeader.PixelShader.Hash}"), materialHeader).ToString());
        vmat.AppendLine("}");

        string terrainDir = isTerrain ? "/Terrain/" : "";
        if (isTerrain)
            Directory.CreateDirectory($"{savePath}/Source2/materials/{terrainDir}");

        if (!File.Exists($"{savePath}/Source2/materials/{terrainDir}{hash}.vmat"))
        {
            try
            {
                File.WriteAllText($"{savePath}/Source2/materials/{terrainDir}{hash}.vmat", vmat.ToString());
            }
            catch (IOException)
            {
            }
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
                vmat.AppendLine($"  TextureColor \"materials/Textures/{materialHeader.EnumeratePSTextures().ElementAt(0).Texture.Hash}.png\"");
        }

        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
            {
                continue;
            }

            vmat.AppendLine($"  TextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
        }

        vmat.AppendLine("}");

        if (!File.Exists($"{savePath}/Source2/materials/{hash}_decal.vmat"))
        {
            try
            {
                Directory.CreateDirectory($"{savePath}/Source2/materials/");
                File.WriteAllText($"{savePath}/Source2/materials/{hash}_decal.vmat", vmat.ToString());
            }
            catch (IOException)
            {
            }
        }
    }

    public static StringBuilder PopulateCBuffers(string hlsl, IMaterial materialHeader, bool isVertexShader = false)
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
}
