using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class ActivityMusicView : UserControl
{
    public ActivityMusicView()
    {
        InitializeComponent();
    }

    // Activity only has one music table ever so no taglist
    public void LoadUI(FileHash activityHash)
    {
        TagList.LoadContent(ETagListType.MusicList, activityHash, true);
    }
}
