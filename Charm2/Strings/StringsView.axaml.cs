using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Charm.ViewModels;
using Charm.Views.Strings;
using Tiger;
using Tiger.Schema;

namespace Charm.Strings;

internal partial class StringsView : UserControl
{
    public StringsView()
    {
        InitializeComponent();

        StringsFileView.ListControl.ViewModel.UpdateItems(LoadStringContainersList());

        // StringsFileView.Load();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private IEnumerable<ListItemViewModel> LoadStringContainersList()
    {
        // If there are packages, we don't want to reload the view as very poor for performance.
        // if (_allTagItems != null)
        //     return;

        // MainWindow.Progress.SetProgressStages(new List<string>
        // {
        //     "caching string tags",
        //     "load string list",
        // });

        List<TigerHash> stringsList = PackageResourcer.Get().GetAllHashes<LocalizedStrings>();
        List<ListItemViewModel> listItems = new();
        // stringsList.ForEach(x => listItems.Add(new ListItemViewModel{Title="Some",Hash=x, FontSize=16, Type="StringContainer"}));
        return listItems;
        // await Task.Run(() =>
        // {
        //     _allTagItems = new ConcurrentBag<TagItem>();
        //     var vals = PackageHandler.GetAllTagsWithReference(0x808099ef);
        //     // PackageHandler.CacheHashDataList(vals.Select(x => x.Hash).ToArray());
        //     MainWindow.Progress.CompleteStage();
        //
        //     Parallel.ForEach(vals, val =>
        //     {
        //         _allTagItems.Add(new TagItem
        //         {
        //             Hash = val,
        //             Name = $"{val}",
        //             TagType = ETagListType.StringContainer
        //         });
        //     });
        //     MainWindow.Progress.CompleteStage();
        //
        //     MakePackageTagItems();
        // });

        // RefreshItemList();  // bc of async stuff
    }

    private void LoadStringContainer(FileHash tagHash)
    {
        // SetViewer(TagView.EViewerType.TagList);
        // var viewer = GetViewer();
        // viewer.TagListControl.LoadContent(ETagListType.Strings, tagHash, true);
    }

    // Would be nice to do something with colour formatting.
    private void LoadStrings(FileHash tagHash)
    {
        // var viewer = GetViewer();
        // _allTagItems = new ConcurrentBag<TagItem>();
        // StringContainer stringContainer = PackageHandler.GetTag(typeof(StringContainer), tagHash);
        // Parallel.ForEach(stringContainer.Header.StringHashTable, hash =>
        // {
        //     _allTagItems.Add(new TagItem
        //     {
        //         Name = stringContainer.GetStringFromHash(FbxIOSettings.ELanguage.English, hash),
        //         Hash = hash,
        //         TagType = ETagListType.String
        //     });
        // });
        // RefreshItemList();
        // SetExportFunction(ExportString, (int)EExportTypeFlag.Full);
        // viewer.ExportControl.SetExportInfo(tagHash);
    }

    // private void ExportString(ExportInfo info)
    // {
    //     //
    //     // StringContainer stringContainer = PackageHandler.GetTag(typeof(StringContainer), new TagHash(info.Hash));
    //     // StringBuilder text = new StringBuilder();
    //     //
    //     // Parallel.ForEach(stringContainer.Header.StringHashTable, hash =>
    //     // {
    //     //     text.Append($"{hash} : {stringContainer.GetStringFromHash(FbxIOSettings.ELanguage.English, hash)} \n");
    //     // });
    //     //
    //     // string saveDirectory = ConfigHandler.GetExportSavePath() + $"/Strings/{info.Hash}_{info.Name}/";
    //     // Directory.CreateDirectory(saveDirectory);
    //     //
    //     // File.WriteAllText(saveDirectory + "strings.txt", text.ToString());
    //
    // }
}
