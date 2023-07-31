using Arithmic;

namespace Tiger.Commandlets;

public class FindBytesInPackagesCommandlet : ICommandlet
{
    private string bytesStr;
    private byte[] bytes;

    public void Run(CharmArgs args)
    {
        // var config = Strategy.GetStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307);
        // config.PackagesDirectory = "I:/v6307/packages/";
        // Strategy.UpdateStrategyConfiguration(TigerStrategy.DESTINY2_WITCHQUEEN_6307, config);

        string packageFilter;
        if (!args.GetArgValue("packageFilter", out packageFilter))
        {
            Log.Warning("No packageFilter argument provided, searching all packages");
            // return;
        }

        if (!args.GetArgValue("bytes", out bytesStr))
        {
            Log.Error("No bytes argument provided");
            return;
        }
        bytesStr = bytesStr.Replace(" ", "");

        if (bytesStr.Length % 8 != 0 || bytesStr.Length == 0)
        {
            Log.Error("Bytes string must be multiple of 8 (4 bytes)");
            return;
        }

        // take some ABCD string and convert int 0xab, 0xcd
        bytes = StringToByteArray(bytesStr);

        SearchForBytes(packageFilter);
    }

    private static byte[] StringToByteArray(string hex)
    {
        // Convert the string to a byte array.
        byte[] bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    private void SearchForBytes(string packageFilter)
    {
        List<string> packagePaths = PackageResourcer.Get().PackagePathsCache.GetAllPackagesMap().Where(pair => pair.Value.Contains(packageFilter)).ToList().ConvertAll(pair => pair.Value);
        Log.Info($"Searching {packagePaths.Count} packages for '{bytesStr}'");
        Parallel.ForEach(packagePaths, SearchPackage);
    }

    private void SearchPackage(string packagePath)
    {
        using (TigerReader reader = new(File.Open(packagePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
        {
            reader.Seek(0, SeekOrigin.End);
            long length = reader.Position;
            reader.Seek(0, SeekOrigin.Begin);
            while (reader.Position <= length - bytes.Length)
            {
                if (reader.ReadBytes(bytes.Length).SequenceEqual(bytes))
                {
                    Log.Info($"Found in {packagePath} at offset {reader.Position - bytes.Length}");
                    break; // stop after one instance
                }
            }
        }
    }
}
