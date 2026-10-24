namespace Tiger.Schema.Entity;

public class EntityAnimations : EntityComponent
{
    public HashSet<Animation.Animation> AnimationTags = new();
    public List<EntitySequencer>? Sequences;
    public EntityAnimations(FileHash resource) : base(resource)
    {
    }

    public void GetAllAnimations()
    {
        if (Sequences != null)
        {
            foreach (var sequence in Sequences)
                AnimationTags.UnionWith(sequence.GetSequencerAnimations());

            //foreach (var ents in Sequences.Select(x => x.GetSequencerEntities()))
            //{
            //    foreach (var ent in ents)
            //    {
            //        if (ent is null || ent.Sequences is null)
            //            continue;

            //        foreach (var sequence in ent.Sequences)
            //            AnimationTags.UnionWith(sequence.GetSequencerAnimations());
            //    }
            //}

        }

        if (Hash == FileHash.InvalidHash32)
            return;

        if (GetUnk18() is not S808025F8 resource)
            return;

        foreach (var entry in resource.Container.TagData.Clips)
        {
            if (entry.Animation is null)
                continue;

            AnimationTags.Add(entry.Animation);
        }

        if (Strategy.IsD1())
            return;

        foreach (var entry in resource.Container.TagData.Clips2)
        {
            if (entry.Animation is null)
                continue;

            AnimationTags.Add(entry.Animation);
        }

        if (resource.Container.TagData.UnkExtraClips != null)
        {
            foreach (var entry in resource.Container.TagData.UnkExtraClips.TagData.Unk08)
            {
                foreach (var entry2 in entry.Unk10)
                {
                    if (entry2.Animation is null)
                        continue;

                    AnimationTags.Add(entry2.Animation);
                }
                foreach (var entry2 in entry.Unk20)
                {
                    if (entry2.Animation is null)
                        continue;

                    AnimationTags.Add(entry2.Animation);
                }
            }
        }
    }

    public Dictionary<FileHash, List<string>>? GetWeaponAnimations(TigerHash weaponTag)
    {
        if (Hash == FileHash.InvalidHash32 || weaponTag is null || !weaponTag.IsValid())
            return null;

        if (GetUnk18() is not S808025F8 resource || resource.QueryTable is null || resource.Container is null)
            return null;

        List<dynamic?> queries = ReadQueries(resource);
        if (queries.Any(x => x is not (S808025E1 or S808025DF)))
            return null;

        Dictionary<int, List<string>> names = QueryNames(resource);
        uint[]? mask = queries.OfType<S808025DF>()
            .SelectMany(x => x.Nodes)
            .FirstOrDefault(x => x.Tag.Hash32 == weaponTag.Hash32).Mask;

        Dictionary<int, List<string>> rows = new();
        for (int i = 0; i < queries.Count; i++)
        {
            var query = queries[i];
            if (query is S808025E1 single)
            {
                AddRows(rows, single.Table.Select(x => x.Index), names.GetValueOrDefault(i));
                continue;
            }

            S808025DF tree = query;
            HashSet<int> leaves = new();
            if (tree.Branches.Count == 0)
                leaves.Add(0);
            else
                TreeLeaves(tree, 0, weaponTag.Hash32, mask, leaves, new HashSet<int>(), 0);

            foreach (int leaf in leaves)
            {
                if (leaf >= 0 && leaf < tree.Sets.Count)
                    AddRows(rows, tree.Sets[leaf].Table.Select(x => x.Index), names.GetValueOrDefault(i));
            }
        }

        return RowAnimations(resource, rows);
    }

    public Dictionary<FileHash, List<string>>? GetEntityAnimations()
    {
        if (Hash == FileHash.InvalidHash32)
            return null;

        if (GetUnk18() is not S808025F8 resource || resource.QueryTable is null || resource.Container is null)
            return null;

        List<dynamic?> queries = ReadQueries(resource);
        Dictionary<int, List<string>> names = QueryNames(resource);

        Dictionary<int, List<string>> rows = new();
        for (int i = 0; i < queries.Count; i++)
        {
            var query = queries[i];
            if (query is S808025E1 single)
            {
                AddRows(rows, single.Table.Select(x => x.Index), names.GetValueOrDefault(i));
            }
            else if (query is S808025DF tree)
            {
                foreach (var set in tree.Sets)
                    AddRows(rows, set.Table.Select(x => x.Index), names.GetValueOrDefault(i));
            }
        }

        return RowAnimations(resource, rows);
    }

    private static List<dynamic?> ReadQueries(S808025F8 resource)
    {
        using TigerReader reader = resource.QueryTable.GetReader();
        return resource.QueryTable.TagData.Unk18.Select(x => x.Pointer.GetValue(reader)).ToList();
    }

    private static Dictionary<int, List<string>> QueryNames(S808025F8 resource)
    {
        Dictionary<int, List<string>> names = new();
        foreach (var key in resource.QueryTable.TagData.Table)
        {
            if (!names.TryGetValue(key.Index, out var list))
                names.Add(key.Index, list = new List<string>());

            var name = GlobalStrings.Get().GetString(key.Name);
            if (name.Length <= 8 && name.All(Uri.IsHexDigit))
                continue;

            list.Add(name);
        }
        return names;
    }

    private static void AddRows(Dictionary<int, List<string>> rows, IEnumerable<int> indices, List<string>? names)
    {
        foreach (int index in indices)
        {
            if (!rows.TryGetValue(index, out var list))
                rows.Add(index, list = new List<string>());

            foreach (string name in names ?? new List<string>())
            {
                if (!list.Contains(name))
                    list.Add(name);
            }
        }
    }

    private static Dictionary<FileHash, List<string>> RowAnimations(S808025F8 resource, Dictionary<int, List<string>> rows)
    {
        var container = resource.Container.TagData;
        Dictionary<FileHash, List<string>> animations = new();
        foreach (var (row, rowNames) in rows)
        {
            if (row < 0 || row >= container.Clips2.Count)
                continue;

            var entry = container.Clips2[row];
            Animation.Animation? animation = entry.Animation;
            if (animation is null)
            {
                int slot = entry.Unk18;
                if (slot >= 0 && slot < container.Clips.Count)
                    animation = container.Clips[slot].Animation;
            }

            if (animation is null)
                continue;

            if (!animations.TryGetValue(animation.Hash, out var list))
                animations.Add(animation.Hash, list = new List<string>());

            foreach (string name in rowNames)
            {
                if (!list.Contains(name))
                    list.Add(name);
            }
        }

        return animations;
    }

    private static void TreeLeaves(S808025DF tree, int branch, uint tag, uint[]? mask, HashSet<int> leaves, HashSet<int> visited, int depth)
    {
        if (depth > 512 || branch < 0 || branch >= tree.Branches.Count || !visited.Add(branch))
            return;

        S80802629 entry = tree.Branches[branch];
        if (entry.Node < 0 || entry.Node >= tree.Nodes.Count)
            return;

        if (entry.Slot == 3)
        {
            int node = Walk(tree, entry.Node, tag, mask, depth + 1);
            Follow(tree, node == -1 ? entry.Node : node, tag, mask, leaves, visited, depth + 1);
            return;
        }

        foreach (int node in Subtree(tree, entry.Node))
            Follow(tree, node, tag, mask, leaves, visited, depth + 1);
    }

    private static void Follow(S808025DF tree, int node, uint tag, uint[]? mask, HashSet<int> leaves, HashSet<int> visited, int depth)
    {
        short result = tree.Nodes[node].Result;
        if (result >= 0)
            TreeLeaves(tree, result - 1, tag, mask, leaves, visited, depth);
        else
            leaves.Add(~result);
    }

    private static int Walk(S808025DF tree, int node, uint tag, uint[]? mask, int depth)
    {
        int found = -1;
        while (depth++ < 512 && node >= 0 && node < tree.Nodes.Count)
        {
            S8080262A entry = tree.Nodes[node];
            if (entry.Tag.Hash32 == tag)
                return node;

            if (Holds(entry.Mask, mask))
            {
                if (entry.Child != -1)
                    found = Walk(tree, entry.Child, tag, mask, depth);
                if (found == -1)
                    return node;
            }

            if (entry.Sibling == -1)
                return found;

            node = entry.Sibling;
        }
        return found;
    }

    private static IEnumerable<int> Subtree(S808025DF tree, int start)
    {
        HashSet<int> seen = new();
        Stack<int> pending = new();
        pending.Push(start);
        while (pending.Count > 0)
        {
            int node = pending.Pop();
            if (node < 0 || node >= tree.Nodes.Count || !seen.Add(node))
                continue;

            yield return node;
            pending.Push(tree.Nodes[node].Sibling);
            pending.Push(tree.Nodes[node].Child);
        }
    }

    private static bool Holds(uint[]? node, uint[]? tag)
    {
        if (node is null || tag is null || node.Length != tag.Length)
            return false;

        int last = node.Length - 1;
        if ((node[last] & tag[last] & 0x80000000) == 0)
            return false;

        for (int i = 0; i < node.Length; i++)
        {
            if ((node[i] & ~tag[i]) != 0)
                return false;
        }
        return true;
    }
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802B92, 0x330)] //922B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803466, 0x700)] //66348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802613, 0x830)] //13268080, component 0x8
public struct S80802613
{
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808020BB, 0xD8)] //BB208080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080344B, 0x120)] //4B348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025F8, 0x130)] //F8258080
public struct S808025F8
{
    [SchemaField(0x90, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x90, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xA8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S8080289F> Container;

    [SchemaField(0x9C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x9C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xB4, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80802612> QueryTable;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080222A, 0x28)] //2A228080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803465, 0x28)] //65348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802612, 0x28)] //12268080
public struct S80802612
{
    [SchemaField(0x8)]
    public DynamicArray<S808025DB> Table;
    public DynamicArray<S808025DC> Unk18;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802153, 0x8)] //53218080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080342E, 0x8)] //2E348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025DB, 0x8)] //DB258080
public struct S808025DB
{
    public StringHash Name;
    public int Index; // i think
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808023C9, 0x10)] //C9238080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080342F, 0x10)] //2F348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025DC, 0x10)] //DC258080
public struct S808025DC
{
    public int Unk00;
    [SchemaField(0x8)]
    public ResourcePointer Pointer; // E1258080
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080277A, 0x10)] //7A278080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803434, 0x10)] //34348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025E1, 0x10)] //E1258080
public struct S808025E1
{
    public DynamicArray<S808025E6> Table;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802831, 0x8)] //31288080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803439, 0x8)] //39348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025E6, 0x8)] //E6258080
public struct S808025E6
{
    public int Index;    // index into Container.Clips2
    public float Weight;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080200E, 0x70)] //0E208080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803432, 0x70)] //32348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025DF, 0x70)] //DF258080
public struct S808025DF
{
    public DynamicArray<S808025E4> Sets;
    public DynamicArray<S80808912> Slots;
    public DynamicArray<S8080262A> Nodes;
    public DynamicArray<S80802629> Branches;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802A9D, 0x10)] //9D2A8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80803437, 0x10)] //37348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808025E4, 0x10)] //E4258080
public struct S808025E4
{
    public DynamicArray<S808025E6> Table;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080480D, 0x1)] //0D448080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808DA4, 0x1)] //A48D8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808912, 0x1)] //12898080
public struct S80808912
{
    public byte Slot;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802B27, 0x14)] //272B8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080347D, 0x1C)] //7D348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080262A, 0x24)] //2A268080
public struct S8080262A
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, ArraySizeConst = 2)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, ArraySizeConst = 4)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, ArraySizeConst = 6)]
    public uint[] Mask;
    public TigerHash Tag;
    public short Child;
    public short Sibling;
    public short Result;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808022C3, 0x4)] //C3228080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080347C, 0x4)] //7C348080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802629, 0x4)] //29268080
public struct S80802629
{
    public byte Slot;
    public byte Unk01;
    public short Node;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802C0E, 0x90)] //0E2C8080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808036F6, 0x90)] //F6368080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080289F, 0x78)]
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080289F, 0x80)] //9F288080
public struct S8080289F
{
    [SchemaField(0x8)]
    public DynamicArray<S80808BDF> Clips;

    [SchemaField(0x58, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x48, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public DynamicArray<S80808AEA> ClipNames; // unsure
    public DynamicArray<S80808BDE> Clips2;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x8C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0x74, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80802889> UnkExtraClips;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080408D, 0x4)] //8D408080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F48, 0x4)] //488F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BDF, 0x10)] //DF8B8080
public struct S80808BDF
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800525, 0x4)] //25058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808F1C, 0x4)] //1C8F8080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808AEA, 0x4)] //EA8A8080
public struct S80808AEA
{
    public StringHash ClipName;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800577, 0x20)] //77058080
[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x80808BDE, 0x20)] //02908080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808BDE, 0x30)] //DE8B8080
public struct S80808BDE
{
    [SchemaField(0x10)]
    public TigerHash Unk10;
    public float Unk14; // duration?

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public short Unk18; // index into Clips1?
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808036BD, 0x18)] //BD368080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80802889, 0x18)] //89288080
public struct S80802889
{
    [SchemaField(0x8)]
    public DynamicArray<S808023DF> Unk08;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x8080323C, 0x40)] //3C328080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x808023DF, 0x40)] //DF238080
public struct S808023DF
{
    public TigerHash Unk00;
    [SchemaField(0x10)]
    public DynamicArray<S8080255E> Unk10;
    public DynamicArray<S8080255D> Unk20;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808033B1, 0xC)] //B1338080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080255E, 0x18)] //5E258080
public struct S8080255E
{
    public TigerHash Unk00;
    public float Unk04;

    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;
}

[SchemaStruct(TigerStrategy.DESTINY2_SHADOWKEEP_2601, 0x808033B0, 0x10)] //B0338080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x8080255D, 0x38)] //5D258080
public struct S8080255D
{
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x8, TigerStrategy.DESTINY2_BEYONDLIGHT_3402, Tag64 = true)]
    public Animation.Animation Animation;

    public TigerHash Unk18;
}
