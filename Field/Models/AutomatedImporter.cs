namespace Field.Models;

public class AutomatedImporter
{
    public static void SaveInteropUnrealPythonFile(string saveDirectory, string meshName, bool bStaticMesh = false)
    {
        // Copy and rename file
        File.Copy("import_to_ue5.py", $"{saveDirectory}/import_to_ue5_{meshName}.py", true);
        if (bStaticMesh)
        {
            string text = File.ReadAllText($"{saveDirectory}/import_to_ue5_{meshName}.py");
            text = text.Replace("importer.import_entity()", "importer.import_static()");
            File.WriteAllText($"{saveDirectory}/import_to_ue5_{meshName}.py", text);
        }
    }
}