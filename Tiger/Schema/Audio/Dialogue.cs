namespace Tiger.Audio;

public class Dialogue : Tag<D2Class_B8978080>
{
    public Dialogue(FileHash hash) : base(hash)
    {

    }

    /// <summary>
    /// Generates a nested list of different sequences of audio, collapsing redundant structures.
    /// </summary>
    /// <returns>A dynamic list of D2Class_33978080, in lists of their sequence and structure.</returns>
    public List<dynamic?> Load()
    {
        List<dynamic?> result = new();
        foreach (var entry in _tag.Unk18.Select(GetReader(), u => u.Unk08.Value))
        {

            switch (entry)
            {
                case D2Class_2D978080:
                    List<dynamic?> res2d = Collapse2D97(entry);
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
                    List<dynamic?> res2a = Collapse2A97(entry);
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
                    result.Add(entry);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        return result;
    }

    private List<dynamic?> Collapse2D97(D2Class_2D978080 entry)
    {
        List<dynamic?> sounds = new();
        foreach (dynamic? e in entry.Unk20.Select(GetReader(), u => u.Unk20.Value))
        {
            switch (e)
            {
                case D2Class_2A978080:
                    List<dynamic?> result = Collapse2A97(e);
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
                    sounds.Add(e);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }

    private List<dynamic?> Collapse2A97(D2Class_2A978080 entry)
    {
        List<dynamic?> sounds = new();
        foreach (var e in entry.Unk28.Select(GetReader(), u => u.Unk40.Value))
        {
            switch (e)
            {
                case D2Class_2A978080:
                    List<dynamic?> result = Collapse2A97(e);
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
                    List<dynamic?> result2 = Collapse2D97(e);
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
                    sounds.Add(e);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }
}
