using System.Text;
using System.Collections.Generic;
using System.IO;
using System;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema;
using Tiger.Exporters;

namespace Charm;

public partial class StaticView : UserControl
{
    public StaticView()
    {
        InitializeComponent();
    }

    public async void LoadStatic(FileHash hash, ExportDetailLevel detailLevel)
    {
        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
        var parts = staticMesh.Load(detailLevel);
        await Task.Run(() =>
        {
            parts = staticMesh.Load(detailLevel);
        });
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        MVM.Clear();
        var displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = hash;
    }

    public static void ExportStatic(FileHash hash, string name, ExportTypeFlag exportType, string extraPath = "")
    {
        bool lodexport = false;
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        bool source2Models = config.GetS2VMDLExportEnabled();
        FbxHandler fbxHandler = new FbxHandler(exportType == ExportTypeFlag.Full);
        FbxHandler lodfbxHandler = new FbxHandler(exportType == ExportTypeFlag.Full);
        string savePath = config.GetExportSavePath() + "/" + extraPath + "/";
        string meshName = hash;
        if (exportType == ExportTypeFlag.Full)
        {
            savePath += $"/{name}";
        }

        StaticMesh staticMesh = FileResourcer.Get().GetFile<StaticMesh>(hash);
        List<StaticPart> parts = staticMesh.Load(ExportDetailLevel.MostDetailed);
        fbxHandler.AddStaticToScene(parts, meshName);
        Directory.CreateDirectory(savePath);
        if (exportType == ExportTypeFlag.Full)
        {
            staticMesh.SaveMaterialsFromParts(savePath, parts, config.GetUnrealInteropEnabled() || config.GetS2ShaderExportEnabled());
            fbxHandler.InfoHandler.SetMeshName(meshName);
            if (config.GetUnrealInteropEnabled())
            {
                fbxHandler.InfoHandler.SetUnrealInteropPath(config.GetUnrealInteropPath());
                AutomatedExporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedExporter.ImportType.Static, config.GetOutputTextureFormat());
                AutomatedExporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedExporter.ImportType.Static, config.GetOutputTextureFormat());
            }

            if(source2Models)
            {
                File.Copy("Exporters/template.vmdl", $"{savePath}/{meshName}.vmdl", true);
                string text = File.ReadAllText($"{savePath}/{meshName}.vmdl");
                StringBuilder mats = new StringBuilder();

                // {
                //     from = ""
                //     to = "materials/"
                // },
                int i = 0;
                foreach (StaticPart staticpart in parts)
                {
                    mats.AppendLine("{");
                    //mats.AppendLine($"    from = \"{meshName}_Group{staticpart.GroupIndex}_index{staticpart.Index}_{i}_{staticpart.LodCategory}_{i}.vmat\"");
                    mats.AppendLine($"    from = \"{staticpart.Material.Hash}.vmat\"");
                    mats.AppendLine($"    to = \"materials/{staticpart.Material.Hash}.vmat\"");
                    mats.AppendLine("},\n");
                    i++;
                }

                text = text.Replace("%MATERIALS%", mats.ToString());
                text = text.Replace("%FILENAME%", $"models/{meshName}.fbx");
                text = text.Replace("%MESHNAME%", meshName);

                File.WriteAllText($"{savePath}/{meshName}.vmdl", text);
            }

        }
        fbxHandler.InfoHandler.AddType("Static");
        fbxHandler.ExportScene($"{savePath}/{name}.fbx");

        if(lodexport)
        {
            List<StaticPart> lodparts = staticMesh.Load(ExportDetailLevel.LeastDetailed);
            Directory.CreateDirectory(savePath + "/LOD");

            foreach (StaticPart lodpart in lodparts)
            {
                Console.WriteLine($"Exporting LOD {lodpart.LodCategory}");
                Console.WriteLine(lodpart.Material.Hash.ToString());
            }

            lodfbxHandler.AddStaticToScene(lodparts, $"{meshName}_LOD");
            lodfbxHandler.InfoHandler.SetMeshName($"{meshName}_LOD");
            lodfbxHandler.ExportScene($"{savePath}/LOD/{name}_LOD.fbx");
        }
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(List<StaticPart> containerParts)
    {
        List<MainViewModel.DisplayPart> displayParts = new List<MainViewModel.DisplayPart>();
        foreach (StaticPart part in containerParts)
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
