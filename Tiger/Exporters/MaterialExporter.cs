using ConcurrentCollections;
using Tiger.Schema;

namespace Tiger.Exporters;

// TODO: Clean this up
public class MaterialExporter : AbstractExporter
{
    public override void Export(Exporter.ExportEventArgs args)
    {
        ConcurrentHashSet<Texture> mapTextures = new();
        ConcurrentHashSet<ExportMaterial> mapMaterials = new();
        bool saveShaders = ConfigSubsystem.Get().GetUnrealInteropEnabled() || ConfigSubsystem.Get().GetS2ShaderExportEnabled();
        bool saveIndiv = ConfigSubsystem.Get().GetIndvidualStaticsEnabled();

        Parallel.ForEach(args.Scenes, scene =>
        {
            if (scene.Type is ExportType.Entity or ExportType.Static or ExportType.API or ExportType.D1API)
            {
                ConcurrentHashSet<Texture> textures = scene.Textures;

                foreach (ExportMaterial material in scene.Materials)
                {
                    foreach (STextureTag texture in material.Material.EnumerateVSTextures())
                    {
                        if (texture.Texture == null)
                        {
                            continue;
                        }
                        textures.Add(texture.Texture);
                    }
                    foreach (STextureTag texture in material.Material.EnumeratePSTextures())
                    {
                        if (texture.Texture == null)
                        {
                            continue;
                        }
                        textures.Add(texture.Texture);
                    }

                    if (saveShaders)
                    {
                        string shaderSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, scene.Name);
                        shaderSaveDirectory = $"{shaderSaveDirectory}/Shaders";

                        material.Material.SavePixelShader(shaderSaveDirectory, material.IsTerrain);
                        material.Material.SaveVertexShader(shaderSaveDirectory);
                    }
                }

                string textureSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, scene.Name);
                textureSaveDirectory = $"{textureSaveDirectory}/Textures";

                Directory.CreateDirectory(textureSaveDirectory);
                foreach (Texture texture in textures)
                {
                    texture.SavetoFile($"{textureSaveDirectory}/{texture.Hash}");
                }
            }
            else
            {
                mapTextures.UnionWith(scene.Textures);
                foreach (ExportMaterial material in scene.Materials)
                {
                    foreach (STextureTag texture in material.Material.EnumerateVSTextures())
                    {
                        mapTextures.Add(texture.Texture);
                    }
                    foreach (STextureTag texture in material.Material.EnumeratePSTextures())
                    {
                        mapTextures.Add(texture.Texture);
                    }

                    if (material.Material.VertexShader != null || material.Material.PixelShader != null)
                    {
                        mapMaterials.Add(material);
                    }
                }

                if (ConfigSubsystem.Get().GetS2ShaderExportEnabled())
                {
                    foreach (var cubemap in scene.Cubemaps)
                    {
                        if (cubemap.CubemapTexture is null)
                            continue;

                        mapTextures.Add(cubemap.CubemapTexture);

                        string saveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");
                        saveDirectory = $"{saveDirectory}/Textures";
                        Source2Handler.SaveVTEX(cubemap.CubemapTexture, saveDirectory);
                    }
                }
            }
        });

        foreach (Texture texture in mapTextures)
        {
            if (texture is null)
                continue;

            string textureSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");
            textureSaveDirectory = $"{textureSaveDirectory}/Textures";
            Directory.CreateDirectory(textureSaveDirectory);

            texture.SavetoFile($"{textureSaveDirectory}/{texture.Hash}");
        }

        if (saveShaders && saveIndiv)
        {
            foreach (ExportMaterial material in mapMaterials)
            {
                string shaderSaveDirectory = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");
                shaderSaveDirectory = $"{shaderSaveDirectory}/Shaders";
                Directory.CreateDirectory(shaderSaveDirectory);

                material.Material.SavePixelShader(shaderSaveDirectory, material.IsTerrain);
                material.Material.SaveVertexShader(shaderSaveDirectory);
            }
        }
    }
}
