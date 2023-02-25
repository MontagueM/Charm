using Resourcer;

namespace Tomograph;

[TestClass]
public class PackageTests
{
     private static readonly string D2Latest_ValidPackagesDirectory = @"I:\SteamLibrary\steamapps\common\Destiny 2\packages";
     private static readonly string D2Latest_NoPatch = @"w64_ui_startup_unp1_0.pkg";
     private static readonly string D2Latest_Patch = @"w64_sr_raids_011d_2.pkg";
     private static readonly string D2Latest_Patch81Hash = @"w64_sr_gear_0426_7.pkg";
     private string D2Latest_ValidPackagePath_NoPatch = Path.Combine(D2Latest_ValidPackagesDirectory, D2Latest_NoPatch);
     private string D2Latest_ValidPackagePath_Patch = Path.Combine(D2Latest_ValidPackagesDirectory, D2Latest_Patch);
     private string D2Latest_ValidPackagePath_Patch_81Hash = Path.Combine(D2Latest_ValidPackagesDirectory, D2Latest_Patch81Hash);

     private void D2Package_Setup()
     {
          Strategy.Reset();
          Strategy.AddNewStrategy(TigerStrategy.DESTINY2_LATEST, D2Latest_ValidPackagesDirectory);
     }
     
     [TestMethod]
     public void D2Package_ValidPath()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_ValidPackagePath_NoPatch);
          Assert.AreEqual(NormalizePath(D2Latest_ValidPackagePath_NoPatch), NormalizePath(package.PackagePath));
     }
     
     private static readonly string D2Latest_InvalidPackagePath_DoesNotExist = @"I:\SteamLibrary\steamapps\common\Destiny 2\packages\w64_sr_audio_063c_0.pkg";
     [TestMethod]
     [ExpectedExceptionWithMessage(typeof(FileNotFoundException), typeof(D2Package), "PackagePathDoesNotExistMessage")]
     public void D2Package_InvalidPath_DoesNotExist()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_InvalidPackagePath_DoesNotExist);
     }
     
     private static readonly string D2Latest_InvalidPackagePath_InvalidPrefix = @"../../../Packages/D2InvalidPrefix/ps4_test.pkg";
     [TestMethod]
     [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(D2Package), "PackagePathInvalidPrefixMessage")]
     public void D2Package_InvalidPath_InvalidPrefix()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_InvalidPackagePath_InvalidPrefix);
     }
     
     private static readonly string D2Latest_InvalidPackagePath_InvalidExtension = @"../../../Packages/D2InvalidExtension/w64_test.bin";
     [TestMethod]
     [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(D2Package), "PackagePathInvalidExtensionMessage")]
     public void D2Package_InvalidPath_InvalidExtension()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_InvalidPackagePath_InvalidExtension);
     }
     
     public static string NormalizePath(string path)
     {
          return Path.GetFullPath(new Uri(path).LocalPath)
               .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
               .ToUpperInvariant();
     }
     
     [TestMethod]
     public void Package_ValidPackage_ValidPackageMetadata()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_ValidPackagePath_Patch_81Hash);
          PackageMetadata packageMetadata = package.GetPackageMetadata();
          Assert.AreEqual(NormalizePath(D2Latest_ValidPackagePath_Patch_81Hash), NormalizePath(packageMetadata.PackagePath));
          Assert.AreEqual(D2Latest_Patch81Hash, packageMetadata.PackageName);
          Assert.AreEqual(0x426, packageMetadata.PackageId);
          Assert.AreEqual(7, packageMetadata.PatchId);
          Assert.AreEqual(1674107738, packageMetadata.Timestamp);
     }
     
     [TestMethod]
     public void Package_ValidFile_ValidFileMetadata()
     {
          D2Package_Setup();
          
          D2Package packageUnp = new D2Package(D2Latest_ValidPackagePath_NoPatch);
          
          FileMetadata fileMetadata0Unp = packageUnp.GetFileMetadata(new FileHash(0x80a00000 | 0));
          Assert.AreEqual(new FileHash(0x80a00000 | 0).Hash32, fileMetadata0Unp.Hash.Hash32);
          Assert.AreEqual(2155910602, fileMetadata0Unp.Reference.Hash32);
          Assert.AreEqual(192, fileMetadata0Unp.Size);

          D2Package package = new D2Package(D2Latest_ValidPackagePath_Patch);
          
          FileMetadata fileMetadata0 = package.GetFileMetadata(new FileHash(0x80A3A000 | 0));
          Assert.AreEqual(new FileHash(0x80A3A000 | 0).Hash32, fileMetadata0.Hash.Hash32);
          Assert.AreEqual(2155911665, fileMetadata0.Reference.Hash32);
          Assert.AreEqual(34656, fileMetadata0.Size);
          FileMetadata fileMetadata00 = package.GetFileMetadata(0);
          Assert.AreEqual(new FileHash(0x80A3A000 | 0).Hash32, fileMetadata00.Hash.Hash32);
          Assert.AreEqual(2155911665, fileMetadata00.Reference.Hash32);
          Assert.AreEqual(34656, fileMetadata00.Size);
          
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
     public void Package_ValidFile_ValidFileMetadata_81Hash()
     {
          D2Package_Setup();

          D2Package package = new D2Package(D2Latest_ValidPackagePath_Patch_81Hash);
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
     [ExpectedExceptionWithMessage(typeof(ArgumentException), typeof(D2Package), "FileMetadataInvalidPackageIdMessage")]
     public void Package_InvalidFile_InvalidPackageId()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_ValidPackagePath_Patch);
          package.GetFileMetadata(new FileHash(package.GetPackageMetadata().PackageId+1, 0));
     }
     
     [TestMethod]
     [ExpectedExceptionWithMessage(typeof(ArgumentOutOfRangeException), typeof(D2Package), "FileMetadataFileIndexOutOfRangeMessage")]
     public void Package_InvalidFile_OutOfRange()
     {
          D2Package_Setup();
          
          D2Package package = new D2Package(D2Latest_ValidPackagePath_Patch);
          package.GetFileMetadata(0x2000);
     }
}