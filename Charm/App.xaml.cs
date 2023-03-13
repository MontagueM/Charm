using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Field;
using Field.General;
using Field.Models;

namespace Charm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //FixPathCache();
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
                    return;
					// Initialise FNV handler -- must be first bc my code is shit
					FnvHandler.Initialise();

					// to check if we need to update caches
					PackageHandler.Initialise();
                    
                    // Get all hash64 -- must be before InvestmentHandler
                    TagHash64Handler.Initialise();

                    // Initialise investment
                    InvestmentHandler.Initialise();
                    
                    // InvestmentHandler.DebugAllInvestmentEntities();
                    // InvestmentHandler.DebugAPIRequestAllInfo();
                    // InvestmentHandler.DebugAPIRenderMetadata();
                    
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
                        entity.SaveMaterialsFromParts(savePath, dynamicParts, ConfigHandler.GetUnrealInteropEnabled() || ConfigHandler.GetS2ShaderExportEnabled(), ConfigHandler.GetSaveCBuffersEnabled());
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

        private void FixPathCache()
        {
            string[] brokenPKGS =
            {
                "w64_sr_globals_03e8_8.pkg",
                "w64_investment_globals_client_0173_8.pkg",
                "w64_sr_destination_metadata_010a_8.pkg",
                "w64_sr_globals_011a_8.pkg",
                "w64_sr_video_040c_5.pkg",
                "w64_sr_video_0121_5.pkg"
            };

            string cacheFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "paths.cache");

            if (File.Exists(cacheFilePath))
            {
                string lines = File.ReadAllText(cacheFilePath);

                foreach(string pkg in brokenPKGS)
                {
                    Console.WriteLine($"{ConfigHandler.GetPackagesPath()}/{pkg}");
                    if(lines.Contains($"{ConfigHandler.GetPackagesPath()}/{pkg}"))
                    {
                        Console.WriteLine($"{pkg} in cache");
                    }
                }
            }
        }
    }
}