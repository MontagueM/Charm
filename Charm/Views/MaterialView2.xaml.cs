using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;

namespace Charm;

/// <summary>
/// Interaction logic for MaterialView2.xaml
/// </summary>
public partial class MaterialView2 : UserControl, INotifyPropertyChanged
{
    private Material _material;
    public List<KeyValuePair<string, SMaterialShader>> ShaderStages { get; set; } = new();

    private ShaderStageItem _currentStage;
    public ShaderStageItem CurrentStage
    {
        get => _currentStage;
        set
        {
            _currentStage = value;
            OnPropertyChanged(nameof(CurrentStage));
        }
    }

    private TfxBytecodeInterpreter _bytecode;

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    public MaterialView2()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {

    }

    public void Load(FileHash hash)
    {
        _material = FileResourcer.Get().GetFile<Material>(hash);
        if (_material == null) return;

        ShaderStages.Clear();
        if (_material.Pixel.Shader != null)
            ShaderStages.Add(new("Pixel", _material.Pixel));
        if (_material.Vertex.Shader != null)
            ShaderStages.Add(new("Vertex", _material.Vertex));
        if (_material.Compute.Shader != null)
            ShaderStages.Add(new("Compute", _material.Compute));

        if (ShaderStages.Any())
            UIHelper.SelectRadioButton(ShaderStagesList, 0);
    }

    private async void ShaderStack_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton button && button.DataContext is KeyValuePair<string, SMaterialShader> kvp)
        {
            var stage = kvp.Key;
            var shader = kvp.Value;

            CurrentStage = new()
            {
                HLSL = shader.Shader.Decompile($"{stage}_{shader.Shader.Hash}"),
                ShaderHash = $"Shader {shader.Shader.Hash}",
                CB0 = new MaterialViewer_CBuffer
                {
                    Name = "CB0",
                    Data = await GetCB0(shader)
                },
                Constants = new MaterialViewer_CBuffer
                {
                    Name = "Constants",
                    Data = shader.TFX_Bytecode_Constants.Select((vec, index) => new MaterialViewer_CBufferEntry
                    {
                        Index = index,
                        StringVector = $"{vec.Vec.X}, {vec.Vec.Y}, {vec.Vec.Z}, {vec.Vec.W}",
                        Vector = vec.Vec,
                        Color = GetVectorColor(vec.Vec)
                    }).ToList()
                },
                Textures = await GetTextures(shader),
                Samplers = await GetSamplers(shader),
                UsedScopes = _material.EnumerateScopes().ToList(),
                UsedExterns = _material.GetExterns(),
                States = new()
                {
                    Blend = _material.RenderStates.Blend?.ToString() ?? "",
                    Rasterizer = _material.RenderStates.Rasterizer?.ToString() ?? "",
                    DepthBias = _material.RenderStates.DepthBias?.ToString() ?? "",
                },
                PrintedOps = _bytecode.PrintedOps.ToString()
            };

            if (Strategy.IsD1())
            {
                HLSLText.SyntaxHighlighting = null;
                HLSLText.Text = "Shader Decompilation is not supported for Destiny 1 :(";
            }
            else
                HLSLText.Text = CurrentStage.HLSL;
        }
    }

    private async Task<List<MaterialViewer_TextureDetail>> GetTextures(SMaterialShader shader)
    {
        List<MaterialViewer_TextureDetail> items = new();
        await Task.Run(() =>
        {
            foreach (STextureTag tex in shader.EnumerateTextures())
            {
                if (tex.Texture is null)
                    continue;

                items.Add(new MaterialViewer_TextureDetail
                {
                    Hash = $"{tex.Texture.Hash}",
                    Index = $"Index: {tex.TextureIndex}",
                    Type = $"Colorspace: {(tex.Texture.IsSrgb() ? "Srgb" : "Non-Color")}",
                    Dimension = $"Dimension: {EnumExtensions.GetEnumDescription(tex.Texture.GetDimension())}",
                    Format = $"Format: {tex.Texture.TagData.GetFormat()}",
                    Dimensions = $"{tex.Texture.Width}x{tex.Texture.Height}x{tex.Texture.Depth}",
                    Texture = TextureLoader.LoadTexture(tex.Texture, 128, 128)
                });
            }
        });
        return items;
    }

    private async Task<List<MaterialViewer_CBufferEntry>> GetCB0(SMaterialShader shader)
    {
        List<MaterialViewer_CBufferEntry> entries = new();
        await Task.Run(() =>
        {
            _bytecode = shader.GetBytecode();
            Dictionary<int, string> bytecode_hlsl = _bytecode.Evaluate(shader.TFX_Bytecode_Constants, true, _material);
            var cb0 = shader.GetCBuffer0();

            for (int i = 0; i < cb0.Count; i++)
            {
                MaterialViewer_CBufferEntry dataEntry = new();

                dataEntry.Index = i;
                if (bytecode_hlsl.ContainsKey(i))
                    dataEntry.StringVector = $"Bytecode Assigned";
                else
                {
                    dataEntry.StringVector = $"{cb0[i].X}, {cb0[i].Y}, {cb0[i].Z}, {cb0[i].W}";
                    dataEntry.Vector = cb0[i];
                    dataEntry.Color = GetVectorColor(cb0[i]);
                }

                entries.Add(dataEntry);
            }
            var sortedItems = new List<MaterialViewer_CBufferEntry>(entries);
            sortedItems.Sort((a, b) => a.Index.CompareTo(b.Index));
        });

        return entries;
    }

    private async Task<List<MaterialViewer_SamplerDetail>> GetSamplers(SMaterialShader shader)
    {
        List<MaterialViewer_SamplerDetail> items = new();
        await Task.Run(() =>
        {
            if (Strategy.IsD1())
                return;

            for (int i = 0; i < shader.Samplers.Count; i++)
            {
                if (shader.Samplers[i].GetSampler().Hash.GetFileMetadata().Type != 34)
                    continue;

                DirectXSampler.D3D11_SAMPLER_DESC sampler = shader.Samplers[i].GetSampler().Sampler;
                items.Add(new MaterialViewer_SamplerDetail
                {
                    Slot = i + 1,
                    Filter = sampler.Filter.ToString(),
                    AddressU = sampler.AddressU.ToString(),
                    AddressV = sampler.AddressV.ToString(),
                    ComparisonFunc = sampler.ComparisonFunc.ToString()
                });
            }
        });
        return items;
    }

    private Color GetVectorColor(Vector4 vec4)
    {
        float[] data = { vec4.X, vec4.Y, vec4.Z };
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
            return Color.FromArgb(255, r, g, b);
        }
        return Color.FromArgb(255, 0, 0, 0);
    }

    private void CBufferColor_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as MaterialViewer_CBufferEntry;

        try
        {
            Clipboard.SetText($"[{dc.Vector.X}, {dc.Vector.Y}, {dc.Vector.Z}, 1.0]");
        }
        catch (Exception ex)
        {
            PopupBanner test = new()
            {
                //Icon = "⚠️",
                Title = "ERROR",
                Subtitle = "Idk why this breaks sometimes but it can...try again.",
                Description = $"{ex.Message}",

                Style = PopupBanner.PopupStyle.Warning
            };
            test.Show();
        }
    }

    private void Texture_OnClick(object sender, RoutedEventArgs e)
    {
        var s = sender as Button;
        var dc = s.DataContext as MaterialViewer_TextureDetail;

        Texture textureHeader = FileResourcer.Get().GetFile<Texture>(dc.Hash);
        if (textureHeader.IsCubemap())
        {
            var cubemapView = new CubemapView();
            cubemapView.LoadCubemap(textureHeader);
            MainWindow.Current.MakeNewTab(dc.Hash, cubemapView);
        }
        else
        {
            var textureView = new TextureView();
            textureView.LoadTexture(textureHeader);
            MainWindow.Current.MakeNewTab(dc.Hash, textureView);
        }
        MainWindow.Current.SetNewestTabSelected();
    }

    private async void ExportMaterial_OnClick(object sender, RoutedEventArgs e)
    {
        await Task.Run(() =>
        {
            Dispatcher.Invoke(() =>
            {
                _material.Export($"{ConfigSubsystem.Get().GetExportSavePath()}/Materials/{_material.Hash}", true);

                NotificationBanner notify = new()
                {
                    Icon = "☑️",
                    Title = "Export Complete",
                    Description = $"Exported Material {_material.Hash} to \"{ConfigSubsystem.Get().GetExportSavePath()}/Materials/{_material.Hash}/\"",
                    Style = NotificationBanner.PopupStyle.Information
                };
                notify.Show();
            });
        });
    }

    private void OpenMaterial_OnClick(object sender, RoutedEventArgs e)
    {
        DevView.OpenHxD(_material.Hash);
    }

    public struct ShaderStageItem
    {
        public string HLSL { get; set; }
        public string ShaderHash { get; set; }
        public MaterialViewer_CBuffer CB0 { get; set; }
        public MaterialViewer_CBuffer Constants { get; set; }
        public List<MaterialViewer_TextureDetail> Textures { get; set; }
        public List<MaterialViewer_SamplerDetail> Samplers { get; set; }
        public List<TfxScope> UsedScopes { get; set; }
        public List<TfxExtern> UsedExterns { get; set; }
        public MaterialViewer_RenderStates States { get; set; }
        public string PrintedOps { get; set; }
    }

    public class MaterialViewer_CBuffer
    {
        public string Name { get; set; }
        public List<MaterialViewer_CBufferEntry> Data { get; set; } = new();
    }

    public class MaterialViewer_CBufferEntry
    {
        public int Index { get; set; }
        public string StringVector { get; set; }
        public Vector4 Vector { get; set; }
        public Color Color { get; set; } = Color.FromArgb(255, 0, 0, 0);
    }

    public class MaterialViewer_TextureDetail
    {
        public string Hash { get; set; }
        public string Index { get; set; }
        public string Type { get; set; }
        public string Dimension { get; set; }
        public string Format { get; set; }
        public string Dimensions { get; set; }

        public ImageSource Texture { get; set; }
    }

    public class MaterialViewer_SamplerDetail
    {
        public int Slot { get; set; }
        public string Filter { get; set; }
        public string AddressU { get; set; }
        public string AddressV { get; set; }
        public string ComparisonFunc { get; set; }
    }

    public class MaterialViewer_RenderStates
    {
        public string Blend { get; set; }
        public string Rasterizer { get; set; }
        public string DepthBias { get; set; }
    }
}
