using System.Collections.Concurrent;
using System.Data;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Activity
{
    public struct Bubble
    {
        public string Name;
        public Tag<SBubbleParent> MapReference;
    }

    public struct ActivityEntities
    {
        public string BubbleName;
        public string ActivityPhaseName2;
        public FileHash Hash;
        public List<FileHash> DataTables;
    }

    public interface IActivity : ISchema
    {
        public FileHash FileHash { get; }
        public IEnumerable<Bubble> EnumerateBubbles();
        public IEnumerable<ActivityEntities> EnumerateActivityEntities();
    }
}

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
                yield return new Bubble { Name = GetBubbleNameFromBubbleIndex(bubbleIndex), MapReference = bubble.MapReference };
            }
        }

        private string GetBubbleNameFromBubbleIndex(int index)
        {
            return GlobalStrings.Get().GetString(_tag.LocationNames.TagData.BubbleNames.First(e => e.BubbleIndex == index).BubbleName);
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities()
        {
            throw new NotSupportedException();
        }
    }
}

namespace Tiger.Schema.Activity.DESTINY2_WITCHQUEEN_6307
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
                foreach (var mapReference in mapEntry.MapReferences)
                {
                    if (mapReference.MapReference is null ||
                        mapReference.MapReference.TagData.ChildMapReference == null)
                    {
                        continue;
                    }
                    yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), MapReference = mapReference.MapReference };
                }
            }
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities()
        {
            throw new NotSupportedException();
        }

        // protected override void ParseStructs()
        // {
        //     // Getting the string container
        //     LocalizedStrings sc;
        //     using (var handle = GetHandle())
        //     {
        //         handle.BaseStream.Seek(0x28, SeekOrigin.Begin);
        //         var tag = PackageHandler.GetTag<D2Class_8B8E8080>(new FileHash(handle.ReadUInt64()));
        //         sc = tag._tag.LocalizedStrings;
        //     }
        //     Header = ReadHeader<D2Class_8E8E8080>(sc);
        // }
    }
}

namespace Tiger.Schema.Activity.DESTINY2_BEYONDLIGHT_3402
{
    public class Activity : Tag<DESTINY2_WITCHQUEEN_6307.SActivity_WQ>, IActivity
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
                    yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), MapReference = mapEntry.Unk30 };
                }
                else
                {
                    foreach (var mapReference in mapEntry.MapReferences)
                    {

                        if (mapReference.MapReference is null ||
                            mapReference.MapReference.TagData.ChildMapReference == null)
                        {
                            continue;
                        }
                        yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), MapReference = mapReference.MapReference };
                    }

                }
            }
        }

        public IEnumerable<ActivityEntities> EnumerateActivityEntities()
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
                        DataTables = CollapseResourceParent(resource.UnkEntityReference.Hash)
                    };
                }
            }
        }

        private List<FileHash> CollapseResourceParent(FileHash hash)
        {
            ConcurrentBag<FileHash> items = new();
            var entry = FileResourcer.Get().GetSchemaTag<DESTINY2_WITCHQUEEN_6307.D2Class_898E8080>(hash);
            var Unk18 = FileResourcer.Get().GetSchemaTag<DESTINY2_WITCHQUEEN_6307.D2Class_BE8E8080>(entry.TagData.Unk18.Hash);

            foreach(var resource in Unk18.TagData.EntityResources)
            { 
                if(resource.EntityResourceParent != null)
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
    }
}
