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

    public void Run(CharmArgs args)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        List<LocalizedStrings> tags = resourcer.GetAllTags<LocalizedStrings>();

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
        SQLiteConnection.CreateFile(_databasePath);

        // Parallel.ForEach(packageIds, SaveStringsDatabase);
    }
}
