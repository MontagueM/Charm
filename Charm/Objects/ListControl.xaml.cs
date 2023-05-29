using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Arithmic;
using Internal.Fbx;
using Tiger;
using Tiger.Schema;

namespace Charm.Objects;

public enum ETagListType
{
    [Description("BACK")]
    Back,
    [Description("Package")]
    Package,
    [Description("String Containers List [Packages]")]
    StringContainersList,
    [Description("String Container [Final]")]
    StringContainer,
    [Description("Strings")]
    Strings,
    [Description("String [Final]")]
    String,
}


/// <summary>
/// The current implementation of Package is limited so you cannot have nested views below a Package.
/// For future, would be better to split the tag items up so we can cache them based on parents.
/// </summary>
public partial class ListControl : UserControl
{
    HashSet<ListItem> _allItems = new HashSet<ListItem>();
    private bool _hasLoaded = false;
    public ObservableCollection<ListItem> Items { get; set; } = new ObservableCollection<ListItem>();
    // todo should be moved out into an abstraction which specifically wants to make it so clicking the button causes the file view to load.
    private FileControl? _parentFileControl;
    private Type _typeOfData;

    public ListControl()
    {
        InitializeComponent();
        DataContext = new BaseListViewModel();
    }


    public void Load<T>(FileControl fileControl)
    {
        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;
        _parentFileControl = fileControl;

        LoadAllItems<T>();
    }

    private void LoadAllItems<T>()
    {
        _typeOfData = typeof(T);
        Type typeOfData = NestedTypeHelpers.FindNestedGenericType<T>();
        // Type typeOfData = (Type)typeof(T).BaseType.GetField("TypeOfData", BindingFlags.Static | BindingFlags.Public).GetValue(null);

        _allItems = (HashSet<ListItem>)typeof(ListControl)
            .GetMethod("GetAllItems", BindingFlags.NonPublic | BindingFlags.Static)
            ?.MakeGenericMethod(typeof(ListItem), typeOfData)
            .Invoke(PackageResourcer.Get(), null);

        ItemsList.ItemsSource = new ObservableCollection<ListItem>(_allItems);
    }

    // todo make overridable in different list types
    private static HashSet<ListItem> GetAllItems<TView, TData>() where TView : ListItem
    {
        // TData must be a class of type Tag.

        var allHashes = PackageResourcer.Get().GetAllHashes<TData>();
        return allHashes.Select(hash => (Activator.CreateInstance(typeof(TView), hash) as ListItem)).ToHashSet();
    }


    // public void Load()
    // {
    // }

    // todo this function should be in a routed view model command
    private void ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        typeof(FileControl)
            .GetMethod("LoadFileView", BindingFlags.Public | BindingFlags.Instance)
            ?.MakeGenericMethod(typeof(ListItem), _typeOfData)
            .Invoke(_parentFileControl, new []{(sender as ListBox).SelectedItem});
    }
}
