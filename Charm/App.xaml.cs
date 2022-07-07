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

namespace Charm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
         
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // todo args
            if(e.Args.Length == 1)
                MessageBox.Show("Now opening file: \n\n" + e.Args[0]);
        }
    }
}