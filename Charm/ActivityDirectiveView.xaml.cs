using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class ActivityDirectiveView : UserControl
{
    public ActivityDirectiveView()
    {
        InitializeComponent();
    }

    public void LoadUI(FileHash activityHash)
    {
        TagList.LoadContent(ETagListType.DirectiveList, activityHash, true);
    }
}
