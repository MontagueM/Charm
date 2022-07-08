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
    [Description("Back")]
    Back,
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
        switch (tagListType)
        {
            case ETagListType.DestinationGlobalTagBagList:
                LoadDestinationGlobalTagBagList();
                break;
            case ETagListType.Back:
                Back_Clicked();
                return;
            case ETagListType.DestinationGlobalTagBag:
                DestinationGlobalTagBag_Click(tagHash);
                break;
            case ETagListType.BudgetSet:
                BudgetSet_Click(tagHash);
                break;
            case ETagListType.Entity:
                Entity_Click(tagHash);
                return;
            default:
                throw new NotImplementedException();
        }
        
        if (tagHash != null && !bFromBack)
        {
            _parentStack.Push(new ParentInfo { Hash = _currentHash, TagListType = _tagListType, SearchTerm = searchBox.Text});
        }

        _currentHash = tagHash;
        _tagListType = tagListType;
        if (!bFromBack)
        {
            searchBox.Text = "";
        }

        RefreshItemList();
    }

    private void SetItemListByString(string searchStr)
    {
        var displayItems = new ConcurrentBag<TagItem>();
        // Select and sort by relevance to selected string
        Parallel.ForEach(_allTagItems, item =>
        { 
            if (displayItems.Count > 50) return;
            if (item.Name.ToLower().Contains(searchStr))
            {
                displayItems.Add(item);
            }
        });
        
        // If we have a parent, add a TagItem that is actually a back button as first
        if (_parentStack.Count > 0)
        {
            List<TagItem> tagItems = displayItems.ToList();
            tagItems.Insert(0, new TagItem
            {
                Name = "Back",
                TagType = ETagListType.Back,
            });
            TagList.ItemsSource = tagItems;
        }
        else
        {
            TagList.ItemsSource = displayItems;
        }
    }

    private void RefreshItemList()
    {
        SetItemListByString(searchBox.Text);
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
        searchBox.Text = parentInfo.SearchTerm;
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
                Name = $"[{dgtbParent.Header.DestinationGlobalTagBag}] {dgtbParent.Header.DestinationGlobalTagBagName}",
                TagType = ETagListType.DestinationGlobalTagBag
            });
        });
    }
    
    private void DestinationGlobalTagBag_Click(TagHash hash)
    {
        LoadDestinationGlobalTagBag(hash);
        RefreshItemList();
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
                Name = $"[{val.Unk08.Hash}] {val.Unk00}",
                TagType = tagType,
                Type = overrideType
            });
        });
    }

    #endregion
    
    #region Budget Set
    
    private void BudgetSet_Click(TagHash hash)
    {
        LoadBudgetSet(hash);
        RefreshItemList();
    }
    
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
                Name = $"[{val.Unk08.Hash}] {val.Unk00}",
                TagType = ETagListType.Entity,
            });
        });
    }
    
    #endregion

    #region Entity

    private void Entity_Click(TagHash tagHash)
    {
        EntityView.LoadEntity(tagHash);
        ExportView.SetExportInfo(tagHash);
    }

    #endregion
}

public class TagItem
{
    private string _type = String.Empty;
    public string Name { get; set; }
    public string Hash { get; set; }

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