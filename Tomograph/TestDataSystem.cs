using System.Reflection;
using Tiger;

namespace Tomograph;

public class TestDataSystem
{
    public static void VerifyTestData(Type T)
    {
        foreach (TestPackage testPackage in EnumerateTestPackagesFromClass(T))
        {
            bool bShouldSkipAsGarbage = testPackage.Timestamp == 0;
            if (bShouldSkipAsGarbage) 
            {
                continue;
            }
            ValidatePath(testPackage.Path);
            ValidateTimestamp(testPackage);
        }
    }

    private static void ValidatePath(string testPackagePackagePath)
    {
        IPackage.CheckValidPackagePath(testPackagePackagePath);
    }
    
    private static void ValidateTimestamp(TestPackage testPackage)
    {
        IPackage package = PackageResourcer.Get().GetPackage(testPackage.Path);
        if (package.GetPackageMetadata().Timestamp != testPackage.Timestamp)
        {
            throw new Exception($"Package {testPackage.Path} has invalid timestamp. Expected: {package.GetPackageMetadata().Timestamp}. Actual: {testPackage.Timestamp}");
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