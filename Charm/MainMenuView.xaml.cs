using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using Field.General;
using Field.Models;
using Field.Textures;
using SharpDX.Toolkit.Graphics;

namespace Charm;

public partial class MainMenuView : UserControl
{
    private static MainWindow _mainWindow = null;
        
    public MainMenuView()
    {
        InitializeComponent();
    }
        
    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        _mainWindow = Window.GetWindow(this) as MainWindow;
    }

    private void TagHashBoxKeydown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Return && e.Key != Key.H && e.Key != Key.R)
        {
            return;
        }
        string strHash = TagHashBox.Text.Replace(" ", "");
        if (strHash.Length == 16)
        {
            strHash = TagHash64Handler.GetTagHash64(strHash);
        }
        TagHash hash = new TagHash(strHash);
        if (!hash.IsValid())
        {
            TagHashBox.Text = "INVALID HASH";
            return;
        }

        switch (e.Key)
        {
            case Key.Return:
                AddWindow(hash);
                break;
            case Key.H:
                OpenHxD(hash);
                break;
            case Key.R:
                TagHash refHash = PackageHandler.GetEntryReference(hash);
                OpenHxD(refHash);
                break;
        }
    }

    private void OpenHxD(TagHash hash)
    {
        string path = $"I:/temp/{hash}.bin";
        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            using (var writer = new BinaryWriter(fileStream))
            {
                byte[] data = new DestinyFile(hash).GetData();
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
        
    public static void AddWindow(TagHash hash)
    {
        // Adds a new tab to the tab control
        TabItem newTab = new TabItem();
        newTab.Header = hash.GetHashString();
        DestinyHash reference = PackageHandler.GetEntryReference(hash);
        int hType, hSubtype;
        PackageHandler.GetEntryTypes(hash, out hType, out hSubtype);
        if ((hType == 8 || hType == 16) && hSubtype == 0)
        {
            switch (reference.Hash)
            {
                case 0x80809AD8:
                    DynamicView dynamicView = new DynamicView(hash);
                    dynamicView.LoadDynamic(ELOD.MostDetail);
                    newTab.Content = dynamicView;
                    break;
                case 0x80806D44:
                    StaticView staticView = new StaticView(hash);
                    staticView.LoadStatic(ELOD.MostDetail);
                    newTab.Content = staticView;
                    break;
                case 0x808093AD:
                    MapView mapView = new MapView(hash);
                    mapView.LoadMap();
                    newTab.Content = mapView;
                    break;
                case 0x80808E8E:
                    ActivityView activityView = new ActivityView();
                    newTab.Header = activityView.LoadActivity(hash);
                    newTab.Content = activityView;
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

        _mainWindow.MainTabControl.Items.Add(newTab);
        _mainWindow.MainTabControl.SelectedItem = newTab;
    }
}