using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Charm.Objects;
using Tiger;

namespace Charm;

public interface IView
{
}

public interface IViewModel<TData> : IViewModel
{
    /// <summary>
    /// Find what view best represents this view model.
    /// This isn't how you're "meant" to use MVVM, but I want to make it so you can click on a list item and it display
    /// a Texture2D if it's a Texture2D, or a cubemap if it's a cubemap, etc.
    /// If you know of a nicer way please change it
    /// </summary>
    /// <returns>the view that best represents this view model.</returns>
    public static abstract UserControl GetView(TData data);

    public static abstract UserControl? DefaultView { get; }
}

public interface IViewModel
{
    /// <summary>
    /// Get all the list items represented by TData.
    /// </summary>
    /// <returns></returns>
    public static HashSet<IListItem> GetListItems<TViewModel>(short packageId = -1) where TViewModel : IViewModel
    {
        // All IViewModel implementations should have a static method that returns all the list items.
        return (HashSet<IListItem>)typeof(TViewModel)
            .GetMethod(nameof(GetListItems), BindingFlags.Static | BindingFlags.Public)
            !.Invoke(null, new object[] { packageId })!;
    }

    protected static abstract HashSet<IListItem> GetListItems(short packageId);

    public static HashSet<IListItem> GetListItemsInternal<TModel, TData>(short packageId)
    {
        HashSet<TigerHash> allHashes = PackageResourcer.Get().GetAllHashes<TData>();
        return allHashes
            // .Take(10000)
            .Select(hash => (Activator.CreateInstance(typeof(TModel), hash, typeof(TData).Name)))
            .Cast<IListItem>()
            .ToHashSet();
    }
}

public interface IModel<TData> where TData : TigerFile
{
    /// <summary>
    /// Given some IListItem which has a hash, get the data from that hash corresponding to TData.
    /// </summary>
    public static TData GetDataFromItem(HashListItemModel item)
    {
        return FileResourcer.Get().GetFile<TData>(item.Hash);
    }
}

public interface IListItem
{
    public bool ShouldFilterKeep(string searchText);

    /// <summary>
    /// Compare to any other list item.
    /// Safe to assume that the other item is of the same type as this one.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(IListItem? other);

    // public bool GetProxyView(out dynamic? proxyView);
}


public interface IControl
{
    /// <summary>
    /// Call when a control should be loaded.
    /// Useful for loading a control only when it is needed - for example, when a tab is selected.
    /// Standard WPF will load every tab and all internals, even if they're not shown.
    /// </summary>
    public void Load();
}
