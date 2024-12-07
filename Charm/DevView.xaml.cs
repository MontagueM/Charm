using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ConcurrentCollections;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;
using Tiger.Schema.Investment;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Decorator = Tiger.Schema.Static.Decorator;

namespace Charm;

public partial class DevView : UserControl
{
    private static MainWindow _mainWindow = null;
    private FbxHandler _fbxHandler = null;

    public DevView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        _fbxHandler = new FbxHandler(false);
        HashLocation.Text = $"PKG:\nPKG ID:\nEntry Index:";

        //RipAndTear();
    }

    private void TagHashBoxKeydown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return && e.Key != Key.H && e.Key != Key.R && e.Key != Key.E && e.Key != Key.L)
            return;

        string strHash = TagHashBox.Text.Replace(" ", "");
        strHash = Regex.Replace(strHash, @"(\s+|r|h)", "");
        if (strHash.Length == 16)
            strHash = Hash64Map.Get().GetHash32Checked(strHash);

        if (strHash == "")
            return;

        FileHash hash;
        if (strHash.Contains("-"))
        {
            var s = strHash.Split("-");
            var pkgid = Int32.Parse(s[0], NumberStyles.HexNumber);
            var entryindex = Int32.Parse(s[1], NumberStyles.HexNumber);
            hash = new FileHash(pkgid, (uint)entryindex);  // fix to int/uint stuff here
        }
        else
        {
            hash = new FileHash(strHash);
        }

        if (!hash.IsValid())
        {
            if (uint.TryParse(strHash, out uint apiHash))
            {
                Investment.LazyInit();
                var item = Investment.Get().TryGetInventoryItem(new TigerHash(apiHash));
                if (item is not null)
                {
                    MainWindow.Progress.SetProgressStages(new() { "Starting investment system" });
                    Investment.LazyInit();
                    MainWindow.Progress.CompleteStage();

                    item.Load();
                    APIItemView apiItemView = new APIItemView(item);
                    _mainWindow.MakeNewTab(Investment.Get().GetItemName(item), apiItemView);
                    _mainWindow.SetNewestTabSelected();
                }
                else
                    TagHashBox.Text = "INVALID API HASH";

                return;
            }
            else
            {
                TagHashBox.Text = "INVALID HASH";
                return;
            }
        }
        //uint to int
        switch (e.Key)
        {
            case Key.L:
                StringBuilder data = new();
                data.AppendLine($"PKG: {PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId(hash.PackageId)})");
                data.AppendLine($"PKG ID: {hash.PackageId}");
                data.AppendLine($"Entry Index: {hash.FileIndex}");
                data.AppendLine($"Type: {hash.GetFileMetadata().Type} | SubType: {hash.GetFileMetadata().SubType}");
                data.AppendLine($"Reference Hash: {hash.GetReferenceHash()}");
                string h64 = Hash64Map.Get().GetHash64(hash);
                if (!string.IsNullOrEmpty(h64))
                {
                    data.AppendLine($"Hash64: {h64}");
                }

                HashLocation.Text = data.ToString();
                break;
            case Key.Return:
                AddWindow(hash);
                break;
            case Key.H:
                OpenHxD(hash);
                break;
            case Key.R:
                FileHash refHash = hash.GetReferenceHash();
                if (!refHash.ToString().EndsWith("8080"))
                {
                    OpenHxD(refHash);
                }
                else
                {
                    TagHashBox.Text = $"REF {refHash}";
                }
                break;
            case Key.E:
                Entity entity = FileResourcer.Get().GetFile(typeof(Entity), hash);
                if (entity.Model != null)
                {
                    OpenHxD(entity.Model.Hash);
                }
                else
                {
                    TagHashBox.Text = $"NO MODEL";
                }
                break;
        }
    }

    private void ExportWem(ExportInfo info)
    {
        Wem wem = FileResourcer.Get().GetFile<Wem>(info.Hash as FileHash);
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wav");
    }

    private void AddWindow(FileHash hash)
    {
        _fbxHandler.Clear();
        // Adds a new tab to the tab control
        TigerHash reference = hash.GetReferenceHash();
        FileMetadata fileMetadata = PackageResourcer.Get().GetFileMetadata(hash);
        if ((fileMetadata.Type == 26 && fileMetadata.SubType == 7) || (fileMetadata.Type == 8 && fileMetadata.SubType == 21))
        {
            var audioView = new TagView();
            audioView.SetViewer(TagView.EViewerType.TagList);
            audioView.MusicPlayer.SetWem(FileResourcer.Get().GetFile(typeof(Wem), hash));
            audioView.MusicPlayer.Play();
            audioView.ExportControl.SetExportFunction(ExportWem, (int)ExportTypeFlag.Full);
            audioView.ExportControl.SetExportInfo(hash);
            _mainWindow.MakeNewTab(hash, audioView);
            _mainWindow.SetNewestTabSelected();
        }
        else if (fileMetadata.Type == 32)
        {
            Texture textureHeader = FileResourcer.Get().GetFile<Texture>(hash);
            if (textureHeader.IsCubemap())
            {
                var cubemapView = new CubemapView();
                cubemapView.LoadCubemap(textureHeader);
                _mainWindow.MakeNewTab(hash, cubemapView);
            }
            else
            {
                var textureView = new TextureView();
                textureView.LoadTexture(textureHeader);
                _mainWindow.MakeNewTab(hash, textureView);
            }
            _mainWindow.SetNewestTabSelected();
        }
        else if ((fileMetadata.Type == 8 || fileMetadata.Type == 16) && fileMetadata.SubType == 0)
        {
            switch (reference.Hash32)
            {
                case 0x80800734:
                case 0x80809C0F:
                case 0x80809AD8:
                    EntityView entityView = new EntityView();
                    entityView.LoadEntity(hash, _fbxHandler);
                    Entity entity = FileResourcer.Get().GetFile<Entity>(hash);
                    EntityView.Export(new List<Entity> { entity }, hash, ExportTypeFlag.Full);
                    _mainWindow.MakeNewTab(hash, entityView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808071a7:
                case 0x80806D44:
                    StaticView staticView = new StaticView();
                    staticView.LoadStatic(hash, ExportDetailLevel.MostDetailed, Window.GetWindow(this));
                    _mainWindow.MakeNewTab(hash, staticView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808093AD:
                    MapView mapView = new MapView();
                    mapView.LoadMap(hash, ExportDetailLevel.LeastDetailed);
                    _mainWindow.MakeNewTab(hash, mapView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x80808E8E:
                    ActivityView activityView = new ActivityView();
                    activityView.LoadActivity(hash);
                    _mainWindow.MakeNewTab(hash, activityView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808099EF:
                    var stringView = new TagView();
                    stringView.SetViewer(TagView.EViewerType.TagList);
                    stringView.TagListControl.LoadContent(ETagListType.Strings, hash, true);
                    _mainWindow.MakeNewTab(hash, stringView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808097B8:
                    var dialogueView = new DialogueView();
                    dialogueView.Load(hash, null);
                    _mainWindow.MakeNewTab(hash, dialogueView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808071E8:
                case 0x80806DAA:
                    var materialView = new MaterialView();
                    materialView.Load(hash);
                    _mainWindow.MakeNewTab(hash, materialView);
                    _mainWindow.SetNewestTabSelected();
                    Material material = FileResourcer.Get().GetFile<Material>(hash);
                    material.Export($"{ConfigSubsystem.Get().GetExportSavePath()}/Materials/{hash}");
                    break;

                case 0x80801AB5:
                case 0x808073A5:
                case 0x80806F07: //Entity model
                    EntityModel entityModel = FileResourcer.Get().GetFile<EntityModel>(hash);
                    ExporterScene scene = Exporter.Get().CreateScene(hash, ExportType.Entity);
                    scene.AddModel(entityModel);
                    var parts = entityModel.Load(ExportDetailLevel.MostDetailed, null);
                    foreach (DynamicMeshPart part in parts)
                    {
                        if (part.Material == null) continue;
                        scene.Materials.Add(new ExportMaterial(part.Material));
                    }
                    Exporter.Get().Export();

                    EntityView entityModelView = new EntityView();
                    entityModelView.LoadEntityModel(hash, _fbxHandler);
                    _mainWindow.MakeNewTab(hash, entityModelView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x8080714F:
                case 0x80806C81:
                    Terrain terrain = FileResourcer.Get().GetFile<Terrain>(hash);
                    ExporterScene terrainScene = Exporter.Get().CreateScene(hash, ExportType.Terrain);
                    terrain.LoadIntoExporter(terrainScene, ConfigSubsystem.Get().GetExportSavePath());
                    Exporter.Get().Export();
                    break;

                case 0x80801ACE:
                case 0x80806C98: // Decorator 986C8080
                    Decorator decorator = FileResourcer.Get().GetFile<Decorator>(hash);
                    ExporterScene decoratorScene = Exporter.Get().CreateScene(hash, ExportType.Map);
                    decorator.LoadIntoExporter(decoratorScene, ConfigSubsystem.Get().GetExportSavePath());
                    Exporter.Get().Export();
                    break;

                // Testing
                case 0x80801AF2:
                case 0x808071DC:
                case 0x80806DA1:
                    Tag<D2Class_A16D8080> lightData = FileResourcer.Get().GetSchemaTag<D2Class_A16D8080>(hash);
                    TfxBytecodeInterpreter bytecode = new(TfxBytecodeOp.ParseAll(lightData.TagData.Bytecode));
                    _ = bytecode.Evaluate(lightData.TagData.Buffer1, true);

                    //foreach (var a in bytecode_hlsl)
                    //{
                    //    Console.WriteLine($"\n{a.Key} : {a.Value}\n");
                    //}
                    break;

                // Scopes / gear dye (which is a scope)
                case 0x80806DBA:
                    Dye scope_data = FileResourcer.Get().GetFile<Dye>(hash);
                    bytecode = new(TfxBytecodeOp.ParseAll(scope_data.TagData.Bytecode));
                    _ = bytecode.Evaluate(scope_data.TagData.BytecodeConstants, true);
                    break;
                default:
                    MessageBox.Show("Unknown reference: " + Endian.U32ToString(reference));
                    break;
            }
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    public static void OpenHxD(FileHash hash)
    {
        ConfigSubsystem config = CharmInstance.GetSubsystem<ConfigSubsystem>();
        string savePath = config.GetExportSavePath() + "/temp";
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string strategy = Strategy.CurrentStrategy.ToString();
        if (strategy.Contains("_"))
        {
            strategy = strategy.Split("_").Last();
        }
        string path = $"{savePath}/{strategy}_{hash.PackageId:x4}_{hash.GetReferenceHash()}_{hash}.bin";
        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            using (var writer = new BinaryWriter(fileStream))
            {
                byte[] data = FileResourcer.Get().GetFile(hash).GetData();
                writer.Write(data);
            }
        }
        new Process
        {
            StartInfo = new ProcessStartInfo($@"{path}")
            {
                UseShellExecute = true
            }
        }.Start();
    }

    private void BatchExport_Click(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(BatchList.Text))
        {
            BatchList.Text = "Invalid file or does not exist";
            return;
        }
        var hashes = File.ReadAllLines(BatchList.Text);
        foreach (var hash in hashes)
        {
            Material material = FileResourcer.Get().GetFile<Material>(hash);
            material.Export($"{ConfigSubsystem.Get().GetExportSavePath()}/Materials/{hash}");
        }
        MessageBox.Show($"Batch export of {hashes.Length} materials completed");
    }

#if DEBUG
    // Cleaverly done (insert name) but you're not supposed to be here. As a matter of fact, you're not.
    // Get back where you belong and forget about all this...Until we meet again.
    public void RipAndTear()
    {
        bool PackageFilterFunc(string packagePath) => packagePath.Contains("investment") || packagePath.Contains("client_startup");
        ConcurrentHashSet<FileHash> allHashes = PackageResourcer.Get().GetAllHashes(PackageFilterFunc);
        Parallel.ForEach(allHashes, (val, state, i) =>
        {
            if (val.GetReferenceHash().IsValid() && val.GetReferenceHash().ToString().EndsWith("8080"))
            {
                string path = $"C:\\Users\\Michael\\Desktop\\out\\D2\\{val.GetReferenceHash()}_{val}.bin";
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    using (var writer = new BinaryWriter(fileStream))
                    {
                        byte[] data = FileResourcer.Get().GetFile(val).GetData();
                        writer.Write(data);
                    }
                }
            }
        });
    }
#endif
}
