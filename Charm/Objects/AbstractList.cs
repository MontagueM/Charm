using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors.Core;
using Tiger;

namespace Charm.Objects;


public class ListItemModel
{
    public TigerHash Hash { get; set; }
    public string HashString { get => $"[{Hash}]"; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";

    public ListItemModel()
    {
    }

    public ListItemModel(TigerHash hash)
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

    public abstract HashSet<ListItemModel> GetAllItems(TData data);
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


    protected HashSet<ListItemModel> _allItems = new();
    private ObservableCollection<ListItemModel> _items = new();
    public ObservableCollection<ListItemModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    private Type _typeOfData;
    private ListItemModel? _selectedItem;
    public ListItemModel SelectedItem
    {
        get
        {
            return _selectedItem;
        }
        set
        {
            _selectedItem = value;
            if (_selectedItem != null)
            {
                // todo make this generic/virtual, currently just asks FileControl to LoadFileView
                typeof(FileControl)
                    .GetMethod("LoadFileView", BindingFlags.Public | BindingFlags.Instance)
                    ?.MakeGenericMethod(typeof(ListItemModel), _typeOfData)
                    .Invoke(_parentFileControl, new[] {_selectedItem});
            }
        }
    }

    public DataTemplate ItemTemplate
    {
        get
        {
            return new DefaultListItem();
        }
    }

    private FileControl? _parentFileControl;


    /// <summary>
    /// Load view with nothing but a type to fill a list from.
    /// </summary>
    public void LoadView<TView, TData>(FileControl fileControl)
    {
        _typeOfData = typeof(TData);
        _parentFileControl = fileControl;
        _allItems = GetAllItems<TView, TData>();
        RefreshItemList();
    }

    public HashSet<ListItemModel> GetAllItems<TView, TData>()
    {
        // todo packages?
        var allHashes = PackageResourcer.Get().GetAllHashes<TData>();
        return allHashes.Select(hash => (Activator.CreateInstance(typeof(TView), hash) as ListItemModel)).ToHashSet();
    }

    protected void RefreshItemList()
    {
        Task.Run(() => FilterItemList(SearchText.ToLower()));
    }

    private void FilterItemList(string filter)
    {
        ConcurrentBag<ListItemModel> filteredItems = new();
        Parallel.ForEach(_allItems, item =>
        {
            if (item.Title.ToLower().Contains(filter) || item.Hash.ToString().ToLower().Contains(filter))
            {
                filteredItems.Add(item);
            }
        });
        var x = filteredItems.ToList();
        x.Sort((x, y) => x.Hash.CompareTo(y.Hash));
        Dispatcher.CurrentDispatcher.Invoke(() => Items = new ObservableCollection<ListItemModel>(x));
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
