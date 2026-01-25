using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Tiger;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402;

namespace Charm;

public partial class DirectiveView : UserControl
{
    public DirectiveView()
    {
        InitializeComponent();
    }

    public void Load(FileHash hash)
    {
        if (Strategy.IsD1())
        {
            ListView.ItemsSource = GetDirectiveItemsD1(hash);
        }
        else
        {
            Tag<SC78E8080> directive = FileResourcer.Get().GetSchemaTag<SC78E8080>(hash);
            ListView.ItemsSource = GetDirectiveItems(directive);
        }
    }

    public List<DirectiveItem> GetDirectiveItems(Tag<SC78E8080> directiveTag)
    {
        // List to maintain order of directives
        var items = new List<DirectiveItem>();


        if (Strategy.IsPreBL())
        {
            foreach (S744F8080 directives in directiveTag.TagData.DirectiveTableSK)
            {
                foreach (SC98E8080 directive in directives.Directives)
                {
                    items.Add(new DirectiveItem
                    {
                        Name = directive.NameStringBL.Value.ToString() ?? "",
                        Description = directive.DescriptionStringBL.Value.ToString() ?? "",
                        Objective = $"{directive.ObjectiveStringBL.Value.ToString() ?? ""}" + (directive.ObjectiveTargetCount != 0 ? $" 0/{directive.ObjectiveTargetCount}" : ""),
                        Unknown = directive.Unk58BL.Value.ToString() ?? "",
                        Hash = directives.Unk00
                    });
                }
            }
        }
        else
        {
            foreach (SC98E8080 directive in directiveTag.TagData.DirectiveTable)
            {
                // TODO: this looks ugly, but eh?
                string nameString = Strategy.IsBL() ? directive.NameStringBL.Value.ToString() : directive.NameString.Value.ToString();
                string descString = Strategy.IsBL() ? directive.DescriptionStringBL.Value.ToString() : directive.DescriptionString.Value.ToString();
                string objString = Strategy.IsBL() ? directive.ObjectiveStringBL.Value.ToString() : directive.ObjectiveString.Value.ToString();
                string unk58String = Strategy.IsBL() ? directive.Unk58BL.Value.ToString() : directive.Unk58.Value.ToString();
                items.Add(new DirectiveItem
                {
                    Name = nameString,
                    Description = descString,
                    Objective = $"{objString}" + (directive.ObjectiveTargetCount != 0 ? $" 0/{directive.ObjectiveTargetCount}" : ""),
                    Unknown = unk58String,
                    Hash = directive.Hash
                });
            }
        }

        return items;
    }

    public List<DirectiveItem> GetDirectiveItemsD1(FileHash hash)
    {
        var items = new List<DirectiveItem>();
        Tag<SUnkActivity_ROI> Activity = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(hash);

        foreach (S0C068080 a in Activity.TagData.Unk48)
        {
            foreach (SA8068080 b in a.Unk08)
            {
                if (b.Unk34.Hash.IsInvalid())
                    continue;

                Tag<SF0088080> c = FileResourcer.Get().GetSchemaTag<SF0088080>(b.Unk34.Hash);
                Tag<SF0088080_Child> c1 = FileResourcer.Get().GetSchemaTag<SF0088080_Child>(c.TagData.Unk1C);
                List<SD3408080> c2 = c1.TagData.Unk08;
                c2.AddRange(c1.TagData.Unk18);
                c2.AddRange(c1.TagData.Unk28);
                foreach (SD3408080 d in c2)
                {
                    Tag<S6E078080> d1 = FileResourcer.Get().GetSchemaTag<S6E078080>(d.Unk00);
                    if (d1.TagData.Strings is not null)
                        GlobalStrings.Get().AddStrings(d1.TagData.Strings);

                    foreach (SE9058080 e in d1.TagData.Unk30)
                    {
                        foreach (S22428080 f in e.Unk18)
                        {
                            if (f.Unk00.TagData.EntityComponent is null)
                                continue;

                            if (f.Unk00.TagData.EntityComponent.TagData.Unk10.GetValue(f.Unk00.TagData.EntityComponent.GetReader()) is S90258080)
                            {
                                var g = ((S93298080)f.Unk00.TagData.EntityComponent.TagData.Unk18.GetValue(f.Unk00.TagData.EntityComponent.GetReader()));
                                foreach (SD7318080 directive in g.Directives)
                                {
                                    // Need to filter out duplicates
                                    if (!items.Any(item => item.Hash == f.Unk00.TagData.EntityComponent.Hash))
                                    {
                                        items.Add(new DirectiveItem
                                        {
                                            Name = GlobalStrings.Get().GetString(directive.Objective2),
                                            Description = GlobalStrings.Get().GetString(directive.Description),
                                            Objective = GlobalStrings.Get().GetString(directive.Objective),
                                            Unknown = g.DevName.Value,
                                            Hash = f.Unk00.TagData.EntityComponent.Hash
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
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

