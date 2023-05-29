using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Tiger.Schema;

namespace Charm.Objects;

public static class EnumerableExtensions
{
    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
    {
        return new ObservableCollection<T>(source);
    }
}

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

    // public override void OnClick()
    // {
    // }
}
