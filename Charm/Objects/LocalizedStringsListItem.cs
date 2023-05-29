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

public class LocalizedStringsList : AbstractList<LocalizedStrings>
{
    public override ObservableCollection<ListItem> GetAllItems(LocalizedStrings data)
    {
        return data.GetAllStringViews().Select(CreateListItem).ToObservableCollection();
    }

    public ListItem CreateListItem(LocalizedStringView stringView)
    {
        return new ListItem {Hash = stringView.StringHash, Title = stringView.RawString};
    }

    public override void OnClick()
    {
    }
}
