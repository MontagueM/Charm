﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

namespace Charm;

public partial class MaterialView : UserControl
{
    private MainWindow _mainWindow = null;
    private IMaterial Material;

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
        ShaderDetail shaderDetail = new();

        if (material is null)
            return;

        Material = material;
        SamplerDataList.ItemsSource = GetSamplerData(material);
        TextureListView.ItemsSource = GetTextureDetails(material);
        UsedScopesList.ItemsSource = material.EnumerateScopes();

        if (material.VertexShader is not null)
        {
            shaderDetail.VertexShaderHash = material.VertexShader.Hash.ToString();

            if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
                shaderDetail.VertexShader = material.Decompile(material.VertexShader.GetBytecode(), $"vs{material.VertexShader.Hash}");
            else
                shaderDetail.VertexShader = "Shader decompilation not supported for Destiny 1";
            VS_CBufferList.ItemsSource = GetCBufferDetails(material, true);
        }

        if (material.PixelShader is not null)
        {
            shaderDetail.PixelShaderHash = material.PixelShader.Hash.ToString();

            if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
                shaderDetail.PixelShader = material.Decompile(material.PixelShader.GetBytecode(), $"ps{material.PixelShader.Hash}");
            else
                shaderDetail.PixelShader = "Shader decompilation not supported for Destiny 1";
            PS_CBufferList.ItemsSource = GetCBufferDetails(material);
        }

        DataContext = shaderDetail;

#if DEBUG
        System.Console.WriteLine($"{material.RenderStates.ToString()}");

        System.Console.WriteLine($"BlendState: {material.RenderStates.BlendState()}");
        System.Console.WriteLine($"RasterizerState: {material.RenderStates.RasterizerState()}");
        System.Console.WriteLine($"DepthBiasState: {material.RenderStates.DepthBiasState()}");
        System.Console.WriteLine($"DepthStencilState: {material.RenderStates.DepthStencilState()}");
#endif
    }

    private List<TextureDetail> GetTextureDetails(IMaterial material)
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
                Dimension = $"Dimension: {EnumExtensions.GetEnumDescription(tex.Texture.GetDimension())}",
                Format = $"Format: {tex.Texture.TagData.GetFormat()}",
                Dimensions = $"{tex.Texture.TagData.Width}x{tex.Texture.TagData.Height}",
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
                Dimension = $"Dimension: {EnumExtensions.GetEnumDescription(tex.Texture.GetDimension())}",
                Format = $"Format: {tex.Texture.TagData.GetFormat()}",
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
                Dimension = $"Dimension: {EnumExtensions.GetEnumDescription(tex.Texture.GetDimension())}",
                Format = $"Format: {tex.Texture.TagData.GetFormat()}",
                Dimensions = $"{tex.Texture.TagData.Width}x{tex.Texture.TagData.Height}",
                Texture = LoadTexture(tex.Texture)
            });
        }

        return items;
    }

    private List<CBufferDetail> GetCBufferDetails(IMaterial material, bool bVertexShader = false)
    {
        var items = new List<CBufferDetail>();

        //Only material provided cbuffer (cb0) is useful to show
        List<Vector4> data = new();
        List<Vector4> const_data = new();
        if (bVertexShader)
        {
            if (material.VSVector4Container.IsValid())
            {
                data = material.GetVec4Container(true);
            }
            else
            {
                foreach (var vec in material.VS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
                foreach (var vec in material.VS_TFX_Bytecode_Constants)
                {
                    const_data.Add(vec.Vec);
                }
            }
        }
        else
        {
            if (material.PSVector4Container.IsValid())
            {
                data = material.GetVec4Container();
            }
            else
            {
                foreach (var vec in material.PS_CBuffers)
                {
                    data.Add(vec.Vec);
                }
                foreach (var vec in material.PS_TFX_Bytecode_Constants)
                {
                    const_data.Add(vec.Vec);
                }
            }
        }

        if (data.Count > 0)
        {
            items.Add(new CBufferDetail
            {
                Index = "CB0",
                Count = $"{data.Count}",
                Data = data,
                Stage = bVertexShader ? CBufferDetail.Shader.Vertex : CBufferDetail.Shader.Pixel
            });
        }
        if (const_data.Count > 0)
        {
            items.Add(new CBufferDetail
            {
                Index = "TFX Constants",
                Count = $"{const_data.Count}",
                Data = const_data,
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
                var bytecode_hlsl = bytecode.Evaluate(dc.Stage == CBufferDetail.Shader.Pixel ? Material.PS_TFX_Bytecode_Constants : Material.VS_TFX_Bytecode_Constants, dc.Index != "TFX Constants", Material);

                ConcurrentBag<CBufferDataDetail> items = new ConcurrentBag<CBufferDataDetail>();
                for (int i = 0; i < dc.Data.Count; i++)
                {
                    CBufferDataDetail dataEntry = new();

                    dataEntry.Index = i;
                    if (bytecode_hlsl.ContainsKey(i) && dc.Index != "TFX Constants")
                        dataEntry.StringVector = $"Bytecode Assigned";
                    else
                    {
                        dataEntry.StringVector = $"{dc.Data[i].X}, {dc.Data[i].Y}, {dc.Data[i].Z}, {dc.Data[i].W}";
                        dataEntry.Vector = dc.Data[i];

                        float[] data = { dc.Data[i].X, dc.Data[i].Y, dc.Data[i].Z };

                        if (data.All(v => v >= 0.0f))
                        {
                            bool needsNormalization = data.Any(v => v > 1.0f);
                            float[] floats;

                            if (needsNormalization)
                            {
                                float factor = data.Max();
                                floats = new float[]
                                {
                                    data[0] / factor,
                                    data[1] / factor,
                                    data[2] / factor,
                                };
                            }
                            else
                            {
                                floats = (float[])data.Clone();
                            }

                            byte r = (byte)(Math.Abs(floats[0]) * 255);
                            byte g = (byte)(Math.Abs(floats[1]) * 255);
                            byte b = (byte)(Math.Abs(floats[2]) * 255);
                            dataEntry.Color = Color.FromArgb(255, r, g, b);
                        }
                    }

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
        bitmapImage.DecodePixelWidth = 256;
        bitmapImage.DecodePixelHeight = 256;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        return bitmapImage;
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

    private List<SamplerDataDetail> GetSamplerData(IMaterial material)
    {
        List<SamplerDataDetail> items = new();
        for (int i = 0; i < material.PS_Samplers.Count; i++)
        {
            if (material.PS_Samplers[i].Hash.GetFileMetadata().Type != 34)
                continue;

            var sampler = material.PS_Samplers[i].Sampler;
            items.Add(new SamplerDataDetail
            {
                Slot = i + 1,
                Filter = sampler.Filter.ToString(),
                AddressU = sampler.AddressU.ToString(),
                AddressV = sampler.AddressV.ToString(),
            });
        }

        return items;
    }

    private void CBufferColor_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as CBufferDataDetail;

        //float r = dc.Color.R / 255;
        //float g = dc.Color.R / 255;
        //float b = dc.Color.R / 255;

        Clipboard.SetText($"[{dc.Vector.X}, {dc.Vector.Y}, {dc.Vector.Z}, 1.0]");
    }

    private void OpenMaterial_OnClick(object sender, RoutedEventArgs e)
    {
        DevView.OpenHxD(Material.FileHash);
    }

    private class ShaderDetail
    {
        public string PixelShaderHash { get; set; }
        public string PixelShader { get; set; }

        public string VertexShaderHash { get; set; }
        public string VertexShader { get; set; }

        public string ComputeShaderHash { get; set; }
        public string ComputeShader { get; set; }
    }

    private class TextureDetail
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

    private class CBufferDetail
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

    private class CBufferDataDetail
    {
        public int Index { get; set; }
        public string StringVector { get; set; }
        public Vector4 Vector { get; set; }
        public Color Color { get; set; } = Color.FromArgb(255, 0, 0, 0);
    }

    private class SamplerDataDetail
    {
        public int Slot { get; set; }
        public string Filter { get; set; }
        public string AddressU { get; set; }
        public string AddressV { get; set; }
    }
}

