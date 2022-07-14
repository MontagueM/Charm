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
                    if (args[c] == "-a")
                    {
                        apiHash = Convert.ToUInt32(args[c + 1]);
                    }
                    c += 2;
                }

                if (apiHash != 0)
                {
                    // Initialise FNV handler -- must be first bc my code is shit
                    FnvHandler.Initialise();

                    // Get all hash64 -- must be before InvestmentHandler
                    TagHash64Handler.Initialise();

                    // Initialise investment
                    InvestmentHandler.Initialise();

                    // Initialise fbx handler
                    FbxHandler.Initialise();


                    DestinyHash hash = new DestinyHash(apiHash);

                    var entities = InvestmentHandler.GetEntitiesFromHash(hash);
                    InfoConfigHandler.MakeFile();
                    string meshName = hash;
                    string savePath = ConfigHandler.GetExportSavePath() + $"/API_{meshName}";
                    Directory.CreateDirectory(savePath);
        
                    foreach (var entity in entities)
                    {
                        var dynamicParts = entity.Load(ELOD.MostDetail);
                        FbxHandler.AddEntityToScene(entity, dynamicParts, ELOD.MostDetail);
                        entity.SaveMaterialsFromParts(savePath, dynamicParts);
                        entity.SaveTexturePlates(savePath);
                    }

                    FbxHandler.ExportScene($"{savePath}/{meshName}.fbx");
                    InfoConfigHandler.SetMeshName(meshName);
                    InfoConfigHandler.SetUnrealInteropPath(ConfigHandler.GetUnrealInteropPath());
                    AutomatedImporter.SaveInteropUnrealPythonFile(savePath, meshName, AutomatedImporter.EImportType.Entity);
                    InfoConfigHandler.WriteToFile(savePath);
                    Console.WriteLine($"[Charm] Saved all data to {savePath}.");
                    var a = 0;
                }
            }
        }
    }
}