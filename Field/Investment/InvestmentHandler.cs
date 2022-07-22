using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Field.Entities;
using Field.Strings;
using Newtonsoft.Json;

namespace Field.General;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
public class InvestmentHandler
{
    private static Tag<D2Class_8C798080> _inventoryItemIndexDictTag = null;
    private static Tag<D2Class_97798080> _inventoryItemMap = null;
    private static Tag<D2Class_F2708080> _artArrangementMap = null;
    private static Tag<D2Class_CE558080> _entityAssignmentTag = null;
    private static Tag<D2Class_434F8080> _entityAssignmentsMap = null;
    private static Tag<D2Class_99548080> _inventoryItemStringThing = null;
    private static Tag<D2Class_8C978080> _sandboxPatternAssignmentsTag = null;
    private static Tag<D2Class_AA528080> _sandboxPatternGlobalTagIdTag = null;
    public static ConcurrentDictionary<int, Tag<D2Class_9F548080>> InventoryItemStringThings = null;
    public static Dictionary<DestinyHash, int> InventoryItemIndexmap = null;
    private static Dictionary<DestinyHash, TagHash> _sortedArrangementHashmap = null;
    private static Dictionary<DestinyHash, TagHash> _sortedPatternGlobalTagIdAssignments = null;
    private static Tag<D2Class_095A8080> _stringContainerIndexTag = null;
    private static Dictionary<int, TagHash> _stringContainerIndexmap = null;
    public static ConcurrentDictionary<DestinyHash, InventoryItem> InventoryItems = null;

    public static void Initialise()
    {
        GetAllInvestmentTags();
    }

    public static string GetItemName(InventoryItem item)
    {
        return GetItemName(item.Header.InventoryItemHash);
    }

    public static string GetItemName(DestinyHash hash)
    {
        return InventoryItemStringThings[GetItemIndex(hash)].Header.ItemName;
    }

    public static int GetItemIndex(DestinyHash hash)
    {
        return InventoryItemIndexmap[hash];
    }

    private static async void GetAllInvestmentTags()
    {
        // Iterate over all investment pkgs until we find all the tags we need
        var unmanagedDictionary = DllGetAllInvestmentTags();
        uint[] keys = new uint[unmanagedDictionary.Keys.dataSize];
        PackageHandler.Copy(unmanagedDictionary.Keys.dataPtr, keys, 0, unmanagedDictionary.Keys.dataSize);
        uint[] vals = new uint[unmanagedDictionary.Values.dataSize];
        PackageHandler.Copy(unmanagedDictionary.Values.dataPtr, vals, 0, unmanagedDictionary.Values.dataSize);
        // maybe i can parallel this? todo maybe parallel
        Parallel.ForEach(vals, (val, state, i) =>
        {
            switch (vals[i])
            {
                case 0x80807997:
                    _inventoryItemMap = new Tag<D2Class_97798080>(new TagHash(keys[i]));
                    break;
                case 0x808070f2:
                    _artArrangementMap = new Tag<D2Class_F2708080>(new TagHash(keys[i]));
                    break;
                case 0x808055ce:
                    _entityAssignmentTag = new Tag<D2Class_CE558080>(new TagHash(keys[i]));
                    break;
                case 0x80805499:
                    _inventoryItemStringThing = new Tag<D2Class_99548080>(new TagHash(keys[i]));
                    break;
                case 0x8080798c:
                    _inventoryItemIndexDictTag = new Tag<D2Class_8C798080>(new TagHash(keys[i]));
                    break;
                case 0x80805a09:
                    _stringContainerIndexTag = new Tag<D2Class_095A8080>(new TagHash(keys[i]));
                    break;
                case 0x80804ea4: // points to parent of the sandbox pattern ref list thing + entity assignment map
                    var parent = new Tag<D2Class_A44E8080>(new TagHash(keys[i]));
                    _sandboxPatternAssignmentsTag = parent.Header.SandboxPatternAssignmentsTag; // also art dye refs

                    _entityAssignmentsMap = parent.Header.EntityAssignmentsMap;

                    break;
                case 0x808052aa: // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
                    _sandboxPatternGlobalTagIdTag = new Tag<D2Class_AA528080>(new TagHash(keys[i]));
                    break;
            }
        });
        GetContainerIndexDict(); // must be before GetInventoryItemStringThings


        Task.WaitAll(new []
        {
            Task.Run(GetInventoryItemDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetSandboxPatternAssignmentsDict),
        });
    }

    private static void GetInventoryItemStringThings()
    {
        int size = (int)_inventoryItemStringThing.Header.StringThings.Count;
        InventoryItemStringThings = new ConcurrentDictionary<int, Tag<D2Class_9F548080>>();
        var temp = new Dictionary<int, uint>(size);

        var br = _inventoryItemStringThing.Header.StringThings.ParentTag.GetHandle();
        br.BaseStream.Seek(_inventoryItemStringThing.Header.StringThings.Offset, SeekOrigin.Begin);
        for (int i = 0; i < size; i++)
        {
            br.BaseStream.Seek(0x10, SeekOrigin.Current);
            temp.Add(i, br.ReadUInt32());
            br.BaseStream.Seek(0xC, SeekOrigin.Current);
        }
        PackageHandler.CacheHashDataList(temp.Values.ToArray());
        Parallel.ForEach(temp, kvp =>
        {
            InventoryItemStringThings.TryAdd(kvp.Key, PackageHandler.GetTag<D2Class_9F548080>(kvp.Value));
        });
        br.Close();
    }
    
    private static void GetContainerIndexDict()
    {
        int size = (int)_stringContainerIndexTag.Header.StringContainerMap.Count;
        _stringContainerIndexmap = new Dictionary<int, TagHash>(size);
        var br = _stringContainerIndexTag.Header.StringContainerMap.ParentTag.GetHandle();
        
        br.BaseStream.Seek(_stringContainerIndexTag.Header.StringContainerMap.Offset, SeekOrigin.Begin);
        for (int i = 0; i < size; i++)
        {
            br.BaseStream.Seek(0x10, SeekOrigin.Current);
            _stringContainerIndexmap.Add(i, new TagHash(br.ReadUInt64()));
        }
        
        // This cache helps the StringThing stuff to be faster
        PackageHandler.CacheHashDataList(_stringContainerIndexmap.Values.Where(x => x.IsValid()).Select(x => x.Hash).ToArray());

        br.Close();
    }

    public static TagHash GetStringContainerFromIndex(uint index)
    {
        return _stringContainerIndexmap[(int) index];
    }

    private static void GetEntityAssignmentDict()
    {
        int size = (int)_entityAssignmentsMap.Header.EntityArrangementMap.Count;
        _sortedArrangementHashmap = new Dictionary<DestinyHash, TagHash>(size);
        var br = _entityAssignmentsMap.Header.EntityArrangementMap.ParentTag.GetHandle();
        
        br.BaseStream.Seek(_entityAssignmentsMap.Header.EntityArrangementMap.Offset, SeekOrigin.Begin);
        for (int i = 0; i < size; i++)
        {
            _sortedArrangementHashmap.Add(new DestinyHash(br.ReadUInt32()), new TagHash(br.ReadUInt32()));
        }
        br.Close();
    }
    
    private static void GetSandboxPatternAssignmentsDict()
    {
        int size = (int)_sandboxPatternAssignmentsTag.Header.AssingmentBSL.Count;
        _sortedPatternGlobalTagIdAssignments = new Dictionary<DestinyHash, TagHash>(size);
        var br = _sandboxPatternAssignmentsTag.Header.AssingmentBSL.ParentTag.GetHandle();
        
        br.BaseStream.Seek(_sandboxPatternAssignmentsTag.Header.AssingmentBSL.Offset, SeekOrigin.Begin);
        for (int i = 0; i < size; i++)
        {
            // skips art dye refs, theyre not tag64
            var h = new DestinyHash(br.ReadUInt32());
            br.BaseStream.Seek(0xC, SeekOrigin.Current);
            _sortedPatternGlobalTagIdAssignments.Add(h, new TagHash(br.ReadUInt64()));
        }
        br.Close();
    }
    
    public static Entity? GetPatternEntityFromHash(DestinyHash hash)
    {
        var item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;
        
        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sortedPatternGlobalTagIdAssignments[patternGlobalId];
        if (PackageHandler.GetEntryReference(patternData) == 0x80809ad8)
        {
            return PackageHandler.GetTag(typeof(Entity), patternData);
        }
        return null;
    }
    
    public static DestinyHash GetPatternGlobalTagId(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.Header.SandboxPatternGlobalTagId[item.GetWeaponPatternIndex()].PatternGlobalTagIdHash;
    }
    
    public static DestinyHash GetWeaponContentGroupHash(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.Header.SandboxPatternGlobalTagId[item.GetWeaponPatternIndex()].WeaponContentGroupHash;
    }

    public static InventoryItem GetInventoryItem(DestinyHash hash)
    {
        return GetInventoryItem(InventoryItemIndexmap[hash]);
    }
    
    public static InventoryItem GetInventoryItem(int index)
    {
        InventoryItem item = new InventoryItem(_inventoryItemMap.Header.InventoryItemDefinitionEntries.ElementAt(index).InventoryItem);
        return item;
    }
    
    public static void GetInventoryItemDict()
    {
        InventoryItemIndexmap = new Dictionary<DestinyHash, int>();
        InventoryItems = new ConcurrentDictionary<DestinyHash, InventoryItem>();
        // Read all hashes and tags synchronously
        Dictionary<DestinyHash, TagHash> temp = new Dictionary<DestinyHash, TagHash>();
        var br = _inventoryItemMap.Header.InventoryItemDefinitionEntries.ParentTag.GetHandle();
        br.BaseStream.Seek(_inventoryItemMap.Header.InventoryItemDefinitionEntries.Offset, SeekOrigin.Begin);
        int size = (int)_inventoryItemMap.Header.InventoryItemDefinitionEntries.Count;
        for (int i = 0; i < size; i++)
        {
            var dh = new DestinyHash(br.ReadUInt32());
            br.BaseStream.Seek(0xC, SeekOrigin.Current);
            temp.Add(dh, new TagHash(br.ReadUInt32()));
            InventoryItemIndexmap.Add(dh, i);
            br.BaseStream.Seek(0xC, SeekOrigin.Current);
        }
        br.Close();
        // Now we can use parallel code as not reading from a single file
        
        // try the many many instead
        PackageHandler.CacheHashDataList(temp.Values.Select(x => x.Hash).ToArray());

        Parallel.ForEach(temp, kvp =>
        {
            InventoryItems.TryAdd(kvp.Key, PackageHandler.GetTag(typeof(InventoryItem), kvp.Value));
        });
    }


    public static DestinyHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.Header.ArtArrangementHashes.ElementAt(item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    public static List<Entity> GetEntitiesFromHash(DestinyHash hash)
    {
        var item = GetInventoryItem(hash);
        var index = item.GetArtArrangementIndex();
        List<Entity> entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    private static List<Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity> entities = new List<Entity>();
        var entry = _entityAssignmentTag.Header.ArtArrangementEntityAssignments.ElementAt(index);
        if (entry.MultipleEntityAssignments.Count == 0)  // single
        {
            if (entry.FeminineSingleEntityAssignment.IsValid())
            {
                entities.Add(GetEntityFromAssignmentHash(entry.FeminineSingleEntityAssignment));
            }
            if (entry.MasculineSingleEntityAssignment.IsValid())
            {
                entities.Add(GetEntityFromAssignmentHash(entry.MasculineSingleEntityAssignment));
            }
        }
        else
        {
            foreach (var entryMultipleEntityAssignment in entry.MultipleEntityAssignments)
            {
                foreach (var assignment in entryMultipleEntityAssignment.EntityAssignmentResource.EntityAssignments)
                {
                    if (assignment.EntityAssignmentHash.IsValid())
                        entities.Add(GetEntityFromAssignmentHash(assignment.EntityAssignmentHash));
                }
            }
        }

        return entities;
    }

    private static Entity GetEntityFromAssignmentHash(DestinyHash assignmentHash)
    {
        // We can binary search here as the list is sorted.
        // var x = new D2Class_454F8080 {AssignmentHash = assignmentHash};
        // var index = _entityAssignmentsMap.Header.EntityArrangementMap.BinarySearch(x, new D2Class_454F8080());
        Tag<D2Class_A36F8080> tag = PackageHandler.GetTag<D2Class_A36F8080>(_sortedArrangementHashmap[assignmentHash]);
        return PackageHandler.GetTag(typeof(Entity), tag.Header.EntityData);
        // return new Entity(_entityAssignmentsMap.Header.EntityArrangementMap[index].EntityParent.Header.Entity);
        // return null;
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllInvestmentTags", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedDictionary DllGetAllInvestmentTags();
}


public class InventoryItem : Tag
{
    public D2Class_9D798080 Header;
    
    public InventoryItem(TagHash hash) : base(hash)
    {
    }
    
    protected override void ParseStructs()
    {
        Header = ReadHeader<D2Class_9D798080>();
    }

    public int GetArtArrangementIndex()
    {
        if (Header.Unk90 is D2Class_77738080 entry)
        {
            if (entry.Arrangements.Count > 0)
                return entry.Arrangements[0].ArtArrangementHash;
        }
        return -1;
    }
    
    public int GetWeaponPatternIndex()
    {
        if (Header.Unk90 is D2Class_77738080 entry)
        {
            if (entry.WeaponPatternIndex > 0)
                return entry.WeaponPatternIndex;
        }
        return -1;
    }
}