using System.Collections.ObjectModel;
using Tiger;

namespace Charm.Objects;

public class MockListControl
{
    public ObservableCollection<ListItem> MockItems { get; set; } = new()
    {
        new ListItem(new TigerHash("ABCD0180")),
    };
}
