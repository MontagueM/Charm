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
    public TagHash Hash;
    private List<Part> parts;

    public StaticView(TagHash hash)
    {
        Hash = hash;
        InitializeComponent();
    }

    private void GetStaticContainer(TagHash hash)
    {
        Container = new StaticContainer(new TagHash(hash.Hash));
    }

    public async void LoadStatic(ELOD detailLevel)
    {
        GetStaticContainer(Hash);
        parts = Container.Load(detailLevel);
        await Task.Run(() =>
        {
            if (Container == null)
            {
                GetStaticContainer(Hash);
            }
            parts = Container.Load(detailLevel);
        });
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        var displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = Hash.GetHashString();
        ExportFullStatic();
    }

    private void ExportFullStatic()
    {
        InfoConfigHandler.MakeFile();
        string meshName = Hash.GetHashString();
        string savePath = ConfigHandler.GetExportSavePath() + $"/{meshName}";
        List<Part> parts = Container.Load(ELOD.MostDetail);
        FbxHandler.AddStaticToScene(parts);
        Directory.CreateDirectory(savePath);
        Container.SaveMaterialsFromParts(savePath, parts);
        FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
        InfoConfigHandler.SetMeshName(meshName);
        InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, true);
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