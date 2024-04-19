using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Arithmic;
using ConcurrentCollections;
using Newtonsoft.Json;
using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
[InitializeAfter(typeof(PackageResourcer))]
public class Investment : Strategy.LazyStrategistSingleton<Investment>
{
    private Tag<D2Class_8C798080> _inventoryItemIndexDictTag = null;
    private Tag<D2Class_97798080> _inventoryItemMap = null;
    private Tag<D2Class_F2708080> _artArrangementMap = null;
    private Tag<D2Class_CE558080> _entityAssignmentTag = null;
    private Tag<D2Class_434F8080> _entityAssignmentsMap = null;
    private Tag<D2Class_99548080> _inventoryItemStringThing = null;
    private Tag<D2Class_8C978080> _sandboxPatternAssignmentsTag = null;
    private Tag<D2Class_AA528080> _sandboxPatternGlobalTagIdTag = null;
    public ConcurrentDictionary<int, Tag<D2Class_9F548080>> InventoryItemStringThings = null;
    private Dictionary<uint, int> _inventoryItemIndexmap = null;
    private Dictionary<uint, Tag<D2Class_A36F8080>> _sortedArrangementHashmap = null;
    // private Dictionary<TigerHash, FileHash> _sortedPatternGlobalTagIdAssignments = null;
    private Tag<D2Class_095A8080> _localizedStringsIndexTag = null;
    private Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;
    private ConcurrentDictionary<uint, InventoryItem> _inventoryItems = null;
    private Tag<D2Class_015A8080> _inventoryItemIconTag = null;
    // private Tag<D2Class_8C978080> _dyeManifestTag = null;
    private Tag<D2Class_C2558080> _artDyeReferenceTag = null;
    private Tag<SDyeChannels> _dyeChannelTag = null;

    private Tag<SC2188080> _talentGridMap = null;
    private Tag<D2Class_CD778080> _randomizedPlugSetMap = null;
    private Tag<D2Class_B6768080> _socketTypeMap = null;
    private Tag<D2Class_594F8080> _socketCategoryMap = null;
    private Tag<D2Class_CF508080> _loreStringMap = null;
    private Tag<D2Class_2D548080> _sandboxPerkMap = null;
    public ConcurrentDictionary<int, D2Class_5D4F8080> SocketCategoryStringThings = null;
    public ConcurrentDictionary<int, D2Class_D3508080> InventoryItemLoreStrings = null;
    public ConcurrentDictionary<int, D2Class_33548080> SandboxPerkStrings = null;

    public Investment(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Reset() => throw new NotImplementedException();

    protected override void Initialise()
    {
        if (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 || _strategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            GetAllInvestmentTags();
        }
        else
        {
            Log.Info("API is not supported for versions below DESTINY2_WITCHQUEEN_6307");
        }
    }

    public string GetItemName(InventoryItem item)
    {
        return GetItemName(item.TagData.InventoryItemHash);
    }

    public string GetItemNameSanitized(InventoryItem item)
    {
        return Regex.Replace(GetItemName(item.TagData.InventoryItemHash), @"[^\u0000-\u007F]", "_");
    }

    public string GetItemName(TigerHash hash)
    {
        return InventoryItemStringThings[GetItemIndex(hash)].TagData.ItemName.Value.ToString();
    }

    public D2Class_D3508080? GetItemLore(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        if (item.TagData.Unk30.GetValue(item.GetReader()) is D2Class_B6738080)
            return InventoryItemLoreStrings[((D2Class_B6738080)item.TagData.Unk30.GetValue(item.GetReader())).LoreEntryIndex];
        else
            return null;
    }

    public Tag<D2Class_9F548080>? GetItemStringThing(TigerHash hash)
    {
        return InventoryItemStringThings[GetItemIndex(hash)];
    }

    public Tag<D2Class_B83E8080>? GetItemIconContainer(InventoryItem item)
    {
        return GetItemIconContainer(item.TagData.InventoryItemHash);
    }

    public Tag<D2Class_B83E8080>? GetItemIconContainer(TigerHash hash)
    {
        if (_strategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            return InventoryItemStringThings[GetItemIndex(hash)].TagData.IconContainer;
        }
        else
        {
            int iconIndex = InventoryItemStringThings[GetItemIndex(hash)].TagData.IconIndex;
            if (iconIndex == -1)
                return null;
            return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), iconIndex).IconContainer;
        }

    }

    public Tag<D2Class_B83E8080>? GetFoundryItemIconContainer(InventoryItem item)
    {
        return GetFoundryItemIconContainer(item.TagData.InventoryItemHash);
    }

    public Tag<D2Class_B83E8080>? GetFoundryItemIconContainer(TigerHash hash)
    {
        int iconIndex = InventoryItemStringThings[GetItemIndex(hash)].TagData.FoundryIconIndex;
        if (iconIndex == -1)
            return null;
        return _inventoryItemIconTag.TagData.InventoryItemIconsMap.ElementAt(_inventoryItemIconTag.GetReader(), iconIndex).IconContainer;
    }

    public int GetItemIndex(TigerHash hash)
    {
        return _inventoryItemIndexmap[hash.Hash32];
    }

    public int GetItemIndex(uint hash32)
    {
        return _inventoryItemIndexmap[hash32];
    }

    public int GetSocketCategoryIndex(int index)
    {
        return _socketTypeMap.TagData.SocketTypeEntries.ElementAt(_socketTypeMap.GetReader(), index).SocketCategoryIndex;
    }

    private void GetAllInvestmentTags()
    {
        ConcurrentHashSet<FileHash> allHashes = new();
        // Iterate over all investment pkgs until we find all the tags we need
        if (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            bool PackageFilterFunc(string packagePath) => packagePath.Contains("investment") || packagePath.Contains("client_startup");
            allHashes = PackageResourcer.Get().GetAllHashes(PackageFilterFunc);
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
                    case 0x808051f2:  // shadowkeep is 0x80805bde
                        _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(val);
                        break;
                }
            });
        }
        else if (_strategy == TigerStrategy.DESTINY1_RISE_OF_IRON) // No need to loop hashes when D1 will never change
        {
            //_inventoryItemIndexDictTag = FileResourcer.Get().GetSchemaTag<D2Class_8C798080>(val); // Unused?
            //_artArrangementMap = FileResourcer.Get().GetSchemaTag<D2Class_F2708080>(val); // Unused?
            //_inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<D2Class_015A8080>(val); // Not in D1, icons are with _inventoryItemStringThing

            _inventoryItemMap = FileResourcer.Get().GetSchemaTag<D2Class_97798080>(new FileHash("BEFFA580"));
            _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<D2Class_CE558080>(new FileHash("A7FFA580"));
            _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<D2Class_99548080>(new FileHash("9CFFA580"));
            _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<D2Class_095A8080>(new FileHash("1AE2A580"));
            _sandboxPatternAssignmentsTag = FileResourcer.Get().GetSchemaTag<D2Class_8C978080>(new FileHash("DCE1A780")); // also art dye refs
            _entityAssignmentsMap = FileResourcer.Get().GetSchemaTag<D2Class_434F8080>(new FileHash("DDE1A780"));

            // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
            _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<D2Class_AA528080>(new FileHash("A9FFA580"));

            _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<D2Class_C2558080>(new FileHash("A8FFA580"));
            _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(new FileHash("49E2A580"));

            _talentGridMap = FileResourcer.Get().GetSchemaTag<SC2188080>(new FileHash("27E2A580"));
        }

        GetLocalizedStringsIndexDict(); // must be before GetInventoryItemStringThings

        // must be after string index is built

        if (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            Parallel.ForEach(allHashes, (val, state, i) =>
            {
                switch (val.GetReferenceHash().Hash32)
                {
                    case 0x808077CD:
                        _randomizedPlugSetMap = FileResourcer.Get().GetSchemaTag<D2Class_CD778080>(val);
                        break;
                    case 0x808076B6:
                        _socketTypeMap = FileResourcer.Get().GetSchemaTag<D2Class_B6768080>(val);
                        break;
                    case 0x80804F59:
                        _socketCategoryMap = FileResourcer.Get().GetSchemaTag<D2Class_594F8080>(val);
                        break;
                    //case 0x8080586B:
                    //    statDefTag = FileResourcer.Get().GetSchemaTag<D2Class_6B588080>(val);
                    //    break;
                    case 0x808050CF:
                        _loreStringMap = FileResourcer.Get().GetSchemaTag<D2Class_CF508080>(val);
                        break;
                    case 0x8080542D:
                        _sandboxPerkMap = FileResourcer.Get().GetSchemaTag<D2Class_2D548080>(val);
                        break;
                        //case 0x80807828:
                        //    collectibleDefTag = FileResourcer.Get().GetSchemaTag<D2Class_28788080>(val);
                        //    break;
                        //case 0x808059BF:
                        //    collectibleStringsDefTag = FileResourcer.Get().GetSchemaTag<D2Class_BF598080>(val);
                        //    break;
                }
            });
        }

        Task.WaitAll(new[]
        {
            Task.Run(GetInventoryItemDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetSocketCategoryStrings),
            Task.Run(GetInventoryItemLoreStrings),
            Task.Run(GetSandboxPerkStrings)
            // Task.Run(GetSandboxPatternAssignmentsDict),
        });
    }

    private void GetInventoryItemStringThings()
    {
        InventoryItemStringThings = new ConcurrentDictionary<int, Tag<D2Class_9F548080>>();
        using TigerReader reader = _inventoryItemStringThing.GetReader();
        for (int i = 0; i < _inventoryItemStringThing.TagData.StringThings.Count; i++)
        {
            InventoryItemStringThings.TryAdd(i, _inventoryItemStringThing.TagData.StringThings[reader, i].GetStringThing());
        }
    }

    private void GetInventoryItemLoreStrings()
    {
        InventoryItemLoreStrings = new();
        using TigerReader reader = _loreStringMap.GetReader();
        for (int i = 0; i < _loreStringMap.TagData.LoreStringMap.Count; i++)
        {
            InventoryItemLoreStrings.TryAdd(i, _loreStringMap.TagData.LoreStringMap[reader, i]);
        }
    }

    private void GetSocketCategoryStrings()
    {
        if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            SocketCategoryStringThings = new ConcurrentDictionary<int, D2Class_5D4F8080>();
            using TigerReader reader = _socketCategoryMap.GetReader();
            for (int i = 0; i < _socketCategoryMap.TagData.SocketCategoryEntries.Count; i++)
            {
                SocketCategoryStringThings.TryAdd(i, _socketCategoryMap.TagData.SocketCategoryEntries[reader, i]);
            }
        }
    }

    private void GetSandboxPerkStrings()
    {
        if (Strategy.CurrentStrategy != TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            SandboxPerkStrings = new();
            using TigerReader reader = _sandboxPerkMap.GetReader();
            for (int i = 0; i < _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries.Count; i++)
            {
                SandboxPerkStrings.TryAdd(i, _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries[reader, i]);
            }
        }
    }

    private void GetLocalizedStringsIndexDict()
    {
        _localizedStringsIndexMap = new Dictionary<int, LocalizedStrings>(_localizedStringsIndexTag.TagData.StringContainerMap.Count);
        using TigerReader reader = _localizedStringsIndexTag.GetReader();
        for (int i = 0; i < _localizedStringsIndexTag.TagData.StringContainerMap.Count; i++)
        {
            _localizedStringsIndexMap.Add(i, _localizedStringsIndexTag.TagData.StringContainerMap[reader, i].GetLocalizedStrings());
        }
    }

    public LocalizedStrings GetLocalizedStringsFromIndex(int index)
    {
        // presume we want to read from it, so load it
        LocalizedStrings ls = _localizedStringsIndexMap[index];
        ls.Load();
        return ls;
    }

    private void GetEntityAssignmentDict()
    {
        _sortedArrangementHashmap = new Dictionary<uint, Tag<D2Class_A36F8080>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (var e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
        {
            _sortedArrangementHashmap.Add(e.AssignmentHash, e.EntityParent);
        }
    }

    public Tag<S63198080> GetTalentGrid(int index)
    {
        return _talentGridMap.TagData.TalentGridEntries.ElementAt(_talentGridMap.GetReader(), index).TalentGrid;
    }

    public DynamicArray<D2Class_D5778080> GetRandomizedPlugSet(int index)
    {
        return _randomizedPlugSetMap.TagData.PlugSetDefinitionEntries.ElementAt(_randomizedPlugSetMap.GetReader(), index).ReusablePlugItems;
    }

    //public D2Class_5D4F8080 GetSocketCategoryInfo(int index)
    //{
    //    int index2 = GetSocketCategoryIndex(index);
    //    return _socketCategoryMap.TagData.SocketCategoryEntries.ElementAt(_socketCategoryMap.GetReader(), index2);
    //}

    public Entity.Entity? GetPatternEntityFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;

        var patternGlobalId = GetPatternGlobalTagId(item);
        var patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);
        if (patternData.HasValue && patternData.Value.GetEntityRelationHash().IsValid()
            && patternData.Value.GetEntityRelationHash().GetReferenceHash() == (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 ? 0x80809ad8 : 0x80800734))
        {
            return FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.GetEntityRelationHash());
        }
        return null;
    }

    public TigerHash GetPatternGlobalTagId(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetWeaponPatternIndex()].PatternGlobalTagIdHash;
    }

    public TigerHash GetWeaponContentGroupHash(InventoryItem item)
    {
        return _sandboxPatternGlobalTagIdTag.TagData.SandboxPatternGlobalTagId[_sandboxPatternGlobalTagIdTag.GetReader(), item.GetWeaponPatternIndex()].WeaponContentGroupHash;
    }

    public TigerHash GetChannelHashFromIndex(short index)
    {
        return _dyeChannelTag.TagData.ChannelHashes[_dyeChannelTag.GetReader(), index].ChannelHash;
    }

    public Dye? GetDyeFromIndex(short index)
    {
        var artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);

        var dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);
        if (dyeEntry.HasValue && dyeEntry.Value.GetEntityRelationHash().GetReferenceHash() == 0x80806fa3)
        {
            return FileResourcer.Get().GetSchemaTag<D2Class_E36C8080>(FileResourcer.Get().GetSchemaTag<D2Class_A36F8080>(dyeEntry.Value.GetEntityRelationHash()).TagData.GetEntityData()).TagData.Dye;
        }
        return null;
    }

    public DyeD1 GetD1DyeFromIndex(short index)
    {
        var artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);
        var dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);

        if (dyeEntry.HasValue && dyeEntry.Value.GetEntityRelationHash().GetReferenceFromManifest() == "63348080")
        {
            return FileResourcer.Get().GetFile<DyeD1>(FileResourcer.Get().GetSchemaTag<D2Class_A36F8080>(dyeEntry.Value.GetEntityRelationHash()).TagData.GetEntityData());
        }
        return null;
    }

    public InventoryItem? TryGetInventoryItem(TigerHash hash)
    {
        if (_inventoryItemIndexmap.ContainsKey(hash))
            return GetInventoryItem(_inventoryItemIndexmap[hash]);
        else
            return null;
    }

    public InventoryItem GetInventoryItem(TigerHash hash)
    {
        return GetInventoryItem(_inventoryItemIndexmap[hash]);
    }

    public InventoryItem GetInventoryItem(int index)
    {
        var item = _inventoryItemMap.TagData.InventoryItemDefinitionEntries.ElementAt(_inventoryItemMap.GetReader(), index).InventoryItem;
        if (!item.IsLoaded())
            item.Load();

        return _inventoryItemMap.TagData.InventoryItemDefinitionEntries.ElementAt(_inventoryItemMap.GetReader(), index).InventoryItem;
    }

    public void GetInventoryItemDict()
    {
        _inventoryItemIndexmap = new Dictionary<uint, int>();
        _inventoryItems = new ConcurrentDictionary<uint, InventoryItem>();

        using TigerReader reader = _inventoryItemMap.GetReader();
        for (int i = 0; i < _inventoryItemMap.TagData.InventoryItemDefinitionEntries.Count; i++)
        {
            D2Class_9B798080 entry = _inventoryItemMap.TagData.InventoryItemDefinitionEntries[reader, i];
            _inventoryItemIndexmap.Add(entry.InventoryItemHash, i);
            _inventoryItems.TryAdd(entry.InventoryItemHash, entry.InventoryItem);
        }
    }

    // Getter so we can load them properly
    public async Task<IEnumerable<InventoryItem>> GetInventoryItems()
    {
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 16, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_inventoryItems.Values, parallelOptions, async (item, ct) =>
        {
            // todo needs a proper consumer queue
            item.Load();
        });
        return _inventoryItems.Values;
    }

    public TigerHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(_artArrangementMap.GetReader(), item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    public List<Entity.Entity> GetEntitiesFromHash(TigerHash hash)
    {
        var item = GetInventoryItem(hash);
        var index = item.GetArtArrangementIndex();
        List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    private List<Entity.Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity.Entity> entities = new();
        var entry = _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), index);
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

    private Entity.Entity GetEntityFromAssignmentHash(TigerHash assignmentHash)
    {
        // We can binary search here as the list is sorted.
        // var x = new D2Class_454F8080 {AssignmentHash = assignmentHash};
        // var index = _entityAssignmentsMap.TagData.EntityArrangementMap.BinarySearch(x, new D2Class_454F8080());
        if (!_sortedArrangementHashmap.ContainsKey(assignmentHash))
            return null;
        Tag<D2Class_A36F8080> tag = _sortedArrangementHashmap[assignmentHash];
        tag.Load();
        if (tag.TagData.GetEntityData().IsInvalid() || tag.TagData.GetEntityData() is null)
            return null;

        // if entity
        if (tag.TagData.GetEntityData().GetReferenceHash() == (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 ? 0x80809ad8 : 0x80800734))
        {
            return FileResourcer.Get().GetFile<Entity.Entity>(tag.TagData.GetEntityData());
        }
        return null;
        // return new Entity(_entityAssignmentsMap.TagData.EntityArrangementMap[index].EntityParent.TagData.Entity);
        // return null;
    }

#if DEBUG
    public void DebugAllInvestmentEntities()
    {
        Dictionary<string, Dictionary<dynamic, TigerHash>> data = new();
        for (int i = (int)_entityAssignmentTag.TagData.ArtArrangementEntityAssignments.Count - 1; i >= 0; i--)
        {
            List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(i);
            foreach (var entity in entities)
            {
                bool bAllValid = true;
                if (entity is null || entity.Model is null)
                    continue;
                foreach (var entry in entity.Model.TagData.Meshes[entity.Model.GetReader(), 0].Parts)
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
                            data[field.Name].Add(fieldValue, _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), i).ArtArrangementHash);
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

    public void DebugAPIRequestAllInfo()
    {
        // get all inventory item hashes

        // var itemHash = 138282166;
        var l = _inventoryItemIndexmap.Keys.ToList();
        var shuffled = l.OrderBy(a => rng.Next()).ToList();
        foreach (var itemHash in shuffled)
        {
            if (itemHash != 731561450)
                continue;
            ManifestData? itemDef;
            byte[] tgxm;
            try
            {
                itemDef = MakeGetRequestManifestData($"https://www.light.gg/db/items/{itemHash}/?raw=2");
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
            if (magic.Equals(new byte[] { 0x54, 0x47, 0x58, 0x4d }))
            {
                continue;
            }
            var version = br.ReadUInt32();
            var fileOffset = br.ReadInt32();
            var fileCount = br.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                br.BaseStream.Seek(fileOffset + 0x110 * i, SeekOrigin.Begin);
                var fileName = Encoding.ASCII.GetString(br.ReadBytes(0x100)).TrimEnd('\0');
                var offset = br.ReadInt32();
                var type = br.ReadInt32();
                var size = br.ReadInt32();
                if (fileName.Contains(".js"))
                {
                    byte[] fileData;
                    Array.Copy(tgxm, offset, fileData = new byte[size], 0, size);
                    File.WriteAllBytes($"C:/T/geom/{itemHash}_{fileName}", fileData);
                }
            }
        }
    }

    public void DebugAPIRenderMetadata()
    {

        var files = Directory.GetFiles("C:/T/geom");
        foreach (var file in files)
        {
            dynamic json = JsonConvert.DeserializeObject(File.ReadAllText(file));
            var data = json["render_model"]["render_meshes"];
            var a = 0;
        }
    }

    private ManifestData MakeGetRequestManifestData(string url)
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

    private byte[] MakeGetRequest(string url)
    {
        using (var client = new HttpClient())
        {
            client.Timeout = TimeSpan.FromSeconds(2);
            var response = client.GetAsync(url).Result;
            var content = response.Content.ReadAsByteArrayAsync().Result;
            return content;
        }
    }
#pragma warning disable S1144 // Unused private types or members should be removed
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

    public void ExportShader(InventoryItem item, string savePath, string name, TextureExportFormat outputTextureFormat)
    {
        if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            Dictionary<string, DyeD1> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is D2Class_77738080 translationBlock)
            {
                foreach (var dyeEntry in translationBlock.CustomDyes)
                {
                    DyeD1 dye = GetD1DyeFromIndex(dyeEntry.DyeIndex);
                    dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                    dyes.Add(DyeD1.GetChannelName(GetChannelHashFromIndex(dyeEntry.ChannelIndex)), dye);
                }
            }
            // appliable shaders in D1 only supported armor
            AutomatedExporter.SaveD1ShaderInfo(savePath, name, outputTextureFormat, new List<DyeD1> { dyes["ArmorPlate"], dyes["ArmorSuit"], dyes["ArmorCloth"] }, "_armor"); // imagine spelling armor with a 'u' (laughs in freedom units)
        }
        else
        {
            Dictionary<string, Dye> dyes = new();
            // export all the customDyes
            if (item.TagData.Unk90.GetValue(item.GetReader()) is D2Class_77738080 translationBlock)
            {
                foreach (var dyeEntry in translationBlock.CustomDyes)
                {
                    Dye dye = GetDyeFromIndex(dyeEntry.DyeIndex);
                    dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                    dyes.Add(Dye.GetChannelName(GetChannelHashFromIndex(dyeEntry.ChannelIndex)), dye);
                }
            }
            // armor
            AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ArmorPlate"], dyes["ArmorSuit"], dyes["ArmorCloth"] }, "_armour");
            // ghost
            AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["GhostMain"], dyes["GhostHighlights"], dyes["GhostDecals"] }, "_ghost");
            // ship
            AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ShipUpper"], dyes["ShipDecals"], dyes["ShipLower"] }, "_ship");
            // sparrow
            AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["SparrowUpper"], dyes["SparrowEngine"], dyes["SparrowLower"] }, "_sparrow");
            // weapon
            AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["Weapon1"], dyes["Weapon2"], dyes["Weapon3"] }, "_weapon");
        }
    }
}


public class InventoryItem : Tag<D2Class_9D798080>
{
    public InventoryItem(FileHash hash, bool shouldParse) : base(hash, shouldParse)
    {
    }

    public int GetArtArrangementIndex()
    {
        if (_tag.Unk90.GetValue(GetReader()) is D2Class_77738080 entry)
        {
            if (entry.Arrangements.Count > 0)
                return entry.Arrangements[GetReader(), 0].ArtArrangementHash;
        }
        return -1;
    }

    public int GetWeaponPatternIndex()
    {
        if (_tag.Unk90.GetValue(GetReader()) is D2Class_77738080 entry)
        {
            if (entry.WeaponPatternIndex > 0)
                return entry.WeaponPatternIndex;
        }
        return -1;
    }

    public List<InventoryItem> GetItemOrnaments()
    {
        List<InventoryItem> ornaments = new();
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && _tag.Unk70.GetValue(GetReader()) is D2Class_C0778080 sockets)
        {
            foreach (var socket in sockets.SocketEntries)
            {
                if (socket.SocketTypeIndex == -1 || Investment.Get().SocketCategoryStringThings[Investment.Get().GetSocketCategoryIndex(socket.SocketTypeIndex)].SocketName.Value != "WEAPON COSMETICS")
                    continue;

                foreach (var plug in socket.PlugItems)
                {
                    if (plug.PlugInventoryItemIndex == -1)
                        continue;

                    ornaments.Add(Investment.Get().GetInventoryItem(plug.PlugInventoryItemIndex));
                }
            }
        }
        else if (Strategy.CurrentStrategy == TigerStrategy.DESTINY1_RISE_OF_IRON && _tag.Unk78.GetValue(GetReader()) is SBD178080 a)
        {
            var talentGrid = Investment.Get().GetTalentGrid(a.TalenGridIndex);
            foreach (var node in talentGrid.TagData.Nodes)
            {
                foreach (var entry in node.Unk18)
                {
                    foreach (var entry2 in entry.Unk70)
                    {
                        if (entry2.PlugItemIndex == -1)
                            continue;

                        ornaments.Add(Investment.Get().GetInventoryItem(entry2.PlugItemIndex));
                    }
                }
            }
        }
        return ornaments;
    }

    private Texture? GetTexture(Tag<D2Class_CF3E8080> iconSecondaryContainer)
    {
        using TigerReader reader = iconSecondaryContainer.GetReader();
        dynamic? prim = iconSecondaryContainer.TagData.Unk10.GetValue(reader);
        if (prim is D2Class_CD3E8080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            return structCD3E8080.Unk00[reader, 0].TextureList[reader, 0].IconTexture;
        }
        if (prim is D2Class_CB3E8080 structCB3E8080)
        {
            return structCB3E8080.Unk00[reader, 0].TextureList[reader, 0].IconTexture;
        }
        return null;
    }

    public UnmanagedMemoryStream? GetIconBackgroundStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBackgroundContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.TagData.IconBackgroundContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconBackgroundOverlayStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBGOverlayContainer == null)
            return null;
        var backgroundIcon = GetTexture(iconContainer.TagData.IconBGOverlayContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconPrimaryStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon.GetTexture();
    }

    public Texture? GetIconPrimaryTexture()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var primaryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon;
    }

    public UnmanagedMemoryStream? GetIconOverlayStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconOverlayContainer == null)
            return null;
        var overlayIcon = GetTexture(iconContainer.TagData.IconOverlayContainer);
        return overlayIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetFoundryIconStream()
    {
        Tag<D2Class_B83E8080>? iconContainer = Investment.Get().GetFoundryItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        var foundryIcon = GetTexture(iconContainer.TagData.IconPrimaryContainer);
        return foundryIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetTextureFromHash(FileHash hash)
    {
        Texture texture = FileResourcer.Get().GetFile<Texture>(hash);

        return texture.GetTexture();
    }
}
