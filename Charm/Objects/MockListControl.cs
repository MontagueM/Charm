using System.Collections.ObjectModel;
using Tiger;

namespace Charm.Objects;

public class MockListControl
{
    public ObservableCollection<ListItemModel> MockItems { get; set; } = new()
    {
        new ListItemModel(new TigerHash("ABCD0180")),
    };
}
