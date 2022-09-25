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
        if (GroupsCombobox.SelectedItem == null)
            return -1;
        string selected = (GroupsCombobox.SelectedItem as ComboBoxItem).Content as string;
        if (selected == String.Empty)
            return -1;
        string i = selected.Split("Group ")[1].Split("/")[0];
        int index = int.Parse(i);
        return index - 1;
    }

    private Action _loadModelFunc = null;
    private bool _bFromSelectionChange = false;

    public void SetModelFunction(Action action)
    {
        _loadModelFunc = action;
    }

    private void LodCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // We need the LoadEntity function bound with its data
        if (_loadModelFunc != null)
        {
            _loadModelFunc();
        }
    }

    private void GroupsCombobox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _bFromSelectionChange = true;
        if (_loadModelFunc != null)
        {
            _loadModelFunc();
        }

        _bFromSelectionChange = false;
    }

    public void SetGroupIndices(HashSet<int> hashSet)
    {
        if (_bFromSelectionChange || hashSet.Count == 0)
            return;
        
        GroupsCombobox.Items.Clear();
        var l = hashSet.ToList();
        if (l != null)
        {
            l.Sort();
            int max = l.Last();
            foreach (var i in l)
            {
                GroupsCombobox.Items.Add(new ComboBoxItem
                {
                    Content = $"Group {i + 1}/{max + 1}",
                    IsSelected = i == l.First()
                });
            }
        }
    }
}