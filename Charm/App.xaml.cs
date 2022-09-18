using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Field.General;
using Field.Models;
using Field.Textures;

namespace Charm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args;
            if (args.Length > 0)
            {
                uint apiHash = 0;
                int c = 0;
                while (c < args.Length)
                {
                    if (args[c] == "--api")
                    {
                        apiHash = Convert.ToUInt32(args[c + 1]);
                        break;
                    }
                    c++;
                }
                if (apiHash != 0)
                {
                    // to check if we need to update caches
                    PackageHandler.Initialise();
                    
                    // Initialise FNV handler -- must be first bc my code is shit
                    FnvHandler.Initialise();

                    // Get all hash64 -- must be before InvestmentHandler
                    TagHash64Handler.Initialise();

                    // Initialise investment
                    InvestmentHandler.Initialise();
                    
                    FbxHandler fbxHandler = new FbxHandler();

                    DestinyHash hash = new DestinyHash(apiHash);

                    var entities = InvestmentHandler.GetEntitiesFromHash(hash);
                    string meshName = hash;
                    string savePath = ConfigHandler.GetExportSavePath() + $"/API_{meshName}";
                    Directory.CreateDirectory(savePath);
        
                    foreach (var entity in entities)
                    {
                        var dynamicParts = entity.Load(ELOD.MostDetail);
                        fbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail);
                        var settings = new ExportSettings() {
                            Unreal = ConfigHandler.GetUnrealInteropEnabled(),
                            Blender = ConfigHandler.GetBlenderInteropEnabled(),
                            Source2 = true,
                            Raw = true
                        };
                        entity.SaveMaterialsFromParts(savePath, dynamicParts, settings);
                        entity.SaveTexturePlates(savePath);
                    }

                    fbxHandler.InfoHandler.SetMeshName(meshName);
                    if (ConfigHandler.GetUnrealInteropEnabled())
                    {
                        fbxHandler.InfoHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
                        AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());
                        //AutomatedImporter.SaveInteropBlenderPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity, ConfigHandler.GetOutputTextureFormat());
                    }
                    fbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
                    Console.WriteLine($"[Charm] Saved all data to {savePath}.");
                    //Shutdown();
                }
            }
        }
    }
}