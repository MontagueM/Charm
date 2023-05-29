using System;

namespace Charm.ListFilters.Switches;

public class TrimNameSwitch : IListFilterSwitch
{
    public string Name => "Trim Names";
    public bool IsEnabled { get; set; }
    public void OnSwitch()
    {
        throw new NotImplementedException();
    }
}
