using System.Collections.Generic;
using Charm.Objects;

namespace Charm.ListFilters;

public interface IListFilter
{
    /// <summary>
    /// Runs to filter out items that should not be added to the list to show.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool ShouldAddItem(ListItemModel item);

    public List<IListFilterSwitch> Switches { get; }
}

public interface IListFilterSwitch
{
    public string Name { get; }
    public bool IsEnabled { get; set; }
    public void OnSwitch();
}
