using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tiger;

namespace Charm;

public partial class ActivityDialogueView
{
    public ActivityDialogueView()
    {
        InitializeComponent();
    }

    public void LoadUI(FileHash fileHash)
    {
        TagList.LoadContent(ETagListType.DialogueList, fileHash, true);
    }
}
