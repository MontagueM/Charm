using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tiger.Schema;

namespace Charm.Objects;

public class LocalizedStringsListViewModel : GenericListViewModel<LocalizedStrings>
{
    public override HashSet<HashListItemModel> GetAllItems(LocalizedStrings data)
    {
        return data.GetAllStringViews().Select(CreateListItem).ToHashSet();
    }

    public HashListItemModel CreateListItem(LocalizedStringView stringView)
    {
        return new LongTextListItemModel {Hash = stringView.StringHash, Text = stringView.RawString, Type = "Raw Localized String"};
    }
}

public class DefaultListViewModel : BaseListViewModel
{
    public string Title { get; set; } = "Title";
}
