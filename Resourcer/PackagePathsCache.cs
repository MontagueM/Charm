using System.Collections.Concurrent;

namespace Resourcer;

public class PackagePathsCache : StrategistSingleton<PackagePathsCache>
{
    private ConcurrentDictionary<ushort, string> PackageIdToPathMap = new();

    public PackagePathsCache(string packagesDirectory)
    {
        Strategy.CheckValidPackagesDirectory(packagesDirectory);
        FillCache();
    }

    private void FillCache()
    {
        if (CacheFileValid())
        {
            FillCacheFromCacheFile();
        }
        else
        {
            FillCacheFromDirectory();
            SaveCacheToFile();
        }
    }

    private bool CacheFileValid()
    {
        // todo
        return false;
    }

    private void FillCacheFromCacheFile() {}

    private void FillCacheFromDirectory() {}

    private void SaveCacheToFile() {}

    private static readonly string PackageIdNotInPackagePathsCacheMessage = "The package id is not in the package paths cache: ";
    public string GetPackagePath(ushort packageId)
    {
        if (PackageIdToPathMap.TryGetValue(packageId, out string packagePath))
        {
            return packagePath;
        }
        throw new ArgumentException(PackageIdNotInPackagePathsCacheMessage + packageId);
    }

    public string GetPackagePathFromId(ushort packageId)
    {
        if (PackageIdToPathMap.TryGetValue(packageId, out string packagePath))
        {
            return packagePath;
        }

        throw new ArgumentException();
    }
}