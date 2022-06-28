using System.Collections.Generic;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Field.General;
using Field.Models;
using Field.Statics;


namespace Charm;

public partial class StaticView : UserControl
{
    public StaticContainer Container;
    public string Hash;
    private List<Part> parts;

    public StaticView(string hash)
    {
        InitializeComponent();
        GetStaticContainer(hash);
    }
    
    private void OnControlLoaded(object sender, RoutedEventArgs e)
    {
        LoadStatic(ELOD.MostDetail);
    }

    private void GetStaticContainer(string hash)
    {
        Container = new StaticContainer(new TagHash(hash));
        var a = 0;
    }

    private async void LoadStatic(ELOD detailLevel)
    {
        await Task.Run(() =>
        {
            if (Container == null)
            {
                GetStaticContainer(Hash);
            }
            parts = Container.Load(detailLevel);
        });
        MainViewModel MVM = (MainViewModel)ModelView.UCModelView.Resources["MVM"];
        var displayParts = MakeDisplayParts(parts);
        MVM.SetChildren(displayParts);
        MVM.Title = Hash;
    }

    private List<MainViewModel.DisplayPart> MakeDisplayParts(List<Part> containerParts)
    {
        List<MainViewModel.DisplayPart> displayParts = new List<MainViewModel.DisplayPart>();
        foreach (Part part in containerParts)
        {
            MainViewModel.DisplayPart displayPart = new MainViewModel.DisplayPart();
            displayPart.BasePart = part;
            displayPart.Translations.Add(Vector3.Zero);
            displayPart.Rotations.Add(Vector4.Quaternion);
            displayPart.Scales.Add(Vector3.One);
            displayParts.Add(displayPart);
        }
        return displayParts;
    }
}