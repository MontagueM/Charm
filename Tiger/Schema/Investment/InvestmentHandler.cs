using System.Collections.Concurrent;
using System.Text;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger.Schema.Entity;

namespace Tiger.Schema.Investment;

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
    public static Dictionary<TigerHash, int> InventoryItemIndexmap = null;
    private static Dictionary<TigerHash, Tag<D2Class_A36F8080>> _sortedArrangementHashmap = null;
    // private static Dictionary<TigerHash, FileHash> _sortedPatternGlobalTagIdAssignments = null;
    private static Tag<D2Class_095A8080> _localizedStringsIndexTag = null;
    private static Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;
    public static ConcurrentDictionary<TigerHash, InventoryItem> InventoryItems = null;
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
        return GetItemName(item.TagData.InventoryItemHash);
    }

    public static string GetItemName(TigerHash hash)
    {
        return InventoryItemStringThings[GetItemIndex(hash)].TagData.ItemName.Value;
    }

    public static Tag<D2Class_B83E8080>? GetItemIconContainer(InventoryItem item)
    {
        return GetItemIconContainer(item.TagData.InventoryItemHash);
    }

    public static Tag<D2Class_B83E8080>? GetItemIconContainer(TigerHash hash)
    {
        int iconIndex = InventoryItemStringThings[GetItemIndex(hash)].TagData.IconIndex;
        if (iconIndex == -1)
            return null;
        return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(iconIndex).IconContainer;
    }


    public static int GetItemIndex(TigerHash hash)
    {
        return InventoryItemIndexmap[hash];
    }

    private static async void GetAllInvestmentTags()
    {
        // Iterate over all investment pkgs until we find all the tags we need
        bool PackageFilterFunc(string packagePath) => packagePath.Contains("investment") || packagePath.Contains("client_startup");
        ConcurrentHashSet<FileHash> allHashes = await PackageResourcer.Get().GetAllHashes(PackageFilterFunc);
        // maybe i can parallel this? todo maybe parallel
        Parallel.ForEach(allHashes, (val, state, i) =>
        {
            switch (val.GetReferenceHash().Hash32)
            {
                case 0x80807997:
                    _inventoryItemMap = FileResourcer.Get().GetSchemaTag<D2Class_97798080>(val);
                    break;
                case 0x808070f2:
                    _artArrangementMap = FileResourcer.Get().GetSchemaTag<D2Class_F2708080>(val);
                    break;
                case 0x808055ce:
                    _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<D2Class_CE558080>(val);
                    break;
                case 0x80805499:
                    _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<D2Class_99548080>(val);
                    break;
                case 0x8080798c:
                    _inventoryItemIndexDictTag = FileResourcer.Get().GetSchemaTag<D2Class_8C798080>(val);
                    break;
                case 0x80805a09:
                    _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<D2Class_095A8080>(val);
                    break;
                case 0x80804ea4: // points to parent of the sandbox pattern ref list thing + entity assignment map
                    var parent = FileResourcer.Get().GetSchemaTag<D2Class_A44E8080>(val);
                    _sandboxPatternAssignmentsTag = parent.TagData.SandboxPatternAssignmentsTag; // also art dye refs

                    _entityAssignmentsMap = parent.TagData.EntityAssignmentsMap;

                    break;
                case 0x808052aa: // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
                    _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<D2Class_AA528080>(val);
                    break;
                case 0x80805a01:
                    _inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<D2Class_015A8080>(val);
                    break;
                case 0x808055c2:
                    _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<D2Class_C2558080>(val);
                    break;
                case 0x808051f2:
                    _dyeChannelTag = FileResourcer.Get().GetSchemaTag<D2Class_F2518080>(val);
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
        InventoryItemStringThings = new ConcurrentDictionary<int, Tag<D2Class_9F548080>>();
        for (int i = 0; i < _inventoryItemStringThing.TagData.StringThings.Count; i++)
        {
            InventoryItemStringThings.TryAdd(i, _inventoryItemStringThing.TagData.StringThings[_inventoryItemStringThing.GetReader(), i].StringThing);
        }
    }

    private static void GetContainerIndexDict()
    {
        _localizedStringsIndexMap = new Dictionary<int, LocalizedStrings>(_inventoryItemStringThing.TagData.StringThings.Count);
        for (int i = 0; i < _inventoryItemStringThing.TagData.StringThings.Count; i++)
        {
            _localizedStringsIndexMap.Add(i, _localizedStringsIndexTag.TagData.StringContainerMap[_entityAssignmentsMap.GetReader(), i].StringContainer);
        }
    }

    public static LocalizedStrings GetLocalizedStringsFromIndex(uint index)
    {
        return _localizedStringsIndexMap[(int) index];
    }

    private static void GetEntityAssignmentDict()
    {
        _sortedArrangementHashmap = new Dictionary<TigerHash, Tag<D2Class_A36F8080>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (var e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
        {
            _sortedArrangementHashmap.Add(e.AssignmentHash, e.EntityParent);
        }
    }

    // private static void GetSandboxPatternAssignmentsDict()
    // {
    //     int size = (int)_sandboxPatternAssignmentsTag.TagData.AssignmentBSL.Count;
    //     _sortedPatternGlobalTagIdAssignments = new Dictionary<TigerHash, FileHash>(size);
    //     var br = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.ParentTag.GetHandle();
    //
    //     br.BaseStream.Seek(_sandboxPatternAssignmentsTag.TagData.AssignmentBSL.Offset, SeekOrigin.Begin);
    //     for (int i = 0; i < size; i++)
    //     {
    //         // skips art dye refs, theyre not tag64
    //         var h = new TigerHash(br.ReadUInt32());
    //         br.BaseStream.Seek(0xC, SeekOrigin.Current);
    //         _sortedPatternGlobalTagIdAssignments.Add(h, new FileHash(br.ReadUInt64()));
    //     }
    //     br.Close();
    // }

    public static Entity.Entity? GetPatternEntityFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);
        if (patternData.HasValue && patternData.Value.EntityRelationHash.GetReferenceHash() == 0x80809ad8)
        {
            return FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);
        }
        return null;
    }

    public static TigerHash GetPatternGlobalTagId(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[item.GetWeaponPatternIndex()].PatternGlobalTagIdHash;
    }

    public static TigerHash GetWeaponContentGroupHash(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[item.GetWeaponPatternIndex()].WeaponContentGroupHash;
    }

    public static TigerHash GetChannelHashFromIndex(short index)
    {
        return _dyeChannelTag.TagData.ChannelHashes[index].ChannelHash;
    }

    public static Dye? GetDyeFromIndex(short index)
    {
         var artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(index);

         var dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);
         if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceHash() == 0x80806fa3)
         {
             return FileResourcer.Get().GetSchemaTag<D2Class_E36C8080>(FileResourcer.Get().GetSchemaTag<D2Class_A36F8080>(dyeEntry.Value.EntityRelationHash).TagData.EntityData).TagData.Dye;
         }
         return null;
    }

    public static InventoryItem GetInventoryItem(TigerHash hash)
    {
        return GetInventoryItem(InventoryItemIndexmap[hash]);
    }

    public static InventoryItem GetInventoryItem(int index)
    {
        return _inventoryItemMap.TagData.InventoryItemDefinitionEntries.ElementAt(index).InventoryItem;
    }

    public static void GetInventoryItemDict()
    {
        InventoryItemIndexmap = new Dictionary<TigerHash, int>();
        InventoryItems = new ConcurrentDictionary<TigerHash, InventoryItem>();

        foreach (var e in _inventoryItemMap.TagData.InventoryItemDefinitionEntries.Enumerate(_inventoryItemMap.GetReader()))
        {
            InventoryItems.TryAdd(e.InventoryItemHash, e.InventoryItem);
        }
    }

    public static TigerHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    public static List<Entity.Entity> GetEntitiesFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        var index = item.GetArtArrangementIndex();
        List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    private static List<Entity.Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity.Entity> entities = new();
        var entry = _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(index);
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
                foreach (var assignment in entryMultipleEntityAssignment.EntityAssignmentResource.Value.EntityAssignments)
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

    private static Entity.Entity GetEntityFromAssignmentHash(TigerHash assignmentHash)
    {
        // We can binary search here as the list is sorted.
        // var x = new D2Class_454F8080 {AssignmentHash = assignmentHash};
        // var index = _entityAssignmentsMap.TagData.EntityArrangementMap.BinarySearch(x, new D2Class_454F8080());
        Tag<D2Class_A36F8080> tag = _sortedArrangementHashmap[assignmentHash];
        if (tag.TagData.EntityData is null)
            return null;
        // if entity
        if (tag.TagData.EntityData.GetReferenceHash() == 0x80809ad8)
        {
            return FileResourcer.Get().GetFile<Entity.Entity>(tag.TagData.EntityData);
        }
        return null;
        // return new Entity(_entityAssignmentsMap.TagData.EntityArrangementMap[index].EntityParent.TagData.Entity);
        // return null;
    }

#if DEBUG
    public static void DebugAllInvestmentEntities()
    {
        Dictionary<string, Dictionary<dynamic, TigerHash>> data = new();
        for (int i = (int)_entityAssignmentTag.TagData.ArtArrangementEntityAssignments.Count-1; i >= 0; i--)
        {
            List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(i);
            foreach (var entity in entities)
            {
                bool bAllValid = true;
                if (entity is null || entity.Model is null)
                    continue;
                foreach (var entry in entity.Model.TagData.Meshes[0].Parts)
                {
                    foreach (var field in typeof(D2Class_CB6E8080).GetFields())
                    {
                        if (!data.ContainsKey(field.Name))
                        {
                            data.Add(field.Name, new Dictionary<dynamic, TigerHash>());
                        }
                        dynamic fieldValue = field.GetValue(entry);
                        if (fieldValue is not null && !data[field.Name].ContainsKey(fieldValue) && data[field.Name].Count < 10)
                        {
                            data[field.Name].Add(fieldValue, _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(i).ArtArrangementHash);
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
            if (itemHash.Hash32 != 731561450)
                continue;
            ManifestData? itemDef;
            byte[] tgxm;
            try
            {
                itemDef = MakeGetRequestManifestData($"https://www.light.gg/db/items/{itemHash.Hash32}/?raw=2");
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
                    File.WriteAllBytes($"C:/T/geom/{itemHash.Hash32}_{fileName}", fileData);
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
    public static void ExportShader(InventoryItem item, string savePath, string name, TextureExportFormat outputTextureFormat)
    {
        Dictionary<string, Dye> dyes = new();

        // export all the customDyes
        if (item.TagData.Unk90.Value is D2Class_77738080 translationBlock)
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


public class InventoryItem : Tag<D2Class_9D798080>
{
    public InventoryItem(FileHash hash) : base(hash)
    {
    }

    public int GetArtArrangementIndex()
    {
        if (_tag.Unk90.Value is D2Class_77738080 entry)
        {
            if (entry.Arrangements.Count > 0)
                return entry.Arrangements[0].ArtArrangementHash;
        }
        return -1;
    }

    public int GetWeaponPatternIndex()
    {
        if (_tag.Unk90.Value is D2Class_77738080 entry)
        {
            if (entry.WeaponPatternIndex > 0)
                return entry.WeaponPatternIndex;
        }
        return -1;
    }

    private Texture? GetTexture(Tag<D2Class_CF3E8080> iconSecondaryContainer)
    {
        dynamic? prim = iconSecondaryContainer.TagData.Unk10.Value;
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
        if (iconContainer == null || iconContainer.TagData.IconBackgroundContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.TagData.IconBackgroundContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconPrimaryStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = InvestmentHandler.GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconOverlayStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = InvestmentHandler.GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconOverlayContainer == null)
            return null;
        var overlayIcon = GetTexture(iconContainer.TagData.IconOverlayContainer);
        return overlayIcon.GetTexture();
    }
}
