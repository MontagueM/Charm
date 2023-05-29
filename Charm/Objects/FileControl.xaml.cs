using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Tiger;
using Tiger.Schema;

namespace Charm.Objects;

/// <summary>
///
/// </summary>
// /// <typeparam name="TItem">The type of item that the list presents.</typeparam>
/// <typeparam name="TView">The type of view that is populated after an item has been clicked.</typeparam>
public partial class FileControl : UserControl
{
    // private Type _listItemType;

    public Type ListItemType { get; set; }
    // {
    //     get { return _listItemType; }
    //     set
    //     {
    //         // if (!IsAssignableToGenericType(value, typeof(AbstractList<>)))
    //         // {
    //         //     throw new ArgumentException("Type must be an AbstractList<Tag>.", nameof(value));
    //         // }
    //
    //         _listItemType = value;
    //     }
    // }
    private bool _hasLoaded = false;

    private dynamic fileViewModel;

    public Type ViewType { get; set; }

    public FileControl()
    {
        InitializeComponent();
    }

    // If we're not already loaded, load the list items and prepare the list view to use the file view via dependency injection.
    public void Load()
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;

        // Presuming ViewType is a ViewModel, we need to find what UserControl type it is
        // Find the IAbstractFileView interface and get TView from it
        Type typeOfControl = ViewType.GetInterfaces()
            .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAbstractFileView<,>))
            .GenericTypeArguments[0];

        fileViewModel = Activator.CreateInstance(ViewType);

        // todo should be UserControl not ListControl
        ListControl fileView = (ListControl) Activator.CreateInstance(typeOfControl);
        fileView.DataContext = fileViewModel;
        fileView.ItemsList.DataContext = fileViewModel;

        FileContentPresenter.Content = fileView;

        // Load list items
        typeof(ListControl)
            .GetMethod("Load")
            ?.MakeGenericMethod(ListItemType)
            .Invoke(ListControl, new object[] { this });
    }

    public void LoadFileView<TView, TData>(TView data) where TView : ListItem where TData : TigerFile
    {
        fileViewModel.LoadView(FileResourcer.Get().GetFile<TData>(data.Hash));
    }

    public static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        foreach (var it in interfaceTypes)
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
                return true;
        }

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        Type baseType = givenType.BaseType;
        if (baseType == null) return false;

        return IsAssignableToGenericType(baseType, genericType);
    }

    // public void Load(ListControl listView, IEnumerable<dynamic> listItems, ContentPresenter contentPresenter)
    // {
    //     // listView.Populate(listItems);
    //     contentPresenter.Content = new TView();
    // }
}


// public struct ListItem : IListItem
// {
//     public string Name { get; }
//     public string Description { get; }
//
//     public ListItem(string name, string description)
//     {
//         Name = name;
//         Description = description;
//     }
// }
//
// public struct FileListItem : IListItem
// {
//     public string Name { get; }
//     public string Description { get; }
//
//     public FileListItem(FileHash hash)
//     {
//         Name = hash.ToString();
//     }
// }
//
// public struct StringFileListItem : IListItem
// {
//     public string Name { get; }
//     public string Description { get; }
//
//     public StringFileListItem(TigerString tigerString)
//     {
//         Name = tigerString.RawValue;
//         Description = tigerString.Hash.ToString();
//     }
// }
