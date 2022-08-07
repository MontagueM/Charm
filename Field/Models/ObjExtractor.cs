using System.Text;

namespace Field.Models;

public static class ObjExtractor
{
    public static void SaveFromVertices(List<Vector3> vertexPositions, string savePath)
    {
        string directory = Path.GetDirectoryName(savePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        StringBuilder sb = new StringBuilder();
        foreach (var translation in vertexPositions)
        {
            sb.AppendLine($"v {translation.X} {translation.Y} {translation.Z}");
        }
        File.WriteAllText(savePath, sb.ToString());
    }
}