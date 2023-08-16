using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity;
using Tiger.Schema.Activity.DESTINY2_WITCHQUEEN_6307;

namespace Charm;

public partial class DirectiveView : UserControl
{
    public DirectiveView()
    {
        InitializeComponent();
    }

    public void Load(FileHash hash)
    {
        Tag<D2Class_C78E8080> directive = FileResourcer.Get().GetSchemaTag<D2Class_C78E8080>(hash);

        ListView.ItemsSource = GetDirectiveItems(directive, directive.TagData.DirectiveTable);
    }

    public List<DirectiveItem> GetDirectiveItems(Tag<D2Class_C78E8080> directiveTag, DynamicArray<D2Class_C98E8080> directiveTable)
    {
        // List to maintain order of directives
        var items = new List<DirectiveItem>();

        foreach (var directive in directiveTable)
        {
            // TODO: this looks ugly, but eh?
            string nameString = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ? directive.NameStringBL.Value.ToString() : directive.NameString.Value.ToString();
            string descString = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ? directive.DescriptionStringBL.Value.ToString() : directive.DescriptionString.Value.ToString();
            string objString = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ? directive.ObjectiveStringBL.Value.ToString() : directive.ObjectiveString.Value.ToString();
            string unk58String = Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402 ? directive.Unk58BL.Value.ToString() : directive.Unk58.Value.ToString();
            items.Add(new DirectiveItem
            {
                Name = nameString,
                Description = descString,
                Objective = $"{objString} 0/{directive.ObjectiveTargetCount}",
                Unknown = unk58String,
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

