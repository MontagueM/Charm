using System.Collections.ObjectModel;
using Tiger;

namespace Charm.Objects;

public class MockListControl
{
    public ObservableCollection<HashListItemModel> Items { get; set; } = new()
    {
        new HashListItemModel(new TigerHash("ABCD0180"), "typename"),
    };
}
