using Field.General;
using Field.Investment;
using Field;
namespace Field.Models;
using System.Configuration;
using System.IO;

public class AutomatedImporter
{
    public enum EImportType
    {
        Static,
        Entity,
        Map,
        Terrain,
        API
    }
    
    public static void SaveInteropUnrealPythonFile(string saveDirectory, string meshName, EImportType importType, ETextureFormat textureFormat, bool bSingleFolder = true)
    {
        // Copy and rename file
        File.Copy("import_to_ue5.py", $"{saveDirectory}/{meshName}_import_to_ue5.py", true);
        if (importType == EImportType.Static)
        {
            string text = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_ue5.py");
            text = text.Replace("importer.import_entity()", "importer.import_static()");
            File.WriteAllText($"{saveDirectory}/{meshName}_import_to_ue5.py", text);
        }
        else if (importType == EImportType.Map)
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
            case ETextureFormat.PNG:
                textExtensions = textExtensions.Replace(".dds", ".png");
                break;
            case ETextureFormat.TGA:
                textExtensions = textExtensions.Replace(".dds", ".tga");
                break;
        }
        File.WriteAllText($"{saveDirectory}/{meshName}_import_to_ue5.py", textExtensions);
    }

    public static void SaveInteropBlenderPythonFile(string saveDirectory, string meshName, EImportType importType, ETextureFormat textureFormat)
    {
        //Not gonna delete just in case

        //// Copy and rename file
        //saveDirectory = saveDirectory.Replace("\\", "/");
        //File.Copy("import_to_blender.py", $"{saveDirectory}/{meshName}_import_to_blender.py", true);
       
        ////Lets just make a py for all exports now because why not
        //string text = File.ReadAllText($"{saveDirectory}/{meshName}_import_to_blender.py");
        //text = text.Replace("HASH", $"{meshName}");
        //text = text.Replace("OUTPUT_DIR", $"{saveDirectory}");
        //text = text.Replace("IMPORT_TYPE", $"{importType.ToString().Replace("EImportType.", "")}");
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

    
    public static void SaveBlenderApiFile(string saveDirectory, string meshName, ETextureFormat outputTextureFormat, List<Dye> dyes, string fileSuffix = "")
    {
        File.Copy($"blender_api_template.py", $"{saveDirectory}/{meshName}_blender_api{fileSuffix}.py", true);
        string text = File.ReadAllText($"{saveDirectory}/{meshName}_blender_api{fileSuffix}.py");

        string[] components = {"X", "Y", "Z", "W"};
        
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
                    text = text.Replace($"{valueName}{dyeIndex}.{components[i]}", $"{value[i]}");
                }
            }

            var diff = dye.Header.DyeTextures[0];
            text = text.Replace($"DiffMap{dyeIndex}", $"{diff.Texture.Hash}_{diff.TextureIndex}.{TextureExtractor.GetExtension(outputTextureFormat)}");
            var norm = dye.Header.DyeTextures[1];
            text = text.Replace($"NormMap{dyeIndex}", $"{norm.Texture.Hash}_{norm.TextureIndex}.{TextureExtractor.GetExtension(outputTextureFormat)}");

            dyeIndex++;
        }

        text = text.Replace("OUTPUTPATH", $"Textures");
        text = text.Replace("SHADERNAMEENUM", $"{meshName}{fileSuffix}");
        File.WriteAllText($"{saveDirectory}/{meshName}_blender_api{fileSuffix}.py", text);
    }
}