using System.Windows;
using System.Windows.Controls;
using Charm.Objects;

namespace Charm.Views;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TabItem))]
[TemplatePart(Name = "PART_SelectedContentHost", Type = typeof(ContentPresenter))]
public partial class CharmTabControl : TabControl
{
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        ((e.AddedItems[0] as TabItem)?.Content as FileControl)?.Load();
    }
}

public partial class ManyListsView : UserControl
{
    public ManyListsView()
    {
        InitializeComponent();
    }
}

