using System.Reflection;
using Tiger;
using SKPackage = Tiger.DESTINY2_SHADOWKEEP_2601.Package;
using WQPackage = Tiger.DESTINY2_WITCHQUEEN_6307.Package;

namespace Tomograph;

public abstract class CharmPackageTests
{
    public void InitialiseTest()
    {
        TestPackage.TestPackageStrategy = Helpers.GetTestClassStrategy(GetType());
        Strategy.AddNewStrategy(Helpers.GetTestClassStrategy(GetType()), TestPackage.TestPackageDataDirectory);
        TestDataSystem.VerifyTestData(GetType());
    }

    public void CleanupTest() { Strategy.Reset(); }
}

public interface IPackageTests
{
    void Package_PathValid();
    void Package_PathDoesNotExist();
    void Package_PathInvalidPrefix();
    void Package_PathInvalidExtension();

    void PackageMetadata_Valid();

    void FileMetadata_Valid();
    void FileMetadata_Valid_81Hash();
    void FileMetadata_InvalidPackageId();
    void FileMetadata_FileIndexOutOfRange();

    void FileBytes_ValidDecryptedAndDecompressed_File();
}

public struct TestPackage
{
    private static readonly string TestPackageDataTopDirectory = System.IO.Path.Join("../../..", "TestData");
    public static TigerStrategy TestPackageStrategy { get; set; }
    public static string TestPackageDataDirectory => System.IO.Path.GetFullPath(System.IO.Path.Join(TestPackageDataTopDirectory, TestPackageStrategy.ToString(), "packages"));
    public string Name;

    public static string OverrideDirectory { get; set; }

    public string Path
    {
        get
        {
            if (string.IsNullOrEmpty(OverrideDirectory))
            {
                return System.IO.Path.Join(TestPackageDataDirectory, Name);
            }
            else
            {
                return System.IO.Path.Join(OverrideDirectory, Name);
            }
        }
    }
    public uint Timestamp;

    public TestPackage(string name, uint timestamp)
    {
        Name = name;
        Timestamp = timestamp;
    }

    public TestPackage(string name)
    {
        Name = name;
    }
}

[TestClass, TestCategory("DESTINY2_SHADOWKEEP_2601"), TestStrategy(TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
public class DESTINY2_SHADOWKEEP_2601_PackageTests : CharmPackageTests, IPackageTests
{
    private static readonly TestPackage ValidNoPatch = new("w64_ui_startup_unp1_0.pkg", 1567640311);
    private static readonly TestPackage ValidPatchLast = new("w64_audio_01a7_3.pkg", 1567664383);
    private static readonly TestPackage ValidPatch81 = new("w64_cinematics_06d6_3.pkg", 1567667820);

    [TestInitialize]
    public void Initialize()
    {
        InitialiseTest();
    }

    [TestCleanup]
    public void Cleanup() { CleanupTest(); }

    [TestMethod]
    public void Package_PathValid()
    {
        SKPackage package = new SKPackage(ValidNoPatch.Path);
        Assert.AreEqual(NormalizePath(ValidNoPatch.Path), NormalizePath(package.PackagePath));
    }

    private static readonly TestPackage InvalidPackagePath_DoesNotExist = new("w64_sr_audio_063c_0.pkg");
    [TestMethod, ExpectedExceptionWithMessage(typeof(FileNotFoundException), "The package path '{packagePath}' does not exist")]
    public void Package_PathDoesNotExist()
    {
        SKPackage package = new SKPackage(InvalidPackagePath_DoesNotExist.Path);
    }

    private static readonly string InvalidPackagePath_InvalidPrefix = @"../../../TestData/D2InvalidPrefix/ps4_test.pkg";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the correct prefix '{prefix}'")]
    public void Package_PathInvalidPrefix()
    {
        SKPackage package = new SKPackage(InvalidPackagePath_InvalidPrefix);
    }

    private static readonly string InvalidPackagePath_InvalidExtension = @"../../../TestData/D2InvalidExtension/w64_test.bin";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the extension {PackageExtension}.")]
    public void Package_PathInvalidExtension()
    {
        SKPackage package = new SKPackage(InvalidPackagePath_InvalidExtension);
    }

    static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    [TestMethod]
    public void PackageMetadata_Valid()
    {
        SKPackage package = new SKPackage(ValidPatch81.Path);
        PackageMetadata packageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(NormalizePath(ValidPatch81.Path), NormalizePath(packageMetadata.Path));
        Assert.AreEqual(ValidPatch81.Name, packageMetadata.Name);
        Assert.AreEqual(0x6d6, packageMetadata.Id);
        Assert.AreEqual(3, packageMetadata.PatchId);
        Assert.AreEqual(ValidPatch81.Timestamp, packageMetadata.Timestamp);
    }

    [TestMethod]
    public void FileMetadata_Valid()
    {
        SKPackage packageUnp = new SKPackage(ValidNoPatch.Path);

        FileMetadata fileMetadata0Unp = packageUnp.GetFileMetadata(new FileHash(0x80a12000 | 0));
        Assert.AreEqual(new FileHash(0x80a12000 | 0).Hash32, fileMetadata0Unp.Hash.Hash32);
        Assert.AreEqual(0x80809a8a, fileMetadata0Unp.Reference.Hash32);
        Assert.AreEqual(61600, fileMetadata0Unp.Size);
        FileMetadata fileMetadata00 = packageUnp.GetFileMetadata(0);
        Assert.AreEqual(new FileHash(0x80a12000 | 0).Hash32, fileMetadata00.Hash.Hash32);
        Assert.AreEqual(0x80809a8a, fileMetadata00.Reference.Hash32);
        Assert.AreEqual(61600, fileMetadata00.Size);

        SKPackage package = new SKPackage(ValidPatchLast.Path);

        FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x80b4e000 | 0));
        Assert.AreEqual(new FileHash(0x80b4e000 | 0).Hash32, fileMetadata0.Hash.Hash32);
        Assert.AreEqual(0x80809a8a, fileMetadata0.Reference.Hash32);
        Assert.AreEqual(23952, fileMetadata0.Size);

        FileMetadata fileMetadata1000 = package.GetFileMetadata(new FileHash(0x80b4e000 | 0x1000));
        Assert.AreEqual(new FileHash(0x80b4e000 | 0x1000).Hash32, fileMetadata1000.Hash.Hash32);
        Assert.AreEqual(0x80809a8a, fileMetadata1000.Reference.Hash32);
        Assert.AreEqual(3152, fileMetadata1000.Size);

        FileMetadata fileMetadata1fff = package.GetFileMetadata(new FileHash(0x80b4e000 | 0x1fff));
        Assert.AreEqual(new FileHash(0x80b4e000 | 0x1fff).Hash32, fileMetadata1fff.Hash.Hash32);
        Assert.AreEqual(0xc0fc93e4, fileMetadata1fff.Reference.Hash32);
        Assert.AreEqual(1610, fileMetadata1fff.Size);
        Assert.AreEqual(26, fileMetadata1fff.Type);
        Assert.AreEqual(5, fileMetadata1fff.SubType);
    }

    [TestMethod]
    public void FileMetadata_Valid_81Hash()
    {
        SKPackage package = new SKPackage(ValidPatch81.Path);
        FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x815ac000 | 0));
        Assert.AreEqual(new FileHash(0x815ac000 | 0).Hash32, fileMetadata0.Hash.Hash32);
        Assert.AreEqual(0x80809c36, fileMetadata0.Reference.Hash32);
        Assert.AreEqual(4896, fileMetadata0.Size);
        FileMetadata fileMetadata00 = package.GetFileMetadata(0);
        Assert.AreEqual(new FileHash(0x815ac000 | 0).Hash32, fileMetadata00.Hash.Hash32);
        Assert.AreEqual(0x80809c36, fileMetadata00.Reference.Hash32);
        Assert.AreEqual(4896, fileMetadata00.Size);

        FileMetadata fileMetadata500 = package.GetFileMetadata(new FileHash(0x815ac000 | 500));
        Assert.AreEqual(new FileHash(0x815ac000 | 500).Hash32, fileMetadata500.Hash.Hash32);
        Assert.AreEqual(0x80809c36, fileMetadata500.Reference.Hash32);
        Assert.AreEqual(6304, fileMetadata500.Size);

        FileMetadata fileMetadata1666 = package.GetFileMetadata(new FileHash(0x815ac000 | 1666));
        Assert.AreEqual(new FileHash(0x815ac000 | 1666).Hash32, fileMetadata1666.Hash.Hash32);
        Assert.AreEqual(0x815ac681, fileMetadata1666.Reference.Hash32);
        Assert.AreEqual(948, fileMetadata1666.Size);
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The provided file hash has an invalid package id: {fileHash.PackageId}")]
    public void FileMetadata_InvalidPackageId()
    {
        SKPackage package = new SKPackage(ValidPatchLast.Path);
        package.GetFileMetadata(new FileHash(package.GetPackageMetadata().Id + 1, 0));
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentOutOfRangeException), "The provided file hash has an out-of-range file index {fileIndex} >= {FileEntries.Count}")]
    public void FileMetadata_FileIndexOutOfRange()
    {
        SKPackage package = new SKPackage(ValidPatchLast.Path);
        package.GetFileMetadata(0x2000);
    }

    [TestMethod]
    public void FileBytes_ValidDecryptedAndDecompressed_File()
    {
        SKPackage package = new SKPackage(ValidPatchLast.Path);
        byte[] actualFileBytes = package.GetFileBytes(new FileHash(package.GetPackageMetadata().Id, 0));
        byte[] expectedFileBytes = File.ReadAllBytes("../../../TestData/SKPackageTests/FileBytes_ValidDecryptedAndDecompressed_File.bin");
        CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes);
    }
}

// New package tests are only required when a new package type is added
[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class DESTINY2_WITCHQUEEN_6307_PackageTests : CharmPackageTests, IPackageTests
{
    private static readonly TestPackage ValidNoPatch = new("w64_ui_startup_unp1_0.pkg", 1674714492);
    private static readonly TestPackage ValidPatchFirst = new("w64_sr_raids_011d_0.pkg", 1601620832);
    private static readonly TestPackage ValidPatchLast = new("w64_sr_raids_011d_7.pkg", 1674717874);
    private static readonly TestPackage ValidPatch81 = new("w64_sr_gear_0426_7.pkg", 1674718077);

    [TestInitialize]
    public void Initialize()
    {
        InitialiseTest();
    }

    [TestCleanup]
    public void Cleanup() { CleanupTest(); }

    [TestMethod]
    public void Package_PathValid()
    {
        WQPackage package = new WQPackage(ValidNoPatch.Path);
        Assert.AreEqual(NormalizePath(ValidNoPatch.Path), NormalizePath(package.PackagePath));
    }

    private static readonly TestPackage InvalidPackagePath_DoesNotExist = new("w64_sr_audio_063c_0.pkg");
    [TestMethod, ExpectedExceptionWithMessage(typeof(FileNotFoundException), "The package path '{packagePath}' does not exist")]
    public void Package_PathDoesNotExist()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_DoesNotExist.Path);
    }

    private static readonly string InvalidPackagePath_InvalidPrefix = @"../../../TestData/D2InvalidPrefix/ps4_test.pkg";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the correct prefix '{prefix}'")]
    public void Package_PathInvalidPrefix()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_InvalidPrefix);
    }

    private static readonly string InvalidPackagePath_InvalidExtension = @"../../../TestData/D2InvalidExtension/w64_test.bin";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the extension {PackageExtension}.")]
    public void Package_PathInvalidExtension()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_InvalidExtension);
    }

    static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    [TestMethod]
    public void PackageMetadata_Valid()
    {
        WQPackage package = new WQPackage(ValidPatch81.Path);
        PackageMetadata packageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(NormalizePath(ValidPatch81.Path), NormalizePath(packageMetadata.Path));
        Assert.AreEqual(ValidPatch81.Name, packageMetadata.Name);
        Assert.AreEqual(0x426, packageMetadata.Id);
        Assert.AreEqual(7, packageMetadata.PatchId);
        Assert.AreEqual(ValidPatch81.Timestamp, packageMetadata.Timestamp);
    }

    [TestMethod]
    public void FileMetadata_Valid()
    {
        WQPackage packageUnp = new WQPackage(ValidNoPatch.Path);

        FileMetadata fileMetadata0Unp = packageUnp.GetFileMetadata(new FileHash(0x80a00000 | 0));
        Assert.AreEqual(new FileHash(0x80a00000 | 0).Hash32, fileMetadata0Unp.Hash.Hash32);
        Assert.AreEqual(2155910602, fileMetadata0Unp.Reference.Hash32);
        Assert.AreEqual(192, fileMetadata0Unp.Size);

        WQPackage package = new WQPackage(ValidPatchFirst.Path);

        FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x80A3A000 | 0));
        Assert.AreEqual(new FileHash(0x80A3A000 | 0).Hash32, fileMetadata0.Hash.Hash32);
        Assert.AreEqual(2155911665, fileMetadata0.Reference.Hash32);
        Assert.AreEqual(34480, fileMetadata0.Size);
        FileMetadata fileMetadata00 = package.GetFileMetadata(0);
        Assert.AreEqual(new FileHash(0x80A3A000 | 0).Hash32, fileMetadata00.Hash.Hash32);
        Assert.AreEqual(2155911665, fileMetadata00.Reference.Hash32);
        Assert.AreEqual(34480, fileMetadata00.Size);

        FileMetadata fileMetadata1000 = package.GetFileMetadata(new FileHash(0x80A3A000 | 0x1000));
        Assert.AreEqual(new FileHash(0x80A3A000 | 0x1000).Hash32, fileMetadata1000.Hash.Hash32);
        Assert.AreEqual(2155899168, fileMetadata1000.Reference.Hash32);
        Assert.AreEqual(64, fileMetadata1000.Size);

        FileMetadata fileMetadata1fff = package.GetFileMetadata(new FileHash(0x80A3A000 | 0x1a86));
        Assert.AreEqual(new FileHash(0x80A3A000 | 0x1a86).Hash32, fileMetadata1fff.Hash.Hash32);
        Assert.AreEqual(2155900330, fileMetadata1fff.Reference.Hash32);
        Assert.AreEqual(2304, fileMetadata1fff.Size);
    }

    [TestMethod]
    public void FileMetadata_Valid_81Hash()
    {
        WQPackage package = new WQPackage(ValidPatch81.Path);
        FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x8104c000 | 0));
        Assert.AreEqual(new FileHash(0x8104c000 | 0).Hash32, fileMetadata0.Hash.Hash32);
        Assert.AreEqual(2163982335, fileMetadata0.Reference.Hash32);
        Assert.AreEqual(40, fileMetadata0.Size);
        FileMetadata fileMetadata00 = package.GetFileMetadata(0);
        Assert.AreEqual(new FileHash(0x8104c000 | 0).Hash32, fileMetadata00.Hash.Hash32);
        Assert.AreEqual(2163982335, fileMetadata00.Reference.Hash32);
        Assert.AreEqual(40, fileMetadata00.Size);

        FileMetadata fileMetadata1000 = package.GetFileMetadata(new FileHash(0x8104c000 | 0x1000));
        Assert.AreEqual(new FileHash(0x8104c000 | 0x1000).Hash32, fileMetadata1000.Hash.Hash32);
        Assert.AreEqual(2155899168, fileMetadata1000.Reference.Hash32);
        Assert.AreEqual(64, fileMetadata1000.Size);

        FileMetadata fileMetadata1a85 = package.GetFileMetadata(new FileHash(0x8104c000 | 0x1a85));
        Assert.AreEqual(new FileHash(0x8104c000 | 0x1a85).Hash32, fileMetadata1a85.Hash.Hash32);
        Assert.AreEqual(2164578948, fileMetadata1a85.Reference.Hash32);
        Assert.AreEqual(1396736, fileMetadata1a85.Size);
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The provided file hash has an invalid package id: {fileHash.PackageId}")]
    public void FileMetadata_InvalidPackageId()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        package.GetFileMetadata(new FileHash(package.GetPackageMetadata().Id + 1, 0));
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentOutOfRangeException), "The provided file hash has an out-of-range file index {fileIndex} >= {FileEntries.Count}")]
    public void FileMetadata_FileIndexOutOfRange()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        package.GetFileMetadata(0x2000);
    }

    [TestMethod]
    public void FileBytes_ValidDecryptedAndDecompressed_File()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        byte[] actualFileBytes = package.GetFileBytes(new FileHash(package.GetPackageMetadata().Id, 0));
        byte[] expectedFileBytes = File.ReadAllBytes("../../../TestData/WQPackageTests/FileBytes_ValidDecryptedAndDecompressed_File.bin");
        CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes);
    }

    // todo more tests to validate different logical paths e.g. not compressed, not encrypted, different patch id, failure on invalid data due to oodle version etc
}

[TestClass, TestCategory("DESTINY2_LATEST"), TestStrategy(TigerStrategy.DESTINY2_LATEST)]
public class DESTINY2_LATEST_PackageTests : CharmPackageTests, IPackageTests
{
    private static readonly TestPackage ValidNoPatch = new("w64_ui_startup_unp1_0.pkg");
    private static readonly TestPackage ValidPatchLast = new("w64_sr_sandbox_0123_3.pkg");

    public new void InitialiseTest()
    {
        ConfigSubsystem config = new ConfigSubsystem("../../../../Tomograph/TestData/latest_config.json");
        Helpers.CallNonPublicMethod(config, "Initialise");
        Strategy.SetStrategy(TigerStrategy.DESTINY2_LATEST);
        TestPackage.OverrideDirectory = Strategy.CurrentStrategy.GetStrategyConfiguration().PackagesDirectory;
        TestDataSystem.VerifyTestData(GetType());
    }

    [TestInitialize]
    public void Initialize()
    {
        InitialiseTest();
    }

    [TestCleanup]
    public void Cleanup() { CleanupTest(); }

    [TestMethod]
    public void Package_PathValid()
    {
        WQPackage package = new WQPackage(ValidNoPatch.Path);
        Assert.AreEqual(NormalizePath(ValidNoPatch.Path), NormalizePath(package.PackagePath));
    }

    private static readonly TestPackage InvalidPackagePath_DoesNotExist = new("w64_sr_audio_063c_0.pkg");
    [TestMethod, ExpectedExceptionWithMessage(typeof(FileNotFoundException), "The package path '{packagePath}' does not exist")]
    public void Package_PathDoesNotExist()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_DoesNotExist.Path);
    }

    private static readonly string InvalidPackagePath_InvalidPrefix = @"../../../TestData/D2InvalidPrefix/ps4_test.pkg";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the correct prefix '{prefix}'")]
    public void Package_PathInvalidPrefix()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_InvalidPrefix);
    }

    private static readonly string InvalidPackagePath_InvalidExtension = @"../../../TestData/D2InvalidExtension/w64_test.bin";
    [TestMethod, ExpectedExceptionWithMessage(typeof(ArgumentException), "The package path '{packagePath}' does not have the extension {PackageExtension}.")]
    public void Package_PathInvalidExtension()
    {
        WQPackage package = new WQPackage(InvalidPackagePath_InvalidExtension);
    }

    static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    [TestMethod]
    public void PackageMetadata_Valid()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        PackageMetadata packageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(NormalizePath(ValidPatchLast.Path), NormalizePath(packageMetadata.Path));
        Assert.AreEqual(ValidPatchLast.Name, packageMetadata.Name);
        Assert.AreEqual(0x123, packageMetadata.Id);
        Assert.AreEqual(3, packageMetadata.PatchId);
    }

    [TestMethod]
    public void FileMetadata_Valid()
    {
        // WQPackage packageUnp = new WQPackage(ValidNoPatch.Path);
        //
        // FileMetadata fileMetadata0Unp = packageUnp.GetFileMetadata(new FileHash(0x80a12000 | 0));
        // Assert.AreEqual(new FileHash(0x80a12000 | 0).Hash32, fileMetadata0Unp.Hash.Hash32);
        // Assert.AreEqual(0x80809a8a, fileMetadata0Unp.Reference.Hash32);
        // Assert.AreEqual(61600, fileMetadata0Unp.Size);
        // FileMetadata fileMetadata00 = packageUnp.GetFileMetadata(0);
        // Assert.AreEqual(new FileHash(0x80a12000 | 0).Hash32, fileMetadata00.Hash.Hash32);
        // Assert.AreEqual(0x80809a8a, fileMetadata00.Reference.Hash32);
        // Assert.AreEqual(61600, fileMetadata00.Size);
        //
        // WQPackage package = new WQPackage(ValidPatchLast.Path);
        //
        // FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x80b4e000 | 0));
        // Assert.AreEqual(new FileHash(0x80b4e000 | 0).Hash32, fileMetadata0.Hash.Hash32);
        // Assert.AreEqual(0x80809a8a, fileMetadata0.Reference.Hash32);
        // Assert.AreEqual(23952, fileMetadata0.Size);
        //
        // FileMetadata fileMetadata1000 = package.GetFileMetadata(new FileHash(0x80b4e000 | 0x1000));
        // Assert.AreEqual(new FileHash(0x80b4e000 | 0x1000).Hash32, fileMetadata1000.Hash.Hash32);
        // Assert.AreEqual(0x80809a8a, fileMetadata1000.Reference.Hash32);
        // Assert.AreEqual(3152, fileMetadata1000.Size);
        //
        // FileMetadata fileMetadata1fff = package.GetFileMetadata(new FileHash(0x80b4e000 | 0x1fff));
        // Assert.AreEqual(new FileHash(0x80b4e000 | 0x1fff).Hash32, fileMetadata1fff.Hash.Hash32);
        // Assert.AreEqual(0xc0fc93e4, fileMetadata1fff.Reference.Hash32);
        // Assert.AreEqual(1610, fileMetadata1fff.Size);
        // Assert.AreEqual(26, fileMetadata1fff.Type);
        // Assert.AreEqual(5, fileMetadata1fff.SubType);
    }

    [TestMethod]
    public void FileMetadata_Valid_81Hash()
    {
        // there are no 81s in latest game
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentException), "The provided file hash has an invalid package id: {fileHash.PackageId}")]
    public void FileMetadata_InvalidPackageId()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        package.GetFileMetadata(new FileHash(package.GetPackageMetadata().Id + 1, 0));
    }

    [TestMethod]
    [ExpectedExceptionWithMessage(typeof(ArgumentOutOfRangeException), "The provided file hash has an out-of-range file index {fileIndex} >= {FileEntries.Count}")]
    public void FileMetadata_FileIndexOutOfRange()
    {
        WQPackage package = new WQPackage(ValidPatchLast.Path);
        package.GetFileMetadata(0x2000);
    }

    [TestMethod]
    public void FileBytes_ValidDecryptedAndDecompressed_File()
    {
        // WQPackage package = new WQPackage(ValidPatchLast.Path);
        // byte[] actualFileBytes = package.GetFileBytes(new FileHash(package.GetPackageMetadata().Id, 0));
        // byte[] expectedFileBytes = File.ReadAllBytes("../../../TestData/WQPackageTests/FileBytes_ValidDecryptedAndDecompressed_File.bin");
        // CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes);
    }
}
