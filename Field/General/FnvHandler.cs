using System.Collections.Concurrent;
using System.Text.Json;

namespace Field.General;

public class FnvHandler
{
    private static ConcurrentDictionary<uint, string> _fnvMap;

    public static void Initialise()
    {
        string fileName = "fnv_charm.json";
        string jsonData = System.IO.File.ReadAllText(fileName);
        _fnvMap = JsonSerializer.Deserialize<ConcurrentDictionary<uint, string>>(jsonData);
        var a = 0;
    }

    public static string GetStringFromHash(uint fnvHash)
    {
        if (_fnvMap.ContainsKey(fnvHash))
        {
            return _fnvMap[fnvHash];
        }

        return fnvHash.ToString();
    }
}