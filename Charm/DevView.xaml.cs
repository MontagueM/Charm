using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Field.General;
using Field.Models;

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
                _mainWindow.AddWindow(hash);
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
}