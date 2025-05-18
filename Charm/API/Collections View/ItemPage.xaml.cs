using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Charm;

public partial class ItemPage : UserControl
{
    public List<Button> Buttons;
    public int ItemsPerPage = 3;
    public int Columns { get; set; } = 3;
    private int CurrentPage = 0;

    public ItemPage()
    {
        InitializeComponent();
        Buttons = new();
    }

    private void OnControlLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        DisplayItems();
        DataContext = this;
    }

    public void DisplayItems()
    {
        PreviousPage.Visibility = Buttons.Count > ItemsPerPage ? Visibility.Visible : Visibility.Hidden;
        NextPage.Visibility = Buttons.Count > ItemsPerPage ? Visibility.Visible : Visibility.Hidden;

        var itemsToShow = Buttons.Skip(CurrentPage * ItemsPerPage).Take(ItemsPerPage).ToList();

        // Add placeholders if necessary
        //int placeholderCount = ItemsPerPage - itemsToShow.Count;
        //for (int i = 0; i < placeholderCount; i++)
        //{
        //    itemsToShow.Add(new Button
        //    {
        //        Style = (Style)FindResource("MainItemsButton")
        //    });
        //}

        ItemList.ItemsSource = itemsToShow;
        UIHelper.AnimateFade(ItemList, 0.15f, 1f, 0.1f);
    }

    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Buttons is not null && Buttons.Count > 0)
            {
                if (CurrentPage > 0)
                {
                    CurrentPage--;
                    DisplayItems();
                }
            }

            CheckPages();

        }), DispatcherPriority.Background);
    }

    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (Buttons is not null && Buttons.Count > 0)
            {
                if ((CurrentPage + 1) * ItemsPerPage < Buttons.Count)
                {
                    CurrentPage++;
                    DisplayItems();
                }
            }

            CheckPages();

        }), DispatcherPriority.Background);
    }

    public void CheckPages()
    {
        PreviousPage.IsEnabled = CurrentPage != 0;
        NextPage.IsEnabled = Buttons.Count > 0 ? (CurrentPage + 1) * ItemsPerPage < Buttons.Count : false;
    }
}
