using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;
using Field;

namespace Field.General;

public class TagHash64Handler
{
    private static Dictionary<ulong, uint> tagHash64Dict = new Dictionary<ulong, uint>();
    
    public static uint GetTagHash64(ulong tagHash64)
    {
        if (CheckTagHash64Valid(tagHash64))
        {
            if (tagHash64Dict.ContainsKey(tagHash64))
            {
                return tagHash64Dict[tagHash64];
            }  
        }

        return 0;
    }
    
    public static string GetTagHash64(string tagHash64Str)
    {
        ulong tagHash64 = Endian.SwapU64(UInt64.Parse(tagHash64Str, NumberStyles.HexNumber));
        if (tagHash64Dict.ContainsKey(tagHash64))
        {
            return Endian.SwapU32(tagHash64Dict[tagHash64]).ToString("X");
        }

        return "";
    }
    
    private static void AddTagHash64(ulong tag, uint hash)
    {
        tagHash64Dict.TryAdd(tag, hash);
    }
    
    public static bool CheckTagHash64Valid(ulong hash)
    {
        if ((hash & 0xffff) != 0)  // Only want to accept a subset of them
        {
            return false;
        }
        return true;
    }

    public static void Initialise()
    {
        DestinyFile.UnmanagedDictionary unmanagedDictionary = DllInitialiseTH64H();
        long[] keys = new long[unmanagedDictionary.Keys.dataSize];
        Marshal.Copy(unmanagedDictionary.Keys.dataPtr, keys, 0, unmanagedDictionary.Keys.dataSize);
        int[] vals = new int[unmanagedDictionary.Values.dataSize];
        Marshal.Copy(unmanagedDictionary.Values.dataPtr, vals, 0, unmanagedDictionary.Values.dataSize);
        List<ulong> kl = keys.Select(x => (ulong)x).ToList();
        tagHash64Dict = kl.ToDictionary(x => x, x => (uint)vals[kl.IndexOf(x)]);
    }

    [DllImport("Symmetry.dll", EntryPoint = "DllInitialiseTH64H", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedDictionary DllInitialiseTH64H();
}