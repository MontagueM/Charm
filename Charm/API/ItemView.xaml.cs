using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Arithmic;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm;

/// <summary>
/// Interaction logic for ItemView2.xaml
/// </summary>
public partial class ItemView : UserControl, INotifyPropertyChanged
{
    private InventoryItem _invItem;

    private APIItem _item;
    public APIItem Item
    {
        get => _item;
        set
        {
            if (_item != value)
            {
                _item = value;
                OnPropertyChanged(nameof(Item));
            }
        }
    }
    public EmblemItem Emblem { get; set; }

    public SC4548080? StatGroup = null;

    private ObservableCollection<SocketCategory> _socketCategories = new();
    public ObservableCollection<SocketCategory> SocketCategories
    {
        get => _socketCategories;
        set
        {
            if (_socketCategories != value)
            {
                _socketCategories = value;
                OnPropertyChanged(nameof(SocketCategories));
            }
        }
    }

    public ObservableCollection<StatEntry> StatEntries { get; set; } = new();
    public ObservableCollection<StatEntry> NumericStatEntries { get; set; } = new();

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public ItemView(InventoryItem item)
    {
#if DEBUG
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
        Console.WriteLine($"----{item.Name}----");
        Console.WriteLine($"ITEM: {item.Hash} ({item.ApiHash})");
        Console.WriteLine($"STRINGS: {item.GetItemStrings()?.Hash}");
        Console.WriteLine($"ICON: {Investment.Get().GetItemIconContainer(item)?.Hash}");
#endif

        _invItem = item;
        StatGroup = Investment.Get().GetStatGroup(_invItem);
        CompositionTarget.Rendering += OnRender;

        InitializeComponent();
        LoadItem();

        SocketCategoriesPage.OnBeforePageChange += (s, e) =>
        {
            foreach (var popup in UIHelper.FindVisualChildren<HoverPopupWrapper>(this))
            {
                popup.ForceClose();
            }
        };
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        Focusable = true;
        Focus();
    }

    public void LoadItem()
    {
        var collectible = Investment.Get().GetCollectibleStringsFromItemIndex(_invItem.GetItemIndex());
        Item = new()
        {
            Item = _invItem,
            ItemName = _invItem.Name.ToUpper(),
            ItemType = _invItem.Type.ToUpper(),
            ItemFlavorText = _invItem.FlavorText,
            ItemLore = _invItem.Lore,
            ItemHash = _invItem.ApiHash,
            ItemSource = collectible != null ? collectible.Value.SourceString?.Value : "",
            ItemRarity = _invItem.GetItemRarity(),
            ItemDamageType = DestinyDamageType.GetDamageType(_invItem.DamageTypeIndex),

            ItemIconBackground = ApiImageUtils.MakeItemIconBackground(_invItem),
            ItemIcon = ApiImageUtils.MakeItemIconForeground(_invItem),
            ItemIconOverlay = ApiImageUtils.MakeItemIconOverlay(_invItem),
            ItemWatermark = ApiImageUtils.GetPlugWatermark(_invItem),

            ItemBackground = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{_invItem.ApiHash}.jpg")),
            ItemFoundryBanner = !_invItem.IsEmblem ? ApiImageUtils.MakeFoundryBanner(_invItem) : null,
        };

        if (_invItem.IsEmblem)
        {
            short index = _invItem.GetItemStrings().TagData.EmblemContainerIndex;
            var container = Investment.Get().GetItemIconContainer(index);
            if (container is null)
                return;

            var col = container.TagData.DyeColorR;
            Emblem = new()
            {
                EmblemLarge = ApiImageUtils.MakeIcon(index, containerIndex: 5),
                EmblemMedium = ApiImageUtils.MakeIcon(index),
                EmblemSmall = ApiImageUtils.MakeIcon(index, containerIndex: 4),
                EmblemColor = (Color.FromScRgb(col.W, col.X, col.Y, col.Z))
            };
        }

        DataContext = this;

        LoadItemStats();
        LoadSocketCategories();

        if (Item.ItemLore != string.Empty)
            ShowLoreHint.Visibility = Visibility.Visible;
    }

    public void LoadSocketCategories()
    {
        SocketCategories.Clear();

        if (_invItem.TagData.Unk70.GetValue(_invItem.GetReader()) is SC0778080 sockets)
        {
            List<SocketCategory> socketCategories = new();
            List<SocketEntry> socketEntries = new();

            for (int i = 0; i < sockets.IntrinsicSockets.Count; i++)
            {
                SC8778080 socket = sockets.IntrinsicSockets[i];
                if (socket.SocketTypeIndex == -1)
                    continue;

                SBA768080 type = Investment.Get().GetSocketType(socket.SocketTypeIndex);

                if (type.SocketVisiblity == 1)// && !type.PlugWhitelists.Any(x => x.PlugCategoryHash.Hash32 == 778194869))
                    continue;

                S5D4F8080 category = Investment.Get().SocketCategoryStrings[type.SocketCategoryIndex];
                SocketCategory socketCategory = new()
                {
                    CategoryStyle = category.CategoryStyle,
                    CategoryHash = category.SocketCategoryHash,
                    CategoryName = category.SocketName?.Value ?? "",
                    CategoryDescription = category.SocketDescription?.Value ?? "",
                    CategoryIndex = type.SocketCategoryIndex,
                    Sockets = new List<SocketEntry>()
                };

                // Only add if not already present
                if (!socketCategories.Any(x => x.CategoryHash == category.SocketCategoryHash))
                    socketCategories.Add(socketCategory);
                ///--------------------------

                SocketEntry socketEntry = new();
                socketEntry.SocketTypeIndex = socket.SocketTypeIndex;
                var plugItem = CreatePlugItem(socket.PlugItemIndex, category.CategoryStyle);
                if (plugItem is not null)
                {
                    plugItem.IsSelected = true;
                    plugItem.ParentSocket = socketEntry;
                    ApplyPlugStats(plugItem);
                }

                socketEntry.SingleInitialItem = plugItem;
                socketEntry.CategoryStyle = category.CategoryStyle;
                socketEntry.CategoryHash = category.SocketCategoryHash;
                socketEntries.Add(socketEntry);
            }

            for (int i = 0; i < sockets.SocketEntries.Count; i++)
            {
                SC3778080 socket = sockets.SocketEntries[i];
                if (socket.SocketTypeIndex == -1)
                    continue;

                SBA768080 type = Investment.Get().GetSocketType(socket.SocketTypeIndex);

                if (type.SocketVisiblity == 1)// && !type.PlugWhitelists.Any(x => x.PlugCategoryHash.Hash32 == 778194869))
                    continue;

                S5D4F8080 category = Investment.Get().SocketCategoryStrings[type.SocketCategoryIndex];
                if (category.CategoryStyle == DestinySocketCategoryStyle.EnergyMeter)
                    continue; // Dont really care about this one, its just a visual thing

                SocketCategory socketCategory = new()
                {
                    CategoryStyle = category.CategoryStyle,
                    CategoryHash = category.SocketCategoryHash,
                    CategoryName = category.SocketName?.Value ?? "",
                    CategoryDescription = category.SocketDescription?.Value ?? "",
                    CategoryIndex = type.SocketCategoryIndex,
                    Sockets = new List<SocketEntry>()
                };
                if (socketCategory.CategoryName.Contains("cosmetic", StringComparison.InvariantCultureIgnoreCase))
                    socketCategory.CategoryIndex = 99999;

                // Only add if not already present
                if (!socketCategories.Any(x => x.CategoryHash == category.SocketCategoryHash))
                    socketCategories.Add(socketCategory);

                SocketEntry socketEntry = new();

                /// Plug Items ----------------------------
                List<APIPlugItem> plugItems = new();
                APIPlugItem? plugItem;

                APIPlugItem? initialPlugItem = CreatePlugItem(socket.SingleInitialItemIndex, category.CategoryStyle);
                if (initialPlugItem is not null)
                {
                    initialPlugItem.IsSelected = true;
                    initialPlugItem.ParentSocket = socketEntry;
                    initialPlugItem.Index = i;
                    plugItems.Add(initialPlugItem);
                    ApplyPlugStats(initialPlugItem);
                }

                foreach (SD5778080 plug in socket.PlugItems)
                {
                    plugItem = CreatePlugItem(plug.PlugInventoryItemIndex, category.CategoryStyle);
                    if (plugItem is not null)
                    {
                        plugItem.ParentSocket = socketEntry;
                        plugItem.Index = i;
                        plugItems.Add(plugItem);
                    }
                }

                foreach (short index in new short[] { socket.ReusablePlugSetIndex1, socket.ReusablePlugSetIndex2 })
                {
                    if (index != -1)
                    {
                        foreach (SD5778080 randomPlugs in Investment.Get().GetRandomizedPlugSet(index))
                        {
                            plugItem = CreatePlugItem(randomPlugs.PlugInventoryItemIndex, category.CategoryStyle);
                            if (plugItem is not null)
                            {
                                plugItem.ParentSocket = socketEntry;
                                plugItem.Index = i;
                                plugItems.Add(plugItem);
                            }
                        }
                    }
                }

                if (type.SocketVisiblity == 2 && plugItems.Count == 0) // "HiddenWhenEmpty"
                    continue;

                ///--------------------------
                socketEntry.SocketTypeIndex = socket.SocketTypeIndex;
                socketEntry.CategoryStyle = category.CategoryStyle;
                socketEntry.CategoryHash = category.SocketCategoryHash;
                socketEntry.PlugItems = plugItems.DistinctBy(x => x.Hash).ToList();
                if (initialPlugItem is null && socketEntry.PlugItems.Count != 0)
                {
                    var newInitial = socketEntry.PlugItems.First();
                    newInitial.IsSelected = true;
                    socketEntry.SingleInitialItem = newInitial;
                    ApplyPlugStats(newInitial);
                }
                else
                    socketEntry.SingleInitialItem = initialPlugItem;

                // TODO find a way to filter out "duplicate" nodes, each upgrade set (1-5) has the same name but different api hashes

                // Kinda bad probably, just moves the Masterwork socket to be right after Infusion
                if (type.PlugWhitelists.Any(x => x.PlugCategoryHash.Hash32 is 2198080209 or 3185182717) && socketEntries.Count >= 2) // "v460.plugs.armor.masterworks" or "v400.plugs.weapons.masterworks"
                    socketEntries.Insert(1, socketEntry);
                else
                    socketEntries.Add(socketEntry);
            }


            // Armor set bonuses
            if (_invItem.IsArmor && _invItem.TagData.Unk18.GetValue(_invItem.GetReader()) is SE7778080 equippingBlock)
            {
                if (equippingBlock.ItemSetIndex != -1)
                {
                    var itemSet = Investment.Get().EquipableItemSets[equippingBlock.ItemSetIndex];
                    var itemSetStrings = Investment.Get().EquipableItemSetStrings[equippingBlock.ItemSetIndex];

                    SocketCategory setCategory = new()
                    {
                        CategoryStyle = DestinySocketCategoryStyle.ArmorPerkSet,
                        CategoryHash = itemSet.SetHash,
                        CategoryName = itemSetStrings.SetName?.Value ?? "",
                        CategoryDescription = itemSetStrings.SetDescription?.Value ?? "",
                        CategoryIndex = 99998,
                        Sockets = new List<SocketEntry>
                        {
                            new()
                            {
                                CategoryStyle = DestinySocketCategoryStyle.ArmorPerkSet,
                            }
                        }
                    };

                    List<APIPlugItem> plugs = new();
                    foreach (var perk in itemSet.SetPerks)
                    {
                        var sandboxPerk = Investment.Get().SandboxPerkStrings[perk.PerkIndex];
                        var plugItem = new APIPlugItem()
                        {
                            OverrideName = $"{perk.SetCount} PIECE | {sandboxPerk.SandboxPerkName?.Value}",
                            OverrideDescription = sandboxPerk.SandboxPerkDescription?.Value ?? "",
                            Hash = sandboxPerk.SandboxPerkHash,
                        };

                        plugItem._iconLoader = new AsyncImageLoader(
                        () => ApiImageUtils.MakeIcon(sandboxPerk.IconIndex),
                        () => OnPropertyChanged(nameof(APIPlugItem.Icon)), true);

                        plugs.Add(plugItem);
                    }
                    setCategory.Sockets.First().PlugItems = plugs;

                    socketCategories.Add(setCategory);
                }
            }


            // Group socketEntries by CategoryHash and add to corresponding SocketCategory
            foreach (var category in socketCategories.OrderBy(x => x.CategoryIndex))
            {
                var entriesForCategory = socketEntries.Where(e => e.CategoryHash.Equals(category.CategoryHash));
                foreach (var entry in entriesForCategory)
                {
                    category.Sockets.Add(entry);
                }
                SocketCategories.Add(category);
            }
        }


        if (SocketCategories.Any(x => x.CategoryName.Contains("cosmetic", StringComparison.InvariantCultureIgnoreCase)))
        {
            SocketCategoriesPage.ItemsPerPage = SocketCategories.Count - 1;
            AppearanceSubscreen.Visibility = Visibility.Visible;
        }
    }

    public static APIPlugItem CreatePlugItem(int index, DestinySocketCategoryStyle parentSocketStyle)
    {
        if (index == -1)
            return null;

        InventoryItem item = Investment.Get().GetInventoryItem(index);
        var plug = new APIPlugItem(item);
        plug.ParentSocketStyle = parentSocketStyle;
        return plug;
    }

    private void LoadItemStats()
    {
        List<StatEntry> entries = new();
        if (_invItem.TagData.Unk78.GetValue(_invItem.GetReader()) is S81738080 stats)
        {
            SC4548080? statGroup = StatGroup;

            if (statGroup is not null)
            {
                foreach (SC8548080 scaledStat in statGroup.Value.ScaledStats)
                {
                    S6F588080 StatEntry = Investment.Get().StatStrings[scaledStat.StatIndex];

                    int statValue = stats.InvestmentStats.Where(x => x.StatTypeIndex == scaledStat.StatIndex).FirstOrDefault().Value;
                    int displayValue = MakeDisplayValue(scaledStat.StatIndex, statValue);

                    entries.Add(new StatEntry
                    {
                        StatHash = StatEntry.StatHash,
                        StatIndex = scaledStat.StatIndex,
                        StatName = StatEntry.StatName.Value.ToString(),
                        StatDescription = StatEntry.StatDescription.Value.ToString(),
                        StatDisplayValue = displayValue,
                        StatBaseValue = statValue,
                        StatValue = statValue,
                        StatDisplayNumeric = scaledStat.DisplayAsNumeric == 1,
                        StatIsLinear = scaledStat.IsLinear == 1
                    });
                    //Console.WriteLine($"Base {StatEntry.StatName.Value} : Val {statValue} Disp {displayValue}");
                }
            }
        }

        foreach (var entry in entries.Where(x => !x.StatDisplayNumeric))
            StatEntries.Add(entry);

        foreach (var entry in entries.Where(x => x.StatDisplayNumeric))
            NumericStatEntries.Add(entry);
    }

    private void ApplyPlugStats(APIPlugItem item)
    {
        if (item.ParentSocket.SelectedPlug is not null)
            RemovePreviousPlugStats(item.ParentSocket.SelectedPlug);

        item.ParentSocket.SelectedPlug = item;

        if (item.Item?.TagData.Unk78.GetValue(item.Item.GetReader()) is not S81738080)
            return;

        var stats = GetItemStatValues(item.Item);

        foreach ((StatEntry stat, int value) in stats)
        {
            //Console.WriteLine($"PlugItem_Checked {item.Item.Name}: {stat.StatName} Val {value} Disp {MakeDisplayValue(stat.StatIndex, value)}");
            stat.StatValue = Math.Min(StatGroup?.MaximumValue ?? 100, stat.StatValue + value); ;
            stat.StatDisplayValue = MakeDisplayValue(stat.StatIndex, stat.StatValue);
            stat.StatAdjustValue = 0;
        }
    }

    private void RemovePreviousPlugStats(APIPlugItem item)
    {
        if (item.Item?.TagData.Unk78.GetValue(item.Item.GetReader()) is not S81738080)
            return;

        var stats = GetItemStatValues(item.Item);

        foreach ((StatEntry stat, int value) in stats)
        {
            //Console.WriteLine($"RemovePreviousPlugStats {item.Item.Name}: {stat.StatName} Val {value} Disp {MakeDisplayValue(stat.StatIndex, value)} : Base {stat.StatBaseValue}, {stat.StatValue - value}");
            stat.StatValue = Math.Max(stat.StatBaseValue, stat.StatValue - value);
            stat.StatDisplayValue = MakeDisplayValue(stat.StatIndex, stat.StatValue);
            stat.StatAdjustValue = 0;
        }
    }

    public Dictionary<StatEntry, int> GetItemStatValues(InventoryItem item)
    {
        Dictionary<StatEntry, int> statValues = new();
        if (item.TagData.Unk78.GetValue(item.GetReader()) is S81738080 stats)
        {
            foreach (S86738080 stat in stats.InvestmentStats)
            {
                S6F588080 StatEntry = Investment.Get().StatStrings[stat.StatTypeIndex];
                if (StatEntry.StatName.Value is not null)
                {
                    StatEntry? _statEntry = StatEntries.Union(NumericStatEntries).FirstOrDefault(x => x.StatHash == StatEntry.StatHash);
                    if (_statEntry is null)
                        continue;

                    statValues.TryAdd(_statEntry, stat.Value);
                }
            }
        }
        return statValues;
    }

    private int MakeDisplayValue(int statIndex, int statValue)
    {
        if (_invItem.TagData.Unk78.GetValue(_invItem.GetReader()) is S81738080 investmentStats)
        {
            SC4548080? statGroup = StatGroup;
            if (!statGroup.HasValue || statGroup is null)
                return statValue;

            SC8548080 stat = statGroup.Value.ScaledStats.FirstOrDefault(x => x.StatIndex == statIndex);
            if (statValue < 0 || stat.DisplayInterpolation is null)
                return statValue;

            if (stat.DisplayInterpolation.Any())
            {
                return InterpolateStatValue(stat, statValue, statGroup.Value.MaximumValue);
            }
            else if (stat.IsLinear == 1) // Is this even a thing? Idk if I've been wrong about this
            {
                return statValue;
            }
            else // This shouldnt happen, i don't think?
                return statValue;
        }
        return 0;
    }

    // https://github.com/DestinyItemManager/DIM/blob/60b460587f8c22ffa170ca8b05dd59384bd1bef2/src/app/inventory/store/stats.ts#L522
    public int InterpolateStatValue(SC8548080 statDisp, int value, int maxValue)
    {
        var statStr = Investment.Get().StatStrings[statDisp.StatIndex];

        // Clamp the value to prevent overfilling
        value = Math.Min(value, maxValue);

        var interp = statDisp.DisplayInterpolation;
        int endIndex = interp.FindIndex(p => p.Value > value);
        // value < 0 is for mods with negative stats
        if (endIndex < 0)
            endIndex = interp.Count - 1;
        int startIndex = Math.Max(0, endIndex - 1);

        var start = interp[startIndex];
        var end = interp[endIndex];
        int range = end.Value - start.Value;
        if (range == 0)
            return start.Weight;

        float t = (float)(value - start.Value) / (float)(end.Value - start.Value);
        float interpValue = start.Weight + t * (end.Weight - start.Weight);

        return statStr.StatHash.Hash32 == (uint)StatHashes.Magazine
        ? (int)Math.Round(interpValue)
        : (int)BankersRound(interpValue);
    }

    /// <summary>
    /// "Banker's rounding" rounds numbers that perfectly fall halfway between two integers to the nearest
    /// even integer, instead of always rounding up.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public int BankersRound(float x)
    {
        int r = (int)Math.Round(x);
        return (x > 0 ? x : -x) % 1 == 0.5 ? (r % 2 == 0 ? r : r - 1) : r;
    }

    private void PlugItem_Checked(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not APIPlugItem)
            return;

        APIPlugItem item = (APIPlugItem)(sender as FrameworkElement).DataContext;
        if (item.IsSelected)
        {
            ApplyPlugStats(item);

            if (item.Item.IsOrnament)
            {
                try
                {
                    Item.ItemBackground = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{item.Item.ApiHash}.jpg"));
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to get background for {item.Item.Hash}: {ex.Message}");
                }
            }
            else if (item.Item.Name.Contains("default ornament", StringComparison.InvariantCultureIgnoreCase))
            {
                Item.ItemBackground = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{_invItem.ApiHash}.jpg"));
            }
        }
    }

    // Displays the Stat deltas as green or red bars
    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not APIPlugItem)
            return;

        APIPlugItem item = (APIPlugItem)(sender as FrameworkElement).DataContext;
        if (item.IsSelected)
            return;

        if (item.Item?.TagData.Unk78.GetValue(item.Item.GetReader()) is not S81738080)
            return;

        var newStats = GetItemStatValues(item.Item);
        var oldStats = (item.ParentSocket is not null && item.ParentSocket.SelectedPlug is not null)
            ? GetItemStatValues(item.ParentSocket.SelectedPlug.Item)
            : new();

        var allStats = new HashSet<StatEntry>(newStats.Keys);
        allStats.UnionWith(oldStats.Keys);
        foreach (var stat in allStats)
        {
            int newValue = newStats.TryGetValue(stat, out var n) ? n : 0;
            int oldValue = oldStats.TryGetValue(stat, out var o) ? o : 0;
            int delta = newValue - oldValue;
            //Console.WriteLine($"{stat.StatName}" +
            //    $"\nCur {stat.StatValue}, Disp {stat.StatDisplayValue}" +
            //    $"\nAdjust {delta}, Disp {MakeDisplayValue(stat.StatIndex, delta)}" +
            //    $"\nTotal {stat.StatValue + delta}, Disp {MakeDisplayValue(stat.StatIndex, stat.StatValue + delta)}\n");

            stat.StatAdjustDisplayValue = MakeDisplayValue(stat.StatIndex, stat.StatValue + delta);
            stat.StatAdjustValue = stat.StatAdjustDisplayValue - stat.StatDisplayValue;
        }
    }

    private void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is not APIPlugItem)
            return;

        foreach (var stat in StatEntries.Union(NumericStatEntries))
        {
            stat.StatAdjustValue = 0;
        }
    }

    private void Consumable_RightClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var item = (sender as FrameworkElement).DataContext as APIPlugItem;

        if (item.Item.IsOrnament)
        {
            try
            {
                Item.ItemBackground = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{item.Item.ApiHash}.jpg"));
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get background for {item.Item.Hash}: {ex.Message}");
            }
        }
        else if (item.Item.Name.Contains("default ornament", StringComparison.InvariantCultureIgnoreCase))
        {
            Item.ItemBackground = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{_invItem.ApiHash}.jpg"));
        }
    }

    private void ItemTooltip_MouseEnter(object sender, MouseEventArgs e)
    {
        FrameworkElement element = sender as FrameworkElement;
        MainWindow.Current.ToolTip.ActiveItem = element;

        string name = "";
        string description = "";
        switch (element.DataContext)
        {
            case SocketCategory item:
                name = item.CategoryName;
                description = item.CategoryDescription;
                break;

            case StatEntry item:
                name = item.StatName;
                description = item.StatDescription;
                break;
        }

        MainWindow.Current.ToolTip.MakeTooltip(new GenericTooltip()
        {
            Name = name,
            Description = description,
            Style = HeaderBlock.HeaderStyle.Category
        });
    }

    private void ItemTooltip_MouseLeave(object sender, MouseEventArgs e)
    {
        MainWindow.Current.ToolTip.ActiveItem = null;
        MainWindow.Current.ToolTip.ClearTooltip();
    }

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.IsRepeat)
            return;

        // I'm sure there's a better way to do this, but it works so eh
        if (e.Key == Key.S)
        {
            if (SocketCategoriesPage.SelectNextPage())
            {
                StatsPanel.Visibility = Visibility.Hidden;
                UIHelper.AnimateFade(SubscreenText, 0.05f, 0.8f, 0);
                SubscreenText.Text = "";
                SubscreenKey.Text = "";

                UIHelper.AnimateFade(SubscreenArrowUp, 0.1f, 0.5f, 0);
                SubscreenArrowUp.Visibility = Visibility.Visible;
                SubscreenArrowDown.Visibility = Visibility.Hidden;
            }
        }

        if (e.Key == Key.W)
        {
            if (SocketCategoriesPage.SelectPreviousPage())
            {
                StatsPanel.Visibility = Visibility.Visible;
                UIHelper.AnimateFade(SubscreenText, 0.1f, 0.8f, 0);
                SubscreenText.Text = UIHelper.AddSpacesBetweenChars("APPEARANCE", 1);
                SubscreenKey.Text = "";

                SubscreenArrowUp.Visibility = Visibility.Hidden;
                UIHelper.AnimateFade(SubscreenArrowDown, 0.05f, 0.5f, 0);
                SubscreenArrowDown.Visibility = Visibility.Visible;
            }

        }

        if (e.Key == Key.A)
        {
            if (Item.ItemLore == null || Item.ItemLore == string.Empty)
                return;

            if (!MainContainer.IsVisible && LoreEntry.Visibility == Visibility.Collapsed)
                return;

            ShowLoreHint.Text = LoreEntry.IsVisible ? " Show Lore" : " Hide Lore";
            if (LoreEntry.Visibility == Visibility.Collapsed)
            {
                UIHelper.AnimateSlide(LoreEntry, 0.05f, new(0, 0), new(-800, 0));
                UIHelper.AnimateFade(LoreEntry, 0.1f, 1f, 0f);
                LoreEntry.Visibility = Visibility.Visible;

                UIHelper.AnimateFade(MainContainer, 0.1f, 0f, 1f, (s, e) =>
                {
                    MainContainer.Visibility = Visibility.Hidden;
                });
            }
            else
            {
                MainContainer.Visibility = Visibility.Visible;
                UIHelper.AnimateFade(MainContainer, 0.1f, 1f, 0f);
                UIHelper.AnimateFade(LoreEntry, 0.1f, 0f, 1f, (s, e) =>
                {
                    LoreEntry.Visibility = Visibility.Collapsed;
                });
            }
        }

        if (e.Key is Key.LeftCtrl or Key.RightCtrl)
        {
            if (LoreEntry.IsVisible)
                return;

            HideMenuHint.Text = MainContainer.IsVisible ? " Show Menu" : " Hide Menu";
            if (MainContainer.Visibility != Visibility.Visible)
            {
                if (Item.ItemLore != string.Empty)
                    ShowLoreHint.Visibility = Visibility.Visible;
                MainContainer.Visibility = Visibility.Visible;
                ItemRarityBanner.Visibility = Visibility.Visible;
                UIHelper.AnimateFade(MainContainer, 0.1f, 1f, 0f);
                UIHelper.AnimateFade(ItemRarityBanner, 0.1f, 1f, 0f);
            }
            else
            {
                ShowLoreHint.Visibility = Visibility.Collapsed;
                UIHelper.AnimateFade(MainContainer, 0.1f, 0f, 1f, (s, e) =>
                {
                    MainContainer.Visibility = Visibility.Collapsed;
                });
                UIHelper.AnimateFade(ItemRarityBanner, 0.1f, 0f, 1f, (s, e) =>
                {
                    ItemRarityBanner.Visibility = Visibility.Collapsed;
                });
            }
        }
    }

    private void OnRender(object sender, EventArgs e)
    {
        if (!ConfigSubsystem.Get().GetMotionEffects())
            return;

        float x = -12f / (float)MainWindow.Current.ActualWidth;
        float y = -12f / (float)MainWindow.Current.ActualHeight;
        Point position = Mouse.GetPosition(this);

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = (int)Math.Round(position.X * x);
        gridTransform.Y = (int)Math.Round(position.Y * y);
    }

    public class APIItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public InventoryItem Item { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string ItemFlavorText { get; set; }
        public string ItemLore { get; set; }
        public string ItemSource { get; set; }
        public uint ItemHash { get; set; }
        public DestinyTierType ItemRarity { get; set; }
        public DestinyDamageTypeEnum ItemDamageType { get; set; }

        public ImageSource ItemIconBackground { get; set; }
        public ImageSource ItemIcon { get; set; }
        public ImageSource ItemIconOverlay { get; set; }
        public ImageSource ItemWatermark { get; set; }
        public ImageSource ItemFoundryBanner { get; set; }

        private ImageSource _itemBackground = null;
        public ImageSource ItemBackground
        {
            get => _itemBackground;
            set
            {
                if (_itemBackground != value)
                {
                    _itemBackground = value;
                    OnPropertyChanged(nameof(ItemBackground));
                }
            }
        }
    }

    public class EmblemItem
    {
        public ImageSource EmblemLarge { get; set; }
        public ImageSource EmblemMedium { get; set; }
        public ImageSource EmblemSmall { get; set; }
        public Color EmblemColor { get; set; }
    }
}

public class APIPlugItem : CharmUIElement
{
    public APIPlugItem() { }

    public APIPlugItem(InventoryItem item)
    {
        _iconBgLoader = new AsyncImageLoader(
            () => ApiImageUtils.MakeItemIconBackground(item),
            () => OnPropertyChanged(nameof(IconBackground)));

        _iconLoader = new AsyncImageLoader(
            () => ApiImageUtils.MakeItemIconForeground(item),
            () => OnPropertyChanged(nameof(Icon)));

        _iconOverlayLoader = new AsyncImageLoader(
            () => ApiImageUtils.MakeItemIconOverlay(item),
            () => OnPropertyChanged(nameof(IconOverlay)));

        _watermarkLoader = new AsyncImageLoader(
            () => ApiImageUtils.GetPlugWatermark(item),
            () => OnPropertyChanged(nameof(ItemWatermark)));

        Item = item;
        Hash = item.ApiHash;
    }

    public string OverrideName { get; set; } // Used for raw sandbox perks, since they arent InventoryItems
    public string OverrideDescription { get; set; }

    public SocketEntry ParentSocket;
    public DestinySocketCategoryStyle ParentSocketStyle { get; set; } // meh
    public Color RarityColor => Item?.GetItemRarity().GetColor() ?? Color.FromArgb(0, 0, 0, 0);

    public uint Hash { get; set; }

    private InventoryItem _item = null;
    public InventoryItem Item
    {
        get => _item;
        set
        {
            if (_item == value)
                return;

            _item = value;
            OnPropertyChanged(nameof(Item));
        }
    }

    protected internal AsyncImageLoader _iconLoader;
    public AsyncImageLoader IconLoader
    {
        get => _iconLoader;
        set
        {
            _iconLoader = value;
            OnPropertyChanged(nameof(IconLoader));
        }
    }

    private readonly AsyncImageLoader _iconBgLoader;
    private readonly AsyncImageLoader _iconOverlayLoader;
    private readonly AsyncImageLoader _watermarkLoader;

    public ImageSource IconBackground => _iconBgLoader.GetImage();
    public ImageSource IconOverlay => _iconOverlayLoader.GetImage();
    public ImageSource ItemWatermark => Strategy.IsD1() ? null : _watermarkLoader.GetImage();

    public ImageSource Icon
    {
        get => _iconLoader?.GetImage() ?? null;
        set
        {
            _iconLoader?.SetImage(value);
            OnPropertyChanged(nameof(Icon));
        }
    }
}

public class SocketCategory : CharmUIElement
{
    public DestinySocketCategoryStyle CategoryStyle { get; set; }
    public TigerHash CategoryHash { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }
    public int CategoryIndex { get; set; }

    public List<SocketEntry> Sockets { get; set; } = new();
}

public class SocketEntry : CharmUIElement
{
    private APIPlugItem _selectedPlug = null;
    public APIPlugItem SelectedPlug
    {
        get => _selectedPlug;
        set
        {
            if (_selectedPlug != value)
            {
                _selectedPlug = value;
                OnPropertyChanged();
            }
        }
    }

    public DestinySocketCategoryStyle CategoryStyle { get; set; }
    public TigerHash CategoryHash { get; set; }
    public APIPlugItem SingleInitialItem { get; set; }
    public List<APIPlugItem> PlugItems { get; set; } = new();
    public int SocketTypeIndex { get; set; }

    // currently for renderer to get the parent item the shader socket belongs to
    public APIPlugItem ParentItem { get; set; }
}

public class StatEntry : CharmUIElement
{
    public TigerHash StatHash { get; set; }
    public int StatIndex { get; set; }
    public string StatName { get; set; }
    public string StatType { get; set; }
    public string StatDescription { get; set; }

    public bool StatDisplayNumeric { get; set; }
    public bool StatIsLinear { get; set; }

    public int StatBaseValue { get; set; }

    private int _statValue;
    public int StatValue
    {
        get { return _statValue; }
        set
        {
            _statValue = Math.Max(0, value);
            OnPropertyChanged(nameof(StatValue));
        }
    }

    private int _statDisplayValue;
    public int StatDisplayValue
    {
        get { return _statDisplayValue; }
        set
        {
            _statDisplayValue = Math.Max(0, value);
            OnPropertyChanged(nameof(StatDisplayValue));
        }
    }

    private int _statAdjustValue;
    public int StatAdjustValue
    {
        get { return _statAdjustValue; }
        set
        {
            _statAdjustValue = value;
            OnPropertyChanged(nameof(StatAdjustValue));
        }
    }

    private int _statAdjustDisplayValue;
    public int StatAdjustDisplayValue
    {
        get { return _statAdjustDisplayValue; }
        set
        {
            _statAdjustDisplayValue = Math.Max(0, value);
            OnPropertyChanged(nameof(StatAdjustDisplayValue));
        }
    }
}

public class RarityBannerConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        DestinyTierType rarity = (DestinyTierType)value;
        float divisor = float.Parse(parameter as string, CultureInfo.InvariantCulture);
        return rarity.GetColor().Divide(divisor);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class SocketTemplateSelector : DataTemplateSelector
{
    public DataTemplate ReusableTemplate { get; set; }
    public DataTemplate ConsumableTemplate { get; set; }
    public DataTemplate LargePerkTemplate { get; set; }
    public DataTemplate ArmorPerkSetTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is SocketEntry entry)
        {
            switch (entry.CategoryStyle)
            {
                case DestinySocketCategoryStyle.Unlockable:
                case DestinySocketCategoryStyle.Reusable:
                    return ReusableTemplate;
                case DestinySocketCategoryStyle.Consumable:
                    return ConsumableTemplate;
                case DestinySocketCategoryStyle.LargePerk:
                    return LargePerkTemplate;
                case DestinySocketCategoryStyle.ArmorPerkSet:
                    return ArmorPerkSetTemplate;
                default:
                    //Console.WriteLine(entry.CategoryStyle);
                    break;
            }
        }

        return base.SelectTemplate(item, container);
    }
}

public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int intValue) && int.TryParse(parameter?.ToString(), out int totalWidth))
            return (intValue / 100f) * totalWidth;

        return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class MarginConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // need to adjust the margin if the stat is negative
        if (value is int adjustmentValue && adjustmentValue < 0)
            return new Thickness((adjustmentValue / 100f) * 210f, 0, 0, 0);

        return new Thickness(0, 0, 0, 0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class SignToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int adjustmentValue)
            return adjustmentValue >= 0;

        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class FlipSignPercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int adjustmentValue && int.TryParse(parameter?.ToString(), out int totalWidth))
        {
            if (adjustmentValue < 0)
                return ((adjustmentValue * -1) / 100f) * totalWidth;

            return ((adjustmentValue * 1) / 100f) * totalWidth;
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
