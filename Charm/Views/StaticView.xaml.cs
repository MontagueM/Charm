using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HelixToolkit.SharpDX;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Static;

namespace Charm;

public partial class StaticView : UserControl
{
    private MainViewModel MVM;
    public StaticView()
    {
        InitializeComponent();
    }

    private FileHash currentHash;
    private ExportDetailLevel currentDetailLevel;

    public void LoadStatic(FileHash hash, ExportDetailLevel detailLevel)
    {
        currentHash = hash;
        currentDetailLevel = detailLevel;

        SetupCheckboxHandlers();
        ModelView.Visibility = Visibility.Visible;
        ModelView.TextureCheckBox.Visibility = Visibility.Visible;

        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
        List<StaticPart> parts = staticMesh.Load(detailLevel);

        if (MVM is null)
            MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];

        MVM.Clear();
        List<MainViewModel.DisplayPart> displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = hash;
        MVM.SubTitle = $"{displayParts.Sum(p => p.BasePart.Indices.Count)} triangles";
    }


    public static void ExportStatic(FileHash hash, string name, ExportTypeFlag exportType, string extraPath = "")
    {
        ExporterScene scene = Exporter.Get().CreateScene(hash, ExportType.Statics);
        ConfigSubsystem config = ConfigSubsystem.Get();
        string meshName = hash;

        string savePath = Path.Combine(config.GetExportSavePath(), extraPath);
        //if (extraPath != string.Empty)
        //    savePath = Path.Combine(savePath, name);

        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        scene.AddStatic(hash, parts);
        staticMesh.SaveMaterialsFromParts(scene, parts);

        Directory.CreateDirectory(savePath);
        if (exportType == ExportTypeFlag.Full)
        {
            if (config.GetUnrealInteropEnabled())
            {
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Static, config.GetOutputTextureFormat());
            }
        }

        if (extraPath != string.Empty)
            Exporter.Get().Export($"{savePath}");
        else
            Exporter.Get().Export();
    }



    private List<MainViewModel.DisplayPart> MakeDisplayParts(List<StaticPart> containerParts)
    {
        bool useTextures = ModelView.TextureCheckBox.IsChecked == true;
        List<MainViewModel.DisplayPart> displayParts = new();

        foreach (StaticPart part in containerParts)
        {
            var displayPart = new MainViewModel.DisplayPart
            {
                BasePart = part,
                Translations = { Vector3.Zero },
                Rotations = { Vector4.Quaternion },
                Scales = { Vector3.One }
            };

            if (useTextures && part.Material?.Pixel.Textures.Any() == true)
            {
                Stream texture = TextureView.RemoveAlpha(part.Material.Pixel.Textures[0].Texture.GetTexture());
                displayPart.DiffuseMaterial = new()
                {
                    DiffuseMap = new TextureModel(texture, true),
                };
            }

            displayParts.Add(displayPart);
        }

        return displayParts;
    }

    private void SetupCheckboxHandlers()
    {
        // Detach first to prevent multiple subscriptions
        ModelView.TextureCheckBox.Checked -= TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked -= TextureCheckBox_Unchecked;

        ModelView.TextureCheckBox.Checked += TextureCheckBox_Checked;
        ModelView.TextureCheckBox.Unchecked += TextureCheckBox_Unchecked;
    }

    private void TextureCheckBox_Checked(object sender, RoutedEventArgs e) =>
        LoadStatic(currentHash, currentDetailLevel);

    private void TextureCheckBox_Unchecked(object sender, RoutedEventArgs e) =>
        LoadStatic(currentHash, currentDetailLevel);

}

