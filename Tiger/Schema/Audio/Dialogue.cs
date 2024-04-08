using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Audio;

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
        foreach (var entry1 in _tag.Unk18)
        {
            foreach (var u in _tag.Unk18)
            {
                var entry = u.Unk08.GetValue(GetReader());
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
        }
        return result;
    }

    private List<dynamic?> Collapse2D97(D2Class_2D978080 entry)
    {
        List<dynamic?> sounds = new();
        foreach (dynamic? e in entry.Unk20.Select(u => u.Unk20.GetValue(GetReader())))
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

        // todo GetReader() here is wrong
        // todo do a performance comparison of using the manual GetReader vs loading automatically and ignoring it
        foreach (var e in entry.Unk28.Select(u => u.Unk40.GetValue(GetReader())))
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

public class DialogueD1
{
    public DialogueD1(FileHash hash)
    {
        Activity = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(hash);
    }
    private Tag<SUnkActivity_ROI> Activity;

    // Lord forgive me for this monstrosity of code
    public List<dynamic?> Load()
    {
        List<dynamic?> sounds = new();
        foreach (var a in Activity.TagData.Unk48)
        {
            foreach (var b in a.Unk08)
            {
                if (b.Unk34.Hash.IsInvalid())
                    continue;

                var c = FileResourcer.Get().GetSchemaTag<SF0088080>(b.Unk34.Hash);
                var c1 = FileResourcer.Get().GetSchemaTag<SF0088080_Child>(c.TagData.Unk1C);
                List<SD3408080> c2 = c1.TagData.Unk08;
                c2.AddRange(c1.TagData.Unk18);
                c2.AddRange(c1.TagData.Unk28);
                foreach (var d in c2)
                {
                    var d1 = FileResourcer.Get().GetSchemaTag<S6E078080>(d.Unk00);
                    foreach (var e in d1.TagData.Unk30)
                    {
                        foreach (var f in e.Unk18)
                        {
                            if (f.Unk00.TagData.EntityResource is null)
                                continue;

                            if (f.Unk00.TagData.EntityResource.TagData.Unk10.GetValue(f.Unk00.TagData.EntityResource.GetReader()) is SB9268080)
                            {
                                var g = ((SDA288080)f.Unk00.TagData.EntityResource.TagData.Unk18.GetValue(f.Unk00.TagData.EntityResource.GetReader())).Unk68;
                                if (g is null)
                                    continue;

                                foreach (var g2 in g.TagData.EntityResources.Select(g.GetReader(), r => r.Resource))
                                {
                                    if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && g2.GetReferenceHash() != 0x80800861)
                                        continue;
                                    EntityResource resource = FileResourcer.Get().GetFile<EntityResource>(g2);
                                    if (resource.TagData.Unk10.GetValue(resource.GetReader()) is S9A078080)
                                    {
                                        var h = (D2Class_79818080)resource.TagData.Unk18.GetValue(resource.GetReader());
                                        List<D2Class_F1918080> h1 = h.WwiseSounds1;
                                        h1.AddRange(h.WwiseSounds2);
                                        foreach (var h2 in h1)
                                        {
                                            if (h2.Unk10.GetValue(resource.GetReader()) is SAA078080 dialogue)
                                            {
                                                //if (!sounds.Select(x => dialogue.Dialogue.Hash).Any())
                                                if (!sounds.Contains(dialogue))
                                                    sounds.Add(dialogue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        return sounds;
    }
}
