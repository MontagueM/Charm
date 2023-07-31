using System.Collections.Concurrent;
using System.Data.SQLite;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Strings;

namespace Tiger.Commandlets;



public class SaveStringsDatabaseCommandlet : ICommandlet
{
    private static string _databasePath = $"./StringsDatabases/{Strategy.CurrentStrategy}.db";
    private static string _connectionString = $"Data Source=\"{_databasePath}\";Version=3;";

    private static ConcurrentDictionary<uint, List<LocalizedStringView>> _strings = new();

    public void Run(CharmArgs args)
    {
        PackageResourcer resourcer = PackageResourcer.Get();
        HashSet<LocalizedStrings> tags = resourcer.GetAllFiles<LocalizedStrings>();

        Parallel.ForEach(tags, GetStringsData);

        Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
        SQLiteConnection.CreateFile(_databasePath);


        // then in one go do the sqlite transaction
        using (SQLiteConnection connection = new(_connectionString))
        {
            connection.Open();
            SQLiteTransaction transaction = connection.BeginTransaction();
            SQLHandle handle = new(connection, transaction);

            SQLTable<LocalizedStringView> _stringViewTable = new();
            _stringViewTable.CreateTable(connection);

            foreach (List<LocalizedStringView> value in _strings.Values)
            {
                _stringViewTable.InsertValues(handle, value);
            }

            transaction.Commit();
        }
    }

    private void GetStringsData(LocalizedStrings tag)
    {
        List<LocalizedStringView> strings = tag.GetAllStringViews();

        _strings.TryAdd(tag.Hash.Hash32, strings);
    }
}
