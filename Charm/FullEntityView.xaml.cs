using System.Windows.Controls;
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
        AnimationControl.LoadContent(ETagListType.AnimationList, entityHash, true);

        return bLoadedSuccessfully;
    }
}