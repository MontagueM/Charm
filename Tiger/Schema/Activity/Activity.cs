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
        public string DestinationName { get; }
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

        private string _destinationName;
        public string DestinationName
        {
            get
            {
                if (_destinationName != null)
                    return _destinationName;

                _destinationName = Helpers.SanitizeString(GetDestinationName()); ;
                return _destinationName;
            }
        }

        public Activity(FileHash hash) : base(hash)
        {
        }

        public string GetDestinationName()
        {
            string activityName = PackageResourcer.Get().GetActivityName(Hash);
            string first = activityName.Split(":")[1];

            Dictionary<FileHash, TagClassHash> activities = PackageResourcer.Get().GetD1Activities();
            foreach (KeyValuePair<FileHash, TagClassHash> activity in activities)
            {
                if (activity.Value == "16068080")
                {
                    Tag<SUnkActivity_ROI> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_ROI>(activity.Key);
                    if (tag.TagData.ActivityDevName == first)
                        if (GlobalStrings.Get().GetString(tag.TagData.DestinationName) == tag.TagData.DestinationName)
                            return EnumerateBubbles().Count() == 1 ? EnumerateBubbles().FirstOrDefault().Name : first;
                        else
                            return GlobalStrings.Get().GetString(tag.TagData.DestinationName);
                }
            }

            return $"{Hash}";
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            for (int bubbleIndex = 0; bubbleIndex < _tag.Bubbles.Count; bubbleIndex++)
            {
                S0A418080 bubble = _tag.Bubbles[bubbleIndex];
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
            foreach (S0C068080 entry in tag.TagData.Unk48)
            {
                foreach (SA8068080 entry2 in entry.Unk08)
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

            Tag<SF0088080> entry = FileResourcer.Get().GetSchemaTag<SF0088080>(hash);
            Tag<SF0088080_Child> entry2 = FileResourcer.Get().GetSchemaTag<SF0088080_Child>(entry.TagData.Unk1C);
            DynamicArray<SD3408080> entries = entry2.TagData.Unk08;
            entries.AddRange(entry2.TagData.Unk18);
            entries.AddRange(entry2.TagData.Unk28);

            foreach (SD3408080 resource in entries)
            {
                Tag<S6E078080> Unk00 = FileResourcer.Get().GetSchemaTag<S6E078080>(resource.Unk00);
                foreach (SE9058080 a in Unk00.TagData.Unk30)
                {
                    if (a.Unk10 is not null && a.Unk10.TagData.DataEntries.Count > 0)
                        if (!items.Contains(a.Unk10.Hash))
                            items.Add(a.Unk10.Hash);

                    foreach (S22428080 b in a.Unk18)
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
        private string _destinationName;

        public string DestinationName
        {
            get
            {
                if (_destinationName != null)
                    return _destinationName;

                _destinationName = Helpers.SanitizeString(GetDestinationName());
                return _destinationName;
            }
        }

        public Activity(FileHash hash) : base(hash)
        {
        }

        private string GetDestinationName()
        {
            ConcurrentCollections.ConcurrentHashSet<FileHash> valsChild = PackageResourcer.Get().GetAllHashes<SUnkActivity_SK>();
            string mapRoot = PackageResourcer.Get().GetActivityName(Hash);
            string first = mapRoot.Split(":")[1];

            foreach (FileHash val in valsChild)
            {
                Tag<SUnkActivity_SK> tag = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(val);
                if (tag.TagData.ActivityDevName == first)
                    if (GlobalStrings.Get().GetString(tag.TagData.DestinationName) == tag.TagData.DestinationName)
                        return EnumerateBubbles().Count() == 1 ? EnumerateBubbles().FirstOrDefault().Name : first;
                    else
                        return GlobalStrings.Get().GetString(tag.TagData.DestinationName);
            }

            return $"{Hash}";
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            for (int bubbleIndex = 0; bubbleIndex < _tag.Bubbles.Count; bubbleIndex++)
            {
                S537D8080 bubble = _tag.Bubbles[bubbleIndex];
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
            foreach (S4D928080 entry in tag.TagData.Unk50)
            {
                foreach (S4F928080 entry2 in entry.Unk08)
                {
                    if (entry2.Unk44 is null)
                        continue;

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

            Tag<S5B928080> tag = FileResourcer.Get().GetSchemaTag<DESTINY2_SHADOWKEEP_2601.S5B928080>(hash);
            if (tag.TagData.Unk14 is null)
                return items.ToList();

            List<S60928080> tables = tag.TagData.Unk14.TagData.Unk08;
            tables.AddRange(tag.TagData.Unk14.TagData.Unk18);
            tables.AddRange(tag.TagData.Unk14.TagData.Unk28);

            foreach (S60928080 resource in tables)
            {
                if (resource.Unk00 is null)
                    continue;
                foreach (S64948080 a in resource.Unk00.TagData.Unk38)
                {
                    foreach (S66948080 table in a.Unk08)
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

        public List<FileHash> GetActivityDialogueTables(FileHash UnkActivity)
        {
            List<FileHash> entries = new();
            foreach (EntityResource resource in GetActivityResources(UnkActivity))
            {
                if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S4C4F8080 d)
                {
                    if (d.DialogueTable is null)
                        continue;
                    entries.Add(d.DialogueTable.Hash);
                }
            }
            return entries;
        }

        public List<FileHash> GetActivityDirectiveTables(FileHash UnkActivity)
        {
            List<FileHash> entries = new();
            foreach (EntityResource resource in GetActivityResources(UnkActivity))
            {
                if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S544F8080 d)
                {
                    if (d.DirectiveTable is null)
                        continue;
                    entries.Add(d.DirectiveTable.Hash);
                }
            }
            return entries;
        }

        public List<FileHash> GetActivityMusicList(FileHash UnkActivity)
        {
            List<FileHash> entries = new();
            foreach (EntityResource resource in GetActivityResources(UnkActivity))
            {
                if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S8F4E8080 d)
                {
                    entries.Add(resource.Hash);
                }
            }
            return entries;
        }

        private List<EntityResource> GetActivityResources(FileHash UnkActivity)
        {
            Tag<SUnkActivity_SK> activity = FileResourcer.Get().GetSchemaTag<SUnkActivity_SK>(UnkActivity);
            List<EntityResource> entries = new();
            foreach (DynamicArray<S4F928080>? entry in activity.TagData.Unk50.Select(x => x.Unk08))
            {
                foreach (S4F928080 entry2 in entry)
                {
                    if (entry2.Unk44 is null)
                        continue;
                    Tag<S5B928080> tag = FileResourcer.Get().GetSchemaTag<S5B928080>(entry2.Unk44.Hash);
                    if (tag.TagData.Unk14 is null)
                        continue;

                    List<S60928080> tables = tag.TagData.Unk14.TagData.Unk08;
                    tables.AddRange(tag.TagData.Unk14.TagData.Unk18);
                    tables.AddRange(tag.TagData.Unk14.TagData.Unk28);

                    foreach (S60928080 entry3 in tables)
                    {
                        if (entry3.Unk00 is null)
                            continue;
                        foreach (S64948080 a in entry3.Unk00.TagData.Unk38)
                        {
                            foreach (S66948080 b in a.Unk08)
                            {
                                if (b.Unk00 is null)
                                    continue;
                                foreach (S139B8080 c in b.Unk00.TagData.Unk10)
                                {
                                    EntityResource? resource = c.Unk00.TagData.EntityResource;
                                    if (resource is null)
                                        continue;

                                    entries.Add(resource);
                                }
                            }
                        }
                    }
                }
            }
            return entries;
        }
    }
} // Shadowkeep launch to SK last

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402 // BL + all the way to Latest
{
    public class Activity : Tag<SActivity_WQ>, IActivity
    {
        public FileHash FileHash => Hash;

        private string _destinationName;
        public string DestinationName
        {
            get
            {
                if (_destinationName != null)
                    return _destinationName;

                _destinationName = Helpers.SanitizeString(GetDestinationName());
                return _destinationName;
            }
        }

        public Activity(FileHash hash) : base(hash)
        {
        }

        private string GetDestinationName()
        {
            return GlobalStrings.Get().GetString(new StringHash(_tag.LocationName.Hash32));
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            Strings.LocalizedStrings? stringContainer = FileResourcer.Get().GetSchemaTag<S8B8E8080>(_tag.Destination).TagData.StringContainer;
            foreach (S24898080 mapEntry in _tag.Unk50)
            {
                if (Strategy.CurrentStrategy == TigerStrategy.DESTINY2_BEYONDLIGHT_3402)
                {
                    if (mapEntry.Unk30 is null || mapEntry.Unk30.TagData.ChildMapReference == null)
                        continue;

                    string name = stringContainer is null ? mapEntry.BubbleName : stringContainer.GetStringFromHash(mapEntry.BubbleName);
                    if ((name.Contains("NotFound") || mapEntry.BubbleName.ToString() == name) && mapEntry.UnkBubbleName.Value is not null) // this is dumb
                        name = mapEntry.UnkBubbleName.Value;

                    yield return new Bubble
                    {
                        Name = name,
                        ChildMapReference = mapEntry.Unk30.TagData.ChildMapReference
                    };
                }
                else
                {
                    foreach (S1D898080 mapReference in mapEntry.MapReferences)
                    {

                        if (mapReference.MapReference is null || mapReference.MapReference.TagData.ChildMapReference == null)
                            continue;

                        string name = stringContainer is null ? mapEntry.BubbleName : stringContainer.GetStringFromHash(mapEntry.BubbleName);
                        if ((name.Contains("NotFound") || mapEntry.BubbleName.ToString() == name)) // this is dumb
                            name = GlobalStrings.Get().GetString(mapEntry.BubbleName);

                        yield return new Bubble
                        {
                            Name = name,
                            ChildMapReference = mapReference.MapReference.TagData.ChildMapReference
                        };
                    }

                }
            }
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities(FileHash UnkActivity = null)
        {
            Strings.LocalizedStrings? stringContainer = FileResourcer.Get().GetSchemaTag<S8B8E8080>(_tag.Destination).TagData.StringContainer;
            foreach (S24898080 entry in _tag.Unk50)
            {
                foreach (S48898080 resource in entry.Unk18)
                {
                    string name = stringContainer is null ? resource.BubbleName : stringContainer.GetStringFromHash(resource.BubbleName);
                    yield return new ActivityEntities
                    {
                        BubbleName = name,
                        Hash = resource.UnkEntityReference.Hash,
                        ActivityPhaseName2 = GlobalStrings.Get().GetString(new StringHash(resource.ActivityPhaseName2.Hash32)),
                        DataTables = CollapseResourceParent(resource.UnkEntityReference.Hash),
                        WorldIDs = GetWorldIDs(resource.UnkEntityReference.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();
            Tag<S898E8080> entry = FileResourcer.Get().GetSchemaTag<S898E8080>(hash);
            Tag<SBE8E8080> Unk18 = FileResourcer.Get().GetSchemaTag<SBE8E8080>(entry.TagData.Unk18.Hash);

            foreach (S42898080 resource in Unk18.TagData.EntityResources)
            {
                if (resource.EntityResourceParent != null)
                {
                    dynamic? resourceValue = resource.EntityResourceParent.TagData.EntityResource.TagData.Unk18.GetValue(resource.EntityResourceParent.TagData.EntityResource.GetReader());
                    switch (resourceValue)
                    {
                        case SD8928080:
                            var tag = (SD8928080)resourceValue;
                            if (tag.Unk84 is not null && tag.Unk84.TagData.DataEntries.Count > 0)
                            {
                                items.Add(tag.Unk84.Hash);
                            }
                            break;

                        case SEF8C8080:
                            var tag2 = (SEF8C8080)resourceValue;
                            if (tag2.Unk58 is not null && tag2.Unk58.TagData.DataEntries.Count > 0)
                            {
                                items.Add(tag2.Unk58.Hash);
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
            Tag<S898E8080> entry = FileResourcer.Get().GetSchemaTag<S898E8080>(hash);
            Tag<SBE8E8080> Unk18 = FileResourcer.Get().GetSchemaTag<SBE8E8080>(entry.TagData.Unk18.Hash);

            foreach (S42898080 resource in Unk18.TagData.EntityResources)
            {
                if (resource.EntityResourceParent != null)
                {
                    dynamic? resourceValue = resource.EntityResourceParent.TagData.EntityResource.TagData.Unk18.GetValue(resource.EntityResourceParent.TagData.EntityResource.GetReader());
                    switch (resourceValue)
                    {
                        //This is kinda dumb 
                        case S95468080:
                        case S26988080:
                        case S6F418080:
                        case SEF988080:
                        case SF88C8080:
                        case SFA988080:
                            if (resource.EntityResourceParent.TagData.EntityResource.TagData.UnkHash80 != null)
                            {
                                Tag<S6B908080> unk80 = FileResourcer.Get().GetSchemaTag<S6B908080>(resource.EntityResourceParent.TagData.EntityResource.TagData.UnkHash80.Hash);
                                foreach (S029D8080 a in unk80.TagData.Unk08)
                                {
                                    if (a.Unk00.Value?.Name.Value is not null)
                                    {
                                        strings.TryAdd(Helpers.Fnv1a32(a.Unk00.Value.Value.Name.Value), a.Unk00.Value.Value.Name.Value);
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
