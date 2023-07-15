using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Charm.Views;
using CommunityToolkit.Mvvm.Input;
using DirectXTex;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Assimp;
using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using Tiger;
using Tiger.Schema;
using MeshGeometry3D = HelixToolkit.SharpDX.Core.MeshGeometry3D;
using PerspectiveCamera = HelixToolkit.Wpf.SharpDX.PerspectiveCamera;

namespace Charm.Objects;

// need a nice way of storing <Texture> while keeping it type safe (just make it one type to inherit from)
public class StaticViewModel : GenericListViewModel<StaticMesh>, IViewModel<StaticMesh>, IModel<StaticMesh>
{
    public override HashSet<HashListItemModel> GetAllItems(StaticMesh data)
    {
        return new HashSet<HashListItemModel>{new StaticListItemModel(data)};
    }

    // todo this is probably overkill for many types that will only ever have a single view, maybe?
    public static UserControl GetView(StaticMesh data)
    {
        UserControl view = new ModelView();
        view.DataContext = new StaticListItemModel(data);

        return view;
    }

    public static UserControl? DefaultView { get; } = new ModelView();


    public static HashSet<IListItem> GetListItems(short packageId) => IViewModel.GetListItemsInternal<StaticListItemModel, StaticMesh>(packageId);
}

public class StaticListItemModel : ModelViewModel
{
    private BitmapImage? _thumbnailImageSource;
    public BitmapImage? ThumbnailImageSource
    {
        get { return _thumbnailImageSource; }
        set { SetField(ref _thumbnailImageSource, value); }
    }

    public static Viewport3DX? Viewport { get; set; }

    // todo make this async call
    // todo remove typeName
    public StaticListItemModel(FileHash textureHash, string typeName) : base(textureHash, typeName)
    {
    }

    public StaticListItemModel(StaticMesh staticMesh) : base(staticMesh.Hash, "Static Mesh")
    {
        Initialise();
    }

    public StaticListItemModel()
    {
        Initialise();
    }

    // todo need to support optional difference between grid and list view, eg thumbnail generation
    public override void Load(dynamic? data = null, UserControl? control = null)
    {
        StaticMesh staticMesh;
        if (data == null || data is not StaticMesh)
        {
            if (Hash.IsInvalid())
            {
                return;
            }
            staticMesh = FileResourcer.Get().GetFile<StaticMesh>(Hash);
        }
        else
        {
            staticMesh = data;
        }

        Load(staticMesh, (control as ModelView)?.Viewport);
    }

    private void Load(StaticMesh staticMesh, Viewport3DX viewport)
    {
        // List<StaticPart> staticParts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        // Element3DPresenter element3DPresenter = new();
        // SceneNodeGroupModel3D sceneNodeGroupModel3D = DrawParts(staticParts);
        // element3DPresenter.Content = sceneNodeGroupModel3D;
        // viewport.Items.Add(element3DPresenter);
        Viewport.Width = 512;
        Viewport.Height = 512;
        ClearRemoteViewport(Viewport);
        List<StaticPart> staticParts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        DrawPartsToRemoteViewport(staticParts, Viewport);
        BitmapSource? bitmapSource = Viewport?.RenderBitmap();
        ThumbnailImageSource = bitmapSource as BitmapImage;
        return;
        // List<StaticPart> staticParts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        // DrawParts(staticParts);
        // Title = staticMesh.Hash;
        // SubTitle = staticParts.Select(part => part.Indices.Count).Sum() + " triangles";
    }

    public override void Unload()
    {
        Clear();
    }
}
