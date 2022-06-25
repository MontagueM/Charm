using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Charm;

public partial class ModelView : UserControl
{
    public ModelView()
    {
        InitializeComponent();
    }

    // Menu buttons

    private void Grid_Checked(object sender, RoutedEventArgs e)
    {
        // if (HelixGrid != null)
        // {
        // HelixGrid.Visible = true;
        // }
    }

    private void Grid_Unchecked(object sender, RoutedEventArgs e)
    {
        // if (HelixGrid != null)
        // {
        // HelixGrid.Visible = false;
        // }
    }
}