using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

    public static HashSet<IListItem> GetListItems(short packageId) => IViewModel.GetListItemsInternal<TextureListItemModel, Texture>(packageId);
}

public class TextureListItemModel : HashListItemModel
{
    private BitmapImage? _imageSource;
    public BitmapImage? ImageSource
    {
        get { return _imageSource; }
        set { SetField(ref _imageSource, value); }
    }

    // todo make this async call
    // todo remove typeName
    public TextureListItemModel(FileHash textureHash, string typeName) : base(textureHash, typeName)
    {
        Task.Run(() =>
        {
            // Texture texture = FileResourcer.Get().GetFile<Texture>(textureHash);
            // Load2(texture);
        });
    }

    public TextureListItemModel(Texture texture) : base(texture.Hash, "Texture")
    {
        // Task.Run(() => Load2(texture));
    }

    public TextureListItemModel()
    {
    }

    public void Load2()
    {
        Texture texture = FileResourcer.Get().GetFile<Texture>(Hash);
        Load2(texture);
    }

    ~TextureListItemModel()
    {
        Unload();
    }

    public void Load2(Texture texture)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = texture.GetTexture();
        bitmapImage.CacheOption = BitmapCacheOption.OnDemand;

        bitmapImage.DecodePixelWidth = texture.TagData.Width;
        bitmapImage.DecodePixelHeight = texture.TagData.Height;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        // dispatcher invoke?
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

    public void Unload()
    {
        ImageSource?.StreamSource.Dispose();
    }
}
