using System.Diagnostics;
using Arithmic;

namespace Tiger.Commandlets;

public class PerfTestCommandlet : ICommandlet
{
    public void Run(CharmArgs args)
    {
        TestPackageReads();
        TestAllPackagesAndFiles();
    }

    private void TestAllPackagesAndFiles()
    {
        List<ushort> packageIds = PackageResourcer.Get().PackagePathsCache.GetAllPackageIds();

        RunSerialAllTest(packageIds);
        RunParallelAllTest(packageIds);
    }

    private void RunSerialAllTest(List<ushort> packageIds)
    {
        Log.Info("Running serial test");
        foreach (ushort packageId in packageIds)
        {
            IPackage pkg = PackageResourcer.Get().GetPackage(packageId);
            // read every file in the package
            PackageMetadata pkgMetadata = pkg.GetPackageMetadata();
            ushort fileCount = (ushort)pkgMetadata.FileCount;

            RunTest(RunGroupedTest, pkg, fileCount);
            // RunTest(RunSerialTest, pkg, fileCount);
            // RunTest(RunParallelTest, pkg, fileCount);
        }
    }

    private void RunParallelAllTest(List<ushort> packageIds)
    {
        Log.Info("Running parallel test");
        Parallel.ForEach(packageIds, packageId =>
        {
            IPackage pkg = PackageResourcer.Get().GetPackage(packageId);
            // read every file in the package
            PackageMetadata pkgMetadata = pkg.GetPackageMetadata();
            ushort fileCount = (ushort)pkgMetadata.FileCount;

            RunTest(RunGroupedTest, pkg, fileCount);
            // RunTest(RunSerialTest, pkg, fileCount);
            // RunTest(RunParallelTest, pkg, fileCount);
        });
    }


    private void TestPackageReads()
    {
        ushort pkgId = PackageResourcer.Get().PackagePathsCache.GetAllPackagesMap()
            .First(pair => pair.Value.Contains("w64_sr_gear_0426_7.pkg")).Key;
        IPackage pkg = PackageResourcer.Get().GetPackage(pkgId);

        // read every file in the package
        PackageMetadata pkgMetadata = pkg.GetPackageMetadata();
        ushort fileCount = (ushort)pkgMetadata.FileCount;

        int testCount = 3;
        RunManyTests(RunGroupedTest, testCount, pkg, fileCount);
        // RunManyTests(RunSerialTest, testCount, pkg, fileCount);
        // RunManyTests(RunParallelTest, testCount, pkg, fileCount);
    }

    private double RunTest(Action<IPackage, ushort> testFunc, IPackage pkg, ushort fileCount)
    {
        Stopwatch sw = Stopwatch.StartNew();
        testFunc(pkg, fileCount);
        sw.Stop();
        Log.Info($"{testFunc.Method.Name}: Generated {fileCount} reads in {sw.ElapsedMilliseconds}ms");
        return sw.ElapsedMilliseconds;
    }

    private void RunManyTests(Action<IPackage, ushort> testFunc, int testCount, IPackage pkg, ushort fileCount)
    {
        List<double> results = new(testCount);
        for (int i = 0; i < testCount; i++)
        {
            results.Add(RunTest(testFunc, pkg, fileCount));
        }

        (double mean, double stdDeviation) = MathNet.Numerics.Statistics.Statistics.MeanStandardDeviation(results);
        Log.Info($"{testFunc.Method.Name}: took {mean} pm {stdDeviation} ms over {testCount} runs");
    }

#pragma warning disable S1144 // Unused private types or members should be removed
    private void RunSerialTest(IPackage pkg, ushort fileCount)
    {
        for (ushort i = 0; i < fileCount; i++)
        {
            // get pkg handle
            pkg.GetFileBytes(i);
        }
    }

    private void RunParallelTest(IPackage pkg, ushort fileCount)
    {
        Parallel.For(0, fileCount, i =>
        {
            // get pkg handle
            pkg.GetFileBytes((ushort)i);
        });
    }

    private void RunGroupedTest(IPackage pkg, ushort fileCount)
    {
        pkg.GetAllFileData();
    }
}
