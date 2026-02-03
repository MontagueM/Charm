using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Arithmic;
using ConcurrentCollections;
using Tiger.Exporters;
using Tiger.Schema.Strings;

namespace Tiger.Schema.Investment;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
[InitializeAfter(typeof(Hash64Map))]
public class Investment : Strategy.LazyStrategistSingleton<Investment>
{
    private Tag<S97798080> _inventoryItemMap = null;
    private Tag<SF2708080> _artArrangementMap = null;
    private Tag<SCE558080> _entityAssignmentTag = null;
    private Tag<S434F8080> _entityAssignmentsMap = null;
    private Tag<S99548080> _inventoryItemStringThing = null;
    private Tag<S8C978080> _sandboxPatternAssignmentsTag = null;
    private Tag<SAA528080> _sandboxPatternGlobalTagIdTag = null;
    private Tag<S095A8080> _localizedStringsIndexTag = null;
    private Tag<S26BA8080> _localizedStringsIndexTag2 = null;
    private Tag<S015A8080> _inventoryItemIconTag = null;
    private Tag<SC2558080> _artDyeReferenceTag = null;
    private Tag<SDyeChannels> _dyeChannelTag = null;

    private Tag<SC2188080> _talentGridMap = null;
    private Tag<SCD778080> _randomizedPlugSetMap = null;
    private Tag<SB6768080> _socketTypeMap = null;
    private Tag<S594F8080> _socketCategoryMap = null;
    private Tag<SCF508080> _loreStringMap = null;
    private Tag<S2D548080> _sandboxPerkMap = null;
    private Tag<SAA768080> _sandboxPerkMap2 = null;
    private Tag<S6B588080> _statDefinitionMap = null;
    private Tag<SBE548080> _statGroupDefinitionMap = null;
    private Tag<S28788080> _collectableDefinitionMap = null;
    private Tag<SBF598080> _collectableStringsMap = null;
    private Tag<S3C758080> _objectiveDefinitionMap = null;
    private Tag<S4C588080> _objectiveStringsMap = null;
    public Tag<SC9798080> _powerCapDefinitionMap = null; // Literally 0 reason for this but fuck it we ball
    public Tag<SD7788080> _presentationNodeDefinitionMap = null;
    public Tag<S03588080> _presentationNodeDefinitionStringMap = null;
    public Tag<S1F718080> _recordNodeDefinitionMap = null;
    public Tag<S87588080> _recordNodeDefinitionStringMap = null;
    public Tag<S80807108> _seasonDefinitionMap = null;
    public Tag<S80804F7E> _seasonDefinitionStringMap = null;
    public Tag<S80807900> _traitDefinitionMap = null;
    public Tag<S808057F6> _traitDefinitionStringMap = null;

    public Tag<S80805615> _unkStyleContainer1 = null; // Event/Activity/Seasonal style(?) container

    public Tag<S8080B3ED> _itemFilterDefinitions = null;

    public Tag<S8080B44E> _equipableItemSetDefinition = null;
    public Tag<S8080B2C6> _equipableItemSetDefinitionStrings = null;

    // Inventory item index -> Collectible index
    private ConcurrentDictionary<int, int> _collectableIndexMap = null;
    public ConcurrentDictionary<int, S2C788080> Collectables = null;
    public ConcurrentDictionary<int, SC3598080> CollectableStrings = null;

    private ConcurrentDictionary<uint, int> _inventoryItemHashIndexMap = null;
    private ConcurrentDictionary<int, InventoryItem> _inventoryItemIndexMap = null;
    private ConcurrentDictionary<uint, InventoryItem> _inventoryItems = null;
    private ConcurrentDictionary<int, S075A8080> _inventoryItemIconMap = null;

    private Dictionary<uint, Tag<SA36F8080>> _sortedArrangementHashmap = null;
    private Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;
    public ConcurrentDictionary<int, Tag<S9F548080>> InventoryItemStringThings = null;

    public ConcurrentDictionary<int, S5D4F8080> SocketCategoryStrings = null;
    public ConcurrentDictionary<int, SD3508080> InventoryItemLoreStrings = null;
    public ConcurrentDictionary<int, S33548080> SandboxPerkStrings = null;
    public ConcurrentDictionary<int, S6F588080> StatStrings = null;

    public ConcurrentDictionary<int, SAE7680800> SandboxPerkMap2 = null;

    public ConcurrentDictionary<int, S40758080> Objectives = null;
    public ConcurrentDictionary<int, S50588080> ObjectiveStrings = null;

    public ConcurrentDictionary<int, SD3778080> RandomizedPlugSetMap = null;
    public ConcurrentDictionary<int, SBA768080> SocketTypeMap = null;

    public ConcurrentDictionary<int, S54B48080> EquipableItemSets = null;
    public ConcurrentDictionary<int, S7AB28080> EquipableItemSetStrings = null;

    public ConcurrentDictionary<int, SFA578080> TraitIndexMap = null;
    public ConcurrentDictionary<DestinyTraitID, SFA578080> TraitMap = null;

    // For exporting purposes, Parent item -> Ornaments
    private ConcurrentDictionary<InventoryItem, ConcurrentBag<InventoryItem>> _ornaments = new();

    // uses item index
    public ConcurrentHashSet<int> FeaturedItems = new();

    public Investment(TigerStrategy strategy) : base(strategy)
    {
    }

    protected override void Reset() => throw new NotImplementedException();

    protected override void Initialise()
    {
        if (_strategy is >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 or TigerStrategy.DESTINY1_RISE_OF_IRON)
        {
            GetAllInvestmentTags();
        }
        else
        {
            Log.Info("API is not supported for versions below DESTINY2_WITCHQUEEN_6307");
        }
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
                // Dumb but string index tags must be set first since StringIndexReference depends on it
                switch (val.GetReferenceHash().Hash32)
                {
                    case 0x80805a09:
                        _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S095A8080>(val);
                        break;
                    case 0x8080BA26:
                        _localizedStringsIndexTag2 = FileResourcer.Get().GetSchemaTag<S26BA8080>(val);
                        break;
                }
            });
            GetLocalizedStringsIndexDict(); // must be done before anything else that uses strings

            Parallel.ForEach(allHashes, (val, state, i) =>
            {
                switch (val.GetReferenceHash().Hash32)
                {
                    case 0x80807997:
                        _inventoryItemMap = FileResourcer.Get().GetSchemaTag<S97798080>(val);
                        break;
                    case 0x808070f2:
                        _artArrangementMap = FileResourcer.Get().GetSchemaTag<SF2708080>(val);
                        break;
                    case 0x808055ce:
                        _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<SCE558080>(val);
                        break;
                    case 0x80805499:
                        _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<S99548080>(val);
                        break;
                    case 0x80804ea4: // points to parent of the sandbox pattern ref list thing + entity assignment map
                        Tag<SA44E8080> parent = FileResourcer.Get().GetSchemaTag<SA44E8080>(val);
                        _sandboxPatternAssignmentsTag = parent.TagData.SandboxPatternAssignmentsTag; // also art dye refs
                        _entityAssignmentsMap = parent.TagData.EntityAssignmentsMap;
                        break;
                    case 0x808052aa: // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
                        _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<SAA528080>(val);
                        break;
                    case 0x80805a01:
                        _inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<S015A8080>(val);
                        break;
                    case 0x808055c2:
                        _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<SC2558080>(val);
                        break;
                    case 0x808051f2:  // shadowkeep is 0x80805bde
                        _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(val);
                        break;


                    case 0x808077CD:
                        _randomizedPlugSetMap = FileResourcer.Get().GetSchemaTag<SCD778080>(val);
                        break;
                    case 0x808076B6:
                        _socketTypeMap = FileResourcer.Get().GetSchemaTag<SB6768080>(val);
                        break;
                    case 0x80804F59:
                        _socketCategoryMap = FileResourcer.Get().GetSchemaTag<S594F8080>(val);
                        break;
                    case 0x808050CF:
                        _loreStringMap = FileResourcer.Get().GetSchemaTag<SCF508080>(val);
                        break;
                    case 0x8080542D:
                        _sandboxPerkMap = FileResourcer.Get().GetSchemaTag<S2D548080>(val);
                        break;
                    case 0x808076AA:
                        _sandboxPerkMap2 = FileResourcer.Get().GetSchemaTag<SAA768080>(val);
                        break;
                    case 0x8080586B:
                        _statDefinitionMap = FileResourcer.Get().GetSchemaTag<S6B588080>(val);
                        break;
                    case 0x808054BE:
                        _statGroupDefinitionMap = FileResourcer.Get().GetSchemaTag<SBE548080>(val);
                        break;
                    case 0x80807828:
                        _collectableDefinitionMap = FileResourcer.Get().GetSchemaTag<S28788080>(val);
                        break;
                    case 0x808059BF:
                        _collectableStringsMap = FileResourcer.Get().GetSchemaTag<SBF598080>(val);
                        break;
                    case 0x8080753C:
                        _objectiveDefinitionMap = FileResourcer.Get().GetSchemaTag<S3C758080>(val);
                        break;
                    case 0x8080584C:
                        _objectiveStringsMap = FileResourcer.Get().GetSchemaTag<S4C588080>(val);
                        break;
                    case 0x808079C9:
                        _powerCapDefinitionMap = FileResourcer.Get().GetSchemaTag<SC9798080>(val);
                        break;
                    case 0x808078D7:
                        _presentationNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<SD7788080>(val);
                        break;
                    case 0x80805803:
                        _presentationNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S03588080>(val);
                        break;
                    case 0x8080711F: //1F718080
                        _recordNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<S1F718080>(val);
                        break;
                    case 0x80805887: //87588080
                        _recordNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S87588080>(val);
                        break;
                    case 0x80807108:
                        _seasonDefinitionMap = FileResourcer.Get().GetSchemaTag<S80807108>(val);
                        break;
                    case 0x80804F7E:
                        _seasonDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S80804F7E>(val);
                        break;
                    case 0x80805615:
                        _unkStyleContainer1 = FileResourcer.Get().GetSchemaTag<S80805615>(val);
                        break;
                    case 0x80807900:
                        _traitDefinitionMap = FileResourcer.Get().GetSchemaTag<S80807900>(val);
                        break;
                    case 0x808057F6:
                        _traitDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S808057F6>(val);
                        break;
                    case 0x8080B3ED:
                        _itemFilterDefinitions = FileResourcer.Get().GetSchemaTag<S8080B3ED>(val);
                        break;
                    case 0x8080B44E:
                        _equipableItemSetDefinition = FileResourcer.Get().GetSchemaTag<S8080B44E>(val);
                        break;
                    case 0x8080B2C6:
                        _equipableItemSetDefinitionStrings = FileResourcer.Get().GetSchemaTag<S8080B2C6>(val);
                        break;
                }
            });
        }
        else if (_strategy == TigerStrategy.DESTINY1_RISE_OF_IRON) // No need to loop hashes when D1 will never change
        {
            _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S095A8080>(new FileHash("1AE2A580"));
            GetLocalizedStringsIndexDict();

            _inventoryItemMap = FileResourcer.Get().GetSchemaTag<S97798080>(new FileHash("BEFFA580"));
            _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<SCE558080>(new FileHash("A7FFA580"));
            _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<S99548080>(new FileHash("9CFFA580"));
            _sandboxPatternAssignmentsTag = FileResourcer.Get().GetSchemaTag<S8C978080>(new FileHash("DCE1A780")); // also art dye refs
            _entityAssignmentsMap = FileResourcer.Get().GetSchemaTag<S434F8080>(new FileHash("DDE1A780"));

            // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
            _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<SAA528080>(new FileHash("A9FFA580"));

            _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<SC2558080>(new FileHash("A8FFA580"));
            _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(new FileHash("49E2A580"));

            _talentGridMap = FileResourcer.Get().GetSchemaTag<SC2188080>(new FileHash("27E2A580"));
        }


        Task.WaitAll(new[]
        {
            Task.Run(DebugPrintTags),
            Task.Run(GetInventoryItemDict),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetItemIconMap),
            Task.Run(GetSocketCategoryStrings),
            Task.Run(GetInventoryItemLoreStrings),
            Task.Run(GetSandboxPerkStrings),
            Task.Run(GetStatStrings),
            Task.Run(GetCollectableIndexDict),
            Task.Run(GetCollectables),
            Task.Run(GetCollectableStrings),
            Task.Run(GetObjectives),
            Task.Run(GetObjectiveStrings),
            Task.Run(GetSandboxPerkMap2),
            Task.Run(GetRandomPlugSetMap),
            Task.Run(GetSocketTypeMap),
            Task.Run(GetFeaturedItemsList),
            Task.Run(GetEquipableItemSetMap),
            Task.Run(GetTraitMap),
        });

        // Debug, slower load times but helps track down issues when things hang
        //RunWithLogging(nameof(DebugPrintTags), DebugPrintTags);
        //RunWithLogging(nameof(GetInventoryItemDict), GetInventoryItemDict);
        //RunWithLogging(nameof(GetEntityAssignmentDict), GetEntityAssignmentDict);
        //RunWithLogging(nameof(GetInventoryItemStringThings), GetInventoryItemStringThings);
        //RunWithLogging(nameof(GetItemIconMap), GetItemIconMap);
        //RunWithLogging(nameof(GetSocketCategoryStrings), GetSocketCategoryStrings);
        //RunWithLogging(nameof(GetInventoryItemLoreStrings), GetInventoryItemLoreStrings);
        //RunWithLogging(nameof(GetSandboxPerkStrings), GetSandboxPerkStrings);
        //RunWithLogging(nameof(GetStatStrings), GetStatStrings);
        //RunWithLogging(nameof(GetCollectableIndexDict), GetCollectableIndexDict);
        //RunWithLogging(nameof(GetCollectables), GetCollectables);
        //RunWithLogging(nameof(GetCollectableStrings), GetCollectableStrings);
        //RunWithLogging(nameof(GetObjectives), GetObjectives);
        //RunWithLogging(nameof(GetObjectiveStrings), GetObjectiveStrings);
        //RunWithLogging(nameof(GetSandboxPerkMap2), GetSandboxPerkMap2);
        //RunWithLogging(nameof(GetRandomPlugSetMap), GetRandomPlugSetMap);
        //RunWithLogging(nameof(GetSocketTypeMap), GetSocketTypeMap);
        //RunWithLogging(nameof(GetFeaturedItemsList), GetFeaturedItemsList);
        //RunWithLogging(nameof(GetEquipableItemSetMap), GetEquipableItemSetMap);
        //RunWithLogging(nameof(GetTraitMap), GetTraitMap);
    }

    public void GetInventoryItemDict()
    {
        _inventoryItemHashIndexMap = new ConcurrentDictionary<uint, int>();
        _inventoryItemIndexMap = new ConcurrentDictionary<int, InventoryItem>();
        _inventoryItems = new ConcurrentDictionary<uint, InventoryItem>();

        using TigerReader reader = _inventoryItemMap.GetReader();
        for (int i = 0; i < _inventoryItemMap.TagData.InventoryItemDefinitionEntries.Count; i++)
        {
            S9B798080 entry = _inventoryItemMap.TagData.InventoryItemDefinitionEntries[reader, i];
            _inventoryItemHashIndexMap.TryAdd(entry.InventoryItemHash, i); // Hash -> Index
            _inventoryItemIndexMap.TryAdd(i, entry.InventoryItem); // Index -> InventoryItem
            _inventoryItems.TryAdd(entry.InventoryItemHash, entry.InventoryItem); // Hash -> InventoryItem
        }
    }

    public async Task<IEnumerable<InventoryItem>> GetInventoryItems()
    {
        ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = CancellationToken.None };
        await Parallel.ForEachAsync(_inventoryItems.Values, parallelOptions, async (item, ct) =>
        {
            // todo needs a proper consumer queue
            item.Load();
        });
        return _inventoryItems.Values;
    }

    public IEnumerable<InventoryItem> GetInventoryItemsUnloaded()
    {
        return _inventoryItems.Values;
    }

    #region Strings
    public string GetItemNameSanitized(InventoryItem item)
    {
        return Regex.Replace(GetItemName(item), @"[^\u0000-\u007F]", "_");
    }

    public string GetItemName(InventoryItem item)
    {
        Tag<S9F548080>? entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemName.Value.ToString();
    }

    public string GetItemType(InventoryItem item)
    {
        Tag<S9F548080>? entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemType.Value.ToString();
    }

    public SD3508080? GetItemLore(InventoryItem item)
    {
        if (item.TagData.Unk30.GetValue(item.GetReader()) is SB6738080)
            return GetItemLore(((SB6738080)item.TagData.Unk30.GetValue(item.GetReader())).LoreEntryIndex);
        else
            return null;
    }

    public SD3508080? GetItemLore(int index)
    {
        if (index == -1)
            return null;
        return InventoryItemLoreStrings[index];
    }

    public Tag<S9F548080>? GetItemStrings(TigerHash hash)
    {
        Tag<S9F548080> entry = GetItemStrings(GetItemIndex(hash));
        return entry;
    }

    public Tag<S9F548080>? GetItemStrings(int index)
    {
        Tag<S9F548080> entry = InventoryItemStringThings[index];
        return entry;
    }

    private void GetSandboxPerkStrings()
    {
        if (Strategy.IsD1())
            return;

        SandboxPerkStrings = new();
        using TigerReader reader = _sandboxPerkMap.GetReader();
        for (int i = 0; i < _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries.Count; i++)
        {
            SandboxPerkStrings.TryAdd(i, _sandboxPerkMap.TagData.SandboxPerkDefinitionEntries[reader, i]);
        }
    }

    private void GetInventoryItemStringThings()
    {
        InventoryItemStringThings = new ConcurrentDictionary<int, Tag<S9F548080>>();
        using TigerReader reader = _inventoryItemStringThing.GetReader();
        for (int i = 0; i < _inventoryItemStringThing.TagData.StringThings.Count; i++)
        {
            InventoryItemStringThings.TryAdd(i, _inventoryItemStringThing.TagData.StringThings[reader, i].StringThing);
        }
    }

    private void GetInventoryItemLoreStrings()
    {
        if (Strategy.IsD1())
            return;

        InventoryItemLoreStrings = new();
        using TigerReader reader = _loreStringMap.GetReader();
        for (int i = 0; i < _loreStringMap.TagData.LoreStringMap.Count; i++)
        {
            InventoryItemLoreStrings.TryAdd(i, _loreStringMap.TagData.LoreStringMap[reader, i]);
        }
    }

    private void GetLocalizedStringsIndexDict()
    {
        _localizedStringsIndexMap = new Dictionary<int, LocalizedStrings>(_localizedStringsIndexTag.TagData.StringContainerMap.Count);
        using TigerReader reader = _localizedStringsIndexTag.GetReader();
        for (int i = 0; i < _localizedStringsIndexTag.TagData.StringContainerMap.Count; i++)
        {
            _localizedStringsIndexMap.Add(i, _localizedStringsIndexTag.TagData.StringContainerMap[reader, i].LocalizedStrings);
        }
    }

    public LocalizedStrings GetLocalizedStringsFromIndex(int index)
    {
        // presume we want to read from it, so load it
        LocalizedStrings ls = _localizedStringsIndexMap[index];
        if (ls is not null)
        {
            ls.Load();
            return ls;
        }
        else // idfk why bungie is doing this...
        {
            using TigerReader reader = _localizedStringsIndexTag.GetReader();
            int otherIndex = _localizedStringsIndexTag.TagData.StringContainerMap[reader, index].Index;
            ls = _localizedStringsIndexTag2.TagData.LocalizedStrings[otherIndex].LocalizedStrings;
            if (ls is not null)
            {
                ls.Load();
                return ls;
            }
        }
        return null;
    }

    private void GetTraitMap()
    {
        if (Strategy.IsD1())
            return;

        TraitMap = new();
        TraitIndexMap = new();
        for (int i = 0; i < _traitDefinitionStringMap.TagData.TraitStrings.Count; i++)
        {
            var trait = _traitDefinitionStringMap.TagData.TraitStrings[i];
            TraitMap.TryAdd(trait.TraitHash, trait);
            TraitIndexMap.TryAdd(i, trait);
        }
    }

    public SFA578080? GetTrait(int index)
    {
        if (!TraitIndexMap.ContainsKey(index))
            return null;

        return TraitIndexMap[index];
    }

    public SFA578080? GetTrait(DestinyTraitID traitID)
    {
        if (!TraitMap.ContainsKey(traitID))
            return null;

        return TraitMap[traitID];
    }

    #endregion

    #region Icons
    private void GetItemIconMap()
    {
        if (Strategy.IsD1())
            return;

        _inventoryItemIconMap = new();
        using TigerReader reader = _inventoryItemIconTag.GetReader();
        for (int i = 0; i < _inventoryItemIconTag.TagData.InventoryItemIconsMap.Count; i++)
        {
            _inventoryItemIconMap.TryAdd(i, _inventoryItemIconTag.TagData.InventoryItemIconsMap[reader, i]);
        }
    }

    public int GetItemIconContainerIndex(InventoryItem item)
    {
        return GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash)).TagData.IconIndex;
    }

    public Tag<SB83E8080>? GetItemIconContainer(InventoryItem item)
    {
        if (Strategy.IsD1())
        {
            return GetItemStrings(GetItemIndex(item.ApiHash)).TagData.IconContainer;
        }
        else
        {
            int iconIndex = GetItemIconContainerIndex(item);
            if (iconIndex == -1)
                return null;

            return GetItemIconContainer(iconIndex);
        }
    }

    public Tag<SB83E8080>? GetItemIconContainer(int index)
    {
        if (index == -1)
            return null;

        var container = _inventoryItemIconMap[index].IconContainer;
        if (container is null)
            return null;

        container.Load();
        return container;
    }

    public Tag<SB83E8080>? GetFoundryItemIconContainer(InventoryItem item)
    {
        int iconIndex = Strategy.IsLatest() ? GetItemStrings(GetItemIndex(item.ApiHash)).TagData.EmblemContainerIndex : GetItemStrings(GetItemIndex(item.ApiHash)).TagData.FoundryIconIndex;
        if (iconIndex == -1)
            return null;

        return GetItemIconContainer(iconIndex);
    }

    public Texture? GetTextureFromContainer(Tag<SCF3E8080> iconContainer, int index = 0, int listIndex = 0)
    {
        using TigerReader reader = iconContainer.GetReader();
        dynamic? prim = iconContainer.TagData.Unk10.GetValue(reader);
        if (prim is SCD3E8080 structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (index >= structCD3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCD3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        if (prim is SCB3E8080 structCB3E8080)
        {
            if (index >= structCB3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCB3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        return null;
    }


    public Texture? GetTextureFromContainer(FileHash containerHash, int index, int listIndex = 0)
    {
        return GetTextureFromContainer(FileResourcer.Get().GetSchemaTag<SCF3E8080>(containerHash), index, listIndex);
    }

    #endregion

    #region Stats/Sockets
    private void GetSocketTypeMap()
    {
        if (Strategy.IsD1())
            return;

        SocketTypeMap = new();
        using TigerReader reader = _socketTypeMap.GetReader();
        for (int i = 0; i < _socketTypeMap.TagData.SocketTypeEntries.Count; i++)
        {
            SocketTypeMap.TryAdd(i, _socketTypeMap.TagData.SocketTypeEntries[reader, i]);
        }
    }

    private void GetRandomPlugSetMap()
    {
        if (Strategy.IsD1())
            return;

        RandomizedPlugSetMap = new();
        using TigerReader reader = _randomizedPlugSetMap.GetReader();
        for (int i = 0; i < _randomizedPlugSetMap.TagData.PlugSetDefinitionEntries.Count; i++)
        {
            RandomizedPlugSetMap.TryAdd(i, _randomizedPlugSetMap.TagData.PlugSetDefinitionEntries[reader, i]);
        }
    }

    private void GetSocketCategoryStrings()
    {
        if (Strategy.IsD1())
            return;

        SocketCategoryStrings = new ConcurrentDictionary<int, S5D4F8080>();
        using TigerReader reader = _socketCategoryMap.GetReader();
        for (int i = 0; i < _socketCategoryMap.TagData.SocketCategoryEntries.Count; i++)
        {
            SocketCategoryStrings.TryAdd(i, _socketCategoryMap.TagData.SocketCategoryEntries[reader, i]);
        }
    }

    private void GetStatStrings()
    {
        if (Strategy.IsD1())
            return;

        StatStrings = new();
        using TigerReader reader = _statDefinitionMap.GetReader();
        for (int i = 0; i < _statDefinitionMap.TagData.StatDefinitions.Count; i++)
        {
            StatStrings.TryAdd(i, _statDefinitionMap.TagData.StatDefinitions[reader, i]);
        }
    }

    private void GetSandboxPerkMap2()
    {
        if (Strategy.IsD1())
            return;

        SandboxPerkMap2 = new();
        using TigerReader reader = _sandboxPerkMap2.GetReader();
        for (int i = 0; i < _sandboxPerkMap2.TagData.SandboxPerkDefinitionEntries.Count; i++)
        {
            SandboxPerkMap2.TryAdd(i, _sandboxPerkMap2.TagData.SandboxPerkDefinitionEntries[reader, i]);
        }
    }

    public SBA768080 GetSocketType(int index)
    {
        return SocketTypeMap[index];
    }

    public int GetSocketCategoryIndex(int index)
    {
        return SocketTypeMap[index].SocketCategoryIndex;
    }

    private int GetStatGroupIndex(InventoryItem item)
    {
        Tag<S9F548080>? stringThing = GetItemStrings(item.TagData.InventoryItemHash);

        if (stringThing.TagData.Unk78.GetValue(stringThing.GetReader()) is SCA548080 details)
            return details.StatGroupIndex;

        return -1;
    }

    public SC4548080? GetStatGroup(InventoryItem item)
    {
        int index = GetStatGroupIndex(item);
        if (index == -1 || index > _statGroupDefinitionMap.TagData.StatGroupDefinitions.Count)
            return null;

        return _statGroupDefinitionMap.TagData.StatGroupDefinitions.ElementAt(_statGroupDefinitionMap.GetReader(), index);
    }

    public Tag<S63198080> GetTalentGrid(int index)
    {
        return _talentGridMap.TagData.TalentGridEntries.ElementAt(_talentGridMap.GetReader(), index).TalentGrid;
    }

    public DynamicArray<SD5778080> GetRandomizedPlugSet(int index)
    {
        return RandomizedPlugSetMap[index].ReusablePlugItems;
    }
    #endregion

    #region Collectible
    private void GetCollectables()
    {
        if (Strategy.IsD1())
            return;

        Collectables = new();
        using TigerReader reader = _collectableDefinitionMap.GetReader();
        for (int i = 0; i < _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.Count; i++)
        {
            Collectables.TryAdd(i, _collectableDefinitionMap.TagData.CollectibleDefinitionEntries[reader, i]);
        }
    }

    private void GetCollectableStrings()
    {
        if (Strategy.IsD1())
            return;

        CollectableStrings = new();
        using TigerReader reader = _collectableStringsMap.GetReader();
        for (int i = 0; i < _collectableStringsMap.TagData.CollectibleDefinitionStringEntries.Count; i++)
        {
            CollectableStrings.TryAdd(i, _collectableStringsMap.TagData.CollectibleDefinitionStringEntries[reader, i]);
        }
    }

    // Inventory Item index -> Collectible index
    public void GetCollectableIndexDict()
    {
        if (Strategy.IsD1())
            return;

        _collectableIndexMap = new ConcurrentDictionary<int, int>();

        using TigerReader reader = _collectableDefinitionMap.GetReader();
        for (int i = 0; i < _collectableDefinitionMap.TagData.CollectibleDefinitionEntries.Count; i++)
        {
            short itemIndex = _collectableDefinitionMap.TagData.CollectibleDefinitionEntries[reader, i].InventoryItemIndex;
            _collectableIndexMap.TryAdd(itemIndex, i);
        }
    }

    public S2C788080? GetCollectible(int index)
    {
        if (index == -1 || index > Collectables.Count)
            return null;

        return Collectables[index];
    }

    public SC3598080? GetCollectibleStrings(int index)
    {
        if (index == -1 || index > CollectableStrings.Count || Strategy.IsD1())
            return null;

        return CollectableStrings[index];
    }

    public SC3598080? GetCollectibleStringsFromItemIndex(int itemIndex)
    {
        if (itemIndex == -1)
            return null;

        var collectibleIndex = GetCollectibleIndexFromItemIndex(itemIndex);
        return GetCollectibleStrings(collectibleIndex);
    }

    public int GetCollectibleIndexFromItemIndex(int itemIndex)
    {
        if (itemIndex == -1 || Strategy.IsD1())
            return -1;

        if (_collectableIndexMap.TryGetValue(itemIndex, out int collectibleIndex))
            return collectibleIndex;
        else
            return -1;
    }
    #endregion

    #region Objective
    private void GetObjectives()
    {
        if (Strategy.IsD1())
            return;

        Objectives = new();
        using TigerReader reader = _objectiveDefinitionMap.GetReader();
        for (int i = 0; i < _objectiveDefinitionMap.TagData.ObjectiveDefinitionEntries.Count; i++)
        {
            Objectives.TryAdd(i, _objectiveDefinitionMap.TagData.ObjectiveDefinitionEntries[reader, i]);
        }
    }

    private void GetObjectiveStrings()
    {
        if (Strategy.IsD1())
            return;

        ObjectiveStrings = new();
        using TigerReader reader = _objectiveStringsMap.GetReader();
        for (int i = 0; i < _objectiveStringsMap.TagData.ObjectiveDefinitionStringEntries.Count; i++)
        {
            ObjectiveStrings.TryAdd(i, _objectiveStringsMap.TagData.ObjectiveDefinitionStringEntries[reader, i]);
        }
    }

    public int GetObjectiveValue(int index)
    {
        if (index == -1 || index > Objectives.Count)
            return 0;

        return Objectives[index].CompletionValue;
    }

    public S50588080? GetObjective(int index)
    {
        if (index == -1 || index > ObjectiveStrings.Count)
            return null;

        return ObjectiveStrings[index];
    }
    #endregion

    #region Item specific
    public InventoryItem? TryGetInventoryItem(TigerHash hash)
    {
        if (_inventoryItemHashIndexMap.ContainsKey(hash))
            return GetInventoryItem(_inventoryItemHashIndexMap[hash]);
        else
            return null;
    }

    public InventoryItem GetInventoryItem(TigerHash hash)
    {
        return GetInventoryItem(_inventoryItemHashIndexMap[hash]);
    }

    public InventoryItem GetInventoryItem(int index)
    {
        InventoryItem item = _inventoryItemIndexMap[index];
        if (!item.IsLoaded())
            item.Load();

        return item;
    }

    public int GetItemIndex(TigerHash hash)
    {
        return _inventoryItemHashIndexMap[hash.Hash32];
    }

    public int GetItemIndex(uint hash32)
    {
        return _inventoryItemHashIndexMap[hash32];
    }

    private void GetEntityAssignmentDict()
    {
        _sortedArrangementHashmap = new Dictionary<uint, Tag<SA36F8080>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (S454F8080 e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
        {
            _sortedArrangementHashmap.Add(e.AssignmentHash, e.EntityParent);
        }
    }

    public Entity.Entity? GetPatternEntityFromHash(TigerHash hash)
    {
        InventoryItem item = GetInventoryItem(hash);
        if (item.GetWeaponPatternIndex() == -1)
            return null;

        TigerHash patternGlobalId = GetPatternGlobalTagId(item);
        Optional<S0F878080> patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);
        if (patternData.HasValue && patternData.Value.EntityRelationHash.IsValid()
            && patternData.Value.EntityRelationHash.GetReferenceHash() == (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 ? 0x80809ad8 : 0x80800734))
        {
            return FileResourcer.Get().GetFile<Entity.Entity>(patternData.Value.EntityRelationHash);
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

    public TigerHash GetArtArrangementHash(InventoryItem item)
    {
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(_artArrangementMap.GetReader(), item.GetArtArrangementIndex()).ArtArrangementHash;
    }

    public List<Entity.Entity> GetEntitiesFromHash(InventoryItem item)
    {
        List<Entity.Entity> entities = new();
        int index = item.GetArtArrangementIndex();
        if (index == -1)
        {
            Log.Warning($"Item {item.Name} ({item.ApiHash}) has no art arrangement index.");
            return entities;
        }

        entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    public List<Entity.Entity> GetEntitiesFromHash(TigerHash hash)
    {
        InventoryItem item = GetInventoryItem(hash);
        int index = item.GetArtArrangementIndex();
        List<Entity.Entity> entities = GetEntitiesFromArrangementIndex(index);
        return entities;
    }

    private List<Entity.Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity.Entity> entities = new();
        SD4558080 entry = _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), index);
        if (entry.MultipleEntityAssignments.Count == 0)  // single
        {
            if (entry.FeminineSingleEntityAssignment.IsValid())
            {
                Entity.Entity entity = GetEntityFromAssignmentHash(entry.FeminineSingleEntityAssignment);
                if (entity != null)
                {
                    entity.Gender = DestinyGenderDefinition.Feminine;
                    entities.Add(entity);
                }
            }
            if (entry.MasculineSingleEntityAssignment.IsValid())
            {
                Entity.Entity entity = GetEntityFromAssignmentHash(entry.MasculineSingleEntityAssignment);
                if (entity != null)
                {
                    entity.Gender = DestinyGenderDefinition.Masculine;
                    entities.Add(entity);
                }
            }
        }
        else
        {
            foreach (SD7558080 entryMultipleEntityAssignment in entry.MultipleEntityAssignments)
            {
                foreach (SDA558080 assignment in entryMultipleEntityAssignment.EntityAssignmentResource.Value.Value.EntityAssignments)
                {
                    if (assignment.EntityAssignmentHash.IsValid())
                    {
                        Entity.Entity assignmentEntity = GetEntityFromAssignmentHash(assignment.EntityAssignmentHash);
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
        // var index = _entityAssignmentsMap.TagData.EntityArrangementMap.BinarySearch(x, new S454F8080());
        if (!_sortedArrangementHashmap.ContainsKey(assignmentHash))
            return null;

        Tag<SA36F8080> tag = _sortedArrangementHashmap[assignmentHash];
        tag.Load();

        if (tag.TagData.EntityData.IsInvalid() || tag.TagData.EntityData is null)
            return null;

        // if entity
        if (tag.TagData.EntityData.GetReferenceHash() == (_strategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 ? 0x80809ad8 : 0x80800734))
            return FileResourcer.Get().GetFile<Entity.Entity>(tag.TagData.EntityData);

        Log.Warning($"Hash is not an Entity: {tag.TagData.EntityData}");
        return null;
    }

    private async Task PopulateOrnaments()
    {
        IEnumerable<InventoryItem> inventoryItems = await GetInventoryItems();
        await Parallel.ForEachAsync(inventoryItems, async (item, ct) =>
        {
            _ornaments.TryAdd(item, item.GetItemOrnaments());
        });
    }

    public async Task<InventoryItem> GetOrnamentParent(InventoryItem ornament)
    {
        if (_ornaments is null || _ornaments.Count is 0)
            await PopulateOrnaments();

        var parent = _ornaments.FirstOrDefault(kv => kv.Value.Contains(ornament)).Key;

        return parent;
    }
    #endregion

    #region Dyes
    public TigerHash GetChannelHashFromIndex(short index)
    {
        return _dyeChannelTag.TagData.ChannelHashes[_dyeChannelTag.GetReader(), index].ChannelHash;
    }

    public Dye? GetDyeFromIndex(short index)
    {
        SC6558080 artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);

        Optional<S0F878080> dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);
        if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceHash() == 0x80806fa3)
        {
            if (dyeEntry.Value.EntityRelationHash is null || dyeEntry.Value.EntityRelationHash.IsInvalid())
                return null;

            var dyeHash = FileResourcer.Get().GetSchemaTag<SA36F8080>(dyeEntry.Value.EntityRelationHash).TagData.EntityData;
            if (dyeHash is null || dyeHash.IsInvalid())
                return null;

            return FileResourcer.Get().GetSchemaTag<SE36C8080>(dyeHash).TagData.Dye;
        }
        return null;
    }

    public DyeD1 GetD1DyeFromIndex(short index)
    {
        SC6558080 artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);
        Optional<S0F878080> dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);

        if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceFromManifest() == "63348080")
        {
            return FileResourcer.Get().GetFile<DyeD1>(FileResourcer.Get().GetSchemaTag<SA36F8080>(dyeEntry.Value.EntityRelationHash).TagData.EntityData);
        }
        return null;
    }

    public void ExportShader(InventoryItem item, string savePath, string name, TextureExportFormat outputTextureFormat)
    {
        if (Strategy.IsD1())
        {
            Dictionary<string, DyeD1> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S77738080 translationBlock)
            {
                foreach (S7B738080 dyeEntry in translationBlock.CustomDyes)
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
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S77738080 translationBlock)
            {
                foreach (S7B738080 dyeEntry in translationBlock.CustomDyes)
                {
                    Dye dye = GetDyeFromIndex(dyeEntry.DyeIndex);
                    dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                    dyes.Add(Dye.GetChannelName(GetChannelHashFromIndex(dyeEntry.ChannelIndex)), dye);
#if DEBUG
                    System.Console.WriteLine($"{item.GetItemName()}: DefaultDye {dye.Hash}");
#endif
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

            Texture iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExtractor.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
    }
    #endregion

    #region Misc
    public void GetFeaturedItemsList()
    {
        if (!Strategy.IsLatest())
            return;

        FeaturedItems = new();
        using TigerReader reader = _itemFilterDefinitions.GetReader();
        foreach (var item in _itemFilterDefinitions.TagData.Filters.First(x => x.FilterHash.Hash32 == 1812452478).FilterList)
        {
            FeaturedItems.Add(item.ItemIndex);
        }
    }

    public void GetEquipableItemSetMap()
    {
        if (!Strategy.IsLatest())
            return;

        EquipableItemSets = new();
        using TigerReader reader = _equipableItemSetDefinition.GetReader();
        for (int i = 0; i < _equipableItemSetDefinition.TagData.ItemSetDefinitions.Count; i++)
        {
            EquipableItemSets.TryAdd(i, _equipableItemSetDefinition.TagData.ItemSetDefinitions[reader, i]);
        }

        // Strings
        EquipableItemSetStrings = new();
        using TigerReader reader2 = _equipableItemSetDefinitionStrings.GetReader();
        for (int i = 0; i < _equipableItemSetDefinitionStrings.TagData.ItemSetDefinitionStrings.Count; i++)
        {
            EquipableItemSetStrings.TryAdd(i, _equipableItemSetDefinitionStrings.TagData.ItemSetDefinitionStrings[reader2, i]);
        }
    }
    #endregion

    private void RunWithLogging(string methodName, Action method)
    {
        try
        {
            Log.Debug($"Starting {methodName}");
            method();
            Log.Debug($"Completed {methodName}");
        }
        catch (Exception ex)
        {
            Log.Error($"Error in {methodName}: {ex.Message}");
            throw;
        }
    }

    public void DebugPrintTags()
    {
        if (Strategy.IsD1())
            return;
#if DEBUG
        var fields = typeof(Investment).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var field in fields)
        {
            if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(Tag<>))
            {
                var tagInstance = field.GetValue(this);
                if (tagInstance != null)
                {
                    var hashProperty = field.FieldType.GetField("Hash");
                    var hashValue = hashProperty?.GetValue(tagInstance) ?? null;
                    Console.WriteLine($"{field.Name}: {(hashValue ?? $"NULL")}");
                }
            }
        }
#endif
    }
}


public class InventoryItem : Tag<S9D798080>
{
    public InventoryItem(FileHash hash, bool shouldParse) : base(hash, shouldParse)
    {
    }

    public int CollectibleIndex => Investment.Get().GetCollectibleIndexFromItemIndex(GetItemIndex());
    public string Source => CollectibleIndex != -1 ? Investment.Get().GetCollectibleStrings(CollectibleIndex)?.SourceString.Value : "";

    public uint ApiHash => _tag.InventoryItemHash.Hash32;

    private IReadOnlyCollection<DestinyTraitID> _traits;
    public IReadOnlyCollection<DestinyTraitID> ItemTraits => _traits ??= GetItemTraits(); // cache traits on first use

    public bool IsWeapon => ItemTraits.Any(x => x.ToString().Contains("item_weapon"));
    public bool IsArmor => ItemTraits.Any(x => x.ToString().Contains("item_armor"));

    public bool IsGhost => ItemTraits.Contains(DestinyTraitID.item_ghost);
    public bool IsShip => ItemTraits.Contains(DestinyTraitID.item_ship);
    public bool IsSparrow => ItemTraits.Contains(DestinyTraitID.item_vehicle);

    public bool IsEmblem => ItemTraits.Contains(DestinyTraitID.item_emblem) || Type == "Emblem"; // Some emblems dont have the trait?
    public bool IsShader => ItemTraits.Contains(DestinyTraitID.item_shader);
    public bool IsWeaponOrnament => ItemTraits.Contains(DestinyTraitID.item_ornament_weapon);
    public bool IsArmorOrnament => ItemTraits.Contains(DestinyTraitID.item_ornament_armor);
    public bool IsOrnament => IsWeaponOrnament || IsArmorOrnament;

    private string _name = null;
    public string Name
    {
        get
        {
            if (_name is not null)
                return _name;

            _name = GetItemName();
            return _name;
        }
        set // Only really used for Artifacts since their "real" inventory item is nameless
        {
            _name = value;
        }
    }

    private bool? _isHolofoil = null;
    public bool IsHolofoil
    {
        get
        {
            if (_isHolofoil is not null)
                return _isHolofoil.Value;

            _isHolofoil = IsItemHolofoil();
            return _isHolofoil.Value;
        }
    }

    public string Type => GetItemType();
    public string Description => GetItemDescription();
    public string FlavorText => GetItemFlavorText();

    private ConcurrentBag<InventoryItem> _ornaments = null;
    public ConcurrentBag<InventoryItem> Ornaments
    {
        get
        {
            if (_ornaments is not null)
                return _ornaments;

            _ornaments = GetItemOrnaments();
            return _ornaments;
        }
    }

    // If this item is an ornament this will be its parent item
    public InventoryItem Parent = null;

    public override void Load(bool force = false)
    {
        base.Load(force);

        // this is needed to make sure its ornaments are loaded (if it has any)
        // which in turn will set the ornaments parent item
        _ = Ornaments;
    }

    private bool IsItemHolofoil()
    {
        if (!Strategy.IsD1() && _tag.Unk78_EoF.GetValue(GetReader()) is S74738080 Unk && Unk.Unk20.Any(x => x.Unk00 == 0xF3))
            return true;

        return false;
    }

    public int GetItemIndex()
    {
        return Investment.Get().GetItemIndex(_tag.InventoryItemHash);
    }

    public List<DestinyTraitID> GetItemTraits()
    {
        List<DestinyTraitID> traits = new();
        if (Strategy.IsD1())  // D1 items dont have traits
            return MakeD1ItemTraitMap();

        foreach (var index in _tag.TraitIndices.Select(x => x.Index))
        {
            traits.Add(Investment.Get().GetTrait(index).Value.TraitHash);
        }

        // not ideal, should be done elsewhere
        if (GetItemRarity() == DestinyTierType.Exotic && (traits.Any(x => x.ToString().Contains("item_weapon")) || traits.Any(x => x.ToString().Contains("item_armor"))))
            Investment.Get().FeaturedItems.Add(GetItemIndex());

        if (Investment.Get().FeaturedItems.Contains(GetItemIndex()))
            traits.Add(DestinyTraitID.item_featured);

        // Custom assignments
        if (_tag.TraitIndices.Count == 0)
        {
            if (_tag.BucketTypeIndex == 42) // Seasonal Artifact
                traits.Add(DestinyTraitID.item_seasonal_artifact);

            if (Type.Contains("mask ornament", StringComparison.InvariantCultureIgnoreCase))
                traits.Add(DestinyTraitID.item_mask);
        }

        // inherits traits from the parent, just release for now tho
        if (Parent is not null)
            traits.AddRange(Parent.ItemTraits.Where(x => x.ToString().Contains("releases")));

        return traits;
    }

    [Obsolete("Power cap? Never heard of it.")]
    public int GetItemPowerCap()
    {
        if (_tag.Unk50.GetValue(GetReader()) is SDC778080 quality)
        {
            if (quality.Versions.Count == 0 || quality.Versions[0].PowerCapIndex == -1)
                return 0;

            return (int)Investment.Get()._powerCapDefinitionMap.TagData.PowerCapDefinitions[quality.Versions[0].PowerCapIndex].PowerCap * 10;
        }
        return 0;
    }

    public string GetItemLore()
    {
        return Investment.Get().GetItemLore(this)?.LoreDescription?.Value.ToString() ?? string.Empty;
    }

    public Tag<S9F548080> GetItemStrings()
    {
        return Investment.Get().GetItemStrings(Investment.Get().GetItemIndex(_tag.InventoryItemHash));
    }

    public string GetItemName()
    {
        var name = Investment.Get().GetItemName(this);
        if (Strategy.IsD1() && IsOrnament && name == "" && Parent != null) // ew
            name = $"{Parent.Name} Ornament {Parent.Ornaments
                .OrderBy(x => x.ApiHash)
                .ToList()
                .IndexOf(this)}";

        return name;
    }

    public string GetItemType()
    {
        return Investment.Get().GetItemType(this) ?? "";
    }

    public string GetItemDescription()
    {
        return GetItemStrings().TagData.ItemDescription?.Value.ToString() ?? "";
    }

    public DestinyTierType GetItemRarity()
    {
        return (DestinyTierType)_tag.ItemRarity;
    }

    public string GetItemFlavorText()
    {
        return GetItemStrings().TagData.ItemFlavourText.Value.ToString();
    }

    public int GetItemDamageTypeIndex()
    {
        var index = -1;
        if (_tag.Unk78.GetValue(GetReader()) is S81738080 perks)
        {
            foreach (S87738080 perk in perks.Perks)
            {
                if (Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex != -1)
                {
                    index = Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex;
                    break;
                }
            }
        }

        // if the damage type wasnt found in perks
        if (index == -1 && _tag.Unk70.GetValue(GetReader()) is SC0778080 sockets)
        {
            sockets.SocketEntries.ForEach(entry =>
            {
                if (entry.SocketTypeIndex == -1 || entry.SingleInitialItemIndex == -1)
                    return;

                SBA768080 socket = Investment.Get().GetSocketType(entry.SocketTypeIndex);
                foreach (SC5768080 a in socket.PlugWhitelists)
                {
                    if (a.PlugCategoryHash.Hash32 == 1466776700) // 'v300.weapon.damage_type.energy', Y1 weapon that uses a damage type mod from ye olden days
                    {
                        InventoryItem item = Investment.Get().GetInventoryItem(entry.SingleInitialItemIndex);
                        item.Load(true); // idk why the item sometimes isnt fully loaded
                        index = item.GetItemDamageTypeIndex();
                        break;
                    }
                }
            });
        }
        return index;
    }

    public int GetArtArrangementIndex()
    {
        if (_tag.Unk90 is null) return -1;
        if (_tag.Unk90.GetValue(GetReader()) is S77738080 entry && entry.Arrangements.Count > 0)
            return entry.Arrangements[GetReader(), 0].ArtArrangementHash;

        return -1;
    }

    public int GetWeaponPatternIndex()
    {
        if (_tag.Unk90.GetValue(GetReader()) is S77738080 entry && entry.WeaponPatternIndex > 0)
            return entry.WeaponPatternIndex;

        return -1;
    }

    public ConcurrentBag<InventoryItem> GetItemOrnaments()
    {
        ConcurrentBag<InventoryItem> ornaments = new();
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && _tag.Unk70.GetValue(GetReader()) is SC0778080 sockets)
        {
            foreach (SC3778080 socket in sockets.SocketEntries)
            {
                if (socket.SocketTypeIndex == -1)
                    continue;

                SBA768080 socketType = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                if (!socketType.PlugWhitelists.Any(x => // hopefully this is better than just checking the sockets name
                   x.PlugCategoryHash.Hash32 != 3940152116 // exotic_all_skins
                || x.PlugCategoryHash.Hash32 != 3356843615)) // armor_skins_empty
                    continue;

                if (socket.ReusablePlugSetIndex1 != -1) // huh?
                {
                    foreach (SD5778080 randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
                    {
                        if (randomPlugs.PlugInventoryItemIndex == -1)
                            continue;

                        var item = Investment.Get().GetInventoryItem(randomPlugs.PlugInventoryItemIndex);
                        // hacky and stupid
                        if (item.Type.Contains("universal ornament", StringComparison.InvariantCultureIgnoreCase) || item.Type == "Armor Ornament")
                            continue;

                        if (item.IsOrnament && !ornaments.Contains(item))
                        {
                            item.Parent = this;
                            ornaments.Add(item);
                        }
                    }
                }

                foreach (SD5778080 plug in socket.PlugItems)
                {
                    if (plug.PlugInventoryItemIndex == -1)
                        continue;

                    var item = Investment.Get().GetInventoryItem(plug.PlugInventoryItemIndex);
                    // hacky and stupid
                    if (item.Type.Contains("universal ornament", StringComparison.InvariantCultureIgnoreCase) || item.Type == "Armor Ornament")
                        continue;

                    if (item.IsOrnament && !ornaments.Contains(item))
                    {
                        item.Parent = this;
                        ornaments.Add(item);
                    }
                }
            }
        }
        else if (Strategy.IsD1() && _tag.Unk78.GetValue(GetReader()) is SBD178080 a)
        {
            Tag<S63198080> talentGrid = Investment.Get().GetTalentGrid(a.TalenGridIndex);
            foreach (S28178080 node in talentGrid.TagData.Nodes)
            {
                foreach (S58178080 entry in node.Unk18)
                {
                    foreach (S940F8080 entry2 in entry.Unk70)
                    {
                        if (entry2.PlugItemIndex == -1)
                            continue;

                        var item = Investment.Get().GetInventoryItem(entry2.PlugItemIndex);
                        item.Parent = this;
                        ornaments.Add(item);
                    }
                }
            }
        }
        return ornaments;
    }

    #region Icon Background
    public UnmanagedMemoryStream? GetIconBackgroundStream()
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBackgroundContainer == null)
            return null;
        Texture? backgroundIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconBackgroundContainer);
        return backgroundIcon.GetTexture();
    }

    public UnmanagedMemoryStream? GetIconBackgroundOverlayStream()
    {
        Texture? backgroundIcon = GetIconBackgroundOverlayTexture();
        return backgroundIcon?.GetTexture();
    }

    public Texture? GetIconBackgroundOverlayTexture()
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconBGOverlayContainer == null)
            return null;
        Texture? backgroundIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconBGOverlayContainer);
        return backgroundIcon;
    }
    #endregion

    #region Icon Foreground
    public UnmanagedMemoryStream? GetIconPrimaryStream()
    {
        Texture? primaryIcon = GetIconPrimaryTexture();
        return primaryIcon?.GetTexture();
    }

    public Texture? GetIconPrimaryTexture()
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        Texture? primaryIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon;
    }

    public Texture? GetIconPrimaryTexture(int index, int listIndex = 0)
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        Texture? primaryIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconPrimaryContainer, index, listIndex);
        return primaryIcon;
    }
    #endregion

    #region Icon Overlay
    public UnmanagedMemoryStream? GetIconOverlayStream(int index = 0)
    {
        Texture? overlayIcon = GetIconOverlayTexture(index);
        return overlayIcon?.GetTexture();
    }

    public Texture? GetIconOverlayTexture(int index = 0)
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconOverlayContainer == null)
            return null;
        Texture? overlayIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconOverlayContainer, index);
        return overlayIcon;
    }
    #endregion

    public UnmanagedMemoryStream? GetFoundryIconStream()
    {
        Texture? foundryIcon = GetFoundryIconTexture();
        return foundryIcon?.GetTexture();
    }

    public Texture? GetFoundryIconTexture()
    {
        Tag<SB83E8080>? iconContainer = Investment.Get().GetFoundryItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        Texture? foundryIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconPrimaryContainer);
        return foundryIcon;
    }

    // I hate this and theres probably a better way but I'm lazy
    public List<DestinyTraitID> MakeD1ItemTraitMap()
    {
        List<DestinyTraitID> traits = new();
        switch (Type.ToLower().Trim())
        {
            case "emote":
                traits.Add(DestinyTraitID.item_emote);
                break;
            case "ghost shell":
                traits.Add(DestinyTraitID.item_ghost);
                break;
            case "ship":
                traits.Add(DestinyTraitID.item_ship);
                break;
            case "sparrow":
                traits.Add(DestinyTraitID.item_vehicle);
                break;
            case "emblem":
                traits.Add(DestinyTraitID.item_emblem);
                break;
            case "armor shader":
                traits.Add(DestinyTraitID.item_shader);
                break;
            case "pulse rifle":
                traits.Add(DestinyTraitID.item_weapon_pulse_rifle);
                break;
            case "hand cannon":
                traits.Add(DestinyTraitID.item_weapon_hand_cannon);
                break;
            case "auto rifle":
                traits.Add(DestinyTraitID.item_weapon_auto_rifle);
                break;
            case "scout rifle":
                traits.Add(DestinyTraitID.item_weapon_scout_rifle);
                break;
            case "fusion rifle":
                traits.Add(DestinyTraitID.item_weapon_fusion_rifle);
                break;
            case "shotgun":
                traits.Add(DestinyTraitID.item_weapon_shotgun);
                break;
            case "sniper rifle":
                traits.Add(DestinyTraitID.item_weapon_sniper_rifle);
                break;
            case "rocket launcher":
                traits.Add(DestinyTraitID.item_weapon_rocket_launcher);
                break;
            case "machine gun":
                traits.Add(DestinyTraitID.item_weapon_machinegun);
                break;
            case "sidearm":
                traits.Add(DestinyTraitID.item_weapon_sidearm);
                break;
            case "sword":
                traits.Add(DestinyTraitID.item_weapon_sword);
                break;
            case "armor ornament":
                traits.Add(DestinyTraitID.item_ornament_armor);
                break;
            case "weapon ornament":
                traits.Add(DestinyTraitID.item_ornament_weapon);
                break;
            case "helmet":
                traits.Add(DestinyTraitID.item_armor_head);
                break;
            case "gauntlets":
                traits.Add(DestinyTraitID.item_armor_arms);
                break;
            case "chest armor":
                traits.Add(DestinyTraitID.item_armor_chest);
                break;
            case "leg armor":
                traits.Add(DestinyTraitID.item_armor_arms);
                break;
            case "hunter cloak":
            case "titan mark":
            case "warlock bond":
                traits.Add(DestinyTraitID.item_armor_class);
                break;
            default:
                traits.Add(DestinyTraitID.item_other);
                break;
        }
        return traits;
    }
}
