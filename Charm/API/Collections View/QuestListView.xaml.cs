using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.CollectionsView;

namespace Charm.Collections;

/// <summary>
/// Interaction logic for QuestListView.xaml
/// </summary>
public partial class QuestListView : UserControl, INotifyPropertyChanged
{
    public ConcurrentDictionary<DestinyTraitID, ConcurrentBag<QuestCategoryEntry>> FilteredQuests = new();

    private QuestCategory _currentCategory;
    public QuestCategory CurrentCategory
    {
        get => _currentCategory;
        set
        {
            if (_currentCategory != value)
            {
                _currentCategory = value;
                OnPropertyChanged(nameof(CurrentCategory));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }

    public List<DestinyTraitID> TraitCategories = new()
        {
            DestinyTraitID.item_quest_all,
            DestinyTraitID.item_quest_new_light,
            DestinyTraitID.item_quest_current_release,
            DestinyTraitID.item_quest_playlists,
            DestinyTraitID.item_quest_seasonal,
            DestinyTraitID.item_quest_exotic,
            DestinyTraitID.item_quest_past,
        };

    public QuestListView(Category itemCategory)
    {
        InitializeComponent();
        DataContext = this;

        Load();
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Focusable = true;
        Focus();
    }

    public async void Load()
    {
        await LoadQuests();
        LoadCategories();
        UIHelper.SelectRadioButton(CategoriesPanel, 0);
    }

    public void LoadCategories()
    {
        foreach (var trait in TraitCategories)
        {
            var category = new QuestCategory()
            {
                CategoryName = $"{trait.GetEnumDescription()}s",
                CategoryTrait = trait,
                Quests = FilteredQuests[trait].OrderBy(x => x.QuestItem.GetItemIndex()).ToList(),
            };

            if (trait == DestinyTraitID.item_quest_all)
            {
                var traitDef = Investment.Get()._traitDefinitionStringMap.TagData.TraitStrings.First(x => (int)x.TraitHash == 1434215347);
                category.CategoryName = "All Quests";
                category.CategoryLabel = "All Quests";
                category.CategoryDescription = "All quests from all categories";
                category.CategoryIcon = ApiImageUtils.MakeIcon(traitDef.IconIndex);
                category.GradientColor = Color.FromArgb(0xFF, 0x2b, 0x39, 0x96);
            }
            else
            {
                var traitDef = Investment.Get().GetTrait(trait).Value;
                if (traitDef.IconIndex != -1)
                {
                    var iconContainer = Investment.Get().GetItemIconContainer(traitDef.IconIndex);
                    var col = iconContainer.TagData.DyeColorR;

                    category.CategoryLabel = trait != DestinyTraitID.item_quest_current_release ? traitDef.TraitName.Value : "Renegades"; // stupid cus trait tag chooses Final Shape instead
                    category.CategoryDescription = traitDef.TraitDescription.Value;
                    category.CategoryIcon = ApiImageUtils.MakeIcon(traitDef.IconIndex);
                    category.CategoryBanner = ApiImageUtils.MakeIcon(traitDef.IconIndex, listIndex: 1);
                    category.GradientColor = Color.FromScRgb(col.W, col.X, col.Y, col.Z);
                    //Console.WriteLine($"{trait} {category.GradientColor.R:X},{category.GradientColor.G:X},{category.GradientColor.B:X}");
                }
            }

            CategoriesPanel.Items.Add(category);
        }
    }

    public async Task LoadQuests()
    {
        MainWindow.Progress.SetProgressStage("Loading Quests");
        IEnumerable<InventoryItem> inventoryItems = await Investment.Get().GetInventoryItems();

        List<InventoryItem> sortedQuests = new();
        foreach (var quest in inventoryItems.Where(x => x.ItemTraits.Any(k => k == DestinyTraitID.inventory_filtering_quest || k == DestinyTraitID.inventory_filtering_quest_featured)))
        {
            if (quest is null || quest.TagData.Unk58.GetValue(quest.GetReader()) is not S88738080 quests)
                continue;

            var questSteps = quests.ItemList ?? new();
            if (!questSteps.Any() || questSteps.First().Index == -1)
                continue;

            var item = Investment.Get().GetInventoryItem(questSteps.First().Index);
            if (item is not null && !sortedQuests.Contains(item))
                sortedQuests.Add(item);
        }

        foreach (InventoryItem item in sortedQuests)
        {
            var priorityTrait = GetPriorityTrait(item.ItemTraits);
            AddToFilteredQuests(priorityTrait, item);
        }

        MainWindow.Progress.CompleteStage();
    }

    private DestinyTraitID GetPriorityTrait(IEnumerable<DestinyTraitID> traits)
    {
        if (traits.Contains(DestinyTraitID.item_quest_exotic)) return DestinyTraitID.item_quest_exotic;

        if (traits.Contains(DestinyTraitID.item_quest_current_release)
         || traits.Contains(DestinyTraitID.item_quest_frontier_apollo))
            return DestinyTraitID.item_quest_current_release;

        if (traits.Contains(DestinyTraitID.item_quest_past))
            return DestinyTraitID.item_quest_past;

        if (traits.Contains(DestinyTraitID.item_quest_playlists))
            return DestinyTraitID.item_quest_playlists;

        if (traits.Contains(DestinyTraitID.item_quest_seasonal)
         || traits.Contains(DestinyTraitID.item_quest_event))
            return DestinyTraitID.item_quest_seasonal;

        if (traits.Contains(DestinyTraitID.item_quest_new_light))
            return DestinyTraitID.item_quest_new_light;

        return DestinyTraitID.item_quest_all;
    }

    private void AddToFilteredQuests(DestinyTraitID trait, InventoryItem item)
    {
        if (!FilteredQuests.ContainsKey(trait))
            FilteredQuests[trait] = new();

        QuestCategoryEntry questEntry = new()
        {
            QuestItem = item,
            QuestIcon = ApiImageUtils.MakeIcon(item) ?? ApiImageUtils.MakeIcon(Investment.Get().GetTrait(trait).Value.IconIndex, listIndex: 3),
            QuestName = item.Name,
            QuestDescription = item.Description,
            MainTrait = trait
        };
        FilteredQuests[trait].Add(questEntry);

        if (!FilteredQuests.ContainsKey(DestinyTraitID.item_quest_all))
            FilteredQuests[DestinyTraitID.item_quest_all] = new();

        if (!FilteredQuests[DestinyTraitID.item_quest_all].Contains(questEntry))
            FilteredQuests[DestinyTraitID.item_quest_all].Add(questEntry);
    }

    private void CategoryButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        QuestCategory category = (sender as RadioButton).DataContext as QuestCategory;

        float transitionSpeed = 0.165f;
        UIHelper.AnimateSlide(QuestsPanel, transitionSpeed, new(0, -11), new(0, 0));

        UIHelper.AnimateFade(CategoryRibbon, transitionSpeed, 0f, 1f);
        UIHelper.AnimateFade(CategoryGradient, transitionSpeed, 0f, 1f);

        UIHelper.AnimateFade(QuestsPanel, transitionSpeed, 0f, 1f, (s, e) =>
        {
            CurrentCategory = category;
            QuestsList.Items = category.Quests;
            QuestsList.DisplayItems(true);

            UIHelper.AnimateSlide(QuestsPanel, transitionSpeed, new(0, 0), new(0, 11));
            UIHelper.AnimateFade(QuestsPanel, transitionSpeed, 1f, 0, additive: true);

            UIHelper.AnimateFade(CategoryRibbon, transitionSpeed, 1f, 0, additive: true);
            UIHelper.AnimateFade(CategoryGradient, transitionSpeed, 1f, 0, additive: true);
        }, additive: true);
    }

    private void QuestItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        QuestCategoryEntry quest = (sender as FrameworkElement).DataContext as QuestCategoryEntry;
        if (quest != null)
        {
            MainWindow.Current._ToolTip.ActiveItem = sender as FrameworkElement;
            MainWindow.Current._ToolTip.MakeTooltip(quest.QuestItem);
        }
    }

    private void QuestItem_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        QuestCategoryEntry quest = (sender as FrameworkElement).DataContext as QuestCategoryEntry;
        if (quest is null || quest.QuestItem.TagData.Unk58.GetValue(quest.QuestItem.GetReader()) is not S88738080 quests)
            return;

        var userControl = new QuestView(quest.QuestItem, quest.MainTrait);
        MainWindow.Current.MakeNewTab(quest.QuestItem.Name, userControl);
        MainWindow.Current.SetNewestTabSelected();
    }

    private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            var tab = (TabItem)MainWindow.Current._MainTabControl.Items[MainWindow.Current._MainTabControl.SelectedIndex];
            MainWindow.Current._MainTabControl.Items.Remove(tab);
        }
    }

    public class QuestCategory : CharmUIElement
    {
        public string CategoryName { get; set; }
        public string CategoryLabel { get; set; }
        public string CategoryDescription { get; set; }

        public DestinyTraitID CategoryTrait { get; set; }
        public ImageSource CategoryIcon { get; set; }
        public ImageSource CategoryBanner { get; set; }
        public Color GradientColor { get; set; }
        public List<QuestCategoryEntry> Quests { get; set; } = new();
    }

    public class QuestCategoryEntry : CharmUIElement
    {
        public InventoryItem QuestItem { get; set; }
        public ImageSource QuestIcon { get; set; }
        public string QuestName { get; set; }
        public string QuestDescription { get; set; }
        public DestinyTraitID MainTrait { get; set; }
    }
}
