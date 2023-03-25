using System.Reflection;
using Resourcer;

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

public interface IPackageTests {
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
    private static readonly string TestPackageDataTopDirectory = Path.Join("../../..", "TestData");
    public static TigerStrategy TestPackageStrategy { get; set; }
    public static string TestPackageDataDirectory => Path.GetFullPath(Path.Join(TestPackageDataTopDirectory, TestPackageStrategy.ToString(), "packages"));
    public string PackagePath => Path.Join(TestPackageDataDirectory, PackageName);
    public string PackageName;
    public uint PackageTimestamp;

    public TestPackage(string packageName, uint packageTimestamp)
    {
        PackageName = packageName;
        PackageTimestamp = packageTimestamp;
    }
    
    public TestPackage(string packageName)
    {
        PackageName = packageName;
    }
}

// New package tests are only required when a new package type is added
[TestClass, TestCategory("DESTINY2_WITCHQUEEN_6307"), TestStrategy(TigerStrategy.DESTINY2_WITCHQUEEN_6307)]
public class DESTINY2_WITCHQUEEN_6307_PackageTests : CharmPackageTests, IPackageTests
{
    private static readonly TestPackage ValidNoPatch = new("w64_ui_startup_unp1_0.pkg", 1674714492);
    private static readonly TestPackage ValidPatchFirst = new("w64_sr_raids_011d_0.pkg", 1601620832);
    private static readonly TestPackage ValidPatchMid = new("w64_sr_raids_011d_4.pkg", 1652365275);
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
        D2Package package = new D2Package(ValidNoPatch.PackagePath);
        Assert.AreEqual(NormalizePath(ValidNoPatch.PackagePath), NormalizePath(package.PackagePath));
    }

    private static readonly TestPackage InvalidPackagePath_DoesNotExist = new("w64_sr_audio_063c_0.pkg");
    [TestMethod]
    [ExpectedExceptionWithMessageVariable(typeof(FileNotFoundException), typeof(D2Package), "PackagePathDoesNotExistMessage")]
    public void Package_PathDoesNotExist() { D2Package package = new D2Package(InvalidPackagePath_DoesNotExist.PackagePath); }

    private static readonly string InvalidPackagePath_InvalidPrefix = @"../../../TestData/D2InvalidPrefix/ps4_test.pkg";
    [TestMethod]
    [ExpectedExceptionWithMessageVariable(typeof(ArgumentException), typeof(D2Package), "PackagePathInvalidPrefixMessage")]
    public void Package_PathInvalidPrefix() { D2Package package = new D2Package(InvalidPackagePath_InvalidPrefix); }

    private static readonly string InvalidPackagePath_InvalidExtension = @"../../../TestData/D2InvalidExtension/w64_test.bin";
    [TestMethod]
    [ExpectedExceptionWithMessageVariable(typeof(ArgumentException), typeof(D2Package), "PackagePathInvalidExtensionMessage")]
    public void Package_PathInvalidExtension() { D2Package package = new D2Package(InvalidPackagePath_InvalidExtension); }

    static string NormalizePath(string path)
    {
        return Path.GetFullPath(new Uri(path).LocalPath)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .ToUpperInvariant();
    }

    [TestMethod]
    public void PackageMetadata_Valid()
    {
        D2Package package = new D2Package(ValidPatch81.PackagePath);
        PackageMetadata packageMetadata = package.GetPackageMetadata();
        Assert.AreEqual(NormalizePath(ValidPatch81.PackagePath), NormalizePath(packageMetadata.PackagePath));
        Assert.AreEqual(ValidPatch81.PackageName, packageMetadata.PackageName);
        Assert.AreEqual(0x426, packageMetadata.PackageId);
        Assert.AreEqual(7, packageMetadata.PackagePatchId);
        Assert.AreEqual(ValidPatch81.PackageTimestamp, packageMetadata.PackageTimestamp);
    }

    [TestMethod]
    public void FileMetadata_Valid()
    {
        D2Package packageUnp = new D2Package(ValidNoPatch.PackagePath);

        FileMetadata fileMetadata0Unp = packageUnp.GetFileMetadata(new FileHash(0x80a00000 | 0));
        Assert.AreEqual(new FileHash(0x80a00000 | 0).Hash32, fileMetadata0Unp.Hash.Hash32);
        Assert.AreEqual(2155910602, fileMetadata0Unp.Reference.Hash32);
        Assert.AreEqual(192, fileMetadata0Unp.Size);

        D2Package package = new D2Package(ValidPatchFirst.PackagePath);

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
        D2Package package = new D2Package(ValidPatch81.PackagePath);
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
    [ExpectedExceptionWithMessageVariable(typeof(ArgumentException), typeof(D2Package), "FileMetadataInvalidPackageIdMessage")]
    public void FileMetadata_InvalidPackageId()
    {
        D2Package package = new D2Package(ValidPatchLast.PackagePath);
        package.GetFileMetadata(new FileHash(package.GetPackageMetadata().PackageId + 1, 0));
    }

    [TestMethod]
    [ExpectedExceptionWithMessageVariable(typeof(ArgumentOutOfRangeException), typeof(D2Package), "FileMetadataFileIndexOutOfRangeMessage")]
    public void FileMetadata_FileIndexOutOfRange()
    {
        D2Package package = new D2Package(ValidPatchLast.PackagePath);
        package.GetFileMetadata(0x2000);
    }

    [TestMethod]
    public void FileBytes_ValidDecryptedAndDecompressedSamePatch_Block()
    {
        D2Package package = new D2Package(ValidPatchLast.PackagePath);
        D2BlockEntry blockEntry = Helpers.CallNonPublicMethod<D2BlockEntry>(package, "GetBlockEntry", 253);
        byte[] encryptedAndCompressedBlockBuffer = File.ReadAllBytes(ValidPatchLast.PackagePath).Skip(0x41800).Take(0xDD4).ToArray();
        Helpers.CallNonPublicMethod(
            package, "DecryptAndDecompressBlockBufferIfRequired", new object[] { encryptedAndCompressedBlockBuffer, blockEntry });
    }
    
    [TestMethod]
    public void FileBytes_ValidDecryptedAndDecompressed_File()
    {
        D2Package package = new D2Package(ValidPatchLast.PackagePath);
        byte[] actualFileBytes = package.GetFileBytes(new FileHash(package.GetPackageMetadata().PackageId, 0));
        byte[] expectedFileBytes = File.ReadAllBytes("../../../TestData/D2PackageTests/FileBytes_ValidDecryptedAndDecompressed_File.bin");
        CollectionAssert.AreEqual(expectedFileBytes, actualFileBytes);
    }

    // todo more tests to validate different logical paths e.g. not compressed, not encrypted, different patch id, failure on invalid data due to oodle version etc
}

// [TestClass]
// [TestCategory("D1PS4")]
// public class D1PS4_PackageTests : IPackageTests
// {
//
// }
