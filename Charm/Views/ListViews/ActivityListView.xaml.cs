using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Charm.Shared;
using SharpVectors.Converters;
using Tiger;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;
using Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601;

namespace Charm;

public partial class ActivityListView : UserControl
{
    private ConfigSubsystem Config = TigerInstance.GetSubsystem<ConfigSubsystem>();
    private ConcurrentDictionary<ActivityCategoryType, ActivityCategoryItem> ActivityCategories;
    private ConcurrentBag<ActivityItem> Activities = new();

    private int SortByIndex = 1;

    public ActivityListView()
    {
        InitializeComponent();
#if DEBUG
        // I can't be asked to fix these seemingly harmless but lag inducing xaml binding errors
        PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
#endif
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        if (Strategy.IsPreBL() || Strategy.IsD1())
            PreBlWarn.Visibility = Visibility.Visible;
    }

    public async void LoadContent()
    {
        MainWindow.Progress.SetProgressStages(new List<string>
        {
            "Creating Activity List.",
        });

        await CreateActivityCategories();

        MainWindow.Progress.CompleteStage();

        CreateFilterOptions();
    }

    private void CreateFilterOptions()
    {
        ComboBoxControl sortBy = new();
        sortBy.Text = "Sort By";
        sortBy.TextFontSize = 16;
        sortBy.Box.MinWidth = 250;
        sortBy.Box.ItemsSource = new List<ComboBoxItem>()
        {
            new() { Content = "Activity Name ↓", Tag = 1 },
            new() { Content = "Activity Name ↑", Tag = 2 },
            new() { Content = "Destination ↓", Tag = 3 },
            new() { Content = "Destination ↑", Tag = 4 },
        };
        if (sortBy.Box.SelectedIndex == -1)
        {
            sortBy.Box.SelectedIndex = 0;
        }

        sortBy.Box.SelectionChanged += SortBy_OnSelectionChanged;
        FilterOptions.Children.Add(sortBy);
    }

    private async Task CreateActivityCategories()
    {
        await Task.Run(() =>
        {
            ActivityCategories = new();
            var names = LoadActivityDestinationNames();

            if (Strategy.IsD1())
                PopulateD1Activities(names);
            else
                PopulateD2Activities(names);
        });

        ActivityCategories[ActivityCategoryType.All].Activities =
            ActivityCategories.Values.SelectMany(x => x.Activities).ToList();

        ActivityCategoryList.ItemsSource = ActivityCategories.Values
            .OrderBy(x => x.CategoryName)
            .ToList();

        RefreshActivityCategoryList();
    }

    private Dictionary<string, string> LoadActivityDestinationNames()
    {
        ConcurrentDictionary<string, StringHash> nameHashes = new();
        var globalStrings = GlobalStrings.Get();
        switch (Strategy.CurrentStrategy)
        {
            case TigerStrategy.DESTINY1_RISE_OF_IRON:
                Parallel.ForEach(PackageResourcer.Get().GetD1Activities(), activity =>
                {
                    if (activity.Value != 0x80800616) return;
                    var tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(activity.Key);
                    nameHashes.TryAdd(tag.TagData.ActivityDevName.Value, tag.TagData.DestinationName);
                    globalStrings.AddStrings(tag.TagData.LocalizedStrings);
                });
                break;

            case TigerStrategy.DESTINY2_SHADOWKEEP_2601 or TigerStrategy.DESTINY2_SHADOWKEEP_2999:
                Parallel.ForEach(PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>(), val =>
                {
                    var tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                    nameHashes.TryAdd(tag.TagData.ActivityDevName.Value, tag.TagData.DestinationName);
                    globalStrings.AddStrings(tag.TagData.LocalizedStrings);
                });
                break;

            default:
                Parallel.ForEach(PackageResourcer.Get().GetAllHashes<S8B8E8080>(), val =>
                {
                    var tag = FileResourcer.Get().GetSchemaTag<S8B8E8080>(val);
                    nameHashes.TryAdd(tag.TagData.DestinationName, tag.TagData.LocationName);
                    globalStrings.AddStrings(tag.TagData.StringContainer);
                });
                break;
        }

        return nameHashes.ToDictionary(
            kvp => kvp.Key,
            kvp => GlobalStrings.Get().GetString(kvp.Value)
        );
    }

    private void PopulateD1Activities(Dictionary<string, string> names)
    {
        foreach (var activity in PackageResourcer.Get().GetD1Activities())
        {
            if (activity.Value != "2E058080") continue;

            string activityName = PackageResourcer.Get().GetActivityName(activity.Key);
            var splitName = activityName.Split(":");
            var destName = names.TryGetValue(splitName[1], out string dest) ? dest : string.Empty;

            var categoryType = GetActivityCategoryType(splitName[1]);
            var category = GetCategory(categoryType);
            category.Activities.Add(new ActivityItem
            {
                ActivityIcon = GetCategoryIcon(categoryType),
                ActivityColor = GetCategoryColor(categoryType),
                ActivityHash = activity.Key,
                ActivityHashString = $"{activity.Key}",
                ActivityName = activityName,
                ActivityDestinationDev = splitName[1],
                ActivityDestination = destName,
            });
        }
    }

    private void PopulateD2Activities(Dictionary<string, string> names)
    {
        bool isBL = Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_BEYONDLIGHT_3402;

        foreach (var val in PackageResourcer.Get().GetAllHashes<IActivity>())
        {
            string activityName = PackageResourcer.Get().GetActivityName(val);
            if (activityName.EndsWith("_ambient")) continue;

            var splitName = isBL ? activityName.Split(".") : activityName.Split(":");
            string destKey = isBL ? splitName[0] : splitName[1];

            var categoryType = GetActivityCategoryType(isBL ? activityName : destKey);
            var category = GetCategory(categoryType);

            var activityType = activityName.EndsWith("iron_banner") ? ActivityCategoryType.IronBanner
                : activityName.EndsWith("trials") ? ActivityCategoryType.Trials
                : category.CategoryType;

            var destName = names.TryGetValue(destKey, out string dest) ? dest : string.Empty;
            if (Helpers.IsValidHexHash(destName) || destName == string.Empty)
            {
                // some activities (Crucible map Solitude for example), dont have a valid destination name in the way we're checking
                // so this pretty much just makes it use the first bubble name
                var activity = FileResourcer.Get().GetFileInterface<IActivity>(val);
                destName = activity.DestinationName;
            }

            category.Activities.Add(new ActivityItem
            {
                ActivityIcon = GetCategoryIcon(activityType),
                ActivityColor = GetCategoryColor(activityType),
                ActivityHash = val,
                ActivityHashString = $"{val}",
                ActivityName = splitName[1],
                ActivityType = activityType,
                ActivityDestinationDev = destKey,
                ActivityDestination = destName
            });
        }
    }

    private ActivityCategoryType GetActivityCategoryType(string activityName)
    {
        if (activityName.StartsWith("crucible") || activityName.StartsWith("pvp_"))
            return ActivityCategoryType.Crucible;
        if (activityName.StartsWith("gambit")) return ActivityCategoryType.Gambit;
        if (activityName.StartsWith("quest") && !activityName.Contains("exotic"))
            return ActivityCategoryType.Quests;
        if (activityName.Contains("exotic")) return ActivityCategoryType.ExoticQuests;
        if (activityName.Contains("raid") || activityName.Contains("kingsfall"))
            return ActivityCategoryType.Raids;
        if (activityName.Contains("dungeon")) return ActivityCategoryType.Dungeons;
        if (activityName.Contains("mission") || activityName.Contains("campaign"))
            return ActivityCategoryType.Story;
        if (activityName.Contains("strike") || activityName.Contains("battleground"))
            return ActivityCategoryType.Strikes;
        if (activityName.Contains("freeroam") || activityName.Contains("patrol"))
            return ActivityCategoryType.Patrols;
        if (activityName.EndsWith("_ls") || activityName.Contains("_ls_"))
            return ActivityCategoryType.LostSectors;

        return ActivityCategoryType.All;
    }

    private ActivityCategoryItem GetCategory(ActivityCategoryType categoryType)
    {
        return ActivityCategories.GetOrAdd(categoryType, type => new ActivityCategoryItem
        {
            CategoryIcon = GetCategoryIcon(categoryType),
            CategoryColor = GetCategoryColor(categoryType),
            CategoryName = EnumExtensions.GetEnumDescription(categoryType),
            CategoryType = categoryType,
            Activities = new()
        });
    }

    private async void ActivityCategory_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn)
            return;

        if (btn.DataContext is ActivityCategoryItem item)
        {
            await LoadActivityList(item);
        }
    }

    private async Task LoadActivityList(ActivityCategoryItem item)
    {
        await Task.Run(() =>
        {
            if (Activities.Count != 0)
                Activities.Clear();

            Activities = new ConcurrentBag<ActivityItem>(item.Activities);
        });

        RefreshActivityList();
    }

    public void RefreshActivityCategoryList()
    {
        if (ActivityCategories == null)
            return;
        if (ActivityCategories.IsEmpty)
            return;

        string searchStr = ActivitySearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentDictionary<ActivityCategoryType, ActivityCategoryItem>();
        Parallel.ForEach(ActivityCategories, activityCat =>
        {
            if (isHash && activityCat.Value.Activities.Any(x => x.ActivityHash.Hash32 == parsedHash)) // hacky but eh
            {
                IEnumerable<ActivityItem> activities = activityCat.Value.Activities.Where(x => x.ActivityHash.Hash32 == parsedHash);
                displayItems.TryAdd(activityCat.Key, new ActivityCategoryItem
                {
                    CategoryIcon = activityCat.Value.CategoryIcon,
                    CategoryColor = activityCat.Value.CategoryColor,
                    CategoryName = activityCat.Value.CategoryName,
                    CategoryType = activityCat.Value.CategoryType,
                    Activities = activities.ToList()
                });
            }
            else if (activityCat.Value.CategoryName.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.TryAdd(activityCat.Key, activityCat.Value);
            }
            else
            {
                var matched = activityCat.Value.Activities.Where(
                    x => x.ActivityName.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
                    || x.ActivityDestination.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
                    || x.ActivityDestinationDev.Contains(searchStr, StringComparison.OrdinalIgnoreCase)).ToList();

                if (matched.Count == 0) return;
                displayItems.TryAdd(activityCat.Key, new ActivityCategoryItem
                {
                    CategoryIcon = activityCat.Value.CategoryIcon,
                    CategoryColor = activityCat.Value.CategoryColor,
                    CategoryName = activityCat.Value.CategoryName,
                    CategoryType = activityCat.Value.CategoryType,
                    Activities = matched
                });
            }
        });

        ActivityCategoryList.ItemsSource = displayItems.Values
           .OrderBy(x => x.CategoryName)
           .ToList();
    }

    private void RefreshActivityList()
    {
        if (Activities == null)
            return;

        if (Activities.IsEmpty)
        {
            ActivityList.ItemsSource = null;
            return;
        }

        string searchStr = ActivityItemSearchBox.Text;

        uint parsedHash = 0;
        bool isHash = Helpers.ParseHash(searchStr, out parsedHash);

        var displayItems = new ConcurrentBag<ActivityItem>();
        Parallel.ForEach(Activities, act =>
        {
            if ((isHash && act.ActivityHash.Hash32 == parsedHash)
            || act.ActivityHash.ToString().Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || act.ActivityName.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || act.ActivityDestination.Contains(searchStr, StringComparison.OrdinalIgnoreCase)
            || act.ActivityDestinationDev.Contains(searchStr, StringComparison.OrdinalIgnoreCase))
            {
                displayItems.Add(act);
            }
        });

        List<ActivityItem> items = SortByIndex switch
        {
            1 => displayItems.OrderBy(x => x.ActivityName).ToList(),
            2 => displayItems.OrderByDescending(x => x.ActivityName).ToList(),
            3 => displayItems.OrderBy(x => x.ActivityDestination).ToList(),
            4 => displayItems.OrderByDescending(x => x.ActivityDestination).ToList(),
            _ => displayItems.ToList()
        };

        ActivityList.ItemsSource = items;
        UIHelper.ScrollToTop(ActivityList);
    }

    private async void ActivityItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn)
            return;

        if (btn.DataContext is ActivityItem item)
        {
            ActivityView activityView = new();
            await activityView.LoadActivity(item.ActivityHash);

            MainWindow.Current.MakeNewTab(PackageResourcer.Get().GetActivityName(item.ActivityHash), activityView);
            MainWindow.Current.SetNewestTabSelected();
        }
    }

    private CancellationTokenSource _searchDebounce;
    private async void ActivitySearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounce?.Cancel();
        _searchDebounce = new CancellationTokenSource();
        try
        {
            await Task.Delay(100, _searchDebounce.Token);
            RefreshActivityCategoryList();
        }
        catch (TaskCanceledException) { }
    }

    private async void ActivityItemSearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchDebounce?.Cancel();
        _searchDebounce = new CancellationTokenSource();
        try
        {
            await Task.Delay(100, _searchDebounce.Token);
            RefreshActivityList();
        }
        catch (TaskCanceledException) { }
    }

    private void SortBy_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SortByIndex = (int)((sender as ComboBox).SelectedItem as ComboBoxItem).Tag;
        RefreshActivityList();
    }

    private class ActivityCategoryItem : CharmUIElement
    {
        public SolidColorBrush CategoryColor { get; set; }
        public string CategoryName { get; set; }
        public ActivityCategoryType CategoryType { get; set; }
        public ImageSource CategoryIcon { get; set; }

        public List<ActivityItem> Activities { get; set; } = new();
    }

    private class ActivityItem : CharmUIElement
    {
        public SolidColorBrush ActivityColor { get; set; }
        public string ActivityName { get; set; }
        public ActivityCategoryType ActivityType { get; set; }
        public string ActivityDestinationDev { get; set; }
        public string ActivityDestination { get; set; }
        public ImageSource ActivityIcon { get; set; }
        public FileHash ActivityHash { get; set; }
        public string ActivityHashString { get; set; }
    }

    private enum ActivityCategoryType
    {
        // General activity type
        All,
        [Description("Crucible")]
        Crucible,
        [Description("Gambit")]
        Gambit,
        [Description("Raids")]
        Raids,
        [Description("Dungeons")]
        Dungeons,
        [Description("Strikes")]
        Strikes,
        [Description("Patrols")]
        Patrols,
        [Description("Story")]
        Story,
        [Description("Quests")]
        Quests,
        [Description("Exotic Quests")]
        ExoticQuests,
        [Description("Lost Sectors")]
        LostSectors,

        // Activity Specific
        Default,
        Ambient,
        Trials,
        IronBanner,
    }

    private static readonly Dictionary<ActivityCategoryType, SolidColorBrush> _colorCache = Enum.GetValues<ActivityCategoryType>().ToDictionary(t => t, t => CreateBrush(t));
    private static SolidColorBrush CreateBrush(ActivityCategoryType type)
    {
        var col = type switch
        {
            ActivityCategoryType.Crucible => Color.FromArgb(0xFF, 145, 37, 29),
            ActivityCategoryType.Trials => Color.FromArgb(0xFF, 198, 159, 99),
            ActivityCategoryType.IronBanner => Color.FromArgb(0xFF, 57, 119, 94),
            ActivityCategoryType.Gambit => Color.FromArgb(0xFF, 57, 119, 94),
            ActivityCategoryType.Raids => Color.FromArgb(0xFF, 50, 50, 50),
            ActivityCategoryType.Dungeons => Color.FromArgb(0xFF, 104, 85, 72),
            ActivityCategoryType.Strikes => Color.FromArgb(0xFF, 57, 100, 128),
            ActivityCategoryType.Patrols => Color.FromArgb(0xFF, 57, 128, 128),
            ActivityCategoryType.Story => Color.FromArgb(0xFF, 38, 68, 127),
            ActivityCategoryType.ExoticQuests => Color.FromArgb(0xFF, 191, 153, 65),
            ActivityCategoryType.LostSectors => Color.FromArgb(0xFF, 80, 73, 159),
            ActivityCategoryType.All => Color.FromArgb(0xFF, 100, 100, 100),
            _ => Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)
        };

        var brush = new SolidColorBrush(col);
        brush.Freeze();
        return brush;
    }

    private SolidColorBrush GetCategoryColor(ActivityCategoryType type) => _colorCache.GetValueOrDefault(type, _colorCache[ActivityCategoryType.All]);

    private static readonly ConcurrentDictionary<ActivityCategoryType, ImageSource> _iconCache = new();
    private ImageSource GetCategoryIcon(ActivityCategoryType type)
    {
        return _iconCache.GetOrAdd(type, static t =>
        {
            ImageSource image;

            if (t == ActivityCategoryType.All)
            {
                image = new BitmapImage(UIHelper.MakePackUri("/Assets/icons/globe.png"));
            }
            else
            {
                string filename = t switch
                {
                    ActivityCategoryType.Crucible => "crucible",
                    ActivityCategoryType.Trials => "osiris",
                    ActivityCategoryType.IronBanner => "iron-banner",
                    ActivityCategoryType.Gambit => "gambit",
                    ActivityCategoryType.Raids => "raid",
                    ActivityCategoryType.Dungeons => "dungeon",
                    ActivityCategoryType.Strikes => "strike",
                    ActivityCategoryType.Patrols => "patrol",
                    ActivityCategoryType.Story => "quest",
                    ActivityCategoryType.ExoticQuests => "engram",
                    ActivityCategoryType.LostSectors => "lost-sector",
                    _ => null
                };

                if (filename is null) return null;

                image = (ImageSource)new SvgImageExtension
                {
                    Source = UIHelper.MakePackUri($"/Assets/icons/{filename}.svg").ToString()
                }.ProvideValue(null);
            }

            image.Freeze();
            return image;
        });
    }
}


