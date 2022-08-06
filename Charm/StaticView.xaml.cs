using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field.General;
using Field.Models;
using Field.Statics;


namespace Charm;

public partial class StaticView : UserControl
{
    public StaticView()
    {
        InitializeComponent();
    }
    
    public async void LoadStatic(TagHash hash, ELOD detailLevel)
    {
        var container = new StaticContainer(new TagHash(hash.Hash));
        var parts = container.Load(detailLevel);
        await Task.Run(() =>
        {
            parts = container.Load(detailLevel);
        });
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
        var displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = hash.GetHashString();
    }

    public void ExportStatic(TagHash hash, string name, EExportTypeFlag exportType)
    {
        FbxHandler fbxHandler = new FbxHandler(exportType == EExportTypeFlag.Full);
        string savePath = ConfigHandler.GetExportSavePath();
        string meshName = hash.GetHashString();
        if (exportType == EExportTypeFlag.Full)
        {
            savePath += $"/{name}";
        }
        var container = new StaticContainer(new TagHash(hash.Hash));
        List<Part> parts = container.Load(ELOD.MostDetail);
        fbxHandler.AddStaticToScene(parts, meshName);
        Directory.CreateDirectory(savePath);
        if (exportType == EExportTypeFlag.Full)
        {
            container.SaveMaterialsFromParts(savePath, parts, ConfigHandler.GetUnrealInteropEnabled());
            fbxHandler.InfoHandler.SetMeshName(meshName);
            if (ConfigHandler.GetUnrealInteropEnabled())
            {
                fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
                AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Static, ConfigHandler.GetOutputTextureFormat());
                //AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Static, ConfigHandler.GetOutputTextureFormat());

            }
        }
        fbxHandler.ExportScene($"{savePath}/{name}.fbx");
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(List<Part> containerParts)
    {
        List<MainViewModel.DisplayPart> displayParts = new List<MainViewModel.DisplayPart>();
        foreach (Part part in containerParts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Quaternion);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts;
    }
}