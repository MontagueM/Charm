using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arithmic;
using Tiger;

namespace Charm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Log.Info("Initialising Charm subsystems");
            string[] args = Environment.GetCommandLineArgs();
            CharmInstance.Args = new CharmArgs(args);
            CharmInstance.InitialiseSubsystems();
            Log.Info("Initialised Charm subsystems");
            // todo figure out how to make sure commandlets initialise all the subsystems they need

            if (Commandlet.RunCommandlet())
            {
                return;
            }
        }
    }
}
