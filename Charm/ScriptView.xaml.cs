using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Field.General;
using Field;
using Field.Strings;
using HelixToolkit.SharpDX.Core;
using HelixToolkit.Wpf.SharpDX;
using System.Threading.Tasks;

namespace Charm;

public partial class ScriptView : UserControl
{
    public ScriptView()
    {
        InitializeComponent();
    }

    public void Load(TagHash tagHash)
    {
        Script script = PackageHandler.GetTag(typeof(Script), tagHash);
        string decompile = script.ConvertToString();
        ScriptText.Text = decompile;
    }
}