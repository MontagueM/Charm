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
public partial class ListControl : UserControl
{
    public ListControl()
    {
        InitializeComponent();
        DataContext = new BaseListViewModel();
    }

    public DataTemplate ConvertUserControlToDataTemplate1(UserControl userControl)
    {
        FrameworkElementFactory factory = new FrameworkElementFactory(typeof(ContentPresenter));
        factory.SetValue(ContentPresenter.ContentProperty, new Binding());

        DataTemplate dataTemplate = new DataTemplate { VisualTree = factory };
        dataTemplate.Seal();

        return dataTemplate;
    }

    public DataTemplate ConvertUserControlToDataTemplate(UserControl userControl)
    {
        DataTemplate dataTemplate = new() { VisualTree = new FrameworkElementFactory(userControl.GetType()) };
        dataTemplate.Seal();

        return dataTemplate;
    }
}
