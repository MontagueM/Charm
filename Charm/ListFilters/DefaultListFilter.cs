using System.Collections.Generic;
using Charm.ListFilters.Switches;
using Charm.Objects;

namespace Charm.ListFilters;

public class DefaultListFilter : IListFilter
{
    public bool ShouldAddItem(ListItem item)
    {
        return true;
    }

    public List<IListFilterSwitch> Switches { get; } = new() {new ShowNamedOnlySwitch(), new TrimNameSwitch()};
}
