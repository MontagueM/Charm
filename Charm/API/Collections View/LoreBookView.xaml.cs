using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema.Investment;
using static Charm.CategoryView;

namespace Charm.Collections;

public partial class LoreBookView : UserControl
{
    private DynamicArray<SDB788080> Nodes = Investment.Get()._presentationNodeDefinitionMap.TagData.PresentationNodeDefinitions;
    private DynamicArray<S07588080> NodeStrings = Investment.Get()._presentationNodeDefinitionStringMap.TagData.PresentationNodeDefinitionStrings;

    private DynamicArray<SC16F8080> Records = Investment.Get()._recordNodeDefinitionMap.TagData.RecordDefinitions;
    private DynamicArray<S8B588080> RecordStrings = Investment.Get()._recordNodeDefinitionStringMap.TagData.RecordDefinitionStrings;

    public LoreBookView()
    {
        InitializeComponent();
        LoreBookEntries.ItemTemplate = (DataTemplate)FindResource("LoreEntryTemplate");
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
    }

    public void LoadLoreBook(int index)
    {
        List<CategoryEntry> items = new();
        SDB788080 curNode = Nodes[index];
        S07588080 curNodeStrings = NodeStrings[index];

        LoreBook loreBook = new LoreBook()
        {
            LoreBookName = UIHelper.AddSpacesBetweenChars(curNodeStrings.Name.Value.ToString().ToUpper(), 1),
            LoreBookIcon = ApiImageUtils.MakeIcon(curNodeStrings.IconIndex, 0, 0, 1),
        };
        DataContext = loreBook;

        foreach (var record in Nodes[index].Records)
        {
            var loreIndex = Records[record.RecordDefinitionIndex].LoreIndex;
            if (Investment.Get().GetItemLore(loreIndex) is null)
                continue;

            CategoryEntry subcategory = new()
            {
                EntryType = CategoryEntryType.Record,
                ItemIndex = loreIndex,
                ItemIcon = ApiImageUtils.MakeIcon(new FileHash(Strategy.IsLatest() ? 0x80C23298 : 0x80E64A25)),
                ItemName = RecordStrings[record.RecordDefinitionIndex].Name.Value?.ToString().ToUpper() ?? "",
                ItemType = RecordStrings[record.RecordDefinitionIndex].RecordTypeName.Value?.ToString().ToUpper() ?? ""
            };
            items.Add(subcategory);
        }

        // Gotta do this here since lore entries are actually sorted by lore index in-game
        var sortedItems = items.OrderBy(x => x.ItemIndex).ToList();
        for (int i = 0; i < sortedItems.Count; i++)
        {
            sortedItems[i].Index = i + 1;
        }

        LoreBookEntries.Items = sortedItems;
        LoreBookEntries.DisplayItems(true);
        UIHelper.SelectRadioButton(LoreBookEntries._ItemList, 0);

        UIHelper.AnimateFade(this, 0.1f, 1f, 0.5f);
    }

    private void LoreEntry_OnSelect(object sender, RoutedEventArgs e)
    {
        if ((sender as RadioButton) is null)
            return;

        CategoryEntry item = ((RadioButton)sender).DataContext as CategoryEntry;

        // RecordStrings contains the same text but I wanna use the lore string map instead
        var loreEntry = Investment.Get().GetItemLore(item.ItemIndex);
        var loreName = loreEntry.Value.LoreName?.Value?.ToString() ?? "";
        var loreText = loreEntry.Value.LoreDescription?.Value?.ToString() ?? "";

        Dispatcher.InvokeAsync(() =>
        {
            LoreEntryName.Text = loreName; //RecordStrings[item.ItemCategoryIndex].Name?.Value?.ToString() ?? "";
            LoreEntry.Text = loreText; //RecordStrings[item.ItemCategoryIndex].Description?.Value?.ToString() ?? "";
            LoreEntryScroll.ScrollToTop();
            UIHelper.AnimateFade(LoreEntryMain, 0.25f, 1f, 0.1f);
        }, DispatcherPriority.Background);
    }

    public struct LoreBook
    {
        public string LoreBookName { get; set; }
        public ImageSource LoreBookIcon { get; set; }
    }
}
