using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Charm.Collections;
using MaterialDesignColors.ColorManipulation;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Investment;

namespace Charm;

public partial class CollectionsView : UserControl
{
    private static MainWindow _mainWindow = null;

    private DynamicArray<SDB788080> PresentationNodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S07588080> PresentationNodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    public CollectionsView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        MouseMove += UserControl_MouseMove;
    }

    public async void LoadContent()
    {
        LoadCollectibles();
        LoadBadges();
        LoadRecords();
        LoadMisc();
    }

    public void LoadCollectibles()
    {
        List<Category> _buttons = new();

        int totalItemAmount = 0;
        var nodes = PresentationNodes;
        var strings = PresentationNodeStrings;

        var presNodes = nodes.Find(x => x.Hash.Hash32 == 3790247699).PresentationNodes;

        foreach (var node in presNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

            Category itemCategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C23280 : 0x80E64A0B)), // inactive
                Tag = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C23282 : 0x80E64A0D)), // active
                ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1),
                ItemCategoryIcon3 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 0),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex),
                CategoryBannerColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x93, 0x82, 0x4F)),
                ScreenStyle = curNodeStrings.ScreenStyle,
                DisplayStyle = curNodeStrings.DisplayStyle
            };
            totalItemAmount += itemCategory.ItemCategoryAmount;

            _buttons.Add(itemCategory);
        }

        ItemsTextTab.Text = UIHelper.AddSpacesBetweenChars($"ITEMS - {totalItemAmount}", 2);
        CollectiblesList.Items = _buttons;

        DataContext = this;
    }

    public void LoadBadges()
    {
        List<Category> _buttons = new();

        int totalItemAmount = 0;
        var nodes = PresentationNodes;
        var strings = PresentationNodeStrings;

        var presNodes = nodes.Find(x => x.Hash.Hash32 == 498211331).PresentationNodes;

        foreach (var node in presNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];
            Tag<SB83E8080>? container = Investment.Get().GetItemIconContainer(curNodeStrings.IconIndex);

            Category itemCategory = new()
            {
                ItemCategoryIndex = node.PresentationNodeIndex,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex),
                ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 2),
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex),
                CategoryBannerColor = UIHelper.Vec4ToBrush(container.TagData.DyeColorR),
                ScreenStyle = curNodeStrings.ScreenStyle,
                DisplayStyle = curNodeStrings.DisplayStyle,
            };
            totalItemAmount++;

            _buttons.Add(itemCategory);
        }

        BadgesTextTab.Text = UIHelper.AddSpacesBetweenChars($"BADGES - {totalItemAmount}", 2);
        BadgesList.ItemTemplate = (DataTemplate)FindResource("BadgeItemButton");

        BadgesList.Items = _buttons;
    }

    public void LoadRecords()
    {
        List<Category> _buttons = new();

        var nodes = PresentationNodes;
        var strings = PresentationNodeStrings;

        var presNodes = nodes.Find(x => x.Hash.Hash32 == 1163735237).PresentationNodes;

        foreach (var node in presNodes)
        {
            var curNode = nodes[node.PresentationNodeIndex];
            var curNodeStrings = strings[node.PresentationNodeIndex];

            if (curNode.Hash.Hash32 == 511607103) // Just Exotic catalysts, not patterns & catalysts
                continue;

            Category itemCategory = new()
            {
                ItemCategoryHash = curNode.Hash.Hash32,
                ItemCategoryIndex = curNode.PresentationNodes[0].PresentationNodeIndex,
                ItemCategoryIcon2 = curNodeStrings.IconIndex != -1 ? ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 2) : null,
                ItemCategoryName = curNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = curNodeStrings.Description.Value,
                ItemCategoryAmount = GetItemCategoryAmount(node.PresentationNodeIndex),
                ScreenStyle = curNodeStrings.ScreenStyle,
                DisplayStyle = curNodeStrings.DisplayStyle,
            };

            switch (curNode.Hash.Hash32) // I don't like hardcoding these hashes but they havent changed in awhile (ofc they changed in EOF)
            {
                case 3901403713: // Medals
                    itemCategory.ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C2328A : 0x80E64A15));
                    itemCategory.Order = 0;
                    break;
                case 1993337477: // Lore
                    itemCategory.ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C23293 : 0x80E64A1E));
                    itemCategory.ItemCategoryIcon2 = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1);
                    itemCategory.Order = 2;
                    break;
                case 1866538467: // Triumphs
                    itemCategory.ItemCategoryIndex = node.PresentationNodeIndex;
                    itemCategory.ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C2327D : 0x80E64A08));
                    itemCategory.ItemCategoryIcon2 = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232CD : 0x80E64A56));
                    itemCategory.Order = 3;
                    break;
                default:
                    itemCategory.ItemCategoryIcon = curNodeStrings.IconIndex != -1 ? ApiImageUtils.MakeIcon(curNodeStrings.IconIndex) : null;
                    break;
            }

            _buttons.Add(itemCategory);
        }

        var patterns = nodes.Find(x => x.Hash.Hash32 == 2642502414).PresentationNodes[0];
        var patternsNode = nodes[patterns.PresentationNodeIndex];
        var patternsNodeStrings = strings[patterns.PresentationNodeIndex];
        Category patternsCategory = new()
        {
            ItemCategoryHash = 2642502414,
            ItemCategoryIndex = patterns.PresentationNodeIndex,
            ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C2328F : 0x80E64A1A)),
            ItemCategoryIcon2 = patternsNodeStrings.IconIndex != -1 ? ApiImageUtils.MakeIcon(patternsNodeStrings.IconIndex, 0, 2) : null,
            ItemCategoryName = patternsNodeStrings.Name.Value.ToString().ToUpper(),
            ItemCategoryDescription = patternsNodeStrings.Description.Value,
            ItemCategoryAmount = GetItemCategoryAmount(patterns.PresentationNodeIndex),
            ScreenStyle = patternsNodeStrings.ScreenStyle,
            DisplayStyle = patternsNodeStrings.DisplayStyle,
            Order = 1
        };
        _buttons.Add(patternsCategory);

        RecordsList.ItemTemplate = (DataTemplate)FindResource("RecordItemButton");
        RecordsList.ItemsPerPage = 4;
        RecordsList.Columns = 2;
        RecordsList.Items = _buttons.OrderBy(x => x.Order);
    }

    public void LoadMisc()
    {
        int iconIndex = -1;

        List<Category> _buttons = new();

        var seasonsMap = Investment.Get()._seasonDefinitionMap.TagData.SeasonDefinitions;
        var seasonsStringMap = Investment.Get()._seasonDefinitionStringMap.TagData.SeasonDefinitionStrings;

        var nodes = PresentationNodes;
        var strings = PresentationNodeStrings;

        // Seasonal Challenges

        var index = nodes.FindIndex(x => x.Hash.Hash32 == 3443694067);
        var challengesNode = nodes[index];
        var challengesNodeStrings = strings[index];

        if (challengesNode.PresentationNodes.Count != 0)
        {
            iconIndex = strings[challengesNode.PresentationNodes[0].PresentationNodeIndex].IconIndex;
            Category challengesCategory = new()
            {
                ItemCategoryHash = 3443694067,
                ItemCategoryIndex = index,
                ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80A9FA4F : 0x80A78315)),
                ItemCategoryIcon2 = challengesNodeStrings.IconIndex != -1 ? ApiImageUtils.MakeIcon(iconIndex, 0, 0, 0) : null,
                ItemCategoryIcon3 = challengesNodeStrings.IconIndex != -1 ? ApiImageUtils.MakeIcon(iconIndex, 0, 0, 0) : null,
                ItemCategoryName = challengesNodeStrings.Name.Value.ToString().ToUpper(),
                ItemCategoryDescription = challengesNodeStrings.Description.Value,
                ItemCategoryAmount = Investment.Get().GetObjectiveValue(challengesNode.ObjectiveIndex),
                ScreenStyle = challengesNodeStrings.ScreenStyle,
                DisplayStyle = challengesNodeStrings.DisplayStyle,
                Tag = new RibbonCategoryTheme()
            };

            var curSeason = seasonsStringMap[seasonsMap.Count - 2]; //seasonsStringMap.FindLast(x => x.SeasonName.Value != null);
            var container = Investment.Get()._unkStyleContainer1.TagData.Entries[curSeason.Unk34].Container.TagData.Container.TagData.Unk08.Find(x => x.Unk00.Hash32 == 2959035011);

            Texture? texture = ApiImageUtils.GetTexture(container.Container);
            challengesCategory.ItemCategoryBackground = ApiImageUtils.MakeIcon(texture.Hash);
            challengesCategory.Tag.RibbonBackgroundImage = ApiImageUtils.MakeIcon(texture.Hash);
            challengesCategory.Tag.DropShadowOpacity = 0.5f;
            _buttons.Add(challengesCategory);

        }


        // Seals

        index = nodes.FindIndex(x => x.Hash.Hash32 == 616318467);
        var sealsNode = nodes[index];
        var sealsNodeStrings = strings[index];

        iconIndex = strings[sealsNode.PresentationNodes[0].PresentationNodeIndex].IconIndex;
        Category sealsCategory = new()
        {
            ItemCategoryHash = 616318467,
            ItemCategoryIndex = index,
            ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80A9FA4F : 0x80A78315)),
            ItemCategoryIcon2 = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232BE : 0x80E64A49)),
            ItemCategoryIcon3 = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C232BE : 0x80E64A49)),
            ItemCategoryName = sealsNodeStrings.Name.Value.ToString().ToUpper(),
            ItemCategoryDescription = sealsNodeStrings.Description.Value,
            ItemCategoryAmount = sealsNode.PresentationNodes.Count + nodes.Find(x => x.Hash.Hash32 == 1881970629).PresentationNodes.Count, // Regular + legacy titles
            ScreenStyle = sealsNodeStrings.ScreenStyle,
            DisplayStyle = sealsNodeStrings.DisplayStyle,
            Tag = new RibbonCategoryTheme()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xC8, 0xBA, 0x85)), // rich gold background
                Border = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xEC, 0xA9)), // muted antique gold
                RibbonBackground = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xE5, 0x86)), // lighter highlight gold
                RibbonBackgroundImage = MakeLayeredBackground(index),
                RibbonBackgroundMargin = new(0, 0, 0, 0),
                DropShadowOpacity = 1f
            }
        };
        _buttons.Add(sealsCategory);

        // quests

        var traitDef = Investment.Get().GetTrait(DestinyTraitID.item_quest_current_release).Value;
        var colVec = Investment.Get().GetItemIconContainer(traitDef.IconIndex).TagData.DyeColorR;
        var col = Color.FromScRgb(colVec.W, colVec.X, colVec.Y, colVec.Z);
        Category quests = new()
        {
            ItemCategoryHash = uint.MaxValue,
            ItemCategoryIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80A9FA4F : 0x80A78315)),
            ItemCategoryIcon3 = ApiImageUtils.MakeIcon(traitDef.IconIndex),
            ItemCategoryName = "QUESTS",
            ItemCategoryDescription = "",

            Tag = new RibbonCategoryTheme()
            {
                Background = new SolidColorBrush(col),
                Border = new SolidColorBrush(col.Lighten(1)),
                RibbonBackground = new SolidColorBrush(col.ShiftLightness(-1.25f)),
                RibbonBackgroundImage = MakeLayeredBackground(),
                RibbonBackgroundMargin = new(15),
                DropShadowOpacity = 0f
            }
        };
        _buttons.Add(quests);

        MiscList.Items = _buttons;
    }

    // For titles background
    private DrawingImage MakeLayeredBackground(int presNodeIndex)
    {
        var group = new DrawingGroup();
        int i = 0;

        var nodes = PresentationNodes[presNodeIndex].PresentationNodes;
        foreach (var title in nodes)
        {
            Tag<SB83E8080>? container = Investment.Get().GetItemIconContainer(PresentationNodeStrings[title.PresentationNodeIndex].IconIndex);
            Texture? texture = ApiImageUtils.GetTexture(container.TagData.IconPrimaryContainer, 1, 0);
            UnmanagedMemoryStream? primaryStream = texture?.GetTexture();
            BitmapImage? primary = primaryStream != null ? ApiImageUtils.MakeBitmapImage(primaryStream, 200, 200) : null;
            group.Children.Add(new ImageDrawing(primary, new Rect(i, 0, 100, 100)));
            i += 55;
            if (i > 250)
                break;
        }

        var dw = new DrawingImage(group);
        dw.Freeze();
        return dw;
    }

    // for quests
    private DrawingImage MakeLayeredBackground()
    {
        List<DestinyTraitID> TraitCategories = new()
            {
                DestinyTraitID.item_quest_past,
                DestinyTraitID.item_quest_playlists,
                DestinyTraitID.item_quest_exotic,
                DestinyTraitID.item_quest_current_release,
            };

        var group = new DrawingGroup();
        int i = 0;

        foreach (var trait in TraitCategories)
        {
            Tag<SB83E8080>? container = Investment.Get().GetItemIconContainer(Investment.Get().GetTrait(trait).Value.IconIndex);
            Texture? texture = ApiImageUtils.GetTexture(container.TagData.IconPrimaryContainer, 0, 4);
            UnmanagedMemoryStream? primaryStream = texture?.GetTexture();
            BitmapImage? primary = primaryStream != null ? ApiImageUtils.MakeBitmapImage(primaryStream, 128, 128) : null;
            group.Children.Add(new ImageDrawing(primary, new Rect(i, 0, 32, 32)));
            i += 32;
        }

        var dw = new DrawingImage(group);
        dw.Freeze();
        return dw;
    }

    private void BadgeCategory_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Category item = ((Button)sender).DataContext as Category;

        UserControl categoryView = new BadgeView(item);
        MainWindow.Current.MakeNewTab(item.ItemCategoryName, categoryView);
        MainWindow.Current.SetNewestTabSelected();
    }

    private void Category_OnClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Category item = ((Button)sender).DataContext as Category;
        UserControl userControl = null;

        switch (item.ItemCategoryHash)
        {
            case 1993337477: // Lore
                userControl = new LoreView(item);
                break;

            case 1866538467: // Triumphs
            case 616318467: // Seals
                item.CategoryBannerColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x93, 0x82, 0x4F));
                userControl = new TriumphView(item);
                break;

            case uint.MaxValue: // quests
                userControl = new QuestListView(item);
                break;

            default:
                userControl = new CategoryView(item);
                break;
        }

        _mainWindow.MakeNewTab(item.ItemCategoryName, userControl);
        _mainWindow.SetNewestTabSelected();
    }

    public int GetItemCategoryAmount(int index)
    {
        SDB788080 node = PresentationNodes[index];
        int count = node.Collectibles.Count;

        for (int j = 0; j < node.PresentationNodes.Count; j++)
        {
            count += GetItemCategoryAmount(node.PresentationNodes[j].PresentationNodeIndex);
        }

        return count;
    }

    private void ItemCategory_MouseEnter(object sender, RoutedEventArgs e)
    {
    }

    public void ItemCategory_MouseLeave(object sender, MouseEventArgs e)
    {
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (!ConfigSubsystem.Get().GetMotionEffects())
            return;

        float x = -12f / (float)MainWindow.Current.ActualWidth;
        float y = -12f / (float)MainWindow.Current.ActualHeight;
        Point position = e.GetPosition(this);

        TranslateTransform gridTransform = (TranslateTransform)MainContainer.RenderTransform;
        gridTransform.X = (int)Math.Round(position.X * x);
        gridTransform.Y = (int)Math.Round(position.Y * y);
    }

    // Essentially DestinyPresentationNodeDefinition
    public class Category : CharmUIElement
    {
        public uint ItemCategoryHash;
        public int ItemCategoryIndex;

        public string ItemCategoryName { get; set; }
        public string ItemCategoryType { get; set; }
        public string ItemCategoryDescription { get; set; }

        public string ItemCategoryLabel1 { get; set; }
        public string ItemCategoryLabel2 { get; set; }
        public string ItemCategoryLabel3 { get; set; }

        public DestinyPresentationScreenStyle ScreenStyle { get; set; }
        public DestinyPresentationDisplayStyle DisplayStyle { get; set; }

        public ImageSource ItemCategoryIcon { get; set; }
        public ImageSource ItemCategoryIcon2 { get; set; }
        public ImageSource ItemCategoryIcon3 { get; set; }
        public ImageSource ItemCategoryIcon4 { get; set; }
        public ImageSource ItemCategoryBackground { get; set; }

        public SolidColorBrush CategoryBannerColor { get; set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x72, 0xB6, 0xB1));

        public int ItemCategoryAmount { get; set; }
        public int Order;
    }

    public class RibbonCategoryTheme
    {
        public SolidColorBrush Background { get; set; } = new SolidColorBrush(Color.FromArgb(0xD4, 0x00, 0x69, 0x6B));
        public SolidColorBrush Border { get; set; } = new SolidColorBrush(Color.FromArgb(0xF7, 0x00, 0x7A, 0x7D));
        public SolidColorBrush RibbonBackground { get; set; } = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x7d, 0x80));
        public ImageSource RibbonBackgroundImage { get; set; }
        public Thickness RibbonBackgroundMargin { get; set; } = new(0, -75, 0, 0);
        public float DropShadowOpacity { get; set; } = 0f;
    }
}

