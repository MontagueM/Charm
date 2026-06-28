using System.Collections.Concurrent;

namespace Tiger.Schema.Investment;

public class InventoryItem : Tag<S8080799D>
{
    public InventoryItem(FileHash hash, bool shouldParse) : base(hash, shouldParse)
    {
    }

    public int CollectibleIndex => Investment.Get().GetCollectibleIndexFromItemIndex(GetItemIndex());

    private string _source = null;
    public string Source => _source ??= CollectibleIndex != -1 ? Investment.Get().GetCollectibleStrings(CollectibleIndex)?.SourceString.Value : "";

    public uint ApiHash => _tag.InventoryItemHash.Hash32;

    private IReadOnlyCollection<DestinyTraitID> _traits;
    public IReadOnlyCollection<DestinyTraitID> ItemTraits => _traits ??= GetItemTraits(); // cache traits on first use

    private bool? _isWeapon;
    public bool IsWeapon => _isWeapon ??= ItemTraits.Any(x => x.ToString().StartsWith("item_weapon_", StringComparison.Ordinal));

    private bool? _isArmor;
    public bool IsArmor => _isArmor ??= ItemTraits.Any(x => x.ToString().StartsWith("item_armor", StringComparison.Ordinal));

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
    public bool IsHolofoil => _isHolofoil ??= IsItemHolofoil();

    private string _type = null;
    public string Type => _type ??= GetItemType();

    private string _description = null;
    public string Description => _description ??= GetItemDescription();

    private string _flavorText = null;
    public string FlavorText => _flavorText ??= GetItemFlavorText();

    private string _lore = null;
    public string Lore => _lore ??= GetItemLore();

    private ConcurrentBag<InventoryItem> _ornaments = null;
    public ConcurrentBag<InventoryItem> Ornaments => _ornaments ??= GetItemOrnaments();

    // If this item is an ornament this will be its parent item
    public InventoryItem Parent = null;

    private int? _artArrangementIndex = null;
    public int ArtArrangementIndex => _artArrangementIndex ??= GetArtArrangementIndex();

    private int? _damageTypeIndex = null;
    public int DamageTypeIndex => _damageTypeIndex ??= GetItemDamageTypeIndex();

    public override void Load(bool force = false)
    {
        base.Load(force);

        // this is needed to make sure its ornaments are loaded (if it has any)
        // which in turn will set the ornaments parent item
        _ = Ornaments;
    }

    private bool IsItemHolofoil()
    {
        if (!Strategy.IsD1() && _tag.Unk78_EoF.GetValue(GetReader()) is S80807374 Unk && Unk.Unk20.Any(x => x.Unk00 == 0xF2))
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

        // Featured items were retired in MoT
        //if (GetItemRarity() == DestinyTierType.Exotic && (traits.Any(x => x.ToString().Contains("item_weapon")) || traits.Any(x => x.ToString().Contains("item_armor"))))
        //    Investment.Get().FeaturedItems.Add(GetItemIndex());

        //if (Investment.Get().FeaturedItems.Contains(GetItemIndex()))
        //    traits.Add(DestinyTraitID.item_featured);

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
        if (_tag.Unk50.GetValue(GetReader()) is S808077DC quality)
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

    public Tag<S8080549F> GetItemStrings()
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

    public DestinyDamageTypeEnum GetDamageType()
    {
        return DestinyDamageType.GetDamageType(DamageTypeIndex);
    }

    public int GetItemDamageTypeIndex()
    {
        var index = -1;
        if (_tag.Unk78.GetValue(GetReader()) is S80807381 perks)
        {
            foreach (S80807387 perk in perks.Perks)
            {
                if (Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex != -1)
                {
                    index = Investment.Get().SandboxPerkMap2[perk.PerkIndex].UnkIndex;
                    break;
                }
            }
        }

        // if the damage type wasnt found in perks
        if (index == -1 && _tag.Unk70.GetValue(GetReader()) is S808077C0 sockets)
        {
            sockets.SocketEntries.ForEach(entry =>
            {
                if (entry.SocketTypeIndex == -1 || entry.SingleInitialItemIndex == -1)
                    return;

                S808076BA socket = Investment.Get().GetSocketType(entry.SocketTypeIndex);
                foreach (S808076C5 a in socket.PlugWhitelists)
                {
                    if (a.PlugCategoryHash.Hash32 == 1466776700) // 'v300.weapon.damage_type.energy', Y1 weapon that uses a damage type mod from ye olden days
                    {
                        InventoryItem item = Investment.Get().GetInventoryItem(entry.SingleInitialItemIndex);
                        item.Load(true); // idk why the item sometimes isnt fully loaded
                        index = item.DamageTypeIndex;
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
        if (_tag.Unk90.GetValue(GetReader()) is S80807377 entry && entry.Arrangements.Count > 0)
            return entry.Arrangements[GetReader(), 0].ArtArrangementHash;

        return -1;
    }

    public int GetArtArrangementCount()
    {
        if (_tag.Unk90 is null) return 0;
        if (_tag.Unk90.GetValue(GetReader()) is S80807377 entry)
            return entry.Arrangements.Count;

        return 0;
    }

    // very few items (practically only the Lightguard sets for example) have multiple art arrangements, this returns them all
    public List<int> GetArtArrangementIndices()
    {
        if (_tag.Unk90 is null) return new();
        if (_tag.Unk90.GetValue(GetReader()) is S80807377 entry && entry.Arrangements.Count > 0)
            return entry.Arrangements.Enumerate(GetReader()).Select(x => (int)x.ArtArrangementHash).ToList();

        return new();
    }

    public int GetWeaponPatternIndex()
    {
        if (_tag.Unk90.GetValue(GetReader()) is S80807377 entry && entry.WeaponPatternIndex > 0)
            return entry.WeaponPatternIndex;

        return -1;
    }

    public ConcurrentBag<InventoryItem> GetItemOrnaments()
    {
        ConcurrentBag<InventoryItem> ornaments = new();
        if (Strategy.CurrentStrategy >= TigerStrategy.DESTINY2_WITCHQUEEN_6307 && _tag.Unk70.GetValue(GetReader()) is S808077C0 sockets)
        {
            foreach (S808077C3 socket in sockets.SocketEntries)
            {
                if (socket.SocketTypeIndex == -1)
                    continue;

                S808076BA socketType = Investment.Get().GetSocketType(socket.SocketTypeIndex);
                if (!socketType.PlugWhitelists.Any(x => // hopefully this is better than just checking the sockets name
                   x.PlugCategoryHash.Hash32 != 3940152116 // exotic_all_skins
                || x.PlugCategoryHash.Hash32 != 3356843615)) // armor_skins_empty
                    continue;

                if (socket.ReusablePlugSetIndex1 != -1) // huh?
                {
                    foreach (S808077D5 randomPlugs in Investment.Get().GetRandomizedPlugSet(socket.ReusablePlugSetIndex1))
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

                foreach (S808077D5 plug in socket.PlugItems)
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
        else if (Strategy.IsD1() && _tag.Unk78.GetValue(GetReader()) is S808017BD a)
        {
            Tag<S80801963> talentGrid = Investment.Get().GetTalentGrid(a.TalenGridIndex);
            foreach (S80801728 node in talentGrid.TagData.Nodes)
            {
                foreach (S80801758 entry in node.Unk18)
                {
                    foreach (S80800F94 entry2 in entry.Unk70)
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
        Tag<S80803EB8>? iconContainer = Investment.Get().GetItemIconContainer(this);
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
        Tag<S80803EB8>? iconContainer = Investment.Get().GetItemIconContainer(this);
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
        Tag<S80803EB8>? iconContainer = Investment.Get().GetItemIconContainer(this);
        if (iconContainer == null || iconContainer.TagData.IconPrimaryContainer == null)
            return null;
        Texture? primaryIcon = Investment.Get().GetTextureFromContainer(iconContainer.TagData.IconPrimaryContainer);
        return primaryIcon;
    }

    public Texture? GetIconPrimaryTexture(int index, int listIndex = 0)
    {
        Tag<S80803EB8>? iconContainer = Investment.Get().GetItemIconContainer(this);
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
        Tag<S80803EB8>? iconContainer = Investment.Get().GetItemIconContainer(this);
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
        Tag<S80803EB8>? iconContainer = Investment.Get().GetFoundryItemIconContainer(this);
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

