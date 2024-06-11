using System.Collections.Concurrent;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Activity
{
    public struct Bubble
    {
        public string Name;
        public Tag<SBubbleDefinition> ChildMapReference;
    }

    public struct ActivityEntities
    {
        public string BubbleName;
        public string ActivityPhaseName2;
        public FileHash Hash;
        public List<FileHash> DataTables;
        public Dictionary<ulong, ActivityEntity> WorldIDs; //World ID, name/subname
    }

    public interface IActivity : ISchema
    {
        public FileHash FileHash { get; }
        public IEnumerable<Bubble> EnumerateBubbles();
        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null);
    }

    public struct ActivityEntity
    {
        public string Name;
        public string SubName;
    }
}


namespace Tiger.Schema.Activity.DESTINY1_RISE_OF_IRON
{
    public class Activity : Tag<SActivity_ROI>, IActivity
    {
        public FileHash FileHash => Hash;

        public Activity(FileHash hash) : base(hash)
        {
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            for (int bubbleIndex = 0; bubbleIndex < _tag.Bubbles.Count; bubbleIndex++)
            {
                var bubble = _tag.Bubbles[bubbleIndex];
                if (bubble.ChildMapReference is null)
                    continue;

                yield return new Bubble { Name = GetBubbleNameFromBubbleIndex(bubbleIndex), ChildMapReference = bubble.ChildMapReference };
            }
        }

        private string GetBubbleNameFromBubbleIndex(int index)
        {
            return GlobalStrings.Get().GetString(_tag.LocationNames.TagData.BubbleNames.First(e => e.BubbleIndex == index).BubbleName);
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null)
        {
            Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(UnkActivity);
            foreach (var entry in tag.TagData.Unk48)
            {
                foreach (var entry2 in entry.Unk08)
                {
                    yield return new ActivityEntities
                    {
                        BubbleName = GlobalStrings.Get().GetString(entry2.UnkName1),
                        Hash = entry2.Unk34.Hash,
                        ActivityPhaseName2 = GlobalStrings.Get().GetString(entry2.UnkName0),
                        DataTables = CollapseResourceParent(entry2.Unk34.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();

            var entry = FileResourcer.Get().GetSchemaTag<SF0088080>(hash);
            var entry2 = FileResourcer.Get().GetSchemaTag<SF0088080_Child>(entry.TagData.Unk1C);
            var entries = entry2.TagData.Unk08;
            entries.AddRange(entry2.TagData.Unk18);
            entries.AddRange(entry2.TagData.Unk28);

            foreach (var resource in entries)
            {
                var Unk00 = FileResourcer.Get().GetSchemaTag<S6E078080>(resource.Unk00);
                foreach (var a in Unk00.TagData.Unk30)
                {
                    if (a.Unk10 is not null && a.Unk10.TagData.DataEntries.Count > 0)
                        if (!items.Contains(a.Unk10.Hash))
                            items.Add(a.Unk10.Hash);

                    foreach (var b in a.Unk18)
                    {
                        if (!items.Contains(b.Unk00.Hash))
                            items.Add(b.Unk00.Hash);

                        // For NPCs, enemies and other AI (it's cool but not really worth adding)
                        //if (b.Unk00.TagData.EntityResource.TagData.Unk10.GetValue(b.Unk00.TagData.EntityResource.GetReader()) is SBC078080 c)
                        //{
                        //    var d = (SA7058080)b.Unk00.TagData.EntityResource.TagData.Unk18.GetValue(b.Unk00.TagData.EntityResource.GetReader());
                        //    if (!items.Contains(d.Unk68.Hash))
                        //        items.Add(d.Unk68.Hash);
                        //}
                    }
                }
            }

            return items.ToList();
        }
    }
} // I wonder what this is for

namespace Tiger.Schema.Activity.DESTINY2_SHADOWKEEP_2601
{
    public class Activity : Tag<SActivity_SK>, IActivity
    {
        public FileHash FileHash => Hash;

        public Activity(FileHash hash) : base(hash)
        {
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            for (int bubbleIndex = 0; bubbleIndex < _tag.Bubbles.Count; bubbleIndex++)
            {
                var bubble = _tag.Bubbles[bubbleIndex];
                if (bubble.MapReference is null ||
                    bubble.MapReference.TagData.ChildMapReference == null)
                {
                    continue;
                }
                yield return new Bubble { Name = GetBubbleNameFromBubbleIndex(bubbleIndex), ChildMapReference = bubble.MapReference.TagData.ChildMapReference };
            }
        }

        private string GetBubbleNameFromBubbleIndex(int index)
        {
            return GlobalStrings.Get().GetString(_tag.LocationNames.TagData.BubbleNames.First(e => e.BubbleIndex == index).BubbleName);
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null)
        {
            Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(UnkActivity);
            foreach (var entry in tag.TagData.Unk50)
            {
                foreach (var entry2 in entry.Unk08)
                {
                    yield return new ActivityEntities
                    {
                        BubbleName = GlobalStrings.Get().GetString(entry2.UnkName1),
                        Hash = entry2.Unk44.Hash,
                        ActivityPhaseName2 = GlobalStrings.Get().GetString(entry2.UnkName0),
                        DataTables = CollapseResourceParent(entry2.Unk44.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();

            var entry = FileResourcer.Get().GetSchemaTag<DESTINY2_SHADOWKEEP_2601.S5B928080>(hash);

            // :)))
            foreach (var resource in entry.TagData.Unk14.TagData.Unk08)
            {
                foreach (var a in resource.Unk00.TagData.Unk38)
                {
                    foreach (var table in a.Unk08)
                    {
                        if (table.Unk00 is null || table.Unk00.TagData.DataTable is null)
                            continue;

                        if (table.Unk00.TagData.DataTable.TagData.DataEntries.Count > 0)
                        {
                            items.Add(table.Unk00.TagData.DataTable.Hash);
                        }
                    }
                }
            }
            foreach (var resource in entry.TagData.Unk14.TagData.Unk18)
            {
                foreach (var a in resource.Unk00.TagData.Unk38)
                {
                    foreach (var table in a.Unk08)
                    {
                        if (table.Unk00 is null || table.Unk00.TagData.DataTable is null)
                            continue;

                        if (table.Unk00.TagData.DataTable.TagData.DataEntries.Count > 0)
                        {
                            items.Add(table.Unk00.TagData.DataTable.Hash);
                        }
                    }
                }
            }
            foreach (var resource in entry.TagData.Unk14.TagData.Unk28)
            {
                foreach (var a in resource.Unk00.TagData.Unk38)
                {
                    foreach (var table in a.Unk08)
                    {
                        if (table.Unk00 is null || table.Unk00.TagData.DataTable is null)
                            continue;

                        if (table.Unk00.TagData.DataTable.TagData.DataEntries.Count > 0)
                        {
                            items.Add(table.Unk00.TagData.DataTable.Hash);
                        }
                    }
                }
            }

            return items.ToList();
        }
    }
} // Shadowkeep launch to SK last

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402 // BL + all the way to Latest
{
    public class Activity : Tag<SActivity_WQ>, IActivity
    {
        public FileHash FileHash => Hash;

        public Activity(FileHash hash) : base(hash)
        {
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            foreach (var mapEntry in _tag.Unk50)
            {
                if (Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
                {
                    if (mapEntry.Unk30 is null ||
                        mapEntry.Unk30.TagData.ChildMapReference == null)
                    {
                        continue;
                    }
                    yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), ChildMapReference = mapEntry.Unk30.TagData.ChildMapReference };
                }
                else
                {
                    var stringContainer = FileResourcer.Get().GetSchemaTag<D2Class_8B8E8080>(_tag.Destination).TagData.StringContainer;
                    foreach (var mapReference in mapEntry.MapReferences)
                    {

                        if (mapReference.MapReference is null ||
                            mapReference.MapReference.TagData.ChildMapReference == null)
                        {
                            continue;
                        }
                        yield return new Bubble { Name = stringContainer.GetStringFromHash(mapEntry.BubbleName), ChildMapReference = mapReference.MapReference.TagData.ChildMapReference };
                        //yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), ChildMapReference = mapReference.MapReference.TagData.ChildMapReference };
                    }

                }
            }
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null)
        {
            foreach (var entry in _tag.Unk50)
            {
                foreach (var resource in entry.Unk18)
                {
                    yield return new ActivityEntities
                    {
                        BubbleName = GlobalStrings.Get().GetString(resource.BubbleName),
                        Hash = resource.UnkEntityReference.Hash,
                        ActivityPhaseName2 = resource.ActivityPhaseName2,
                        DataTables = CollapseResourceParent(resource.UnkEntityReference.Hash),
                        WorldIDs = GetWorldIDs(resource.UnkEntityReference.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();
            var entry = FileResourcer.Get().GetSchemaTag<D2Class_898E8080>(hash);
            var Unk18 = FileResourcer.Get().GetSchemaTag<D2Class_BE8E8080>(entry.TagData.Unk18.Hash);

            foreach (var resource in Unk18.TagData.EntityResources)
            {
                if (resource.EntityResourceParent != null)
                {
                    var resourceValue = resource.EntityResourceParent.TagData.EntityResource.TagData.Unk18.GetValue(resource.EntityResourceParent.TagData.EntityResource.GetReader());
                    switch (resourceValue)
                    {
                        case D2Class_D8928080:
                            var tag = (D2Class_D8928080)resourceValue;
                            if (tag.Unk84 is not null)
                            {
                                if (tag.Unk84.TagData.DataEntries.Count > 0)
                                {
                                    items.Add(tag.Unk84.Hash);
                                }
                            }
                            break;

                        case D2Class_EF8C8080:
                            var tag2 = (D2Class_EF8C8080)resourceValue;
                            if (tag2.Unk58 is not null)
                            {
                                if (tag2.Unk58.TagData.DataEntries.Count > 0)
                                {
                                    items.Add(tag2.Unk58.Hash);
                                }
                            }
                            break;
                    }
                }
            }

            return items.ToList();
        }

        private Dictionary<ulong, ActivityEntity> GetWorldIDs(FileHash hash)
        {
            Dictionary<ulong, ActivityEntity> items = new();
            Dictionary<uint, string> strings = new();
            var entry = FileResourcer.Get().GetSchemaTag<D2Class_898E8080>(hash);
            var Unk18 = FileResourcer.Get().GetSchemaTag<D2Class_BE8E8080>(entry.TagData.Unk18.Hash);

            foreach (var resource in Unk18.TagData.EntityResources)
            {
                if (resource.EntityResourceParent != null)
                {
                    var resourceValue = resource.EntityResourceParent.TagData.EntityResource.TagData.Unk18.GetValue(resource.EntityResourceParent.TagData.EntityResource.GetReader());
                    switch (resourceValue)
                    {
                        //This is kinda dumb 
                        case D2Class_95468080:
                        case D2Class_26988080:
                        case D2Class_6F418080:
                        case D2Class_EF988080:
                        case D2Class_F88C8080:
                        case D2Class_FA988080:
                            if (resource.EntityResourceParent.TagData.EntityResource.TagData.UnkHash80 != null)
                            {
                                var unk80 = FileResourcer.Get().GetSchemaTag<D2Class_6B908080>(resource.EntityResourceParent.TagData.EntityResource.TagData.UnkHash80.Hash);
                                foreach (var a in unk80.TagData.Unk08)
                                {
                                    if (a.Unk00.Value.Name.Value is not null)
                                    {
                                        strings.TryAdd(Helpers.Fnv(a.Unk00.Value.Name.Value), a.Unk00.Value.Name.Value);
                                    }
                                }
                                foreach (var worldid in resourceValue.Unk58)
                                {
                                    if (strings.ContainsKey(worldid.FNVHash.Hash32) && strings.Any(kv => kv.Key == worldid.FNVHash.Hash32))
                                    {
                                        ActivityEntity ent = new();
                                        if (strings.ContainsKey(resourceValue.FNVHash.Hash32))
                                        {
                                            ent.Name = strings[worldid.FNVHash.Hash32];
                                            ent.SubName = strings[resourceValue.FNVHash.Hash32];
                                            items.TryAdd(worldid.WorldID, ent);
                                        }
                                        else
                                        {
                                            ent.Name = strings[worldid.FNVHash.Hash32];
                                            ent.SubName = "";
                                            items.TryAdd(worldid.WorldID, ent);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return items;
        }
    }
}
