using System.Windows;
using System.Windows.Controls;
using Field.Entities;
using Field.General;
using Field.Models;

namespace Charm;

public partial class FullEntityView : UserControl
{
    public FullEntityView()
    {
        InitializeComponent();
    }

    public bool LoadEntity(TagHash entityHash, FbxHandler fbxHandler)
    {
        bool bLoadedSuccessfully = true;
        
        // Load basic geometry
        bLoadedSuccessfully &= GeometryControl.LoadEntity(entityHash, fbxHandler, true);

        // Load geom for animation screen + the animation list
        Entity entity = PackageHandler.GetTag(typeof(Entity), entityHash);
        if (entity.AnimationGroup != null)
            AnimationControl.LoadContent(ETagListType.AnimationList, entityHash, true);
        else
        {
            AnimationTab.IsSelected = false;
            AnimationTab.Visibility = Visibility.Hidden;
            GeometryTab.IsSelected = true;
        }
        
        return bLoadedSuccessfully;
    }
}