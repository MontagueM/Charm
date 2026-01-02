using System.Collections.Concurrent;
using Arithmic;
using Newtonsoft.Json;
using Tiger.Schema;

namespace Tiger.Exporters;

public class AutomatedExporter
{
    public static object _lock = new();
    public enum ImportType
    {
        Static,
        Entity,
        Map,
        Terrain,
        API
    }

    public static void SaveInteropUnrealPythonFile(string saveDirectory, string meshName, ImportType importType, TextureExportFormat textureFormat, bool bSingleFolder = true)
    {
        // Copy and rename file
        File.Copy("Exporters/import_to_ue5.py", $"{saveDirectory}/{meshName}_import_to_ue5.py", true);
        if (importType == ImportType.Static)
        {
            string text = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_ue5.py");
            text = text.Replace("importer.import_entity()", "importer.import_static()");
            File.WriteAllText($"{saveDirectory}/{meshName}_import_to_ue5.py", text);
        }
        else if (importType == ImportType.Map)
        {
            string text = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_ue5.py");
            text = text.Replace("b_unique_folder=False", $"b_unique_folder={!bSingleFolder}");
            text = text.Replace("importer.import_entity()", "importer.import_map()");
            File.WriteAllText($"{saveDirectory}/{meshName}_import_to_ue5.py", text);
        }
        // change extension
        string textExtensions = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_ue5.py");
        switch (textureFormat)
        {
            case TextureExportFormat.PNG:
                textExtensions = textExtensions.Replace(".dds", ".png");
                break;
            case TextureExportFormat.TGA:
                textExtensions = textExtensions.Replace(".dds", ".tga");
                break;
        }
        File.WriteAllText($"{saveDirectory}/{meshName}_import_to_ue5.py", textExtensions);
    }


    // TODO, clean this up, its getting ugly
    public static void SaveBlenderApiFile(string saveDirectory, string meshName, TextureExportFormat outputTextureFormat, List<Dye> dyes, string fileSuffix = "")
    {
        try
        {
            lock (_lock)
            {
                string text = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}/Exporters/blender_api_template.py");
                string[] components = { "X", "Y", "Z", "W" };
                int dyeIndex = 1;

                // TEMP and gross fix for very rare case where theres just no dyes...
                if (dyes.Count == 0)
                {
                    DyeInfo dyeInfo = Dye.DefaultDye();
                    foreach (System.Reflection.FieldInfo fieldInfo in dyeInfo.GetType().GetFields())
                    {
                        Vector4 value = (Vector4)fieldInfo.GetValue(dyeInfo);
                        if (!fieldInfo.CustomAttributes.Any())
                            continue;
                        string valueName = fieldInfo.CustomAttributes.First().ConstructorArguments[0].Value.ToString();
                        for (int i = 0; i < 4; i++)
                        {
                            text = text.Replace($"{valueName}{dyeIndex}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                            text = text.Replace($"{valueName}{dyeIndex + 1}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                            text = text.Replace($"{valueName}{dyeIndex + 2}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                        }
                    }
                }

                foreach (Dye? dye in dyes)
                {
                    DyeInfo dyeInfo = dye is not null ? dye.GetDyeInfo() : Dye.DefaultDye();
                    if (dye is not null)
                    {
                        dye.ExportTextures($"{saveDirectory}/Textures", outputTextureFormat);
                        STextureTag diff = dye.TagData.Pixel.Value.Textures[0];
                        text = text.Replace($"DiffMap{dyeIndex}", $"{diff.Texture.Hash}.{TextureExtractor.GetExtension(outputTextureFormat)}");
                        STextureTag norm = dye.TagData.Pixel.Value.Textures[1];
                        text = text.Replace($"NormMap{dyeIndex}", $"{norm.Texture.Hash}.{TextureExtractor.GetExtension(outputTextureFormat)}");
                    }

                    foreach (System.Reflection.FieldInfo fieldInfo in dyeInfo.GetType().GetFields())
                    {
                        Vector4 value = (Vector4)fieldInfo.GetValue(dyeInfo);
                        if (!fieldInfo.CustomAttributes.Any())
                            continue;
                        string valueName = fieldInfo.CustomAttributes.First().ConstructorArguments[0].Value.ToString();
                        for (int i = 0; i < 4; i++)
                        {
                            text = text.Replace($"{valueName}{dyeIndex}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");

                            // Should be 3 total. Armor, Cloth, Suit
                            if (dyes.Count == 1)
                            {
                                text = text.Replace($"{valueName}{dyeIndex + 1}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                                text = text.Replace($"{valueName}{dyeIndex + 2}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                            }
                            else if (dyes.Count == 2 && dyeIndex >= 1) // Dumb
                            {
                                text = text.Replace($"{valueName}{dyeIndex + 2}.{components[i]}", $"{value[i].ToString().Replace(",", ".")}");
                            }
                        }
                    }
                    dyeIndex++;
                }

                text = text.Replace("OUTPUTPATH", $"Textures");
                text = text.Replace("SHADERNAMEENUM", $"{meshName}{fileSuffix}");
                File.WriteAllText($"{saveDirectory}/{meshName}{fileSuffix}.py", text);
            }
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}");
        }
    }


    public static void SaveD1ShaderInfo(string saveDirectory, string meshName, TextureExportFormat outputTextureFormat, List<DyeD1> dyes, string fileSuffix = "")
    {
        ConcurrentDictionary<DyeSlot, ConcurrentBag<D1DyeJSON>> shader = new();

        foreach (DyeD1 dye in dyes)
        {
            SDye_D1 info = dye.TagData;
            if (!shader.ContainsKey((DyeSlot)info.SlotTypeIndex))
                shader[(DyeSlot)info.SlotTypeIndex] = new();

            shader[(DyeSlot)info.SlotTypeIndex].Add(new D1DyeJSON
            {
                DevName = info.DevName,
                PrimaryColor = $"[{info.PrimaryColor.X}, {info.PrimaryColor.Y}, {info.PrimaryColor.Z}, {info.PrimaryColor.W}]",
                SecondaryColor = $"[{info.SecondaryColor.X}, {info.SecondaryColor.Y}, {info.SecondaryColor.Z}, {info.SecondaryColor.W}]",
                DetailDiffuse = info.DetailDiffuse is not null ? $"textures/{info.DetailDiffuse.Hash}.{outputTextureFormat}" : "",
                DetailNormal = info.DetailNormal is not null ? $"textures/{info.DetailNormal.Hash}.{outputTextureFormat}" : "",
                DetailTransform = $"[{info.DetailTransform.X}, {info.DetailTransform.Y}, {info.DetailTransform.Z}, {info.DetailTransform.W}]",
                DetailNormalContributionStrength = $"[{info.DetailNormalContributionStrength.X}, {info.DetailNormalContributionStrength.Y}, {info.DetailNormalContributionStrength.Z}, {info.DetailNormalContributionStrength.W}]",
                SubsurfaceScatteringStrength = $"[{info.SubsurfaceScatteringStrength.X}, {info.SubsurfaceScatteringStrength.Y}, {info.SubsurfaceScatteringStrength.Z}, {info.SubsurfaceScatteringStrength.W}]",
                SpecularProperties = $"[{info.SpecularProperties.X}, {info.SpecularProperties.Y}, {info.SpecularProperties.Z}, {info.SpecularProperties.W}]",
                DecalAlphaMapTransform = $"[{info.DecalAlphaMapTransform.X}, {info.DecalAlphaMapTransform.Y}, {info.DecalAlphaMapTransform.Z}, {info.DecalAlphaMapTransform.W}]",
                DecalBlendOption = info.DecalBlendOption,
                Decal = info.Decal is not null ? $"textures/{info.Decal.Hash}.{outputTextureFormat}" : ""
            });
        }

        File.WriteAllText($"{saveDirectory}/{meshName}{fileSuffix}.json", JsonConvert.SerializeObject(shader, Formatting.Indented));
    }

    public struct D1DyeJSON
    {
        public string DevName;

        public string PrimaryColor;
        public string SecondaryColor;

        public string DetailDiffuse;
        public string DetailNormal;
        public string DetailTransform;
        public string DetailNormalContributionStrength;

        public string SubsurfaceScatteringStrength;
        public string SpecularProperties;

        public string DecalAlphaMapTransform;
        public int DecalBlendOption;
        public string Decal;
    }

    public enum DyeSlot
    {
        Armor,
        Cloth,
        Suit
    }
}
