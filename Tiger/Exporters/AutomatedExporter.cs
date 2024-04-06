using System.Text;
using Newtonsoft.Json;
using Tiger.Schema;

namespace Tiger.Exporters;

public class AutomatedExporter
{
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

    public static void SaveInteropBlenderPythonFile(string saveDirectory, string meshName, ImportType importType, TextureExportFormat textureFormat)
    {
        //Not gonna delete just in case

        //// Copy and rename file
        //saveDirectory = saveDirectory.Replace("\\", "/");
        //File.Copy("Exporters/import_to_blender.py", $"{saveDirectory}/{meshName}_import_to_blender.py", true);

        ////Lets just make a py for all exports now because why not
        //string text = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_blender.py");
        //text = text.Replace("HASH", $"{meshName}");
        //text = text.Replace("OUTPUT_DIR", $"{saveDirectory}");
        //text = text.Replace("IMPORT_TYPE", $"{importType.ToString().Replace("ImportType.", "")}");
        //File.WriteAllText($"{saveDirectory}/{meshName}_import_to_blender.py", text);

        //// change extension
        //string textExtensions = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_blender.py");
        //switch (textureFormat)
        //{
        //    case ETextureFormat.PNG:
        //        textExtensions = textExtensions.Replace("TEX_EXT", ".png");
        //        break;
        //    case ETextureFormat.TGA:
        //        textExtensions = textExtensions.Replace("TEX_EXT", ".tga");
        //        break;
        //    default:
        //        textExtensions = textExtensions.Replace("TEX_EXT", ".dds");
        //        break;
        //}
        //File.WriteAllText($"{saveDirectory}/{meshName}_import_to_blender.py", textExtensions);
    }


    public static void SaveBlenderApiFile(string saveDirectory, string meshName, TextureExportFormat outputTextureFormat, List<Dye> dyes, string fileSuffix = "")
    {
        File.Copy($"Exporters/blender_api_template.py", $"{saveDirectory}/{meshName}{fileSuffix}.py", true);
        string text = File.ReadAllText($"{saveDirectory}/{meshName}{fileSuffix}.py");

        string[] components = { "X", "Y", "Z", "W" };

        int dyeIndex = 1;
        foreach (var dye in dyes)
        {
            dye.ExportTextures($"{saveDirectory}/Textures", outputTextureFormat);
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
        File.WriteAllText($"{saveDirectory}/{meshName}{fileSuffix}.py", text);
    }

    public static void SaveD1ShaderInfo(string saveDirectory, string meshName, TextureExportFormat outputTextureFormat, List<DyeD1> dyes, string fileSuffix = "")
    {
        StringBuilder text = new();

        foreach (var dye in dyes)
        {
            text.AppendLine(JsonConvert.SerializeObject(dye, Formatting.Indented));
        }

        File.WriteAllText($"{saveDirectory}/{meshName}{fileSuffix}.json", text.ToString());
    }
}
