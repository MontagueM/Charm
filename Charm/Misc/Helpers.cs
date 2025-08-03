using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Investment;
using static Charm.CategoryView;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Point = System.Windows.Point;

namespace Charm;

public class CharmUIElement : INotifyPropertyChanged
{
    public bool IsPlaceholder { get; set; } = false;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    // Not ideal but useful for triggering something like an animation for just this element if it gets added
    // to an ItemPage or something. Should be set to false asap though, such as Loaded event
    private bool _isNewlyAdded = false;
    public bool IsNewlyAdded
    {
        get => _isNewlyAdded;
        set
        {
            if (_isNewlyAdded != value)
            {
                _isNewlyAdded = value;
                OnPropertyChanged(nameof(IsNewlyAdded));
            }
        }
    }

    public int Index { get; set; }
    public dynamic Tag { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public static class ApiImageUtils
{
    public static BitmapImage MakeBitmapImage(UnmanagedMemoryStream ms, int width, int height)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = ms;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.DecodePixelWidth = width;
        bitmapImage.DecodePixelHeight = height;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    public static BitmapImage MakeBitmapImage(MemoryStream ms, int width, int height)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = ms;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.DecodePixelWidth = width;
        bitmapImage.DecodePixelHeight = height;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    public static DrawingImage MakeFullIcon(InventoryItem item)
    {
        bool isD1Ornament = false;
        if (Strategy.IsD1() && item.IsArmorOrnament && item.Parent != null) // ew
        {
            item = item.Parent;
            isD1Ornament = true;
        }

        var group = new DrawingGroup();

        // streams
        UnmanagedMemoryStream? bgStream = item.GetIconBackgroundStream();
        UnmanagedMemoryStream? bgOverlayStream = item.GetIconBackgroundOverlayStream();
        UnmanagedMemoryStream? primaryStream = item.GetIconPrimaryStream();
        UnmanagedMemoryStream? overlayStream = item.GetIconOverlayStream();

        // Main background (rarity color)
        BitmapImage? bg = bgStream != null ? MakeBitmapImage(bgStream, 96, 96) : null;
        group.Children.Add(new ImageDrawing(bg, new Rect(0, 0, 96, 96)));

        // Background overlay (ornament, shiny, etc.)
        // Most if not all legendary armor will use the ornament overlay because of transmog (I assume)
        BitmapImage? bgOverlay = bgOverlayStream != null && !item.IsArmor ? MakeBitmapImage(bgOverlayStream, 96, 96) : null;
        if (!Strategy.IsD1())
            group.Children.Add(new ImageDrawing(bgOverlay, new Rect(0, 0, 96, 96)));

        // For D1 Age Of Triumph ornaments
        if (isD1Ornament)
        {
            bgOverlay = MakeBitmapImage(Texture.GetTextureFromHash(new(0x80A63BAA)), 96, 96);
            var bgOverlayNew = ChangeOpacity(bgOverlay, 0.5f);
            group.Children.Add(new ImageDrawing(bgOverlayNew, new Rect(0, 0, 96, 96)));
        }

        // The main icon
        BitmapImage? primary = primaryStream != null ? MakeBitmapImage(primaryStream, 96, 96) : null;
        if (bgOverlayStream != null && Strategy.IsD1()) // D1 Icon dyes
            primary = MakeDyedIcon(item);

        group.Children.Add(new ImageDrawing(primary, new Rect(0, 0, 96, 96)));

        // Overlay (watermark, masterwork, etc.)
        int wh = item.GetIconOverlayTexture()?.Width ?? 96;
        if (overlayStream != null && wh == 96) // Actual full overlay, not the crappy new watermarks
        {
            BitmapImage? overlay = MakeBitmapImage(overlayStream, wh, wh);
            group.Children.Add(new ImageDrawing(overlay, new Rect(0, 0, wh, wh)));

            // Tints the watermark overlay blue for D1 ornaments (just to distinguish them)
            if (isD1Ornament)
            {
                var overlayTinted = TintImage(overlay, Color.FromArgb(255, 0, 200, 255));
                group.Children.Add(new ImageDrawing(overlayTinted, new Rect(0, 0, 96, 96)));
            }
        }

        // Crafted overlay for patterns
        if (!Strategy.IsD1() && item.TagData.Unk10.GetValue(item.GetReader()) is S49298080)
        {
            var craftedOverlay = MakeBitmapImage(Texture.GetTextureFromHash(new(Strategy.IsLatest() ? 0x80A9F577 : 0x80E55268)), 96, 96);
            group.Children.Add(new ImageDrawing(craftedOverlay, new Rect(0, 0, 96, 96)));
        }

        var dw = new DrawingImage(group);
        dw.Freeze();

        return dw;
    }

    public static DrawingImage MakeFoundryBanner(InventoryItem item)
    {
        UnmanagedMemoryStream? foundryStream = item.GetFoundryIconStream();
        BitmapImage? foundry = foundryStream != null ? MakeBitmapImage(foundryStream, 596, 596) : null;

        var group = new DrawingGroup();
        group.Children.Add(new ImageDrawing(foundry, new Rect(0, 0, 596, 596)));

        var dw = new DrawingImage(group);
        dw.Freeze();

        return dw;
    }

    public static ImageSource GetPlugWatermark(InventoryItem item)
    {
        UnmanagedMemoryStream? overlayStream = item.GetIconOverlayStream(1);
        BitmapImage? overlay = overlayStream != null ? MakeBitmapImage(overlayStream, 96, 96) : null;
        var dw = new ImageBrush(overlay);
        dw.Freeze();
        return dw.ImageSource;
    }

    public static ImageSource MakeIcon(FileHash textureHash)
    {
        var texture = FileResourcer.Get().GetFile<Texture>(textureHash);

        if (texture == null)
            return null;

        BitmapImage? primary = MakeBitmapImage(texture.GetTexture(), texture.TagData.Width, texture.TagData.Height);

        var dw = new ImageBrush(primary);
        dw.Freeze();

        return dw.ImageSource;
    }

    public static ImageSource MakeIcon(int index, int containerIndex = 0, int iconIndex = 0, int listIndex = 0)
    {
        Tag<SB83E8080>? container = Investment.Get().GetItemIconContainer(index);
        //Console.WriteLine($"container {container.Hash}");
        if (container == null)
            return null;

        List<Tag<SCF3E8080>> containers = new()
        {
            container.TagData.IconPrimaryContainer,
            container.TagData.IconAdContainer,
            container.TagData.IconBGOverlayContainer,
            container.TagData.IconBackgroundContainer,
            container.TagData.IconOverlayContainer,
            container.TagData.IconSpecialContainer
        };
        if (containers[containerIndex] is null)
            return null;

        Texture? texture = GetTexture(containers[containerIndex], iconIndex, listIndex);
        UnmanagedMemoryStream? primaryStream = texture?.GetTexture();
        BitmapImage? primary = primaryStream != null ? MakeBitmapImage(primaryStream, texture.TagData.Width, texture.TagData.Height) : null;

        var dw = new ImageBrush(primary);
        dw.Freeze();

        return dw.ImageSource;
    }

    public static Texture? GetTexture(Tag<SCF3E8080> iconContainer, int texIndex = 0, int listIndex = 0)
    {
        using TigerReader reader = iconContainer.GetReader();
        dynamic? prim = iconContainer.TagData.Unk10.GetValue(reader);
        if (prim is SCD3E8080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (listIndex >= structCD3E8080.Unk00.Count || texIndex >= structCD3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;
            return structCD3E8080.Unk00[reader, listIndex].TextureList[reader, texIndex].IconTexture;
        }
        if (prim is SCB3E8080 structCB3E8080)
        {
            if (listIndex >= structCB3E8080.Unk00.Count || texIndex >= structCB3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;
            return structCB3E8080.Unk00[reader, listIndex].TextureList[reader, texIndex].IconTexture;
        }
        return null;
    }

    public static BitmapImage MakeDyedIcon(InventoryItem item)
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(item);
        UnmanagedMemoryStream? primaryStream = item.GetIconPrimaryStream();
        UnmanagedMemoryStream? maskStream = item.GetIconBackgroundOverlayStream();

        Bitmap mainImage = primaryStream != null ? MakeBitmap(primaryStream) : null;
        Bitmap colorMaskImage = maskStream != null ? MakeBitmap(maskStream) : null;
        if (mainImage is null || colorMaskImage is null)
            return Bitmap2BitmapImage(mainImage, 96, 96);

        // both mask and main have to be the same size
        if (iconContainer.TagData.IconBGOverlayContainer is not null && (GetTexture(iconContainer.TagData.IconBGOverlayContainer).TagData.Height < GetTexture(iconContainer.TagData.IconPrimaryContainer).TagData.Height))
            colorMaskImage = MakeBitmap(maskStream, GetTexture(iconContainer.TagData.IconPrimaryContainer).TagData.Height);

        // Define RGB colors
        System.Drawing.Color[] overlayColors = new System.Drawing.Color[]
        {
            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorR.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorR.Z, 0.5) * 255)),   // Red channel overlay color

            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorG.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorG.Z, 0.5) * 255)),   // Green channel overlay color

            System.Drawing.Color.FromArgb((byte)(iconContainer.TagData.DyeColorB.W * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.X, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.Y, 0.5) * 255),
            (byte)(Math.Pow(iconContainer.TagData.DyeColorB.Z, 0.5) * 255))    // Blue channel overlay color
        };


        // Apply color from color mask
        int width = mainImage.Width;
        int height = mainImage.Height;

        // Iterate over each pixel in the color mask and apply color to the main image
        for (int y = 0; y < height; y++)
        {
            //Console.WriteLine($"H {y} : {height}");
            for (int x = 0; x < width; x++)
            {
                // Get color mask pixel color
                System.Drawing.Color maskColor = colorMaskImage.GetPixel(x, y);

                // Get main image pixel color
                System.Drawing.Color mainColor = mainImage.GetPixel(x, y);
                System.Drawing.Color blendedColor = System.Drawing.Color.FromArgb(mainColor.A, 0, 0, 0);

                // Mask R
                blendedColor = ColorUtility.BlendColors(mainColor, overlayColors[0], maskColor.R);
                // Mask G
                blendedColor = ColorUtility.AddColors(blendedColor, ColorUtility.BlendColors(mainColor, overlayColors[1], maskColor.G));
                // Mask B
                blendedColor = ColorUtility.AddColors(blendedColor, ColorUtility.BlendColors(mainColor, overlayColors[2], maskColor.B));

                // Set the modified pixel color
                if (!blendedColor.IsZero())
                    mainImage.SetPixel(x, y, blendedColor);
            }
        }

        return Bitmap2BitmapImage(mainImage, 96, 96);
    }

    private static Bitmap MakeBitmap(UnmanagedMemoryStream stream, int wH = 0)
    {
        using (var memoryStream = new System.IO.MemoryStream())
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(stream));
            encoder.Save(memoryStream);

            if (wH == 0)
                return new Bitmap(memoryStream);
            else
            {
                Bitmap originalBitmap = new(memoryStream);
                Bitmap resizedBitmap = new(wH, wH);

                using (Graphics graphics = Graphics.FromImage(resizedBitmap))
                {
                    graphics.DrawImage(originalBitmap, 0, 0, wH, wH);
                }
                return resizedBitmap;
            }
        }
    }

    public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap, int width, int height)
    {
        using (MemoryStream memoryStream = new())
        {
            // Save bitmap to memory stream as PNG (to preserve alpha channel)
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;

            // Create new BitmapImage and load it from memory stream
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.DecodePixelWidth = width;
            bitmapImage.DecodePixelHeight = height;
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze the BitmapImage to make it immutable

            return bitmapImage;
        }
    }

    public static WriteableBitmap ChangeOpacity(BitmapSource source, double opacity)
    {
        if (opacity < 0 || opacity > 1)
            throw new ArgumentOutOfRangeException(nameof(opacity), "Opacity must be between 0 and 1.");

        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;

        byte[] pixelData = new byte[height * stride];
        source.CopyPixels(pixelData, stride, 0);

        for (int i = 0; i < pixelData.Length; i += 4)
        {
            // pixelData[i+3] is the alpha channel
            pixelData[i + 3] = (byte)(pixelData[i + 3] * opacity);
        }

        WriteableBitmap writeable = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        writeable.WritePixels(new Int32Rect(0, 0, width, height), pixelData, stride, 0);

        return writeable;
    }

    public static WriteableBitmap TintImage(BitmapSource source, Color tintColor)
    {
        int width = source.PixelWidth;
        int height = source.PixelHeight;
        int stride = width * 4;

        byte[] pixels = new byte[height * stride];
        source.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            byte b = pixels[i];
            byte g = pixels[i + 1];
            byte r = pixels[i + 2];
            byte a = pixels[i + 3];

            // Multiply original RGB by tint color
            pixels[i] = (byte)((b * tintColor.B) / 255); // Blue
            pixels[i + 1] = (byte)((g * tintColor.G) / 255); // Green
            pixels[i + 2] = (byte)((r * tintColor.R) / 255); // Red
            pixels[i + 3] = a; // Preserve original alpha
        }

        WriteableBitmap dyed = new WriteableBitmap(width, height, source.DpiX, source.DpiY, PixelFormats.Bgra32, null);
        dyed.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);
        return dyed;
    }
}


public static class StyleHelper
{
    #region Borders
    // BorderThickness attached property
    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.RegisterAttached(
            "BorderThickness",
            typeof(Thickness),
            typeof(StyleHelper),
            new PropertyMetadata(new Thickness(1))); // Default thickness

    public static void SetBorderThickness(UIElement element, Thickness value)
    {
        element.SetValue(BorderThicknessProperty, value);
    }

    public static Thickness GetBorderThickness(UIElement element)
    {
        return (Thickness)element.GetValue(BorderThicknessProperty);
    }

    // BorderThickness attached property
    public static readonly DependencyProperty MarginProperty =
        DependencyProperty.RegisterAttached(
            "Margin",
            typeof(Thickness),
            typeof(StyleHelper),
            new PropertyMetadata(new Thickness(1))); // Default

    public static void SetMargin(UIElement element, Thickness value)
    {
        element.SetValue(MarginProperty, value);
    }

    public static Thickness GetMargin(UIElement element)
    {
        return (Thickness)element.GetValue(MarginProperty);
    }
    #endregion

    #region Corners
    // CornerRadius attached property
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(StyleHelper),
            new PropertyMetadata(new CornerRadius(0))); // Default

    public static void SetCornerRadius(UIElement element, CornerRadius value)
    {
        element.SetValue(CornerRadiusProperty, value);
    }

    public static CornerRadius GetCornerRadius(UIElement element)
    {
        return (CornerRadius)element.GetValue(CornerRadiusProperty);
    }
    #endregion

    #region Colors
    // BackgroundColor attached property
    public static readonly DependencyProperty BackgroundColorProperty =
        DependencyProperty.RegisterAttached(
            "BackgroundColor",
            typeof(Brush),
            typeof(StyleHelper),
            new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#433C3C41")))); // Default background color

    public static void SetBackgroundColor(UIElement element, Brush value)
    {
        element.SetValue(BackgroundColorProperty, value);
    }

    public static Brush GetBackgroundColor(UIElement element)
    {
        return (Brush)element.GetValue(BackgroundColorProperty);
    }
    #endregion
}

public static class UIHelper
{
    /// <summary>
    /// Animates the opacity of a UI element from a starting value to a target value.
    /// </summary>
    public static void AnimateFade(
        UIElement obj,
        float seconds,
        float to = 1,
        float from = 0,
        EventHandler? completed = null,
        bool autoReverse = false,
        bool additive = false,
        IEasingFunction? easing = null)
    {
        if (additive && obj.Opacity != (double)from)
            from = (float)obj.Opacity;

        obj.Opacity = from;

        Dispatcher.CurrentDispatcher.BeginInvoke(() =>
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = easing ?? new QuadraticEase { EasingMode = EasingMode.EaseOut },
                AutoReverse = autoReverse,
            };
            if (completed is not null)
                animation.Completed += completed;

            obj.BeginAnimation(UIElement.OpacityProperty, animation);

        }, DispatcherPriority.Render);
    }

    public static void AnimateSlide(UIElement obj, float seconds, Point to, Point from,
        bool autoReverse = false, IEasingFunction easing = null, EventHandler? completed = null)
    {
        var group = EnsureTransformGroup(obj);
        var translate = GetOrAddTransform<TranslateTransform>(group);

        // Set initial position before animation to avoid a flash
        translate.X = from.X;
        translate.Y = from.Y;

        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
        {
            // Animate X
            var animX = new DoubleAnimation
            {
                From = from.X,
                To = to.X,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = easing is null ? new QuadraticEase { EasingMode = EasingMode.EaseOut } : easing,
                AutoReverse = autoReverse,
            };
            if (completed is not null)
                animX.Completed += completed;

            // Animate Y
            var animY = new DoubleAnimation
            {
                From = from.Y,
                To = to.Y,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = easing is null ? new QuadraticEase { EasingMode = EasingMode.EaseOut } : easing,
                AutoReverse = autoReverse,
            };

            translate.BeginAnimation(TranslateTransform.XProperty, animX);
            translate.BeginAnimation(TranslateTransform.YProperty, animY);

        }), DispatcherPriority.Render);
    }

    public static void AnimateScale(UIElement obj, float seconds, Point to, Point from, bool autoReverse = false, EventHandler? completed = null)
    {
        var group = EnsureTransformGroup(obj);
        var scale = GetOrAddTransform<ScaleTransform>(group);

        // Set initial scale before animation to avoid a flash
        scale.ScaleX = from.X;
        scale.ScaleY = from.Y;

        Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
        {
            // Animate X
            var animX = new DoubleAnimation
            {
                From = from.X,
                To = to.X,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                AutoReverse = autoReverse,
            };
            if (completed is not null)
                animX.Completed += completed;

            // Animate Y
            var animY = new DoubleAnimation
            {
                From = from.Y,
                To = to.Y,
                Duration = TimeSpan.FromSeconds(seconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut },
                AutoReverse = autoReverse,
            };

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, animY);

        }), DispatcherPriority.Render);
    }

    public static TransformGroup EnsureTransformGroup(UIElement obj)
    {
        if (obj.RenderTransform is not TransformGroup group)
        {
            group = new TransformGroup();
            obj.RenderTransform = group;
        }

        return group;
    }

    public static T GetOrAddTransform<T>(TransformGroup group) where T : Transform, new()
    {
        var transform = group.Children.OfType<T>().FirstOrDefault();
        if (transform == null)
        {
            transform = new T();
            group.Children.Add(transform);
        }

        return transform;
    }

    public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                return t;
            }
            T result = FindVisualChild<T>(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T t)
                yield return t;

            foreach (T childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }

    public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

            T result = (child as T) ?? GetChildOfType<T>(child);
            if (result != null) return result;
        }
        return null;
    }

    public static List<T> GetChildrenOfType<T>(DependencyObject depObj) where T : DependencyObject
    {
        var children = new List<T>();
        if (depObj == null) return children;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T)
            {
                children.Add(child as T);
            }
            else
            {
                children.AddRange(GetChildrenOfType<T>(child));
            }
        }
        return children;
    }

    /// <summary>
    /// Returns the parent at the specified depth from the given element.
    /// Depth 1 is the immediate parent, 2 is the grandparent, etc.
    /// Returns null if the chain is not that deep.
    /// </summary>
    public static DependencyObject GetParentAtDepth(DependencyObject element, int depth)
    {
        if (element == null || depth < 1)
            return null;

        DependencyObject current = element;

        for (int i = 0; i < depth; i++)
        {
            current = VisualTreeHelper.GetParent(current);
            if (current == null)
                return null;
        }

        return current;
    }

    public static FrameworkElement FindElementWithDataType(DependencyObject start, Type targetType)
    {
        DependencyObject current = start;
        while (current != null)
        {
            if (current is FrameworkElement fe && targetType.IsInstanceOfType(fe.DataContext))
            {
                return fe;
            }
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    public static string AddSpacesBetweenChars(string input, int spaces)
    {
        if (string.IsNullOrEmpty(input) || spaces < 0)
            return input;

        // the space is actually a fake space char to allow whole word wrapping in xaml
        string spacer = new string(' ', spaces * 2);
        var result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            result.Append(input[i]);

            // Don't add spaces after the last character
            if (i < input.Length - 1)
            {
                if (input[i] != ' ') // real space
                    result.Append(spacer);
            }
        }

        return result.ToString();
    }

    public static void SelectRadioButton(ItemsControl itemsControl, int index)
    {
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            if (index < 0 || index >= itemsControl.Items.Count)
                return;

            object item = itemsControl.Items[index];
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
            {
                RadioButton radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }), DispatcherPriority.Background);
    }

    public static void UnselectAllRadioButtons(ItemsControl itemsControl)
    {
        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (object? item in itemsControl.Items)
            {
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
                {
                    RadioButton radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                    if (radioButton != null)
                    {
                        radioButton.IsChecked = false;
                    }
                }
            }
        }));
    }

    public static SolidColorBrush Vec4ToBrush(Vector4 vec)
    {
        return new SolidColorBrush(Color.FromArgb(
            (byte)(vec.W * 255),
            (byte)(vec.X * 255),
            (byte)(vec.Y * 255),
            (byte)(vec.Z * 255)));
    }

    public static Color Vec4ToColor(Vector4 vec)
    {
        return Color.FromArgb(
            (byte)(vec.W * 255),
            (byte)(vec.X * 255),
            (byte)(vec.Y * 255),
            (byte)(vec.Z * 255));
    }

    public static Color Divide(this Color color, float divisor, bool dAlpha = false)
    {
        if (divisor == 0)
            throw new DivideByZeroException("Cannot divide color components by zero.");

        byte DivideComponent(byte component) =>
            (byte)Math.Clamp(component / divisor, 0, 255);

        return Color.FromArgb(
            dAlpha ? DivideComponent(color.A) : color.A,
            DivideComponent(color.R),
            DivideComponent(color.G),
            DivideComponent(color.B));
    }
}

public class CharSpacingConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string input = value as string;
        if (string.IsNullOrEmpty(input))
            return input;

        // Default spacer settings
        char spacerChar = ' '; // narrow no-break space
        int repeat = 2;

        // Parse parameter in the form "char:count", e.g. " :2" or ".:3"
        if (parameter is string paramStr)
        {
            var parts = paramStr.Split(':');
            if (parts.Length >= 1 && !string.IsNullOrEmpty(parts[0]))
                spacerChar = parts[0][0];
            if (parts.Length == 2 && int.TryParse(parts[1], out int parsedRepeat))
                repeat = Math.Max(0, parsedRepeat);
        }

        string spacer = new string(spacerChar, repeat);
        var result = new StringBuilder();

        for (int i = 0; i < input.Length; i++)
        {
            result.Append(input[i]);
            if (i < input.Length - 1 && input[i] != ' ')
                result.Append(spacer);
        }

        return result.ToString();
    }


    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class ItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate Template1 { get; set; } // Main item template
    public DataTemplate Template2 { get; set; } // Placeholder template

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        switch (item)
        {
            case CategoryEntry itemObj:
                var invItem = Investment.Get().GetInventoryItem(itemObj.ItemIndex);
                if (invItem.GetItemStrings().TagData.ItemType.Value != "Emblem")
                    return Template1;
                else
                    return Template2;

            case RewardBlock rewardBlock:
                if (rewardBlock.IsEmblem)
                    return Template2;
                else
                    return Template1;

            default:
                return Template1;
        }
        //return item is ApiItem itemObj && itemObj.IsPlaceholder ? PlaceholderTemplate : NormalItemTemplate;
    }
}

public class EnumToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return Visibility.Collapsed;

        if (Enum.IsDefined(value.GetType(), value) && Enum.TryParse(value.GetType(), parameter.ToString(), out object enumValue))
        {
            return value.Equals(enumValue) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool invert = false;
        if (parameter is not null)
            bool.TryParse(parameter.ToString(), out invert);

        bool isVisible = value != null;
        return (invert ? !isVisible : isVisible) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class TitleCaseConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string input && !string.IsNullOrWhiteSpace(input))
        {
            TextInfo textInfo = culture.TextInfo;
            return textInfo.ToTitleCase(input.ToLower());
        }

        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value;
    }
}

public class ColorToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Color color)
            return new SolidColorBrush(color);
        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DamageTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string damageType = value.ToString();

        if (string.IsNullOrEmpty(damageType) || damageType == "Kinetic")
        {
            if (parameter.ToString() == "Visibility")
                return Visibility.Collapsed;
            if (parameter.ToString() == "Text")
                return string.Empty;
            if (parameter.ToString() == "Foreground")
                return new SolidColorBrush(Colors.Transparent); // Or some default color
        }

        switch (damageType)
        {
            //case "Kinetic":
            //    if (parameter.ToString() == "Text")
            //        return "Kinetic";
            //    if (parameter.ToString() == "Foreground")
            //        return new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
            //    break;
            case "Arc":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x85, 0xc5, 0xec)); // #85c5ec
                break;
            case "Solar":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0xf2, 0x71, 0x1b)); // #f2711b
                break;
            case "Void":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0xb1, 0x84, 0xc5)); // #b184c5
                break;
            case "Stasis":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x4d, 0x88, 0xff)); // #4d88ff
                break;
            case "Strand":
                if (parameter.ToString() == "Text")
                    return "";
                if (parameter.ToString() == "Foreground")
                    return new SolidColorBrush(Color.FromRgb(0x35, 0xe3, 0x66)); // #35e366
                break;
            default:
                return DependencyProperty.UnsetValue;
        }

        if (parameter.ToString() == "Visibility")
            return Visibility.Visible;

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringToUpperConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value as string).ToUpper();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StringNullOrEmptyVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null || (string)value == "")
            return Visibility.Collapsed;

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public class StringContainsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var substring = parameter as string;

        if (value is string str && !string.IsNullOrEmpty(substring))
        {
            return str.Contains(substring);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class Investment_IsFeaturedItem : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (Strategy.IsLatest() && value is uint hash && hash != 0)
        {
            return Investment.Get().FeaturedItems.Contains(Investment.Get().GetItemIndex(hash));
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class IsCollectionEmptyToVisConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isEmpty = true;

        if (value is IEnumerable collection)
            isEmpty = !collection.GetEnumerator().MoveNext();

        if (Invert)
            isEmpty = !isEmpty;

        return isEmpty ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException("IsCollectionEmptyToVisConverter does not support ConvertBack.");
    }
}

public class TextureFromHashConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string hash || string.IsNullOrWhiteSpace(hash))
            return null;

        var texture = FileResourcer.Get().GetFile<Texture>(hash);

        if (texture == null)
            return null;

        BitmapImage? primary = ApiImageUtils.MakeBitmapImage(texture.GetTexture(), texture.TagData.Width, texture.TagData.Height);

        var dw = new ImageBrush(primary);
        dw.Freeze();

        return dw.ImageSource;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
