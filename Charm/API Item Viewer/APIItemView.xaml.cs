using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Arithmic;
using Tiger.Schema.Investment;

namespace Charm;

/// <summary>
/// Interaction logic for APIItemView.xaml
/// </summary>
public partial class APIItemView : UserControl
{
    private ApiItemView ApiItem;
    private List<PerkItem> _perkItems;
    Button? ActivePerkItemButton;

    public APIItemView(InventoryItem item)
    {
        InitializeComponent();
        _perkItems = new();
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
        _perkItems = new();

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

            Console.WriteLine($"https://www.bungie.net/common/destiny2_content/screenshots/{ApiItem.Item.TagData.InventoryItemHash.Hash32}.jpg");
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

        // socket type : plug entry, plugs
        ConcurrentDictionary<int, ConcurrentDictionary<int, List<int>>> SocketPlugs = new ConcurrentDictionary<int, ConcurrentDictionary<int, List<int>>>();

        if (ApiItem.Item.TagData.Unk70.GetValue(ApiItem.Item.GetReader()) is D2Class_C0778080 sockets)
        {
            for (int i = 0; i < sockets.SocketEntries.Count; i++)
            {
                var socket = sockets.SocketEntries[i];
                System.Console.WriteLine($"SocketTypeIndex: {socket.SocketTypeIndex} "
                    + (socket.SocketTypeIndex != -1 ? Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketName.Value
                    + $" ({Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)} : {Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketCategoryHash})" : ""));

                //System.Console.WriteLine($"SingleInitialItemIndex: {socket.SingleInitialItemIndex} " + (socket.SingleInitialItemIndex != -1 ?
                //    Investment.Get().GetItemName(Investment.Get().GetInventoryItem(socket.SingleInitialItemIndex)) + $" ({Investment.Get().GetInventoryItem(socket.SingleInitialItemIndex).Hash})" : ""));

                //System.Console.WriteLine($"ReusablePlugSetIndex1: {socket.ReusablePlugSetIndex1}");
                if (socket.SocketTypeIndex == -1)
                    continue;

                var socketCategoryIndex = Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex);
                if (socket.ReusablePlugSetIndex1 != -1)
                {
                    foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
                    {
                        if (randomPlugs.PlugInventoryItemIndex == -1)
                            continue;

                        if (!SocketPlugs.ContainsKey(socketCategoryIndex))
                            SocketPlugs[socketCategoryIndex] = new();
                        if (!SocketPlugs[socketCategoryIndex].ContainsKey(i))
                            SocketPlugs[socketCategoryIndex][i] = new();

                        SocketPlugs[socketCategoryIndex][i].Add(randomPlugs.PlugInventoryItemIndex);
                    }
                }

                if (socket.ReusablePlugSetIndex2 != -1)
                {
                    foreach (var randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex2))
                    {
                        if (randomPlugs.PlugInventoryItemIndex == -1)
                            continue;

                        if (!SocketPlugs.ContainsKey(socketCategoryIndex))
                            SocketPlugs[socketCategoryIndex] = new();
                        if (!SocketPlugs[socketCategoryIndex].ContainsKey(i))
                            SocketPlugs[socketCategoryIndex][i] = new();

                        SocketPlugs[socketCategoryIndex][i].Add(randomPlugs.PlugInventoryItemIndex);
                    }
                }

                foreach (var plug in socket.PlugItems)
                {
                    if (plug.PlugInventoryItemIndex == -1)
                        continue;

                    if (!SocketPlugs.ContainsKey(socketCategoryIndex))
                        SocketPlugs[socketCategoryIndex] = new();
                    if (!SocketPlugs[socketCategoryIndex].ContainsKey(i))
                        SocketPlugs[socketCategoryIndex][i] = new();

                    SocketPlugs[socketCategoryIndex][i].Add(plug.PlugInventoryItemIndex);
                }
                //System.Console.WriteLine($"-------------------------");
            }
        }

        foreach (var (socket, plugs) in SocketPlugs)
        {
            Console.WriteLine($"{Investment.Get().SocketCategoryStringThings[socket].SocketName.Value.ToString()} ({socket}, {Investment.Get().SocketCategoryStringThings[socket].SocketCategoryHash})");
            switch (socket)
            {
                case 13 or 16: // Armor/Weapon Perks
                    var perkrows = new List<List<PerkItem>>();
                    foreach (var plugSet in plugs)
                    {
                        Console.WriteLine($"weapon perk {plugSet.Key}");
                        var row = new List<PerkItem>();
                        foreach (var plug in plugSet.Value)
                        {
                            var perkItem = CreateApiItem(Investment.Get().GetInventoryItem(plug));
                            if (!row.Contains(perkItem))
                                row.Add(perkItem);
                        }
                        row = new List<PerkItem>(row.GroupBy(x => x.PerkItemHash).Select(group => group.First()));
                        perkrows.Add(row);
                    }
                    PerkItemsControl.ItemsSource = perkrows;
                    break;
                case 11 or 17: // Armor/Weapon Mods
                    var modrows = new List<List<PerkItem>>();
                    foreach (var plugSet in plugs)
                    {
                        Console.WriteLine($"weapon mod {plugSet.Key}");
                        var row = new List<PerkItem>();
                        foreach (var plug in plugSet.Value)
                        {
                            var perkItem = CreateApiItem(Investment.Get().GetInventoryItem(plug));
                            if (!row.Contains(perkItem))
                                row.Add(perkItem);
                        }
                        row = new List<PerkItem>(row.GroupBy(x => x.PerkItemHash).Select(group => group.First()));
                        modrows.Add(row);
                    }
                    ModItemsControl.ItemsSource = modrows;
                    break;
                case 18: // Intrinsic Traits
                    var intrinsicrows = new List<List<PerkItem>>();
                    foreach (var plugSet in plugs)
                    {
                        Console.WriteLine($"intrinsic {plugSet.Key}");
                        var row = new List<PerkItem>();
                        foreach (var plug in plugSet.Value)
                        {
                            var perkItem = CreateApiItem(Investment.Get().GetInventoryItem(plug));
                            if (!row.Contains(perkItem))
                                row.Add(perkItem);
                        }
                        row = new List<PerkItem>(row.GroupBy(x => x.PerkItemHash).Select(group => group.First()));
                        intrinsicrows.Add(row);
                    }
                    IntrinsicTraitsControl.ItemsSource = intrinsicrows;
                    break;
            }
            //rows.Clear();
        }
    }

    private PerkItem CreateApiItem(InventoryItem item)
    {
        var icon = MakeIcon(item);
        var type = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemType.Value.ToString();
        var description = Investment.Get().InventoryItemStringThings[Investment.Get().GetItemIndex(item.TagData.InventoryItemHash)].TagData.ItemDisplaySource.Value.ToString();

        if (type == "Weapon Mod" && item.TagData.Unk78.GetValue(item.GetReader()) is D2Class_81738080)
            description = Investment.Get().SandboxPerkStrings[((D2Class_81738080)item.TagData.Unk78.GetValue(item.GetReader())).Perks[0].StatTypeIndex].SandboxPerkDescription.Value.ToString();

        PerkItem perkItem = new PerkItem
        {
            Item = item,
            PerkItemHash = item.TagData.InventoryItemHash.Hash32,
            PerkItemName = Investment.Get().GetItemName(item).ToUpper(),
            PerkItemType = type,
            PerkItemDescription = description,
            PerkImageSource = icon.Keys.First(),
        };

        return perkItem;
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

    private void PerkItem_MouseEnter(object sender, MouseEventArgs e)
    {
        PerkInfoBox.Visibility = Visibility.Visible;
        PerkInfoBox.Visibility = Visibility.Visible;

        ActivePerkItemButton = (sender as Button);

        var fadeIn = FindResource("FadeIn 0.2") as Storyboard;
        PerkInfoBox.BeginStoryboard(fadeIn);
    }

    private void PerkItem_MouseLeave(object sender, MouseEventArgs e)
    {
        PerkInfoBox.Visibility = Visibility.Collapsed;
        ActivePerkItemButton = null;
    }

    private void PerkItem_OnClick(object sender, RoutedEventArgs e)
    {
        PerkItem item = (PerkItem)(sender as Button).DataContext;

        Console.WriteLine($"{item.PerkItemName}");
        Console.WriteLine($"Item {item.Item.Hash}");
        Console.WriteLine($"Strings {Investment.Get().GetItemStringThing(item.Item.TagData.InventoryItemHash).Hash}");
        Console.WriteLine($"Icons {Investment.Get().GetItemIconContainer(item.Item.TagData.InventoryItemHash).Hash}");
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (PerkInfoBox.Visibility == Visibility.Visible && ActivePerkItemButton != null)
        {
            float xOffset = 13;
            float yOffset = 12;

            Point position = e.GetPosition(Application.Current.MainWindow);
            // this is stupid
            if (position.X >= Application.Current.MainWindow.RenderSize.Width / 2)
                xOffset = 16f;

            TranslateTransform transform = (TranslateTransform)PerkInfoBox.RenderTransform;
            transform.X = position.X - 725;
            transform.Y = position.Y - 550;

            PerkInfoBox.DataContext = ActivePerkItemButton.DataContext;
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
        public System.Windows.Media.ImageSource ImageSource { get; set; }
        public System.Windows.Media.ImageSource FoundryIconSource { get; set; }

        public System.Windows.Media.ImageBrush GridBackground { get; set; }

        public InventoryItem Item { get; set; }
        public InventoryItem Parent { get; set; }
    }

    public class PerkItem
    {
        public string PerkItemName { get; set; }
        public string PerkItemType { get; set; }
        public string PerkItemDescription { get; set; }
        public uint PerkItemHash { get; set; }
        public System.Windows.Media.ImageSource PerkImageSource { get; set; }
        public InventoryItem Item { get; set; }
    }
}
