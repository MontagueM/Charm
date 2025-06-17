using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
using ActivitySK = Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601.Activity;
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
                case ETagListType.Sound:
                    LoadSound(contentValue as FileHash);
                    break;
                case ETagListType.MusicList:
                    LoadMusicList(contentValue as FileHash);
                    break;
                case ETagListType.Music:
                    LoadMusic(contentValue as FileHash, fullTag);
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
                if (pkg is not null && pkg.GetPackageMetadata().Name.Contains("redacted"))
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
        if (pkg is not null && pkg.GetPackageMetadata().Name.Contains("redacted"))
        {
            if (!PackageResourcer.Get().Keys.ContainsKey(pkg.GetPackageMetadata().PackageGroup))
            {
                //MessageBox.Show($"No decryption key found, can not display content.", $"This item belongs to a redacted package.", MessageBoxButton.OK);

                // This could be a lot better probably but oh well
                PopupBanner warn = new()
                {
                    Icon = "🔐",
                    Title = "ERROR",
                    Subtitle = "No decryption key found, can not display content.",
                    Description = "This item belongs to a redacted package, which means its content can not be shown.",
                    Style = PopupBanner.PopupStyle.Warning
                };
                warn.Show();

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

                    _allTagItems.Add(new TagItem
                    {
                        Hash = destinationGlobalTagBag.DestinationGlobalTagBag,
                        Name = destinationGlobalTagBag.DestinationGlobalTagBagName,
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
            _allTagItems.Add(new TagItem
            {
                Hash = val.Tag.Hash,
                Name = val.TagPath ?? "",
                Subname = val.TagNote ?? "",
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
    private async Task LoadEntityList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        Stopwatch stopwatch = new();
        stopwatch.Start();

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Caching Entity Names, may take some time",
            "Loading Entities, may take some time"
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            ConcurrentDictionary<string, List<string>> NamedEntities = TryGetEntityNames().Result;
            MainWindow.Progress.CompleteStage();

            var eVals = PackageResourcer.Get().GetAllHashes<Entity>();
            ConcurrentHashSet<uint> existingEntities = new();
            Parallel.ForEach(eVals, hash =>
            {
                var entity = FileResourcer.Get().GetFile<Entity>(hash);
                if (entity.HasGeometry())
                {
                    string entityName = entity.EntityName != null ? entity.EntityName : entity.Hash;
                    string subname = $"{entity.TagData.EntityResources.Count} Resources";

                    // Most of the time the most specific entity name comes from a map resource (bosses usually)
                    if (NamedEntities.ContainsKey(entity.Hash))
                    {
                        if (!NamedEntities[entity.Hash].Contains(entityName) && entityName != entity.Hash)
                            NamedEntities[entity.Hash].Add(entityName);

                        foreach (string entry in NamedEntities[entity.Hash])
                        {
                            _allTagItems.Add(new TagItem
                            {
                                Hash = entity.Hash,
                                Name = entry,
                                Subname = subname,
                                TagType = ETagListType.Entity
                            });
                        }
                    }
                    else
                    {
                        _allTagItems.Add(new TagItem
                        {
                            Hash = entity.Hash,
                            Name = entityName,
                            Subname = subname,
                            TagType = ETagListType.Entity
                        });
                    }

                }
            });
            MainWindow.Progress.CompleteStage();
            stopwatch.Stop();
            Log.Info($"Loaded {_allTagItems.Count} Entities in {stopwatch.Elapsed.TotalSeconds} seconds");

            MakePackageTagItems();
        });

        RefreshItemList();  // bc of async stuff
    }

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

    private async Task<ConcurrentDictionary<string, List<String>>> TryGetEntityNames()
    {
        NamedEntities Ents = new()
        {
            EntityNames = new()
        };

        if (!File.Exists($"./EntityNames.json"))
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));

        try
        {
            Ents = JsonConvert.DeserializeObject<NamedEntities>(File.ReadAllText($"./EntityNames.json"));
        }
        catch (JsonSerializationException) // Likely old version of the json
        {
            File.Delete($"./EntityNames.json");
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
        }

        if (Ents.EntityNames.TryGetValue(Strategy.CurrentStrategy, out ConcurrentDictionary<string, List<string>>? names) && !Ents.EntityNames[Strategy.CurrentStrategy].IsEmpty)
        {
            return names;
        }
        else
        {
            Ents.EntityNames[Strategy.CurrentStrategy] = new();
            if (Strategy.IsD1())
            {
                // Name and entity is in a map data table
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<SD9128080>();
                Parallel.ForEach(vals, val =>
                {
                    Tag<SD9128080> entry = FileResourcer.Get().GetSchemaTag<SD9128080>(val);
                    foreach (SD6148080 a in entry.TagData.Unk20)
                    {
                        foreach (S48138080 b in a.Unk08)
                        {
                            if (b.Pointer.GetValue(entry.GetReader()) is SMapDataEntry datatable)
                            {
                                if (datatable.DataResource.GetValue(entry.GetReader()) is S33138080 name)
                                {
                                    if (name.EntityName.IsValid())
                                    {
                                        FileHash entityHash = datatable.Entity.Hash;
                                        string entityName = GlobalStrings.Get().GetString(name.EntityName);

                                        Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                    }
                                }
                            }
                        }
                    }
                });

                // Name is in an EntityResource, with the entity in a map data table in that EntityResource
                ConcurrentHashSet<FileHash> vals2 = await PackageResourcer.Get().GetAllHashesAsync<SF6038080>();
                Parallel.ForEach(vals2, val =>
                {
                    Tag<SF6038080> entry = FileResourcer.Get().GetSchemaTag<SF6038080>(val);
                    if (entry.TagData.EntityResource is not null)
                    {
                        if (entry.TagData.EntityResource.TagData.Unk10.GetValue(entry.TagData.EntityResource.GetReader()) is S2E098080)
                        {
                            var resource = (SDD078080)entry.TagData.EntityResource.TagData.Unk18.GetValue(entry.TagData.EntityResource.GetReader());
                            foreach (SMapDataEntry dataentry in resource.DataEntries)
                            {
                                if (dataentry.Entity.Hash.IsValid())
                                {
                                    FileHash entityHash = dataentry.Entity.Hash;
                                    string entityName = resource.DevName.Value ?? entityHash.ToString();

                                    Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                }
                            }
                        }
                    }
                });
            }
            else if (Strategy.IsPreBL()) // SK
            {
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<S149B8080>();
                Parallel.ForEach(vals, val =>
                {
                    //Console.WriteLine($"Resource {val}");
                    Tag<S149B8080> entry = FileResourcer.Get().GetSchemaTag<S149B8080>(val);
                    if (entry.TagData.EntityResource is not null)
                    {
                        EntityResource resource = entry.TagData.EntityResource;
                        if (resource.TagData.Unk10.GetValue(resource.GetReader()) is S3B9A8080)
                        {
                            var D2Class8F948080 = (S8F948080)resource.TagData.Unk18.GetValue(resource.GetReader());
                            foreach (S56838080 entry2 in D2Class8F948080.UnkA8)
                            {
                                List<DynamicArray<S58838080>> tables = new() { entry2.Table1, entry2.Table2, entry2.Table3, entry2.Table4, entry2.Table5, entry2.Table6 };

                                foreach (DynamicArray<S58838080> datatable in tables)
                                {
                                    foreach (S58838080 dataEntry in datatable)
                                    {
                                        SMapDataEntry? value = dataEntry.Datatable.Value;
                                        if (value is null)
                                            continue;

                                        if (value.Value.DataResource.GetValue(resource.GetReader()) is SB67E8080 name)
                                        {
                                            if (name.EntityName.IsValid())
                                            {
                                                FileHash entityHash = value.Value.Entity.Hash;
                                                string entityName = GlobalStrings.Get().GetString(name.EntityName);

                                                Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            else if (Strategy.IsPostBL()) // WQ+
            {
                // Name and entity is in a map data table
                ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<SMapDataTable>();
                Parallel.ForEach(vals, val =>
                {
                    if (!val.ContainsHash(0x80808019))
                        return;

                    Tag<SMapDataTable> entry = FileResourcer.Get().GetSchemaTag<SMapDataTable>(val);
                    foreach (SMapDataEntry dataEntry in entry.TagData.DataEntries)
                    {
                        if (dataEntry.DataResource.GetValue(entry.GetReader()) is S19808080 name)
                        {
                            if (name.EntityName.IsValid())
                            {
                                FileHash entityHash = dataEntry.Entity.Hash;
                                string entityName = GlobalStrings.Get().GetString(name.EntityName);

                                Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                            }
                        }
                    }
                });


                // Name is in an EntityResource, with the entity in a map data table in that EntityResource
                ConcurrentHashSet<FileHash> resources = await PackageResourcer.Get().GetAllHashesAsync<EntityResource>();
                Parallel.ForEach(resources, val =>
                {
                    // don't want to load the resource but need to check it first
                    //var data = PackageResourcer.Get().GetFileData(val);

                    //using TigerReader reader = new(data);
                    //reader.Seek(0x10, SeekOrigin.Begin);
                    //var offset = reader.ReadInt32() - 0x8;

                    //reader.Seek(offset, SeekOrigin.Current);
                    //var reference = reader.ReadUInt32();

                    if (val.ContainsHash(0x8080470E))//if (reference == 0x8080470E)
                    {
                        EntityResource resource = FileResourcer.Get().GetFile<EntityResource>(val);
                        foreach (S96468080 entry in ((SB5468080)resource.TagData.Unk18.GetValue(resource.GetReader())).Unk80)
                        {
                            if (entry.DataTable is null)
                                continue;

                            foreach (SMapDataEntry dataEntry in entry.DataTable.TagData.DataEntries)
                            {
                                if (dataEntry.DataResource.GetValue(entry.DataTable.GetReader()) is S19808080 name)
                                {
                                    if (entry.Name.IsValid())
                                    {
                                        FileHash entityHash = dataEntry.Entity.Hash;
                                        string entityName = GlobalStrings.Get().GetString(entry.Name);

                                        Ents.AddEntityName(Strategy.CurrentStrategy, entityHash, entityName);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            File.WriteAllText($"./EntityNames.json", JsonConvert.SerializeObject(Ents, Formatting.Indented));
        }

        return Ents.EntityNames[Strategy.CurrentStrategy];
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
                    IEnumerable<FileHash> directiveTables = class20978080.PEDirectiveTables.Select(x => x.DirectiveTable.Hash);

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

    #region Static

    private async Task LoadStaticList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        if (_allTagItems != null)
            return;

        MainWindow.Progress.SetProgressStages(new List<string>
        {
            $"Loading Statics List",
        });

        await Task.Run(async () =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            ConcurrentHashSet<FileHash> eVals = await PackageResourcer.Get().GetAllHashesAsync<SStaticMesh>();
            Parallel.ForEach(eVals, val =>
            {
                FileMetadata metadata = val.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Static {metadata.FileIndex}",
                    Subname = $"{Helpers.GetReadableSize(metadata.Size)}",
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
        TagView viewer = GetViewer();
        SetViewer(TagView.EViewerType.Static);
        viewer.StaticControl.LoadStatic(fileHash, ExportDetailLevel.MostDetailed);
        SetExportFunction(ExportStatic, (int)ExportTypeFlag.Full | (int)ExportTypeFlag.Minimal);
        viewer.ExportControl.SetExportInfo(fileHash);
    }

    private void ExportStatic(ExportInfo info)
    {
        TagView viewer = GetViewer();
        StaticView.ExportStatic(info.Hash as FileHash, info.Name, info.ExportType, info.SubPath);
    }

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
        TextureExtractor.ExportTexture(info.Hash as FileHash);
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
            "Caching String Tags",
            "Loading String List",
        });

        await Task.Run(async () =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();
            ConcurrentHashSet<FileHash> vals = await PackageResourcer.Get().GetAllHashesAsync<LocalizedStrings>();
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(vals, val =>
            {
                FileMetadata metadata = val.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"String Container {metadata.FileIndex}",
                    Subname = $"{Helpers.GetReadableSize(metadata.Size)}",
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
        TagView viewer = GetViewer();
        viewer.TagListControl.LoadContent(ETagListType.Strings, fileHash, true);
    }

    // Would be nice to do something with colour formatting.
    private void LoadStrings(FileHash fileHash)
    {
        TagView viewer = GetViewer();
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
        SetExportFunction(ExportString, (int)ExportTypeFlag.Full, hideBulkExport: true);
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

        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string saveDirectory = config.GetExportSavePath() + $"/Strings/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);

        File.WriteAllText(saveDirectory + "strings.txt", text.ToString());

    }

    #endregion

    #region Sound

    private async Task LoadBKHDGroupList()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Loading Sound Banks",
        });

        await Task.Run(() =>
        {
            HashSet<WwiseSound> banks = PackageResourcer.Get().GetAllFiles<WwiseSound>();
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
                        Subname = $"{bank.TagData.Wems.Count} Sounds",
                        TagType = ETagListType.BKHDGroup
                    });
                }
            });
        });

        MainWindow.Progress.CompleteStage();
        RefreshItemList();
    }

    private void LoadBKHDAudioGroup(FileHash hash)
    {
        TagView viewer = GetViewer();
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

    #region Weapon Audio

    private async Task LoadWeaponAudioGroupList()
    {
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();
        _allTagItems = new ConcurrentBag<TagItem>();
        Parallel.ForEach(inventoryItems, item =>
        {
            if (item.GetWeaponPatternIndex() == -1)
                return;
            string name = item.GetItemName();
            string type = item.GetItemType();
            if (type == null)
            {
                type = "";
            }
            if (type is "Vehicle" or "Ship" or "Ship Schematics" or "Ghost Shell")
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
        TagView viewer = GetViewer();
        SetViewer(TagView.EViewerType.TagList);
        viewer.TagListControl.LoadContent(ETagListType.WeaponAudioList, apiHash, true);
        viewer.MusicPlayer.Visibility = Visibility.Visible;
    }

    // Sword audio 0x18 B6368080, E043EA80 (E143EA80 pattern ent) for testing
    private void LoadWeaponAudioList(TigerHash apiHash)
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        Entity? val = Investment.Get().GetPatternEntityFromHash(apiHash);
        if (val == null || (val.PatternAudio == null && val.PatternAudioUnnamed == null))
        {
            RefreshItemList();
            return;
        }
        _weaponItemName = Investment.Get().GetItemNameSanitized(Investment.Get().GetInventoryItem(apiHash));

        TigerReader resourceUnnamedReader = val.PatternAudioUnnamed.GetReader();
        var resourceUnnamed = (SF42C8080)val.PatternAudioUnnamed.TagData.Unk18.GetValue(resourceUnnamedReader);
        var resource = (S6E358080)val.PatternAudio.TagData.Unk18.GetValue(val.PatternAudio.GetReader());
        InventoryItem item = Investment.Get().GetInventoryItem(apiHash);
        TigerHash weaponContentGroupHash = Investment.Get().GetWeaponContentGroupHash(item);

        Log.Verbose($"Loading weapon entity audio {val.Hash}, ContentGroupHash {weaponContentGroupHash}");
        // Named
        Tag<S0D8C8080>? audioGroup = null;

        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroup1Hash == weaponContentGroupHash).Any())
        {
            Log.Verbose($"No PatterAudioGroups with matching Content Group Hash {weaponContentGroupHash}, trying fallback audio");
            if (resource.FallbackAudioGroup != null)
            {
                audioGroup = FileResourcer.Get().GetSchemaTag<S0D8C8080>(resource.FallbackAudioGroup.TagData.EntityData);
            }
        }
        else
        {
            foreach (S9B318080 entry in resource.PatternAudioGroups)
            {
                if (entry.WeaponContentGroup1Hash.Equals(weaponContentGroupHash) && entry.AudioGroup != null)
                {
                    audioGroup = FileResourcer.Get().GetSchemaTag<S0D8C8080>(entry.AudioGroup.TagData.EntityData);
                }
            }
        }

        if (audioGroup != null)
        {
            audioGroup.TagData.Audio.ForEach(audio =>
            {
                foreach (S138C8080 s in audio.Sounds)
                {
                    WwiseSound sound = FileResourcer.Get().GetFile<WwiseSound>(s.Data);
                    if (sound == null)
                        continue;

                    _allTagItems.Add(new TagItem
                    {
                        Hash = sound.Hash,
                        Name = s.WwiseEventName,
                        Subname = audio.WwiseEventHash,
                        TagType = ETagListType.WeaponAudio
                    });
                }
            });
        }


        // Unnamed
        List<WwiseSound> sounds = GetWeaponUnnamedSounds(resourceUnnamed, weaponContentGroupHash, resourceUnnamedReader);
        foreach (WwiseSound sound in sounds)
        {
            if (sound == null)
                continue;

            string name = "";
            if (Strategy.IsD1()) // && name == "")
                name = sound.TagData.SoundbankBL.GetNameFromBank();
            else if (Strategy.IsPostBL())
                name = sound.TagData.SoundbankWQ.TagData.SoundBank.GetNameFromBank();

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

    public List<WwiseSound> GetWeaponUnnamedSounds(SF42C8080 resource, TigerHash weaponContentGroupHash, TigerReader reader)
    {
        List<WwiseSound> sounds = new();
        List<Entity> entities = new();

        if (!resource.PatternAudioGroups.Where(x => x.WeaponContentGroupHash == weaponContentGroupHash).Any())
        {
            Log.Verbose($"No unnamed PatterAudioGroups with matching Content Group Hash {weaponContentGroupHash}, trying fallback audio");
            if (resource.FallbackAudio1 != null)
                entities.Add(resource.FallbackAudio1);
            if (resource.FallbackAudio2 != null)
                entities.Add(resource.FallbackAudio2);
            if (resource.FallbackAudio3 != null)
                entities.Add(resource.FallbackAudio3);
        }
        else
        {
            resource.PatternAudioGroups.ForEach(entry =>
            {
                if (!entry.WeaponContentGroupHash.Equals(weaponContentGroupHash))
                    return;

                List<TigerFile> entitiesParents = new() { entry.Unk60, entry.Unk78, entry.Unk90, entry.UnkA8, entry.UnkC0, entry.UnkD8, entry.AudioEntityParent, entry.Unk130, entry.Unk148, entry.Unk1C0, entry.Unk1D8, entry.Unk248 };

                if (entry.Unk118.GetValue(reader) is S0A2D8080 or S40238080)
                {
                    dynamic resourceUnk118 = Strategy.IsD1() ? (S40238080)entry.Unk118.GetValue(reader) : (S0A2D8080)entry.Unk118.GetValue(reader);
                    if (resourceUnk118.Unk08 != null)
                        entities.Add(resourceUnk118.Unk08);
                    if (resourceUnk118.Unk20 != null)
                        entities.Add(resourceUnk118.Unk20);
                    if (resourceUnk118.Unk38 != null)
                        entities.Add(resourceUnk118.Unk38);
                }

                foreach (TigerFile tag in entitiesParents)
                {
                    if (tag == null)
                        continue;

                    FileHash? reference = Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON ? tag.Hash.GetReferenceHash() : tag.Hash.GetReferenceFromManifest();
                    if (reference == 0x80806fa3 || reference == 0x80803463)
                    {
                        FileHash entityData = FileResourcer.Get().GetSchemaTag<SA36F8080>(tag.Hash).TagData.EntityData;
                        FileHash reference2 = entityData.GetReferenceHash();
                        if (reference2 == 0x80802d09 || reference2 == 0x80803165)
                        {
                            if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
                            {
                                Tag<S092D8080> tagInner = FileResourcer.Get().GetSchemaTag<S092D8080>(entityData);
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
                                Tag<S65318080> tagInner = FileResourcer.Get().GetSchemaTag<S65318080>(entityData);
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

        foreach (Entity entity in entities)
        {
            foreach (FileHash? resourceHash in entity.TagData.EntityResources.Select(entity.GetReader(), r => r.Resource))
            {
                if (Strategy.IsD1() && resourceHash.GetReferenceHash() != 0x80800861)
                    continue;

                EntityResource e = FileResourcer.Get().GetFile<EntityResource>(resourceHash);
                if (e.TagData.Unk18.GetValue(e.GetReader()) is S79818080 a)
                {
                    foreach (SF1918080 d2ClassF1918080 in a.Array1)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is S40668080 b)
                        {
                            sounds.Add(b.Sound);
                        }
                    }
                    foreach (SF1918080 d2ClassF1918080 in a.Array2)
                    {
                        if (d2ClassF1918080.Unk10.GetValue(e.GetReader()) is S40668080 b)
                        {
                            sounds.Add(b.Sound);
                        }
                    }
                }
            }
        }
        return sounds;
    }

    private async Task LoadWeaponAudio(FileHash fileHash)
    {
        TagView viewer = GetViewer();
        WwiseSound tag = FileResourcer.Get().GetFile<WwiseSound>(fileHash);
        if (tag.TagData.Wems.Count == 0)
            return;
        await viewer.MusicPlayer.SetSound(tag);
        SetExportFunction(ExportWEM, (int)ExportTypeFlag.Full, hideBulkExport: true); // todo bulk export just does nothing here
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
            "Caching Materials",
            "Adding Materials to UI",
        });

        await Task.Run(() =>
        {
            _allTagItems = new ConcurrentBag<TagItem>();

            ConcurrentHashSet<FileHash> mats = PackageResourcer.Get().GetAllHashes<Material>();
            MainWindow.Progress.CompleteStage();

            Parallel.ForEach(mats, val =>
            {
                FileMetadata metadata = val.GetFileMetadata();
                _allTagItems.Add(new TagItem
                {
                    Hash = val,
                    Name = $"Material {metadata.FileIndex}",
                    Subname = $"{Helpers.GetReadableSize(metadata.Size)}",
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
