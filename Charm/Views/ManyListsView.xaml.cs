using System.Windows;
using System.Windows.Controls;
using Charm.Objects;
using Tiger.Schema;

namespace Charm.Views;

// todo this can probably be a ViewModel for a FileControl
public class TextureSingleView : FileControl<TextureViewModel>
{

}

public class TextureGridView : GridListView<TextureViewModel>
{

}

public class StaticSingleView : FileControl<StaticViewModel>
{

}

public class StaticGridView : GridListView<StaticViewModel>
{

}

public partial class ManyListsView : UserControl
{
    public ManyListsView()
    {
        InitializeComponent();
    }

    private void FileControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            ((e.AddedItems[0] as TabItem)?.Content as IControl)?.Load();
        }
    }

    private void ListControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // ((e.AddedItems[0] as TabItem)?.Content as ListControl)?.LoadDataView();
    }
}

