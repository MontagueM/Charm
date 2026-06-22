using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Tiger;
using Tiger.Schema.Investment;

namespace Charm.Collections;

/// <summary>
/// Interaction logic for QuestView.xaml
/// </summary>
public partial class QuestView : UserControl
{
    public QuestItem CurrentQuest { get; set; }

    public QuestView(InventoryItem item, DestinyTraitID? trait = null)
    {
        InitializeComponent();
        LoadQuest(item, trait);

        QuestSteps.OnBeforePageChange += (s, items) =>
        {
            UIHelper.AnimateFade(QuestStepSummaryText, 0.05f, 0f, 0.6f, additive: true);
        };

        QuestSteps.OnAfterPageChange += (s, items) =>
        {
            UIHelper.AnimateFade(QuestStepSummaryText, 0.05f, 0.6f, 0f, additive: true);

            if (QuestSteps.CurrentPageItems.Any() && CurrentQuest is not null)
            {
                CurrentQuest.CurrentStep = QuestSteps.CurrentPageItems.First() as QuestStep;
            }
        };

        UIHelper.AnimateSlide(QuestHeaderIcon, 0.25f, new(0, 0), new(0, -15));
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        Focusable = true;
        Focus();
    }

    private void LoadQuest(InventoryItem questItem, DestinyTraitID? overrideTrait)
    {
        CurrentQuest = new()
        {
            QuestName = questItem.Name.ToUpper(),
            QuestInvItem = questItem,
            QuestSteps = new()
        };

        var icon = ApiImageUtils.MakeIcon(questItem.GetItemStrings().TagData.IconIndex, 0, 3, 0);
        CurrentQuest.QuestIcon = icon;

        foreach (var index in questItem.TagData.TraitIndices.Select(x => x.Index))
        {
            var trait = Investment.Get().GetTrait(index).Value;
            if (trait.IconIndex == -1)
                continue;

            SetQuestDetails(trait.TraitHash);
            break;
        }

        // If nothing was set, try the override trait if provided
        if (CurrentQuest.QuestType is null && overrideTrait is not null)
            SetQuestDetails(overrideTrait.Value);

        if (questItem.TagData.Unk58.GetValue(questItem.GetReader()) is not S80807388 questSteps)
            return;

        foreach (var questStep in questSteps.ItemList)
        {
            if (questStep.Index == -1)
                continue;

            var item = Investment.Get().GetInventoryItem(questStep.Index);
            var strings = item.GetItemStrings();
            QuestStep step = new()
            {
                QuestStepItem = item,
                QuestStepDescription = strings.TagData.ItemDescription.Value,
                QuestStepFlavorText = strings.TagData.ItemDisplaySource.Value,
                QuestStepSummary = ((S808054D0)strings.TagData.Unk58.GetValue(strings.GetReader())).QuestStepSummary.Value,
                QuestObjectives = new(),
                QuestRewards = new(),
            };

            if (item.TagData.Unk38.GetValue(item.GetReader()) is S808073B0 objectives)
            {
                foreach (var objective in objectives.Objectives)
                {
                    S80805850? obj = Investment.Get().GetObjective(objective.ObjectiveIndex);
                    if (obj is not null && obj.Value.ProgressDescription.Value is not null)
                    {
                        QuestObjective questObj = new()
                        {
                            Description = obj.Value.ProgressDescription.Value,
                            Location = obj.Value.LocationIndex, // TODO
                            Value = Investment.Get().GetObjectiveValue(objective.ObjectiveIndex),
                            Style = (DestinyUnlockValueUIStyle)obj.Value.InProgressValueStyle
                        };

                        if (questObj.Style == DestinyUnlockValueUIStyle.Automatic)
                        {
                            if (questObj.Value == 1)
                                questObj.Style = DestinyUnlockValueUIStyle.Checkbox;
                        }

                        step.QuestObjectives.Add(questObj);
                    }
                }
            }

            if (item.TagData.Unk80_EoF.GetValue(item.GetReader()) is S8080757C questRewards)
            {
                foreach (var reward in new List<DynamicStruct<SQuestStepReward>> { questRewards.Reward1, questRewards.Reward2, questRewards.Reward3, questRewards.Reward4, questRewards.Reward5, questRewards.Reward6 })
                {
                    if (reward.Value.ItemIndex != -1)
                    {
                        step.QuestRewards.Add(new()
                        {
                            Item = new APIPlugItem(Investment.Get().GetInventoryItem(reward.Value.ItemIndex)),
                            Quantity = reward.Value.Quantity
                        });
                    }
                }
            }

            CurrentQuest.QuestSteps.Add(step);
        }

        if (CurrentQuest.QuestSteps.Any())
            CurrentQuest.CurrentStep = CurrentQuest.QuestSteps.First();

        DataContext = CurrentQuest;
    }

    private void SetQuestDetails(DestinyTraitID traitID)
    {
        if (traitID == DestinyTraitID.item_quest_all) // I don't like this but whatever
            traitID = DestinyTraitID.item_quest_current_release;

        var investment = Investment.Get();
        var trait = investment.GetTrait(traitID).Value;

        if (trait.IconIndex == -1)
            return;

        var iconContainer = investment.GetItemIconContainer(trait.IconIndex);
        var col = iconContainer.TagData.DyeColorR;

        CurrentQuest.GradientColor = Color.FromScRgb(col.W, col.X, col.Y, col.Z);
        CurrentQuest.QuestTypeIconIndex = trait.IconIndex;
        CurrentQuest.QuestTypeIconContainer = iconContainer.TagData.IconPrimaryContainer.Hash;
        CurrentQuest.QuestType = trait.TraitHash.GetEnumDescription();

        if (CurrentQuest.QuestIcon is null)
            CurrentQuest.QuestIcon = ApiImageUtils.MakeIcon(trait.IconIndex, 0, 0, 3);
    }

    private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Escape)
        {
            var tab = (TabItem)MainWindow.Current._MainTabControl.Items[MainWindow.Current._MainTabControl.SelectedIndex];
            MainWindow.Current._MainTabControl.Items.Remove(tab);
        }
    }

    public class QuestItem : INotifyPropertyChanged
    {
        public string QuestName { get; set; }
        public string QuestType { get; set; }

        public ImageSource QuestIcon { get; set; }
        public int QuestTypeIconIndex { get; set; }
        public FileHash QuestTypeIconContainer { get; set; }

        public Color GradientColor { get; set; }

        public InventoryItem QuestInvItem { get; set; }
        public List<QuestStep> QuestSteps { get; set; }

        private QuestStep _currentStep;
        public QuestStep CurrentStep
        {
            get => _currentStep;
            set
            {
                if (_currentStep != value)
                {
                    _currentStep = value;

                    var questBannerIndex = _currentStep.QuestStepItem.GetItemStrings().TagData.EmblemContainerIndex;
                    if (questBannerIndex != -1 && _currentStep.QuestObjectives.Any(x => x.Location != -1))
                        _currentStep.QuestStepBanner = ApiImageUtils.MakeIcon(questBannerIndex);
                    else
                        _currentStep.QuestStepBanner = ApiImageUtils.MakeIcon(QuestTypeIconIndex, 0, 0, 2);

                    OnPropertyChanged(nameof(CurrentStep));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class QuestStep : CharmUIElement
    {
        public QuestStep() { }

        public InventoryItem QuestStepItem { get; set; }
        public ImageSource QuestStepBanner { get; set; }
        public string QuestStepDescription { get; set; }
        public string QuestStepFlavorText { get; set; }
        public string QuestStepSummary { get; set; }
        public List<QuestObjective> QuestObjectives { get; set; }

        public List<QuestReward> QuestRewards { get; set; }
    }

    public class QuestObjective
    {
        public string Description { get; set; }
        public int Location { get; set; }
        public int Value { get; set; }
        public DestinyUnlockValueUIStyle Style { get; set; }
    }

    public class QuestReward
    {
        public APIPlugItem Item { get; set; }
        public int Quantity { get; set; }
    }
}
