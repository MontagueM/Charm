using Field.Textures;

namespace Field.Models;

public class AutomatedImporter
{
    public enum EImportType
    {
        Static,
        Entity,
        Map
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
}