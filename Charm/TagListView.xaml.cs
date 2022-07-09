using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ConcurrentCollections;
using Field;
using Field.General;
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
    [Description("Entity")]
    Entity,
    [Description("BACK")]
    Back,
    [Description("Api List")]
    ApiList,
    [Description("Api Entity")]
    ApiEntity,
    [Description("Entity List [Packages]")]
    EntityList,
    [Description("Package")]
    Package,
}

public partial class TagListView : UserControl
{
    private struct ParentInfo
    {
        public ETagListType TagListType;
        public TagHash? Hash;
        public string SearchTerm;
    }
    
    private ConcurrentBag<TagItem> _allTagItems;
    private static MainWindow _mainWindow = null;
    private ETagListType _tagListType;
    private TagHash? _currentHash = null;
    private Stack<ParentInfo> _parentStack = new Stack<ParentInfo>();
    private bool _bTrimName = true;
    private readonly ILogger _tagListLogger = Log.ForContext<TagListView>();

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }
    
    public TagListView()
    {
        InitializeComponent();
    }

    public void LoadContent(ETagListType tagListType, TagHash contentValue = null, bool bFromBack = false)
    {
        _tagListLogger.Debug($"Loading content type {tagListType} contentValue {contentValue} from back {bFromBack}");
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
                return;
            case ETagListType.ApiList:
                LoadApiList();
                break;
            case ETagListType.ApiEntity:
                LoadApiEntity(contentValue);
                return;
            case ETagListType.EntityList:
                LoadEntityList();
                break;
            case ETagListType.Package:
                LoadPackage(contentValue);
                break;
            default:
                throw new NotImplementedException();
        }
        
        if (contentValue != null && !bFromBack)
        {
            _parentStack.Push(new ParentInfo { Hash = _currentHash, TagListType = _tagListType, SearchTerm = SearchBox.Text});
        }

        _currentHash = contentValue;
        _tagListType = tagListType;
        if (!bFromBack)
        {
            SearchBox.Text = "";
        }

        RefreshItemList();
        _tagListLogger.Debug($"Loaded content type {tagListType} contentValue {contentValue} from back {bFromBack}");
    }

    /// <summary>
    /// For when we want stuff in packages, we then split up based on what the taghash value is.
    /// I kinda cheat here, I store everything in one massive _allTagItems including the packages
    /// </summary>
    /// <param name="packageId">Package ID for this package to load data for.</param>
    private void LoadPackage(TagHash pkgHash)
    {
        int pkgId = pkgHash.GetPkgId();
        _allTagItems = new ConcurrentBag<TagItem>(_allTagItems.Where(x => x.Hash.GetPkgId() == pkgId && x.TagType != ETagListType.Package));
    }

    private void SetItemListByString(string searchStr)
    {
        if (_allTagItems == null)
            return;
        if (_allTagItems.IsEmpty)
            return;

        if (_allTagItems.First().Name.Contains("\\"))
        {
            TrimCheckbox.Visibility = Visibility.Visible;
        }
        else
        {
            TrimCheckbox.Visibility = Visibility.Hidden;
        }

        var displayItems = new ConcurrentBag<TagItem>();
        // Select and sort by relevance to selected string
        Parallel.ForEach(_allTagItems, item =>
        {
            if (!TagItem.GetEnumDescription(_tagListType).Contains("List"))
            {
                if (displayItems.Count > 50) return;

            }
            else if (TagItem.GetEnumDescription(_tagListType).Contains("[Packages]"))
            {
                // Package-enabled lists have [Packages] in their enum
                if (item.TagType != ETagListType.Package)
                {
                    return;
                }
            }
            string name = _bTrimName ? TrimName(item._name) : item._name;
            if (name.ToLower().Contains(searchStr) || item.Hash.GetHashString().ToLower().Contains(searchStr) || item.Hash.Hash.ToString().Contains(searchStr))
            {
                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    Subname = item.Subname,
                    FontSize = _bTrimName ? 16 : 12,
                });
            }
        });
        
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
        btn.IsChecked = false;  // todo fix this for entity
        LoadContent(tagItem.TagType, tagHash);
    }

    /// <summary>
    /// Use the ParentInfo to go back to previous tag data.
    /// </summary>
    private void Back_Clicked()
    {
        ParentInfo parentInfo = _parentStack.Pop();
        SearchBox.Text = parentInfo.SearchTerm;
        LoadContent(parentInfo.TagListType, parentInfo.Hash, true);
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
            Tag<D2Class_75988080> dgtbParent = new Tag<D2Class_75988080>( new TagHash(val));
            if (!dgtbParent.Header.DestinationGlobalTagBag.IsValid())
                return;
            _allTagItems.Add(new TagItem 
            { 
                Hash = dgtbParent.Header.DestinationGlobalTagBag,
                Name = dgtbParent.Header.DestinationGlobalTagBagName,
                TagType = ETagListType.DestinationGlobalTagBag
            });
        });
    }
    
    private void LoadDestinationGlobalTagBag(TagHash hash)
    {
        Tag<D2Class_30898080> destinationGlobalTagBag = new Tag<D2Class_30898080>(hash);
        
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
        Tag<D2Class_7E988080> budgetSetHeader = new Tag<D2Class_7E988080>(hash);
        Tag<D2Class_ED9E8080> budgetSet = new Tag<D2Class_ED9E8080>(budgetSetHeader.Header.Unk00.Hash);
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
        EntityView.LoadEntity(tagHash);
        ExportView.SetExportInfo(tagHash);
    }
    
    /// <summary>
    /// We load all of them including no names, but add an option to only show names.
    /// Named: destination global tag bags 0x80808930, budget sets 0x80809eed
    /// All others: reference 0x80809ad8
    /// They're sorted into packages first.
    /// </summary>
    private void LoadEntityList()
    {
        _allTagItems = new ConcurrentBag<TagItem>();
        // only in 010a
        var dgtbVals = PackageHandler.GetAllEntriesOfReference(0x010a, 0x80808930);
        // only in the sr_globals, not best but it works
        var bsVals = PackageHandler.GetAllEntriesOfReference(0x010f, 0x80809eed);
        bsVals.AddRange(PackageHandler.GetAllEntriesOfReference(0x011a, 0x80809eed));
        bsVals.AddRange( PackageHandler.GetAllEntriesOfReference(0x0312, 0x80809eed));
        // everywhere
        var eVals = PackageHandler.GetAllEntities();
        ConcurrentHashSet<uint> existingEntities = new ConcurrentHashSet<uint>();
        Parallel.ForEach(dgtbVals, val =>
        {
            Tag<D2Class_30898080> dgtb = new Tag<D2Class_30898080>( new TagHash(val));
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
        Parallel.ForEach(bsVals, val =>
        {
            Tag<D2Class_ED9E8080> bs = new Tag<D2Class_ED9E8080>( new TagHash(val));
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
        Parallel.ForEach(eVals, val =>
        {
            if (existingEntities.Contains(val)) // O(1) check
                return; 
            
            _allTagItems.Add(new TagItem
            {
                Hash = val,
                TagType = ETagListType.Entity
            });
        });

        MakePackageTagItems();
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
        EntityView.LoadEntityFromApi(apiHash);
        ExportView.SetExportInfo(apiHash);
    }

    #endregion

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = true;
        RefreshItemList();
    }

    private string TrimName(string name)
    {
        return name.Split("\\").Last().Split(".")[0];
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        _bTrimName = false;
        RefreshItemList();
    }
}

public class TagItem
{
    private string _type = String.Empty;
    public string _name = String.Empty;

    public string Name
    {
        get
        {
            if (_name == "BACK")
                return _name;
            if (TagType == ETagListType.ApiEntity)
                return $"[{Hash.Hash}]  {_name}";
            if (TagType == ETagListType.Package)
                return $"[{Hash.GetPkgId():X4}]  {_name}";
            
            return $"[{Hash}]  {_name}";
        }
        set => _name = value;
    }
    
    public string Subname { get; set; }
    
    public DestinyHash Hash { get; set; }
    
    public int FontSize { get; set; }

    public string Type
    {
        get => _type == String.Empty ? GetEnumDescription(TagType) : _type;
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