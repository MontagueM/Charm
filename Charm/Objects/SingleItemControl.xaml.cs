using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Arithmic;
using Internal.Fbx;
using Tiger;
using Tiger.Schema;

namespace Charm.Objects;

/// <summary>
/// The current implementation of Package is limited so you cannot have nested views below a Package.
/// For future, would be better to split the tag items up so we can cache them based on parents.
/// </summary>
public partial class SingleItemControl : UserControl
{
    public SingleItemControl()
    {
        InitializeComponent();
        DataContext = new BaseListViewModel();
    }

    public Type DataType { get; set; }
    public Type ItemType { get; set; }

    public void LoadView(FileControl fileControl)
    {
        // (DataContext as BaseItemViewModel).LoadView(fileControl, ItemType, DataType);
    }

    public void LoadDataView()
    {
        // (DataContext as BaseItemViewModel).LoadDataView(ItemType, DataType);
    }
}
