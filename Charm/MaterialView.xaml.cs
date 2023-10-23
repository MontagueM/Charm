using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DirectXTexNet;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

namespace Charm;

public partial class MaterialView : UserControl
{
    private static MainWindow _mainWindow = null;
    public MaterialView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    public void Load(FileHash hash)
    {
        IMaterial material = FileResourcer.Get().GetFileInterface<IMaterial>(hash);

        if (material is null)
            return;

        UnkDataList.ItemsSource = GetUnkDataDetails(material);

        if (material.VertexShader is not null)
        {
            VertexShader.Text = material.Decompile(material.VertexShader.GetBytecode(), $"ps{material.VertexShader.Hash}");
            VS_CBufferList.ItemsSource = GetCBufferDetails(material, true);
        }

        if (material.PixelShader is not null)
        {
            TextureListView.ItemsSource = GetTextureDetails(material);
            PixelShader.Text = material.Decompile(material.PixelShader.GetBytecode(), $"ps{material.PixelShader.Hash}");
            PS_CBufferList.ItemsSource = GetCBufferDetails(material); 
        }
    }

    public List<TextureDetail> GetTextureDetails(IMaterial material)
    {
        var items = new List<TextureDetail>();

        foreach (var tex in material.EnumerateVSTextures())
        {
            if (tex.Texture is null)
                continue;

            items.Add(new TextureDetail
            {
                Shader = "Vertex Shader",
                Hash = $"{tex.Texture.Hash}",
                Index = $"Index: {tex.TextureIndex}",
                Type = $"Colorspace: {(tex.Texture.IsSrgb() ? "Srgb" : "Non-Color")}",
                Dimension = $"Dimension: {tex.Texture.GetDimension()}",
                Format = $"Format: {(DXGI_FORMAT)tex.Texture.TagData.Format}",
                Dimensions = $"Texture Dimensions: {tex.Texture.TagData.Width}x{tex.Texture.TagData.Height}",
                Texture = LoadTexture(tex.Texture)
            });
        }

        foreach (var tex in material.EnumeratePSTextures())
        {
            if (tex.Texture is null)
                continue;

            items.Add(new TextureDetail
            {
                Shader = "Pixel Shader",
                Hash = $"{tex.Texture.Hash}",
                Index = $"Index: {tex.TextureIndex}",
                Type = $"Colorspace: {(tex.Texture.IsSrgb() ? "Srgb" : "Non-Color")}",
                Dimension = $"Dimension: {tex.Texture.GetDimension()}",
                Format = $"Format: {(DXGI_FORMAT)tex.Texture.TagData.Format}",
                Dimensions = $"{tex.Texture.TagData.Width}x{tex.Texture.TagData.Height}",
                Texture = LoadTexture(tex.Texture)
            });
        } 

        return items;
    }

    public List<CBufferDetail> GetCBufferDetails(IMaterial material, bool bVertexShader = false)
    {
        var items = new List<CBufferDetail>();

        foreach (var cbuffer in GetCBuffers(material, bVertexShader))
        {
            items.Add(new CBufferDetail
            {
                Index = $"CB{cbuffer.Key.Index}",
                Count = $"Count: {cbuffer.Key.Count}",
                Data = cbuffer.Value
            });
        }
        var sortedItems = new List<CBufferDetail>(items);
        sortedItems.Sort((a, b) => a.Index.CompareTo(b.Index));
        return sortedItems;
    }

    private async void LoadCBufferData(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as CBufferDetail;

        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                ConcurrentBag<CBufferDataDetail> items = new ConcurrentBag<CBufferDataDetail>();
                for (int i = 0; i < dc.Data.Count; i++)
                {
                    CBufferDataDetail dataEntry = new();
                    dataEntry.Index = i;
                    dataEntry.Vector = $"{dc.Data[i].X}, {dc.Data[i].Y}, {dc.Data[i].Z}, {dc.Data[i].W}";

                    items.Add(dataEntry);
                }
                var sortedItems = new List<CBufferDataDetail>(items);
                sortedItems.Sort((a, b) => a.Index.CompareTo(b.Index));
                CBufferDataList.ItemsSource = sortedItems;
            });
        });
    }

    public BitmapImage LoadTexture(Texture textureHeader)
    {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = (textureHeader.IsCubemap() || textureHeader.IsVolume()) ? textureHeader.GetCubemapFace(0) : textureHeader.GetTexture();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        // Divide aspect ratio to fit 960x1000
        float widthDivisionRatio = (float)textureHeader.TagData.Width / 960;
        float heightDivisionRatio = (float)textureHeader.TagData.Height / 1000;
        float transformRatio = Math.Max(heightDivisionRatio, widthDivisionRatio);
        int imgWidth = (int)Math.Floor(textureHeader.TagData.Width / transformRatio);
        int imgHeight = (int)Math.Floor(textureHeader.TagData.Height / transformRatio);
        bitmapImage.DecodePixelWidth = imgWidth;
        bitmapImage.DecodePixelHeight = imgHeight;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    public static ConcurrentDictionary<Cbuffer, List<Vector4>> GetCBuffers(IMaterial material, bool isVertexShader = false)
    {
        StringReader reader = new(material.Decompile((isVertexShader ? material.VertexShader : material.PixelShader).GetBytecode(), $"ps{(isVertexShader ? material.VertexShader : material.PixelShader).Hash}"));

        List<Cbuffer> buffers = new();
        ConcurrentDictionary<Cbuffer, List<Vector4>> cbuffers = new();

        string line = string.Empty;
        do
        {
            line = reader.ReadLine();
            if (line != null)
            {
                if (line.Contains("cbuffer"))
                {
                    reader.ReadLine();
                    line = reader.ReadLine();
                    Cbuffer cbuffer = new Cbuffer();
                    cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
                    cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
                    cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
                    cbuffer.Type = line.Split("cb")[0].Trim();
                    buffers.Add(cbuffer);
                }
            }

        } while (line != null);

        foreach (var cbuffer in buffers)
        {
            List<Vector4> bufferData = new();
            dynamic data = null;

            if (isVertexShader)
            {
                if (cbuffer.Count == material.UnkA0.Count)
                {
                    data = material.UnkA0;
                }
                else if (cbuffer.Count == material.UnkC0.Count)
                {
                    data = material.UnkC0;
                }
            }
            else
            {
                if (cbuffer.Count == material.Unk2E0.Count)
                {
                    data = material.Unk2E0;
                }
                else if (cbuffer.Count == material.Unk300.Count)
                {
                    data = material.Unk300;
                }
                else
                {
                    if (material.PSVector4Container.IsValid())
                    {
                        // Try the Vector4 storage file
                        TigerFile container = new(material.PSVector4Container.GetReferenceHash());
                        byte[] containerData = container.GetData();
                        int num = containerData.Length / 16;
                        if (cbuffer.Count == num)
                        {
                            List<Vector4> float4s = new();
                            for (int i = 0; i < containerData.Length / 16; i++)
                            {
                                float4s.Add(containerData.Skip(i * 16).Take(16).ToArray().ToType<Vector4>());
                            }

                            data = float4s;
                        }
                    }
                }
            }

            for (int i = 0; i < cbuffer.Count; i++)
            {
                if (data == null)
                    bufferData.Add(new Vector4(0f,0f,0f,0f));
                else
                {
                    try
                    {
                        if (data[i] is Vec4)
                        {
                            bufferData.Add(data[i].Vec);
                        }
                        else if (data[i] is Vector4)
                        {
                            bufferData.Add(new Vector4(data[i].X, data[i].Y, data[i].Z, data[i].W));
                        }
                    }
                    catch (Exception e)
                    {
                        bufferData.Add(new Vector4(0f, 0f, 0f, 0f));
                    }
                }
            }

            cbuffers.TryAdd(cbuffer, bufferData);
        }

        return cbuffers;
    }

    private void Texture_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as TextureDetail;

        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(dc.Hash);
        if (textureHeader.IsCubemap())
        {
            var cubemapView = new CubemapView();
            cubemapView.LoadCubemap(textureHeader);
            _mainWindow.MakeNewTab(dc.Hash, cubemapView);
        }
        else
        {
            var textureView = new TextureView();
            textureView.LoadTexture(textureHeader);
            _mainWindow.MakeNewTab(dc.Hash, textureView);
        }
        _mainWindow.SetNewestTabSelected();
    }

    public List<UnkDataDetail> GetUnkDataDetails(IMaterial material)
    {
        var items = new List<UnkDataDetail>();

        items.Add(new UnkDataDetail
        {
            Name = "Test",
            Value = "123"
        });

        return items;

    }
}

public class TextureDetail
{
    public string Shader { get; set; }
    public string Hash { get; set; }
    public string Index { get; set; }
    public string Type { get; set; }
    public string Dimension { get; set; }
    public string Format { get; set; }
    public string Dimensions { get; set; }

    public BitmapImage Texture { get; set; }
}

public class CBufferDetail
{
    public string Index { get; set; }
    public string Count { get; set; }
    public List<Vector4> Data { get; set; }
}

public class CBufferDataDetail
{
    public int Index { get; set; }
    public string Vector { get; set; }
}

public class UnkDataDetail
{
    public string Name { get; set; }
    public string Value { get; set; }
}
