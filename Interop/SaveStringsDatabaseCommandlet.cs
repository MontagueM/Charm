using System.Collections.Concurrent;
using System.Data.SQLite;
using Schema;
using Tiger;

namespace Interop;

struct LocalizedStringView
{
    public FileHash ParentFileHash { get; }
    public FileHash DataFileHash { get; }
    public int StringIndex { get; }
    public StringHash StringHash { get; }
    public string RawString { get; }
}

public class SaveStringsDatabaseCommandlet : ICommandlet
{
    private static string _databasePath = $"./StringsDatabases/{Strategy.CurrentStrategy}.db";
    private static string _connectionString = $"Data Source=\"{_databasePath}\";Version=3;";
    private static ConcurrentBag<LocalizedStringView> _localizedStrings = new();

    public void Run(CharmArgs args)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        List<ushort> packageIds = resourcer.GetAllPackages();

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
        SQLiteConnection.CreateFile(_databasePath);

        Parallel.ForEach(packageIds, SaveStringsDatabase);
    }
    
    private void SaveStringsDatabase(ushort packageId)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        List<LocalizedStrings> tags = resourcer.GetAllTags<LocalizedStrings>();
        
        // _localizedStrings.Add(package.GetAllLocalizedStrings());
    }
}