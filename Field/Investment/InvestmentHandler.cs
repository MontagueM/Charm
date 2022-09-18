using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using DirectXTexNet;
using Field.Entities;
using Field.Investment;
using Field.Models;
using Field.Strings;
using Field.Textures;
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
    // private static Dictionary<DestinyHash, TagHash> _sortedPatternGlobalTagIdAssignments = null;
    private static Tag<D2Class_095A8080> _stringContainerIndexTag = null;
    private static Dictionary<int, TagHash> _stringContainerIndexmap = null;
    public static ConcurrentDictionary<DestinyHash, InventoryItem> InventoryItems = null;
    private static Tag<D2Class_015A8080> _inventoryItemIconTag = null;
    // private static Tag<D2Class_8C978080> _dyeManifestTag = null;
    private static Tag<D2Class_C2558080> _artDyeReferenceTag = null;
    private static Tag<D2Class_F2518080> _dyeChannelTag = null;
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
    
    public static Tag<D2Class_B83E8080>? GetItemIconContainer(InventoryItem item)
    {
        return GetItemIconContainer(item.Header.InventoryItemHash);
    }

    public static Tag<D2Class_B83E8080>? GetItemIconContainer(DestinyHash hash)
    {
        int iconIndex = InventoryItemStringThings[GetItemIndex(hash)].Header.IconIndex;
        if (iconIndex == -1)
            return null;
        return _inventoryItemIconTag.Header.InventoryItemIconsMap.ElementAt(iconIndex).IconContainer;
    }

    
    public static int GetItemIndex(DestinyHash hash)
    {
        return InventoryItemIndexmap[hash];
    }

    private static async void GetAllInvestmentTags()
    {
        // Iterate over all investment pkgs until we find all the tags we need
        var unmanagedDictionary = DllGetAllInvestmentTags(PackageHandler.GetExecutionDirectoryPtr());
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
                case 0x80805a01:
                    _inventoryItemIconTag = new Tag<D2Class_015A8080>(new TagHash(keys[i]));
                    break;
                case 0x808055c2:
                    _artDyeReferenceTag = new Tag<D2Class_C2558080>(new TagHash(keys[i]));
                    break;
                case 0x808051f2:
                    _dyeChannelTag = new Tag<D2Class_F2518080>(new TagHash(keys[i]));
                    break;
            }
        });
        GetContainerIndexDict(); // must be before GetInventoryItemStringThings


        Task.WaitAll(new []
        {
            Task.Run(GetInventoryItemDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetEntityAssignmentDict),
            // Task.Run(GetSandboxPatternAssignmentsDict),
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
    
    // private static void GetSandboxPatternAssignmentsDict()
    // {
    //     int size = (int)_sandboxPatternAssignmentsTag.Header.AssignmentBSL.Count;
    //     _sortedPatternGlobalTagIdAssignments = new Dictionary<DestinyHash, TagHash>(size);
    //     var br = _sandboxPatternAssignmentsTag.Header.AssignmentBSL.ParentTag.GetHandle();
    //     
    //     br.BaseStream.Seek(_sandboxPatternAssignmentsTag.Header.AssignmentBSL.Offset, SeekOrigin.Begin);
    //     for (int i = 0; i < size; i++)
    //     {
    //         // skips art dye refs, theyre not tag64
    //         var h = new DestinyHash(br.ReadUInt32());
    //         br.BaseStream.Seek(0xC, SeekOrigin.Current);
    //         _sortedPatternGlobalTagIdAssignments.Add(h, new TagHash(br.ReadUInt64()));
    //     }
    //     br.Close();
    // }

    public static Entity? GetPatternEntityFromHash(DestinyHash hash)
    {
        var item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;
        
        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sandboxPatternAssignmentsTag.Header.AssignmentBSL.BinarySearch(patternGlobalId);
        if (patternData.HasValue)
        {
            if (PackageHandler.GetEntryReference(patternData.Value.EntityRelationHash) == 0x80809ad8)
            {
                return PackageHandler.GetTag(typeof(Entity), patternData.Value.EntityRelationHash);
            }
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

    public static DestinyHash GetChannelHashFromIndex(short index)
    {
        return _dyeChannelTag.Header.ChannelHashes[index].ChannelHash;
    }
    
    public static Dye GetDyeFromIndex(short index)
    {
         var artEntry = _artDyeReferenceTag.Header.ArtDyeReferences.ElementAt(index);

         var dyeEntry = _sandboxPatternAssignmentsTag.Header.AssignmentBSL.BinarySearch(artEntry.DyeManifestHash);
         if (dyeEntry.HasValue)
         {
             if (PackageHandler.GetEntryReference(dyeEntry.Value.EntityRelationHash) == 0x80806fa3)
             {
                 return PackageHandler.GetTag<D2Class_E36C8080>(PackageHandler.GetTag<D2Class_A36F8080>(dyeEntry.Value.EntityRelationHash).Header.EntityData).Header.Dye;
             }
         }
         return null;
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
                    {
                        var assignmentEntity = GetEntityFromAssignmentHash(assignment.EntityAssignmentHash);
                        if (assignmentEntity != null)
                            entities.Add(assignmentEntity);
                    }
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
        if (tag.Header.EntityData is null)
            return null;
        // if entity
        if (PackageHandler.GetEntryReference(tag.Header.EntityData) == 0x80809ad8)
        {
            return PackageHandler.GetTag(typeof(Entity), tag.Header.EntityData);
        }
        return null;
        // return new Entity(_entityAssignmentsMap.Header.EntityArrangementMap[index].EntityParent.Header.Entity);
        // return null;
    }
    
    [DllImport("Symmetry.dll", EntryPoint = "DllGetAllInvestmentTags", CallingConvention = CallingConvention.StdCall)]
    public extern static DestinyFile.UnmanagedDictionary DllGetAllInvestmentTags(IntPtr executionDirectoryPtr);

    #if DEBUG
    public static void DebugAllInvestmentEntities()
    {
        Dictionary<string, Dictionary<dynamic, DestinyHash>> data = new Dictionary<string, Dictionary<dynamic, DestinyHash>>();
        for (int i = (int)_entityAssignmentTag.Header.ArtArrangementEntityAssignments.Count-1; i >= 0; i--)
        {
            List<Entity> entities = GetEntitiesFromArrangementIndex(i);
            foreach (var entity in entities)
            {
                bool bAllValid = true;
                if (entity is null || entity.Model is null)
                    continue;
                foreach (var entry in entity.Model.Header.Meshes[0].Parts)
                {
                    foreach (var field in typeof(D2Class_CB6E8080).GetFields())
                    {
                        if (!data.ContainsKey(field.Name))
                        {
                            data.Add(field.Name, new Dictionary<dynamic, DestinyHash>());
                        }
                        dynamic fieldValue = field.GetValue(entry);
                        if (fieldValue is not null && !data[field.Name].ContainsKey(fieldValue) && data[field.Name].Count < 10)
                        {
                            data[field.Name].Add(fieldValue, _entityAssignmentTag.Header.ArtArrangementEntityAssignments.ElementAt(i).ArtArrangementHash);
                        }

                        bAllValid &= data[field.Name].Count > 1;
                    }
                }
                if (bAllValid)
                {
                    var a = 0;
                }
            }
        }
    }
    
    private static Random rng = new Random();

    public static void DebugAPIRequestAllInfo()
    {
        // get all inventory item hashes
        
        // var itemHash = 138282166;
        var l = InventoryItemIndexmap.Keys.ToList();
        var shuffled = l.OrderBy(a => rng.Next()).ToList();
        foreach (var itemHash in shuffled)
        {
            if (itemHash != 731561450)
                continue;
            ManifestData? itemDef;
            byte[] tgxm;
            try
            {
                itemDef = MakeGetRequestManifestData($"https://www.light.gg/db/items/{itemHash.Hash}/?raw=2");
                // ManifestData? itemDef = MakeGetRequestManifestData($"https://lowlidev.com.au/destiny/api/gearasset/{itemHash.Hash}?destiny2");
                if (itemDef is null || itemDef.gearAsset is null || itemDef.gearAsset.content.Length == 0 || itemDef.gearAsset.content[0].geometry is null || itemDef.gearAsset.content[0].geometry.Length == 0)
                    continue;
                tgxm = MakeGetRequest(
                    $"https://www.bungie.net/common/destiny2_content/geometry/platform/mobile/geometry/{itemDef.gearAsset.content[0].geometry[0]}");
            }
            catch (Exception e)
            {
                continue;
            }

            // Read TGXM
            // File.WriteAllBytes("C:/T/geometry.tgxm", tgxm);
            var br = new BinaryReader(new MemoryStream(tgxm));
            // br.BaseStream.Seek(8, SeekOrigin.Begin);
            var magic = br.ReadBytes(4);
            if (magic.Equals(new byte [] {0x54, 0x47, 0x58, 0x4d}))
            {
                continue;
            }
            var version = br.ReadUInt32();
            var fileOffset = br.ReadInt32();
            var fileCount = br.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Seek(fileOffset+0x110*i, SeekOrigin.Begin);
                var fileName = Encoding.ASCII.GetString(br.ReadBytes(0x100)).TrimEnd('\0');
                var offset = br.ReadInt32();
                var type = br.ReadInt32();
                var size = br.ReadInt32();
                if (fileName.Contains(".js"))
                {
                    byte[] fileData;
                    Array.Copy(tgxm, offset, fileData = new byte[size], 0, size);
                    File.WriteAllBytes($"C:/T/geom/{itemHash.Hash}_{fileName}", fileData);
                }
            }
        }
    }

    public static void DebugAPIRenderMetadata()
    {
        
        var files = Directory.GetFiles("C:/T/geom");
        foreach (var file in files)
        {
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
            var data = json["render_model"]["render_meshes"];
            var a = 0;
        }
    }

    private static ManifestData MakeGetRequestManifestData(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsStringAsync().Result;
            if (content.Contains("\"gearAsset\": false"))
            {
                return null;
            }
            ManifestData item = System.Text.Json.JsonSerializer.Deserialize<ManifestData>(content);
            return item;
        }
    }
    
    private static byte[] MakeGetRequest(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsByteArrayAsync().Result;
            return content;
        }
    }
    
    private class ManifestData
    {
        public dynamic requestedId { get; set; }
        public DestinyGearAssetsDefinition gearAsset { get; set; }
        public dynamic definition { get; set; }
    }
    
    private class DestinyGearAssetsDefinition
    {
        public string[] gear { get; set; }
        public ContentDefinition[] content { get; set; }
    }
    
    private class ContentDefinition
    {
        public string platform { get; set; }
        public string[] geometry { get; set; }
        public string[] textures { get; set; }
        public IndexSet male_index_set { get; set; }
        public IndexSet female_index_set { get; set; }
        public IndexSet dye_index_set { get; set; }
        public Dictionary<string, IndexSet[]> region_index_sets { get; set; }
    }
    
    private class IndexSet
    {
        public int[] textures { get; set; }
        public int[] geometry { get; set; }
    }
    #endif
    public static void ExportShader(InventoryItem item, string savePath, string name, ETextureFormat outputTextureFormat)
    {
        Dictionary<string, Dye> dyes = new Dictionary<string, Dye>();
        
        // export all the customDyes
        if (item.Header.Unk90 is D2Class_77738080 translationBlock)
        {
            foreach (var dyeEntry in translationBlock.CustomDyes)
            {
                Dye dye = GetDyeFromIndex(dyeEntry.DyeIndex);
                dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                dyes.Add(Dye.GetChannelName(GetChannelHashFromIndex(dyeEntry.ChannelIndex)), dye);
            }
        }
        

        
        // armour
        AutomatedImporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye>{dyes["ArmorPlate"],dyes["ArmorSuit"],dyes["ArmorCloth"]}, "_armour");

        // ghost
        AutomatedImporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye>{dyes["GhostMain"],dyes["GhostHighlights"],dyes["GhostDecals"]}, "_ghost");

        // ship
        AutomatedImporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye>{dyes["ShipUpper"],dyes["ShipDecals"],dyes["ShipLower"]}, "_ship");

        // sparrow
        AutomatedImporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye>{dyes["SparrowUpper"],dyes["SparrowEngine"],dyes["SparrowLower"]}, "_sparrow");

        // weapon
        AutomatedImporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye>{dyes["Weapon1"],dyes["Weapon2"],dyes["Weapon3"]}, "_weapon");
    }
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

    private TextureHeader? GetTexture(Tag<D2Class_CF3E8080> iconSecondaryContainer)
    {
        var prim = iconSecondaryContainer.Header.Unk10;
        if (prim is D2Class_CD3E8080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            return structCD3E8080.Unk00[0].TextureList[0].IconTexture;
        }
        if (prim is D2Class_CB3E8080 structCB3E8080)
        {
            return structCB3E8080.Unk00[0].TextureList[0].IconTexture;
        }

        return null;
    }

    public UnmanagedMemoryStream? GetIconBackgroundStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = InvestmentHandler.GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.Header.IconBackgroundContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.Header.IconBackgroundContainer);
        return backgroundIcon.GetTexture();
    }
    
    public UnmanagedMemoryStream? GetIconPrimaryStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = InvestmentHandler.GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.Header.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.Header.IconPrimaryContainer);
        return primaryIcon.GetTexture();
    }
    
    public UnmanagedMemoryStream? GetIconOverlayStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = InvestmentHandler.GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.Header.IconOverlayContainer == null)
            return null;
        var overlayIcon = GetTexture(iconContainer.Header.IconOverlayContainer);
        return overlayIcon.GetTexture();
    }
}