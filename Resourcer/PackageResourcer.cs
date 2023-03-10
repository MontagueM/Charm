using System.Collections.Concurrent;
using Tiger;

namespace Resourcer;

/// <summary>
/// Provides an interface for getting IPackage objects from a directory of packages.
/// There is one PackageResourcer per game.
/// </summary>
public class PackageResourcer : StrategistSingleton<PackageResourcer>
{
    struct PackageQueueItem
    {
        private int PkgId;
    }

    interface IPackageResourcerTask {
    }

    private ConcurrentQueue<PackageQueueItem> _packageQueue = new ConcurrentQueue<PackageQueueItem>();
    private Dictionary<ushort, IPackage> _packagesCache = new Dictionary<ushort, IPackage>();
    public string PackagesDirectory { get; private set; }

    public PackageResourcer(StrategyConfiguration strategyConfiguration) { PackagesDirectory = strategyConfiguration.PackagesDirectory; }

    /// <summary>
    /// Gets a package which represents a .pkg disk file.
    /// </summary>
    /// <returns>IPackage object, type determined by the selected strategy.</returns>
    public IPackage GetPackage(ushort packageId)
    {
        if (_packagesCache.TryGetValue(packageId, out IPackage package))
        {
            return package;
        }

        return LoadPackageIntoCacheFromDisk(packageId);
        // return Get().GetPackage(packageId);
        return null;
    }

    // todo this needs to be a producer-consumer style queue thing to avoid locking maybe
    // could try it this way first then compare performance with a queue approach

    private IPackage LoadPackageIntoCacheFromDisk(ushort packageId)
    {
        PackagePathsCache.Get().GetPackagePathFromId(packageId);
        IPackage package = (IPackage) Activator.CreateInstance(Strategy.GetPackageType(), packageId);
        if (_packagesCache.TryAdd(packageId, package))
        {
        }
        return package;
    }
}