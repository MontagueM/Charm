using Tiger.Schema.Audio;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Activity
{
    public struct Bubble
    {
        public string Name;
        public Tag<SBubbleParent> MapReference;
    }

    public interface IActivity : ISchema
    {
        public FileHash FileHash { get; }
        public IEnumerable<Bubble> EnumerateBubbles();
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
    public class Activity : Tag<SActivity_BL>, IActivity
    {
        public FileHash FileHash => Hash;

        public Activity(FileHash hash) : base(hash)
        {
        }

        public IEnumerable<Bubble> EnumerateBubbles()
        {
            foreach (var mapEntry in _tag.Unk80)
            {
                if (mapEntry.Unk30 is null ||
                    mapEntry.Unk30.TagData.ChildMapReference == null)
                {
                    continue;
                }
                yield return new Bubble { Name = GlobalStrings.Get().GetString(mapEntry.BubbleName), MapReference = mapEntry.Unk30 };
            }
        }
    }
}
