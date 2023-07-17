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
using ConcurrentCollections;
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
using Vector3 = Tiger.Schema.Vector3;
using Vector4 = Tiger.Schema.Vector4;

namespace Charm.Objects;

public enum LoadType
{
    Minimal,
    Full
}

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

    // public static UserControl? DefaultView { get; } = new ModelView();


    public static async Task<HashSet<IListItem>> GetListItems(short packageId) => await IViewModel.GetListItemsInternal<StaticListItemModel, StaticMesh>(packageId);
}

public class StaticListItemModel : ModelViewModel
{
    private string _triangleCount;
    public string TriangleCount
    {
        get
        {
            return _triangleCount;
        }

        private set
        {
            SetField(ref _triangleCount, value);
        }
    }

    private string _materialCount;
    public string MaterialCount
    {
        get
        {
            return _materialCount;
        }

        private set
        {
            SetField(ref _materialCount, value);
        }
    }

    private string _partCount;
    public string PartCount
    {
        get
        {
            return _partCount;
        }

        private set
        {
            SetField(ref _partCount, value);
        }
    }

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
    public override void Load(LoadType loadType, dynamic? data = null)
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

        Load(staticMesh, loadType);
    }

    private async void Load(StaticMesh staticMesh, LoadType loadType)
    {
        // List<StaticPart> staticParts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        // Element3DPresenter element3DPresenter = new();
        // SceneNodeGroupModel3D sceneNodeGroupModel3D = DrawParts(staticParts);
        // element3DPresenter.Content = sceneNodeGroupModel3D;
        // viewport.Items.Add(element3DPresenter);
        // Viewport.Width = 512;
        // Viewport.Height = 512;
        // ClearRemoteViewport(Viewport);
        // List<StaticPart> staticParts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        // DrawPartsToRemoteViewport(staticParts, Viewport);
        // BitmapSource? bitmapSource = Viewport?.RenderBitmap();
        // ThumbnailImageSource = bitmapSource as BitmapImage;
        // return;
        List<StaticPart> staticParts = await staticMesh.LoadAsync(ExportDetailLevel.MostDetailed);

        if (loadType == LoadType.Full)
        {
            AddPartsToViewport(MakeDisplayParts(staticParts));
        }

        TriangleCount = $"{staticParts.Select(part => part.Indices.Count).Sum()} triangles";
        MaterialCount = $"{staticParts.Select(part => part.Material).Distinct().Count()} materials";
        PartCount = $"{staticParts.Count} parts";
        Title = staticMesh.Hash;

        SubTitle = TriangleCount;
    }

    public override void Unload(LoadType loadType)
    {
        if (loadType == LoadType.Full)
        {
            Clear();
        }
    }

    private List<DisplayPart> MakeDisplayParts(List<StaticPart> meshParts)
    {
        List<DisplayPart> displayParts = new();
        foreach (StaticPart part in meshParts)
        {
            DisplayPart displayPart = new(part);
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Quaternion);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts;
    }
}
