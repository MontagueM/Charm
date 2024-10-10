using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Arithmic;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Investment;
using Color = System.Windows.Media.Color;

namespace Charm;

// This code is garbaaaaage
public partial class APIItemView : UserControl
{
    private ApiItemView ApiItem;
    private ObservableCollection<PlugItem> _plugItems;
    private ObservableCollection<StatItem> _statItems;
    private APITooltip ToolTip;

    Grid? ActiveStatItemGrid;

    public APIItemView(InventoryItem item)
    {
        InitializeComponent();
        string name = Investment.Get().GetItemName(item);
        string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)).TagData.ItemType.Value;
        if (type == null)
            type = "";

        var source = Investment.Get().GetCollectibleStringsFromItemIndex(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));
        var sourceString = source != null ? source.Value.SourceName.Value.ToString() : "";

        var foundryBanner = ApiImageUtils.MakeFoundryBanner(item);
        var image = ApiImageUtils.MakeIcon(item);
        ApiItem = new ApiItemView
        {
            Item = item,
            ItemName = name.ToUpper(),
            ItemType = type.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStrings(item.TagData.InventoryItemHash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(item.TagData.InventoryItemHash)?.LoreDescription.Value.ToString(),
            ItemSource = sourceString,
            ImageSource = image.Keys.First(),
            FoundryIconSource = foundryBanner,
            ItemDamageType = (DestinyDamageType.GetDamageType(item.GetItemDamageTypeIndex())).GetEnumDescription(),
            ItemPowerCap = item.GetItemPowerCap()
        };
        Load();
    }

    public APIItemView(ApiItem apiItem)
    {
        InitializeComponent();

        var source = Investment.Get().GetCollectibleStringsFromItemIndex(Investment.Get().GetItemIndex(apiItem.Item.TagData.InventoryItemHash));
        var sourceString = source != null ? source.Value.SourceName.Value.ToString() : "";

        var hash = apiItem.Item.TagData.InventoryItemHash;
        var foundryBanner = ApiImageUtils.MakeFoundryBanner(apiItem.Item);
        ApiItem = new ApiItemView
        {
            Item = apiItem.Item,
            ItemName = apiItem.ItemName?.ToUpper(),
            ItemType = apiItem.ItemType?.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStrings(hash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(hash)?.LoreDescription.Value.ToString(),
            ItemSource = sourceString,
            ImageSource = apiItem.ImageSource,
            FoundryIconSource = foundryBanner,
            ItemDamageType = (DestinyDamageType.GetDamageType(apiItem.Item.GetItemDamageTypeIndex())).GetEnumDescription(),
            ItemPowerCap = apiItem.Item.GetItemPowerCap()
        };
        Load();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
#if DEBUG
        Console.WriteLine($"Item {ApiItem.ItemName} {ApiItem.Item.Hash}");
        Console.WriteLine($"Strings {Investment.Get().GetItemStrings(ApiItem.Item.TagData.InventoryItemHash)?.Hash}");
        Console.WriteLine($"Icons {Investment.Get().GetItemIconContainer(ApiItem.Item.TagData.InventoryItemHash)?.Hash}");
#endif
        if (ApiItem.ItemLore != null)
        {
            LoreEntry.DataContext = ApiItem;
            ShowLoreHint.Visibility = Visibility.Visible;
        }

        try
        {
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{ApiItem.Item.TagData.InventoryItemHash.Hash32}.jpg"));
            ItemBackground.Background = imageBrush;
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to get background for {ApiItem.Item.Hash}: {ex.Message}");
        }
    }

    private void Load()
    {
        MouseMove += UserControl_MouseMove;
        KeyDown += UserControl_KeyDown;
        MainContainer.DataContext = ApiItem;
        BackgroundContainer.DataContext = ApiItem;

        ToolTip = new();
        //MouseMove += ToolTip.UserControl_MouseMove;
        Panel.SetZIndex(ToolTip, 50);
        MainGrid.Children.Add(ToolTip);

        _statItems = new();
        _plugItems = new();

        if (ApiItem.ItemType == "EMBLEM")
        {
            var index = ApiItem.Item.GetItemStrings().TagData.EmblemContainerIndex;
            EmblemItem emblem = new()
            {
                EmblemLarge = ApiImageUtils.MakeFullIcon(index, 5),
                EmblemMedium = ApiImageUtils.MakeFullIcon(index),
                EmblemSmall = ApiImageUtils.MakeFullIcon(index, 4),
            };
            EmblemContainer.DataContext = emblem;
        }
        if (ApiItem.ItemDamageType == "")
        {
            if (ApiItem.Item.TagData.Unk70.GetValue(ApiItem.Item.GetReader()) is D2Class_C0778080 sockets)
            {
                sockets.SocketEntries.ForEach(entry =>
                {
                    if (entry.SocketTypeIndex == -1)
                        return;
                    var socket = Investment.Get().GetSocketType(entry.SocketTypeIndex);
                    foreach (var a in socket.PlugWhitelists)
                    {
                        if (a.PlugCategoryHash.Hash32 == 1466776700) // 'v300.weapon.damage_type.energy', Y1 weapon that uses a damage type mod from ye olden days
                        {
                            var item = Investment.Get().GetInventoryItem(entry.SingleInitialItemIndex);
                            item.Load(true); // idk why the item sometimes isnt fully loaded
                            var index = item.GetItemDamageTypeIndex();
                            ApiItem.ItemDamageType = DestinyDamageType.GetDamageType(index).GetEnumDescription();
                        }
                    }
                });
            }
        }

        CreateSocketCategories();
        GetItemStats();
    }

    private PlugItem? CreatePlugItem(int plugIndex)
    {
        if (plugIndex == -1)
            return null;

        var item = Investment.Get().GetInventoryItem(plugIndex);
        var strings = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));
        var type = strings.TagData.ItemType.Value.ToString();
        if (type == "Shader") // Too slow atm, not really important either
            return null;

        var icon = ApiImageUtils.MakeIcon(item);
        var name = Investment.Get().GetItemName(item).ToUpper();
        var description = strings.TagData.ItemDisplaySource.Value.ToString();
        //var socketName = Investment.Get().SocketCategoryStringThings[socketIndex].SocketName.Value.ToString();
        if (name == "" && type == "" && description == "")
            return null;

        TigerHash plugCategoryHash = null;
        if (item.TagData.Unk48.GetValue(item.GetReader()) is D2Class_A1738080 plug)
            plugCategoryHash = plug.PlugCategoryHash;

        PlugItem PlugItem = new PlugItem
        {
            Item = item,
            Hash = item.TagData.InventoryItemHash,
            Name = name,
            Type = type,
            Description = description,
            PlugImageSource = icon.Keys.First(),
            //PlugSocketIndex = socketIndex,
            PlugCategoryHash = plugCategoryHash,
            PlugWatermark = ApiImageUtils.GetPlugWatermark(item),
            PlugRarity = (DestinyTierType)item.TagData.ItemRarity,
            PlugRarityColor = ((DestinyTierType)item.TagData.ItemRarity).GetColor(),
            PlugSelected = false,
            PlugStyle = DestinySocketCategoryStyle.Reusable
        };

        return PlugItem;
    }

    private void CreateSocketCategories()
    {
        if (ApiItem.Item.TagData.Unk70.GetValue(ApiItem.Item.GetReader()) is D2Class_C0778080 sockets)
        {
            List<SocketCategoryItem> socketCategories = new();
            List<SocketEntryItem> socketEntries = new();

            foreach (var socket in sockets.IntrinsicSockets)
            {
                if (socket.PlugItemIndex == -1)
                    continue;

                List<PlugItem> plugItems = new();

                var type = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                var category = Investment.Get().SocketCategoryStringThings[type.SocketCategoryIndex];
                SocketCategoryItem socketCategory = new SocketCategoryItem
                {
                    Hash = category.SocketCategoryHash,
                    Name = category.SocketName.Value.ToString().ToUpper(),
                    Description = category.SocketDescription.Value.ToString(),
                    UICategoryStyle = (DestinySocketCategoryStyle)category.CategoryStyle,
                    SocketCategoryIndex = type.SocketCategoryIndex
                };
                if (!socketCategories.Any(x => x.Hash == category.SocketCategoryHash))
                    socketCategories.Add(socketCategory);

                SocketTypeItem socketType = new SocketTypeItem
                {
                    Hash = type.SocketTypeHash,
                    SocketCategory = socketCategory,
                    PlugCategoryWhitelist = type.PlugWhitelists.Select(x => x.PlugCategoryHash).ToList()
                };

                var plugItem = CreatePlugItem(socket.PlugItemIndex);
                if (plugItem is not null)
                {
                    plugItem.PlugStyle = socketCategory.UICategoryStyle;
                    plugItems.Add(plugItem);
                }

                SocketEntryItem socketEntry = new SocketEntryItem
                {
                    SocketType = socketType,
                    //SingleInitialItem = CreatePlugItem(socket.SingleInitialItemIndex),
                    PlugItems = plugItems.DistinctBy(x => x.Hash).ToList()
                };
                socketEntries.Add(socketEntry);
            }

            for (int i = 0; i < sockets.SocketEntries.Count; i++)
            {
                var socket = sockets.SocketEntries[i];
                if (socket.SocketTypeIndex == -1)
                    continue;

                List<PlugItem> plugItems = new();
                var type = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                if (type.SocketVisiblity == 1) // Hidden
                    continue;

                var category = Investment.Get().SocketCategoryStringThings[type.SocketCategoryIndex];
                SocketCategoryItem socketCategory = new SocketCategoryItem
                {
                    Hash = category.SocketCategoryHash,
                    Name = category.SocketName.Value.ToString().ToUpper(),
                    Description = category.SocketDescription.Value.ToString(),
                    UICategoryStyle = (DestinySocketCategoryStyle)category.CategoryStyle,
                    SocketCategoryIndex = type.SocketCategoryIndex
                };
                if (!socketCategories.Any(x => x.Hash == category.SocketCategoryHash))
                    socketCategories.Add(socketCategory);

                SocketTypeItem socketType = new SocketTypeItem
                {
                    Hash = type.SocketTypeHash,
                    SocketCategory = socketCategory,
                    PlugCategoryWhitelist = type.PlugWhitelists.Select(x => x.PlugCategoryHash).ToList()
                };

                foreach (var index in new short[] { socket.ReusablePlugSetIndex1, socket.ReusablePlugSetIndex2 })
                {
                    if (index != -1)
                    {
                        foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(index))
                        {
                            if (randomPlugs.PlugInventoryItemIndex == -1)
                                continue;

                            var plugItem = CreatePlugItem(randomPlugs.PlugInventoryItemIndex);
                            if (plugItem is not null)
                            {
                                plugItem.PlugOrderIndex = i;
                                plugItem.PlugStyle = socketCategory.UICategoryStyle;
                                plugItems.Add(plugItem);
                            }
                        }
                    }
                }

                foreach (var plug in socket.PlugItems)
                {
                    if (plug.PlugInventoryItemIndex == -1)
                        continue;

                    var plugItem = CreatePlugItem(plug.PlugInventoryItemIndex);
                    if (plugItem is not null)
                    {
                        plugItem.PlugOrderIndex = i;
                        plugItem.PlugStyle = socketCategory.UICategoryStyle;
                        plugItems.Add(plugItem);
                    }
                }

                if (socket.SingleInitialItemIndex != -1)
                {
                    var plugItem = CreatePlugItem(socket.SingleInitialItemIndex);
                    if (plugItem != null)
                    {
                        plugItem.PlugOrderIndex = i;
                        plugItem.PlugStyle = socketCategory.UICategoryStyle;
                        // Things like default shader/ornament, empty sockets, etc are single intial items and will always be first
                        plugItems.Insert(0, plugItem);
                        // Remove the last occurence (if needed) of said item since its gonna be first anyways
                        var lastOccurrenceIndex = plugItems.LastIndexOf(plugItem);
                        if (lastOccurrenceIndex != 0)
                            plugItems.RemoveAt(lastOccurrenceIndex);
                    }
                }

                SocketEntryItem socketEntry = new SocketEntryItem
                {
                    SocketType = socketType,
                    //SingleInitialItem = CreatePlugItem(socket.SingleInitialItemIndex),
                    PlugItems = plugItems.DistinctBy(x => x.Hash).ToList()
                };
                socketEntries.Add(socketEntry);
            }

            foreach (var socketCategory in socketCategories.OrderBy(x => x.SocketCategoryIndex))
            {
                if (socketCategory.Name == string.Empty && socketCategory.Description == string.Empty)
                    continue;

                string style = "Reusable";
                switch (socketCategory.UICategoryStyle)
                {
                    case DestinySocketCategoryStyle.Unknown:
                    case DestinySocketCategoryStyle.Supers: // Dont need
                    case DestinySocketCategoryStyle.Abilities: // Dont need
                    case DestinySocketCategoryStyle.Unlockable: // TODO? Looks same as Reusable?
                        style = "Reusable";
                        break;
                    case DestinySocketCategoryStyle.EnergyMeter:
                        style = socketCategory.UICategoryStyle.ToString();
                        socketCategory.SocketRarityColor = new SolidColorBrush(ApiItem.ItemRarityColor);
                        break;
                    default:
                        style = socketCategory.UICategoryStyle.ToString();
                        break;
                }

                var categoryTemplate = (DataTemplate)FindResource($"{style}Template");
                FrameworkElement content = (FrameworkElement)categoryTemplate.LoadContent();
                var contentStackPanel = content.FindName($"{style}Panel") as StackPanel;
                content.DataContext = socketCategory;

                foreach (var socketEntry in socketEntries.Where(x => x.SocketType.SocketCategory.Hash == socketCategory.Hash))
                {
                    if (socketEntry.PlugItems.Count == 0 || socketCategory.UICategoryStyle == DestinySocketCategoryStyle.EnergyMeter)
                        continue;

                    ListBox listBox = new ListBox();
                    var template = (DataTemplate)FindResource($"{style}ItemTemplate");
                    listBox.ItemTemplate = template;
                    listBox.ItemsSource = socketEntry.PlugItems; //socketEntry.PlugItems.Where(x => socketEntry.SocketType.PlugCategoryWhitelist.Contains(x.PlugCategoryHash)); not needed? idk

                    contentStackPanel.Children.Add(listBox);
                }

                SocketContainer.Children.Add(content);
            }
        }
    }

    private void GetItemStats()
    {
        if (ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader()) is D2Class_81738080 stats)
        {
            var statGroup = Investment.Get().GetStatGroup(ApiItem.Item);

            if (statGroup is not null)
            {
                foreach (var scaledStat in statGroup.Value.ScaledStats)
                {
                    var statItem = Investment.Get().StatStrings[scaledStat.StatIndex];

                    int statValue = stats.InvestmentStats.Where(x => x.StatTypeIndex == scaledStat.StatIndex).FirstOrDefault().Value;
                    int displayValue = MakeDisplayValue(scaledStat.StatIndex, statValue);

                    var displayStat = new StatItem
                    {
                        Hash = statItem.StatHash,
                        Name = statItem.StatName.Value.ToString(),
                        Description = statItem.StatDescription.Value.ToString(),
                        StatDisplayValue = displayValue,
                        StatValue = statValue,
                        StatDisplayNumeric = scaledStat.DisplayAsNumeric == 1,
                        StatIsLinear = scaledStat.IsLinear == 1
                    };
                    _statItems.Add(displayStat);
                    //Console.WriteLine($"{displayStat.Name} ({displayStat.Description}) : {displayStat.StatDisplayValue} ({displayStat.StatValue})");
                }
            }
        }

        // this is dumbbbbb
        ItemStatsControl.ItemsSource = _statItems.Where(x => !x.StatDisplayNumeric);
        ItemNumericStatsControl.ItemsSource = _statItems.Where(x => x.StatDisplayNumeric);

        // For some unknown unholy reason, this breaks showing the lore tab if Visibility.Collapsed?????
        //StatsContainer.Visibility = _statItems.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
        StatsContainer.Opacity = _statItems.Count != 0 ? 1 : 0;
    }

    private int MakeDisplayValue(int statIndex, int statValue)
    {
        if (ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader()) is D2Class_81738080 investmentStats)
        {
            var statGroup = Investment.Get().GetStatGroup(ApiItem.Item);
            if (!statGroup.HasValue || statGroup is null)
                return statValue;

            var stat = statGroup.Value.ScaledStats.FirstOrDefault(x => x.StatIndex == statIndex);
            if (statValue < 0 || stat.DisplayInterpolation is null)
                return statValue;

            if (stat.DisplayInterpolation.Where(x => x.Value == statValue).Any())
            {
                return stat.DisplayInterpolation.First(x => x.Value == statValue).Weight;
            }
            else if (stat.IsLinear == 1)
            {
                return statValue;
            }
            else // value is likely between 2 values in DisplayInterpolation
            {
                int? lowerKey = null;
                int? upperKey = null;

                // Get all keys
                var keys = stat.DisplayInterpolation;

                // Find the keys that are just below and above the targetKey
                for (int i = 0; i < keys.Count - 1; i++)
                {
                    if (keys[i].Value <= statValue && keys[i + 1].Value >= statValue)
                    {
                        lowerKey = keys[i].Value;
                        upperKey = keys[i + 1].Value;
                        break;
                    }
                }

                if (lowerKey != null && upperKey != null)
                {
                    var lowerValue = keys.First(x => x.Value == lowerKey).Weight;
                    var upperValue = keys.First(x => x.Value == upperKey).Weight;

                    // Interpolate median value between lower and upper values
                    var interpolatedMedian = Interpolate(lowerKey.Value, lowerValue, upperKey.Value, upperValue, statValue);
                    return (int)Math.Round(interpolatedMedian);
                }
            }
        }
        return 0;
    }

    // Function to interpolate between two values
    private float Interpolate(int x0, float y0, int x1, float y1, int x)
    {
        return y0 + (y1 - y0) * (x - x0) / (x1 - x0);
    }

    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.ActiveItem = (sender as Button);
        PlugItem item = (PlugItem)(sender as Button).DataContext;

        if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            foreach (var stat in stats.InvestmentStats)
            {
                var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
                var adjustValue = MakeDisplayValue(stat.StatTypeIndex, stat.Value);

                if (statItem.StatName.Value is not null)
                {
                    var _stat = _statItems.FirstOrDefault(x => x.Hash == statItem.StatHash);
                    if (_stat is null)
                        continue;
                    _stat.StatAdjustValue = adjustValue;
                }
            }
        }

        ToolTip.MakeTooltip(item);
    }

    private void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.ClearTooltip();
        ToolTip.ActiveItem = null;

        PlugItem item = (PlugItem)(sender as Button).DataContext;
        if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            foreach (var stat in stats.InvestmentStats)
            {
                var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];

                if (statItem.StatName.Value is not null)
                {
                    var _stat = _statItems.FirstOrDefault(x => x.Hash == statItem.StatHash);
                    if (_stat is null)
                        continue;
                    _stat.StatAdjustValue = 0;
                }
            }
        }
    }

    private void PlugItem_OnClick(object sender, RoutedEventArgs e)
    {
        PlugItem item = (PlugItem)(sender as Button).DataContext;
#if DEBUG
        Console.WriteLine($"{item.Name} {item.Item.Hash} ({item.PlugRarity})");
#endif

        if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            foreach (var stat in stats.InvestmentStats)
            {
                var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
                if (statItem.StatName.Value is null)
                    continue;

                //var mainStat = ((D2Class_81738080)ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader())).InvestmentStats.FirstOrDefault(x => x.StatTypeIndex == stat.StatTypeIndex);
                //#if DEBUG
                //  Console.WriteLine($"{statItem.StatName.Value.ToString()} : {stat.Value} ({MakeDisplayValue(stat.StatTypeIndex, stat.Value)}) (perk) + {mainStat.Value} ({MakeDisplayValue(mainStat.StatTypeIndex, mainStat.Value)}) (main) = {stat.Value + mainStat.Value}");
                //#endif
                if (!item.PlugSelected)
                {
                    //TODO: Can only have one perk selected in each row, clear any added stats from current selected perk
                    var statToChange = _statItems.FirstOrDefault(x => x.Hash == statItem.StatHash);
                    if (statToChange is null)
                        continue;
                    statToChange.StatDisplayValue += MakeDisplayValue(stat.StatTypeIndex, stat.Value);
                }
            }
        }

        if (item.Type.ToLower().Contains("ornament") || item.Name.ToLower() == "default ornament")
        {
            var hash = item.Item.TagData.InventoryItemHash.Hash32;
            if (item.Name.ToLower().Contains("default"))
                hash = ApiItem.Item.TagData.InventoryItemHash.Hash32;

            try
            {
                ImageBrush imageBrush = new ImageBrush();
                imageBrush.ImageSource = new BitmapImage(new Uri($"https://www.bungie.net/common/destiny2_content/screenshots/{hash}.jpg"));
                ItemBackground.Background = imageBrush;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get background for {item.Item.Hash}: {ex.Message}");
            }
        }

        item.PlugSelected = !item.PlugSelected;
    }

    private void StatItem_MouseEnter(object sender, MouseEventArgs e)
    {
        ToolTip.InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        ToolTip.InfoBox.BeginStoryboard(fadeIn);
        ActiveStatItemGrid = (sender as Grid);
        ToolTip.ActiveItem = ActiveStatItemGrid;

        var stat = (StatItem)ActiveStatItemGrid.DataContext;
        PlugItem statItem = new PlugItem
        {
            Hash = stat.Hash,
            Name = stat.Name.ToUpper(),
            PlugStyle = DestinySocketCategoryStyle.Reusable,
            Description = stat.Description,
        };

        ToolTip.MakeTooltip(statItem);
        //ToolTip.InfoBox.DataContext = statItem;
        //ToolTip.AddToTooltip(statItem, APITooltip.TooltipType.TextBlock);
    }

    private void StatItem_MouseLeave(object sender, MouseEventArgs e)
    {
        ToolTip.InfoBoxStackPanel.Children.Clear();
        ToolTip.InfoBox.Visibility = Visibility.Collapsed;
        ActiveStatItemGrid = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = position.X * -0.01;
        gridTransform.Y = position.Y * -0.01;
    }

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.A && ApiItem.ItemLore != null)
        {
            if (LoreEntry.Visibility != Visibility.Visible)
            {
                // Fade in LoreEntry
                LoreEntry.Visibility = Visibility.Visible;
                DoubleAnimation fadeInAnimation = new DoubleAnimation();
                fadeInAnimation.From = 0;
                fadeInAnimation.To = 1;
                fadeInAnimation.Duration = TimeSpan.FromSeconds(0.1);
                LoreEntry.BeginAnimation(OpacityProperty, fadeInAnimation);

                // Apply blur effect and fade it in
                if (MainContainer.Effect == null || !(MainContainer.Effect is BlurEffect))
                {
                    MainContainer.Effect = new BlurEffect { Radius = 0 };
                    BackgroundContainer.Effect = new BlurEffect { Radius = 0 };
                }

                DoubleAnimation blurAnimation = new DoubleAnimation();
                blurAnimation.From = 0;
                blurAnimation.To = 20;
                blurAnimation.Duration = TimeSpan.FromSeconds(0.1);
                (MainContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
                (BackgroundContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            }
            else
            {
                // Fade out LoreEntry
                DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                fadeOutAnimation.From = 1;
                fadeOutAnimation.To = 0;
                fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.1);
                fadeOutAnimation.Completed += (s, args) => LoreEntry.Visibility = Visibility.Collapsed;
                LoreEntry.BeginAnimation(OpacityProperty, fadeOutAnimation);

                // Fade out blur effect
                if (MainContainer.Effect != null && MainContainer.Effect is BlurEffect)
                {
                    DoubleAnimation blurAnimation = new DoubleAnimation();
                    blurAnimation.From = 20;
                    blurAnimation.To = 0;
                    blurAnimation.Duration = TimeSpan.FromSeconds(0.1);
                    blurAnimation.Completed += (s, args) =>
                    {
                        MainContainer.Effect = null; // Remove blur effect after fading out
                        BackgroundContainer.Effect = null;
                    };

                    (MainContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
                    (BackgroundContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
                }
            }
        }
    }


    public class ApiItemView
    {
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public string ItemFlavorText { get; set; }
        public string ItemLore { get; set; }
        public DestinyTierType ItemRarity => (DestinyTierType)Item.TagData.ItemRarity;
        public Color ItemRarityColor => ItemRarity.GetColor();
        public string ItemHash { get; set; }
        public string ItemSource { get; set; }
        public string ItemDamageType { get; set; }
        public int ItemPowerCap { get; set; }

        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public ImageSource ImageSource { get; set; }
        public ImageSource FoundryIconSource { get; set; }

        public ImageBrush GridBackground { get; set; }

        public InventoryItem Item { get; set; }
        public InventoryItem Parent { get; set; }
    }

    public class PlugItem : INotifyPropertyChanged
    {
        public InventoryItem Item { get; set; }
        public TigerHash Hash { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Description { get; set; } = "";

        public int PlugSocketIndex { get; set; }
        public TigerHash PlugCategoryHash { get; set; }
        public int PlugOrderIndex { get; set; }
        public ImageSource PlugImageSource { get; set; }
        public ImageSource PlugWatermark { get; set; }
        public DestinyTierType PlugRarity { get; set; } = DestinyTierType.Common;
        public Color PlugRarityColor { get; set; }
        public DestinyDamageTypeEnum PlugDamageType { get; set; }
        public DestinySocketCategoryStyle PlugStyle { get; set; }
        public bool HasControls { get; set; } = false;

        private bool _plugSelected;
        public bool PlugSelected
        {
            get { return _plugSelected; }
            set
            {
                _plugSelected = value;
                OnPropertyChanged(nameof(PlugSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StatItem : INotifyPropertyChanged
    {
        public TigerHash Hash { get; set; }
        public string Name { get; set; }
        public string Type { get; set; } = "";
        public string Description { get; set; }

        public bool StatDisplayNumeric { get; set; }
        public bool StatIsLinear { get; set; }

        private int _statValue;
        public int StatValue
        {
            get { return _statValue; }
            set
            {
                _statValue = value;
                OnPropertyChanged(nameof(StatValue));
            }
        }

        private int _statDisplayValue;
        public int StatDisplayValue
        {
            get { return _statDisplayValue; }
            set
            {
                _statDisplayValue = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SocketEntryItem
    {
        public SocketTypeItem SocketType { get; set; }
        public PlugItem SingleInitialItem { get; set; } // Probably not needed as its usually defined in the reusable/random set anyways
        public List<PlugItem> PlugItems { get; set; }
    }

    public class SocketTypeItem
    {
        public TigerHash Hash;
        public SocketCategoryItem SocketCategory { get; set; }
        public List<TigerHash> PlugCategoryWhitelist { get; set; }
        // TODO?: visibility, overridesUiAppearance
    }

    public class SocketCategoryItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public TigerHash Hash { get; set; }
        public DestinySocketCategoryStyle UICategoryStyle { get; set; }
        public int SocketCategoryIndex { get; set; }
        public SolidColorBrush SocketRarityColor { get; set; } // Only used for EnergyMeter
    }

    public class EmblemItem
    {
        public ImageSource EmblemLarge { get; set; }
        public ImageSource EmblemMedium { get; set; }
        public ImageSource EmblemSmall { get; set; }
    }
}

public static class ApiImageUtils
{
    public static BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = ms;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.DecodePixelWidth = width;
        bitmapImage.DecodePixelHeight = height;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    public static Dictionary<DrawingImage, ImageBrush> MakeIcon(InventoryItem item)
    {
        Dictionary<DrawingImage, ImageBrush> icon = new();
        string? type = Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)).TagData.ItemType.Value;
        if (type is null)
            type = "";

        // streams
        var bgStream = item.GetIconBackgroundStream();
        var bgOverlayStream = item.GetIconBackgroundOverlayStream();
        var primaryStream = item.GetIconPrimaryStream();
        var overlayStream = item.GetIconOverlayStream();

        //sometimes only the primary icon is valid
        var primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;

        // Icon dyes
        if (bgOverlayStream != null && Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
            primary = MakeDyedIcon(item);

        var bg = bgStream != null ? MakeBitmapImage(bgStream, 96, 96) : null;

        //Most if not all legendary armor will use the ornament overlay because of transmog (I assume)
        var bgOverlay = bgOverlayStream != null && type.Contains("Ornament") ? MakeBitmapImage(bgOverlayStream, 96, 96) : null;
        var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;

        var group = new DrawingGroup();
        group.Children.Add(new ImageDrawing(bg, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(bgOverlay, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));
        group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, 96, 96)));

        var dw = new DrawingImage(group);
        dw.Freeze();

        ImageBrush brush = new ImageBrush(bg);
        brush.Freeze();

        icon.TryAdd(dw, brush);

        return icon;
    }

    public static DrawingImage MakeFoundryBanner(InventoryItem item)
    {
        var foundryStream = item.GetFoundryIconStream();
        var foundry = foundryStream != null ? MakeBitmapImage(foundryStream, 596, 596) : null;

        var group = new DrawingGroup();
        group.Children.Add(new ImageDrawing(foundry, new Rect(0, 0, 596, 596)));

        var dw = new DrawingImage(group);
        dw.Freeze();

        return dw;
    }

    public static ImageSource GetPlugWatermark(InventoryItem item)
    {
        var overlayStream = item.GetIconOverlayStream(1);
        var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;
        var dw = new ImageBrush(overlay);
        dw.Freeze();
        return dw.ImageSource;
    }

    public static ImageSource MakeIcon(int index)
    {
        var container = Investment.Get().GetItemIconContainer(index);
        if (container == null || container.TagData.IconPrimaryContainer == null)
            return null;

        var primaryStream = GetTexture(container.TagData.IconPrimaryContainer).GetTexture();
        var primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;

        var dw = new ImageBrush(primary);
        dw.Freeze();

        return dw.ImageSource;
    }

    public static ImageSource MakeFullIcon(int index, int containerIndex = 0, int iconIndex = 0, int listIndex = 0)
    {
        var container = Investment.Get().GetItemIconContainer(index);
        if (container == null)
            return null;

        List<Tag<D2Class_CF3E8080>> containers = new()
        {
            container.TagData.IconPrimaryContainer,
            container.TagData.IconAdContainer,
            container.TagData.IconBGOverlayContainer,
            container.TagData.IconBackgroundContainer,
            container.TagData.IconOverlayContainer,
            container.TagData.IconSpecialContainer
        };
        if (containers[containerIndex] is null)
            return null;

        var texture = GetTexture(containers[containerIndex], iconIndex, listIndex);
        var primaryStream = texture.GetTexture();
        var primary = primaryStream != null ? MakeBitmapImage(primaryStream, texture.TagData.Width, texture.TagData.Height) : null;

        var dw = new ImageBrush(primary);
        dw.Freeze();

        return dw.ImageSource;
    }

    private static Texture? GetTexture(Tag<D2Class_CF3E8080> iconContainer, int texIndex = 0, int listIndex = 0)
    {
        using TigerReader reader = iconContainer.GetReader();
        dynamic? prim = iconContainer.TagData.Unk10.GetValue(reader);
        if (prim is D2Class_CD3E8080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (texIndex >= structCD3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;
            return structCD3E8080.Unk00[reader, listIndex].TextureList[reader, texIndex].IconTexture;
        }
        if (prim is D2Class_CB3E8080 structCB3E8080)
        {
            if (texIndex >= structCB3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;
            return structCB3E8080.Unk00[reader, listIndex].TextureList[reader, texIndex].IconTexture;
        }
        return null;
    }

    public static BitmapImage MakeDyedIcon(InventoryItem item)
    {
        var iconContainer = Investment.Get().GetItemIconContainer(item);
        var primaryStream = item.GetIconPrimaryStream();
        var maskStream = item.GetIconBackgroundOverlayStream();

        Bitmap mainImage = primaryStream != null ? MakeBitmap(primaryStream) : null;
        Bitmap colorMaskImage = maskStream != null ? MakeBitmap(maskStream) : null;
        if (mainImage is null || colorMaskImage is null)
            return Bitmap2BitmapImage(mainImage, 96, 96);

        // both mask and main have to be the same size
        if (iconContainer.TagData.IconBGOverlayContainer is not null && (GetTexture(iconContainer.TagData.IconBGOverlayContainer).TagData.Height < GetTexture(iconContainer.TagData.IconPrimaryContainer).TagData.Height))
            colorMaskImage = MakeBitmap(maskStream, GetTexture(iconContainer.TagData.IconPrimaryContainer).TagData.Height);

        // Define RGB colors
        System.Drawing.Color[] overlayColors = new System.Drawing.Color[]
        {
            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorR.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.Z, 0.5) * 255)),   // Red channel overlay color

            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorG.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.Z, 0.5) * 255)),   // Green channel overlay color

            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorB.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.Z, 0.5) * 255))    // Blue channel overlay color
        };


        // Apply color from color mask
        int width = mainImage.Width;
        int height = mainImage.Height;

        // Iterate over each pixel in the color mask and apply color to the main image
        for (int y = 0; y < height; y++)
        {
            //Console.WriteLine($"H {y} : {height}");
            for (int x = 0; x < width; x++)
            {
                // Get color mask pixel color
                System.Drawing.Color maskColor = colorMaskImage.GetPixel(x, y);

                // Get main image pixel color
                System.Drawing.Color mainColor = mainImage.GetPixel(x, y);
                System.Drawing.Color blendedColor = System.Drawing.Color.FromArgb(mainColor.A, 0, 0, 0);

                // Mask R
                blendedColor = ColorUtility.BlendColors(mainColor, overlayColors[0], maskColor.R);
                // Mask G
                blendedColor = ColorUtility.AddColors(blendedColor, ColorUtility.BlendColors(mainColor, overlayColors[1], maskColor.G));
                // Mask B
                blendedColor = ColorUtility.AddColors(blendedColor, ColorUtility.BlendColors(mainColor, overlayColors[2], maskColor.B));

                // Set the modified pixel color
                if (!blendedColor.IsZero())
                    mainImage.SetPixel(x, y, blendedColor);
            }
        }

        return Bitmap2BitmapImage(mainImage, 96, 96);
    }

    private static Bitmap MakeBitmap(UnmanagedMemoryStream stream, int wH = 0)
    {
        using (var memoryStream = new System.IO.MemoryStream())
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(stream));
            encoder.Save(memoryStream);

            if (wH == 0)
                return new Bitmap(memoryStream);
            else
            {
                Bitmap originalBitmap = new Bitmap(memoryStream);
                Bitmap resizedBitmap = new Bitmap(wH, wH);

                using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                {
                    graphics.DrawImage(originalBitmap, 0, 0, wH, wH);
                }
                return resizedBitmap;
            }
        }
    }

    public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap, int width, int height)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // Save bitmap to memory stream as PNG (to preserve alpha channel)
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            // Create new BitmapImage and load it from memory stream
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.DecodePixelWidth = width;
            bitmapImage.DecodePixelHeight = height;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze the BitmapImage to make it immutable

            return bitmapImage;
        }
    }
}

// ugh
public class PercentageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (int.TryParse(value?.ToString(), out int intValue) && int.TryParse(parameter?.ToString(), out int totalWidth))
            return ((float)intValue / 100f) * (float)totalWidth;

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
            return new Thickness((adjustmentValue / 100f) * 210, 0, 0, 0);

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
        if (value is int adjustmentValue && adjustmentValue < 0 && int.TryParse(parameter?.ToString(), out int totalWidth))
            return ((adjustmentValue * -1) / 100f) * (float)totalWidth;

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

