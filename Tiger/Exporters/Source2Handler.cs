using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Arithmic;
using Tiger.Schema;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;

using Pfim;
using Pfim.dds;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;

using ValveTexture = ValveResourceFormat.ResourceTypes.Texture;
using Texture = Tiger.Schema.Texture;

namespace Tiger.Exporters;

public class SBoxHandler
{
    private static ConfigSubsystem _config = CharmInstance.GetSubsystem<ConfigSubsystem>();
    public static bool sboxShaders = _config.GetSBoxShaderExportEnabled();
    public static bool sboxModels = _config.GetSBoxModelExportEnabled();
    public static bool sboxMaterials = _config.GetSBoxMaterialExportEnabled();

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
                    mats.AppendLine("{");
                    if (staticpart.Material == null)
                    {
                        mats.AppendLine($"    from = \"{staticMeshName}_Group{staticpart.GroupIndex}_Index{staticpart.Index}_{i}_{staticpart.LodCategory}.vmat\"");
                        mats.AppendLine($"    to = \"materials/black_matte.vmat\"");
                    }
                    else
                    {
                        mats.AppendLine($"    from = \"{staticpart.Material.FileHash}.vmat\"");
                        mats.AppendLine($"    to = \"materials/{staticpart.Material.FileHash}.vmat\"");
                    }
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"models/{staticMeshName}.fbx");
                text = text.Replace("%MESHNAME%", staticMeshName);

                File.WriteAllText($"{savePath}/{staticMeshName}.vmdl", text);
            }
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
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
                    mats.AppendLine("{");
                    if (part.Material == null)
                    {
                        mats.AppendLine($"    from = \"{hash}_Group{part.GroupIndex}_Index{part.Index}_{i}_{part.LodCategory}.vmat\"");
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
                text = text.Replace("%FILENAME%", $"models/{hash}.fbx");
                text = text.Replace("%MESHNAME%", hash);

                File.WriteAllText($"{savePath}/{hash}.vmdl", text);
            }
        }
        catch(Exception e)
        {
            Log.Error(e.Message);
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
        Directory.CreateDirectory($"{savePath}/Materials");
        StringBuilder vmat = new StringBuilder();

        vmat.AppendLine("Layer0 \n{");

        //If the shader doesnt exist, just use the default complex.shader
        if (!File.Exists($"{savePath}/Shaders/PS_{materialHeader.PixelShader?.Hash}.shader"))
        {
            vmat.AppendLine($"  shader \"complex.shader\"");

            //Use just the first texture for the diffuse
            if (materialHeader.EnumeratePSTextures().Any())
            {
                if (materialHeader.EnumeratePSTextures().ElementAt(0).Texture is not null)
                    vmat.AppendLine($"  TextureColor \"Textures/{materialHeader.EnumeratePSTextures().ElementAt(0).Texture.Hash}.png\"");
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

            vmat.AppendLine($"\tDynamicParams\r\n\t{{");
            foreach (var entry in bytecode_hlsl)
            {
                vmat.AppendLine($"\t\tcb0_{entry.Key} \"{entry.Value}\"");
            }

            vmat.AppendLine($"\t\tcb2_0 \"float4(0,1,1,1)\"");
            vmat.AppendLine($"\t\tcb2_1 \"float4(0,1,1,1)\"");

            for(int i = 0; i < 37; i++)
            {
                vmat.AppendLine($"\t\tcb8_{i} \"float4(1,1,1,1)\"");
            }

            vmat.AppendLine($"\t\tcb12_4 \"float4(1,0,0,0)\"");
            vmat.AppendLine($"\t\tcb12_5 \"float4(0,1,0,0)\"");
            vmat.AppendLine($"\t\tcb12_6 \"float4(0,0,1,0)\"");

            vmat.AppendLine($"\t\tcb13_0 \"Time\"");
            vmat.AppendLine($"\t\tcb13_1 \"float4(1,1,1,1)\"");

            vmat.AppendLine($"\t}}");
        }

        foreach (var e in materialHeader.EnumeratePSTextures())
        {
            if (e.Texture == null)
                continue;

            vmat.AppendLine($"\tTextureT{e.TextureIndex} \"Textures/{e.Texture.Hash}.png\"");
        }

        //vmat.AppendLine(PopulateCBuffers(materialHeader.Decompile(materialHeader.VertexShader.GetBytecode(), $"vs{materialHeader.VertexShader.Hash}"), materialHeader, true).ToString());
        vmat.AppendLine(PopulateCBuffers(materialHeader).ToString());
        vmat.AppendLine("}");

        string terrainDir = isTerrain ? "/Terrain/" : "";
        if (isTerrain)
            Directory.CreateDirectory($"{savePath}/materials/{terrainDir}");

        try
        {
            File.WriteAllText($"{savePath}/materials/{terrainDir}{hash}.vmat", vmat.ToString());
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

    public static void SaveCubemapVTEX(Texture tex, string savePath)
    {
        if(tex.IsCubemap())
        {
            var file = TextureFile.CreateDefault(tex, ImageDimension.CUBEARRAY);
            var json = JsonSerializer.Serialize(file, JsonSerializerOptions.Default);
            File.WriteAllText($"{savePath}/{tex.Hash}.vtex", json);

            //DDS2Vtex(tex, savePath);
        }
    }

    //Doesnt really work but im gonna leave it here anyways i guess
    //public static void DDS2Vtex(Texture tex, string savePath)
    //{
    //    using var img = Pfimage.FromStream(tex.GetTexture(), new PfimConfig(decompress: false));
    //    var dds = img as Dds;

    //    if (dds == null)
    //    {
    //        Console.Error.WriteLine($"Error, file is not a DDS, but a {img.GetType()} (format {img.Format}, datalen {img.DataLen})");
    //    }

    //    var flags = VTexFlags.NO_LOD;
    //    var numMipLevels = (byte)1;

    //    if (dds.Header.MipMapCount != 0)
    //    {
    //        Console.Error.WriteLine("Warning, DDS has mipmaps, which may not work correctly.");
    //        flags &= ~VTexFlags.NO_LOD;
    //        numMipLevels = (byte)dds.Header.MipMapCount;
    //    }

    //    var format = dds switch
    //    {
    //        Dxt1Dds => VTexFormat.DXT1,
    //        Dxt5Dds => VTexFormat.DXT5,
    //        Bc6hDds => VTexFormat.BC6H,
    //        Bc7Dds => VTexFormat.BC7,
    //        _ => VTexFormat.UNKNOWN,
    //    };

    //    if (format == VTexFormat.UNKNOWN)
    //    {
    //        Console.Error.WriteLine($"Error, do not handle DDS with format {dds.GetType()}.");
    //    }

    //    // TODO: check blocksize
    //    using FileStream stream = new FileStream($"{savePath}/{tex.Hash}.vtex_c", FileMode.Create);
    //    using var writer = new BinaryWriter(stream);
    //    ValveTexture vtex = null!;
    //    var nonDataSize = 0;
    //    var offsetOfDataSize = 0;

    //    using (var resource = new Resource())
    //    {
    //        var assembly = Assembly.GetExecutingAssembly();
    //        using var template = assembly.GetManifestResourceStream("vtex.template");
    //        resource.Read(template);
    //        vtex = (ValveTexture)resource.DataBlock;

    //        // Write a copy of the vtex_c up to the DATA block region
    //        nonDataSize = (int)resource.DataBlock.Offset;

    //        resource.Reader.BaseStream.Seek(8, SeekOrigin.Begin);
    //        var blockOffset = resource.Reader.ReadUInt32();
    //        var blockCount = resource.Reader.ReadUInt32();
    //        resource.Reader.BaseStream.Seek(blockOffset - 8, SeekOrigin.Current); // 8 is 2 uint32s we just read
    //        for (var i = 0; i < blockCount; i++)
    //        {
    //            var blockType = Encoding.UTF8.GetString(resource.Reader.ReadBytes(4));
    //            resource.Reader.BaseStream.Position += 8; // Offset, size
    //            if (blockType == "DATA")
    //            {
    //                offsetOfDataSize = (int)resource.Reader.BaseStream.Position - 4;
    //                break;
    //            }
    //        }

    //        resource.Reader.BaseStream.Position = 0;
    //        writer.Write(resource.Reader.ReadBytes(nonDataSize).ToArray());
    //    }

    //    // Write the VTEX data
    //    writer.Write(vtex.Version);
    //    writer.Write((ushort)flags);
    //    writer.Write(vtex.Reflectivity[0]);
    //    writer.Write(vtex.Reflectivity[1]);
    //    writer.Write(vtex.Reflectivity[2]);
    //    writer.Write(vtex.Reflectivity[3]);
    //    writer.Write((ushort)dds.Width);
    //    writer.Write((ushort)dds.Height);
    //    writer.Write((ushort)(dds.Header.Depth != 0 ? dds.Header.Depth : 1));
    //    writer.Write((byte)format);
    //    writer.Write((byte)numMipLevels);
    //    writer.Write((uint)0);

    //    // Extra data
    //    writer.Write((uint)0);
    //    writer.Write((uint)0);

    //    var resourceSize = (uint)stream.Length;
    //    var resourceDataSize = (uint)(resourceSize - nonDataSize);

    //    // Dxt data goes here
    //    writer.Write(dds.Data);

    //    // resource: fixup the full and DATA block size
    //    writer.Seek(0, SeekOrigin.Begin);
    //    writer.Write(resourceSize);

    //    writer.Seek(offsetOfDataSize, SeekOrigin.Begin);
    //    writer.Write(resourceDataSize);
    //}
}
