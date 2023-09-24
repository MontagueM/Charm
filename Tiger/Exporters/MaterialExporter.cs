using ConcurrentCollections;
using Tiger.Schema;
using Tiger.Schema.Shaders;

namespace Tiger.Exporters;

public class MaterialExporter : AbstractExporter
{
    public override void Export(Exporter.ExportEventArgs args)
    {
        ExportTextures(args);
    }

    private void ExportTextures(Exporter.ExportEventArgs args)
    {
        ConcurrentHashSet<Texture> mapTextures = new();
        Parallel.ForEach(args.Scenes, scene =>
        {
            if (scene.Type is ExportType.Entity or ExportType.Static)
            {
                ConcurrentHashSet<Texture> textures = scene.Textures;
                foreach (IMaterial material in scene.Materials)
                {
                    foreach (STextureTag texture in material.EnumerateVSTextures())
                    {
                        if (texture.Texture == null)
                        {
                            continue;
                        }
                        textures.Add(texture.Texture);
                    }
                    foreach (STextureTag texture in material.EnumeratePSTextures())
                    {
                        if (texture.Texture == null)
                        {
                            continue;
                        }
                        textures.Add(texture.Texture);
                    }
                }

                string saveDirectory = $"{args.OutputDirectory}/{scene.Name}/Textures";
                Directory.CreateDirectory(saveDirectory);
                foreach (Texture texture in textures)
                {
                    texture.SavetoFile($"{saveDirectory}/{texture.Hash}");
                }
            }
            else
            {
                mapTextures.UnionWith(scene.Textures);
                foreach (IMaterial material in scene.Materials)
                {
                    foreach (STextureTag texture in material.EnumerateVSTextures())
                    {
                        mapTextures.Add(texture.Texture);
                    }
                    foreach (STextureTag texture in material.EnumeratePSTextures())
                    {
                        mapTextures.Add(texture.Texture);
                    }
                }
            }
        });

        string saveDirectory = $"{args.OutputDirectory}/Maps/Textures";
        Directory.CreateDirectory(saveDirectory);
        foreach (Texture texture in mapTextures)
        {
            texture.SavetoFile($"{saveDirectory}/{texture.Hash}");
        }
    }
}
