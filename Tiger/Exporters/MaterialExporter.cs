using ConcurrentCollections;
using Tiger.Schema;

namespace Tiger.Exporters;

// TODO: Clean this up
public class MaterialExporter : AbstractExporter
{
    public override void Export(Exporter.ExportEventArgs args)
    {
        var _config = ConfigSubsystem.Get();

        ConcurrentHashSet<Texture> mapTextures = new();
        ConcurrentHashSet<ExportMaterial> mapMaterials = new();
        bool saveShaders = _config.GetUnrealInteropEnabled() || _config.GetS2ShaderExportEnabled() || _config.GetExportHLSL();
        bool saveIndiv = _config.GetIndvidualStaticsEnabled();

        Parallel.ForEach(args.Scenes, scene =>
        {
            if (scene.Type is ExportType.Entity or ExportType.Static or ExportType.API or ExportType.D1API)
            {
                ConcurrentHashSet<Texture> textures = scene.Textures;

                foreach (ExportMaterial material in scene.Materials)
                {
                    foreach (STextureTag texture in material.Material.Vertex.EnumerateTextures())
                    {
                        if (texture.GetTexture() == null)
                        {
                            continue;
                        }
                        textures.Add(texture.GetTexture());
                    }
                    foreach (STextureTag texture in material.Material.Pixel.EnumerateTextures())
                    {
                        if (texture.GetTexture() == null)
                        {
                            continue;
                        }
                        textures.Add(texture.GetTexture());
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
                    foreach (STextureTag texture in material.Material.Vertex.EnumerateTextures())
                    {
                        mapTextures.Add(texture.GetTexture());
                    }
                    foreach (STextureTag texture in material.Material.Pixel.EnumerateTextures())
                    {
                        mapTextures.Add(texture.GetTexture());
                    }

                    if (material.Material.Vertex.Shader != null || material.Material.Pixel.Shader != null)
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

        // TODO?: Move this to a global AbstractExporter?

        if (Exporter.Get().GetOrCreateGlobalScene().TryGetItem<SMapAtmosphere>(out SMapAtmosphere atmosphere))
        {
            List<Texture> AtmosTextures = new();
            if (atmosphere.Lookup0 != null)
                AtmosTextures.Add(atmosphere.Lookup0);
            if (atmosphere.Lookup1 != null)
                AtmosTextures.Add(atmosphere.Lookup1);
            if (atmosphere.Lookup2 != null)
                AtmosTextures.Add(atmosphere.Lookup2);
            if (atmosphere.Lookup3 != null)
                AtmosTextures.Add(atmosphere.Lookup3);
            if (atmosphere.UnkD0 != null)
                AtmosTextures.Add(atmosphere.UnkD0);

            string savePath = args.AggregateOutput ? args.OutputDirectory : Path.Join(args.OutputDirectory, $"Maps");
            savePath = $"{savePath}/Textures/Atmosphere";
            Directory.CreateDirectory(savePath);

            foreach (var tex in AtmosTextures)
            {
                // Not ideal but it works
                TextureExtractor.SaveTextureToFile($"{savePath}/{tex.Hash}", tex.IsVolume() ? Texture.FlattenVolume(tex.GetScratchImage(true)) : tex.GetScratchImage());
                if (_config.GetS2ShaderExportEnabled())
                    Source2Handler.SaveVTEX(tex, $"{savePath}", "Atmosphere");
            }
        }
    }
}
