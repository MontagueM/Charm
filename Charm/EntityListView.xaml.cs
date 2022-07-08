using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Field;
using Field.Entities;
using Field.General;
using Field.Models;

namespace Charm;

public enum EEntityListType
{
    DestinationGlobalTagBag,
    BudgetSet,
}

public partial class EntityListView : UserControl
{
    public EntityListView()
    {
        InitializeComponent();
    }
    
    public void LoadContent(EEntityListType entityListType, TagHash hash)
    {
        Dictionary<string, Tag> tags = new Dictionary<string, Tag>();

        if (entityListType == EEntityListType.DestinationGlobalTagBag)
        {
            Tag<D2Class_30898080> destinationGlobalTagBag = new Tag<D2Class_30898080>(hash);
            foreach (var entry in destinationGlobalTagBag.Header.Unk18)
            {
                tags.Add(entry.Unk00, entry.Unk08);
            }
        }
        else
        {
            Tag<D2Class_ED9E8080> budgetSet = new Tag<D2Class_ED9E8080>(hash);
            foreach (var entry in budgetSet.Header.Unk28)
            {
                tags.Add(entry.Unk00, entry.Unk08);
            }
        }

        

        
        
        foreach (var kvp in tags)
        {
            if (kvp.Key.Contains(".fx_sequence.tft"))
            {
                continue;
            }
            // allNames.Add(kvp.Key);
            var btn = new ToggleButton();
            btn.Focusable = true;

            btn.Content = new TextBlock
            {
                Text = (bool)TrimNamesCBox.IsChecked ? TrimName(kvp.Key) : kvp.Key,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13
            };
            
            btn.Height = 70;
            btn.HorizontalContentAlignment = HorizontalAlignment.Left;
            btn.Click += Entity_Click;

            EntityPanel.Children.Add(btn);
        }
        ScrollView.ScrollToTop();
    }

    private void Entity_Click(object sender, RoutedEventArgs e)
    {
        foreach (ToggleButton button in EntityPanel.Children)
        {
            button.IsChecked = false;
        }
        (sender as ToggleButton).IsChecked = true;
        var index = EntityPanel.Children.IndexOf(sender as ToggleButton);
        // var btnText = allNames[index];
        // var tag = tags[btnText];
        // if (btnText.Contains(".pattern.tft"))
        // {
            // EntityView.LoadEntity(tag.Hash);
        // }
        // else if (btnText.Contains(".budget_set.tft"))
        // {
            // var budgetSetHeader = new Tag<D2Class_7E988080>(tag.Hash);
            // MainMenuView.AddWindow(budgetSetHeader.Header.Unk00.Hash);
        // }
        // else
        // {
            // throw new NotImplementedException();
        // }
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < EntityPanel.Children.Count; i++)
        {
            ToggleButton button = EntityPanel.Children[i] as ToggleButton;
            // (button.Content as TextBlock).Text = TrimName(allNames[i]);
        }
    }

    private string TrimName(string name)
    {
        return name.Split("\\").Last().Split(".")[0];
    }

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        for (int i = 0; i < EntityPanel.Children.Count; i++)
        {
            ToggleButton button = EntityPanel.Children[i] as ToggleButton;
            // (button.Content as TextBlock).Text = allNames[i];
        }
    }
}