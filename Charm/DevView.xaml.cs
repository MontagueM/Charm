using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tiger;
using Tiger.Exporters;
using Tiger.Schema;
using Tiger.Schema.Audio;
using Tiger.Schema.Entity;

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
    }

    private void TagHashBoxKeydown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return && e.Key != Key.H && e.Key != Key.R && e.Key != Key.E && e.Key != Key.L)
        {
            return;
        }
        string strHash = TagHashBox.Text.Replace(" ", "");
        strHash = Regex.Replace(strHash, @"(\s+|r|h)", "");
        if (strHash.Length == 16)
        {
            strHash = Hash64Map.Get().GetHash32Checked(strHash);
        }
        if (strHash == "")
        {
            TagHashBox.Text = "INVALID HASH";
            return;
        }

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
            TagHashBox.Text = "INVALID HASH";
            return;
        }
        //uint to int
        switch (e.Key)
        {
            case Key.L:
                StringBuilder data = new();
                data.AppendLine($"PKG: {PackageResourcer.Get().PackagePathsCache.GetPackagePathFromId(hash.PackageId)})");
                data.AppendLine($"PKG ID: {hash.PackageId}");
                data.AppendLine($"Entry Index: {hash.FileIndex}");
                // data.AppendLine($"Dev String: {hash.GetDevString() ?? hash.GetContainerString() ?? "NULL"}");
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
        if (fileMetadata.Type == 26 && fileMetadata.SubType == 7)
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
                    dialogueView.Load(hash);
                    _mainWindow.MakeNewTab(hash, dialogueView);
                    _mainWindow.SetNewestTabSelected();
                    break;
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

    private void OpenHxD(FileHash hash)
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

    private void ExportDevMapButton_OnClick(object sender, RoutedEventArgs e)
    {
        // Not actually a map, but a list of assets that are good for testing
        // The assets are assembled in UE5 so just have to rip the list
        var assets = new List<string>()
        {
            "6C24BB80",
            "a237be80",
            "b540be80",
            "68a8b480",
            "fba4b480",
            "e1c5b280",
            "0F3CBE80",
            "A229BE80",
            "B63BBE80",
            "CB32BE80",
        };

        foreach (var asset in assets)
        {
            StaticView.ExportStatic(new FileHash(asset), asset, ExportTypeFlag.Full, "devmap");
        }
    }
}
