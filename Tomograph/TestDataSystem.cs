using System.Reflection;
using Resourcer;
using Tiger.Attributes;
using Tomograph;

namespace Tomograph;

public class TestDataSystem
{
    public static void VerifyTestData(Type T)
    {
        foreach (TestPackage testPackage in EnumerateTestPackagesFromClass(T))
        {
            bool bShouldSkipAsGarbage = testPackage.PackageTimestamp == 0;
            if (bShouldSkipAsGarbage) 
            {
                continue;
            }
            ValidatePath(testPackage.PackagePath);
            ValidateTimestamp(testPackage);
        }
    }

    private static void ValidatePath(string testPackagePackagePath)
    {
        D2Package.CheckValidPackagePath(testPackagePackagePath);
    }
    
    private static void ValidateTimestamp(TestPackage testPackage)
    {
        D2Package package = new D2Package(testPackage.PackagePath);
        if (package.GetPackageMetadata().PackageTimestamp != testPackage.PackageTimestamp)
        {
            throw new Exception($"Package {testPackage.PackagePath} has invalid timestamp. Expected: {package.GetPackageMetadata().PackageTimestamp}. Actual: {testPackage.PackageTimestamp}");
        }
    }

    public static IEnumerable<TestPackage> EnumerateTestPackagesFromClass(Type T)
    {
        if (T.IsSubclassOf(typeof(CharmPackageTests)) == null)
        {
            throw new ArgumentException("Type must implement CharmTests");
        }
        foreach (var field in T
                     .GetFields(BindingFlags.Static | BindingFlags.NonPublic)
                     .Where(x => x.FieldType == typeof(TestPackage)))
        {
            TestPackage package = (TestPackage) field.GetValue(null);
            yield return package;
        }
    }
}