using System.Diagnostics;
using System.Reflection;
using Tiger;
using Tomograph;
using TomographData;

public class Program
{
    private static DepotDownloader depotDownloader = new DepotDownloader();
    private static Dictionary<TigerStrategy, List<string>> fileLists = new();
    private static List<Type> testTypes = new();

    public static async Task Main(string[] args)
    {
        SetDepotDownloaderCredentials(args);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName.StartsWith("Tomograph"))
            .SelectMany(s => s.GetTypes());


        foreach (Type type in types)
        {
            TestStrategyAttribute? attribute = (TestStrategyAttribute)type.GetCustomAttribute(typeof(TestStrategyAttribute), false);
            if (attribute == null)
            {
                Debug.WriteLine($"TestClass {type} has no TestStrategyAttribute, skipping test data initialization.");
                continue;
            }
            StrategyMetadataAttribute strategyMetadata = attribute.Strategy.GetStrategyMetadata();

            if (!strategyMetadata.DepotManifestVersionMain.HasValue || !strategyMetadata.DepotManifestVersionAudio.HasValue)
            {
                Console.WriteLine($"TestClass Strategy {strategyMetadata} has no DepotManifestVersionMain and DepotManifestVersionAudio, skipping test data initialization.");
                continue;
            }

            Console.WriteLine($"Getting test data for strategy '{attribute.Strategy}' and type {type}.");
            List<string> fileList = GetTestData(type, attribute.Strategy, strategyMetadata);
            if (!fileLists.ContainsKey(attribute.Strategy))
            {
                fileLists.Add(attribute.Strategy, new List<string>());
            }
            fileLists[attribute.Strategy].AddRange(fileList);
            testTypes.Add(type);
        }

        await Task.Run(Download);
    }

    private static async void Download()
    {
        foreach ((TigerStrategy strategy, List<string> fileList) in fileLists)
        {
            StrategyMetadataAttribute strategyMetadata = strategy.GetStrategyMetadata();
            await depotDownloader.Download(strategyMetadata.DepotManifestVersionMain.Value, strategy.ToString(), fileList);
        }

        foreach (Type testType in testTypes)
        {
            TestDataSystem.VerifyTestData(testType);
        }
    }

    private static void SetDepotDownloaderCredentials(string[] args)
    {
        depotDownloader.SetCredentials(args[0], args[1]);
    }

    public static List<string> GetTestData(Type testClassType, TigerStrategy strategy, StrategyMetadataAttribute strategyMetadata)
    {
        List<TestPackage> testPackages = TestDataSystem.EnumerateTestPackagesFromClass(testClassType).ToList();
        if (testPackages.Count == 0)
        {
            Console.WriteLine($"No test packages in {testClassType}");
            return new List<string>();
        }
        HashSet<string> allPackagePaths = GetRequiredPatches(testPackages, strategy);
        return allPackagePaths.Select(x => Path.Join("packages", x)).ToList();
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
