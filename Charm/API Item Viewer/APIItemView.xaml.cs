using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Tiger.Schema.Investment;

namespace Charm;

// This code is garbaaaaage
public partial class APIItemView : UserControl
{
    private ApiItemView ApiItem;
    private ObservableCollection<PlugItem> _plugItems;
    private ObservableCollection<StatItem> _statItems;
    Button? ActivePlugItemButton;
    Grid? ActiveStatItemGrid;

    public APIItemView(InventoryItem item)
    {
        InitializeComponent();
        string name = Investment.Get().GetItemName(item);
        string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
        if (type == null)
            type = "";

        var source = Investment.Get().GetCollectible(Investment.Get().GetItemIndex(item.TagData.InventoryItemHash));
        var sourceString = source != null ? source.Value.SourceName.Value.ToString() : "";

        var foundryBanner = MakeFoundryBanner(item);
        var image = MakeIcon(item);
        ApiItem = new ApiItemView
        {
            Item = item,
            ItemName = name.ToUpper(),
            ItemType = type.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStringThing(item.TagData.InventoryItemHash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(item.TagData.InventoryItemHash)?.LoreDescription.Value.ToString(),
            ItemSource = sourceString,
            ImageSource = image.Keys.First(),
            FoundryIconSource = foundryBanner,
            ItemDamageType = ((DestinyDamageType)item.GetItemDamageTypeIndex()).GetEnumDescription()
        };
        Load();
    }

    public APIItemView(ApiItem apiItem)
    {
        InitializeComponent();

        var source = Investment.Get().GetCollectible(Investment.Get().GetItemIndex(apiItem.Item.TagData.InventoryItemHash));
        var sourceString = source != null ? source.Value.SourceName.Value.ToString() : "";

        var hash = apiItem.Item.TagData.InventoryItemHash;
        var foundryBanner = MakeFoundryBanner(apiItem.Item);
        ApiItem = new ApiItemView
        {
            Item = apiItem.Item,
            ItemName = apiItem.ItemName.ToUpper(),
            ItemType = apiItem.ItemType.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStringThing(hash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(hash)?.LoreDescription.Value.ToString(),
            ItemSource = sourceString,
            ImageSource = apiItem.ImageSource,
            FoundryIconSource = foundryBanner,
            ItemDamageType = ((DestinyDamageType)apiItem.Item.GetItemDamageTypeIndex()).GetEnumDescription()
        };
        Load();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine($"Item {ApiItem.Item.Hash}");
        Console.WriteLine($"Strings {Investment.Get().GetItemStringThing(ApiItem.Item.TagData.InventoryItemHash).Hash}");
        Console.WriteLine($"Icons {Investment.Get().GetItemIconContainer(ApiItem.Item.TagData.InventoryItemHash).Hash}");
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
        ItemRarityBanner.DataContext = ApiItem;
        _statItems = new();
        _plugItems = new();

        CreateSocketCategories();
        GetItemStats();
    }

    private PlugItem? CreatePlugItem(int plugIndex)
    {
        if (plugIndex == -1)
            return null;

        var item = Investment.Get().GetInventoryItem(plugIndex);
        var icon = MakeIcon(item);
        var name = Investment.Get().GetItemName(item).ToUpper();
        var type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value.ToString();
        var description = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemDisplaySource.Value.ToString();
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
            PlugWatermark = GetPlugWatermark(item),
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
            for (int i = 0; i < sockets.SocketEntries.Count; i++)
            {
                var socket = sockets.SocketEntries[i];
                if (socket.SocketTypeIndex == -1)
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
                    case DestinySocketCategoryStyle.Reusable:
                    case DestinySocketCategoryStyle.EnergyMeter: // TODO
                    case DestinySocketCategoryStyle.Supers: // TODO
                    case DestinySocketCategoryStyle.Abilities: // TODO
                    case DestinySocketCategoryStyle.Unlockable: // TODO?
                        style = "Reusable";
                        break;
                    case DestinySocketCategoryStyle.Consumable:
                        style = "Consumable";
                        break;
                    case DestinySocketCategoryStyle.LargePerk:
                        style = "LargePerk";
                        break;
                }

                var categoryTemplate = (DataTemplate)FindResource($"{style}Template");
                FrameworkElement content = (FrameworkElement)categoryTemplate.LoadContent();
                var contentStackPanel = content.FindName($"{style}Panel") as StackPanel;
                content.DataContext = socketCategory;

                foreach (var socketEntry in socketEntries.Where(x => x.SocketType.SocketCategory.Hash == socketCategory.Hash))
                {
                    if (socketEntry.PlugItems.Count == 0)
                        continue;

                    ListBox listBox = new ListBox();
                    var template = (DataTemplate)FindResource($"{style}ItemTemplate");
                    listBox.ItemTemplate = template;
                    listBox.ItemsSource = socketEntry.PlugItems.Where(x => socketEntry.SocketType.PlugCategoryWhitelist.Contains(x.PlugCategoryHash));

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
                    //Console.WriteLine($"{displayStat.StatName} ({displayStat.StatDescription}) : {displayStat.StatDisplayValue} ({displayStat.StatValue})");
                }
            }
        }

        // this is dumbbbbb
        ItemStatsControl.ItemsSource = _statItems.Where(x => !x.StatDisplayNumeric);
        ItemNumericStatsControl.ItemsSource = _statItems.Where(x => x.StatDisplayNumeric);
        StatsContainer.Visibility = _statItems.Count != 0 ? Visibility.Visible : Visibility.Collapsed;
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

    private static BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
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

    public Dictionary<DrawingImage, ImageBrush> MakeIcon(InventoryItem item)
    {
        Dictionary<DrawingImage, ImageBrush> icon = new();
        string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
        if (type is null)
            type = "";

        // streams
        var bgStream = item.GetIconBackgroundStream();
        var bgOverlayStream = item.GetIconBackgroundOverlayStream();
        var primaryStream = item.GetIconPrimaryStream();
        var overlayStream = item.GetIconOverlayStream();

        //sometimes only the primary icon is valid
        var primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;
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

    public DrawingImage MakeFoundryBanner(InventoryItem item)
    {
        var foundryStream = item.GetFoundryIconStream();
        var foundry = foundryStream != null ? MakeBitmapImage(foundryStream, 596, 596) : null;

        var group = new DrawingGroup();
        group.Children.Add(new ImageDrawing(foundry, new Rect(0, 0, 596, 596)));

        var dw = new DrawingImage(group);
        dw.Freeze();

        return dw;
    }

    public ImageSource GetPlugWatermark(InventoryItem item)
    {
        var overlayStream = item.GetIconOverlayStream(1);
        var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;
        var dw = new ImageBrush(overlay);
        dw.Freeze();
        return dw.ImageSource;
    }

    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);

        ActivePlugItemButton = (sender as Button);
        InfoBox.DataContext = ActivePlugItemButton.DataContext;

        PlugItem item = (PlugItem)(sender as Button).DataContext;
        if (item.Description != "")
            AddToInfoBox(item, InfoBoxType.TextBlock);

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
            if (item.PlugStyle == DestinySocketCategoryStyle.Reusable)
                return;

            foreach (var perk in stats.Perks)
            {
                var perkStrings = Investment.Get().SandboxPerkStrings[perk.PerkIndex];
                if (perkStrings.IconIndex == -1)
                    continue;

                // Icon
                var overlayStream = item.Item.GetIconPrimaryStream(perkStrings.IconIndex);
                var overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;
                var dw = new ImageBrush(overlay);
                dw.Freeze();

                PlugItem perkItem = new PlugItem
                {
                    Hash = perkStrings.SandboxPerkHash,
                    Description = perkStrings.SandboxPerkDescription.Value,
                    PlugImageSource = dw.ImageSource
                };

                AddToInfoBox(perkItem, InfoBoxType.Grid);
            }
        }

        foreach (var notif in Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.Item.TagData.InventoryItemHash)].TagData.TooltipNotifications)
        {
            PlugItem notifItem = new PlugItem
            {
                Description = notif.DisplayString.Value,
                PlugImageSource = null
            };

            AddToInfoBox(notifItem, InfoBoxType.InfoBlock);
        }
    }

    private void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
        InfoBoxStackPanel.Children.Clear();
        InfoBox.Visibility = Visibility.Collapsed;
        ActivePlugItemButton = null;

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

        Console.WriteLine($"{item.Name} {item.Item.Hash} ({item.PlugRarity})");

        if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            foreach (var stat in stats.InvestmentStats)
            {
                var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
                if (statItem.StatName.Value is null)
                    continue;

                var mainStat = ((D2Class_81738080)ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader())).InvestmentStats.FirstOrDefault(x => x.StatTypeIndex == stat.StatTypeIndex);
                Console.WriteLine($"{statItem.StatName.Value.ToString()} : {stat.Value} ({MakeDisplayValue(stat.StatTypeIndex, stat.Value)}) (perk) + {mainStat.Value} ({MakeDisplayValue(mainStat.StatTypeIndex, mainStat.Value)}) (main) = {stat.Value + mainStat.Value}");

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
        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);
        ActiveStatItemGrid = (sender as Grid);

        var stat = (StatItem)ActiveStatItemGrid.DataContext;
        PlugItem statItem = new PlugItem
        {
            Hash = stat.Hash,
            Name = stat.Name,
            PlugStyle = DestinySocketCategoryStyle.Reusable,
            Description = stat.Description,
        };
        InfoBox.DataContext = statItem;
        AddToInfoBox(statItem, InfoBoxType.TextBlock);
    }

    private void StatItem_MouseLeave(object sender, MouseEventArgs e)
    {
        InfoBoxStackPanel.Children.Clear();
        InfoBox.Visibility = Visibility.Collapsed;
        ActiveStatItemGrid = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        Point position = e.GetPosition(Application.Current.MainWindow);
        if (InfoBox.Visibility == Visibility.Visible && (ActivePlugItemButton != null || ActiveStatItemGrid != null))
        {
            float xOffset = 25;
            float yOffset = 10;

            // this is stupid
            if (position.X >= Application.Current.MainWindow.RenderSize.Width / 2)
                xOffset = (-1 * xOffset) - (float)InfoBox.Width;

            TranslateTransform infoBoxtransform = (TranslateTransform)InfoBox.RenderTransform;
            infoBoxtransform.X = position.X + xOffset;
            infoBoxtransform.Y = position.Y + yOffset - Application.Current.MainWindow.RenderSize.Height;
        }

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

    private void AddToInfoBox(PlugItem item, InfoBoxType type)
    {
        switch (type)
        {
            case InfoBoxType.TextBlock:
                DataTemplate infoTextTemplate = (DataTemplate)FindResource("InfoBoxTextTemplate");
                FrameworkElement textUI = (FrameworkElement)infoTextTemplate.LoadContent();
                textUI.DataContext = item;
                InfoBoxStackPanel.Children.Add(textUI);
                break;
            case InfoBoxType.Grid:
                DataTemplate infoGridTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement gridUi = (FrameworkElement)infoGridTemplate.LoadContent();
                gridUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(gridUi);
                break;
            case InfoBoxType.InfoBlock:
                DataTemplate infoBlockSepTemplate = (DataTemplate)FindResource("InfoBoxSeperatorTemplate");
                FrameworkElement infoBlockSepUi = (FrameworkElement)infoBlockSepTemplate.LoadContent();
                InfoBoxStackPanel.Children.Add(infoBlockSepUi);

                DataTemplate infoBlockTemplate = (DataTemplate)FindResource("InfoBoxGridTemplate");
                FrameworkElement infoBlockUi = (FrameworkElement)infoBlockTemplate.LoadContent();
                infoBlockUi.DataContext = item;
                InfoBoxStackPanel.Children.Add(infoBlockUi);
                break;
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

        public DestinySocketCategoryStyle PlugStyle { get; set; }

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
    }

    private enum InfoBoxType
    {
        InfoBlock,
        TextBlock,
        Grid
    }
}

// https://bungie-net.github.io/multi/schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition.html#schema_Destiny-Definitions-Sockets-DestinySocketCategoryDefinition
// https://bungie-net.github.io/multi/schema_Destiny-DestinySocketCategoryStyle.html#schema_Destiny-DestinySocketCategoryStyle
public enum DestinySocketCategoryStyle : uint
{
    Unknown = 0, // 0
    Reusable = 2656457638, // 1
    Consumable = 1469714392, // 2
                             // where Intrinsic? Replaced with LargePerk? // 4
    Unlockable = 1762428417, // 3
    EnergyMeter = 750616615, // 5
    LargePerk = 2251952357, // 6
    Abilities = 1901312945, // 7
    Supers = 497024337, // 8
}

// TODO: Find where these indexes actually go?
public enum DestinyDamageType : int
{
    [Description("Kinetic")]
    Kinetic = 1319,
    [Description(" Arc")]
    Arc = 1320,
    [Description(" Solar")]
    Solar = 1321,
    [Description(" Void")]
    Void = 1322,
    [Description(" Stasis")]
    Stasis = 1323,
    [Description(" Strand")]
    Strand = 1324
}

public enum DestinyTierType
{
    Unknown = -1,
    Currency = 0,
    Common = 1, // Basic
    Uncommon = 2, // Common
    Rare = 3,
    Legendary = 4, // Superior
    Exotic = 5,
}

public static class DestinyTierTypeColor
{
    private static readonly Dictionary<DestinyTierType, Color> Colors = new Dictionary<DestinyTierType, Color>
    {
        { DestinyTierType.Unknown, Color.FromArgb(255, 66, 66, 66) },
        { DestinyTierType.Currency, Color.FromArgb(255, 195, 188, 180) },
        { DestinyTierType.Common, Color.FromArgb(255, 66, 66, 66) },
        { DestinyTierType.Uncommon, Color.FromArgb(255, 55, 113, 67) },
        { DestinyTierType.Rare, Color.FromArgb(255, 80, 118, 164) },
        { DestinyTierType.Legendary, Color.FromArgb(255, 82, 47, 100) },
        { DestinyTierType.Exotic, Color.FromArgb(255, 206, 174, 51) }
    };

    public static Color GetColor(this DestinyTierType tierType)
    {
        if (Colors.ContainsKey(tierType))
            return Colors[tierType];
        else
            throw new ArgumentException("Invalid DestinyTierType");
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

public class TransparentColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color && double.TryParse(parameter?.ToString(), out double alphaFactor))
        {
            byte alpha = (byte)(color.A * alphaFactor);
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InfoBoxColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color && float.TryParse(parameter?.ToString(), out float brightnessFactor))
        {
            // hacky 'fix'
            if (brightnessFactor == 0.5f)
                return new SolidColorBrush(Color.FromArgb(230, color.R, color.G, color.B));

            System.Drawing.Color col = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            System.Drawing.Color newColor = ColorUtility.GenerateShades(col, 1, brightnessFactor).First();
            return new SolidColorBrush(Color.FromArgb(230, newColor.R, newColor.G, newColor.B));
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

