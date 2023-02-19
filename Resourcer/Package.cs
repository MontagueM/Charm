namespace Resourcer;

public interface IPackage
{
    // PackageMetadata GetPackageMetadata();
    //
    // FileMetadata GetFileMetadata(FileHash fileId);

    byte[] GetFileBytes(FileHash fileId);
}

public class D2Package : IPackage
{
    // public PackageMetadata GetPackageMetadata()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public FileMetadata GetFileMetadata(FileHash fileId)
    // {
    //     throw new NotImplementedException();
    // }

    public byte[] GetFileBytes(FileHash fileId)
    {
        throw new NotImplementedException();
    }
}