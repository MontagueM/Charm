using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Windows.Input;
using ReactiveUI;
using Tiger;

namespace Charm.ViewModels;

public class ListItemViewModel
{
    public TigerHash Hash { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public int FontSize { get; set; }
    public string Type { get; set; }

    public ListItemViewModel(string title)
    {
        Hash = new TigerHash();
        Title = title;
        Subtitle = "Subtitle 1";
        FontSize = 16;
        Type = "Type 1";
    }

}
