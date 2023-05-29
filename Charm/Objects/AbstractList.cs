using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors.Core;
using Tiger;

namespace Charm.Objects;


public class ListItem
{
    public TigerHash Hash { get; set; }
    public string HashString { get => $"[{Hash}]"; }
    public string Title { get; set; }
    public string Subtitle { get; set; }

    public ListItem()
    {
    }

    public ListItem(TigerHash hash)
    {
        Hash = hash;
    }
}

public interface IAbstractListItem<TData>
{
    public void Initialise(TigerFile file);
    public void OnClick();
}

public static class NestedTypeHelpers
{
    public static Type? FindNestedGenericType<T>()
    {
        Type? nestedType = null;

        Type testType = typeof(T);
        while (nestedType == null && testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType)
            {
                nestedType = testType.GenericTypeArguments[0];
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return nestedType;
    }

    public static Type? GetNonGenericParent(this Type inTestType, Type inheritParentType)
    {
        Type? testType = inTestType;
        while (testType != null && testType != typeof(object))
        {
            if (testType.IsGenericType && testType.GenericTypeArguments.Length > 0 && testType.GetGenericTypeDefinition() == inheritParentType)
            {
                return testType;
            }
            else
            {
                testType = testType.BaseType;
            }
        }

        return null;
    }
}

public interface IAbstractFileView<in TView, in TData>
{
    public abstract void LoadView(TData dataToView);
}

/// <summary>
/// Takes some data of type <see cref="TData"/> and generates a display item for a list based on the data given.
/// On click, it returns a routed  <see cref="TView"/> and populates it with the data.
/// </summary>
/// <typeparam name="TData">The type of data type to use for generating the information of this item. Must be a schema struct or class of type <see cref="Tag"/>.</typeparam>
/// <typeparam name="TView">The type of view this item opens and populates on click. Must be of type <see cref="IAbstractFileView{TData}"/>.</typeparam>
public abstract class GenericListViewModel<TData> : BaseListViewModel, IAbstractFileView<ListControl, TData>
{
    public void LoadView(TData dataToView)
    {
        _allItems = GetAllItems(dataToView);
        RefreshItemList();
    }

    public abstract HashSet<ListItem> GetAllItems(TData data);
}


public class BaseListViewModel : BaseViewModel
{
    public string _searchText = "";
    public string SearchText
    {
        get
        {
            return _searchText;
        }
        set
        {
            _searchText = value;
            RefreshItemList();
        }
    }

    public BaseListViewModel()
    {
    }

    protected HashSet<ListItem> _allItems = new();
    private ObservableCollection<ListItem> _items = new();
    public ObservableCollection<ListItem> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    protected void RefreshItemList()
    {
        var x = SearchText.ToLower();
        Task.Run(() => FilterItemList(x));
    }

    private void FilterItemList(string filter)
    {
        ConcurrentBag<ListItem> filteredItems = new();
        Parallel.ForEach(_allItems, item =>
        {
            if (item.Title.ToLower().Contains(filter) || item.Hash.ToString().ToLower().Contains(filter))
            {
                filteredItems.Add(item);
            }
        });
        var x = filteredItems.ToList();
        x.Sort((x, y) => x.Hash.CompareTo(y.Hash));
        Dispatcher.CurrentDispatcher.Invoke(() => Items = new ObservableCollection<ListItem>(x));
    }
}


public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string memberName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName));
    }
}
