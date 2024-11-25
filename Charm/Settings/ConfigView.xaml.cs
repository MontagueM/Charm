using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Tiger;

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
            new ButtonData { Text = "GENERAL" },
            new ButtonData { Text = "S&BOX" },
            new ButtonData { Text = "UNREAL" }
        };

        Settings.ItemsSource = ButtonCollection;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        SelectRadioButton(Settings, 0);

        if (ConfigSubsystem.Get().GetAnimatedBackground())
        {
            SpinnerShader _spinner = new SpinnerShader();
            Spinner.Effect = _spinner;
            SizeChanged += _spinner.OnSizeChanged;
            _spinner.ScreenWidth = (float)ActualWidth;
            _spinner.ScreenHeight = (float)ActualHeight;
            _spinner.Scale = new(4, 4);
            _spinner.Offset = new(-3.6, -3.3);
            SpinnerContainer.Visibility = Visibility.Visible;
        }
    }

    // Not ideal but it works
    // Wanted to use TabItems but couldnt get styling to work right
    private void SettingsCategory_OnClick(object sender, RoutedEventArgs e)
    {
        RadioButton button = sender as RadioButton;
        ButtonData textBlock = button.DataContext as ButtonData;
        string buttonText = textBlock.Text;

        // Set all controls to collapsed initially
        foreach (var control in controlMapping.Values)
        {
            UIHelper.AnimateFadeIn(control, 0.25f, 0.25f, 1);
            control.Visibility = Visibility.Collapsed;
        }

        // Set the corresponding control to visible
        if (controlMapping.TryGetValue(buttonText, out UIElement targetControl))
        {
            targetControl.Visibility = Visibility.Visible;
            UIHelper.AnimateFadeIn(targetControl, 0.25f, 1, 0.25f);
        }
    }

    public void SelectRadioButton(ItemsControl itemsControl, int index)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (index < 0 || index >= itemsControl.Items.Count)
                return;

            var item = itemsControl.Items[index];
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
            {
                var radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                if (radioButton != null)
                {
                    radioButton.IsChecked = true;
                }
            }
        }), DispatcherPriority.Background);
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
