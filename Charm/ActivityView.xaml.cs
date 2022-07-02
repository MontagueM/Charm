using System.Windows.Controls;
using Field;
using Field.General;

namespace Charm;

public partial class ActivityView : UserControl
{
    private Activity _activity;
    
    public ActivityView()
    {
        InitializeComponent();
    }

    public string LoadActivity(TagHash hash)
    {
        _activity = new Activity(hash);
        return PackageHandler.GetActivityName(hash);
    }
}