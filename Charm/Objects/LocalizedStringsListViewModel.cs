using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tiger.Schema;

namespace Charm.Objects;

public class LocalizedStringsListViewModel : GenericListViewModel<LocalizedStrings>
{
    public override HashSet<ListItem> GetAllItems(LocalizedStrings data)
    {
        return data.GetAllStringViews().Select(CreateListItem).ToHashSet();
    }

    public ListItem CreateListItem(LocalizedStringView stringView)
    {
        return new ListItem {Hash = stringView.StringHash, Title = stringView.RawString};
    }
}

public class DefaultListViewModel : BaseListViewModel
{

}
