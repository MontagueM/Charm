using System.Collections.Concurrent;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class TagListViewerView : UserControl
{
    public TagListViewerView()
    {
        InitializeComponent();
    }

    public void LoadContent(ETagListType tagListType, FileHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null)
    {
        TagList.LoadContent(tagListType, contentValue, bFromBack, overrideItems);
    }
}
