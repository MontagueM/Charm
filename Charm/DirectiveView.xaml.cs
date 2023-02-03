using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tiger;
using Tiger.General;

namespace Charm;

public partial class DirectiveView : UserControl
{
    public DirectiveView()
    {
        InitializeComponent();
    }
    
    public void Load(TagHash hash)
    {
        Tag<D2Class_C78E8080> directive = PackageHandler.GetTag<D2Class_C78E8080>(hash);
        
        ListView.ItemsSource = GetDirectiveItems(directive.Header.DirectiveTable);
    }

    public List<DirectiveItem> GetDirectiveItems(List<D2Class_C98E8080> directiveTable)
    {
        // List to maintain order of directives
        var items = new List<DirectiveItem>();
        
        foreach (var directive in directiveTable)
        {
            items.Add(new DirectiveItem
            {
                Name = directive.NameString,
                Description = directive.DescriptionString,
                Objective = $"{directive.ObjectiveString} 0/{directive.ObjectiveTargetCount}",
                Unknown = directive.Unk58,
                Hash = directive.Hash
            });
        }

        return items;
    }
}

public class DirectiveItem
{
    private string _objective;
    
    public string Name { get; set; }
    public string Description { get; set; }

    public string Objective
    {
        get => _objective.Contains("0/0") ? "" : _objective;
        set => _objective = value;
    }
    public string Unknown { get; set; }
    public string Hash { get; set; }
}

