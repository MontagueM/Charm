using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Charm;

public partial class TagView : UserControl
{
    public TagView()
    {
        InitializeComponent();
    }

    public enum EViewerType
    {
        [Description("Entity")]
        Entity,
        [Description("Static")]
        Static,
        [Description("Texture2D")]
        Texture2D,
        [Description("TextureCube")]
        TextureCube,
        [Description("Dialogue")]
        Dialogue,
        [Description("Directive")]
        Directive,
        [Description("Music")]
        Music,
        [Description("TagList")]
        TagList,
        [Description("Material")]
        Material,
    }

    public void SetViewer(EViewerType eViewerType)
    {
        EntityControl.Visibility = eViewerType == EViewerType.Entity ? Visibility.Visible : Visibility.Hidden;
        // ActivityControl.Visibility = eViewerType == EViewerType.Activity ? Visibility.Visible : Visibility.Hidden;
        StaticControl.Visibility = eViewerType == EViewerType.Static ? Visibility.Visible : Visibility.Hidden;
        TextureControl.Visibility = eViewerType == EViewerType.Texture2D ? Visibility.Visible : Visibility.Hidden;
        CubemapControl.Visibility = eViewerType == EViewerType.TextureCube ? Visibility.Visible : Visibility.Hidden;
        DialogueControl.Visibility = eViewerType == EViewerType.Dialogue ? Visibility.Visible : Visibility.Hidden;
        DirectiveControl.Visibility = eViewerType == EViewerType.Directive ? Visibility.Visible : Visibility.Hidden;
        MusicControl.Visibility = eViewerType == EViewerType.Music ? Visibility.Visible : Visibility.Hidden;
        TagListControl.Visibility = eViewerType == EViewerType.TagList ? Visibility.Visible : Visibility.Hidden;
        MaterialControl.Visibility = eViewerType == EViewerType.Material ? Visibility.Visible : Visibility.Hidden;
        ExportControl.Visibility = Visibility.Visible;  // always see unless we dont want to
        MusicPlayer.Visibility = Visibility.Hidden;  // always hidden unless specifically required
    }
}
