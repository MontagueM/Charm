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
using Decorator = Tiger.Schema.Decorator;

namespace Charm;

public partial class DevView : UserControl
{
    private static MainWindow _mainWindow = null;

    public DevView()
    {
        InitializeComponent();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
        HashLocation.Text = $"PKG:\nPKG ID:\nEntry Index:";

        //RipAndTear();
    }

    private void TagHashBoxKeydown(object sender, KeyEventArgs e)
    {
        if (e.Key is not Key.Return and not Key.H and not Key.R and not Key.E and not Key.L)
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
            string[] s = strHash.Split("-");
            int pkgid = Int32.Parse(s[0], NumberStyles.HexNumber);
            int entryindex = Int32.Parse(s[1], NumberStyles.HexNumber);
            hash = new FileHash(pkgid, (uint)entryindex);  // fix to int/uint stuff here
        }
        else
        {
            // Flips tag hash to the "intended" way (sigh) ex 80BB6216 -> 1662BB80
            if ((strHash.StartsWith("80") || strHash.StartsWith("81")) &&
                (!strHash.EndsWith("80") && !strHash.EndsWith("81")) && strHash.Length == 8)
            {
                byte[] bytes = Helpers.HexStringToByteArray(strHash);
                Array.Reverse(bytes);
                strHash = BitConverter.ToString(bytes).Replace("-", "");
            }

            hash = new FileHash(strHash);
        }

        if (!hash.IsValid())
        {
            if (uint.TryParse(strHash, out uint apiHash))
            {
                Investment.LazyInit();
                InventoryItem? item = Investment.Get().TryGetInventoryItem(new TigerHash(apiHash));
                if (item is not null)
                {
                    MainWindow.Progress.SetProgressStages(new() { "Starting investment system" });
                    Investment.LazyInit();
                    MainWindow.Progress.CompleteStage();

                    item.Load();
                    ItemView apiItemView = new(item);
                    _mainWindow.MakeNewTab(item.Name, apiItemView);
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
                data.AppendLine($"PKG ID: {hash.PackageId:X4}");
                data.AppendLine($"Entry Index: {hash.FileIndex}");
                data.AppendLine($"Type: {hash.GetFileMetadata().Type} | SubType: {hash.GetFileMetadata().SubType}");
                data.AppendLine($"Reference Hash: {hash.GetReferenceHash().Hash32:X2}");
                var h64 = Hash64Map.Get().GetHash64(hash);
                if (h64 != FileHash.InvalidHash32)
                {
                    data.AppendLine($"Hash64: {h64:X2}");
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
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
        string saveDirectory = config.GetExportSavePath() + $"/Sound/{info.Hash}_{info.Name}/";
        Directory.CreateDirectory(saveDirectory);
        wem.SaveToFile($"{saveDirectory}/{info.Name}.wav");
    }

    private void AddWindow(FileHash hash)
    {
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
                    EntityView entityView = new();
                    entityView.LoadEntity(hash);

                    Entity entity = FileResourcer.Get().GetFile<Entity>(hash);
                    List<Entity> entities = new() { entity };
                    entities.AddRange(entity.GetEntityChildren());

                    if (entity.ModelParent is not null)
                    {
                        var permutations = entity.ModelParent.GetModelPermutations();
                        if (permutations is not null)
                        {
                            //Console.WriteLine($"Configuration:");
                            //Console.WriteLine($"Permutation Index: {permutations.CalculatePermutationIndex()}");
                            //foreach (var kvp in permutations.Configuration)
                            //{
                            //    var k = GlobalStrings.Get().GetString(kvp.Key);
                            //    var v = GlobalStrings.Get().GetString(kvp.Value);
                            //    Console.WriteLine($"Key: {k}, Value: {v}");
                            //}

                            Console.WriteLine($"\nKeys:");
                            foreach (var kvp in permutations.Keys)
                            {
                                var k = GlobalStrings.Get().GetString(kvp.Key);
                                Console.WriteLine($"Key: {k} ({kvp.Key})");
                                foreach (var v in kvp.Value)
                                {
                                    var vn = GlobalStrings.Get().GetString(v);
                                    Console.WriteLine($"Value: {vn} ({v})");
                                }
                            }

                            Console.WriteLine($"\nPairsToPermutation:");
                            foreach (var kvp in permutations.PairsToPermutation)
                            {
                                Console.WriteLine($"Keys for permutation {kvp.Value}:");
                                foreach (var key in kvp.Key)
                                {
                                    var item1 = GlobalStrings.Get().GetString(key.Item1);
                                    var item2 = GlobalStrings.Get().GetString(key.Item2);
                                    Console.WriteLine($"{item1} : {item2}");
                                }
                            }

                            var newConfig = new Dictionary<uint, uint>
                            {
                                { 2954315994, 980603538 }, // color, red
                                { 4164757166, 84696443 }, // grate, c
                                { 2995982517, 2256756024 } // invert, enable
                            };

                            ModelPermutation.UpdateConfiguration(permutations, newConfig);

                            Console.WriteLine($"\nUpdated Configuration:");
                            Console.WriteLine($"Permutation Index: {permutations.CalculatePermutationIndex()}");
                            foreach (var kvp in permutations.Configuration)
                            {
                                var k = GlobalStrings.Get().GetString(kvp.Key);
                                var v = GlobalStrings.Get().GetString(kvp.Value);
                                Console.WriteLine($"Key: {k}, Value: {v}");
                            }
                        }
                    }

                    Entity.Export(entities, hash);
                    _mainWindow.MakeNewTab(hash, entityView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808071a7:
                case 0x80806D44:
                    StaticView staticView = new();
                    staticView.LoadStatic(hash, ExportDetailLevel.MostDetailed);
                    _mainWindow.MakeNewTab(hash, staticView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x808093AD:
                    MapView mapView = new();
                    mapView.LoadMap(hash, ExportDetailLevel.LeastDetailed);
                    _mainWindow.MakeNewTab(hash, mapView);
                    _mainWindow.SetNewestTabSelected();
                    break;

                case 0x80808E8E:
                    ActivityView activityView = new();
                    activityView.LoadActivity(hash);
                    _mainWindow.MakeNewTab(hash, activityView);
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
                    var materialView = new MaterialView2();
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
                    ExporterScene scene = Exporter.Get().CreateScene(hash, ExportType.Entities);
                    scene.AddModel(entityModel);
                    List<DynamicMeshPart> parts = entityModel.Load(ExportDetailLevel.MostDetailed, null);
                    foreach (DynamicMeshPart part in parts)
                    {
                        if (part.Material == null) continue;
                        scene.Materials.Add(new ExportMaterial(part.Material));
                    }
                    Exporter.Get().Export();

                    EntityView entityModelView = new();
                    entityModelView.LoadEntityModel(hash);
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
                    ExporterScene decoratorScene = Exporter.Get().CreateScene(hash, ExportType.Decorators);
                    ExporterScene treesScene = Exporter.Get().CreateScene(hash, ExportType.SpeedTrees);
                    decorator.LoadIntoExporter(decoratorScene, treesScene, ConfigSubsystem.Get().GetExportSavePath());
                    Exporter.Get().Export();
                    break;

                // Testing
                case 0x80801AF2:
                case 0x808071DC:
                case 0x80806DA1:
                    Tag<S80806DA1> lightData = FileResourcer.Get().GetSchemaTag<S80806DA1>(hash);
                    TfxBytecodeInterpreterHLSL bytecode = new(TfxBytecodeOp.ParseAll(lightData.TagData.Bytecode));
                    _ = bytecode.Evaluate(lightData.TagData.Buffer1, true);

                    //foreach (var a in bytecode_hlsl)
                    //{
                    //    Console.WriteLine($"\n{a.Key} : {a.Value}\n");
                    //}
                    break;

                // Scopes / gear dye (which is a scope)
                case 0x80806DBA:
                    Dye scope_data = FileResourcer.Get().GetFile<Dye>(hash);

                    Console.WriteLine($"\n---- PIXEL ----");
                    _ = scope_data.TagData.Pixel.Value.GetBytecode().Evaluate(scope_data.TagData.Pixel.Value.TFX_Bytecode_Constants, true);
                    Console.WriteLine($"\n---- Vertex ----");
                    _ = scope_data.TagData.Vertex.Value.GetBytecode().Evaluate(scope_data.TagData.Vertex.Value.TFX_Bytecode_Constants, true);
                    break;

                case 0x80808AC5:
                    Tag<S80808AC5> skyComplex = FileResourcer.Get().GetSchemaTag<S80808AC5>(hash);
                    var a = (S80808B43)skyComplex.TagData.Pointer.GetValue(skyComplex.GetReader());

                    Console.WriteLine($"\n{skyComplex.Hash}: Unk00 {a.Unk00.Count}");
                    for (int i = 0; i < a.Unk00.Count; i += 3)
                    {
                        Vector3 half = new(a.Unk00[i].Value, a.Unk00[i + 1].Value, a.Unk00[i + 2].Value);
                        Console.WriteLine(half);
                    }
                    break;

                case 0x8080695B: // decal tag 5B698080, 5BF3AC80
                    //Decals decal = FileResourcer.Get().GetFile<Decals>(hash);
                    //decal.ExportCube($"C:\\Users\\Michael\\Desktop\\cube\\cube.obj", decal.GetCube());
                    //decal.DebugExport("C:\\Users\\Michael\\Desktop\\cube");

                    //var allDecals = PackageResourcer.Get().GetAllFiles<Decals>();
                    //List<ShaderBytecode> shaderSize = new();

                    //foreach (var file in allDecals)
                    //{
                    //    foreach (var instance in file.TagData.DecalResources)
                    //    {
                    //        shaderSize.Add(instance.Material.Vertex.Shader);
                    //    }
                    //}

                    //var first = shaderSize.First();
                    //for (int i = 0; i < shaderSize.Count; i++)
                    //{
                    //    Console.WriteLine($"{shaderSize[i].Hash}: {shaderSize[i].GetBytecode().Count()} , {first.Hash}: {first.GetBytecode().Count()}");
                    //    Debug.Assert(shaderSize[i].GetBytecode().Equals(first.GetBytecode()), $"{shaderSize[i].Hash}, {first.Hash}");
                    //}
                    //Console.WriteLine("Yep, all the same size");

                    break;

                //case 0x80806927: // particle system, 80E11F57 taken eye test
                //    Tag<S80806927> farticle = FileResourcer.Get().GetSchemaTag<S80806927>(hash);
                //    bytecode = new(TfxBytecodeOp.ParseAll(farticle.TagData.UnkBytecode, TfxBytecodeOp.BytecodeType.Sequencer));
                //    _ = bytecode.Evaluate(farticle.TagData.UnkConstants, true);

                //    break;

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
        ConfigSubsystem config = TigerInstance.GetSubsystem<ConfigSubsystem>();
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
        string[] hashes = File.ReadAllLines(BatchList.Text);
        foreach (string hash in hashes)
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
