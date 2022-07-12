using System.Windows.Controls;
using Field.General;

namespace Charm;

public partial class ActivityDirectiveView : UserControl
{
    public ActivityDirectiveView()
    {
        InitializeComponent();
    }
    
    public void LoadUI(TagHash tagHash)
    {
        TagList.LoadContent(ETagListType.DirectiveList, tagHash, true);
    }
}