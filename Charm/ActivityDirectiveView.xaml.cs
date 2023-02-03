using System.Windows.Controls;
using Tiger.General;

namespace Charm;

public partial class ActivityDirectiveView : UserControl
{
    public ActivityDirectiveView()
    {
        InitializeComponent();
    }
    
    public void LoadUI(TagHash activityHash)
    {
        TagList.LoadContent(ETagListType.DirectiveList, activityHash, true);
    }
}