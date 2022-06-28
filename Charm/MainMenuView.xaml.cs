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
        if (e.Key == Key.Return)
        {
            string hash = TagHashBox.Text.Replace(" ", "");
            if (Field.General.Tag.CheckTagHashValid(hash))
            {
                AddWindow(hash);
            }
            else
            {
                TagHashBox.Text = "INVALID HASH";
            }
        }
        else if (e.Key == Key.H)
        {
            string hash = TagHashBox.Text.Replace(" ", "");
            if (Field.General.Tag.CheckTagHashValid(hash))
            {
                if (hash.Length == 16)
                {
                    hash = TagHash64Handler.GetTagHash64(hash);
                }
                OpenHxD(hash);
            }
            else
            {
                TagHashBox.Text = "INVALID HASH";
            } 
        }
        else if (e.Key == Key.R)
        {
            string hash = TagHashBox.Text.Replace(" ", "");
            if (Field.General.Tag.CheckTagHashValid(hash))
            {
                string refHash = PackageHandler.GetEntryReference(hash);
                if (Field.General.Tag.CheckTagHashValid(refHash))
                {
                    OpenHxD(refHash);
                }
                else
                {
                    TagHashBox.Text = "INVALID HASH";
                } 
            }
            else
            {
                TagHashBox.Text = "INVALID HASH";
            } 
        }
    }

    private void OpenHxD(string hash)
    {
        string path = $"I:/temp/{hash}.bin";
        using (var fileStream = new FileStream(path, FileMode.Create))
        {
            using (var writer = new BinaryWriter(fileStream))
            {
                byte[] data = new Field.General.File(hash).GetData();
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
        
    public static void AddWindow(string hash)
    {
        // Adds a new tab to the tab control
        TabItem newTab = new TabItem();
        newTab.Header = hash;
        uint reference = Endian.SwapU32(Convert.ToUInt32(PackageHandler.GetEntryReference(hash), 16));
        int hType, hSubtype;
        PackageHandler.GetEntryTypes(hash, out hType, out hSubtype);
        if (hType == 8)
        {
            switch (reference)
            {
                case 0x80809AD8:
                    newTab.Content = new DynamicView(hash);
                    break;
                case 0x80806D44:
                    newTab.Content = new StaticView(hash);
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