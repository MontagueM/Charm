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

    public void LoadContent(ETagListType tagListType, TagHash tagHash = null, bool bFromBack = false)
    {
        _tagListLogger.Debug($"Loading content type {tagListType} tagHash {tagHash} from back {bFromBack}");
        switch (tagListType)
        {
            case ETagListType.DestinationGlobalTagBagList:
                LoadDestinationGlobalTagBagList();
                break;
            case ETagListType.Back:
                Back_Clicked();
                return;
            case ETagListType.DestinationGlobalTagBag:
                LoadDestinationGlobalTagBag(tagHash);
                break;
            case ETagListType.BudgetSet:
                LoadBudgetSet(tagHash);
                break;
            case ETagListType.Entity:
                LoadEntity(tagHash);
                return;
            case ETagListType.ApiList:
                LoadApiList();
                break;
            case ETagListType.ApiEntity:
                LoadApiEntity(tagHash);
                return;
            default:
                throw new NotImplementedException();
        }
        
        if (tagHash != null && !bFromBack)
        {
            _parentStack.Push(new ParentInfo { Hash = _currentHash, TagListType = _tagListType, SearchTerm = SearchBox.Text});
        }

        _currentHash = tagHash;
        _tagListType = tagListType;
        if (!bFromBack)
        {
            SearchBox.Text = "";
        }

        RefreshItemList();
        _tagListLogger.Debug($"Loaded content type {tagListType} tagHash {tagHash} from back {bFromBack}");
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
            if (displayItems.Count > 50) return;
            string name = _bTrimName ? TrimName(item._name) : item._name;
            if (name.ToLower().Contains(searchStr))
            {
                displayItems.Add(new TagItem
                {
                    Hash = item.Hash,
                    Name = name,
                    TagType = item.TagType,
                    Type = item.Type,
                    FontSize = _bTrimName ? 16 : 12,
                });
            }
        });
        
        List<TagItem> tagItems = displayItems.ToList();
        tagItems.Sort((p, q) => String.Compare(p.Name, q.Name, StringComparison.OrdinalIgnoreCase));

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
            TagHash reference = PackageHandler.GetEntryReference(val.Unk08.Hash);
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
                Hash = val.Unk08.Hash,
                Name = val.Unk00,
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
                Hash = val.Unk08.Hash,
                Name = val.Unk00,
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
    
    private void LoadApiEntity(TagHash hash)
    {
        var apiHash = new DestinyHash(hash.GetHashString(), false);
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
        get => _name == "BACK" ? "BACK" : $"[{Hash}]  {_name}";
        set => _name = value;
    }
    public string Hash { get; set; }
    
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