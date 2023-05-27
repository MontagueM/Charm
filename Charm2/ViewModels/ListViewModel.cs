using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using ReactiveUI;
using Tiger;

namespace Charm.ViewModels;

public class ListViewModel : ViewModelBase// ReactiveObject
{
    private IEnumerable<ListItemViewModel> _items;
    public ObservableCollection<ListItemViewModel> Items { get; set; }
    public SelectionModel<ListItemViewModel> Selection { get; }
    public EventHandler<TextChangedEventArgs>? SearchChanged { get; set; }

    public ListViewModel()
    {
        _items = new List<ListItemViewModel> { new ListItemViewModel("A"), new ListItemViewModel("B") };
        Items = new ObservableCollection<ListItemViewModel>(_items);

        // SearchChanged = (sender, args) => DoSearch((sender as TextBox)?.Text ?? string.Empty);
    }

    public void UpdateItems(IEnumerable<ListItemViewModel> items)
    {
        _items = items;
        Items.Clear();
        foreach (var item in items)
        {
            Items.Add(item);
        }
    }

    private CancellationTokenSource? _cancellationTokenSource;

    private async void DoSearch(string s)
    {
        // IsBusy = true;
        Items.Clear();

        foreach (ListItemViewModel listItemViewModel in _items)
        {
            if (listItemViewModel.Title.Contains(s))
            {
                Items.Add(listItemViewModel);
            }
        }
        // if (!string.IsNullOrWhiteSpace(s))
        // {
        //     var albums = await Album.SearchAsync(s);
        //
        //     foreach (var album in albums)
        //     {
        //         var vm = new AlbumViewModel(album);
        //
        //         Items.Add(vm);
        //     }
        // }

        // IsBusy = false;
    }
}
