using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
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
    private static IMaterial Material;

    public MaterialView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    private async void ExportMaterial_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as CBufferDetail;

        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                Material.SaveMaterial($"{ConfigSubsystem.Get().GetExportSavePath()}/Materials/{Material.FileHash}");
            });
        });
    }

    public void Load(FileHash hash)
    {
        IMaterial material = FileResourcer.Get().GetFileInterface<IMaterial>(hash);

        if (material is null)
            return;

        Material = material;
        UnkDataList.ItemsSource = GetUnkDataDetails(material);
        TextureListView.ItemsSource = GetTextureDetails(material);

        if (material.VertexShader is not null)
        {
            VertexShader.Text = material.Decompile(material.VertexShader.GetBytecode(), $"vs{material.VertexShader.Hash}");
            VS_CBufferList.ItemsSource = GetCBufferDetails(material, true);

            var vs_test = material.VertexShader?.Resources;
            Console.WriteLine($"----Vertex----");
            foreach (var vsr in vs_test)
            {
                Console.WriteLine($"Type: {vsr.ResourceType} | Index:{vsr.Index} | Count: {vsr.Count}");
            }

            var vs_in = material.VertexShader.InputSignatures;
            foreach (var b in vs_in)
            {
                Console.WriteLine($"v{b.RegisterIndex}: {b.ToString()}{b.SemanticIndex}.{b.Mask}");
            }

            var vs_out = material.VertexShader.OutputSignatures;
            foreach (var b in vs_out)
            {
                Console.WriteLine($"o{b.RegisterIndex}: {b.ToString()}{b.SemanticIndex}.{b.Mask}");
            }
        }

        if (material.PixelShader is not null)
        {
            PixelShader.Text = material.Decompile(material.PixelShader.GetBytecode(), $"ps{material.PixelShader.Hash}");
            PS_CBufferList.ItemsSource = GetCBufferDetails(material);

            var ps_test = material.PixelShader?.Resources;
            Console.WriteLine($"----Pixel----");
            foreach (var a in ps_test)
            {
                Console.WriteLine($"Type: {a.ResourceType} | Index:{a.Index} | Count: {a.Count}");
            }

            var ps_in = material.PixelShader.InputSignatures;
            foreach (var b in ps_in)
            {
                Console.WriteLine($"v{b.RegisterIndex}: {b.ToString()}{b.SemanticIndex}.{b.Mask}");
            }

            var ps_out = material.PixelShader.OutputSignatures;
            foreach (var b in ps_out)
            {
                Console.WriteLine($"o{b.RegisterIndex}: {b.ToString()}{b.SemanticIndex}.{b.Mask}");
            }
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

        foreach (var tex in material.EnumerateCSTextures())
        {
            if (tex.Texture is null)
                continue;

            items.Add(new TextureDetail
            {
                Shader = "Compute Shader",
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

        //foreach (var cbuffer in GetCBuffers(material, bVertexShader))
        //{
        //    items.Add(new CBufferDetail
        //    {
        //        Index = $"CB{cbuffer.Key.Index}",
        //        Count = $"Count: {cbuffer.Key.Count}",
        //        Data = cbuffer.Value,
        //        Stage = bVertexShader ? CBufferDetail.Shader.Vertex : CBufferDetail.Shader.Pixel
        //    });
        //}

        //Only material provided cbuffer (cb0) is useful to show
        List<Vector4> data = new();
        if (bVertexShader)
        {
            if (material.VSVector4Container.IsValid())
            {
                data = material.GetVec4Container(material.VSVector4Container.GetReferenceHash());
            }
            else
            {
                foreach (var vec in material.VS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
            }       
        }
        else
        {
            if (material.PSVector4Container.IsValid())
            {
                data = material.GetVec4Container(material.PSVector4Container.GetReferenceHash());
            }
            else
            {
                foreach (var vec in material.PS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
            }
        }

        if (data.Count > 0)
        {
            items.Add(new CBufferDetail
            {
                Index = "CB0",
                Count = $"Count: {data.Count}",
                Data = data,
                Stage = bVertexShader ? CBufferDetail.Shader.Vertex : CBufferDetail.Shader.Pixel
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
                TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(dc.Stage == CBufferDetail.Shader.Pixel ? Material.PS_TFX_Bytecode : Material.VS_TFX_Bytecode));
                var bytecode_hlsl = bytecode.Evaluate(dc.Stage == CBufferDetail.Shader.Pixel ? Material.PS_TFX_Bytecode_Constants : Material.VS_TFX_Bytecode_Constants, true);

                ConcurrentBag<CBufferDataDetail> items = new ConcurrentBag<CBufferDataDetail>();
                for (int i = 0; i < dc.Data.Count; i++)
                {
                    CBufferDataDetail dataEntry = new();
                    
                    dataEntry.Index = i;
                    if(bytecode_hlsl.ContainsKey(i))
                        dataEntry.Vector = $"Bytecode Assigned";
                    else
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
        //// Divide aspect ratio to fit 960x1000
        //float widthDivisionRatio = (float)textureHeader.TagData.Width / 512;
        //float heightDivisionRatio = (float)textureHeader.TagData.Height / 512;
        //float transformRatio = Math.Max(heightDivisionRatio, widthDivisionRatio);
        //int imgWidth = (int)Math.Floor(textureHeader.TagData.Width / transformRatio);
        //int imgHeight = (int)Math.Floor(textureHeader.TagData.Height / transformRatio);
        bitmapImage.DecodePixelWidth = 256;
        bitmapImage.DecodePixelHeight = 256;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
    }

    //public static List<Cbuffer> GetCBuffers(IMaterial material, bool isVertexShader = false)
    //{
    //    StringReader reader = new(material.Decompile((isVertexShader ? material.VertexShader : material.PixelShader).GetBytecode(),
    //        $"{(isVertexShader ? $"vs{material.VertexShader.Hash}" : $"ps{material.PixelShader.Hash}")}"));

    //    List<Cbuffer> buffers = new();

    //    string line = string.Empty;
    //    do
    //    {
    //        line = reader.ReadLine();
    //        if (line != null)
    //        {
    //            if (line.Contains("cbuffer"))
    //            {
    //                reader.ReadLine();
    //                line = reader.ReadLine();
    //                Cbuffer cbuffer = new Cbuffer();
    //                cbuffer.Variable = "cb" + line.Split("cb")[1].Split("[")[0];
    //                cbuffer.Index = Int32.TryParse(new string(cbuffer.Variable.Skip(2).ToArray()), out int index) ? index : -1;
    //                cbuffer.Count = Int32.TryParse(new string(line.Split("[")[1].Split("]")[0]), out int count) ? count : -1;
    //                cbuffer.Type = line.Split("cb")[0].Trim();
    //                buffers.Add(cbuffer);
    //            }
    //        }

    //    } while (line != null);

    //    return buffers;
    //}

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
        //Theres gotta be a better way of doing all this
        var items = new List<UnkDataDetail>
        {
            new UnkDataDetail
            {
                Name = "Unk08",
                Value = material.Unk08.ToString("X2")
            },
            new UnkDataDetail
            {
                Name = "Unk0C",
                Value = material.Unk0C.ToString("X2")
            },
            new UnkDataDetail
            {
                Name = "Unk10",
                Value = material.Unk10.ToString("X2")
            }
        };

        if (material.VS_TFX_Bytecode.Count > 0)
        {
            items.Add(new UnkDataDetail
            {
                Name = "VS TFX Bytecode",
                Value = material.VS_TFX_Bytecode.Count.ToString()
            });
        }

        if (material.PS_TFX_Bytecode.Count > 0)
        {
            items.Add(new UnkDataDetail
            {
                Name = "PS TFX Bytecode",
                Value = material.PS_TFX_Bytecode.Count.ToString()
            });
        }

        if (material.VS_Samplers.Count > 0)
        {
            items.Add(new UnkDataDetail
            {
                Name = "VS Samplers",
                Value = material.VS_Samplers.Count.ToString()
            });
        }

        if (material.PS_Samplers.Count > 0)
        {
            items.Add(new UnkDataDetail
            {
                Name = "PS Samplers",
                Value = material.PS_Samplers.Count.ToString()
            });
        }

        if(material.ComputeShader != null)
        {
            items.Add(new UnkDataDetail
            {
                Name = "Compute Shader",
                Value = material.ComputeShader.Hash.ToString()
            });
        }

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
    public Shader Stage { get; set; }
    public List<Vector4> Data { get; set; }

    public enum Shader
    {
        Pixel,
        Vertex
    }
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
