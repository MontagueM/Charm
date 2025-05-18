using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;
using VersionChecker;

namespace Charm
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ApplicationVersion CurrentVersion = new("2.7.0");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Idk why for some people Charm is looking at system32 instead of the exe location...
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            if (!IsVcRedistInstalled())
            {
                MessageBoxResult result = MessageBox.Show(
                    "Charm requires Visual C++ Redistributables to function properly, would you like to install these now?",
                    "Missing Dependency",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );
                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version",
                        UseShellExecute = true
                    });
                    Environment.Exit(0);
                }
            }

            string[] args = e.Args;
            if (args.Length > 0)
            {
                uint apiHash = 0;
                int c = 0;
                while (c < args.Length)
                {
                    if (args[c] == "--api")
                    {
                        apiHash = Convert.ToUInt32(args[c + 1]);
                        break;
                    }
                    c++;
                }
                if (apiHash != 0)
                {
                    return;
                }
            }
        }

        bool IsVcRedistInstalled()
        {
            // Key for VC++ 2015-2022 Redistributable (x64)
            const string keyPath = @"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object installed = key.GetValue("Installed");
                    if (installed is int value && value == 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    // Idk where else to put this, I don't want to make a whole new file
    public static class StyleHelper
    {
        // BorderThickness attached property
        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.RegisterAttached(
                "BorderThickness",
                typeof(Thickness),
                typeof(StyleHelper),
                new PropertyMetadata(new Thickness(1))); // Default thickness

        public static void SetBorderThickness(UIElement element, Thickness value)
        {
            element.SetValue(BorderThicknessProperty, value);
        }

        public static Thickness GetBorderThickness(UIElement element)
        {
            return (Thickness)element.GetValue(BorderThicknessProperty);
        }

        // BackgroundColor attached property
        public static readonly DependencyProperty BackgroundColorProperty =
            DependencyProperty.RegisterAttached(
                "BackgroundColor",
                typeof(Brush),
                typeof(StyleHelper),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#433C3C41")))); // Default background color

        public static void SetBackgroundColor(UIElement element, Brush value)
        {
            element.SetValue(BackgroundColorProperty, value);
        }

        public static Brush GetBackgroundColor(UIElement element)
        {
            return (Brush)element.GetValue(BackgroundColorProperty);
        }
    }

    public static class UIHelper
    {
        public static void AnimateFade(dynamic obj, float seconds, float to = 1, float from = 0)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                DoubleAnimation fadeInAnimation = new();
                fadeInAnimation.From = from;
                fadeInAnimation.To = to;
                fadeInAnimation.Duration = TimeSpan.FromSeconds(seconds);
                obj.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);

            }), DispatcherPriority.Background);
        }

        public static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                {
                    return t;
                }
                T result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                T result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public static List<T> GetChildrenOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            var children = new List<T>();
            if (depObj == null) return children;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T)
                {
                    children.Add(child as T);
                }
                else
                {
                    children.AddRange(GetChildrenOfType<T>(child));
                }
            }
            return children;
        }

        public static void SelectRadioButton(ItemsControl itemsControl, int index)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (index < 0 || index >= itemsControl.Items.Count)
                    return;

                object item = itemsControl.Items[index];
                if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is ContentPresenter contentPresenter)
                {
                    RadioButton radioButton = UIHelper.FindVisualChild<RadioButton>(contentPresenter);
                    if (radioButton != null)
                    {
                        radioButton.IsChecked = true;
                    }
                }
            }), DispatcherPriority.Background);
        }
    }
}

