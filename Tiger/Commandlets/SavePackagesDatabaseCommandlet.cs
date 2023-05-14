using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Reflection;
using Tiger;
using Tiger.Exporters;

namespace Tiger.Commandlets;

public struct FileMetadataView
{
    public string PackageName;
    public FileHash Hash;
    public TigerHash Reference;
    public int Size;
    public sbyte Type;
    public sbyte SubType;

    public FileMetadataView(FileMetadata fileMetadata, string packageName)
    {
        PackageName = packageName;
        Hash = fileMetadata.Hash;
        Reference = fileMetadata.Reference;
        Size = fileMetadata.Size;
        Type = fileMetadata.Type;
        SubType = fileMetadata.SubType;
    }
}

public class SavePackagesDatabaseCommandlet : ICommandlet
{
    private static string _databasePath = $"./PackageDatabases/{Strategy.CurrentStrategy}.db";
    private static string _connectionString = $"Data Source=\"{_databasePath}\";Version=3;";


    private static ConcurrentDictionary<ushort, PackageMetadata> _packageMetadata = new();
    private static ConcurrentDictionary<ushort, List<FileMetadata>> _fileMetadata = new();

    public void Run(CharmArgs args)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        List<ushort> packageIds = resourcer.PackagePathsCache.GetAllPackageIds();

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
        SQLiteConnection.CreateFile(_databasePath);

        // first get all the data
        Parallel.ForEach(packageIds, GetPackagesData);

        // then in one go do the sqlite transaction
        using (SQLiteConnection connection = new(_connectionString))
        {
            connection.Open();
            SQLiteTransaction transaction = connection.BeginTransaction();
            SQLHandle handle = new(connection, transaction);
            // Add package metadata table to the database

            SQLTable<PackageMetadata> _packageMetadataTable = new();
            _packageMetadataTable.CreateTable(connection);
            _packageMetadataTable.InsertValues(handle, _packageMetadata.Values);

            SQLTable<FileMetadataView> _allFileMetadataTable = new();
            _allFileMetadataTable.CreateTable(connection);

            foreach ((ushort key, List<FileMetadata> value) in _fileMetadata)
            {
                _allFileMetadataTable.InsertValues(handle, value.Select(fileMetadata => new FileMetadataView(fileMetadata, _packageMetadata[key].Name)).ToList());
                // SQLTable<FileMetadata> fileMetadataTable = new(_packageMetadata[key].Name.Split('.')[0]);
                // // Add file metadata to the database
                // fileMetadataTable.CreateTable(connection);
                // fileMetadataTable.InsertValues(handle, value);
            }

            transaction.Commit();
        }
    }

    private void GetPackagesData(ushort packageId)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        IPackage package = resourcer.GetPackage(packageId);

        _packageMetadata[packageId] = package.GetPackageMetadata();
        _fileMetadata[packageId] = package.GetAllFileMetadata();
    }
}
