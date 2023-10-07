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
        if (!File.Exists($"{savePath}/{entity.Hash}.vmdl"))
        {
            File.Copy("Exporters/template.vmdl", $"{savePath}/{entity.Hash}.vmdl", true);
            string text = File.ReadAllText($"{savePath}/{entity.Hash}.vmdl");

            StringBuilder mats = new StringBuilder();

            int i = 0;
            foreach (var part in entity.Load(ExportDetailLevel.MostDetailed))
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
            text = text.Replace("%FILENAME%", $"models/{entity.Hash}.fbx");
            text = text.Replace("%MESHNAME%", entity.Hash);

            File.WriteAllText($"{savePath}/{entity.Hash}.vmdl", text);
        }
    }

    public static void SaveTerrainVMDL(string savePath, string hash, List<MeshPart> parts, STerrain terrainHeader)
    {
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
        }

        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
            {
                continue;
            }

            vmat.AppendLine($"\tTextureT{e.TextureIndex} \"materials/Textures/{e.Texture.Hash}.png\"");
        }

        List<Cbuffer> cbuffers = new List<Cbuffer>();
        StringBuilder buffers = new StringBuilder();
        StringReader hlsl = new StringReader(materialHeader.Decompile(materialHeader.PixelShader.GetBytecode(), $"ps{materialHeader.FileHash}"));

        string line = string.Empty;
        do
        {
            line = hlsl.ReadLine();
            if (line != null)
            {
                if (line.Contains("cbuffer"))
                {
                    hlsl.ReadLine();
                    line = hlsl.ReadLine();
                    Cbuffer cbuffer = new Cbuffer();
                    cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    cbuffers.Add(cbuffer);
                }
            }

        } while (line != null);

        foreach (var cbuffer in cbuffers)
        {
            dynamic data = null;

            if (cbuffer.Count == materialHeader.Unk2E0.Count)
            {
                data = materialHeader.Unk2E0;
            }
            else if (cbuffer.Count == materialHeader.Unk300.Count)
            {
                data = materialHeader.Unk300;
            }
            else
            {
                if (materialHeader.PSVector4Container.IsValid())
                {
                    // Try the Vector4 storage file
                    TigerFile container = new(materialHeader.PSVector4Container.GetReferenceHash());
                    byte[] containerData = container.GetData();
                    int num = containerData.Length / 16;
                    if (cbuffer.Count == num)
                    {
                        List<Vector4> float4s = new();
                        for (int i = 0; i < containerData.Length / 16; i++)
                        {
                            float4s.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
                        }

                        data = float4s;
                    }
                }
            }

            for (int i = 0; i < cbuffer.Count; i++)
            {
                if (data == null)
                    buffers.AppendLine($"\tcb{cbuffer.Index}_{i} \"[0.000 0.000 0.000 0.000]\"");
                else
                {
                    try
                    {
                        if (data[i] is Vec4)
                        {
                            buffers.AppendLine($"\tcb{cbuffer.Index}_{i} \"[{data[i].Vec.X} {data[i].Vec.Y} {data[i].Vec.Z} {data[i].Vec.W}]\"");
                        }
                        else if (data[i] is Vector4)
                        {
                            buffers.AppendLine($"\tcb{cbuffer.Index}_{i} \"[{data[i].X} {data[i].Y} {data[i].Z} {data[i].W}]\"");
                        }
                    }
                    catch (Exception e)
                    {
                        buffers.AppendLine($"\tcb{cbuffer.Index}_{i} \"[0.000 0.000 0.000 0.000]\"");
                    }
                }
            }
        }

        vmat.AppendLine(buffers.ToString());
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
}
