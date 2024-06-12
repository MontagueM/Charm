using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Tiger.Schema.Strings;
using ActivityROI = Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON.Activity;
using ActivityWQ = Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402.Activity;

namespace Charm;

public enum ETagListType
{
    [Description("None")]
    None,
    [Description("Destination Global Tag Bag List")]
    DestinationGlobalTagBagList,
    [Description("Destination Global Tag Bag")]
    DestinationGlobalTagBag,
    [Description("Budget Set")]
    BudgetSet,
    [Description("Entity [Final]")]
    Entity,
    [Description("BACK")]
    Back,
    [Description("Api List")]
    ApiList,
    [Description("Api Entity [Final]")]
    ApiEntity,
    [Description("Entity List [Packages]")]
    EntityList,
    [Description("Package")]
    Package,
    [Description("Activity List")]
    ActivityList,
    [Description("Activity [Final]")]
    Activity,
    [Description("Statics List [Packages]")]
    StaticsList,
    [Description("Static [Final]")]
    Static,
    [Description("Texture List [Packages]")]
    TextureList,
    [Description("Texture [Final]")]
    Texture,
    [Description("Dialogue List")]
    DialogueList,
    [Description("Dialogue [Final]")]
    Dialogue,
    [Description("Directive List")]
    DirectiveList,
    [Description("Directive [Final]")]
    Directive,
    [Description("String Containers List [Packages]")]
    StringContainersList,
    [Description("String Container [Final]")]
    StringContainer,
    [Description("Strings")]
    Strings,
    [Description("String [Final]")]
    String,
    [Description("Sounds Packages List")]
    SoundsPackagesList,
    [Description("Sounds Package [Final]")]
    SoundsPackage,
    [Description("Sounds List")]
    SoundsList,
    [Description("Sound [Final]")]
    Sound,
    [Description("Music List")]
    MusicList,
    [Description("Music [Final]")]
    Music,
    [Description("Weapon Audio Group List")]
    WeaponAudioGroupList,
    [Description("Weapon Audio Group [Final]")]
    WeaponAudioGroup,
    [Description("Weapon Audio List")]
    WeaponAudioList,
    [Description("Weapon Audio [Final]")]
    WeaponAudio,
    [Description("BKHD Group List")]
    BKHDGroupList,
    [Description("BKHD Group [Final]")]
    BKHDGroup,
    [Description("Weapon Audio List")]
    BKHDAudioList,
    [Description("Weapon Audio [Final]")]
    BKHDAudio,
    [Description("Material List [Packages]")]
    MaterialList,
    [Description("Material [Final]")]
    Material,
}

/// <summary>
/// The current implementation of Package is limited so you cannot have nested views below a Package.
/// For future, would be better to split the tag items up so we can cache them based on parents.
/// </summary>
public partial class TagListView : UserControl
{
    private struct ParentInfo
    {
        public ETagListType TagListType;
        public TigerHash? Hash;
        public string SearchTerm;
        public ConcurrentBag<TagItem> AllTagItems;
    }

    private ConcurrentBag<TagItem> _allTagItems;
    private static MainWindow _mainWindow = null;
    private ETagListType _tagListType;
    private TigerHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new Stack<ParentInfo>();
    private bool _bTrimName = true;
    private bool _bShowNamedOnly = false;
    private TagListView _tagListControl = null;
    private ToggleButton _previouslySelected = null;
    private int _selectedIndex = -1;
    private FbxHandler _globalFbxHandler = null;
    private string _weaponItemName = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        _globalFbxHandler = new FbxHandler(false);
    }

    public TagListView()
    {
        InitializeComponent();
    }

    private TagView GetViewer()
    {
        if (Parent is Grid)
        {
            if ((Parent as Grid).Parent is TagListViewerView)
                return ((Parent as Grid).Parent as TagListViewerView).TagView;
            else if ((Parent as Grid).Parent is TagView)
                return (Parent as Grid).Parent as TagView;
        }
        Log.Error($"Parent is not a TagListViewerView, is {Parent.GetType().Name}.");
        return null;
    }

    public async void LoadContent(ETagListType tagListType, TigerHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null)
    {
        Log.Verbose($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
        if (overrideItems != null)
        {
            _allTagItems = overrideItems;
        }
        else
        {
            if (contentValue != null && !bFromBack && !TagItem.GetEnumDescription(tagListType).Contains("[Final]")) // if the type nests no new info, it isnt a parent
            {
                _parentStack.Push(new ParentInfo
                {
                    AllTagItems = _allTagItems,
                    Hash = _currentHash,
                    TagListType = _tagListType,
                    SearchTerm = SearchBox.Text
                });
            }

            switch (tagListType)
            {
                case ETagListType.DestinationGlobalTagBagList:
                    await LoadDestinationGlobalTagBagList();
                    break;
                case ETagListType.Back:
                    Back_Clicked();
                    return;
                case ETagListType.DestinationGlobalTagBag:
                    LoadDestinationGlobalTagBag(contentValue as FileHash);
                    break;
                case ETagListType.BudgetSet:
                    LoadBudgetSet(contentValue as FileHash);
                    break;
                case ETagListType.Entity:
                    LoadEntity(contentValue as FileHash);
                    break;
                case ETagListType.ApiList:
                    await LoadApiList();
                    break;
                case ETagListType.ApiEntity:
                    LoadApiEntity(contentValue);
                    break;
                case ETagListType.EntityList:
                    await LoadEntityList();
                    break;
                case ETagListType.Package:
                    LoadPackage(contentValue as FileHash);
                    break;
                case ETagListType.ActivityList:
                    await LoadActivityList();
                    break;
                case ETagListType.Activity:
                    LoadActivity(contentValue as FileHash);
                    break;
                case ETagListType.StaticsList:
                    await LoadStaticList();
                    break;
                case ETagListType.Static:
                    LoadStatic(contentValue as FileHash);
                    break;
                case ETagListType.TextureList:
                    await LoadTextureList();
                    break;
                case ETagListType.Texture:
                    LoadTexture(contentValue as FileHash);
                    break;
                case ETagListType.DialogueList:
                    LoadDialogueList(contentValue as FileHash);
                    break;
                case ETagListType.Dialogue:
                    LoadDialogue(contentValue as FileHash);
                    break;
                case ETagListType.DirectiveList:
                    LoadDirectiveList(contentValue as FileHash);
                    break;
                case ETagListType.Directive:
                    LoadDirective(contentValue as FileHash);
                    break;
                case ETagListType.StringContainersList:
                    await LoadStringContainersList();
                    break;
                case ETagListType.StringContainer:
                    LoadStringContainer(contentValue as FileHash);
                    break;
                case ETagListType.Strings:
                    LoadStrings(contentValue as FileHash);
                    break;
                case ETagListType.String:
                    break;
                case ETagListType.SoundsPackagesList:
                    await LoadSoundsPackagesList();
                    break;
                case ETagListType.SoundsPackage:
                    LoadSoundsPackage(contentValue as FileHash);
                    break;
                case ETagListType.SoundsList:
                    await LoadSoundsList(contentValue as FileHash);
                    break;
                case ETagListType.Sound:
                    LoadSound(contentValue as FileHash);
                    break;
                case ETagListType.MusicList:
                    LoadMusicList(contentValue as FileHash);
                    break;
                case ETagListType.Music:
                    LoadMusic(contentValue as FileHash);
                    break;
                case ETagListType.WeaponAudioGroupList:
                    await LoadWeaponAudioGroupList();
                    break;
                case ETagListType.WeaponAudioGroup:
                    LoadWeaponAudioGroup(contentValue);
                    break;
                case ETagListType.WeaponAudioList:
                    LoadWeaponAudioList(contentValue);
                    break;
                case ETagListType.WeaponAudio:
                    await LoadWeaponAudio(contentValue as FileHash);
                    break;
                case ETagListType.MaterialList:
                    await LoadMaterialList();
                    break;
                case ETagListType.Material:
                    LoadMaterial(contentValue as FileHash);
                    break;
                case ETagListType.BKHDGroupList:
                    await LoadBKHDGroupList();
                    break;
                case ETagListType.BKHDGroup:
                    LoadBKHDAudioGroup(contentValue as FileHash);
                    break;
                case ETagListType.BKHDAudioList:
                    LoadBKHDAudioList(contentValue as FileHash);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        if (!TagItem.GetEnumDescription(tagListType).Contains("[Final]"))
        {
            _currentHash = contentValue;
            _tagListType = tagListType;
            if (!bFromBack)
            {
                SearchBox.Text = "";
            }

            RefreshItemList();
        }

        Log.Verbose($"Loaded content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    }

    /// <summary>
    /// For when we want stuff in packages, we then split up based on what the FileHash value is.
    /// I kinda cheat here, I store everything in one massive _allTagItems including the packages
    /// </summary>
    /// <param name="packageId">Package ID for this package to load data for.</param>
    private void LoadPackage(FileHash pkgHash)
    {
        int pkgId = pkgHash.PackageId;
        SetBulkGroup(pkgId.ToString("x4"));
        var collection = _allTagItems.Where(x => (x.Hash as FileHash).PackageId == pkgId && x.TagType != ETagListType.Package).ToList();
        _allTagItems = new ConcurrentBag<TagItem>(collection);
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

            //if (!TagItem.GetEnumDescription(_tagListType).Contains("List"))
            //{
            //    if (displayItems.Count > 50) return;
            //}
            if (TagItem.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
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
                || item.Hash.Hash32.ToString().Contains(searchStr)
                || (item.Subname != null && item.Subname.ToLower().Contains(searchStr)))
            {
                var pkg = (item.Hash as FileHash) is not null ? PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId((item.Hash as FileHash).PackageId).Split("\\").Last().Split(".").First() : "";
                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    Subname = searchStr != string.Empty && item.Type != "Package" ?
                            $"{item.Subname} {pkg}"
                            : item.Subname,
                    FontSize = _bTrimName || !bWasTrimmed ? 16 : 12,
                });
            }
        });

        // Check if trim names and filter named should be visible (if there any named items)
        TrimCheckbox.Visibility = bShowTrimCheckbox ? Visibility.Visible : Visibility.Hidden;
        ShowNamedCheckbox.Visibility = bName && bNoName ? Visibility.Visible : Visibility.Hidden;

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
        if (tagItems.Any() && tagItems.First().Type == "Package")
        {
            tagItems.Sort((p, q) => string.Compare(p.Name, q.Name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            tagItems.Sort((a, b) => a.Hash.Hash32 > b.Hash.Hash32 ? 1 : -1);
        }
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

        TagList.ItemsSource = tagItems;
    }

    /// <summary>
    /// From all the existing items in _allTagItems, we generate the packages for it
    /// and add but only if packages don't exist already.
    /// </summary>
    private void MakePackageTagItems()
    {
        ConcurrentHashSet<int> packageIds = new ConcurrentHashSet<int>();
        bool bBroken = false;
        Parallel.ForEach(_allTagItems, (item, state) =>
        {
            if (item.TagType == ETagListType.Package)
            {
                bBroken = true;
                state.Break();
            }

            packageIds.Add((item.Hash as FileHash).PackageId);  // todo fix this garbage 'as' call
        });

        if (bBroken)
            return;

        Parallel.ForEach(packageIds, pkgId =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = string.Join('_', PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId((ushort)pkgId).Split('_').Skip(1).SkipLast(1)),
                Hash = new FileHash(pkgId, 0),
                TagType = ETagListType.Package
            });
        });
    }

    private void RefreshItemList()
    {
        SetItemListByString(SearchBox.Text.ToLower());
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RefreshItemList();
    }

    /// <summary>
    /// This onclick is used by all the different types.
    /// </summary>
    private void TagItem_OnClick(object sender, RoutedEventArgs e)
    {
        var btn = sender as ToggleButton;
        TagItem tagItem = btn.DataContext as TagItem;
        TigerHash tigerHash = tagItem.Hash;
        if (_previouslySelected != null)
            _previouslySelected.IsChecked = false;
        _selectedIndex = TagList.Items.IndexOf(tagItem);
        // if (_previouslySelected == btn)
        // _previouslySelected.IsChecked = !_previouslySelected.IsChecked;
        _previouslySelected = btn;
        LoadContent(tagItem.TagType, tigerHash);
    }

    public static T GetChildOfType<T>(DependencyObject depObj)
        where T : DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            var result = (child as T) ?? GetChildOfType<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    public static List<T> GetChildrenOfType<T>(DependencyObject depObj)
        where T : DependencyObject
    {
        var children = new List<T>();
        if (depObj == null) return children;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T)
            {
                children.Add(child as T);
            }
            else
            {
                children.AddRange(GetChildrenOfType<T>(child));
            }
        }
        return children;
    }

    /// <summary>
    /// Use the ParentInfo to go back to previous tag data.
    /// </summary>
    private void Back_Clicked()
    {
        ParentInfo parentInfo = _parentStack.Pop();
        SearchBox.Text = parentInfo.SearchTerm;
        LoadContent(parentInfo.TagListType, parentInfo.Hash, true, parentInfo.AllTagItems);
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
    private void SetViewer(TagView.EViewerType eViewerType)
    {
        var viewer = GetViewer();
        viewer.SetViewer(eViewerType);
    }

    private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectedIndex == -1)
            return;
        if (TagList.SelectedIndex > _selectedIndex)
        {
            var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex + 1));
            if (nextButton == null)
                return;
            nextButton.IsChecked = true;
            _selectedIndex++;
            TagItem_OnClick(nextButton, null);
        }

        else if (TagList.SelectedIndex < _selectedIndex)
        {
            var currentButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex - 1));
            if (nextButton == null)
                return;
            nextButton.IsChecked = true;
            _selectedIndex--;
            TagItem_OnClick(nextButton, null);

        }
    }

    public void ShowBulkExportButton()
    {
        BulkExportButton.Visibility = Visibility.Visible;
    }

    public void SetBulkGroup(string group)
    {
        var tab = ((Parent as Grid).Parent as TagListViewerView).Parent as TabItem;
        BulkExportButton.Tag = $"{group}_{tab.Header}";
    }

    private async void BulkExport_OnClick(object sender, RoutedEventArgs e)
    {
        if (BulkExportButton.Tag == null)
        {
            return;
        }

        var groupName = BulkExportButton.Tag as string;
        var viewer = GetViewer();
        bool bStaticShowing = viewer.StaticControl.Visibility == Visibility.Visible;
        bool bEntityShowing = viewer.EntityControl.Visibility == Visibility.Visible;
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Hidden : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Hidden : viewer.EntityControl.Visibility;

        // Iterate over all buttons and export it
        var items = TagList.ItemsSource.Cast<TagItem>();
        var exportItems = items.Where(x => x.TagType != ETagListType.Back && x.TagType != ETagListType.Package).ToList();
        if (exportItems.Count == 0)
        {
            MessageBox.Show("No tags to export.");
            return;
        }
        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var tagItem in exportItems)
            {
                var name = tagItem.Name == String.Empty ? tagItem.Hash : tagItem.Name;
                var exportInfo = new ExportInfo
                {
                    Hash = tagItem.Hash as FileHash,
                    Name = $"/Bulk_{groupName}/{name}",
                    ExportType = ExportTypeFlag.Minimal
                };
                viewer.ExportControl.RoutedFunction(exportInfo);
                MainWindow.Progress.CompleteStage();
            }
        });
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Visible : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Visible : viewer.EntityControl.Visibility;
    }

    #region Destination Global Tag Bag

    /// <summary>
    /// Type 0x8080471D and only in sr_destination_metadata_010a?
    /// </summary>
    private async Task LoadDestinationGlobalTagBagList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        var vals = await PackageResourcer.Get().GetAllHashesAsync<D2Class_1D478080>();
        Parallel.ForEach(vals, val =>
        {
            Tag<D2Class_1D478080> dgtbParent = FileResourcer.Get().GetSchemaTag<D2Class_1D478080>(val);
            if (dgtbParent.TagData.DestinationGlobalTagBags.Count < 1)
                return;
            foreach (D2Class_D3598080 destinationGlobalTagBag in dgtbParent.TagData.DestinationGlobalTagBags)
            {
                if (!destinationGlobalTagBag.DestinationGlobalTagBag.IsValid())
                    continue;

                _allTagItems.Add(new TagItem
                {
                    Hash = destinationGlobalTagBag.DestinationGlobalTagBag,
                    Name = destinationGlobalTagBag.DestinationGlobalTagBagName,
                    TagType = ETagListType.DestinationGlobalTagBag
                });
            }
        });
    }

    private void LoadDestinationGlobalTagBag(FileHash hash)
    {
        Tag<D2Class_30898080> destinationGlobalTagBag = FileResourcer.Get().GetSchemaTag<D2Class_30898080>(hash);

        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(destinationGlobalTagBag.TagData.Unk18, val =>
        {
            if (val.Tag == null)
                return;
            FileHash reference = val.Tag.Hash.GetReferenceHash();
            ETagListType tagType;
            string overrideType = String.Empty;
            switch (reference.Hash32)
            {
                case 0x8080987e:
                    tagType = ETagListType.BudgetSet;
                    break;
                case 0x80809ad8:
                    tagType = ETagListType.Entity;
                    break;
                default:
                    tagType = ETagListType.None;
                    overrideType = reference;
                    break;
            }
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath,
                Subname = val.TagNote,
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion

    #region Budget Set

    private void LoadBudgetSet(FileHash hash)
    {
        Tag<D2Class_7E988080> budgetSetHeader = FileResourcer.Get().GetSchemaTag<D2Class_7E988080>(hash);
        Tag<D2Class_ED9E8080> budgetSet = FileResourcer.Get().GetSchemaTag<D2Class_ED9E8080>(budgetSetHeader.TagData.Unk00.Hash);
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(budgetSet.TagData.Unk28, val =>
        {
            if (!val.Tag.Hash.IsValid())
            {
                Log.Error($"BudgetSet {budgetSetHeader.TagData.Unk00.Hash} has an invalid tag hash.");
                return;
            }
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath,
                TagType = ETagListType.Entity,
            });
        });
    }

    #endregion

    #region Entity

    private void LoadEntity(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        bool bLoadedSuccessfully = viewer.EntityControl.LoadEntity(fileHash, _globalFbxHandler);
        if (!bLoadedSuccessfully)
        {
            Log.Error($"UI failed to load entity for hash {fileHash}. You can still try to export the full model instead.");
            _mainWindow.SetLoggerSelected();
        }
        SetExportFunction(ExportEntity, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
        viewer.ExportControl.ExportChildrenBox.Visibility = Visibility.Visible;
        viewer.ExportControl.SetExportInfo(fileHash);
        viewer.EntityControl.ModelView.SetModelFunction(() => viewer.EntityControl.LoadEntity(fileHash, _globalFbxHandler));
    }

    private void ExportEntity(ExportInfo info)
    {
        var viewer = GetViewer();
        Entity entity = FileResourcer.Get().GetFile<Entity>(info.Hash);
        List<Entity> entities = new List<Entity> { entity };
        Dispatcher.Invoke(() =>
        {
            if (viewer.ExportControl.ExportChildrenBox.Visibility == Visibility.Visible && viewer.ExportControl.ExportChildrenBox.IsChecked.Value == true)
                entities.AddRange(entity.GetEntityChildren());
        });
        EntityView.Export(entities, info.Name, info.ExportType);
    }

    /// <summary>
    /// We load all of them including no names, but add an option to only show names.
    /// Named: destination global tag bags 0x80808930, budget sets 0x80809eed
    /// All others: reference 0x80809ad8
    /// They're sorted into packages first.
    /// To check they have a model, I take an approach that means processing 40k entities happens quickly.
    /// To do so, I can't use the tag parser as this is way too slow. Instead, I check
    /// 1. at least 2 resources
    /// 2. the first or second resource contains a Unk0x10 == D2Class_8A6D8080
    /// If someone wants to make this list work for entities with other things like skeletons etc, this is easy to
    /// customise to desired system.
    /// </summary>
    private async Task LoadEntityList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Caching Entity Names",
            "Loading Entities"
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var NamedEntities = TryGetEntityNames();
            MainWindow.Progress.CompleteStage();

            var eVals = PackageResourcer.Get().GetAllFiles<Entity>();
            ConcurrentHashSet<uint> existingEntities = new();
            Parallel.ForEach(eVals, entity =>
            {
                if (entity.HasGeometry())
                {
                    var entityName = entity.EntityName != null ? entity.EntityName : entity.Hash;

                    // Most of the time the most specific entity name comes from a map resource (bosses usually)
                    if (NamedEntities.ContainsKey(entity.Hash))
                        entity.EntityName = NamedEntities[entity.Hash];

                    _allTagItems.Add(new TagItem
                    {
                        Hash = entity.Hash,
                        Name = entityName,
                        TagType = ETagListType.Entity
                    });
                }
            });
            MainWindow.Progress.CompleteStage();
            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private ConcurrentDictionary<string, string> TryGetEntityNames()
    {
        NamedEntities Ents = new()
        {
            EntityNames = new()
        };

        if (!File.Exists($"./EntityNames.json"))
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));

        Ents = JsonConvert.DeserializeObject<NamedEntities>(File.ReadAllText($"./EntityNames.json"));
        if (Ents.EntityNames.ContainsKey(Strategy.CurrentStrategy) && Ents.EntityNames[Strategy.CurrentStrategy].Count > 0)
        {
            return Ents.EntityNames[Strategy.CurrentStrategy];
        }
        else
        {
            Ents.EntityNames[Strategy.CurrentStrategy] = new();
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            {
                var vals = PackageResourcer.Get().GetAllHashes<SD9128080>();
                Parallel.ForEach(vals, val =>
                {
                    var entry = FileResourcer.Get().GetSchemaTag<SD9128080>(val);
                    foreach (var a in entry.TagData.Unk20)
                    {
                        foreach (var b in a.Unk08)
                        {
                            if (b.Pointer.GetValue(entry.GetReader()) is SMapDataEntry datatable)
                            {
                                if (datatable.DataResource.GetValue(entry.GetReader()) is S33138080 name)
                                {
                                    if (name.EntityName.IsValid())
                                        Ents.EntityNames[Strategy.CurrentStrategy].TryAdd(datatable.GetEntityHash(), GlobalStrings.Get().GetString(name.EntityName));
                                }
                            }
                        }
                    }
                });

                var vals2 = PackageResourcer.Get().GetAllHashes<SF6038080>();
                Parallel.ForEach(vals2, val =>
                {
                    var entry = FileResourcer.Get().GetSchemaTag<SF6038080>(val);
                    if (entry.TagData.EntityResource is not null)
                    {
                        if (entry.TagData.EntityResource.TagData.Unk10.GetValue(entry.TagData.EntityResource.GetReader()) is S2E098080)
                        {
                            var resource = (SDD078080)entry.TagData.EntityResource.TagData.Unk18.GetValue(entry.TagData.EntityResource.GetReader());
                            foreach (var dataentry in resource.DataEntries)
                            {
                                if (dataentry.GetEntityHash().IsValid())
                                    Ents.EntityNames[Strategy.CurrentStrategy].TryAdd(dataentry.GetEntityHash(), resource.DevName);
                            }
                        }
                    }
                });
            }
            else if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
            {
                var vals = PackageResourcer.Get().GetAllHashes<SMapDataTable>();
                Parallel.ForEach(vals, val =>
                {
                    var entry = FileResourcer.Get().GetSchemaTag<SMapDataTable>(val);
                    foreach (var dataEntry in entry.TagData.DataEntries)
                    {
                        if (dataEntry.DataResource.GetValue(entry.GetReader()) is D2Class_19808080 name)
                        {
                            if (name.EntityName.IsValid())
                                Ents.EntityNames[Strategy.CurrentStrategy].TryAdd(dataEntry.GetEntityHash(), GlobalStrings.Get().GetString(name.EntityName));
                        }
                    }
                });
            }
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
        }

        return Ents.EntityNames[Strategy.CurrentStrategy];
    }

    #endregion

    #region API

    private async Task LoadApiList()
    {
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(inventoryItems, item =>
        {
            if (item.GetArtArrangementIndex() == -1) return;
            string name = Investment.Get().GetItemName(item);
            string type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
            if (type == "Finisher" || type.Contains("Emote"))
                return;  // they point to Animation instead of Entity
            _allTagItems.Add(new TagItem
            {
                Hash = item.TagData.InventoryItemHash,
                Name = name,
                Type = type.Trim(),
                TagType = ETagListType.ApiEntity
            });  // for some reason some of the types have spaces after
        });
    }

    private void LoadApiEntity(TigerHash apiHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        viewer.EntityControl.LoadEntityFromApi(apiHash, _globalFbxHandler);
        Dispatcher.Invoke(() =>
        {
            SetExportFunction(ExportApiEntityFull, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
            viewer.ExportControl.SetExportInfo(apiHash);
            viewer.EntityControl.ModelView.SetModelFunction(() => viewer.EntityControl.LoadEntityFromApi(apiHash, _globalFbxHandler));
        });
    }

    private void SetExportFunction(Action<ExportInfo> function, int exportTypeFlags)
    {
        var viewer = GetViewer();
        viewer.ExportControl.SetExportFunction(function, exportTypeFlags);
        ShowBulkExportButton();
    }

    private void ExportApiEntityFull(ExportInfo info)
    {
        var viewer = GetViewer();
        EntityView.Export(Investment.Get().GetEntitiesFromHash(info.Hash), info.Name, info.ExportType);
    }

    #endregion

    #region Activity

    /// <summary>
    /// Type 0x80808e8e, but we use a child of it (0x80808e8b) so we can get the location.
    /// </summary>
    private async Task LoadActivityList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        // Getting names
        ConcurrentDictionary<string, StringHash> nameHashes = new();
        ConcurrentDictionary<string, string> names = new();
        switch (Strategy.CurrentStrategy)
        {
            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                var activities = PackageResourcer.Get().GetD1Activities();
                Parallel.ForEach(activities, activity =>
                {
                    if (activity.Value == "16068080")
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(activity.Key);
                        //tag.TagData.LocationName works but some entries dont have a string for it
                        nameHashes.TryAdd(tag.TagData.ActivityDevName.Value, tag.TagData.DestinationName);

                        GlobalStrings.Get().AddStrings(tag.TagData.LocalizedStrings);
                    }
                });
                break;
            case TigerStrategy.DESTINY2_SHADOWKEEP_2601 or TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                var valsChild = await PackageResourcer.Get().GetAllHashesAsync<SUnkActivity_SK>();
                Parallel.ForEach(valsChild, val =>
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    nameHashes.TryAdd(tag.TagData.ActivityDevName.Value, tag.TagData.DestinationName);
                    GlobalStrings.Get().AddStrings(tag.TagData.LocalizedStrings);
                });
                break;
            default:
                valsChild = await PackageResourcer.Get().GetAllHashesAsync<D2Class_8B8E8080>();
                Parallel.ForEach(valsChild, val =>
                {
                    Tag<D2Class_8B8E8080> tag = FileResourcer.Get().GetSchemaTag<D2Class_8B8E8080>(val);
                    nameHashes.TryAdd(tag.TagData.DestinationName, tag.TagData.LocationName);
                    GlobalStrings.Get().AddStrings(tag.TagData.StringContainer);
                });
                break;
        }

        foreach (var keyValuePair in nameHashes)
        {
            names[keyValuePair.Key] = GlobalStrings.Get().GetString(keyValuePair.Value);
        }

        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            var activities = PackageResourcer.Get().GetD1Activities();

            Parallel.ForEach(activities, val =>
            {
                if (val.Value == "2E058080")
                {
                    var activityName = PackageResourcer.Get().GetActivityName(val.Key);
                    var first = activityName.Split(":")[1];
                    _allTagItems.Add(new TagItem
                    {
                        Hash = val.Key,
                        Name = first,
                        Subname = names.ContainsKey(first) ? names[first] : "",
                        TagType = ETagListType.Activity
                    });
                }
            });
        }
        else
        {
            var vals = await PackageResourcer.Get().GetAllHashesAsync<IActivity>();
            Parallel.ForEach(vals, val =>
            {
                var activityName = PackageResourcer.Get().GetActivityName(val);
                var first = Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402
                ? activityName.Split(".").First() : activityName.Split(":")[1];

                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = activityName,
                    Subname = names.ContainsKey(first) ? names[first] : "",
                    TagType = ETagListType.Activity
                });
            });
        }
    }

    private void LoadActivity(FileHash fileHash)
    {
        ActivityView activityView = new ActivityView();
        _mainWindow.MakeNewTab(PackageResourcer.Get().GetActivityName(fileHash), activityView);
        activityView.LoadActivity(fileHash);
        _mainWindow.SetNewestTabSelected();
        // ExportControl.SetExportFunction(ExportActivityMapFull);
        // ExportControl.SetExportInfo(fileHash);
    }

    private void ExportActivityMapFull(object sender, RoutedEventArgs e)
    {
        var btn = sender as Button;
        ExportInfo info = (ExportInfo)btn.Tag;
        // ActivityControl.ExportFull();
    }

    #endregion

    #region Static

    private async Task LoadStaticList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            $"loading static list",
        });

        await Task.Run(async () =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var eVals = await PackageResourcer.Get().GetAllHashesAsync<SStaticMesh>();
            Parallel.ForEach(eVals, val =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    TagType = ETagListType.Static
                });
            });

            MakePackageTagItems();
        });

        MainWindow.Progress.CompleteStage();
        RefreshItemList();  // bc of async stuff
    }

    private void LoadStatic(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Static);
        viewer.StaticControl.LoadStatic(fileHash, ExportDetailLevel.MostDetailed, Window.GetWindow(this));
        // viewer.StaticControl.LoadStatic(fileHash, viewer.StaticControl.ModelView.GetSelectedLod());
        SetExportFunction(ExportStatic, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
        viewer.ExportControl.SetExportInfo(fileHash);
        // viewer.StaticControl.ModelView.SetModelFunction(() => viewer.StaticControl.LoadStatic(fileHash, viewer.StaticControl.ModelView.GetSelectedLod()));
    }

    private void ExportStatic(ExportInfo info)
    {
        var viewer = GetViewer();
        StaticView.ExportStatic(info.Hash as FileHash, info.Name, info.ExportType);
    }

    #endregion

    #region Texture

    private async Task LoadTextureList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "caching textures",
            "adding textures to ui",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            // var tex1d = PackageHandler.GetAllTagsWithTypes(32, 1);
            // var tex2d = PackageHandler.GetAllTagsWithTypes(32, 2);
            // var tex3d = PackageHandler.GetAllTagsWithTypes(32, 3);
            var tex = PackageResourcer.Get().GetAllHashes<Texture>();

            // PackageHandler.CacheHashDataList(tex1d.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();
            // PackageHandler.CacheHashDataList(tex2d.Select(x => x.Hash).ToArray());
            // MainWindow.Progress.CompleteStage();
            // PackageHandler.CacheHashDataList(tex3d.Select(x => x.Hash).ToArray());
            // MainWindow.Progress.CompleteStage();

            tex.ToList().ForEach(val => //Doesnt really need to be parrellizedm, loads quick enough without it and fixes(?) missing entries
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Texture",
                    TagType = ETagListType.Texture
                });
            });
            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    /// <summary>
    /// I could do it tiled, but cba to bother with it when you can just batch export to filesystem.
    /// </summary>
    private void LoadTexture(FileHash fileHash)
    {
        var viewer = GetViewer();
        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(fileHash);
        if (textureHeader.IsCubemap())
        {
            SetViewer(TagView.EViewerType.Texture2D);
            viewer.CubemapControl.LoadCubemap(textureHeader);
        }
        else if (textureHeader.IsVolume())
        {
            SetViewer(TagView.EViewerType.Texture1D);
            viewer.TextureControl.LoadTexture(textureHeader);
        }
        else
        {
            SetViewer(TagView.EViewerType.Texture1D);
            viewer.TextureControl.LoadTexture(textureHeader);
        }
        SetExportFunction(ExportTexture, (int)ExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    private void ExportTexture(ExportInfo info)
    {
        TextureView.ExportTexture(info.Hash as FileHash);
    }

    #endregion

    #region Dialogue

    /// <summary>
    /// We assume all dialogue tables come from activities.
    /// </summary>
    private void LoadDialogueList(FileHash fileHash)
    {
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999 && Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            return;
        }

        _allTagItems = new ConcurrentBag<TagItem>();

        // Dialogue tables can be in the 0x80808948 entries
        ConcurrentDictionary<string, FileHash> dialogueTables = new();
        switch (Strategy.CurrentStrategy)
        {
            case >= TigerStrategy.DESTINY2_WITCHQUEEN_6307:
                ActivityWQ activity = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activity.TagData.Unk18.GetValue(activity.GetReader()) is D2Class_6A988080 entry)
                {
                    foreach (var dirtable in entry.DialogueTables)
                    {
                        if (dirtable.DialogueTable != null)
                            dialogueTables.TryAdd(dirtable.DialogueTable.Hash, dirtable.DialogueTable.Hash);
                    }
                }
                Parallel.ForEach(activity.TagData.Unk50, val =>
                {
                    foreach (var d2Class48898080 in val.Unk18)
                    {
                        var resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                        if (resource is D2Class_D5908080 || resource is D2Class_44938080 || resource is D2Class_45938080 ||
                            resource is D2Class_18978080 || resource is D2Class_19978080)
                        {
                            if (resource.DialogueTable != null)
                                dialogueTables.TryAdd(resource.DialogueTable.Hash, resource.DialogueTable.Hash);
                        }
                    }
                });
                break;

            case TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                ActivityWQ activityBL = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                var resource = activityBL.TagData.Unk18.GetValue(activityBL.GetReader());
                //if (resource is D2Class_D5908080 || resource is D2Class_44938080 || resource is D2Class_45938080 ||
                //    resource is D2Class_18978080 || resource is D2Class_19978080)
                if (resource is D2Class_19978080)
                {
                    if (resource.DialogueTableBL != null)
                        dialogueTables.TryAdd(resource.DialogueTableBL.Hash, resource.DialogueTableBL.Hash);
                }
                break;

            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                ActivityROI activityROI = FileResourcer.Get().GetFile<ActivityROI>(fileHash);
                var valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (var val in valsROI)
                {
                    if (val.Value == "16068080")
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(val.Key);

                        string activityName = PackageResourcer.Get().GetActivityName(activityROI.FileHash).Split(':')[1];
                        if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                        {
                            dialogueTables.TryAdd($"{PackageResourcer.Get().GetActivityName(val.Key).Split(":").First()}", val.Key);
                        }
                    }
                }
                break;
        }


        Parallel.ForEach(dialogueTables, entry =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = entry.Key,
                Hash = entry.Value,
                TagType = ETagListType.Dialogue
            });
        });
    }


    // TODO replace this by deleting DialogueControl and using TagList instead
    private void LoadDialogue(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Dialogue);
        viewer.DialogueControl.Load(fileHash, viewer);
    }

    #endregion

    #region Directive

    private void LoadDirectiveList(FileHash fileHash)
    {
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999 && Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            return;
        }
        _allTagItems = new ConcurrentBag<TagItem>();

        // Dialogue tables can be in the 0x80808948 entries

        // TODO: shrink this? extra if/else statement for single type change looks ugly imo
        switch (Strategy.CurrentStrategy)
        {
            case >= TigerStrategy.DESTINY2_WITCHQUEEN_6307:
                ActivityWQ activityWQ = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is D2Class_6A988080 a988080)
                {
                    var directiveTables = a988080.DirectiveTables.Select(x => x.DirectiveTable.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = hash,
                            Name = hash,
                            TagType = ETagListType.Directive
                        });
                    });
                }
                else if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is D2Class_20978080 class20978080)
                {
                    var directiveTables = class20978080.PEDirectiveTables.Select(x => x.DirectiveTable.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = hash,
                            Name = hash,
                            TagType = ETagListType.Directive
                        });
                    });
                }
                break;

            case TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                ActivityWQ activityBL = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activityBL.TagData.Unk18.GetValue(activityBL.GetReader()) is D2Class_19978080 resource)
                {
                    var directiveTables = resource.DirectiveTables.Select(x => x.DirectiveTable.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = hash,
                            Name = hash,
                            TagType = ETagListType.Directive
                        });
                    });
                }
                break;

            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                ActivityROI activityROI = FileResourcer.Get().GetFile<ActivityROI>(fileHash);
                var valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (var val in valsROI)
                {
                    if (val.Value == "16068080")
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(val.Key);
                        string activityName = PackageResourcer.Get().GetActivityName(activityROI.FileHash).Split(':')[1];
                        if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                        {
                            _allTagItems.Add(new TagItem
                            {
                                Hash = val.Key,
                                Name = PackageResourcer.Get().GetActivityName(val.Key).Split(":").First(),
                                TagType = ETagListType.Directive
                            });
                        }
                    }
                }
                break;
        }
    }

    // TODO replace with taglist control
    private void LoadDirective(FileHash fileHash)
    {
        SetViewer(TagView.EViewerType.Directive);
        var viewer = GetViewer();
        viewer.DirectiveControl.Load(fileHash);
    }

    #endregion

    #region String

    private async Task LoadStringContainersList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "caching string tags",
            "load string list",
        });

        await Task.Run(async () =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var vals = await PackageResourcer.Get().GetAllHashesAsync<LocalizedStrings>();
            // PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(vals, val =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"{val}",
                    TagType = ETagListType.StringContainer
                });
            });
            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private void LoadStringContainer(FileHash fileHash)
    {
        SetViewer(TagView.EViewerType.TagList);
        var viewer = GetViewer();
        viewer.TagListControl.LoadContent(ETagListType.Strings, fileHash, true);
    }

    // Would be nice to do something with colour formatting.
    private void LoadStrings(FileHash fileHash)
    {
        var viewer = GetViewer();
        _allTagItems = new ConcurrentBag<TagItem>();
        LocalizedStrings localizedStrings = FileResourcer.Get().GetFile<LocalizedStrings>(fileHash);

        localizedStrings.GetAllStringViews().ForEach(view =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = view.RawString,
                Hash = view.StringHash,
                TagType = ETagListType.String
            });
        });

        RefreshItemList();
        SetExportFunction(ExportString, (int)ExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    private void ExportString(ExportInfo info)
    {
        LocalizedStrings localizedStrings = FileResourcer.Get().GetFile<LocalizedStrings>(info.Hash);
        StringBuilder text = new();

        localizedStrings.GetAllStringViews().ForEach(view =>
        {
            text.Append($"{view.StringHash} : {view.RawString} \n");
        });

        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        string saveDirectory = config.GetExportSavePath() + $"/Strings/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);

        File.WriteAllText(saveDirectory + "strings.txt", text.ToString());

    }

    #endregion

    #region Sound

    private async Task LoadSoundsPackagesList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "caching sound tags",
            "load sound packages list",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            HashSet<Wem> vals = PackageResourcer.Get().GetAllFiles<Wem>();
            MainWindow.Progress.CompleteStage();

            ConcurrentHashSet<int> packageIds = new();
            Parallel.ForEach(vals, wem =>
            {
                packageIds.Add(wem.Hash.PackageId);
            });

            Parallel.ForEach(packageIds, pkgId =>
            {
                _allTagItems.Add(new TagItem
                {
                    Name = string.Join('_', PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId((ushort)pkgId).Split('_').Skip(1).SkipLast(1)),
                    Hash = new FileHash(pkgId, 0),
                    TagType = ETagListType.SoundsPackage
                });
            });
        });

        MainWindow.Progress.CompleteStage();
        RefreshItemList();  // bc of async stuff
    }

    private void LoadSoundsPackage(FileHash fileHash)
    {
        SetViewer(TagView.EViewerType.TagList);
        var viewer = GetViewer();
        viewer.MusicPlayer.Visibility = Visibility.Visible;
        viewer.TagListControl.LoadContent(ETagListType.SoundsList, fileHash, true);
    }

    private async Task LoadSoundsList(FileHash fileHash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading sounds",
        });

        await Task.Run(() =>
        {
            List<Wem> vals = PackageResourcer.Get().GetPackage(fileHash.PackageId).GetAllFiles<Wem>();
            // PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
            _allTagItems = new ConcurrentBag<TagItem>();
            Parallel.ForEach(vals, wem =>
            {
                if (wem.GetData().Length == 1)
                    return;

                _allTagItems.Add(new TagItem
                {
                    Name = wem.Hash,
                    Hash = wem.Hash,
                    Subname = wem.Duration,
                    TagType = ETagListType.Sound
                });
            });
        });

        MainWindow.Progress.CompleteStage();
        RefreshItemList();
    }

    private async Task LoadBKHDGroupList()
    {
        var banks = PackageResourcer.Get().GetAllFiles<WwiseSound>();
        _allTagItems = new ConcurrentBag<TagItem>();

        Parallel.ForEach(banks, bank =>
        {
            if (bank.TagData.Wems.Count > 0)
            {
                string name = bank.TagData.SoundbankBL.GetNameFromBank();

                _allTagItems.Add(new TagItem
                {
                    Hash = bank.Hash,
                    Name = name,
                    Subname = $"{bank.TagData.Wems.Count} sounds",
                    TagType = ETagListType.BKHDGroup
                });
            }
        });

        _allTagItems.OrderBy(x => x.Name);
    }

    private void LoadBKHDAudioGroup(FileHash hash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.TagList);
        viewer.TagListControl.LoadContent(ETagListType.BKHDAudioList, hash, true);
        viewer.MusicPlayer.Visibility = Visibility.Visible;
    }

    private void LoadBKHDAudioList(FileHash hash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        WwiseSound bank = FileResourcer.Get().GetFile<WwiseSound>(hash);

        Parallel.ForEach(bank.TagData.Wems, wem =>
        {
            if (wem.GetData().Length == 1)
                return;

            _allTagItems.Add(new TagItem
            {
                Name = wem.Hash,
                Hash = wem.Hash,
                Subname = wem.Duration,
                TagType = ETagListType.Sound
            });
        });

        RefreshItemList();
    }

    private void LoadSound(FileHash fileHash)
    {
        var viewer = GetViewer();
        if (viewer.MusicPlayer.SetWem(FileResourcer.Get().GetFile<Wem>(fileHash)))
        {
            viewer.MusicPlayer.Play();
            SetExportFunction(ExportWav, (int)ExportTypeFlag.Full);
            viewer.ExportControl.SetExportInfo(fileHash);
        }
    }

    private void ExportSound(ExportInfo info)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();

        WwiseSound sound = FileResourcer.Get().GetFile<WwiseSound>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{(_weaponItemName == null ? "" : $"{_weaponItemName}/")}{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        sound.ExportSound(saveDirectory);
    }

    private void ExportWem(ExportInfo info)
    {
        // exporting while playing the audio causes a hang
        var viewer = GetViewer();
        Dispatcher.Invoke(() =>
        {
            if (viewer.MusicPlayer.IsPlaying())
                viewer.MusicPlayer.Pause();
        });

        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wem");
    }

    private void ExportWav(ExportInfo info)
    {
        // exporting while playing the audio causes a hang
        var viewer = GetViewer();
        Dispatcher.Invoke(() =>
        {
            if (viewer.MusicPlayer.IsPlaying())
                viewer.MusicPlayer.Pause();
        });

        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wav");
    }

    #endregion

    #region Music

    /// <summary>
    /// We assume all music tables come from activities.
    /// </summary>
    private void LoadMusicList(FileHash fileHash)
    {
        if (Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999)
        {
            return;
        }
        ActivityWQ activity = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
        _allTagItems = new ConcurrentBag<TagItem>();

        ConcurrentBag<FileHash> musics = new();
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
        {
            // TODO: check if wq way of music is also in beyond light
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is D2Class_19978080 res)
            {
                if (res.Music != null)
                    musics.Add(res.Music.Hash);
            }
        }
        else if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            Parallel.ForEach(activity.TagData.Unk50, val =>
            {
                foreach (var d2Class48898080 in val.Unk18)
                {
                    var resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                    if (resource is D2Class_D5908080 res)
                    {
                        if (res.Music != null)
                        {
                            musics.Add(res.Music.Hash);
                        }
                    }
                }
            });
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is D2Class_6A988080 res)
            {
                if (res.Music != null)
                    musics.Add(res.Music.Hash);
            }
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is D2Class_20978080 res2)
            {
                if (res2.Music != null)
                    musics.Add(res2.Music.Hash);
            }
        }

        Parallel.ForEach(musics, hash =>
        {
            _allTagItems.Add(new TagItem
            {
                Hash = hash,
                Name = hash,
                TagType = ETagListType.Music
            });
        });
    }

    private void LoadMusic(FileHash fileHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Music);
        viewer.MusicControl.Load(fileHash);
    }

    #endregion

    #region Weapon Audio

    private async Task LoadWeaponAudioGroupList()
    {
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(inventoryItems, item =>
        {
            if (item.GetWeaponPatternIndex() == -1)
                return;
            string name = Investment.Get().GetItemName(item);
            string type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
            if (type == null)
            {
                type = "";
            }
            if (type == "Vehicle" || type == "Ship" || type == "Ship Schematics" || type == "Ghost Shell")
                return;

            _allTagItems.Add(new TagItem
            {
                Hash = item.TagData.InventoryItemHash,
                Name = name,
                Subname = ((DestinyTierType)item.TagData.ItemRarity).ToString(),
                Type = type.Trim(),
                TagType = ETagListType.WeaponAudioGroup
            });
        });
    }

    private void LoadWeaponAudioGroup(TigerHash apiHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.TagList);
        viewer.TagListControl.LoadContent(ETagListType.WeaponAudioList, apiHash, true);
        viewer.MusicPlayer.Visibility = Visibility.Visible;
    }

    private void LoadWeaponAudioList(TigerHash apiHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        var val = Investment.Get().GetPatternEntityFromHash(apiHash);
        if (val == null || (val.PatternAudio == null && val.PatternAudioUnnamed == null))
        {
            RefreshItemList();
            return;
        }
        _weaponItemName = Investment.Get().GetItemNameSanitized(Investment.Get().GetInventoryItem(apiHash));

        var resourceUnnamedReader = val.PatternAudioUnnamed.GetReader();
        var resourceUnnamed = (D2Class_F42C8080)val.PatternAudioUnnamed.TagData.Unk18.GetValue(resourceUnnamedReader);
        var resource = (D2Class_6E358080)val.PatternAudio.TagData.Unk18.GetValue(val.PatternAudio.GetReader());
        var item = Investment.Get().GetInventoryItem(apiHash);
        var weaponContentGroupHash = Investment.Get().GetWeaponContentGroupHash(item);

        Log.Verbose($"Loading weapon entity audio {val.Hash}, ContentGroupHash {weaponContentGroupHash}");
        // Named
        Tag<D2Class_0D8C8080>? audioGroup = null;

        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroup1Hash == weaponContentGroupHash).Any())
        {
            if (resource.FallbackAudioGroup != null)
            {
                audioGroup = FileResourcer.Get().GetSchemaTag<D2Class_0D8C8080>(resource.FallbackAudioGroup.TagData.GetEntityData());
            }
        }
        else
        {
            foreach (var entry in resource.PatternAudioGroups)
            {
                if (entry.WeaponContentGroup1Hash.Equals(weaponContentGroupHash) && entry.GetAudioGroup() != null)
                {
                    audioGroup = FileResourcer.Get().GetSchemaTag<D2Class_0D8C8080>(entry.GetAudioGroup().TagData.GetEntityData());
                }
            }
        }

        if (audioGroup != null)
        {
            audioGroup.TagData.Audio.ForEach(audio =>
            {
                foreach (var s in audio.Sounds)
                {
                    if (s.GetSound() == null)
                        continue;

                    _allTagItems.Add(new TagItem
                    {
                        Hash = s.GetSound().Hash,
                        Name = s.WwiseEventName,
                        Subname = audio.WwiseEventHash,
                        TagType = ETagListType.WeaponAudio
                    });
                }
            });
        }


        // Unnamed
        var sounds = GetWeaponUnnamedSounds(resourceUnnamed, weaponContentGroupHash, resourceUnnamedReader);
        foreach (var sound in sounds)
        {
            if (sound == null)
                continue;

            string name = "";
            if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON) // && name == "")
                name = sound.TagData.SoundbankBL.GetNameFromBank();

            _allTagItems.Add(new TagItem
            {
                Hash = sound.Hash,
                Name = name,
                Subname = sound.Hash,
                TagType = ETagListType.WeaponAudio
            });
        }

        RefreshItemList();
    }

    public List<WwiseSound> GetWeaponUnnamedSounds(D2Class_F42C8080 resource, TigerHash weaponContentGroupHash, TigerReader reader)
    {
        List<WwiseSound> sounds = new();
        List<Entity> entities = new();

        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroupHash == weaponContentGroupHash).Any())
        {
            if (resource.FallbackAudio1 != null)
                entities.Add(resource.FallbackAudio1);
            if (resource.FallbackAudio2 != null)
                entities.Add(resource.FallbackAudio2);
        }
        else
        {
            resource.PatternAudioGroups.ForEach(entry =>
            {
                if (!entry.WeaponContentGroupHash.Equals(weaponContentGroupHash))
                    return;

                List<TigerFile> entitiesParents = new() { entry.Unk60, entry.Unk78, entry.Unk90, entry.UnkA8, entry.UnkC0, entry.UnkD8, entry.GetAudioEntityParent(), entry.Unk130, entry.Unk148, entry.Unk1C0, entry.Unk1D8, entry.Unk248 };

                if (entry.Unk118.GetValue(reader) is D2Class_0A2D8080 or S40238080)
                {
                    dynamic resourceUnk118 = Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON ? (S40238080)entry.Unk118.GetValue(reader) : (D2Class_0A2D8080)entry.Unk118.GetValue(reader);
                    if (resourceUnk118.Unk08 != null)
                        entities.Add(resourceUnk118.Unk08);
                    if (resourceUnk118.Unk20 != null)
                        entities.Add(resourceUnk118.Unk20);
                    if (resourceUnk118.Unk38 != null)
                        entities.Add(resourceUnk118.Unk38);
                }

                foreach (var tag in entitiesParents)
                {
                    if (tag == null)
                        continue;

                    var reference = Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON ? tag.Hash.GetReferenceHash() : tag.Hash.GetReferenceFromManifest();
                    if (reference == 0x80806fa3 || reference == 0x80803463)
                    {
                        var entityData = FileResourcer.Get().GetSchemaTag<D2Class_A36F8080>(tag.Hash).TagData.GetEntityData();
                        var reference2 = entityData.GetReferenceHash();
                        if (reference2 == 0x80802d09 || reference2 == 0x80803165)
                        {
                            if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
                            {
                                var tagInner = FileResourcer.Get().GetSchemaTag<D2Class_092D8080>(entityData);
                                if (tagInner.TagData.Unk18 != null)
                                    entities.Add(tagInner.TagData.Unk18);
                                if (tagInner.TagData.Unk30 != null)
                                    entities.Add(tagInner.TagData.Unk30);
                                if (tagInner.TagData.Unk48 != null)
                                    entities.Add(tagInner.TagData.Unk48);
                                if (tagInner.TagData.Unk60 != null)
                                    entities.Add(tagInner.TagData.Unk60);
                                if (tagInner.TagData.Unk78 != null)
                                    entities.Add(tagInner.TagData.Unk78);
                                if (tagInner.TagData.Unk90 != null)
                                    entities.Add(tagInner.TagData.Unk90);
                            }
                            else
                            {
                                // These have tag paths but getting the names from the soundbank is better (93% of the time)
                                var tagInner = FileResourcer.Get().GetSchemaTag<S65318080>(entityData);
                                if (tagInner.TagData.Entity1 != null)
                                    entities.Add(tagInner.TagData.Entity1);
                                if (tagInner.TagData.Entity2 != null)
                                    entities.Add(tagInner.TagData.Entity2);
                                if (tagInner.TagData.Entity3 != null)
                                    entities.Add(tagInner.TagData.Entity3);
                                if (tagInner.TagData.Entity4 != null)
                                    entities.Add(tagInner.TagData.Entity4);
                            }
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else if (reference == 0x80809ad8)
                    {
                        entities.Add(FileResourcer.Get().GetFile<Entity>(tag.Hash));
                    }
                    else if (reference != 0x8080325a)  // 0x8080325a materials,
                    {
                        throw new NotImplementedException();
                    }
                }
            });
        }

        foreach (var entity in entities)
        {
            foreach (var resourceHash in entity.TagData.EntityResources.Select(entity.GetReader(), r => r.Resource))
            {
                if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && resourceHash.GetReferenceHash() != 0x80800861)
                    continue;

                EntityResource e = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is D2Class_79818080 a)
                {
                    foreach (var d2ClassF1918080 in a.WwiseSounds1)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is D2Class_40668080 b)
                        {
                            sounds.Add(b.GetSound());
                        }
                    }
                    foreach (var d2ClassF1918080 in a.WwiseSounds2)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is D2Class_40668080 b)
                        {
                            sounds.Add(b.GetSound());
                        }
                    }
                }
            }
        }
        return sounds;
    }

    private async Task LoadWeaponAudio(FileHash fileHash)
    {
        var viewer = GetViewer();
        WwiseSound tag = FileResourcer.Get().GetFile<WwiseSound>(fileHash);
        if (tag.TagData.Wems.Count == 0)
            return;
        await viewer.MusicPlayer.SetSound(tag);
        SetExportFunction(ExportSound, (int)ExportTypeFlag.Full);
        // bit of a cheat but works
        var tagItem = _previouslySelected.DataContext as TagItem;
        viewer.ExportControl.SetExportInfo(tagItem.Name == "" ? tagItem.Subname : $"{tagItem.Subname}_{tagItem.Name}", fileHash);
        viewer.MusicPlayer.Play();
    }

    #endregion

    #region Material
    private async Task LoadMaterialList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "caching materials",
            "adding materials to ui",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();

            var mats = PackageResourcer.Get().GetAllHashes<IMaterial>();
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(mats, val =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Material",
                    TagType = ETagListType.Material
                });
            });
            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private void LoadMaterial(FileHash fileHash)
    {
        var materialView = new MaterialView();
        materialView.Load(fileHash);
        _mainWindow.MakeNewTab(fileHash, materialView);
        _mainWindow.SetNewestTabSelected();
    }
    #endregion

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

    public TigerHash Hash { get; set; }

    public string HashString
    {
        get
        {
            if (Name == "BACK")
                return "";
            if (TagType == ETagListType.ApiEntity)
                return $"[{Hash}]";
            if (TagType == ETagListType.Package)
                return $"[{(Hash as FileHash).PackageId:X4}]";
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
