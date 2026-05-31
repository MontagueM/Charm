using Arithmic;
using Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Audio;

public class Dialogue : Tag<SDialogueTable>
{
    public Dialogue(FileHash hash) : base(hash)
    {

    }

    /// <summary>
    /// Generates a nested list of different sequences of audio, collapsing redundant structures.
    /// </summary>
    /// <returns>A dynamic list of S33978080, in lists of their sequence and structure.</returns>
    public List<dynamic?> Load()
    {
        List<dynamic?> result = new();
        using TigerReader reader = GetReader();
        foreach (S29978080 entry1 in _tag.Unk18)
        {
            foreach (S29978080 u in _tag.Unk18)
            {
                dynamic? entry = u.Unk08.GetValue(reader);
                switch (entry)
                {
                    case S2D978080:
                        List<dynamic?> res2d = Collapse2D97(entry, reader);
                        if (res2d.Count > 0)
                        {
                            result.Add(res2d.Count > 1 ? res2d : res2d[0]);
                        }
                        break;
                    case S2A978080:
                        List<dynamic?> res2a = Collapse2A97(entry, reader);
                        if (res2a.Count > 0)
                        {
                            result.Add(res2a.Count > 1 ? res2a : res2a[0]);
                        }
                        break;
                    case S33978080:
                        result.Add(entry);
                        break;
                    case S1D8D8080: // Shadowkeep
                        List<dynamic?> res1d = Collapse1D8D(entry, reader);
                        if (res1d.Count > 0)
                        {
                            result.Add(res1d.Count > 1 ? res1d : res1d[0]);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        return result;
    }

    private List<dynamic?> Collapse2D97(S2D978080 entry, TigerReader reader)
    {
        List<dynamic?> sounds = new();
        foreach (dynamic? e in entry.Unk20.Select(u => u.Unk20.GetValue(reader)))
        {
            switch (e)
            {
                case S2A978080:
                    List<dynamic?> result = Collapse2A97(e, reader);
                    if (result.Count > 0)
                    {
                        sounds.Add(result.Count > 1 ? result : result[0]);
                    }
                    break;
                case S33978080:
                    sounds.Add(e);
                    break;
                case S8080B6CE:
                    break;
                default:
                    Log.Debug($"Unknown Dialogue Table Unk20 in {Hash}!");
                    break;
                    //throw new NotImplementedException();
            }
        }

        return sounds;
    }

    private List<dynamic?> Collapse2A97(S2A978080 entry, TigerReader reader)
    {
        List<dynamic?> sounds = new();

        // todo GetReader() here is wrong
        // todo do a performance comparison of using the manual GetReader vs loading automatically and ignoring it
        foreach (dynamic? e in entry.Unk28.Select(u => u.Unk40.GetValue(reader)))
        {
            switch (e)
            {
                case S2A978080:
                    List<dynamic?> result = Collapse2A97(e, reader);
                    if (result.Count > 0)
                    {
                        sounds.Add(result.Count > 1 ? result : result[0]);
                    }
                    break;
                case S2D978080:
                    List<dynamic?> result2 = Collapse2D97(e, reader);
                    if (result2.Count > 0)
                    {
                        sounds.Add(result2.Count > 1 ? result2 : result2[0]);
                    }
                    break;
                case S33978080:
                    sounds.Add(e);
                    break;
                case S1D8D8080: // Shadowkeep
                    List<dynamic?> result3 = Collapse1D8D(e, reader);
                    if (result3.Count > 0)
                    {
                        sounds.Add(result3.Count > 1 ? result3 : result3[0]);
                    }
                    break;
                case S8080B6CE:
                    break;
                default:
                    Log.Debug($"Unknown Dialogue Table Unk28 in {Hash}!");
                    throw new NotImplementedException();
            }
        }

        return sounds;
    }

    private List<dynamic?> Collapse1D8D(S1D8D8080 entry, TigerReader reader)
    {
        List<dynamic?> sounds = new();
        foreach (dynamic? e in entry.Unk18.Select(u => u.Pointer.GetValue(reader)))
        {
            switch (e)
            {
                case S2A978080:
                    List<dynamic?> result = Collapse2A97(e, reader);
                    if (result.Count > 0)
                    {
                        sounds.Add(result.Count > 1 ? result : result[0]);
                    }
                    break;
                case S33978080:
                    sounds.Add(e);
                    break;
                case S8080B6CE:
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
                    foreach (SE9058080 e in d1.TagData.Unk30)
                    {
                        foreach (S22428080 f in e.Unk18)
                        {
                            if (f.Unk00.TagData.EntityComponent is null)
                                continue;

                            if (f.Unk00.TagData.EntityComponent.TagData.Unk10.GetValue(f.Unk00.TagData.EntityComponent.GetReader()) is SB9268080)
                            {
                                Entity.Entity? g = ((SDA288080)f.Unk00.TagData.EntityComponent.TagData.Unk18.GetValue(f.Unk00.TagData.EntityComponent.GetReader())).Unk68;
                                if (g is null)
                                    continue;

                                foreach (FileHash? g2 in g.Components)
                                {
                                    if (Strategy.IsD1() && g2.GetReferenceHash() != 0x80800861)
                                        continue;
                                    EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(g2);
                                    if (resource.TagData.Unk10.GetValue(resource.GetReader()) is S79948080)
                                    {
                                        var h = (S79818080)resource.TagData.Unk18.GetValue(resource.GetReader());
                                        List<SF1918080> h1 = h.Array1;
                                        h1.AddRange(h.Array2);
                                        h1.AddRange(h.D1Array3);
                                        foreach (SF1918080 h2 in h1)
                                        {
                                            if ((h2.Unk10.GetValue(resource.GetReader()) is SAA078080 dialogue) && !sounds.Contains(dialogue))
                                            {
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
