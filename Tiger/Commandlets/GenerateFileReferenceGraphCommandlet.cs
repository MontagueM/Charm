using Arithmic;

namespace Tiger.Commandlets;

public class GenerateFileReferenceGraphCommandlet : ICommandlet
{
    private string bytesStr;
    private byte[] bytes;

    public void Run(CharmArgs args)
    {
        string packageFilter;
        if (!args.GetArgValue("packageFilter", out packageFilter))
        {
            Log.Warning("No packageFilter argument provided, searching all packages");
            return;
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
        // todo also search for a hash64 of the hash32 if available
        List<ushort> pkgIds = PackageResourcer.Get().PackagePathsCache.GetAllPackagesMap().Where(pair => pair.Value.Contains(packageFilter)).ToList().ConvertAll(pair => pair.Key);
        Log.Info($"Searching {pkgIds.Count} packages for '{bytesStr}'");
        foreach (ushort pkgId in pkgIds)
        {
            SearchPackage(pkgId);
        }
    }

    private void SearchPackage(ushort pkgId)
    {
        IPackage package = PackageResourcer.Get().GetPackage(pkgId);

        PackageMetadata packageMetadata = package.GetPackageMetadata();



        IEnumerable<ushort> fileIndices = package
            .GetAllFileMetadata()
            // .Where(f => (f.Type == 8 || f.Type == 16) && f.SubType == 0)
            .Select(f => f.FileIndex);
        Parallel.ForEach(fileIndices, fileIndex =>
        {
            byte[] fileBytes = package.GetFileBytes(fileIndex);
            using MemoryStream ms = new(fileBytes);
            using BinaryReader br = new(ms);
            while (ms.Position <= ms.Length - bytes.Length)
            {
                if (br.ReadBytes(bytes.Length).SequenceEqual(bytes))
                {
                    Log.Info($"Found in {new FileHash(pkgId, fileIndex)} at offset {ms.Position - bytes.Length}");
                    break; // stop after one instance
                }
            }
        });
    }
}
