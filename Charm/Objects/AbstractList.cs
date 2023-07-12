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
using Arithmic;
using Microsoft.Xaml.Behaviors.Core;
using Tiger;
using Tiger.Schema;

namespace Charm.Objects;


public abstract class HashListItemModel : IListItem
{
    public TigerHash Hash { get; set; } = new();
    public string HashString { get => $"[{Hash}]"; }

    private string _type = "Type";
    public string Type
    {
        get { return _type; }
        set { SetField(ref _type, value); }
    }

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

    // public abstract void Load();

    // todo really this should be in viewmodel instead

    public virtual bool ShouldFilterKeep(string searchText)
    {
        return HashString.Contains(searchText, StringComparison.OrdinalIgnoreCase) || Type.Contains(searchText, StringComparison.OrdinalIgnoreCase);
    }

    // public virtual bool GetProxyView(out dynamic? proxyView)
    // {
    //     proxyView = null;
    //     return false;
    // }

    public int CompareTo(IListItem? other) => Hash.CompareTo((other as HashListItemModel)?.Hash);
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// public class ControlListItemModel<TModel> : HashListItemModel where TModel : TigerFile
// {
//     public ControlListItemModel(TigerHash hash, string typeName) : base(hash, typeName)
//     {
//         TModel file = FileResourcer.Get().GetFile<TModel>(hash);
//         if (file != null)
//         {
//             Control = TextureViewModel.GetView(file as Texture);
//         }
//     }
//
//     public UserControl? Control { get; set; }
//
//     public override bool GetProxyView(out dynamic? proxyView)
//     {
//         proxyView = Control;
//         return proxyView != null;
//     }
// }

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
        // _allItems = GetAllItems(dataToView);
        RefreshItemList();
    }

    public abstract HashSet<HashListItemModel> GetAllItems(TData data);
}

// public abstract class GenericSingleItemViewModel<TData> : BaseItemViewModel, IAbstractFileView<SingleItemControl, TData>
// {
//     public void LoadView(TData dataToView)
//     {
//         Data = dataToView;
//     }
//
//     public TData Data { get; set; } = default!;
// }

// public class BaseItemViewModel : BaseViewModel
// {
//
// }

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


    protected HashSet<IListItem> _allItems = new();
    private ObservableCollection<dynamic?> _items = new();
    public ObservableCollection<dynamic?> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    private Type _typeOfData;
    private IListItem? _selectedItem;
    public IListItem SelectedItem
    {
        get
        {
            return _selectedItem;
        }
        set
        {
            _selectedItem = value;
            if (_selectedItem != null && _onListItemClicked != null)
            {
                // todo make this generic/virtual, currently just asks FileControl to LoadFileView
                bool processedCorrectly = _onListItemClicked.Invoke(_selectedItem);
                if (processedCorrectly == false)
                {
                    Log.Error($"Failed to process click on {_selectedItem}");
                }
                else
                {
                    Log.Verbose($"Processed click on {_selectedItem}");
                }
                // typeof(FileControl)
                //     .GetMethod("LoadFileView", BindingFlags.Public | BindingFlags.Instance)
                //     ?.MakeGenericMethod(typeof(HashListItemModel), _typeOfData)
                //     .Invoke(_parentFileControl, new[] {_selectedItem});
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

    private OnListItemClicked? _onListItemClicked;


    /// <summary>
    /// Load view with nothing but a type to fill a list from.
    /// </summary>
    public void LoadView<TViewModel>(OnListItemClicked onListItemClicked) where TViewModel : IViewModel
    {
        _onListItemClicked = onListItemClicked;
        LoadView<TViewModel>();
    }

    // public void LoadView(FileControl fileControl, Type viewType, Type dataType)
    // {
    //     _parentFileControl = fileControl;
    //     LoadView(viewType, dataType);
    // }

    // todo might need some method that makes _onListItemClicked null again

    private void LoadView<TViewModel>() where TViewModel : IViewModel
    {
        _allItems = IViewModel.GetListItems<TViewModel>();
        RefreshItemList();
    }

    public void LoadDataView(Type viewType, Type dataType)
    {
        // _typeOfData = dataType;
        // _allItems = GetAllDataItems(viewType, dataType);
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
        ConcurrentBag<IListItem> filteredItems = new();
        Parallel.ForEach(_allItems, item =>
        {
            if (item.ShouldFilterKeep(filter))
            {
                filteredItems.Add(item);
            }
        });
        var x = filteredItems.ToList();
        x.Sort((x, y) => x.CompareTo(y));
        var y = x.Cast<dynamic?>().ToList();
        // for (var i = 0; i < y.Count; i++)
        // {
        //     // todo can probably be an O(1) operation as all items are the same
        //     if (y[i].GetProxyView(out dynamic? proxyView))
        //     {
        //         y[i] = proxyView;
        //     }
        //
        // }
        Dispatcher.CurrentDispatcher.Invoke(() => Items = new ObservableCollection<dynamic?>(y));
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
