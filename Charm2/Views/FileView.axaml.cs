using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Charm.ViewModels;
using Tiger;
using Tiger.Schema;

namespace Charm.Views;

/// <summary>
///
/// </summary>
// /// <typeparam name="TItem">The type of item that the list presents.</typeparam>
/// <typeparam name="TView">The type of view that is populated after an item has been clicked.</typeparam>
public partial class FileView<TView> : UserControl where TView : UserControl, new()
{
    public void Load(ListView listView, IEnumerable<StringFileListItem> listItems, ContentPresenter contentPresenter)
    {
        // listView.Populate(listItems);
        contentPresenter.Content = new TView();
    }
}

public interface IListItem
{
    public string Name { get; }
    public string Description { get; }
}

public struct ListItem : IListItem
{
    public string Name { get; }
    public string Description { get; }

    public ListItem(string name, string description)
    {
        Name = name;
        Description = description;
    }
}

public struct FileListItem : IListItem
{
    public string Name { get; }
    public string Description { get; }

    public FileListItem(FileHash hash)
    {
        Name = hash.ToString();
    }
}

public struct StringFileListItem : IListItem
{
    public string Name { get; }
    public string Description { get; }

    public StringFileListItem(TigerString tigerString)
    {
        Name = tigerString.RawValue;
        Description = tigerString.Hash.ToString();
    }
}


internal partial class FileView : FileView<ListView>
{
    public FileView()
    {
        InitializeComponent();

        List<StringFileListItem> listItems = new();
        listItems.Add(new StringFileListItem(new TigerString(new StringHash(123), "Hello, world!")));
        listItems.Add(new StringFileListItem(new TigerString(new StringHash(564535), "lorem ipsum dolor est")));

        // todo add custom sorts?
        Load(ListControl, listItems, FileContentPresenter);
    }

    public void Load(IEnumerable<StringFileListItem> items)
    {
        Load(ListControl, items, FileContentPresenter);
    }
}
