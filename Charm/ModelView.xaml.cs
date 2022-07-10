using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Field.Models;

namespace Charm;

public partial class ModelView : UserControl
{
    public ModelView()
    {
        InitializeComponent();
    }

    public ELOD GetSelectedLod()
    {
        ELOD selected = (ELOD)LodCombobox.SelectedIndex;
        return selected;
    }

    public int GetSelectedGroupIndex()
    {
        int selected = GroupsCombobox.SelectedIndex;
        return selected;
    }

    private Action _loadEntityFunc = null;
    private bool _bFromSelectionChange = false;

    public void SetEntityFunction(Action action)
    {
        _loadEntityFunc = action;
    }

    private void LodCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // We need the LoadEntity function bound with its data
        if (_loadEntityFunc != null)
        {
            _loadEntityFunc();
        }
    }

    private void GroupsCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _bFromSelectionChange = true;
        if (_loadEntityFunc != null)
        {
            _loadEntityFunc();
        }

        _bFromSelectionChange = false;
    }

    public void SetGroupIndices(HashSet<int> hashSet)
    {
        if (_bFromSelectionChange)
            return;
        
        GroupsCombobox.Items.Clear();
        var l = hashSet.ToList();
        l.Sort();
        
        foreach (var i in l)
        {
            GroupsCombobox.Items.Add(new ComboBoxItem
            {
                Content = $"Group {i+1}/{l.Count}",
                IsSelected = i == l.Last()
            });
        }
    }
}