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

        Console.WriteLine($"{item.TagData.InventoryItemHash}");
        string name = Investment.Get().GetItemName(item);
        string? type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value;
        if (type == null)
            type = "";

        var foundryBanner = MakeFoundryBanner(item);
        var image = MakeIcon(item);
        ApiItem = new ApiItemView
        {
            Item = item,
            ItemName = name.ToUpper(),
            ItemType = type.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStringThing(item.TagData.InventoryItemHash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(item.TagData.InventoryItemHash)?.LoreDescription.Value.ToString(),
            ItemRarity = (ItemTier)item.TagData.ItemRarity,
            ImageSource = image.Keys.First(),
            FoundryIconSource = foundryBanner
        };
        Load();
    }

    public APIItemView(ApiItem apiItem)
    {
        InitializeComponent();

        var hash = apiItem.Item.TagData.InventoryItemHash;
        var foundryBanner = MakeFoundryBanner(apiItem.Item);
        ApiItem = new ApiItemView
        {
            Item = apiItem.Item,
            ItemName = apiItem.ItemName.ToUpper(),
            ItemType = apiItem.ItemType.ToUpper(),
            ItemFlavorText = Investment.Get().GetItemStringThing(hash).TagData.ItemFlavourText.Value.ToString(),
            ItemLore = Investment.Get().GetItemLore(hash)?.LoreDescription.Value.ToString(),
            ItemRarity = apiItem.ItemRarity,
            ImageSource = apiItem.ImageSource,
            FoundryIconSource = foundryBanner
        };
        Load();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine($"Item {ApiItem.Item.Hash}");
        Console.WriteLine($"Strings {Investment.Get().GetItemStringThing(ApiItem.Item.TagData.InventoryItemHash).Hash}");
        Console.WriteLine($"Icons {Investment.Get().GetItemIconContainer(ApiItem.Item.TagData.InventoryItemHash).Hash}");
        if (ApiItem.ItemLore != null)
            LoreEntry.DataContext = ApiItem;

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
        DataContext = this;
        MouseMove += UserControl_MouseMove;
        KeyDown += UserControl_KeyDown;
        MainContainer.DataContext = ApiItem;
        _statItems = new();
        _plugItems = new();

        CreatePlugItems();
        GetItemStats();
    }

    private PlugItem? CreatePlugItem(int plugIndex, int socketIndex)
    {
        var item = Investment.Get().GetInventoryItem(plugIndex);
        var icon = MakeIcon(item);
        var name = Investment.Get().GetItemName(item).ToUpper();
        var type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value.ToString();
        var description = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemDisplaySource.Value.ToString();
        if (name == "" && type == "" && description == "")
            return null;

        var socketName = Investment.Get().SocketCategoryStringThings[socketIndex].SocketName.Value.ToString();
        if (socketName.ToLower().Contains("mods") && item.TagData.Unk78.GetValue(item.GetReader()) is D2Class_81738080 stats && stats.Perks.Count > 0)
            description = Investment.Get().SandboxPerkStrings[stats.Perks[0].StatTypeIndex].SandboxPerkDescription.Value.ToString();

        PlugItem PlugItem = new PlugItem
        {
            Item = item,
            Hash = item.TagData.InventoryItemHash,
            Name = name,
            Type = type,
            Description = description,
            PlugImageSource = icon.Keys.First(),
            PlugSocketIndex = socketIndex,
            PlugSocketName = socketName,
            PlugSelected = false
        };

        return PlugItem;
    }

    private void CreatePlugItems()
    {
        if (ApiItem.Item.TagData.Unk70.GetValue(ApiItem.Item.GetReader()) is D2Class_C0778080 sockets)
        {
            for (int i = 0; i < sockets.SocketEntries.Count; i++)
            {
                var socket = sockets.SocketEntries[i];
                System.Console.WriteLine($"SocketTypeIndex: {socket.SocketTypeIndex} "
                    + (socket.SocketTypeIndex != -1 ? Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketName.Value
                    + $" ({Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)} : {Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketCategoryHash})" : ""));

                if (socket.SocketTypeIndex == -1)
                    continue;

                var socketCategoryIndex = Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex);
                if (socket.ReusablePlugSetIndex1 != -1)
                {
                    foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
                    {
                        if (randomPlugs.PlugInventoryItemIndex == -1)
                            continue;

                        var plugItem = CreatePlugItem(randomPlugs.PlugInventoryItemIndex, socketCategoryIndex);
                        if (plugItem is not null)
                        {
                            plugItem.PlugOrderIndex = i;
                            _plugItems.Add(plugItem);
                        }
                    }
                }

                if (socket.ReusablePlugSetIndex2 != -1)
                {
                    foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex2))
                    {
                        if (randomPlugs.PlugInventoryItemIndex == -1)
                            continue;

                        var plugItem = CreatePlugItem(randomPlugs.PlugInventoryItemIndex, socketCategoryIndex);
                        if (plugItem is not null)
                        {
                            plugItem.PlugOrderIndex = i;
                            _plugItems.Add(plugItem);
                        }
                    }
                }

                foreach (var plug in socket.PlugItems)
                {
                    if (plug.PlugInventoryItemIndex == -1)
                        continue;

                    var plugItem = CreatePlugItem(plug.PlugInventoryItemIndex, socketCategoryIndex);
                    if (plugItem is not null)
                    {
                        plugItem.PlugOrderIndex = i;
                        _plugItems.Add(plugItem);
                    }
                }

                var socketName = Investment.Get().SocketCategoryStringThings[socketCategoryIndex].SocketName.Value.ToString();
                // Perks
                var perks = _plugItems.Where(x => x.PlugSocketIndex == socketCategoryIndex
                && x.PlugOrderIndex == i && x.PlugSocketName.ToLower().Contains("perks"));
                if (perks.Count() != 0)
                {
                    ListBox listBoxPerks = new ListBox();
                    listBoxPerks.ItemsSource = perks;
                    listBoxPerks.ItemTemplate = (DataTemplate)FindResource("PlugItemTemplate");
                    PerksPanel.Children.Add(listBoxPerks);
                    Perks.Visibility = Visibility.Visible;
                    PerksTextBlock.Text = socketName;
                }

                // Mods
                var mods = _plugItems.Where(x => x.PlugSocketIndex == socketCategoryIndex
                && x.PlugOrderIndex == i && x.PlugSocketName.ToLower().Contains("mods"));
                if (mods.Count() != 0)
                {
                    ListBox listBoxMods = new ListBox();
                    listBoxMods.ItemsSource = mods;

                    listBoxMods.ItemTemplate = (DataTemplate)FindResource("ModItemTemplate");
                    ModsPanel.Children.Add(listBoxMods);
                    Mods.Visibility = Visibility.Visible;
                    ModsTextBlock.Text = socketName;
                }

                // Intrinsics
                var intrinsics = _plugItems.Where(x => x.PlugSocketIndex == socketCategoryIndex
                && x.PlugOrderIndex == i && x.PlugSocketName.ToLower().Contains("intrinsic"));
                if (intrinsics.Count() != 0)
                {
                    ListBox listBoxIntrinsics = new ListBox();
                    listBoxIntrinsics.ItemsSource = intrinsics;

                    listBoxIntrinsics.ItemTemplate = (DataTemplate)FindResource("IntrinsicItemTemplate");
                    IntrinsicsPanel.Children.Add(listBoxIntrinsics);
                    Intrinsics.Visibility = Visibility.Visible;
                    IntrinsicTextBlock.Text = socketName;
                }
            }


            foreach (var plug in _plugItems)
            {
                var socketName = Investment.Get().SocketCategoryStringThings[plug.PlugSocketIndex].SocketName.Value;
                if (socketName == "WEAPON PERKS")
                {
                    Console.WriteLine($"{socketName}: {plug.Name}: {plug.Type}, {plug.PlugOrderIndex}");
                }
            }
        }
    }

    private void GetItemStats()
    {
        if (ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader()) is D2Class_81738080 stats)
        {
            var statGroup = Investment.Get().GetStatGroup(ApiItem.Item);
            if (statGroup is null)
                return;

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

        // this is dumbbbbb
        ItemStatsControl.ItemsSource = _statItems.Where(x => !x.StatDisplayNumeric);
        ItemNumericStatsControl.ItemsSource = _statItems.Where(x => x.StatDisplayNumeric);
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

    private BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
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

    private void PlugItem_MouseEnter(object sender, MouseEventArgs e)
    {
        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);

        ActivePlugItemButton = (sender as Button);

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
    }

    private void PlugItem_MouseLeave(object sender, MouseEventArgs e)
    {
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

        Console.WriteLine($"{item.Name}");
        Console.WriteLine($"Item {item.Item.Hash}");
        Console.WriteLine($"Strings {Investment.Get().GetItemStringThing(item.Item.TagData.InventoryItemHash).Hash}");
        Console.WriteLine($"Icons {Investment.Get().GetItemIconContainer(item.Item.TagData.InventoryItemHash).Hash}");

        if (item.Item.TagData.Unk78.GetValue(item.Item.GetReader()) is D2Class_81738080 stats)
        {
            foreach (var stat in stats.InvestmentStats)
            {
                var statItem = Investment.Get().StatStrings[stat.StatTypeIndex];
                if (statItem.StatName.Value is null)
                    continue;

                var mainStat = ((D2Class_81738080)ApiItem.Item.TagData.Unk78.GetValue(ApiItem.Item.GetReader())).InvestmentStats.FirstOrDefault(x => x.StatTypeIndex == stat.StatTypeIndex);
                Console.WriteLine($"{statItem.StatName.Value.ToString()} : {stat.Value} (perk) + {mainStat.Value} (main) = {stat.Value + mainStat.Value}");

                if (!item.PlugSelected)
                {
                    //TODO: Can only have one perk selected in each row, clear any added stats from current selected perk

                    var statToChange = _statItems.FirstOrDefault(x => x.Hash == statItem.StatHash);
                    if (statToChange is null)
                        continue;
                    Console.WriteLine(MakeDisplayValue(stat.StatTypeIndex, stat.Value));
                    statToChange.StatDisplayValue += MakeDisplayValue(stat.StatTypeIndex, stat.Value);
                }
            }
            item.PlugSelected = !item.PlugSelected;
        }
    }

    private void StatItem_MouseEnter(object sender, MouseEventArgs e)
    {
        InfoBox.Visibility = Visibility.Visible;
        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        InfoBox.BeginStoryboard(fadeIn);

        ActiveStatItemGrid = (sender as Grid);
    }

    private void StatItem_MouseLeave(object sender, MouseEventArgs e)
    {
        InfoBox.Visibility = Visibility.Collapsed;
        ActiveStatItemGrid = null;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (InfoBox.Visibility == Visibility.Visible && (ActivePlugItemButton != null || ActiveStatItemGrid != null))
        {
            float xOffset = 12;
            float yOffset = 100;

            Point position = e.GetPosition(Application.Current.MainWindow);
            // this is stupid
            if (position.X >= Application.Current.MainWindow.RenderSize.Width / 2)
                xOffset = 16f;

            TranslateTransform transform = (TranslateTransform)InfoBox.RenderTransform;
            transform.X = position.X - xOffset - InfoBox.ActualWidth / 2;
            transform.Y = position.Y - yOffset - InfoBox.ActualHeight / 2;

            InfoBox.DataContext = ActiveStatItemGrid != null ? ActiveStatItemGrid.DataContext : ActivePlugItemButton.DataContext;
        }
    }

    private void UserControl_KeyDown(object sender, KeyEventArgs e)
    {
        if (ApiItem.ItemLore is null)
            return;

        if (e.Key == Key.A)
        {
            if (LoreEntry.Visibility != Visibility.Visible)
            {
                // Fade in LoreEntry
                LoreEntry.Visibility = Visibility.Visible;
                DoubleAnimation fadeInAnimation = new DoubleAnimation();
                fadeInAnimation.From = 0;
                fadeInAnimation.To = 1;
                fadeInAnimation.Duration = TimeSpan.FromSeconds(0.3);
                LoreEntry.BeginAnimation(OpacityProperty, fadeInAnimation);

                // Apply blur effect and fade it in
                if (MainContainer.Effect == null || !(MainContainer.Effect is BlurEffect))
                    MainContainer.Effect = new BlurEffect { Radius = 0 };
                DoubleAnimation blurAnimation = new DoubleAnimation();
                blurAnimation.From = 0;
                blurAnimation.To = 20;
                blurAnimation.Duration = TimeSpan.FromSeconds(0.2);
                (MainContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
            }
            else
            {
                // Fade out LoreEntry
                DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                fadeOutAnimation.From = 1;
                fadeOutAnimation.To = 0;
                fadeOutAnimation.Duration = TimeSpan.FromSeconds(0.3);
                fadeOutAnimation.Completed += (s, args) => LoreEntry.Visibility = Visibility.Collapsed;
                LoreEntry.BeginAnimation(OpacityProperty, fadeOutAnimation);

                // Fade out blur effect
                if (MainContainer.Effect != null && MainContainer.Effect is BlurEffect)
                {
                    DoubleAnimation blurAnimation = new DoubleAnimation();
                    blurAnimation.From = 20;
                    blurAnimation.To = 0;
                    blurAnimation.Duration = TimeSpan.FromSeconds(0.2);
                    blurAnimation.Completed += (s, args) => MainContainer.Effect = null; // Remove blur effect after fading out
                    (MainContainer.Effect as BlurEffect).BeginAnimation(BlurEffect.RadiusProperty, blurAnimation);
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
        public ItemTier ItemRarity { get; set; }
        public string ItemHash { get; set; }
        public double ImageWidth { get; set; }
        public double ImageHeight { get; set; }
        public ImageSource ImageSource { get; set; }
        public ImageSource FoundryIconSource { get; set; }

        public ImageBrush GridBackground { get; set; }

        public InventoryItem Item { get; set; }
        public InventoryItem Parent { get; set; }
    }

    public class PlugItem
    {
        public TigerHash Hash { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public string PlugSocketName { get; set; }
        public int PlugSocketIndex { get; set; }
        public int PlugOrderIndex { get; set; }
        public bool PlugSelected { get; set; }

        public ImageSource PlugImageSource { get; set; }
        public InventoryItem Item { get; set; }
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

