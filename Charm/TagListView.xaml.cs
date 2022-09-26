using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ConcurrentCollections;
using Field;
using Field.Entities;
using Field.General;
using Field.Models;
using Field.Strings;
using Field;
using Microsoft.Toolkit.Mvvm.Input;
using Serilog;

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
    [Description("Script Containers List [Packages]")]
    ScriptContainersList,
    [Description("Script Container [Final]")]
    ScriptContainer,
    [Description("Scripts")]
    Scripts,
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
        public TagHash? Hash;
        public string SearchTerm;
        public ConcurrentBag<TagItem> AllTagItems;
    }
    
    private ConcurrentBag<TagItem> _allTagItems;
    private static MainWindow _mainWindow = null;
    private ETagListType _tagListType;
    private TagHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new Stack<ParentInfo>();
    private bool _bTrimName = true;
    private bool _bShowNamedOnly = false;
    private readonly ILogger _tagListLogger = Log.ForContext<TagListView>();
    private TagListView _tagListControl = null;
    private ToggleButton _previouslySelected = null;
    private int _selectedIndex = -1;
    private FbxHandler _globalFbxHandler = null;

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
        _tagListLogger.Error($"Parent is not a TagListViewerView, is {Parent.GetType().Name}.");
        return null;
    }

    public async void LoadContent(ETagListType tagListType, TagHash contentValue = null, bool bFromBack = false,
        ConcurrentBag<TagItem> overrideItems = null)
    {
        _tagListLogger.Debug($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
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
                    AllTagItems = _allTagItems, Hash = _currentHash, TagListType = _tagListType,
                    SearchTerm = SearchBox.Text
                });
            }

            switch (tagListType)
            {
                case ETagListType.DestinationGlobalTagBagList:
                    LoadDestinationGlobalTagBagList();
                    break;
                case ETagListType.Back:
                    Back_Clicked();
                    return;
                case ETagListType.DestinationGlobalTagBag:
                    LoadDestinationGlobalTagBag(contentValue);
                    break;
                case ETagListType.BudgetSet:
                    LoadBudgetSet(contentValue);
                    break;
                case ETagListType.Entity:
                    LoadEntity(contentValue);
                    break;
                case ETagListType.ApiList:
                    LoadApiList();
                    break;
                case ETagListType.ApiEntity:
                    LoadApiEntity(contentValue);
                    break;
                case ETagListType.EntityList:
                    await LoadEntityList();
                    break;
                case ETagListType.Package:
                    LoadPackage(contentValue);
                    break;
                case ETagListType.ActivityList:
                    LoadActivityList();
                    break;
                case ETagListType.Activity:
                    LoadActivity(contentValue);
                    break;
                case ETagListType.StaticsList:
                    await LoadStaticList();
                    break;
                case ETagListType.Static:
                    LoadStatic(contentValue);
                    break;
                case ETagListType.TextureList:
                    await LoadTextureList();
                    break;
                case ETagListType.Texture:
                    LoadTexture(contentValue);
                    break;
                case ETagListType.DialogueList:
                    LoadDialogueList(contentValue);
                    break;
                case ETagListType.Dialogue:
                    LoadDialogue(contentValue);
                    break;
                case ETagListType.DirectiveList:
                    LoadDirectiveList(contentValue);
                    break;
                case ETagListType.Directive:
                    LoadDirective(contentValue);
                    break;
                case ETagListType.StringContainersList:
                    LoadStringContainersList();
                    break;
                case ETagListType.StringContainer:
                    LoadStringContainer(contentValue);
                    break;
                case ETagListType.Strings:
                    LoadStrings(contentValue);
                    break;
                case ETagListType.String:
                    break;
                case ETagListType.SoundsPackagesList:
                    LoadSoundsPackagesList();
                    break;
                case ETagListType.SoundsPackage:
                    LoadSoundsPackage(contentValue);
                    break;
                case ETagListType.SoundsList:
                    LoadSoundsList(contentValue);
                    break;
                case ETagListType.Sound:
                    LoadSound(contentValue);
                    break;
                case ETagListType.MusicList:
                    LoadMusicList(contentValue);
                    break;
                case ETagListType.Music:
                    LoadMusic(contentValue);
                    break;
                case ETagListType.WeaponAudioGroupList:
                    LoadWeaponAudioGroupList();
                    break;
                case ETagListType.WeaponAudioGroup:
                    LoadWeaponAudioGroup(contentValue);
                    break;
                case ETagListType.WeaponAudioList:
                    LoadWeaponAudioList(contentValue);
                    break;
                case ETagListType.WeaponAudio:
                    LoadWeaponAudio(contentValue);
                    break;
                case ETagListType.ScriptContainersList:
                    await LoadScriptContainersList();
                    break;
                case ETagListType.ScriptContainer:
                    LoadScriptContainer(contentValue);
                    break;
                case ETagListType.Scripts:
                    LoadScripts(contentValue);
                    break;
                // case ETagListType.Script:
                //     //LoadScript(contentValue);
                //     break;
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

        _tagListLogger.Debug(
            $"Loaded content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    }

    /// <summary>
    /// For when we want stuff in packages, we then split up based on what the taghash value is.
    /// I kinda cheat here, I store everything in one massive _allTagItems including the packages
    /// </summary>
    /// <param name="packageId">Package ID for this package to load data for.</param>
    private void LoadPackage(TagHash pkgHash)
    {
        int pkgId = pkgHash.GetPkgId();
        SetBulkGroup(pkgId.ToString("x4"));
        _allTagItems = new ConcurrentBag<TagItem>(_allTagItems.Where(x => x.Hash.GetPkgId() == pkgId && x.TagType != ETagListType.Package));
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
                || item.Hash.GetHashString().ToLower().Contains(searchStr) 
                || item.Hash.Hash.ToString().Contains(searchStr)
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
            packageIds.Add(item.Hash.GetPkgId());
        });
        
        if (bBroken)
            return;
        
        Parallel.ForEach(packageIds, pkgId =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = String.Join('_', PackageHandler.GetPackageName(pkgId).Split('_').Skip(1).SkipLast(1)),
                Hash = new TagHash(PackageHandler.MakeHash(pkgId, 0)),
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
        TagHash tagHash = tagItem.Hash == null ? null : new TagHash(tagItem.Hash);
        if (_previouslySelected != null)
            _previouslySelected.IsChecked = false;
        _selectedIndex = TagList.Items.IndexOf(tagItem);
        // if (_previouslySelected == btn)
        // _previouslySelected.IsChecked = !_previouslySelected.IsChecked;
        _previouslySelected = btn;
        LoadContent(tagItem.TagType, tagHash);
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
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex+1));
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
            var nextButton = GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex-1));
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
        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i+1}/{exportItems.Count}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (var tagItem in exportItems)
            {
                var name = tagItem.Name == String.Empty ? tagItem.Hash.GetHashString() : tagItem.Name;
                var exportInfo = new ExportInfo
                {
                    Hash = tagItem.Hash,
                    Name = $"/Bulk_{groupName}/{name}",
                    ExportType = EExportTypeFlag.Minimal
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
    /// Type 0x80809830/0x80809875 and only in sr_globals_010a.
    /// </summary>
    private void LoadDestinationGlobalTagBagList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        var vals = PackageHandler.GetAllEntriesOfReference(0x010a, 0x80809875);
        Parallel.ForEach(vals, val =>
        {
            Tag<D2Class_75988080> dgtbParent = PackageHandler.GetTag<D2Class_75988080>(val);
            if (!dgtbParent.Header.DestinationGlobalTagBag.IsValid())
                return;
            _allTagItems.Add(new TagItem 
            { 
                Hash = dgtbParent.Header.DestinationGlobalTagBag,
                // Name = dgtbParent.Header.DestinationGlobalTagBagName,
                TagType = ETagListType.DestinationGlobalTagBag
            });
        });
    }
    
    private void LoadDestinationGlobalTagBag(TagHash hash)
    {
        Tag<D2Class_30898080> destinationGlobalTagBag = PackageHandler.GetTag<D2Class_30898080>(hash);
        
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(destinationGlobalTagBag.Header.Unk18, val =>
        {
            if (val.Tag == null)
                return;
            TagHash reference = PackageHandler.GetEntryReference(val.Tag.Hash);
            ETagListType tagType;
            string overrideType = String.Empty;
            switch (reference.Hash)
            {
                case 0x8080987e:
                    tagType = ETagListType.BudgetSet;
                    break;
                case 0x80809ad8:
                    tagType = ETagListType.Entity;
                    break;
                default:
                    tagType = ETagListType.None;
                    overrideType = reference.GetHashString();
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
    
    private void LoadBudgetSet(TagHash hash)
    {
        Tag<D2Class_7E988080> budgetSetHeader = PackageHandler.GetTag<D2Class_7E988080>(hash);
        Tag<D2Class_ED9E8080> budgetSet = PackageHandler.GetTag<D2Class_ED9E8080>(budgetSetHeader.Header.Unk00.Hash);
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(budgetSet.Header.Unk28, val =>
        {
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

    private void LoadEntity(TagHash tagHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        bool bLoadedSuccessfully = viewer.EntityControl.LoadEntity(tagHash, _globalFbxHandler);
        if (!bLoadedSuccessfully)
        {
            _tagListLogger.Error($"UI failed to load entity for hash {tagHash}. You can still try to export the full model instead.");
            _mainWindow.SetLoggerSelected();
        }
        SetExportFunction(ExportEntity, (int)EExportTypeFlag.Full | (int)EExportTypeFlag.Minimal);
        viewer.ExportControl.SetExportInfo(tagHash);
        viewer.EntityControl.ModelView.SetModelFunction(() => viewer.EntityControl.LoadEntity(tagHash, _globalFbxHandler));
    }
    
    private void ExportEntity(ExportInfo info)
    {
        var viewer = GetViewer();
        Entity entity = PackageHandler.GetTag(typeof(Entity), new TagHash(info.Hash));
        EntityView.Export(new List<Entity> {entity}, info.Name, info.ExportType);
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
            "loading global tag bags",
            "loading budget sets",
            "caching entity tags",
            "loading entities"
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            // only in 010a
            var dgtbVals = PackageHandler.GetAllEntriesOfReference(0x010a, 0x80808930);
            // only in the sr_globals, not best but it works
            var bsVals = PackageHandler.GetAllEntriesOfReference(0x010f, 0x80809eed);
            bsVals.AddRange(PackageHandler.GetAllEntriesOfReference(0x011a, 0x80809eed));
            bsVals.AddRange( PackageHandler.GetAllEntriesOfReference(0x0312, 0x80809eed));
            // everywhere
            var eVals = PackageHandler.GetAllTagsWithReference(0x80809ad8);
            ConcurrentHashSet<uint> existingEntities = new ConcurrentHashSet<uint>();
            Parallel.ForEach(dgtbVals, val =>
            {
                Tag<D2Class_30898080> dgtb = PackageHandler.GetTag<D2Class_30898080>(val);
                foreach (var entry in dgtb.Header.Unk18)
                {
                    if (entry.Tag == null)
                        continue;
                    if (entry.TagPath.Contains(".pattern.tft"))
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = entry.Tag.Hash,
                            Name = entry.TagPath,
                            Subname = entry.TagNote,
                            TagType = ETagListType.Entity
                        });
                        existingEntities.Add(entry.Tag.Hash);
                    }
                }
            });
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(bsVals, val =>
            {
                Tag<D2Class_ED9E8080> bs = PackageHandler.GetTag<D2Class_ED9E8080>(val);
                foreach (var entry in bs.Header.Unk28)
                {
                    if (entry.TagPath.Contains(".pattern.tft"))
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = entry.Tag.Hash,
                            Name = entry.TagPath,
                            TagType = ETagListType.Entity
                        });
                        existingEntities.Add(entry.Tag.Hash);
                    }
                }
            });
            MainWindow.Progress.CompleteStage();

            // We could also cache all the entity resources for an extra speed-up, but should be careful of memory there
            PackageHandler.CacheHashDataList(eVals.Select(x => x.Hash).ToArray());
            var erVals = PackageHandler.GetAllTagsWithReference(0x80809b06);
            PackageHandler.CacheHashDataList(erVals.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();

            
            Parallel.ForEach(eVals, val =>
            {
                if (existingEntities.Contains(val)) // O(1) check
                    return; 
            
                // Check the entity has geometry
                bool bHasGeometry = false;
                using (var handle = new Tag(val).GetHandle())
                {
                    handle.BaseStream.Seek(8, SeekOrigin.Begin);
                    int resourceCount = handle.ReadInt32();
                    if (resourceCount > 2)
                    {
                        handle.BaseStream.Seek(0x10, SeekOrigin.Begin);
                        int resourcesOffset = handle.ReadInt32() + 0x20;
                        for (int i = 0; i < 2; i++)
                        {
                            handle.BaseStream.Seek(resourcesOffset + i * 0xC, SeekOrigin.Begin);
                            using (var handle2 = new Tag(new TagHash(handle.ReadUInt32())).GetHandle())
                            {
                                handle2.BaseStream.Seek(0x10, SeekOrigin.Begin);
                                int checkOffset = handle2.ReadInt32() + 0x10 - 4;
                                handle2.BaseStream.Seek(checkOffset, SeekOrigin.Begin);
                                if (handle2.ReadUInt32() == 0x80806d8a)
                                {
                                    bHasGeometry = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!bHasGeometry)
                    return;
            
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    TagType = ETagListType.Entity
                });
            });
            MainWindow.Progress.CompleteStage();
            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    #endregion

    #region API
    
    private void LoadApiList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(InvestmentHandler.InventoryItems, kvp =>
        {
            if (kvp.Value.GetArtArrangementIndex() == -1) return;
            string name = InvestmentHandler.GetItemName(kvp.Value);
            string type = InvestmentHandler.InventoryItemStringThings[InvestmentHandler.GetItemIndex(kvp.Key)].Header.ItemType;
            if (type == "Finisher" || type.Contains("Emote"))
                return;  // they point to Animation instead of Entity
            _allTagItems.Add(new TagItem 
            {
                Hash = kvp.Key,
                Name = name,
                Type = type.Trim(),
                TagType = ETagListType.ApiEntity
            });  // for some reason some of the types have spaces after
        });
    }
    
    private void LoadApiEntity(DestinyHash apiHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        viewer.EntityControl.LoadEntityFromApi(apiHash, _globalFbxHandler);
        Dispatcher.Invoke(() =>
        {
            SetExportFunction(ExportApiEntityFull, (int)EExportTypeFlag.Full | (int)EExportTypeFlag.Minimal);
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
        EntityView.Export(InvestmentHandler.GetEntitiesFromHash(info.Hash), info.Name, info.ExportType);
    }

    #endregion

    #region Activity

    /// <summary>
    /// Type 0x80808e8e, but we use a child of it (0x80808e8b) so we can get the location.
    /// </summary>
    private void LoadActivityList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        // Getting names
        var valsChild = PackageHandler.GetAllTagsWithReference(0x80808e8b);
        ConcurrentDictionary<string, string> names = new ConcurrentDictionary<string, string>();
        Parallel.ForEach(valsChild, val =>
        {
            Tag<D2Class_8B8E8080> tag = PackageHandler.GetTag<D2Class_8B8E8080>(val);
            foreach (var entry in tag.Header.Activities)
            {
                names.TryAdd(entry.ActivityName, tag.Header.LocationName);
            }
        });
        
        var vals = PackageHandler.GetAllTagsWithReference(0x80808e8e);
        Parallel.ForEach(vals, val =>
        {
            var activityName = PackageHandler.GetActivityName(val);
            _allTagItems.Add(new TagItem 
            { 
                Hash = val,
                Name = activityName,
                Subname = names.ContainsKey(activityName) && !names[activityName].StartsWith("%%NOGLOBALSTRING") ? names[activityName] : "",
                TagType = ETagListType.Activity
            });
        });
    }

    private void LoadActivity(TagHash tagHash)
    {
        ActivityView activityView = new ActivityView();
        _mainWindow.MakeNewTab(PackageHandler.GetActivityName(tagHash), activityView);
        activityView.LoadActivity(tagHash);
        _mainWindow.SetNewestTabSelected();
        // ExportControl.SetExportFunction(ExportActivityMapFull);
        // ExportControl.SetExportInfo(tagHash);
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

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var eVals = PackageHandler.GetAllTagsWithReference(0x80806d44);
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
    
    private void LoadStatic(TagHash tagHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Static);
        viewer.StaticControl.LoadStatic(tagHash, viewer.StaticControl.ModelView.GetSelectedLod());
        SetExportFunction(ExportStatic, (int)EExportTypeFlag.Full | (int)EExportTypeFlag.Minimal);
        viewer.ExportControl.SetExportInfo(tagHash);
        viewer.StaticControl.ModelView.SetModelFunction(() => viewer.StaticControl.LoadStatic(tagHash, viewer.StaticControl.ModelView.GetSelectedLod()));
    }
    
    private void ExportStatic(ExportInfo info)
    {
        var viewer = GetViewer();
        viewer.StaticControl.ExportStatic(new TagHash(info.Hash), info.Name, info.ExportType);
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
            "caching textures 1d",
            "caching textures 2d",
            "caching textures 3d",
            "adding textures to ui 1d",
            "adding textures to ui 2d",
            "adding textures to ui 3d",
        });
        
        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var tex1d = PackageHandler.GetAllTagsWithTypes(32, 1);
            var tex2d = PackageHandler.GetAllTagsWithTypes(32, 2);
            var tex3d = PackageHandler.GetAllTagsWithTypes(32, 3);
            PackageHandler.CacheHashDataList(tex1d.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();
            PackageHandler.CacheHashDataList(tex2d.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();
            PackageHandler.CacheHashDataList(tex3d.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(tex1d, val =>
            {
                using (var handle = new Tag(val).GetHandle())
                {
                    handle.BaseStream.Seek(0x22, SeekOrigin.Begin);
                    _allTagItems.Add(new TagItem
                    {
                        Hash = val,
                        Name = $"Texture 1D {handle.ReadUInt16()} x {handle.ReadUInt16()}",
                        TagType = ETagListType.Texture
                    });
                }
            });
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(tex2d, val =>
            {
                using (var handle = new Tag(val).GetHandle())
                {
                    handle.BaseStream.Seek(0x22, SeekOrigin.Begin);
                    _allTagItems.Add(new TagItem
                    {
                        Hash = val,
                        Name = $"Texture 2D {handle.ReadUInt16()} x {handle.ReadUInt16()}",
                        TagType = ETagListType.Texture
                    });
                }
            });
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(tex3d, val =>
            {
                using (var handle = new Tag(val).GetHandle())
                {
                    handle.BaseStream.Seek(0x22, SeekOrigin.Begin);
                    _allTagItems.Add(new TagItem
                    {
                        Hash = val,
                        Name = $"Texture 3D {handle.ReadUInt16()} x {handle.ReadUInt16()}",
                        TagType = ETagListType.Texture
                    });
                }
            });
            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    /// <summary>
    /// I could do it tiled, but cba to bother with it when you can just batch export to filesystem.
    /// </summary>
    private void LoadTexture(TagHash tagHash)
    {
        var viewer = GetViewer();
        TextureHeader textureHeader = PackageHandler.GetTag(typeof(TextureHeader), tagHash);
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
        SetExportFunction(ExportTexture, (int)EExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(tagHash);
    }
    
    private void ExportTexture(ExportInfo info)
    {
        TextureView.ExportTexture(new TagHash(info.Hash));
    }
    
    #endregion

    #region Dialogue

    /// <summary>
    /// We assume all dialogue tables come from activities.
    /// </summary>
    private void LoadDialogueList(TagHash tagHash)
    {
        Field.Activity activity = PackageHandler.GetTag(typeof(Field.Activity), tagHash);
        _allTagItems = new ConcurrentBag<TagItem>();
  
        // Dialogue tables can be in the 0x80808948 entries
        ConcurrentBag<TagHash> dialogueTables = new ConcurrentBag<TagHash>();
        if (activity.Header.Unk18 is D2Class_6A988080)
        {
            var entry = (D2Class_6A988080) activity.Header.Unk18;
            if (entry.DialogueTable != null)
                dialogueTables.Add(entry.DialogueTable.Hash);
        }
        Parallel.ForEach(activity.Header.Unk50, val =>
        {
            foreach (var d2Class48898080 in val.Unk18)
            {
                var resource = d2Class48898080.UnkEntityReference.Header.Unk10;
                if (resource is D2Class_D5908080 || resource is D2Class_44938080 || resource is D2Class_45938080 ||
                    resource is D2Class_18978080 || resource is D2Class_19978080)
                {
                    if (resource.DialogueTable != null)
                        dialogueTables.Add(resource.DialogueTable.Hash);
                }
            }
        });

        Parallel.ForEach(dialogueTables, hash =>
        {
            _allTagItems.Add(new TagItem 
            { 
                Hash = hash,
                Name = hash,
                TagType = ETagListType.Dialogue
            });
        });
    }


    // TODO replace this by deleting DialogueControl and using TagList instead
    private void LoadDialogue(TagHash tagHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Dialogue);
        viewer.DialogueControl.Load(tagHash);
    }

    #endregion
    
    #region Directive
    
    private void LoadDirectiveList(TagHash tagHash)
    {
        Field.Activity activity = PackageHandler.GetTag(typeof(Field.Activity), tagHash);
        _allTagItems = new ConcurrentBag<TagItem>();
  
        // Dialogue tables can be in the 0x80808948 entries
        if (activity.Header.Unk18 is D2Class_6A988080)
        {
            var directiveTables =
                ((D2Class_6A988080) activity.Header.Unk18).DirectiveTables.Select(x => x.DialogueTable.Hash);
            
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
        else if (activity.Header.Unk18 is D2Class_20978080)
        {
            var directiveTables =
                ((D2Class_20978080) activity.Header.Unk18).PEDirectiveTables.Select(x => x.DialogueTable.Hash);
            
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
    }

    // TODO replace with taglist control
    private void LoadDirective(TagHash tagHash)
    {
        SetViewer(TagView.EViewerType.Directive);
        var viewer = GetViewer();
        viewer.DirectiveControl.Load(tagHash);
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
        
        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var vals = PackageHandler.GetAllTagsWithReference(0x808099ef);
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
    
    private void LoadStringContainer(TagHash tagHash)
    { 
        SetViewer(TagView.EViewerType.TagList);
        var viewer = GetViewer();
        viewer.TagListControl.LoadContent(ETagListType.Strings, tagHash, true);
    }
    
    // Would be nice to do something with colour formatting.
    private void LoadStrings(TagHash tagHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        StringContainer stringContainer = PackageHandler.GetTag(typeof(StringContainer), tagHash);
        Parallel.ForEach(stringContainer.Header.StringHashTable, hash =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = stringContainer.GetStringFromHash(ELanguage.English, hash),
                Hash = hash,
                TagType = ETagListType.String
            });
        });
        RefreshItemList();
    }

    #endregion
    
    #region Scripts
    private async Task LoadScriptContainersList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;
        
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "caching script tags",
            "load script list",
        });
        
        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var vals = PackageHandler.GetAllTagsWithReference(0x80809212);
            //PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(vals, val =>
            {
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"{val.Hash}",
                    TagType = ETagListType.ScriptContainer
                });
            });
            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private void LoadScriptContainer(TagHash tagHash)
    { 
        LoadContent(ETagListType.Scripts, tagHash, true);
    }
    
    private void LoadScripts(TagHash tagHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        if(tagHash.Hash == null)
            return;

        Script script = PackageHandler.GetTag(typeof(Script), tagHash);
        // Parallel.ForEach(script.Header.Unk08, hash =>
        // {
        //     _allTagItems.Add(new TagItem
        //     {
        //         Name = script.ConvertToString(),
        //         Hash = hash.Unk00,
        //         TagType = ETagListType.Script
        //     });
        // });
        
        SetViewer(TagView.EViewerType.Script);
        var viewer = GetViewer();
        viewer.ScriptControl.Load(tagHash);

        SetExportFunction(ExportScript, (int)EExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(tagHash);

    }
    private void ExportScript(ExportInfo info)
    {
        Script script = PackageHandler.GetTag(typeof(Script), info.Hash as TagHash);
        string scriptString = script.ConvertToString();
        string fileName = $"{info.Hash}.txt";
        string saveDirectory = ConfigHandler.GetExportSavePath() + $"/Scripts/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        File.WriteAllText(saveDirectory + fileName, scriptString);
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
            var vals = PackageHandler.GetAllTagsWithTypes(26, 7);
            MainWindow.Progress.CompleteStage();

            ConcurrentHashSet<int> packageIds = new ConcurrentHashSet<int>();
            Parallel.ForEach(vals, hash =>
            {
                packageIds.Add(hash.GetPkgId());
            });
            
            Parallel.ForEach(packageIds, pkgId =>
            {
                _allTagItems.Add(new TagItem
                {
                    Name = String.Join('_', PackageHandler.GetPackageName(pkgId).Split('_').Skip(1).SkipLast(1)),
                    Hash = new TagHash(PackageHandler.MakeHash(pkgId, 0)),
                    TagType = ETagListType.SoundsPackage
                });
            });
        });

        MainWindow.Progress.CompleteStage();
        RefreshItemList();  // bc of async stuff
    }
    
    private void LoadSoundsPackage(TagHash tagHash)
    { 
        SetViewer(TagView.EViewerType.TagList);
        var viewer = GetViewer();
        viewer.MusicPlayer.Visibility = Visibility.Visible;
        viewer.TagListControl.LoadContent(ETagListType.SoundsList, tagHash, true);
    }
    
    private async void LoadSoundsList(TagHash tagHash)
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "loading sounds",
        });
        
        await Task.Run(() =>
        {
            var vals = PackageHandler.GetTagsWithTypes(tagHash.GetPkgId(), 26, 7);
            PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
            _allTagItems = new ConcurrentBag<TagItem>();
            Parallel.ForEach(vals, hash =>
            {
                Wem wem = PackageHandler.GetTag(typeof(Wem), hash);
                if (wem.GetData().Length == 1)
                    return;
                _allTagItems.Add(new TagItem
                {
                    Name = PackageHandler.GetEntryReference(hash),
                    Hash = hash,
                    Subname = wem.Duration,
                    TagType = ETagListType.Sound
                });
            });
        });
        
        MainWindow.Progress.CompleteStage();
        RefreshItemList();
    }
    
    private void LoadSound(TagHash tagHash)
    {
        var viewer = GetViewer();
        if (viewer.MusicPlayer.SetWem(PackageHandler.GetTag(typeof(Wem), tagHash)))
        {
            viewer.MusicPlayer.Play();
            SetExportFunction(ExportWem, (int)EExportTypeFlag.Full);
            viewer.ExportControl.SetExportInfo(tagHash);
        }
    }
    
    private void ExportSound(ExportInfo info)
    {
        WwiseSound sound = PackageHandler.GetTag(typeof(WwiseSound), new TagHash(info.Hash));
        string saveDirectory = ConfigHandler.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        sound.ExportSound(saveDirectory);
    }
    
    private void ExportWem(ExportInfo info)
    {
        Wem wem = PackageHandler.GetTag(typeof(Wem), new TagHash(info.Hash));
        string saveDirectory = ConfigHandler.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wem");
    }

    #endregion

    #region Music

    /// <summary>
    /// We assume all music tables come from activities.
    /// </summary>
    private void LoadMusicList(TagHash tagHash)
    {
        Field.Activity activity = PackageHandler.GetTag(typeof(Field.Activity), tagHash);
        _allTagItems = new ConcurrentBag<TagItem>();
  
        ConcurrentBag<TagHash> musics = new ConcurrentBag<TagHash>();
        Parallel.ForEach(activity.Header.Unk50, val =>
        {
            foreach (var d2Class48898080 in val.Unk18)
            {
                var resource = d2Class48898080.UnkEntityReference.Header.Unk10;
                if (resource is D2Class_D5908080)
                {
                    var res = (D2Class_D5908080)resource;
                    if (res.Music != null)
                    {
                        musics.Add(res.Music.Hash);
                    }
                }
            }
        });

        if (activity.Header.Unk18 is D2Class_6A988080)
        {
            if (((D2Class_6A988080) activity.Header.Unk18).Music != null)
                musics.Add(((D2Class_6A988080) activity.Header.Unk18).Music.Hash);
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

    private void LoadMusic(TagHash tagHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.Music);
        viewer.MusicControl.Load(tagHash);
    }

    #endregion
    
    #region Weapon Audio
    
    private void LoadWeaponAudioGroupList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(InvestmentHandler.InventoryItems, kvp =>
        {
            if (kvp.Value.GetWeaponPatternIndex() == -1) 
                return;
            string name = InvestmentHandler.GetItemName(kvp.Value);
            string type = InvestmentHandler.InventoryItemStringThings[InvestmentHandler.GetItemIndex(kvp.Key)].Header.ItemType;
            if (type == "Vehicle" || type == "Ship")
                return;
            _allTagItems.Add(new TagItem 
            {
                Hash = kvp.Key,
                Name = name,
                Type = type.Trim(),
                TagType = ETagListType.WeaponAudioGroup
            });
        });
    }
    
    private void LoadWeaponAudioGroup(TagHash tagHash)
    {
        var viewer = GetViewer();
        SetViewer(TagView.EViewerType.TagList);
        viewer.TagListControl.LoadContent(ETagListType.WeaponAudioList, tagHash, true);
        viewer.MusicPlayer.Visibility = Visibility.Visible;
    }
    
    private void LoadWeaponAudioList(DestinyHash apiHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        var val = InvestmentHandler.GetPatternEntityFromHash(apiHash);
        if (val == null || (val.PatternAudio == null && val.PatternAudioUnnamed == null))
        {
            RefreshItemList();
            return;
        }

        var resourceUnnamed = (D2Class_F42C8080)val.PatternAudioUnnamed.Header.Unk18;
        var resource = (D2Class_6E358080)val.PatternAudio.Header.Unk18;
        var item = InvestmentHandler.GetInventoryItem(apiHash);
        var weaponContentGroupHash = InvestmentHandler.GetWeaponContentGroupHash(item);
        // Named
        foreach (var entry in resource.PatternAudioGroups)
        {
            if (entry.WeaponContentGroup1Hash.Equals(weaponContentGroupHash) && entry.AudioGroup != null)
            {
                var audioGroup = PackageHandler.GetTag<D2Class_0D8C8080>(entry.AudioGroup.Header.EntityData);
                audioGroup.Header.Audio.ForEach(audio =>
                {
                    foreach (var s in audio.Sounds)
                    {
                        if (s.Sound == null)
                            continue;
                    
                        _allTagItems.Add(new TagItem
                        {
                            Hash = s.Sound.Hash,
                            Name = s.WwiseEventName,
                            Subname = audio.WwiseEventHash,
                            TagType = ETagListType.WeaponAudio
                        });
                    }
                });
            }
        }
        // Unnamed
        var sounds = GetWeaponUnnamedSounds(resourceUnnamed, weaponContentGroupHash);
        foreach (var s in sounds)
        {
            if (s == null)
                continue;
                    
            _allTagItems.Add(new TagItem
            {
                Hash = s.Hash,
                Subname = s.Hash,
                TagType = ETagListType.WeaponAudio
            });
        }
        
        RefreshItemList();
    }

    public List<WwiseSound> GetWeaponUnnamedSounds(D2Class_F42C8080 resource, DestinyHash weaponContentGroupHash)
    {
        List<WwiseSound> sounds = new List<WwiseSound>();
        resource.PatternAudioGroups.ForEach(entry =>
        {
            if (!entry.WeaponContentGroupHash.Equals(weaponContentGroupHash))
                return;
            
            List<Tag> entitiesParents = new List<Tag> {entry.Unk60, entry.Unk78, entry.Unk90, entry.UnkA8, entry.UnkC0, entry.UnkD8, entry.AudioEntityParent, entry.Unk130, entry.Unk148, entry.Unk1C0, entry.Unk1D8, entry.Unk248};
            List<Entity> entities = new List<Entity>();
            foreach (var tag in entitiesParents)
            {
                if (tag == null)
                    continue;
                var reference = PackageHandler.GetEntryReference(tag.Hash);
                if (reference == 0x80806fa3)
                {
                    var entityData = PackageHandler.GetTag<D2Class_A36F8080>(tag.Hash).Header.EntityData;
                    var reference2 = PackageHandler.GetEntryReference(entityData);
                    if (reference2 == 0x80802d09)
                    {
                        var tagInner = PackageHandler.GetTag<D2Class_2D098080>(entityData);
                        if (tagInner.Header.Unk18 != null)
                            entities.Add(tagInner.Header.Unk18);
                        if (tagInner.Header.Unk30 != null)
                            entities.Add(tagInner.Header.Unk30);
                        if (tagInner.Header.Unk48 != null)
                            entities.Add(tagInner.Header.Unk48);
                        if (tagInner.Header.Unk60 != null)
                            entities.Add(tagInner.Header.Unk60);
                        if (tagInner.Header.Unk78 != null)
                            entities.Add(tagInner.Header.Unk78);
                        if (tagInner.Header.Unk90 != null)
                            entities.Add(tagInner.Header.Unk90);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (reference == 0x80809ad8)
                {
                    entities.Add(PackageHandler.GetTag(typeof(Entity), tag.Hash));
                }
                else if (reference != 0x8080325a)  // 0x8080325a materials, 
                {
                    throw new NotImplementedException();
                }
            }
            foreach (var entity in entities)
            {
                foreach (var e in entity.Header.EntityResources)
                {
                    if (e.ResourceHash.Header.Unk18 is D2Class_79818080 a)
                    {
                        foreach (var d2ClassF1918080 in a.WwiseSounds1)
                        {
                            if (d2ClassF1918080.Unk10 is D2Class_40668080 b)
                            {
                                sounds.Add(b.Sound);
                            }
                        }
                        foreach (var d2ClassF1918080 in a.WwiseSounds2)
                        {
                            if (d2ClassF1918080.Unk10 is D2Class_40668080 b)
                            {
                                sounds.Add(b.Sound);
                            }
                        }
                    }
                }
            }
        });
        return sounds;
    }
    
    private async void LoadWeaponAudio(TagHash tagHash)
    {
        var viewer = GetViewer();
        WwiseSound tag = PackageHandler.GetTag(typeof(WwiseSound), tagHash);
        if (tag.Header.Unk20.Count == 0)
            return;
        await viewer.MusicPlayer.SetSound(tag);
        SetExportFunction(ExportSound, (int)EExportTypeFlag.Full);
        // bit of a cheat but works
        var tagItem = _previouslySelected.DataContext as TagItem;
        viewer.ExportControl.SetExportInfo(tagItem.Name == "" ? tagItem.Subname : $"{tagItem.Subname}_{tagItem.Name}", tagHash);
        viewer.MusicPlayer.Play();
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
    
    public DestinyHash Hash { get; set; }

    public string HashString
    {
        get
        {
            if (Name == "BACK")
                return "";
            if (TagType == ETagListType.ApiEntity)
                return $"[{Hash.Hash}]";
            if (TagType == ETagListType.Package)
                return $"[{Hash.GetPkgId():X4}]";
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