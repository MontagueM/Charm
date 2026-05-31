using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Arithmic;
using ConcurrentCollections;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Strings;
using ActivityROI = Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON.Activity;
using ActivitySK = Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601.Activity;
using ActivityWQ = Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402.Activity;

namespace Charm;

public enum ETagListType
{
    [Description("None")]
    None,
    [Description("BACK")]
    Back,
    [Description("Package")]
    Package,

    [Description("Destination Global Tag Bag List")]
    DestinationGlobalTagBagList,
    [Description("Destination Global Tag Bag")]
    DestinationGlobalTagBag,

    [Description("Budget Set")]
    BudgetSet,
    [Description("Entity List [Packages]")]
    EntityList,
    [Description("Entity [Final]")]
    Entity,

    [Description("Texture [Final]")]
    Texture,

    [Description("Activity List")]
    ActivityList,
    [Description("Activity [Final]")]
    Activity,

    [Description("Dialogue List")]
    DialogueList,
    [Description("Dialogue [Final]")]
    Dialogue,

    [Description("Directive List")]
    DirectiveList,
    [Description("Directive [Final]")]
    Directive,

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

    [Description("Material List [Packages]")]
    MaterialList,
    [Description("Material [Final]")]
    Material,

    [Description("String Containers List [Packages]")]
    StringContainersList,
    [Description("String Container [Final]")]
    StringContainer,
    [Description("Strings")]
    Strings,
    [Description("String [Final]")]
    String,
}

// TODO Start phasing this out for some things (already done for Texture and Audio viewing).
// Its a nice system for basic things like strings or materials but it's nice to have
// more control over things / more customization when needed

/// <summary>
/// The current implementation of Package is limited so you cannot have nested views below a Package.
/// For future, would be better to split the tag items up so we can cache them based on parents.
/// </summary>
public partial class TagListView : UserControl
{
    private struct ParentInfo
    {
        public string ParentName;
        public ETagListType TagListType;
        public TigerHash? Hash;
        public string SearchTerm;
        public ConcurrentBag<TagItem> AllTagItems;
    }

    private ConcurrentBag<TagItem> _allTagItems;
    private static MainWindow _mainWindow = null;
    private ETagListType _tagListType;
    private TigerHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new();
    private bool _bTrimName = true;
    private bool _bShowNamedOnly = false;
    private TagListView _tagListControl = null;
    private ToggleButton _previouslySelected = null;
    private int _selectedIndex = -1;
    private string _weaponItemName = null;

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
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
        ConcurrentBag<TagItem> overrideItems = null, TagItem fullTag = null)
    {
        Log.Verbose($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
        if (overrideItems != null)
        {
            _allTagItems = overrideItems;
        }
        else
        {
            if (contentValue != null && !bFromBack && !EnumExtensions.GetEnumDescription(tagListType).Contains("[Final]")) // if the type nests no new info, it isnt a parent
            {
                _parentStack.Push(new ParentInfo
                {
                    ParentName = fullTag?.Name ?? "",
                    AllTagItems = _allTagItems,
                    Hash = _currentHash,
                    TagListType = _tagListType,
                    SearchTerm = SearchBox.Text
                });
            }

            switch (tagListType)
            {
                case ETagListType.Back:
                    Back_Clicked();
                    return;
                case ETagListType.DestinationGlobalTagBagList:
                    await LoadDestinationGlobalTagBagList();
                    break;
                case ETagListType.DestinationGlobalTagBag:
                    LoadDestinationGlobalTagBag(contentValue as FileHash);
                    break;
                case ETagListType.BudgetSet:
                    LoadBudgetSet(contentValue as FileHash);
                    break;
                case ETagListType.Entity:
                    LoadEntity(contentValue as FileHash);
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
                case ETagListType.Sound:
                    LoadSound(contentValue as FileHash);
                    break;
                case ETagListType.MusicList:
                    LoadMusicList(contentValue as FileHash);
                    break;
                case ETagListType.Music:
                    LoadMusic(contentValue as FileHash, fullTag);
                    break;

                case ETagListType.MaterialList:
                    await LoadMaterialList();
                    break;
                case ETagListType.Material:
                    LoadMaterial(contentValue as FileHash);
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

                default:
                    throw new NotImplementedException();
            }
        }

        if (!EnumExtensions.GetEnumDescription(tagListType).Contains("[Final]"))
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
        if (Strategy.IsD1() && pkgId == 0x0180)
            MessageBox.Show($"This pkg contains entries that CAN/WILL cause crashes!!\nNot worth fixing at the moment, sorry. Blame Bungie.", "¯\\_(ツ)_/¯", MessageBoxButton.OK, MessageBoxImage.Warning);

        SetBulkGroup(pkgId.ToString("x4"));
        var collection = _allTagItems.Where(x => (x.Hash as FileHash).PackageId == pkgId && x.TagType != ETagListType.Package).ToList();
        _allTagItems = new ConcurrentBag<TagItem>(collection);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {

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

            if (EnumExtensions.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
            {
                // Package-enabled lists have [Packages] in their enum
                if (item.TagType != ETagListType.Package)
                {
                    return;
                }
            }

            string name = item.Name != "" ? item.Name : item.Hash;
            bool bWasTrimmed = false;
            if (item.Name.Contains('\\') && _bTrimName)
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
                Package pkg = (item.Hash as FileHash) is not null ? PackageResourcer.Get().GetPackage((item.Hash as FileHash).PackageId) : null;
                if (pkg is not null && (item.Hash as FileHash).IsRedacted) //&& pkg.GetPackageMetadata().Name.Contains("redacted"))
                    name = $"🔐 {name}";

                string subname = searchStr != string.Empty && item.Type != "Package" ?
                            $"{item.Subname}" + (pkg != null ? $" : [{pkg.GetPackageMetadata().Name}]" : "")
                            : item.Subname;

                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    Subname = subname,
                    FontSize = _bTrimName || !bWasTrimmed ? 16 : 12,
                    Extra = item.Extra
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

        if (displayItems.Count == 0 && EnumExtensions.GetEnumDescription(_tagListType).Contains("[Packages]") && !bPackageSearchAllOverride)
        {
            SetItemListByString(searchStr, true);
            return;
        }

        List<TagItem> tagItems = displayItems.ToList();
        if (tagItems.Count != 0 && tagItems.First().Type == "Package")
        {
            tagItems.Sort((p, q) => string.Compare(p.Name, q.Name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            tagItems = tagItems.OrderBy(x => x.Hash.Hash32).ToList();
        }

        // If we have a parent, add a TagItem that is actually a back button as first
        if (_parentStack.Count > 0)
        {
            tagItems.Insert(0, new TagItem
            {
                Name = "BACK",
                Subname = $"{_parentStack.First().ParentName}",
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
        ConcurrentHashSet<int> packageIds = new();
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
        string searchStr = SearchBox.Text;

        // Flips tag hash to the "intended" way (sigh) ex 80BB6216 -> 1662BB80
        if (Helpers.ParseHash(searchStr, out uint parsedHash))
        {
            searchStr = new TigerHash(parsedHash).ToString();
        }
        SetItemListByString(searchStr.ToLower());
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

        Package pkg = (tagItem.Hash as FileHash) is not null ? PackageResourcer.Get().GetPackage((tagItem.Hash as FileHash).PackageId) : null;
        if (pkg is not null && (tagItem.Hash as FileHash).IsRedacted)
        {
            if (!PackageResourcer.Get().Keys.ContainsKey(pkg.GetPackageMetadata().PackageGroup))
            {
                //MessageBox.Show($"No decryption key found, can not display content.", $"This item belongs to a redacted package.", MessageBoxButton.OK);
                PopupBanner.ShowRedactedPopup();

                btn.IsChecked = false;
                return;
            }
        }
        LoadContent(tagItem.TagType, tigerHash, fullTag: tagItem);
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
        TagView viewer = GetViewer();
        viewer.SetViewer(eViewerType);
    }

    private void TagList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectedIndex == -1)
            return;
        if (TagList.SelectedIndex > _selectedIndex)
        {
            ToggleButton currentButton = UIHelper.GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            ToggleButton nextButton = UIHelper.GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex + 1));
            if (nextButton == null)
                return;
            nextButton.IsChecked = true;
            _selectedIndex++;
            TagItem_OnClick(nextButton, null);
        }

        else if (TagList.SelectedIndex < _selectedIndex)
        {
            ToggleButton currentButton = UIHelper.GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex));
            if (currentButton == null)
                return;
            currentButton.IsChecked = false;
            ToggleButton nextButton = UIHelper.GetChildOfType<ToggleButton>(TagList.ItemContainerGenerator.ContainerFromIndex(_selectedIndex - 1));
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

        string? groupName = BulkExportButton.Tag as string;
        TagView viewer = GetViewer();
        bool bStaticShowing = viewer.StaticControl.Visibility == Visibility.Visible;
        bool bEntityShowing = viewer.EntityControl.Visibility == Visibility.Visible;
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Hidden : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Hidden : viewer.EntityControl.Visibility;

        // Iterate over all buttons and export it
        IEnumerable<TagItem> items = TagList.ItemsSource.Cast<TagItem>();
        var exportItems = items.Where(x => x.TagType is not ETagListType.Back and not ETagListType.Package).ToList();
        if (exportItems.Count == 0)
        {
            MessageBox.Show("No tags to export.");
            return;
        }
        MainWindow.Progress.SetProgressStages(exportItems.Select((x, i) => $"Exporting {i + 1}/{exportItems.Count}: {x.Hash}").ToList());
        await Task.Run(() =>
        {
            foreach (TagItem? tagItem in exportItems)
            {
                string name = tagItem.Name == String.Empty ? tagItem.Hash : tagItem.Name;
                var exportInfo = new ExportInfo
                {
                    Hash = tagItem.Hash as FileHash,
                    Name = name,
                    SubPath = $"Bulk_{groupName}",
                    ExportType = ExportTypeFlag.Full
                };
                viewer.ExportControl.RoutedFunction(exportInfo);
                MainWindow.Progress.CompleteStage();
            }
        });
        viewer.StaticControl.Visibility = bStaticShowing ? Visibility.Visible : viewer.StaticControl.Visibility;
        viewer.EntityControl.Visibility = bEntityShowing ? Visibility.Visible : viewer.EntityControl.Visibility;
    }

    private void SetExportFunction(Action<ExportInfo> function, int exportTypeFlags, bool disableLoadingBar = false, bool hideBulkExport = false)
    {
        TagView viewer = GetViewer();
        viewer.ExportControl.SetExportFunction(function, exportTypeFlags, disableLoadingBar);
        if (!hideBulkExport)
            ShowBulkExportButton();
        else
            BulkExportButton.Visibility = Visibility.Hidden;
    }

    #region Destination Global Tag Bag

    /// <summary>
    /// Type 0x8080471D and only in sr_destination_metadata_010a?
    /// </summary>
    private async Task LoadDestinationGlobalTagBagList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        if (Strategy.IsPreBL())
        {
            ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S30898080>();
            Parallel.ForEach(vals, val =>
            {
                Tag<S30898080> bag = FileResourcer.Get().GetSchemaTag<S30898080>(val);
                if (bag.TagData.Entries.Count == 0)
                    return;

                _allTagItems.Add(new TagItem
                {
                    Hash = bag.Hash,
                    Name = bag.Hash,
                    Subname = $"",
                    TagType = ETagListType.DestinationGlobalTagBag
                });

            });
        }
        else
        {
            ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S1D478080>();
            Parallel.ForEach(vals, val =>
            {
                Tag<S1D478080> dgtbParent = FileResourcer.Get().GetSchemaTag<S1D478080>(val);
                if (dgtbParent.TagData.DestinationGlobalTagBags.Count == 0)
                    return;
                foreach (SD3598080 destinationGlobalTagBag in dgtbParent.TagData.DestinationGlobalTagBags)
                {
                    if (!destinationGlobalTagBag.DestinationGlobalTagBag.IsValid())
                        continue;

                    string name = destinationGlobalTagBag.DestinationGlobalTagBagName;
                    _allTagItems.Add(new TagItem
                    {
                        Hash = destinationGlobalTagBag.DestinationGlobalTagBag,
                        Name = name,
                        Subname = $"{Helpers.GetReadableSize(destinationGlobalTagBag.DestinationGlobalTagBag.GetFileMetadata().Size)}",
                        TagType = ETagListType.DestinationGlobalTagBag
                    });
                }
            });
        }

    }

    private void LoadDestinationGlobalTagBag(FileHash hash)
    {
        Tag<S30898080> destinationGlobalTagBag = FileResourcer.Get().GetSchemaTag<S30898080>(hash);

        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(destinationGlobalTagBag.TagData.Entries, val =>
        {
            if (val.Tag == null)
                return;
            FileHash reference = val.Tag.Hash.GetReferenceHash();
            ETagListType tagType;
            string overrideType = String.Empty;

            switch (reference.Hash32)
            {
                case 0x808099D1 when Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                case 0x8080987E when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                    tagType = ETagListType.BudgetSet;
                    break;

                case 0x80809C0F when Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                case 0x80809AD8 when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                    tagType = ETagListType.Entity;
                    break;

                default:
                    if (val.Tag.Hash.GetFileMetadata().Type == 32)
                    {
                        tagType = ETagListType.Texture;
                        break;
                    }
                    tagType = ETagListType.None;
                    overrideType = reference;
                    break;
            }

            string name = val.TagPath ?? "";
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = name,
                Subname = "",
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion

    #region Budget Set

    private void LoadBudgetSet(FileHash hash)
    {
        Tag<S7E988080> budgetSetHeader = FileResourcer.Get().GetSchemaTag<S7E988080>(hash);
        Tag<SED9E8080> budgetSet = FileResourcer.Get().GetSchemaTag<SED9E8080>(budgetSetHeader.TagData.Bag.Hash);
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(budgetSet.TagData.Unk28, val =>
        {
            if (val.Tag is null || !val.Tag.Hash.IsValid())
            {
                Log.Error($"BudgetSet {budgetSetHeader.TagData.Bag.Hash} has an invalid tag hash.");
                return;
            }
            ETagListType tagType = ETagListType.None;
            FileHash reference = val.Tag.Hash.GetReferenceHash();
            string overrideType = String.Empty;
            switch (reference.Hash32)
            {
                case 0x80809C0F when Strategy.CurrentStrategy <= TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                case 0x80809AD8 when Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                    tagType = ETagListType.Entity;
                    break;

                default:
                    if (val.Tag.Hash.GetFileMetadata().Type == 32)
                    {
                        tagType = ETagListType.Texture;
                        break;
                    }
                    tagType = ETagListType.None;
                    overrideType = reference;
                    break;
            }
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath ?? "",
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion

    // TODO Entity Viewer 2.0
    #region Entity
    private void LoadEntity(FileHash fileHash)
    {
        TagView viewer = GetViewer();
        SetViewer(TagView.EViewerType.Entity);
        bool bLoadedSuccessfully = viewer.EntityControl.LoadEntity(fileHash);
        if (!bLoadedSuccessfully)
        {
            Log.Error($"UI failed to load entity for hash {fileHash}. You can still try to export the full model instead.");
            _mainWindow.SetLoggerSelected();
        }
        SetExportFunction(ExportEntity, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
        viewer.ExportControl.ExportChildrenBox.Visibility = Visibility.Visible;
        viewer.ExportControl.SetExportInfo(fileHash);
        viewer.EntityControl.ModelView.SetModelFunction(() => viewer.EntityControl.LoadEntity(fileHash));
    }

    private void ExportEntity(ExportInfo info)
    {
        TagView viewer = GetViewer();
        Entity entity = FileResourcer.Get().GetFile<Entity>(info.Hash);
        List<Entity> entities = new() { entity };
        Dispatcher.Invoke(() =>
        {
            if (viewer.ExportControl.ExportChildrenBox.Visibility == Visibility.Visible && viewer.ExportControl.ExportChildrenBox.IsChecked.Value == true)
                entities.AddRange(entity.GetEntityChildren());
            viewer.EntityControl.ModelView.Visibility = Visibility.Hidden;
        });
        EntityView.Export(entities, info.Name, exportType: info.ExportType);

        Dispatcher.Invoke(() =>
        {
            NotificationBanner notify = new()
            {
                Icon = "☑️",
                Title = "Export Complete",
                Description = $"Exported Entity {info.Name} to \"{ConfigSubsystem.Get().GetExportSavePath()}\\{info.Name}\\\"",
                Style = NotificationBanner.PopupStyle.Information
            };
            notify.OnProgressComplete += () => Dispatcher.Invoke(() => viewer.EntityControl.ModelView.Visibility = Visibility.Visible);
            notify.Show();
        });
    }

    #endregion

    // TODO Activity Viewer 2.0?
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
                Dictionary<FileHash, TagClassHash> activities = PackageResourcer.Get().GetD1Activities();
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
                ConcurrentHashSet<FileHash> valsChild = await PackageResourcer.Get().GetAllHashesAsync<SUnkActivity_SK>();
                Parallel.ForEach(valsChild, val =>
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    nameHashes.TryAdd(tag.TagData.ActivityDevName.Value, tag.TagData.DestinationName);
                    GlobalStrings.Get().AddStrings(tag.TagData.LocalizedStrings);
                });
                break;

            default:
                valsChild = await PackageResourcer.Get().GetAllHashesAsync<S8B8E8080>();
                Parallel.ForEach(valsChild, val =>
                {
                    Tag<S8B8E8080> tag = FileResourcer.Get().GetSchemaTag<S8B8E8080>(val);
                    nameHashes.TryAdd(tag.TagData.DestinationName, tag.TagData.LocationName);
                    GlobalStrings.Get().AddStrings(tag.TagData.StringContainer);
                });
                break;
        }

        foreach (KeyValuePair<string, StringHash> keyValuePair in nameHashes)
        {
            names[keyValuePair.Key] = GlobalStrings.Get().GetString(keyValuePair.Value);
        }

        if (Strategy.IsD1())
        {
            Dictionary<FileHash, TagClassHash> activities = PackageResourcer.Get().GetD1Activities();

            Parallel.ForEach(activities, val =>
            {
                if (val.Value == "2E058080")
                {
                    string activityName = PackageResourcer.Get().GetActivityName(val.Key);
                    string first = activityName.Split(":")[1];
                    _allTagItems.Add(new TagItem
                    {
                        Hash = val.Key,
                        Name = first,
                        Subname = names.TryGetValue(first, out string name) ? name : "",
                        TagType = ETagListType.Activity
                    });
                }
            });
        }
        else
        {
            ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<IActivity>();
            Parallel.ForEach(vals, val =>
            {
                string activityName = PackageResourcer.Get().GetActivityName(val);
                string first = Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402
                ? activityName.Split(".").First() : activityName.Split(":")[1];

                // These are silly
                if (activityName.EndsWith("_ls") || activityName.Contains("_ls_"))
                    activityName = $" {activityName}"; // Lost sector icon
                if (activityName.Contains("exotic"))
                    activityName = $" {activityName}"; // Quest crown icon
                if (activityName.Contains("dungeon") || activityName.Contains("raid") || activityName.Contains("kingsfall"))
                    activityName = $" {activityName}"; // Revive token icon (could do 💀 if people dont like it)

                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = activityName,
                    Subname = names.TryGetValue(first, out string name) ? name : "",
                    TagType = ETagListType.Activity
                });
            });
        }
    }

    private void LoadActivity(FileHash fileHash)
    {
        ActivityView activityView = new();
        _mainWindow.MakeNewTab(PackageResourcer.Get().GetActivityName(fileHash), activityView);
        activityView.LoadActivity(fileHash);
        _mainWindow.SetNewestTabSelected();
    }

    #region Activity Music

    /// <summary>
    /// We assume all music tables come from activities.
    /// </summary>
    private void LoadMusicList(FileHash fileHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        ConcurrentBag<FileHash> musics = new();

        if (Strategy.IsPreBL())
        {
            ActivitySK activitySK = FileResourcer.Get().GetFile<ActivitySK>(fileHash);
            ConcurrentHashSet<FileHash> valsSK = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
            foreach (FileHash val in valsSK)
            {
                Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                string activityName = PackageResourcer.Get().GetActivityName(activitySK.FileHash).Split(':')[1];

                if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                {
                    foreach (FileHash? tableHash in activitySK.GetActivityMusicList(val).Distinct())
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = tableHash,
                            Name = $"{PackageResourcer.Get().GetActivityName(val).Split(":").First()}",
                            TagType = ETagListType.Music
                        });
                    }
                }
            }
        }
        else if (Strategy.IsBL())
        {
            ActivityWQ activity = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
            // TODO: check if wq way of music is also in beyond light
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S19978080 res)
            {
                if (res.Music != null)
                    musics.Add(res.Music.Hash);
            }
        }
        else if (Strategy.IsPostBL())
        {
            ActivityWQ activity = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
            Parallel.ForEach(activity.TagData.Unk50, val =>
            {
                foreach (S48898080 d2Class48898080 in val.Unk18)
                {
                    dynamic? resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                    if (resource is SD5908080 res)
                    {
                        if (res.Music != null)
                        {
                            musics.Add(res.Music.Hash);
                        }
                    }
                    else if (resource is S18978080 res2)
                    {
                        if (res2.Unk1C != null)
                        {
                            musics.Add(res2.Unk1C.Hash);
                        }
                    }
                }
            });
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S6A988080 res)
            {
                if (res.Music != null)
                    musics.Add(res.Music.Hash);

                if (res.Music2 is not null)
                {
                    _allTagItems.Add(new TagItem
                    {
                        Hash = res.Music2.Hash,
                        Name = res.Music2.Hash,
                        TagType = ETagListType.Music,
                        Extra = res.Music2
                    });
                }

                if (res.DescentMusic is not null)
                {
                    _allTagItems.Add(new TagItem
                    {
                        Hash = res.DescentMusic.Hash,
                        Name = res.DescentMusicPath.Value,
                        TagType = ETagListType.Music,
                        Extra = res.DescentMusic
                    });
                }
            }
            if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S20978080 res2)
            {
                if (res2.Music != null)
                    musics.Add(res2.Music.Hash);
            }
        }

        Parallel.ForEach(musics.Distinct(), hash =>
        {
            _allTagItems.Add(new TagItem
            {
                Hash = hash,
                Name = hash,
                TagType = ETagListType.Music
            });
        });
    }

    private void LoadMusic(FileHash fileHash, TagItem extra = null)
    {
        TagView viewer = GetViewer();
        SetViewer(TagView.EViewerType.Music);
        if (extra is not null)
            viewer.MusicControl.Load(fileHash, extra.Extra);
        else
            viewer.MusicControl.Load(fileHash);

        SetExportFunction(viewer.MusicControl.Export, (int)ExportTypeFlag.Full, true);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    #endregion

    #region Activity Dialogue

    /// <summary>
    /// We assume all dialogue tables come from activities.
    /// </summary>
    private void LoadDialogueList(FileHash fileHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        // Dialogue tables can be in the 0x80808948 entries
        ConcurrentDictionary<string, FileHash> dialogueTables = new();
        switch (Strategy.CurrentStrategy)
        {
            case >= TigerStrategy.DESTINY2_WITCHQUEEN_6307:
                ActivityWQ activity = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activity.TagData.Unk18.GetValue(activity.GetReader()) is S6A988080 entry)
                {
                    foreach (SB7978080 dirtable in entry.DialogueTables)
                    {
                        if (dirtable.DialogueTable != null)
                            dialogueTables.TryAdd(dirtable.DialogueTable.Hash, dirtable.DialogueTable.Hash);
                    }
                }
                Parallel.ForEach(activity.TagData.Unk50, val =>
                {
                    foreach (S48898080 d2Class48898080 in val.Unk18)
                    {
                        dynamic? resource = d2Class48898080.UnkEntityReference.TagData.Unk10.GetValue(d2Class48898080.UnkEntityReference.GetReader());
                        if (resource is SD5908080 or S44938080 or S45938080 or
                            S18978080 or S19978080)
                        {
                            if (resource.DialogueTable != null)
                                dialogueTables.TryAdd(resource.DialogueTable.Hash, resource.DialogueTable.Hash);
                        }
                    }
                });
                break;

            case TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                ActivityWQ activityBL = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                dynamic? resource = activityBL.TagData.Unk18.GetValue(activityBL.GetReader());
                //if (resource is SD5908080 || resource is S44938080 || resource is S45938080 ||
                //    resource is S18978080 || resource is S19978080)
                if (resource is S19978080)
                {
                    if (resource.DialogueTableBL != null)
                        dialogueTables.TryAdd(resource.DialogueTableBL.Hash, resource.DialogueTableBL.Hash);
                }
                break;

            case TigerStrategy.DESTINY2_SHADOWKEEP_2601:
            case TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                ActivitySK activitySK = FileResourcer.Get().GetFile<ActivitySK>(fileHash);
                ConcurrentHashSet<FileHash> valsSK = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
                foreach (FileHash val in valsSK)
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    string activityName = PackageResourcer.Get().GetActivityName(activitySK.FileHash).Split(':')[1];

                    if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                    {
                        foreach (FileHash tableHash in activitySK.GetActivityDialogueTables(val))
                        {
                            dialogueTables.TryAdd($"{PackageResourcer.Get().GetActivityName(val).Split(":").First()}", tableHash);
                        }
                    }
                }
                break;

            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                ActivityROI activityROI = FileResourcer.Get().GetFile<ActivityROI>(fileHash);
                Dictionary<FileHash, TagClassHash> valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (KeyValuePair<FileHash, TagClassHash> val in valsROI)
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
        TagView viewer = GetViewer();
        SetViewer(TagView.EViewerType.Dialogue);
        viewer.DialogueControl.Load(fileHash, viewer);
    }

    #endregion

    #region Activity Directives

    private void LoadDirectiveList(FileHash fileHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();

        ConcurrentDictionary<string, FileHash> directiveItems = new();

        switch (Strategy.CurrentStrategy)
        {
            case >= TigerStrategy.DESTINY2_WITCHQUEEN_6307:
                ActivityWQ activityWQ = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is S6A988080 a988080)
                {
                    IEnumerable<FileHash> directiveTables = a988080.DirectiveTables.Select(x => x.DirectiveTable.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        directiveItems.TryAdd(hash, hash);
                    });
                }
                else if (activityWQ.TagData.Unk18.GetValue(activityWQ.GetReader()) is S20978080 class20978080)
                {
                    IEnumerable<FileHash> directiveTables = class20978080.DirectiveTables.Select(x => x.DirectiveTable.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        directiveItems.TryAdd(hash, hash);
                    });
                }
                break;

            case TigerStrategy.DESTINY2_BEYONDLIGHT_3402:
                ActivityWQ activityBL = FileResourcer.Get().GetFile<ActivityWQ>(fileHash);
                if (activityBL.TagData.Unk18.GetValue(activityBL.GetReader()) is S19978080 resource)
                {
                    IEnumerable<FileHash?> directiveTables = resource.DirectiveTables.Where(x => x.DirectiveTable is not null).Select(x => x.DirectiveTable?.Hash);

                    Parallel.ForEach(directiveTables, hash =>
                    {
                        directiveItems.TryAdd(hash ?? "", hash);
                    });
                }
                break;

            case TigerStrategy.DESTINY2_SHADOWKEEP_2601:
            case TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                ActivitySK activitySK = FileResourcer.Get().GetFile<ActivitySK>(fileHash);
                ConcurrentHashSet<FileHash> valsSK = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
                foreach (FileHash val in valsSK)
                {
                    Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    string activityName = PackageResourcer.Get().GetActivityName(activitySK.FileHash).Split(':')[1];

                    if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                    {
                        foreach (FileHash tableHash in activitySK.GetActivityDirectiveTables(val))
                        {
                            directiveItems.TryAdd($"{PackageResourcer.Get().GetActivityName(val).Split(":").First()}", tableHash);
                        }
                    }
                }
                break;


            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                ActivityROI activityROI = FileResourcer.Get().GetFile<ActivityROI>(fileHash);
                Dictionary<FileHash, TagClassHash> valsROI = PackageResourcer.Get().GetD1Activities();
                foreach (KeyValuePair<FileHash, TagClassHash> val in valsROI)
                {
                    if (val.Value == "16068080")
                    {
                        Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(val.Key);
                        string activityName = PackageResourcer.Get().GetActivityName(activityROI.FileHash).Split(':')[1];
                        if (tag.TagData.ActivityDevName.Value.Contains(activityName))
                        {
                            directiveItems.TryAdd(PackageResourcer.Get().GetActivityName(val.Key).Split(":").First(), val.Key);
                        }
                    }
                }
                break;
        }

        Parallel.ForEach(directiveItems, entry =>
        {
            _allTagItems.Add(new TagItem
            {
                Name = entry.Key,
                Hash = entry.Value,
                TagType = ETagListType.Directive
            });
        });
    }

    // TODO replace with taglist control
    private void LoadDirective(FileHash fileHash)
    {
        SetViewer(TagView.EViewerType.Directive);
        TagView viewer = GetViewer();
        viewer.DirectiveControl.Load(fileHash);
    }

    #endregion

    #endregion

    #region Texture

    private void LoadTexture(FileHash fileHash)
    {
        TagView viewer = GetViewer();
        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(fileHash);
        if (textureHeader.IsCubemap())
        {
            SetViewer(TagView.EViewerType.TextureCube);
            viewer.CubemapControl.LoadCubemap(textureHeader);
        }
        else
        {
            SetViewer(TagView.EViewerType.Texture2D);
            viewer.TextureControl.LoadTexture(textureHeader);
        }
        SetExportFunction(ExportTexture, (int)ExportTypeFlag.Full);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    private void ExportTexture(ExportInfo info)
    {
        TextureExporter.ExportTexture(info.Hash as FileHash);
    }

    #endregion

    #region Sound
    private void LoadSound(FileHash fileHash)
    {
        TagView viewer = GetViewer();
        if (viewer.MusicPlayer.SetWem(FileResourcer.Get().GetFile<Wem>(fileHash)))
        {
            viewer.MusicPlayer.Play();
            SetExportFunction(ExportWav, (int)ExportTypeFlag.Full, hideBulkExport: true); // TODO make bulk sound exporting work
            viewer.ExportControl.SetExportInfo(fileHash);
        }
    }

    private void ExportWEM(ExportInfo info)
    {
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();

        WwiseSound sound = FileResourcer.Get().GetFile<WwiseSound>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{(_weaponItemName == null ? "" : $"{_weaponItemName}/")}{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        sound.ExportSound(saveDirectory);
    }

    private void ExportWav(ExportInfo info)
    {
        // exporting while playing the audio causes a hang
        TagView viewer = GetViewer();
        Dispatcher.Invoke(() =>
        {
            if (viewer.MusicPlayer.IsPlaying())
                viewer.MusicPlayer.Pause();
        });

        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash);
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wav");
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
            "Caching Materials",
            "Adding Materials to UI",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();

            ConcurrentHashSet<FileHash> mats = PackageResourcer.Get().GetAllHashes<Material>();
            MainWindow.Progress.CompleteStage();

            // named render global materials
            ConcurrentDictionary<string, FileHash> _added = new();
            var globals = Globals.Get().RenderGlobals;
            Parallel.ForEach(globals.TagData.Pipelines.Enumerate(globals.GetReader()), pipeline =>
            {
                if (pipeline.Technique.IsInvalid())
                    return;

                if (!_added.TryAdd(pipeline.Name, pipeline.Technique))
                    return;

                FileMetadata metadata = pipeline.Technique.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = pipeline.Technique,
                    Name = $"Pipeline: {pipeline.Name.Value}",
                    Subname = Helpers.GetReadableSize(metadata.Size),
                    TagType = ETagListType.Material
                });
            });

            HashSet<FileHash> remainingVals = new HashSet<FileHash>(mats);
            remainingVals.ExceptWith(_added.Values);

            Parallel.ForEach(remainingVals, val =>
            {
                FileMetadata metadata = val.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Material {metadata.FileIndex}",
                    Subname = $"{Helpers.GetReadableSize(metadata.Size)}",
                    TagType = ETagListType.Material
                });

                //Material mat = FileResourcer.Get().GetFile<Material>(val, shouldCache: false);
                //var matOps = mat.Pixel.GetBytecode();
                //if (matOps.Opcodes.Any(x => x.op == TfxBytecode.Clamp))
                //    Console.WriteLine($"{mat.Hash}");
            });

            MainWindow.Progress.CompleteStage();

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

    private void LoadMaterial(FileHash fileHash)
    {
        var materialView = new MaterialView2();
        materialView.Load(fileHash);
        _mainWindow.MakeNewTab(fileHash, materialView);
        _mainWindow.SetNewestTabSelected();
    }
    #endregion

    #region String

    private async Task LoadStringContainersList()
    {
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Caching string tags",
            "Loading string list",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            var vals = PackageResourcer.Get().GetAllHashes<LocalizedStrings>();
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

        localizedStrings.GetAllStringViews().OrderBy(x => x.RawString).ToList().ForEach(view =>
        {
            text.Append($"{view.StringHash} : {view.RawString} \n");
        });

        ConfigSubsystem config = ConfigSubsystem.Get();
        string saveDirectory = config.GetExportSavePath() + $"/Strings/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);

        File.WriteAllText(saveDirectory + "strings.txt", text.ToString());

    }

    #endregion

    private async void TagImage_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.DataContext is TagItem tag)
        {
            //Console.WriteLine($"Loaded {tag.Hash}");
            img.Tag = tag;
            await tag.LoadTagImageAsync();
        }
    }

    private void TagImage_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is Image img && img.Tag is TagItem tag)
        {
            tag.ClearImageSource();
            img.Source = null;
            img.Tag = null;
        }
    }
}

public class TagItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


    private string _name = String.Empty;
    public string Name
    {
        get => _name; set => _name = value;
    }

    private string _subname = String.Empty;
    public string Subname
    {
        get => _subname;
        set
        {
            _subname = value;
            OnPropertyChanged(nameof(Subname));
        }
    }

    public TigerHash Hash { get; set; }

    public string HashString
    {
        get
        {
            if (Name == "BACK")
                return "";
            if (TagType == ETagListType.Package)
                return $"[{(Hash as FileHash).PackageId:X4}]";
            return $"[{Hash:X8}]";
        }
    }

    public int FontSize { get; set; } = 16;

    private string _type = String.Empty;
    public string Type
    {
        get
        {
            if (_type == String.Empty)
            {
                string t = EnumExtensions.GetEnumDescription(TagType);
                if (t.Contains("[Final]"))
                    return t.Split("[Final]")[0].Trim();
                return t;
            }
            return _type;
        }
        set => _type = value;
    }

    public ETagListType TagType { get; set; }

    public dynamic? Extra { get; set; } // This is dumb and should only be used sparingly

    private ImageSource _tagImageSource;
    public ImageSource TagImageSource
    {
        get => _tagImageSource;
        private set
        {
            _tagImageSource = value;
            OnPropertyChanged(nameof(TagImageSource));
        }
    }

    public async Task LoadTagImageAsync()
    {
        if (TagType != ETagListType.Texture || Hash == null || TagImageSource != null)
            return;

        Texture texture = await Task.Run(() => FileResourcer.Get().GetFileAsync<Texture>(Hash, shouldCache: false));
        if (texture == null)
            return;

        ImageSource image = await Task.Run(() => TextureLoader.LoadTexture(texture, 96, 96));

        if (image != null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TagImageSource = image;
                // Sets the Subname to add the Textures dimensions, this gets set after the tag is
                // added to _allTagItems so you can't search by its pixel dimensions, which is why
                // GetTextureDimensionsRaw is used in SortItemListByString()
                Subname = $"{texture.GetDimension().GetEnumDescription()} Texture : {texture.Width}x{texture.Height}";
            });
        }
    }

    public void ClearImageSource()
    {
        TagImageSource = null;
    }
}
