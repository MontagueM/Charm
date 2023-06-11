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


public class HashListItemModel
{
    public TigerHash Hash { get; set; } = new();
    public string HashString { get => $"[{Hash}]"; }
    public string Type { get; set; } = "Type";

    public HashListItemModel()
    {
    }

    public HashListItemModel(TigerHash hash, string typeName)
    {
        Hash = hash;

        // if multiple capital letters in typeName e.g. 'LocalizedStrings', split to 'Localized Strings'
        if (typeName.Length > 1 && typeName.Skip(1).Any(char.IsUpper))
        {
            Type = string.Concat(typeName.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
        else
        {
            Type = typeName;
        }
    }

    public virtual bool ShouldFilterKeep(string searchText)
    {
        return HashString.Contains(searchText, StringComparison.OrdinalIgnoreCase) || Type.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}

public class TitleListItemModel : HashListItemModel
{
    public string Title { get; set; } = "Title";

    public override bool ShouldFilterKeep(string searchText)
    {
        return base.ShouldFilterKeep(searchText) || Title.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}

public class TitleSubtitleListItemModel : TitleListItemModel
{
    public string Subtitle { get; set; } = "Subtitle";

    public override bool ShouldFilterKeep(string searchText)
    {
        return base.ShouldFilterKeep(searchText) || Subtitle.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }
}

public class LongTextListItemModel : HashListItemModel
{
    public string Text { get; set; } = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris";

    public override bool ShouldFilterKeep(string searchText)
    {
        return base.ShouldFilterKeep(searchText) || Text.Contains(searchText, StringComparison.OrdinalIgnoreCase);
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

    public abstract HashSet<HashListItemModel> GetAllItems(TData data);
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


    protected HashSet<HashListItemModel> _allItems = new();
    private ObservableCollection<HashListItemModel> _items = new();
    public ObservableCollection<HashListItemModel> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    private Type _typeOfData;
    private HashListItemModel? _selectedItem;
    public HashListItemModel SelectedItem
    {
        get
        {
            return _selectedItem;
        }
        set
        {
            _selectedItem = value;
            if (_selectedItem != null && _parentFileControl != null)
            {
                // todo make this generic/virtual, currently just asks FileControl to LoadFileView
                typeof(FileControl)
                    .GetMethod("LoadFileView", BindingFlags.Public | BindingFlags.Instance)
                    ?.MakeGenericMethod(typeof(HashListItemModel), _typeOfData)
                    .Invoke(_parentFileControl, new[] {_selectedItem});
            }
        }
    }

    public DataTemplate ItemTemplate
    {
        get
        {
            // var x = new DefaultListItem();
            // x.DataType = typeof(HashListItemModel);
            return ConvertUserControlToDataTemplate<DefaultListItemTemplate>();
        }
    }

    public DataTemplate ListItemTemplate { get; }

    public DataTemplate ConvertUserControlToDataTemplate<T>()
    {
        DataTemplate dataTemplate = new() { VisualTree = new FrameworkElementFactory(typeof(T)) };
        dataTemplate.Seal();

        return dataTemplate;
    }

    private FileControl? _parentFileControl;


    /// <summary>
    /// Load view with nothing but a type to fill a list from.
    /// </summary>
    public void LoadView<TView, TData>(FileControl fileControl)
    {
        LoadView(fileControl, typeof(TView), typeof(TData));
    }

    public void LoadView(FileControl fileControl, Type viewType, Type dataType)
    {
        _parentFileControl = fileControl;
        LoadView(viewType, dataType);
    }

    public void LoadView(Type viewType, Type dataType)
    {
        _typeOfData = dataType;
        _allItems = GetAllHashItems(viewType, dataType);
        RefreshItemList();
    }

    public void LoadDataView(Type viewType, Type dataType)
    {
        _typeOfData = dataType;
        _allItems = GetAllDataItems(viewType, dataType);
        RefreshItemList();
    }

    /// <summary>
    /// Gets all items only based on the hash of that item; used for viewing a list of hashes which can be clicked on to view the data.
    /// </summary>
    public HashSet<HashListItemModel> GetAllHashItems(Type viewType, Type dataType)
    {
        // todo packages?
        HashSet<TigerHash> allHashes = PackageResourcer.Get().GetAllHashes(dataType);
        return allHashes.Select(hash => (Activator.CreateInstance(viewType, hash, dataType.Name) as HashListItemModel)).ToHashSet();
    }

    /// <summary>
    /// Gets all the data of every hash; much slower than <see cref="GetAllHashItems"/> but allows for filtering based on the data.
    /// </summary>
    public HashSet<HashListItemModel> GetAllDataItems(Type viewType, Type dataType)
    {
        HashSet<TigerFile> allFiles = PackageResourcer.Get().GetAllFiles(dataType);
        var allItems = allFiles
            .Select(file =>
                viewType.GetMethod("GetAllItems", BindingFlags.Public | BindingFlags.Instance)
                    .Invoke(Activator.CreateInstance(viewType), new[] {file})
            ).Cast<HashSet<HashListItemModel>>().SelectMany(x => x).ToHashSet();
        return allItems;
    }

    protected void RefreshItemList()
    {
        Task.Run(() => FilterItemList(SearchText.ToLower()));
    }

    private void FilterItemList(string filter)
    {
        ConcurrentBag<HashListItemModel> filteredItems = new();
        Parallel.ForEach(_allItems, item =>
        {
            if (item.ShouldFilterKeep(filter))
            {
                filteredItems.Add(item);
            }
        });
        var x = filteredItems.ToList();
        x.Sort((x, y) => x.Hash.CompareTo(y.Hash));
        Dispatcher.CurrentDispatcher.Invoke(() => Items = new ObservableCollection<HashListItemModel>(x));
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
