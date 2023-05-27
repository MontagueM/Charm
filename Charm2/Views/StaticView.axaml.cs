using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Arithmic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HarfBuzzSharp;
using Internal.Fbx;
using ReactiveUI;
using Splat;
using Tiger;
using Tiger.Schema;

namespace Charm.Views;

internal partial class StaticView : UserControl, IViewFor<MainMenuViewModel>
{
    public StaticView()
    {
        InitializeComponent();
    }

    public MainMenuViewModel? ViewModel { get; set; }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (MainMenuViewModel?)value;
    }
}
