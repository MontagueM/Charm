using System.Runtime.InteropServices;
using Field.General;

namespace Field;

public class Dialogue : Tag
{
    public D2Class_B8978080 Header;
    
    public Dialogue(TagHash hash) : base(hash)
    {
        
    }

    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_B8978080>();
    }

    /// <summary>
    /// Generates a nested list of different sequences of audio, collapsing redundant structures.
    /// </summary>
    /// <returns>A dynamic list of D2Class_33978080, in lists of their sequence and structure.</returns>
    public List<dynamic?> Load()
    {
        List<dynamic?> result = new List<dynamic?>();
        foreach (var entry in Header.Unk18)
        {
            
            switch (entry.Unk08)
            {
                case D2Class_2D978080:
                    List<dynamic?> res2d = Collapse2D97(entry.Unk08);
                    if (res2d.Count > 1)
                    {
                        result.Add(res2d);
                    }
                    else if (res2d.Count == 1)
                    {
                        result.Add(res2d[0]);
                    }
                    break;
                case D2Class_2A978080:
                    List<dynamic?> res2a = Collapse2A97(entry.Unk08);
                    if (res2a.Count > 1)
                    {
                        result.Add(res2a);
                    }
                    else if (res2a.Count == 1)
                    {
                        result.Add(res2a[0]);
                    }
                    break;
                case D2Class_33978080:
                    result.Add(entry.Unk08);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        return result;
    }

    private List<dynamic?> Collapse2D97(D2Class_2D978080 entry)
    {
        List<dynamic?> sounds = new List<dynamic?>();
        foreach (var e in entry.Unk20)
        {
            switch (e.Unk20)
            {
                case D2Class_2A978080:
                    List<dynamic?> result = Collapse2A97(e.Unk20);
                    if (result.Count > 1)
                    {
                        sounds.Add(result);
                    }
                    else if (result.Count == 1)
                    {
                        sounds.Add(result[0]);
                    }
                    break;
                case D2Class_33978080:
                    sounds.Add(e.Unk20);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        
        return sounds;
    }
    
    private List<dynamic?> Collapse2A97(D2Class_2A978080 entry)
    {
        List<dynamic?> sounds = new List<dynamic?>();
        foreach (var e in entry.Unk28)
        {
            switch (e.Unk40)
            {
                case D2Class_2A978080:
                    List<dynamic?> result = Collapse2A97(e.Unk40);
                    if (result.Count > 1)
                    {
                        sounds.Add(result);
                    }
                    else if (result.Count == 1)
                    {
                        sounds.Add(result[0]);
                    }
                    break;
                case D2Class_2D978080:
                    List<dynamic?> result2 = Collapse2D97(e.Unk40);;
                    if (result2.Count > 1)
                    {
                        sounds.Add(result2);
                    }
                    else if (result2.Count == 1)
                    {
                        sounds.Add(result2[0]);
                    }
                    break;
                case D2Class_33978080:
                    sounds.Add(e.Unk40);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }
}