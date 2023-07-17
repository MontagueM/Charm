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
using System.Windows.Threading;
using Charm.Views;
using DirectXTex;
using Tiger;
using Tiger.Schema;

namespace Charm.Objects;

// need a nice way of storing <Texture> while keeping it type safe (just make it one type to inherit from)
public class TextureViewModel : GenericListViewModel<Texture>, IViewModel<Texture>, IModel<Texture>
{
    public override HashSet<HashListItemModel> GetAllItems(Texture data)
    {
        return new HashSet<HashListItemModel>{new TextureListItemModel(data)};
    }

    // todo this is probably overkill for many types that will only ever have a single view, maybe?
    public static UserControl GetView(Texture data)
    {
        UserControl view;

        if (data.IsCubemap())
        {
            view = new TextureCubemapView();
        }
        else if (data.IsVolume())
        {
            view = new TextureVolumeView();
            // throw new NotImplementedException();
        }
        else
        {
            view = new Texture2DView();
        }

        view.DataContext = new TextureListItemModel(data);

        return view;
    }

    // public static UserControl? DefaultView { get; } = null;

    public static async Task<HashSet<IListItem>> GetListItems(short packageId) => await IViewModel.GetListItemsInternal<TextureListItemModel, Texture>(packageId);
}

public class TextureListItemModel : HashListItemModel
{
    private BitmapImage? _imageSource;
    public BitmapImage? ImageSource
    {
        get { return _imageSource; }
        set { SetField(ref _imageSource, value); }
    }

    private CancellationToken _cancellationToken = CancellationToken.None;

    private Stream _stream;

    // todo make this async call
    // todo remove typeName
    public TextureListItemModel(FileHash textureHash, string typeName) : base(textureHash, typeName)
    {
    }

    public TextureListItemModel(Texture texture) : base(texture.Hash, "Texture")
    {
    }

    public TextureListItemModel()
    {
    }

    public override void Load(LoadType loadType, dynamic? data = null)
    {
        Texture texture;
        if (data == null || data is not Texture)
        {
            if (Hash.IsInvalid())
            {
                return;
            }
            texture = FileResourcer.Get().GetFile<Texture>(Hash);
        }
        else
        {
            texture = data;
        }

        Load(texture, loadType);
    }

    private void Load(Texture texture, LoadType loadType)
    {
        BitmapImage bitmapImage = new();

        using (UnmanagedMemoryStream stream = texture.GetTexture())
        {
            bitmapImage.BeginInit();

            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;

            bitmapImage.DecodePixelWidth = texture.TagData.Width;
            bitmapImage.DecodePixelHeight = texture.TagData.Height;

            bitmapImage.EndInit();
        }

        bitmapImage.Freeze();

        ImageSource = bitmapImage;

        string srgb = texture.IsSrgb() ? "sRGB" : "Linear";
        string type = "2D";
        if (texture.IsCubemap())
        {
            type = "Cubemap";
        }
        else if (texture.IsVolume())
        {
            type = "Volume";
        }
        Type = $"{texture.TagData.Width}x{texture.TagData.Height}, {((DirectXTexUtility.DXGIFormat)texture.TagData.Format).ToString()}, {srgb}, {type}";
    }

    public override void Unload(LoadType loadType)
    {
        ImageSource = null;

        // GC.Collect();
    }
}
