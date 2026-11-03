using Arithmic;

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
        var rows = WeaponRows(weaponTag, out S808025F8 resource);
        return rows == null ? null : RowAnimations(resource, rows);
    }

    public Dictionary<FileHash, int>? GetWeaponAnimationRows(TigerHash weaponTag)
    {
        var rows = WeaponRows(weaponTag, out S808025F8 resource);
        if (rows == null)
            return null;

        Dictionary<FileHash, int> result = new();
        foreach (int row in rows.Keys)
        {
            if (RowClip(resource.Container.TagData, row) is { } clip)
                result.TryAdd(clip.Hash, row);
        }

        return result;
    }

    public List<(Animation.Animation Clip, int Row, List<string> Names)>? GetWeaponRowClips(TigerHash weaponTag)
    {
        var rows = WeaponRows(weaponTag, out S808025F8 resource);
        if (rows == null)
            return null;

        List<(Animation.Animation, int, List<string>)> result = new();
        foreach (var (row, names) in rows)
        {
            if (RowClip(resource.Container.TagData, row) is { } clip)
                result.Add((clip, row, names));
        }

        return result;
    }

    public HashSet<uint> GetWeaponTagLineage(TigerHash weaponTag)
    {
        HashSet<uint> lineage = new() { weaponTag.Hash32 };
        // the first hierarchy is the weapon tags
        if (GetUnk18() is not S808025F8 resource || resource.TagHierarchy is null || resource.TagHierarchy.TagData.Hierarchies.Count == 0)
            return lineage;

        var tags = resource.TagHierarchy.TagData.Hierarchies[0].Tags;
        int index = -1;
        for (int i = 0; i < tags.Count; i++)
        {
            if (tags[i].Tag.Hash32 == weaponTag.Hash32)
            {
                index = i;
                break;
            }
        }

        for (int steps = 0; index >= 0 && index < tags.Count && steps < tags.Count; steps++)
        {
            lineage.Add(tags[index].Tag.Hash32);
            int parent = tags[index].Parent;

            if (parent == index)
                break;

            index = parent;
        }

        return lineage;
    }

    public Dictionary<int, Dictionary<int, uint>> GetRowTags()
    {
        Dictionary<int, Dictionary<int, uint>> rows = new();
        if (Hash == FileHash.InvalidHash32 || GetUnk18() is not S808025F8 resource || resource.QueryTable is null)
            return rows;

        foreach (var tree in ReadQueries(resource).OfType<S808025DF>())
        {
            if (tree.Branches.Count > 0)
                CollectRowTags(tree, 0, new Dictionary<int, uint>(), rows, 0);
        }
        return rows;
    }

    private static void CollectRowTags(S808025DF tree, int branch, Dictionary<int, uint> context, Dictionary<int, Dictionary<int, uint>> rows, int depth)
    {
        if (depth > 32 || branch < 0 || branch >= tree.Branches.Count)
            return;

        S80802629 entry = tree.Branches[branch];
        foreach (int node in Subtree(tree, entry.Node))
        {
            var tags = context;
            if (entry.Slot != 3) // the weapon slot goes through the tag lineage instead
                tags = new Dictionary<int, uint>(context) { [entry.Slot] = tree.Nodes[node].Tag.Hash32 };

            short result = tree.Nodes[node].Result;
            if (result >= 0)
            {
                CollectRowTags(tree, result - 1, tags, rows, depth + 1);
                continue;
            }

            int set = ~result;
            if (set >= tree.Sets.Count)
                continue;

            foreach (var row in tree.Sets[set].Table)
            {
                if (!rows.TryGetValue(row.Index, out var known))
                    rows.Add(row.Index, known = new Dictionary<int, uint>());

                foreach (var (slot, tag) in tags)
                    known.TryAdd(slot, tag);
            }
        }
    }

    private Dictionary<int, List<string>>? WeaponRows(TigerHash weaponTag, out S808025F8 resource)
    {
        resource = default;
        if (weaponTag is null || !weaponTag.IsValid())
            return null;

        return QueryRows(weaponTag, out resource);
    }

    public Dictionary<FileHash, List<string>>? GetEntityAnimations()
    {
        var rows = QueryRows(null, out S808025F8 resource);
        return rows == null ? null : RowAnimations(resource, rows);
    }

    private Dictionary<int, List<string>>? QueryRows(TigerHash? weaponTag, out S808025F8 resource)
    {
        resource = default;
        if (Hash == FileHash.InvalidHash32 || GetUnk18() is not S808025F8 found || found.QueryTable is null || found.Container is null)
            return null;

        resource = found;

        var queries = ReadQueries(resource);
        if (weaponTag != null && queries.Any(x => x is not (S808025E1 or S808025DF)))
            return null;

        Dictionary<int, List<string>> names = QueryNames(resource);
        Dictionary<int, List<string>> rows = new();

        var sets = QuerySets(queries, weaponTag);
        for (int i = 0; i < sets.Count; i++)
        {
            foreach (var set in sets[i])
                AddRows(rows, set.Select(x => x.Index), names.GetValueOrDefault(i));
        }

        return rows;
    }

    private static List<List<List<S808025E6>>> QuerySets(List<dynamic?> queries, TigerHash? weaponTag, bool sortSets = false)
    {
        uint tag = weaponTag?.Hash32 ?? 0;
        uint[]? mask = weaponTag is null ? null : queries.OfType<S808025DF>()
            .SelectMany(x => x.Nodes)
            .FirstOrDefault(x => x.Tag.Hash32 == tag).Mask;

        List<List<List<S808025E6>>> result = new();
        foreach (var query in queries)
        {
            List<List<S808025E6>> sets = new();
            if (query is S808025E1 single)
            {
                sets.Add(single.Table);
            }
            else if (query is S808025DF tree)
            {
                IEnumerable<int> reached = Enumerable.Range(0, tree.Sets.Count);
                if (weaponTag is not null && tree.Branches.Count > 0)
                {
                    HashSet<int> leaves = new();
                    TreeLeaves(tree, 0, tag, mask, leaves, new HashSet<int>(), 0);
                    reached = sortSets ? leaves.Order() : leaves;
                }

                sets.AddRange(reached.Where(x => x >= 0 && x < tree.Sets.Count).Select(x => (List<S808025E6>)tree.Sets[x].Table));
            }

            result.Add(sets);
        }
        return result;
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
            if (RowClip(container, row) is not { } animation)
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

    // The actions and the clip sets each can pick from
    public WeaponActions? GetWeaponActions(TigerHash? weaponTag)
    {
        if (Hash == FileHash.InvalidHash32 || GetUnk18() is not S808025F8 resource || resource.QueryTable is null || resource.Container is null)
            return null;

        var querySets = QuerySets(ReadQueries(resource), weaponTag, sortSets: true);
        var container = resource.Container.TagData;
        var result = new WeaponActions { Blends = container.BlendTable?.TagData };
        var keys = resource.QueryTable.TagData.Table;

        for (int i = 0; i < querySets.Count; i++)
        {
            var sets = querySets[i];
            var action = new WeaponAction();
            foreach (var key in keys.Where(x => x.Index == i))
            {
                action.Hashes.Add(key.Name.Hash32);
                string name = GlobalStrings.Get().GetString(key.Name);

                if (!(name.Length <= 8 && name.All(Uri.IsHexDigit)))
                    action.Names.Add(name);
            }

            if (action.Hashes.Count == 0)
                continue;

            foreach (var set in sets)
            {
                var choices = SetChoices(container, set);
                if (choices.Count > 0)
                    action.Sets.Add(choices);
            }

            if (action.Sets.Count == 0)
                continue;

            if (result.Blends is S80805E38 blends)
            {
                foreach (var entry in blends.Actions)
                {
                    if (action.Hashes.Contains(entry.Action.Hash32))
                    {
                        action.Group = entry.Group;
                        break;
                    }
                }
            }

            result.Actions.Add(action);
        }

        return result;
    }

    private static List<(Animation.Animation Clip, float Chance, int Row)> SetChoices(S8080289F container, List<S808025E6> rows)
    {
        bool cumulative = rows.Count > 0 && rows.Zip(rows.Skip(1)).All(x => x.Second.Weight >= x.First.Weight) && Math.Abs(rows[^1].Weight - 1f) < 0.01f;
        float total = rows.Sum(x => Math.Max(x.Weight, 0f));

        List<(Animation.Animation, float, int)> choices = new();
        float previous = 0f;

        foreach (var row in rows)
        {
            float chance = cumulative ? row.Weight - previous : total > 0 ? Math.Max(row.Weight, 0f) / total : 1f / rows.Count;
            previous = row.Weight;

            Animation.Animation? clip = RowClip(container, row.Index);
            if (clip != null)
                choices.Add((clip, chance, row.Index));
        }

        return choices;
    }

    private static Animation.Animation? RowClip(S8080289F container, int row)
    {
        if (row < 0 || row >= container.Clips2.Count)
            return null;

        var entry = container.Clips2[row];
        if (entry.Animation is not null)
            return entry.Animation;

        int slot = entry.Unk18;
        return slot >= 0 && slot < container.Clips.Count ? container.Clips[slot].Animation : null;
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

public sealed class WeaponActions
{
    public const float DefaultBlend = 0.1f;
    public List<WeaponAction> Actions = new();
    public S80805E38? Blends;

    public float BlendTime(WeaponAction? from, WeaponAction to)
    {
        if (Blends is not S80805E38 blends)
            return DefaultBlend;

        if (from is null || from.Group < 0 || from.Group >= blends.Groups.Count)
            return blends.DefaultBlend > 0 ? blends.DefaultBlend : DefaultBlend;

        var group = blends.Groups[from.Group];
        foreach (var entry in group.Overrides)
        {
            if (entry.Group == to.Group)
                return entry.Blend;
        }

        return group.DefaultBlend;
    }
}

public sealed class WeaponAction
{
    public List<string> Names = new();
    public List<uint> Hashes = new();
    public int Group = -1; // in the blend table
    public List<List<(Animation.Animation Clip, float Chance, int Row)>> Sets = new();

    public string Name => Names.Count > 0 ? string.Join(" / ", Names) : $"{Hashes[0]:X8}";
}


public sealed record WeaponLayerClip(string Track, int Layer, int Child, Animation.Animation Clip, TrackFrameMode FrameMode, LayerFrameSource Frame, uint? WeightVariable = null)
{
    public bool Loops => FrameMode is TrackFrameMode.Clock or TrackFrameMode.Speed;

    public float Weight(LayerInputs? inputs) => WeightVariable is null ? 1f : inputs?.WeightOf(WeightVariable.Value) ?? 0f;

    public double FrameFor(double baseFrame, int baseFrames, int layerFrames)
    {
        if (layerFrames <= 1)
            return 0;

        switch (FrameMode)
        {
            case TrackFrameMode.Clock:
            case TrackFrameMode.Speed:
                return Math.Max(baseFrame, 0);

            case TrackFrameMode.Variable:
                double value = Frame.Kind switch
                {
                    LayerFrameKind.Phase => baseFrames > 1 ? baseFrame / (baseFrames - 1) : 0,
                    LayerFrameKind.Fixed => Frame.Value,
                    _ => 0,
                };
                return Math.Clamp(value, 0, 1) * (layerFrames - 1);

            default:
                return 0;
        }
    }
}

public sealed class LayerInputs
{
    private const uint WalkWeightVariable = 0x10000 | 55; // global fp_walk_weight
    private const uint RunWeightVariable = 0x10000 | 57; // global fp_run_weight
    private const uint SprintWeightVariable = 0x10000 | 93; // global sprint_state_weight

    public float Walk;
    public float Run;
    public float Sprint;

    public static readonly string[] Names = { "walk", "run", "sprint" };

    public static string? NameOf(uint variable) => variable switch
    {
        WalkWeightVariable => Names[0],
        RunWeightVariable => Names[1],
        SprintWeightVariable => Names[2],
        _ => null,
    };

    public static LayerInputs Only(string name) => new()
    {
        Walk = name == Names[0] ? 1f : 0f,
        Run = name == Names[1] ? 1f : 0f,
        Sprint = name == Names[2] ? 1f : 0f,
    };

    public float WeightOf(uint variable) => variable switch
    {
        WalkWeightVariable => Walk,
        RunWeightVariable => Run,
        SprintWeightVariable => Sprint,
        _ => 0f,
    };
}

public enum TrackFrameMode
{
    Clock = 1,    // own time from when the row starts, t += dt * rate, wrapped by the clip length
    Variable = 2, // t = clamp01(frame variable) * length. Variable = high short of +0x34 (0x40 the action phase, 0x22 the hammer...)
    Speed = 3,    // the frame variable is a playback speed, t += variable * dt, wrapped
}

public enum LayerFrameKind
{
    Zero,
    Phase,
    Fixed,
}

public readonly record struct LayerFrameSource(LayerFrameKind Kind, float Value = 0);

public sealed class AnimationVariables
{
    private const byte PhaseKind = 1;
    private const byte NamedInputKind = 20;

    private readonly Dictionary<int, LayerFrameSource> _locals = new();

    public AnimationVariables(Tag<S8080280D> table)
    {
        var data = table.TagData;
        var reader = table.GetReader();
        for (int i = 0; i < data.Locals.Count; i++)
        {
            var variable = data.Locals[i];
            if (variable.Kind == PhaseKind)
                _locals[i] = new(LayerFrameKind.Phase);
            else if (!Strategy.IsD1() && variable.Kind == NamedInputKind && variable.Default.GetValue(reader) is S8080282A value) // D1 named inputs have no plain default
                _locals[i] = new(LayerFrameKind.Fixed, value.Value.X);
        }
    }

    public LayerFrameSource FrameSource(int variable) => _locals.TryGetValue(variable, out var source) ? source : new(LayerFrameKind.Zero);
}

public sealed class WeaponLayerContext
{
    public S8080269C Tracks { get; }
    public S8080289F? Container { get; } // D1 entries play its clips by index
    public AnimationVariables? Variables { get; }
    public TigerHash WeaponTag { get; }
    public HashSet<uint> Lineage { get; } = new();
    private readonly Dictionary<FileHash, int> _rows = new();
    private readonly Dictionary<int, Dictionary<int, uint>> _rowTags = new();

    private readonly List<WeaponAnimationEntry> _entries = new();
    public IReadOnlyList<WeaponAnimationEntry> Entries => _entries;

    public IEnumerable<WeaponAnimationEntry> EntriesOf(FileHash clip) => _entries.Where(x => x.Clip.Hash == clip);

    private WeaponLayerContext(S8080269C tracks, S8080289F? container, AnimationVariables? variables, TigerHash weaponTag)
    {
        Tracks = tracks;
        Container = container;
        Variables = variables;
        WeaponTag = weaponTag;
        Lineage.Add(weaponTag.Hash32);
    }

    public static WeaponLayerContext? Create(Entity rig, TigerHash weaponTag)
    {
        S8080263C graph;
        try
        {
            if (rig.GetAnimationGraph() is not { Tracks: not null } found)
                return null;

            graph = found;
        }
        catch (Exception e)
        {
            Log.Warning($"Couldnt read the layer tracks of {rig.Hash}: {e.Message}");
            return null;
        }

        AnimationVariables? variables = null;
        try
        {
            if (graph.Variables is not null)
                variables = new AnimationVariables(graph.Variables);
        }
        catch (Exception e)
        {
            Log.Warning($"Couldnt read the animation variables of {rig.Hash}, layer tracks use frame 0: {e.Message}");
        }

        var context = new WeaponLayerContext(graph.Tracks.TagData, graph.Container?.TagData, variables, weaponTag);
        List<(Animation.Animation Clip, int Row, List<string> Names)> rowClips = new();
        foreach (EntityAnimations animations in rig.Animations ?? new())
        {
            try
            {
                foreach (var (clip, row) in animations.GetWeaponAnimationRows(weaponTag) ?? new())
                    context._rows.TryAdd(clip, row);

                rowClips.AddRange(animations.GetWeaponRowClips(weaponTag) ?? new());

                context.Lineage.UnionWith(animations.GetWeaponTagLineage(weaponTag));

                foreach (var (row, tags) in animations.GetRowTags())
                    context._rowTags.TryAdd(row, tags);
            }
            catch (Exception e)
            {
                Log.Warning($"Couldnt read the weapon rows of {animations.Hash}: {e.Message}");
            }
        }

        context.BuildEntries(rowClips);
        return context;
    }

    private void BuildEntries(List<(Animation.Animation Clip, int Row, List<string> Names)> rowClips)
    {
        Dictionary<(FileHash, string), WeaponAnimationEntry> found = new();
        foreach (var (clip, row, names) in rowClips.OrderBy(x => x.Clip.Hash).ThenBy(x => x.Row))
        {
            var key = (clip.Hash, LayerSignature(row));
            if (!found.TryGetValue(key, out var entry))
            {
                entry = new WeaponAnimationEntry(clip, row);
                found.Add(key, entry);
                _entries.Add(entry);
            }

            foreach (string name in names)
            {
                if (!entry.Names.Contains(name))
                    entry.Names.Add(name);
            }
        }

        int clips = rowClips.Select(x => x.Clip.Hash).Distinct().Count();
        Log.Debug($"Weapon {WeaponTag}: {clips} clips from {rowClips.Count} rows, {_entries.Count} animations ({_entries.Count - clips} extra for clips played with different layers)");
    }

    private string LayerSignature(int row)
    {
        try
        {
            return string.Join(",", ForRow(row).Select(x => $"{x.Clip.Hash}/{x.WeightVariable}/{(int)x.FrameMode}/{x.Frame.Kind}/{x.Frame.Value}"));
        }
        catch (Exception e)
        {
            Log.Debug($"Couldnt read the layers of row {row}: {e.Message}");
            return $"row {row}";
        }
    }

    // the first row the weapon plays the clip from
    public int? RowOf(FileHash clip) => _rows.TryGetValue(clip, out int row) ? row : null;

    public List<WeaponLayerClip> ForRow(int row) => WeaponLayers.ForRow(Tracks, row, WeaponTag, Lineage, Variables, Container, _rowTags.GetValueOrDefault(row));
}

public sealed class WeaponAnimationEntry
{
    public WeaponAnimationEntry(Animation.Animation clip, int row)
    {
        Clip = clip;
        Row = row;
    }

    public Animation.Animation Clip { get; }
    public int Row { get; }
    public List<string> Names { get; } = new();

    public string? Name => Names.Count > 0 ? Names[0] : null;
    public string DisplayName => string.Join(" / ", Names);
}

public static class WeaponLayers
{
    private const int WeaponSlot = 3;
    public static Dictionary<int, int> RowState(S8080269C definition, int row)
    {
        Dictionary<int, int> pairs = new();
        if (definition.RowStates is null || definition.States is null || row < 0 || row >= definition.RowStates.Count)
            return pairs;

        int state = definition.RowStates[row].Value;
        if (state < 0 || state >= definition.States.Count)
            return pairs;

        foreach (var pair in definition.States[state].Tracks)
            pairs[pair.Index] = pair.Child;
        return pairs;
    }

    public static List<WeaponLayerClip> ForRow(S8080269C definition, int row, TigerHash? weaponTag, IReadOnlySet<uint>? weaponLineage = null,
        AnimationVariables? variables = null, S8080289F? container = null, IReadOnlyDictionary<int, uint>? rowTags = null)
    {
        List<WeaponLayerClip> clips = new();
        foreach (var (trackIndex, child) in RowState(definition, row).OrderBy(x => x.Key))
        {
            if (trackIndex >= definition.Tracks.Count)
                continue;

            var track = definition.Tracks[trackIndex];
            if (child >= track.ChildCount)
                continue;

            foreach (var entry in track.ChildEntries(child))
            {
                if (WeaponMatch(entry, weaponTag, weaponLineage, rowTags) == false)
                    continue;

                // the first matching entry is the one that plays, even when off
                if (entry.WeightSource is 1 or 2 && (entry.Animation ?? ContainerClip(entry, container)) is { } animation)
                {
                    int frameVariable = entry.FrameVariable;
                    // globals are gameplay inputs we have no value for, only locals index the variables we read
                    var frame = frameVariable == 0xFFFF || entry.FrameVariableIsGlobal || variables is null ? new(LayerFrameKind.Zero) : variables.FrameSource(frameVariable);

                    clips.Add(new WeaponLayerClip(GlobalStrings.Get().GetString(track.Name), track.Layer, child, animation, entry.Mode, frame, entry.WeightSource == 2 ? entry.Unk28 : null));
                }
                break;
            }
        }
        return clips;
    }

    private static Animation.Animation? ContainerClip(S80802905 entry, S8080289F? container)
    {
        if (container?.Clips is not { } containerClips || entry.Clips is not { Count: > 0 } entryClips)
            return null;

        int index = entryClips[0].ClipIndex;
        return index >= 0 && index < containerClips.Count ? containerClips[index].Animation : null;
    }

    private static bool? WeaponMatch(S80802905 entry, TigerHash? weaponTag, IReadOnlySet<uint>? weaponLineage, IReadOnlyDictionary<int, uint>? rowTags)
    {
        bool? result = null;
        if (entry.Conditions is null) // D1 entries have none
            return result;

        foreach (var condition in entry.Conditions)
        {
            bool holds;
            if (condition.Slot == WeaponSlot)
                holds = weaponTag != null && (condition.Tag.Hash32 == weaponTag.Hash32 || weaponLineage?.Contains(condition.Tag.Hash32) == true);
            else if (rowTags != null && rowTags.TryGetValue(condition.Slot, out uint rowTag))
                holds = condition.Tag.Hash32 == rowTag;
            else
                continue;

            result = (result ?? true) && holds;
        }

        return result;
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

    [SchemaField(0x94, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)] // todo?
    [SchemaField(0xAC, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80808AB6> TagHierarchy;

    [SchemaField(0x9C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x9C, TigerStrategy.DESTINY2_SHADOWKEEP_2601)]
    [SchemaField(0xB4, TigerStrategy.DESTINY2_BEYONDLIGHT_3402)]
    public Tag<S80802612> QueryTable;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800368, 0x38)] //68038080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808AB6, 0xC0)] //B68A8080
public struct S80808AB6
{
    [SchemaField(0x8)]
    public DynamicArray<S80808ABA> Hierarchies;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800663, 0x20)] //63068080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80808ABA, 0x20)] //BA8A8080
public struct S80808ABA
{
    public DynamicArray<S80808ABE> Tags;
    public DynamicArray<S80808ABE> Unk10; // same count as Tags
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80800870, 0x8)] //70088080
[SchemaStruct(TigerStrategy.DESTINY2_BEYONDLIGHT_3402, 0x80808ABE, 0x8)] //BE8A8080
public struct S80808ABE
{
    public TigerHash Tag;
    public int Parent;
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

    [SchemaField(0x84, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_SHADOWKEEP_2601, Obsolete = true)]
    [SchemaField(0x6C, TigerStrategy.DESTINY2_LATEST)]
    public Tag<S80805E38> BlendTable;

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
    public float Unk14;

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

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802750, 0x40)] //50278080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805E38, 0x40)] //385E8080
public struct S80805E38
{
    [SchemaField(0x8)]
    public float DefaultBlend; // seconds

    [SchemaField(0x10)]
    public DynamicArray<S80805E42> Actions;
    public DynamicArray<S80805E3F> Groups;
    public DynamicArray<S80805E3C> Unk30;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802AAF, 0x8)] //AF2A8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805E42, 0x8)] //425E8080
public struct S80805E42
{
    public StringHash Action; // query name, idle, fire, etc
    public int Group;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802BBE, 0x18)] //BE2B8080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805E3F, 0x18)] //3F5E8080
public struct S80805E3F
{
    public float DefaultBlend;
    [SchemaField(0x8)]
    public DynamicArray<S80805E41> Overrides;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802073, 0x8)] //73208080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805E41, 0x8)] //415E8080
public struct S80805E41
{
    public int Group; // blending into this group
    public float Blend;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080210F, 0x8)] //0F218080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80805E3C, 0x8)] //3C5E8080
public struct S80805E3C
{
    public StringHash Unk00;
}

// First person rig animation graph resource
[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802790, 0xF8)] //90278080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080263C, 0x150)] //3C268080
public struct S8080263C
{
    [SchemaField(0xE0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x138, TigerStrategy.DESTINY2_LATEST)]
    public Tag<S8080289F> Container;

    [SchemaField(0xE4, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x140, TigerStrategy.DESTINY2_LATEST)]
    public Tag<S8080280D> Variables;

    [SchemaField(0xF0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x14C, TigerStrategy.DESTINY2_LATEST)]
    public Tag<S8080269C> Tracks;
}


[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808029E1, 0x28)] //E1298080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080280D, 0x38)] //0D288080
public struct S8080280D
{
    [SchemaField(0x8)]
    public DynamicArray<S80802811> Globals;
    public DynamicArray<S80802811> Locals;
    // +0x28 conditions 80802809
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808020B4, 0x10)] //B4208080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80802811, 0x18)] //11288080
public struct S80802811
{
    public TigerHash Name;
    public byte Kind;
    public byte Table;

    [SchemaField(0x8)]
    public ResourcePointer Data;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public ResourcePointer Default; // S8080282A
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080282A, 0x10)] //2A288080
public struct S8080282A
{
    public Vector4 Value;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802919, 0x48)] //19298080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080269C, 0x48)] //9C268080
public struct S8080269C
{
    [SchemaField(0x8)]
    public DynamicArray<S808028F9> Tracks;
    // +0x18 5 parameter layers 808028F6 (look_pitch, look, throttle, velocity, 8407553C)

    [SchemaField(0x28)]
    public DynamicArray<SInt16> RowStates;
    public DynamicArray<S808026A0> States;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x8080254E, 0x20)] //4E258080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808026A0, 0x20)] //A0268080
public struct S808026A0
{
    public DynamicArray<S808026A2> Tracks;
    public DynamicArray<S808026A2> ParameterLayers;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808028E6, 0x4)] //E6288080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808026A2, 0x4)] //A2268080
public struct S808026A2
{
    public ushort Child;
    public ushort Index; // track, or parameter layer
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x808022CE, 0x18)] //CE228080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x808028F9, 0x20)] //F9288080
public struct S808028F9
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S80802903> Children;

    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(TigerStrategy.DESTINY2_LATEST, Obsolete = true)]
    public DynamicArray<S80802905> Entries;

    [SchemaField(0x10)]
    public StringHash Name;
    public int Layer; // index into the container's layer names

    public int ChildCount => Children?.Count ?? Entries?.Count ?? 0;

    public IEnumerable<S80802905> ChildEntries(int child)
    {
        if (Children != null)
            return child >= 0 && child < Children.Count ? Children[child].Entries : Enumerable.Empty<S80802905>();

        return Entries != null && child >= 0 && child < Entries.Count ? new[] { Entries[child] } : Enumerable.Empty<S80802905>();
    }
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80802903, 0x10)] //03298080
public struct S80802903
{
    public DynamicArray<S80802905> Entries; // first one whose conditions match?
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802744, 0x30)] //44278080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x80802905, 0x50)] //05298080
public struct S80802905
{
    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x0, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S8080290B> Conditions;

    [SchemaField(0x0, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x10, TigerStrategy.DESTINY2_LATEST)]
    public DynamicArray<S8080290C> Clips;

    [SchemaField(0x10, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x20, TigerStrategy.DESTINY2_LATEST)]
    public int WeightSource; // 1 constant (Unk24), otherwise the variable in Unk28 clamped to 0..1, 0 when it can't be read

    [SchemaField(0x14, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x24, TigerStrategy.DESTINY2_LATEST)]
    public int Unk24; // 1 with a constant weight, float in D1

    [SchemaField(0x18, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x28, TigerStrategy.DESTINY2_LATEST)]
    public uint Unk28; // the weight variable (S8080280D id), 0x1xxxx globals or 0xFFFF

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x2C, TigerStrategy.DESTINY2_LATEST)]
    public uint Unk2C;

    [SchemaField(0x1C, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x30, TigerStrategy.DESTINY2_LATEST)]
    public uint Unk30; // -FLT_MAX or 1

    [SchemaField(0x24, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x34, TigerStrategy.DESTINY2_LATEST)]
    public uint Unk34; // the frame variable, latest high short, D1 low short with its table in the high short (0 locals, 1 globals)

    [SchemaField(0x20, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x38, TigerStrategy.DESTINY2_LATEST)]
    public uint Unk38; // latest the frame variable's table, 1 globals 0x10000 locals. D1 1/30 as a float

    [SchemaField(0x28, TigerStrategy.DESTINY1_RISE_OF_IRON)]
    [SchemaField(0x3C, TigerStrategy.DESTINY2_LATEST)]
    public uint FrameMode;

    [SchemaField(TigerStrategy.DESTINY1_RISE_OF_IRON, Obsolete = true)]
    [SchemaField(0x40, TigerStrategy.DESTINY2_LATEST, Tag64 = true)]
    public Animation.Animation Animation;

    public int FrameVariable => (int)(Strategy.IsD1() ? Unk34 & 0xFFFF : Unk34 >> 16);
    public bool FrameVariableIsGlobal => Strategy.IsD1() ? Unk34 >> 16 == 1 : Unk38 == 1;
    public TrackFrameMode Mode => (TrackFrameMode)(Strategy.IsD1() ? FrameMode >> 24 : FrameMode);
}

[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080290B, 0x8)] //0B298080
public struct S8080290B
{
    public int Slot; // tag slot, 3 weapon, 20 class
    public TigerHash Tag;
}

[SchemaStruct(TigerStrategy.DESTINY1_RISE_OF_IRON, 0x80802687, 0x8)] //87268080
[SchemaStruct(TigerStrategy.DESTINY2_LATEST, 0x8080290C, 0x8)] //0C298080
public struct S8080290C
{
    public int ClipIndex; // into the container's Clips
    public float Weight;
}
