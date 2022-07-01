namespace Field.Models;

public class AutomatedImporter
{
    public enum EImportType
    {
        Static,
        Entity,
        Map
    }
    
    public static void SaveInteropUnrealPythonFile(string saveDirectory, string meshName, EImportType importType)
    {
        // Copy and rename file
        File.Copy("import_to_ue5.py", $"{saveDirectory}/import_to_ue5_{meshName}.py", true);
        if (importType == EImportType.Static)
        {
            string text = File.ReadAllText($"{saveDirectory}/import_to_ue5_{meshName}.py");
            text = text.Replace("importer.import_entity()", "importer.import_static()");
            File.WriteAllText($"{saveDirectory}/import_to_ue5_{meshName}.py", text);
        }
        else if (importType == EImportType.Map)
        {
            string text = File.ReadAllText($"{saveDirectory}/import_to_ue5_{meshName}.py");
            text = text.Replace("importer.import_entity()", "importer.import_map()");
            File.WriteAllText($"{saveDirectory}/import_to_ue5_{meshName}.py", text);
        }
    }
}