namespace Field.Models;

public class AutomatedImporter
{
    public static void SaveInteropUnrealPythonFile(string saveDirectory, string meshName)
    {
        // Copy and rename file
        File.Copy("import_to_ue5.py", $"{saveDirectory}/import_to_ue5_{meshName}.py");
    }
}