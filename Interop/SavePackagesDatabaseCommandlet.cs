using Tiger;
using System.Data.SQLite;
using System.Reflection;

namespace Interop;

public class SavePackagesDatabaseCommandlet : ICommandlet
{
    private static string _databasePath = $"./PackageDatabases/{Strategy.CurrentStrategy}.db";
    private static string _connectionString = $"Data Source=\"{_databasePath}\";Version=3;";
    
    private static SQLTable<PackageMetadata> _packageMetadataTable = new();

    public void Run(CharmArgs args)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        List<ushort> packageIds = resourcer.GetAllPackages();

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
        SQLiteConnection.CreateFile(_databasePath);

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            // Add package metadata table to the database
            _packageMetadataTable.CreateTable(connection);
        }

        Parallel.ForEach(packageIds, SavePackageDatabase);
    }
    
    private void SavePackageDatabase(ushort packageId)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        IPackage package = resourcer.GetPackage(packageId);
        

        using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            
            // Add package metadata to the database
            PackageMetadata packageMetadata = package.GetPackageMetadata();
            _packageMetadataTable.InsertValues(connection, packageMetadata);
            
            SQLTable<FileMetadata> fileMetadataTable = new(packageMetadata.Name.Split('.')[0]);
            // Add file metadata to the database
            fileMetadataTable.CreateTable(connection);
            List<FileMetadata> fileMetadatas = package.GetAllFileMetadata();
            fileMetadataTable.InsertValues(connection, fileMetadatas);
        }
    }
}