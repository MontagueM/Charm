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

    interface IPackageResourcerTask
    {
        
    }

    private ConcurrentQueue<PackageQueueItem> _packageQueue = new ConcurrentQueue<PackageQueueItem>();
    private Dictionary<ushort, IPackage> _packages = new Dictionary<ushort, IPackage>();
    public string PackagesDirectory { get; private set; }
    
    public PackageResourcer(StrategyConfiguration strategyConfiguration)
    {
        PackagesDirectory = strategyConfiguration.PackagesDirectory;
    }

    /// <summary>
    /// Gets a package which represents a .pkg disk file.
    /// </summary>
    /// <returns>IPackage object, type determined by the selected strategy.</returns>
    public IPackage GetPackage(ushort packageId)
    {
        // return Get().GetPackage(packageId);
        return null;
    }
}