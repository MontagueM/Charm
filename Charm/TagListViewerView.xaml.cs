using System.Collections.Concurrent;
using System.Windows.Controls;
using Field.General;

namespace Charm;

public partial class TagListViewerView : UserControl
{
    public TagListViewerView()
    {
        InitializeComponent();
    }

    public void LoadContent(ETagListType tagListType, TagHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null)
    {
        TagList.LoadContent(tagListType, contentValue, bFromBack, overrideItems);
    }
}