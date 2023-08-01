using System.Collections.Concurrent;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tiger;

namespace CharmAvalonia;

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

