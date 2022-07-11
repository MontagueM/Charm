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
    public StaticContainer Container;
    private List<Part> parts;

    public StaticView()
    {
        InitializeComponent();
    }

    private void GetStaticContainer(TagHash hash)
    {
        Container = new StaticContainer(new TagHash(hash.Hash));
    }

    public async void LoadStatic(TagHash hash, ELOD detailLevel)
    {
        GetStaticContainer(hash);
        parts = Container.Load(detailLevel);
        await Task.Run(() =>
        {
            if (Container == null)
            {
                GetStaticContainer(hash);
            }
            parts = Container.Load(detailLevel);
        });
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
        var displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = hash.GetHashString();
    }

    public void ExportFullStatic(TagHash hash)
    {
        InfoConfigHandler.MakeFile();
        string meshName = hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        List<Part> parts = Container.Load(ELOD.MostDetail);
        FbxHandler.AddStaticToScene(parts, meshName);
        Directory.CreateDirectory(savePath);
        Container.SaveMaterialsFromParts(savePath, parts);
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Static);
        InfoConfigHandler.WriteToFile(savePath);
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