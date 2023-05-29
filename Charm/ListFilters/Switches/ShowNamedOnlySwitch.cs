using System;

namespace Charm.ListFilters.Switches;

public class ShowNamedOnlySwitch : IListFilterSwitch
{
    public string Name => "Show Named Only";
    public bool IsEnabled { get; set; }
    public void OnSwitch()
    {
        throw new NotImplementedException();
    }
}
