using System.Reflection;
using Tiger;
using Tomograph;
using TomographData;

public class Program
{
    private static DepotDownloader depotDownloader = new DepotDownloader();

    public static int Main(string[] args)
    {
        SetDepotDownloaderCredentials(args);

        var charmTestsType = typeof(CharmPackageTests);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => charmTestsType.IsAssignableFrom(p));

        foreach (Type type in types)
        {
            TestStrategyAttribute? attribute = (TestStrategyAttribute)type.GetCustomAttribute(typeof(TestStrategyAttribute), false);
            if (attribute == null)
            {
                Console.WriteLine($"TestClass {type} has no TestStrategyAttribute, skipping test data initialization.");
                continue;
            }
            StrategyMetadataAttribute strategyMetadata = attribute.Strategy.GetStrategyMetadata();

            if (!strategyMetadata.DepotManifestVersionMain.HasValue || !strategyMetadata.DepotManifestVersionAudio.HasValue)
            {
                Console.WriteLine($"TestClass Strategy {strategyMetadata} has no DepotManifestVersionMain and DepotManifestVersionAudio, skipping test data initialization.");
                continue;
            }

            GetTestData(type, attribute.Strategy, strategyMetadata);
        }

        return 0;
    }

    private static void SetDepotDownloaderCredentials(string[] args)
    {
        depotDownloader.SetCredentials(args[0], args[1]);
    }

    public static async void GetTestData(Type testClassType, TigerStrategy strategy, StrategyMetadataAttribute strategyMetadata)
    {
        List<TestPackage> testPackages = TestDataSystem.EnumerateTestPackagesFromClass(testClassType).ToList();
        HashSet<string> allPackagePaths = GetRequiredPatches(testPackages, strategy);
        List<string> fileList = allPackagePaths.Select(x => Path.Join("packages", x)).ToList();
        await depotDownloader.Download(strategyMetadata.DepotManifestVersionMain.Value, strategy.ToString(), fileList);

        TestDataSystem.VerifyTestData(testClassType);
    }

    // We might only want a couple of the packages to test, but we'll still need patches in-between to get the data.
    private static HashSet<string> GetRequiredPatches(List<TestPackage> testPackages, TigerStrategy strategy)
    {
        // todo replace with PackageResourcer stuff
        TestPackage.TestPackageStrategy = strategy;
        HashSet<string> fullPackageList = testPackages.Select(x => x.Name).ToHashSet();
        foreach (TestPackage testPackage in testPackages)
        {
            GetAllPackageIdsForPackage(testPackage.Name).ToList().ForEach(x => fullPackageList.Add(x));
        }

        return fullPackageList;
    }

    private static IEnumerable<string> GetAllPackageIdsForPackage(string packageName)
    {
        int patchIdOfPackageName = GetPatchIdFromPackageName(packageName);
        for (int newPatch = 0; newPatch < patchIdOfPackageName; newPatch++)
        {
            string modifiedName = GetPackageNameWithDifferentPatchId(packageName, patchIdOfPackageName, newPatch);
            yield return modifiedName;
        }
    }

    private static int GetPatchIdFromPackageName(string packageName)
    {
        string noExtension = packageName.Contains('.') ? packageName.Split('.')[0] : packageName;
        string[] split = noExtension.Split('_');
        int patchId = Convert.ToInt32(split[^1]);
        return patchId;
    }

    private static string GetPackageNameWithDifferentPatchId(string packageName, int originalPatchId, int newPatchId)
    {
        bool hasExtension = packageName.Contains('.');
        string newPackageName = hasExtension ? packageName.Replace($"_{originalPatchId}.", $"_{newPatchId}.") : packageName.Substring(0, packageName.Length - 1) + newPatchId;
        return newPackageName;
    }
}
