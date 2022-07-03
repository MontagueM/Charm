using System.Runtime.InteropServices;
using Field.Entities;

namespace Field.General;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
public class InvestmentHandler
{
    public static Dictionary<DestinyHash, Tag<D2Class_9D798080>> InventoryItemDefinitions = null;
    private static Tag<D2Class_97798080> _inventoryItemMap = null;
    private static Tag<D2Class_F2708080> _artArrangementMap = null;
    private static Tag<D2Class_CE558080> _entityAssignmentTag = null;
    private static Tag<D2Class_434F8080> _entityAssignmentsMap = null;


    public static void Initialise()
    {
        GetAllInvestmentTags();

        var a = 0;
    }

    private static void GetAllInvestmentTags()
    {
        // Iterate over all investment pkgs until we find all the tags we need
        var unmanagedDictionary = DllGetAllInvestmentTags();
        uint[] keys = new uint[unmanagedDictionary.Keys.dataSize];
        PackageHandler.Copy(unmanagedDictionary.Keys.dataPtr, keys, 0, unmanagedDictionary.Keys.dataSize);
        uint[] vals = new uint[unmanagedDictionary.Values.dataSize];
        PackageHandler.Copy(unmanagedDictionary.Values.dataPtr, vals, 0, unmanagedDictionary.Values.dataSize);
        for (int i = 0; i < vals.Length; i++)
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
                case 0x80804f43:
                    _entityAssignmentsMap = new Tag<D2Class_434F8080>(new TagHash(keys[i]));
                    break;
            }
        }
    }
    
    public static InventoryItem GetInventoryItem(DestinyHash hash)
    {
        InventoryItem item = new InventoryItem(_inventoryItemMap.Header.InventoryItemDefinitionEntries.First(x => x.InventoryItemHash.Equals(hash)).InventoryItem);
        return item;
    }

    public static DestinyHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.Header.ArtArrangementHashes[item.GetArtArrangementIndex()].ArtArrangementHash;
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
        var entry = _entityAssignmentTag.Header.ArtArrangementEntityAssignments[index];
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
                    entities.Add(GetEntityFromAssignmentHash(assignment.EntityAssignmentHash));
                }
            }
        }

        return entities;
    }

    private static Entity GetEntityFromAssignmentHash(DestinyHash assignmentHash)
    {
        // We can binary search here as the list is sorted.
        var x = new D2Class_454F8080 {AssignmentHash = assignmentHash};
        var index = _entityAssignmentsMap.Header.EntityArrangementMap.BinarySearch(x, new D2Class_454F8080());
        return new Entity(_entityAssignmentsMap.Header.EntityArrangementMap[index].EntityParent.Header.Entity);
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
        if (Header.Unk90 is D2Class_77738080)
        {
            return ((D2Class_77738080) Header.Unk90).Arrangements[0].ArtArrangementHash;
        }
        return -1;
    }
}