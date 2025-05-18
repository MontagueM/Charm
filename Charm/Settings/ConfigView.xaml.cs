using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Charm;

public partial class ConfigView : UserControl
{
    public ObservableCollection<ButtonData> ButtonCollection { get; set; }
    private Dictionary<string, UIElement> controlMapping;

    public ConfigView()
    {
        InitializeComponent();
        MouseMove += UserControl_MouseMove;

        controlMapping = new Dictionary<string, UIElement>
        {
            { "GENERAL", ConfigControl },
            { "S&BOX", Source2Control },
            { "UNREAL", UnrealControl }
        };
        ButtonCollection = new ObservableCollection<ButtonData>
        {
            new() { Text = "GENERAL" },
            new() { Text = "S&BOX" },
            new() { Text = "UNREAL" }
        };

        Settings.ItemsSource = ButtonCollection;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UIHelper.SelectRadioButton(Settings, 0);
    }

    // Not ideal but it works
    // Wanted to use TabItems but couldnt get styling to work right
    private void SettingsCategory_OnClick(object sender, RoutedEventArgs e)
    {
        RadioButton button = sender as RadioButton;
        ButtonData textBlock = button.DataContext as ButtonData;
        string buttonText = textBlock.Text;

        // Set all controls to collapsed initially
        foreach (UIElement control in controlMapping.Values)
        {
            UIHelper.AnimateFade(control, 0.25f, 0.25f, 1);
            control.Visibility = Visibility.Collapsed;
        }

        // Set the corresponding control to visible
        if (controlMapping.TryGetValue(buttonText, out UIElement targetControl))
        {
            targetControl.Visibility = Visibility.Visible;
            UIHelper.AnimateFade(targetControl, 0.25f, 1, 0.25f);
        }
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        System.Windows.Point position = e.GetPosition(this);
        TranslateTransform gridTransform = (TranslateTransform)SettingsPage.RenderTransform;
        gridTransform.X = position.X * -0.0075;
        gridTransform.Y = position.Y * -0.0075;
    }

    public class ButtonData
    {
        public string Text { get; set; }
    }
}
