using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using Arithmic;
using ConcurrentCollections;
using Tiger.Exporters;
using Tiger.Schema.Entity;
using Tiger.Schema.Shaders;
using Tiger.Schema.Strings;

using Ent = Tiger.Schema.Entity.Entity;

namespace Tiger.Schema.Investment;

/// <summary>
/// Keeps track of the investment tags.
/// Finds them on launch from their tag class instead of hash.
/// </summary>
[InitializeAfter(typeof(Hash64Map))]
public class Investment : Strategy.LazyStrategistSingleton<Investment>
{
    private Tag<S80807997> _inventoryItemMap = null;
    private Tag<S808070F2> _artArrangementMap = null;
    private Tag<S808055CE> _entityAssignmentTag = null;
    private Tag<S80804F43> _entityAssignmentsMap = null;
    private Tag<S80805499> _inventoryItemStringThing = null;
    private Tag<S8080978C> _sandboxPatternAssignmentsTag = null;
    private Tag<S808052AA> _sandboxPatternGlobalTagIdTag = null;
    private Tag<S80805A09> _localizedStringsIndexTag = null;
    private Tag<S8080BA26> _localizedStringsIndexTag2 = null;
    private Tag<S80805A01> _inventoryItemIconTag = null;
    private Tag<S808055C2> _artDyeReferenceTag = null;
    private Tag<SDyeChannels> _dyeChannelTag = null;

    private Tag<S808018C2> _talentGridMap = null;
    private Tag<S808077CD> _randomizedPlugSetMap = null;
    private Tag<S808076B6> _socketTypeMap = null;
    private Tag<S80804F59> _socketCategoryMap = null;
    private Tag<S808050CF> _loreStringMap = null;
    private Tag<S8080542D> _sandboxPerkMap = null;
    private Tag<S808076AA> _sandboxPerkMap2 = null;
    private Tag<S8080586B> _statDefinitionMap = null;
    private Tag<S808054BE> _statGroupDefinitionMap = null;
    private Tag<S80807828> _collectableDefinitionMap = null;
    private Tag<S808059BF> _collectableStringsMap = null;
    private Tag<S8080753C> _objectiveDefinitionMap = null;
    private Tag<S8080584C> _objectiveStringsMap = null;
    public Tag<S808079C9> _powerCapDefinitionMap = null; // Literally 0 reason for this but fuck it we ball
    public Tag<S808078D7> _presentationNodeDefinitionMap = null;
    public Tag<S80805803> _presentationNodeDefinitionStringMap = null;
    public Tag<S8080711F> _recordNodeDefinitionMap = null;
    public Tag<S80805887> _recordNodeDefinitionStringMap = null;
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
    public ConcurrentDictionary<int, S8080782C> Collectables = null;
    public ConcurrentDictionary<int, S808059C3> CollectableStrings = null;

    private ConcurrentDictionary<uint, int> _inventoryItemHashIndexMap = null;
    private ConcurrentDictionary<int, InventoryItem> _inventoryItemIndexMap = null;
    private ConcurrentDictionary<uint, InventoryItem> _inventoryItems = null;
    private ConcurrentDictionary<int, S80805A07> _inventoryItemIconMap = null;

    private Dictionary<uint, Tag<S80806FA3>> _sortedArrangementHashmap = null;
    private Dictionary<int, LocalizedStrings> _localizedStringsIndexMap = null;
    public ConcurrentDictionary<int, Tag<S8080549F>> InventoryItemStringThings = null;

    public ConcurrentDictionary<int, S80804F5D> SocketCategoryStrings = null;
    public ConcurrentDictionary<int, S808050D3> InventoryItemLoreStrings = null;
    public ConcurrentDictionary<int, S80805433> SandboxPerkStrings = null;
    public ConcurrentDictionary<int, S8080586F> StatStrings = null;

    public ConcurrentDictionary<int, S808076AE> SandboxPerkMap2 = null;

    public ConcurrentDictionary<int, S80807540> Objectives = null;
    public ConcurrentDictionary<int, S80805850> ObjectiveStrings = null;

    public ConcurrentDictionary<int, S808077D3> RandomizedPlugSetMap = null;
    public ConcurrentDictionary<int, S808076BA> SocketTypeMap = null;

    public ConcurrentDictionary<int, S8080B454> EquipableItemSets = null;
    public ConcurrentDictionary<int, S8080B27A> EquipableItemSetStrings = null;

    public ConcurrentDictionary<int, S808057FA> TraitIndexMap = null;
    public ConcurrentDictionary<DestinyTraitID, S808057FA> TraitMap = null;

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
        if (Strategy.IsLatest() || Strategy.IsD1())
            GetAllInvestmentTags();
        else
            Log.Info("Investment is only supported on the latest verison of D2 or D1.");
    }

    private void GetAllInvestmentTags()
    {
        ConcurrentHashSet<FileHash> allHashes = new();
        // Iterate over all investment pkgs until we find all the tags we need
        if (Strategy.IsLatest())
        {
            bool PackageFilterFunc(string packagePath) => packagePath.Contains("investment") || packagePath.Contains("client_startup");
            allHashes = PackageResourcer.Get().GetAllHashes(PackageFilterFunc);
            Parallel.ForEach(allHashes, (val, state, i) =>
            {
                // Dumb but string index tags must be set first since StringIndexReference depends on it
                switch (val.GetReferenceHash().Hash32)
                {
                    case 0x80805a09:
                        _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S80805A09>(val);
                        break;
                    case 0x8080BA26:
                        _localizedStringsIndexTag2 = FileResourcer.Get().GetSchemaTag<S8080BA26>(val);
                        break;
                }
            });
            GetLocalizedStringsIndexDict(); // must be done before anything else that uses strings

            Parallel.ForEach(allHashes, (val, state, i) =>
            {
                switch (val.GetReferenceHash().Hash32)
                {
                    case 0x80807997:
                        _inventoryItemMap = FileResourcer.Get().GetSchemaTag<S80807997>(val);
                        break;
                    case 0x808070f2:
                        _artArrangementMap = FileResourcer.Get().GetSchemaTag<S808070F2>(val);
                        break;
                    case 0x808055ce:
                        _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<S808055CE>(val);
                        break;
                    case 0x80805499:
                        _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<S80805499>(val);
                        break;
                    case 0x80804ea4: // points to parent of the sandbox pattern ref list thing + entity assignment map
                        Tag<S80804EA4> parent = FileResourcer.Get().GetSchemaTag<S80804EA4>(val);
                        _sandboxPatternAssignmentsTag = parent.TagData.SandboxPatternAssignmentsTag; // also art dye refs
                        _entityAssignmentsMap = parent.TagData.EntityAssignmentsMap;
                        break;
                    case 0x808052aa: // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
                        _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<S808052AA>(val);
                        break;
                    case 0x80805a01:
                        _inventoryItemIconTag = FileResourcer.Get().GetSchemaTag<S80805A01>(val);
                        break;
                    case 0x808055c2:
                        _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<S808055C2>(val);
                        break;
                    case 0x808051f2:
                        _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(val);
                        break;


                    case 0x808077CD:
                        _randomizedPlugSetMap = FileResourcer.Get().GetSchemaTag<S808077CD>(val);
                        break;
                    case 0x808076B6:
                        _socketTypeMap = FileResourcer.Get().GetSchemaTag<S808076B6>(val);
                        break;
                    case 0x80804F59:
                        _socketCategoryMap = FileResourcer.Get().GetSchemaTag<S80804F59>(val);
                        break;
                    case 0x808050CF:
                        _loreStringMap = FileResourcer.Get().GetSchemaTag<S808050CF>(val);
                        break;
                    case 0x8080542D:
                        _sandboxPerkMap = FileResourcer.Get().GetSchemaTag<S8080542D>(val);
                        break;
                    case 0x808076AA:
                        _sandboxPerkMap2 = FileResourcer.Get().GetSchemaTag<S808076AA>(val);
                        break;
                    case 0x8080586B:
                        _statDefinitionMap = FileResourcer.Get().GetSchemaTag<S8080586B>(val);
                        break;
                    case 0x808054BE:
                        _statGroupDefinitionMap = FileResourcer.Get().GetSchemaTag<S808054BE>(val);
                        break;
                    case 0x80807828:
                        _collectableDefinitionMap = FileResourcer.Get().GetSchemaTag<S80807828>(val);
                        break;
                    case 0x808059BF:
                        _collectableStringsMap = FileResourcer.Get().GetSchemaTag<S808059BF>(val);
                        break;
                    case 0x8080753C:
                        _objectiveDefinitionMap = FileResourcer.Get().GetSchemaTag<S8080753C>(val);
                        break;
                    case 0x8080584C:
                        _objectiveStringsMap = FileResourcer.Get().GetSchemaTag<S8080584C>(val);
                        break;
                    case 0x808079C9:
                        _powerCapDefinitionMap = FileResourcer.Get().GetSchemaTag<S808079C9>(val);
                        break;
                    case 0x808078D7:
                        _presentationNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<S808078D7>(val);
                        break;
                    case 0x80805803:
                        _presentationNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S80805803>(val);
                        break;
                    case 0x8080711F: //1F718080
                        _recordNodeDefinitionMap = FileResourcer.Get().GetSchemaTag<S8080711F>(val);
                        break;
                    case 0x80805887: //87588080
                        _recordNodeDefinitionStringMap = FileResourcer.Get().GetSchemaTag<S80805887>(val);
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
        else // No need to loop hashes when D1 will never change
        {
            _localizedStringsIndexTag = FileResourcer.Get().GetSchemaTag<S80805A09>(new FileHash(0x80A5E21A));
            GetLocalizedStringsIndexDict();

            _inventoryItemMap = FileResourcer.Get().GetSchemaTag<S80807997>(new FileHash(0x80A5FFBE));
            _entityAssignmentTag = FileResourcer.Get().GetSchemaTag<S808055CE>(new FileHash(0x80A5FFA7));
            _inventoryItemStringThing = FileResourcer.Get().GetSchemaTag<S80805499>(new FileHash(0x80A5FF9C));
            _sandboxPatternAssignmentsTag = FileResourcer.Get().GetSchemaTag<S8080978C>(new FileHash(0x80A7E1DC)); // also art dye refs
            _entityAssignmentsMap = FileResourcer.Get().GetSchemaTag<S80804F43>(new FileHash(0x80A7E1DD));

            // inventory item -> sandbox pattern index -> pattern global tag id -> entity assignment
            _sandboxPatternGlobalTagIdTag = FileResourcer.Get().GetSchemaTag<S808052AA>(new FileHash(0x80A7E1DD));

            _artDyeReferenceTag = FileResourcer.Get().GetSchemaTag<S808055C2>(new FileHash(0x80A5FFA8));
            _dyeChannelTag = FileResourcer.Get().GetSchemaTag<SDyeChannels>(new FileHash(0x80A5E249));

            _talentGridMap = FileResourcer.Get().GetSchemaTag<S808018C2>(new FileHash(0x80A5E227));
        }


        Task.WaitAll(new[]
        {
            Task.Run(DebugPrintTags),
            Task.Run(GetInventoryItemDict),
            Task.Run(GetEntityAssignmentDict),
            Task.Run(GetInventoryItemStringThings),
            Task.Run(GetItemIconMap),
            Task.Run(GetSocketCategoryStrings),
            Task.Run(GetInventoryItemLoreStrings), // Biggest load time offender at ~550ms
            Task.Run(GetSandboxPerkStrings),
            Task.Run(GetStatStrings),
            Task.Run(GetCollectableIndexDict),
            Task.Run(GetCollectables),
            Task.Run(GetCollectableStrings), // ~275ms
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
            S8080799B entry = _inventoryItemMap.TagData.InventoryItemDefinitionEntries[reader, i];
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
        Tag<S8080549F>? entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemName.Value.ToString();
    }

    public string GetItemType(InventoryItem item)
    {
        Tag<S8080549F>? entry = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash));
        return entry.TagData.ItemType.Value.ToString();
    }

    public S808050D3? GetItemLore(InventoryItem item)
    {
        if (item.TagData.Unk30.GetValue(item.GetReader()) is S808073B6)
            return GetItemLore(((S808073B6)item.TagData.Unk30.GetValue(item.GetReader())).LoreEntryIndex);
        else
            return null;
    }

    public S808050D3? GetItemLore(int index)
    {
        if (index == -1)
            return null;
        return InventoryItemLoreStrings[index];
    }

    public Tag<S8080549F>? GetItemStrings(TigerHash hash)
    {
        Tag<S8080549F> entry = GetItemStrings(GetItemIndex(hash));
        return entry;
    }

    public Tag<S8080549F>? GetItemStrings(int index)
    {
        Tag<S8080549F> entry = InventoryItemStringThings[index];
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
        InventoryItemStringThings = new ConcurrentDictionary<int, Tag<S8080549F>>();
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

    public S808057FA? GetTrait(int index)
    {
        if (!TraitIndexMap.ContainsKey(index))
            return null;

        return TraitIndexMap[index];
    }

    public S808057FA? GetTrait(DestinyTraitID traitID)
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
        var index = GetItemStrings(GetItemIndex(item.TagData.InventoryItemHash)).TagData.IconIndex;
        return index;
    }

    public Tag<S80803EB8>? GetItemIconContainer(InventoryItem item)
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

    public Tag<S80803EB8>? GetItemIconContainer(int index)
    {
        if (index == -1)
            return null;

        var container = _inventoryItemIconMap[index].IconContainer;
        if (container is null)
            return null;

        container.Load();
        return container;
    }

    public Tag<S80803EB8>? GetFoundryItemIconContainer(InventoryItem item)
    {
        //int iconIndex = Strategy.IsLatest() ? GetItemStrings(GetItemIndex(item.ApiHash)).TagData.EmblemContainerIndex : GetItemStrings(GetItemIndex(item.ApiHash)).TagData.FoundryIconIndex;
        int iconIndex = GetItemStrings(GetItemIndex(item.ApiHash)).TagData.EmblemContainerIndex;
        if (iconIndex == -1)
            return null;

        return GetItemIconContainer(iconIndex);
    }

    public Texture? GetTextureFromContainer(Tag<S80803ECF> iconContainer, int index = 0, int listIndex = 0)
    {
        using TigerReader reader = iconContainer.GetReader();
        dynamic? prim = iconContainer.TagData.Unk10.GetValue(reader);
        if (prim is S80803ECD structCD3E8080)
        {
            // TextureList[0] is default, others are for colourblind modes
            if (index >= structCD3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCD3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        if (prim is S80803ECB structCB3E8080)
        {
            if (index >= structCB3E8080.Unk00[reader, listIndex].TextureList.Count)
                return null;

            return structCB3E8080.Unk00[reader, listIndex].TextureList[reader, index].IconTexture;
        }
        return null;
    }


    public Texture? GetTextureFromContainer(FileHash containerHash, int index, int listIndex = 0)
    {
        return GetTextureFromContainer(FileResourcer.Get().GetSchemaTag<S80803ECF>(containerHash), index, listIndex);
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

        SocketCategoryStrings = new ConcurrentDictionary<int, S80804F5D>();
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

    public S808076BA GetSocketType(int index)
    {
        return SocketTypeMap[index];
    }

    public int GetSocketCategoryIndex(int index)
    {
        return SocketTypeMap[index].SocketCategoryIndex;
    }

    private int GetStatGroupIndex(InventoryItem item)
    {
        Tag<S8080549F>? stringThing = GetItemStrings(item.TagData.InventoryItemHash);

        if (stringThing.TagData.Unk78.GetValue(stringThing.GetReader()) is S808054CA details)
            return details.StatGroupIndex;

        return -1;
    }

    public S808054C4? GetStatGroup(InventoryItem item)
    {
        int index = GetStatGroupIndex(item);
        if (index == -1 || index > _statGroupDefinitionMap.TagData.StatGroupDefinitions.Count)
            return null;

        return _statGroupDefinitionMap.TagData.StatGroupDefinitions.ElementAt(_statGroupDefinitionMap.GetReader(), index);
    }

    public Tag<S80801963> GetTalentGrid(int index)
    {
        return _talentGridMap.TagData.TalentGridEntries.ElementAt(_talentGridMap.GetReader(), index).TalentGrid;
    }

    public DynamicArray<S808077D5> GetRandomizedPlugSet(int index)
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
            int itemIndex = _collectableDefinitionMap.TagData.CollectibleDefinitionEntries[reader, i].InventoryItemIndex;
            _collectableIndexMap.TryAdd(itemIndex, i);
        }
    }

    public S8080782C? GetCollectible(int index)
    {
        if (index == -1 || index > Collectables.Count)
            return null;

        return Collectables[index];
    }

    public S808059C3? GetCollectibleStrings(int index)
    {
        if (index == -1 || index > CollectableStrings.Count || Strategy.IsD1())
            return null;

        return CollectableStrings[index];
    }

    public S808059C3? GetCollectibleStringsFromItemIndex(int itemIndex)
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

    public S80805850? GetObjective(int index)
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
        _sortedArrangementHashmap = new Dictionary<uint, Tag<S80806FA3>>(_entityAssignmentsMap.TagData.EntityArrangementMap.Count);
        foreach (S80804F45 e in _entityAssignmentsMap.TagData.EntityArrangementMap.Enumerate(_entityAssignmentsMap.GetReader()))
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
        Optional<S8080870F> patternData = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), patternGlobalId);
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
        return _artArrangementMap.TagData.ArtArrangementHashes.ElementAt(_artArrangementMap.GetReader(), item.ArtArrangementIndex).ArtArrangementHash;
    }

    public List<Entity.Entity> GetEntitiesFromHash(InventoryItem item)
    {
        List<Entity.Entity> entities = new();
        int index = item.ArtArrangementIndex;
        if (index == -1)
        {
            Log.Warning($"Item {item.Name} ({item.ApiHash}) has no art arrangement index.");
            return entities;
        }

        if (item.GetArtArrangementCount() > 1)
        {
            Log.Info($"Item has multiple Art Arrangements, exporting all.");
            foreach (var i in item.GetArtArrangementIndices())
            {
                entities.AddRange(GetEntitiesFromArrangementIndex(i));
            }
        }
        else
            entities = GetEntitiesFromArrangementIndex(index);

        return entities;
    }

    private List<Entity.Entity> GetEntitiesFromArrangementIndex(int index)
    {
        List<Entity.Entity> entities = new();
        S808055D4 entry = _entityAssignmentTag.TagData.ArtArrangementEntityAssignments.ElementAt(_entityAssignmentTag.GetReader(), index);
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
            foreach (S808055D7 entryMultipleEntityAssignment in entry.MultipleEntityAssignments)
            {
                foreach (S808055DA assignment in entryMultipleEntityAssignment.EntityAssignmentResource.Value.Value.EntityAssignments)
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
        // var index = _entityAssignmentsMap.TagData.EntityArrangementMap.BinarySearch(x, new S80804F45());
        if (!_sortedArrangementHashmap.ContainsKey(assignmentHash))
            return null;

        Tag<S80806FA3> tag = _sortedArrangementHashmap[assignmentHash];
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
    public TigerHash GetChannelHashFromIndex(int index)
    {
        return _dyeChannelTag.TagData.ChannelHashes[_dyeChannelTag.GetReader(), index].ChannelHash;
    }

    public Dye? GetDyeFromIndex(int index)
    {
        S808055C6 artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);

        Optional<S8080870F> dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);
        if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceHash() == 0x80806fa3)
        {
            if (dyeEntry.Value.EntityRelationHash is null || dyeEntry.Value.EntityRelationHash.IsInvalid())
                return null;

            var dyeHash = FileResourcer.Get().GetSchemaTag<S80806FA3>(dyeEntry.Value.EntityRelationHash).TagData.EntityData;
            if (dyeHash is null || dyeHash.IsInvalid())
                return null;

            return FileResourcer.Get().GetSchemaTag<S80806CE3>(dyeHash).TagData.Dye;
        }
        return null;
    }

    public DyeD1 GetD1DyeFromIndex(int index)
    {
        S808055C6 artEntry = _artDyeReferenceTag.TagData.ArtDyeReferences.ElementAt(_artDyeReferenceTag.GetReader(), index);
        Optional<S8080870F> dyeEntry = _sandboxPatternAssignmentsTag.TagData.AssignmentBSL.BinarySearch(_sandboxPatternAssignmentsTag.GetReader(), artEntry.DyeManifestHash);

        if (dyeEntry.HasValue && dyeEntry.Value.EntityRelationHash.GetReferenceFromManifest() == 0x80803463)
        {
            return FileResourcer.Get().GetFile<DyeD1>(FileResourcer.Get().GetSchemaTag<S80806FA3>(dyeEntry.Value.EntityRelationHash).TagData.EntityData);
        }
        return null;
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

    #region Exporting
    public void ExportInventoryItem(InventoryItem item, string savePath, bool aggregateOutput = false)
    {
        // just to be safe, hopefully this doesn't cause issues
        if (item.IsOrnament && item.Parent is null)
            item.Parent = Investment.Get().GetOrnamentParent(item).Result;

        ConfigSubsystem config = ConfigSubsystem.Get();
        string name = item.Name != string.Empty ? Helpers.SanitizeString(item.Name) : $"{item.ApiHash}";
        if (!aggregateOutput)
            savePath = config.GetExportSavePath() + $"/{name}";

        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory($"{savePath}/Textures");
        ExporterScene scene = Tiger.Exporters.Exporter.Get().CreateScene(name, Strategy.IsD1() ? ExportType.D1API : ExportType.API);

        // Dont export gear shader for ghost projections since they dont use it
        if (!item.ItemTraits.Any(x => x == DestinyTraitID.item_ghost_hologram))
            ExportGearShader(item, name, savePath);

        // Export the model
        // todo bad, should be replaced
        EntitySkeleton overrideSkeleton = null;
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307)
        {
            var skeleHash = item.IsGhost ? 0x681CD34630600000 : 0x95952E340F670000;
            Ent skele = FileResourcer.Get().GetFile<Ent>(new FileHash(Hash64Map.Get().GetHash32Checked(skeleHash))); // 64 bit more permanent
            overrideSkeleton = new EntitySkeleton(skele.Skeleton.Hash);
        }
        else if (Strategy.IsD1())
        {
            Ent playerBase = FileResourcer.Get().GetFile<Ent>(new FileHash(0x8184E10A));
            overrideSkeleton = new EntitySkeleton(playerBase.Skeleton.Hash);
        }

        Ent? val = Investment.Get().GetPatternEntityFromHash(item.Parent != null ? item.Parent.TagData.InventoryItemHash : item.TagData.InventoryItemHash);

        Log.Debug($"Pattern Entity {val?.Hash}");

        if (val != null && val.Skeleton != null)
        {
            overrideSkeleton = val.Skeleton;
        }

        List<Ent> entities = Investment.Get().GetEntitiesFromHash(item);

        Log.Info($"Exporting entity model name: {name}");

        foreach (Ent entity in entities)
        {
            if (entity.Hash.CheckRedacted())
            {
                Log.Warning($"Entity {entity.Hash} is redacted, can not export.");
                continue;
            }
            Log.Debug($"Entity {entity?.Hash}: HasGeometry {entity?.HasGeometry()}");

            // ghost projections have just a rectangle mesh, we want just the actual projection mesh 
            if (item.ItemTraits.Any(x => x == DestinyTraitID.item_ghost_hologram))
            {
                ExportGhostProjection(entity, scene);
                continue;
            }

            if (entity.Skeleton == null && overrideSkeleton != null)
                entity.Skeleton = overrideSkeleton;

            List<DynamicMeshPart> dynamicParts = entity.Load(ExportDetailLevel.MostDetailed);
            List<BoneNode> boneNodes = overrideSkeleton != null ? overrideSkeleton.GetBoneNodes() : new List<BoneNode>();
            if (entity.Skeleton != null && overrideSkeleton == null)
            {
                boneNodes = entity.Skeleton.GetBoneNodes();
            }
            scene.AddEntity(entity, dynamicParts, boneNodes);
            entity.SaveMaterialsFromParts(scene, dynamicParts);
            entity.SaveTexturePlates(savePath);
        }

        if (!aggregateOutput)
            Tiger.Exporters.Exporter.Get().Export();
        else
            Tiger.Exporters.Exporter.Get().Export(savePath);

        Log.Info($"Exported entity model {name} to {savePath.Replace('\\', '/')}/");
    }

    // todo, make more generic for entities
    public static void ExportGhostProjection(Ent entity, ExporterScene scene)
    {
        foreach (FileHash hash in entity.Components)
        {
            if (Strategy.IsD1() && hash.GetReferenceHash() != 0x80800861)
                continue;

            EntityComponent resource = FileResourcer.Get().GetFile<EntityComponent>(hash);
            if (resource.TagData.Unk18.GetValue(resource.GetReader()) is S80808179 sequencer)
            {
                // only in Array2 afaik
                foreach (S808091F1 element in sequencer.Array1)
                {
                    Debug.Assert(element.Unk10.GetValue(resource.GetReader()) is not SSequenceParticleSystem);
                }

                List<Tag<SParticleSystem>> particles = new();
                foreach (S808091F1 element in sequencer.Array2)
                {
                    if (element.Unk10.GetValue(resource.GetReader()) is SSequenceParticleSystem particle)
                    {
                        foreach (var entry in particle.Unk28.Select(x => x.ParticleSystem).Where(x => x is not null))
                        {
                            if (entry.TagData.ModelContainer is null)
                                continue;

                            particles.Add(entry);
                        }
                    }
                }

                if (!particles.Any())
                    return;

                // I *think* the last entry is the one used in the inspection screen? All the others have slightly different pixel shaders
                var last = particles.Where(x => x.TagData.ModelContainer is not null).Last();
                var container = last.TagData.ModelContainer;

                Material overrideMat = null;
                if (last.TagData.UnkMat is not null)
                {
                    overrideMat = last.TagData.UnkMat;
                    scene.Materials.Add(new ExportMaterial(overrideMat));
                }

                // Unsure if theres only ever 1 model here
                foreach (var model in container.TagData.Models.Enumerate(container.GetReader()).Where(x => x.Model is not null))
                {
                    if (scene.Entities.Any(x => x.Mesh.Hash == model.Model.Hash))
                        continue;

                    scene.AddModel(model.Model, overrideMat);
                }
            }
        }
    }

    // I don't like this
    public void ExportGearShader(InventoryItem item, string itemName, string savePath)
    {
        var config = ConfigSubsystem.Get();

        Log.Info($"Exporting Gear Shader for: {item.Name}");
        // Export the dye info
        if (Strategy.IsD1())
        {
            Dictionary<TigerHash, DyeD1> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.DefaultDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
                foreach (S8080737B dyeEntry in translationBlock.LockedDyes)
                {
                    DyeD1 dye = Investment.Get().GetD1DyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye != null)
                    {
                        dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
                        dye.ExportTextures($"{savePath}/Textures", config.GetOutputTextureFormat());
                    }
                }
            }
            AutomatedExporter.SaveD1ShaderInfo(savePath, itemName, config.GetOutputTextureFormat(), dyes.Values.ToList());
        }
        else
        {
            Dictionary<TigerHash, Dye> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.DefaultDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye is null)
                        continue;
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
#if DEBUG
                    string dyeChannel = Dye.GetChannelName(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()));
                    System.Console.WriteLine($"{item.Name}: DefaultDye {dye.Hash} - {dyeChannel}");
#endif
                }
                foreach (S8080737B dyeEntry in translationBlock.LockedDyes)
                {
                    Dye dye = Investment.Get().GetDyeFromIndex(dyeEntry.GetDyeIndex());
                    if (dye is null)
                        continue;
                    dyes.Add(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()), dye);
#if DEBUG
                    string dyeChannel = Dye.GetChannelName(Investment.Get().GetChannelHashFromIndex(dyeEntry.GetChannelIndex()));
                    System.Console.WriteLine($"{item.Name}: LockedDye {dye.Hash} - {dyeChannel}");
#endif
                }
            }

            AutomatedExporter.SaveBlenderApiFile(savePath, itemName,
                config.GetOutputTextureFormat(), dyes.Values.ToList());

            Texture iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExporter.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
        Log.Info($"Exported Gear Shader for: {item.Name}");
    }

    public void ExportShader(InventoryItem item, string savePath, string name, TextureExportFormat outputTextureFormat)
    {
        Directory.CreateDirectory(savePath);
        Directory.CreateDirectory(Path.Combine(savePath, "Textures"));
        if (Strategy.IsD1())
        {
            Dictionary<string, DyeD1> dyes = new();
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.CustomDyes)
                {
                    DyeD1 dye = GetD1DyeFromIndex(dyeEntry.GetDyeIndex());
                    dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                    dyes.Add(DyeD1.GetChannelName(GetChannelHashFromIndex(dyeEntry.GetChannelIndex())), dye);
                }
            }
            if (!dyes.Any())
                return;

            // appliable shaders in D1 only supported armor
            AutomatedExporter.SaveD1ShaderInfo(savePath, name, outputTextureFormat, new List<DyeD1> { dyes["ArmorPlate"], dyes["ArmorSuit"], dyes["ArmorCloth"] }, "_armor"); // imagine spelling armor with a 'u' (laughs in freedom units)
        }
        else
        {
            Dictionary<string, Dye> dyes = new();
            // export all the customDyes
            if (item.TagData.Unk90.GetValue(item.GetReader()) is S80807377 translationBlock)
            {
                foreach (S8080737B dyeEntry in translationBlock.CustomDyes)
                {
                    Dye dye = GetDyeFromIndex(dyeEntry.GetDyeIndex());
                    dye.ExportTextures(savePath + "/Textures", outputTextureFormat);
                    dyes.Add(Dye.GetChannelName(GetChannelHashFromIndex(dyeEntry.GetChannelIndex())), dye);

                    Log.Debug($"{item.Name}: DefaultDye {dye.Hash}");
                }
            }
            if (!dyes.Any())
            {
                Log.Warning($"Shader {item.Name} contains no dyes, skipping");
                return;
            }

            // armor
            if (dyes.ContainsKey("ArmorPlate"))
                AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ArmorPlate"], dyes["ArmorSuit"], dyes["ArmorCloth"] }, "_armor");
            // ghost
            if (dyes.ContainsKey("GhostMain"))
                AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["GhostMain"], dyes["GhostHighlights"], dyes["GhostDecals"] }, "_ghost");
            // ship
            if (dyes.ContainsKey("ShipUpper"))
                AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["ShipUpper"], dyes["ShipDecals"], dyes["ShipLower"] }, "_ship");
            // sparrow
            if (dyes.ContainsKey("SparrowUpper"))
                AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["SparrowUpper"], dyes["SparrowEngine"], dyes["SparrowLower"] }, "_sparrow");
            // weapon
            if (dyes.ContainsKey("Weapon1"))
                AutomatedExporter.SaveBlenderApiFile(savePath, name, outputTextureFormat, new List<Dye> { dyes["Weapon1"], dyes["Weapon2"], dyes["Weapon3"] }, "_weapon");

            Texture iridesceneLookup = Globals.Get().RenderGlobals.TagData.Textures.TagData.IridescenceLookup;
            TextureExporter.SaveTextureToFile($"{savePath}/Textures/Iridescence_Lookup", iridesceneLookup.GetScratchImage());
        }
    }
    #endregion

    private void RunWithLogging(string methodName, Action method)
    {
        try
        {
            Stopwatch sw = Stopwatch.StartNew();
            Log.Debug($"Starting {methodName}");
            method();
            sw.Stop();
            Log.Debug($"Completed {methodName} in {sw.Elapsed.Milliseconds}ms");
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
