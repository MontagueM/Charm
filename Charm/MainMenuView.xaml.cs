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
    private static TabItem _newestTab = null;
        
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
        
        // forcing investment
        EntityView investmentView = new EntityView();
        investmentView.LoadEntityFromApi(new DestinyHash(UInt32.Parse(strHash)));
        _newestTab = new TabItem();
        _newestTab.Header = strHash;
        _newestTab.Content = investmentView;
        _mainWindow.MainTabControl.Items.Add(_newestTab);
        SetNewestTabSelected();
        return;
        
        if (strHash.Length == 16)
        {
            strHash = TagHash64Handler.GetTagHash64(strHash);
        }
        if (strHash == "")
        {
            TagHashBox.Text = "INVALID HASH";
            return;
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
        _newestTab = new TabItem();
        _newestTab.Header = hash.GetHashString();
        DestinyHash reference = PackageHandler.GetEntryReference(hash);
        int hType, hSubtype;
        PackageHandler.GetEntryTypes(hash, out hType, out hSubtype);
        if ((hType == 8 || hType == 16) && hSubtype == 0)
        {
            switch (reference.Hash)
            {
                case 0x80809AD8:
                    EntityView dynamicView = new EntityView();
                    dynamicView.LoadEntity(hash);
                    _newestTab.Content = dynamicView;
                    break;
                case 0x80806D44:
                    StaticView staticView = new StaticView(hash);
                    staticView.LoadStatic(ELOD.MostDetail);
                    _newestTab.Content = staticView;
                    break;
                case 0x808093AD:
                    MapView mapView = new MapView(hash);
                    mapView.LoadMap();
                    _newestTab.Content = mapView;
                    break;
                case 0x80808E8E:
                    ActivityView activityView = new ActivityView();
                    activityView.LoadActivity(hash);
                    _newestTab.Content = activityView;
                    SetNewestTabSelected();
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

        _mainWindow.MainTabControl.Items.Add(_newestTab);
    }

    public static void SetNewestTabSelected()
    {
        _mainWindow.MainTabControl.SelectedItem = _newestTab;
    }
    
    public static void SetNewestTabName(string newName)
    {
        _newestTab.Header = newName.Replace('_', '.');
    }

    public static void MakeNewTab(string name, UserControl content)
    {
        // Check if the name already exists, if so set newest tab to that
        var items = _mainWindow.MainTabControl.Items;
        foreach (TabItem item in items)
        {
            if (name == (string) item.Header)
            {
                _newestTab = item;
                return;
            }
        }
        
        _newestTab = new TabItem();
        _newestTab.Content = content;
        _mainWindow.MainTabControl.Items.Add(_newestTab);
        SetNewestTabName(name);
    }

    private void ApiViewButton_OnClick(object sender, RoutedEventArgs e)
    {
        ApiView apiView = new ApiView();
        apiView.LoadApiView();
        MakeNewTab("API View", apiView);
        SetNewestTabSelected();
    }
}