using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    private Type _listItemType;

    /// <summary>
    /// Type must be an <see cref="AbstractListItem{TData}"/>
    /// </summary>
    [DefaultValue(null)]
    public Type ListItemType
    {
        get { return _listItemType; }
        set
        {
            // if (!IsAssignableToGenericType(value, typeof(AbstractListItem<>)))
            // {
            //     throw new ArgumentException("Type must be an AbstractListItem<Tag>.", nameof(value));
            // }

            _listItemType = value;
        }
    }

    public FileControl()
    {
        InitializeComponent();
    }

    // If we're not already loaded, load the list items
    public void Load()
    {
        // run the above function but with ListItemType as a generic type for Load<T>
        typeof(ListControl)
            .GetMethod("Load")
            ?.MakeGenericMethod(_listItemType)
            .Invoke(ListControl, null);
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
