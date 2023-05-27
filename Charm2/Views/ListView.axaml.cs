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
using Arithmic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Charm.ViewModels;
using HarfBuzzSharp;
using Internal.Fbx;
using Splat;
using Tiger;
using Tiger.Schema;

namespace Charm.Views;

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
public partial class ListView : UserControl
{
    private struct ParentInfo
    {
        public ETagListType TagListType;
        public FileHash? Hash;
        public string SearchTerm;
        public ConcurrentBag<TagItem> AllTagItems;
    }

    private ConcurrentBag<TagItem> _allTagItems;
    // private static MainWindow _mainWindow = null;
    // public IEnumerable<IListItem> ListItems { get; set; }

    private ETagListType _tagListType;
    private FileHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new Stack<ParentInfo>();
    private bool _bTrimName = true;
    private bool _bShowNamedOnly = false;
    // private readonly ILogger _tagListLogger = Log.ForContext<TagListView>();
    private ListView _tagListControl = null;
    private ToggleButton _previouslySelected = null;
    private int _selectedIndex = -1;
    // private FbxHandler _globalFbxHandler = null;
    public ListViewModel ViewModel => DataContext as ListViewModel;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        // _mainWindow = Window.GetWindow(this) as MainWindow;
        // _globalFbxHandler = new FbxHandler(false);
    }

    public ListView()
    {
        InitializeComponent();
        DataContext = new ListViewModel();
    }

    public void Populate(IEnumerable<StringFileListItem> listItems)
    {
        // TagList.DataContext = listItems;
        // ObservableCollection<TItem> observableCollection = new(listItems);
        // // TagList.Items.Clear();
        // TagList.ItemsSource = observableCollection;
        // ListBox.ItemsSource = listItems;
    }

    // private TagView GetViewer()
    // {
    //     // if (Parent is Grid)
    //     // {
    //     //     if ((Parent as Grid).Parent is TagListViewerView)
    //     //         return ((Parent as Grid).Parent as TagListViewerView).TagView;
    //     //     else if ((Parent as Grid).Parent is TagView)
    //     //         return (Parent as Grid).Parent as TagView;
    //     // }
    //     // _tagListLogger.Error($"Parent is not a TagListViewerView, is {Parent.GetType().Name}.");
    //     // return null;
    // }

    // public async void LoadContent(ETagListType tagListType, FileHash contentValue = null, bool bFromBack = false,
    //     ConcurrentBag<TagItem> overrideItems = null)
    // {
    //     Log.Info($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    //     if (overrideItems != null)
    //     {
    //         _allTagItems = overrideItems;
    //     }
    //     else
    //     {
    //         if (contentValue != null && !bFromBack && !TagItem.GetEnumDescription(tagListType).Contains("[Final]")) // if the type nests no new info, it isnt a parent
    //         {
    //             _parentStack.Push(new ParentInfo
    //             {
    //                 AllTagItems = _allTagItems, Hash = _currentHash, TagListType = _tagListType,
    //                 SearchTerm = SearchBox.Text
    //             });
    //         }
    //
    //         switch (tagListType)
    //         {
    //             case ETagListType.StringContainersList:
    //                 LoadStringContainersList();
    //                 break;
    //             case ETagListType.StringContainer:
    //                 LoadStringContainer(contentValue);
    //                 break;
    //             case ETagListType.Strings:
    //                 LoadStrings(contentValue);
    //                 break;
    //             case ETagListType.String:
    //                 break;
    //             default:
    //                 throw new NotImplementedException();
    //         }
    //     }
    //
    //     if (!TagItem.GetEnumDescription(tagListType).Contains("[Final]"))
    //     {
    //         _currentHash = contentValue;
    //         _tagListType = tagListType;
    //         if (!bFromBack)
    //         {
    //             SearchBox.Text = "";
    //         }
    //
    //         RefreshItemList();
    //     }
    //
    //     Log.Info(
    //         $"Loaded content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    // }

    /// <summary>
    /// For when we want stuff in packages, we then split up based on what the taghash value is.
    /// I kinda cheat here, I store everything in one massive _allTagItems including the packages
    /// </summary>
    /// <param name="packageId">Package ID for this package to load data for.</param>
    private void LoadPackage(FileHash pkgHash)
    {
        int pkgId = pkgHash.PackageId;
        SetBulkGroup(pkgId.ToString("x4"));
        _allTagItems = new ConcurrentBag<TagItem>(_allTagItems.Where(x => x.Hash.PackageId == pkgId && x.TagType != ETagListType.Package));
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // if ((e.Key == Key.Down || e.Key == Key.Right))
        // {
        //     // find the selected one
        //     List<TagItem> tagItems = TagList.Items.OfType<TagItem>().ToList();
        //     var selected = tagItems.FirstOrDefault(x => x.IsChecked);
        //     if (selected != null)
        //     {
        //         int index = tagItems.IndexOf(selected);
        //         var z = TagList.ItemContainerGenerator.ContainerFromIndex(index);
        //         var w = GetChildOfType<ToggleButton>(z);
        //         w.IsChecked = false;
        //         var x = TagList.ItemContainerGenerator.ContainerFromIndex(index+1);
        //         var y = GetChildOfType<ToggleButton>(x);
        //         y.IsChecked = true;
        //     }
        //     var item = TagList.SelectedItem;
        //     var a = 0;
        // }
    }

    private void SetItemListByString(string searchStr, bool bPackageSearchAllOverride = false)
    {
        if (_allTagItems == null)
            return;
        if (_allTagItems.IsEmpty)
            return;

        bool bShowTrimCheckbox = false;
        bool bNoName = false;
        bool bName = false;

        var displayItems = new ConcurrentBag<TagItem>();
        // Select and sort by relevance to selected string
        Parallel.ForEach(_allTagItems, item =>
        {
            if (item.Name.Contains('\\'))
                bShowTrimCheckbox = true;
            if (item.Name == String.Empty)
                bNoName = true;
            if (item.Name != String.Empty)
                bName = true;

            if (_bShowNamedOnly && item.Name == String.Empty)
            {
                return;
            }

            if (!TagItem.GetEnumDescription(_tagListType).Contains("List"))
            {
                if (displayItems.Count > 50) return;

            }
            else if (TagItem.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
            {
                // Package-enabled lists have [Packages] in their enum
                if (item.TagType != ETagListType.Package)
                {
                    return;
                }
            }
            string name = item.Name;
            bool bWasTrimmed = false;
            if (item.Name.Contains("\\") && _bTrimName)
            {
                name = TrimName(name);
                bWasTrimmed = true;
            }
            // bool bWasTrimmed = name != item.Name;
            if (name.ToLower().Contains(searchStr)
                || item.Hash.ToString().ToLower().Contains(searchStr)
                || item.Hash.ToString().Contains(searchStr)
                || item.Subname.ToLower().Contains(searchStr))
            {
                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    Subname = item.Subname,
                    FontSize = _bTrimName || !bWasTrimmed ? 16 : 12,
                });
            }
        });

        // Check if trim names and filter named should be visible (if there any named items)
        // TrimCheckbox.Visibility = bShowTrimCheckbox ? Visibility.Visible : Visibility.Hidden;
        // ShowNamedCheckbox.Visibility = bName && bNoName ? Visibility.Visible : Visibility.Hidden;

        if (bNoName)
        {
            _bShowNamedOnly = false;
        }

        if (displayItems.Count == 0 && TagItem.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
        {
            SetItemListByString(searchStr, true);
            return;
        }

        List<TagItem> tagItems = displayItems.ToList();
        tagItems.Sort((p, q) => String.Compare(p.Name, q.Name, StringComparison.OrdinalIgnoreCase));
        tagItems = tagItems.DistinctBy(t => t.Hash).ToList();
        // If we have a parent, add a TagItem that is actually a back button as first
        if (_parentStack.Count > 0)
        {
            tagItems.Insert(0, new TagItem
            {
                Name = "BACK",
                TagType = ETagListType.Back,
                FontSize = 24
            });
        }

        // TagList.ItemsSource = tagItems;
    }

    /// <summary>
    /// From all the existing items in _allTagItems, we generate the packages for it
    /// and add but only if packages don't exist already.
    /// </summary>
    private void MakePackageTagItems()
    {
        // ConcurrentHashSet<int> packageIds = new ConcurrentHashSet<int>();
        // bool bBroken = false;
        // Parallel.ForEach(_allTagItems, (item, state) =>
        // {
        //     if (item.TagType == ETagListType.Package)
        //     {
        //         bBroken = true;
        //         state.Break();
        //     }
        //     packageIds.Add(item.Hash.GetPkgId());
        // });
        //
        // if (bBroken)
        //     return;
        //
        // Parallel.ForEach(packageIds, pkgId =>
        // {
        //     _allTagItems.Add(new TagItem
        //     {
        //         Name = String.Join('_', PackageHandler.GetPackageName(pkgId).Split('_').Skip(1).SkipLast(1)),
        //         Hash = new TagHash(PackageHandler.MakeHash(pkgId, 0)),
        //         TagType = ETagListType.Package
        //     });
        // });
    }

    private void RefreshItemList()
    {
        // SetItemListByString(SearchBox.Text.ToLower());
    }

    // private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    // {
    //     RefreshItemList();
    // }

    /// <summary>
    /// This onclick is used by all the different types.
    /// </summary>
    private void TagItem_OnClick(object sender, RoutedEventArgs e)
    {
        // var btn = sender as ToggleButton;
        // TagItem tagItem = btn.DataContext as TagItem;
        // TagHash tagHash = tagItem.Hash == null ? null : new TagHash(tagItem.Hash);
        // if (_previouslySelected != null)
        //     _previouslySelected.IsChecked = false;
        // _selectedIndex = TagList.Items.IndexOf(tagItem);
        // // if (_previouslySelected == btn)
        // // _previouslySelected.IsChecked = !_previouslySelected.IsChecked;
        // _previouslySelected = btn;
        // LoadContent(tagItem.TagType, tagHash);
    }

    // public static T GetChildOfType<T>(DependencyObject depObj)
    //     where T : DependencyObject
    // {
    //     if (depObj == null) return null;
    //
    //     for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
    //     {
    //         var child = VisualTreeHelper.GetChild(depObj, i);
    //
    //         var result = (child as T) ?? GetChildOfType<T>(child);
    //         if (result != null) return result;
    //     }
    //     return null;
    // }
    //
    // public static List<T> GetChildrenOfType<T>(DependencyObject depObj)
    //     where T : DependencyObject
    // {
    //     var children = new List<T>();
    //     if (depObj == null) return children;
    //
    //     for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
    //     {
    //         var child = VisualTreeHelper.GetChild(depObj, i);
    //
    //         if (child is T)
    //         {
    //             children.Add(child as T);
    //         }
    //         else
    //         {
    //             children.AddRange(GetChildrenOfType<T>(child));
    //         }
    //     }
    //     return children;
    // }

    /// <summary>
    /// Use the ParentInfo to go back to previous tag data.
    /// </summary>
    private void Back_Clicked()
    {
        // ParentInfo parentInfo = _parentStack.Pop();
        // SearchBox.Text = parentInfo.SearchTerm;
        // LoadContent(parentInfo.TagListType, parentInfo.Hash, true, parentInfo.AllTagItems);
    }

    private void TrimCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = true;
        RefreshItemList();
    }

    private void TrimCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = false;
        RefreshItemList();
    }

    private string TrimName(string name)
    {
        return name.Split("\\").Last().Split(".")[0];
    }

    private void ShowNamedCheckbox_OnChecked(object sender, RoutedEventArgs e)
    {
        _bShowNamedOnly = true;
        RefreshItemList();
    }

    private void ShowNamedCheckbox_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _bShowNamedOnly = false;
        RefreshItemList();
    }

    /// <summary>
    /// We only allow one viewer visible at a time, so setting the viewer hides the rest.
    /// </summary>
    /// <param name="eViewerType">Viewer type to set visible.</param>
    // private void SetViewer(TagView.EViewerType eViewerType)
    // {
    //     var viewer = GetViewer();
    //     viewer.SetViewer(eViewerType);
    // }

    private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectedIndex == -1)
            return;
        // if (TagList.SelectedIndex > _selectedIndex)
        // {
        //     var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
        //     if (currentButton == null)
        //         return;
        //     currentButton.IsChecked = false;
        //     var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex+1));
        //     if (nextButton == null)
        //         return;
        //     nextButton.IsChecked = true;
        //     _selectedIndex++;
        //     TagItem_OnClick(nextButton, null);
        // }
        //
        // else if (TagList.SelectedIndex < _selectedIndex)
        // {
        //     var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
        //     if (currentButton == null)
        //         return;
        //     currentButton.IsChecked = false;
        //     var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex-1));
        //     if (nextButton == null)
        //         return;
        //     nextButton.IsChecked = true;
        //     _selectedIndex--;
        //     TagItem_OnClick(nextButton, null);
        //
        // }
    }

    public void ShowBulkExportButton()
    {
        // BulkExportButton.Visibility = Visibility.Visible;
    }

    public void SetBulkGroup(string group)
    {
        // var tab = ((Parent as Grid).Parent as TagListViewerView).Parent as TabItem;
        // BulkExportButton.Tag = $"{group}_{tab.Header}";
    }

    private async void BulkExport_OnClick(object sender, RoutedEventArgs e)
    {
        // if (BulkExportButton.Tag == null)
        // {
        //     return;
        // }
        //
        // var groupName = BulkExportButton.Tag as string;
        // var viewer = GetViewer();
        // bool bStaticShowing = viewer.StaticControl.Visibility == Visibility.Visible;
        // bool bEntityShowing = viewer.EntityControl.Visibility == Visibility.Visible;
        // viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Hidden : viewer.StaticControl.Visibility;
        // viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Hidden : viewer.EntityControl.Visibility;
        //
        // // Iterate over all buttons and export it
        // var items = TagList.ItemsSource.Cast<TagItem>();
        // var exportItems = items.Where(x => x.TagType != ETagListType.Back && x.TagType != ETagListType.Package).ToList();
        // if (exportItems.Count == 0)
        // {
        //     MessageBox.Show("No tags to export.");
        //     return;
        // }
        // MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i+1}/{exportItems.Count}: {x.Hash}").ToList());
        // await Task.Run(() =>
        // {
        //     foreach (var tagItem in exportItems)
        //     {
        //         var name = tagItem.Name == String.Empty ? tagItem.Hash.GetHashString() : tagItem.Name;
        //         var exportInfo = new ExportInfo
        //         {
        //             Hash = tagItem.Hash,
        //             Name = $"/Bulk_{groupName}/{name}",
        //             ExportType = EExportTypeFlag.Minimal
        //         };
        //         viewer.ExportControl.RoutedFunction(exportInfo);
        //         MainWindow.Progress.CompleteStage();
        //     }
        // });
        // viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Visible : viewer.StaticControl.Visibility;
        // viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Visible : viewer.EntityControl.Visibility;
    }
}

public class TagItem
{
    private string _type = String.Empty;
    private string _name = String.Empty;

    public string Name
    {
        get => _name; set => _name = value;
    }

    public string Subname { get; set; } = String.Empty;

    public FileHash Hash { get; set; }

    public string HashString
    {
        get
        {
            if (Name == "BACK")
                return "";
            // if (TagType == ETagListType.ApiEntity)
                // return $"[{Hash}]";
            if (TagType == ETagListType.Package)
                return $"[{Hash.PackageId:X4}]";
            return $"[{Hash:X8}]";
        }
    }

    public int FontSize { get; set; } = 16;

    public string Type
    {
        get
        {
            if (_type == String.Empty)
            {
                var t = GetEnumDescription(TagType);
                if (t.Contains("[Final]"))
                    return t.Split("[Final]")[0].Trim();
                return t;
            }
            return _type;
        }
        set => _type = value;
    }

    public ETagListType TagType { get; set; }

    public static string GetEnumDescription(Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());

        DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        if (attributes != null && attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }
}

