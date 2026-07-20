#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// EquipmentSetupBuilder — BCE/Setup/Equipment ▶ Build Starter Items + Catalog
///
/// Generates:
///   • 2 "sneaker" upgrade items (sword_sneaker MainHand, shield_sneaker OffHand) with
///     world models — Paladin placeholders until the real SneakerGeek prefabs are imported.
///   • A base loadout for EVERY class: 5 classes × 6 slots = 30 base items
///     (Head, Chest, Feet, Hands, MainHand, OffHand), themed to each class.
///
/// All of them are collected into Assets/Game/Resources/EquipmentCatalog.asset, loaded at
/// runtime by both the dedicated server (slot lookup) and clients (model + stats).
///
/// serverItemId on each asset MUST match items.id in the auth DB — keep in lockstep with
/// _CONTEXT/equipment-items.sql. Safe to re-run; it overwrites existing assets.
/// </summary>
public static class EquipmentSetupBuilder
{
    const string DefsDir = "Assets/Game/Items/Definitions";
    const string ResDir  = "Assets/Game/Resources";

    // Tripo-generated sneaker weapon models (image-to-3D from the matching icons).
    const string SwordModel  = "Assets/Game/3D Models/Equipment/sword_sneaker/sword_sneaker.fbx";
    const string ShieldModel = "Assets/Game/3D Models/Equipment/shield_sneaker/shield_sneaker.fbx";

    // Icons live in Assets/Game/UI/Icons/Equipment/. Base items share a per-slot icon;
    // items with their own {serverId}.png use it instead.
    const string IconDir = "Assets/Game/UI/Icons/Equipment";

    // Any item with a model at Equipment/{serverId}/{serverId}.fbx is auto-wired.
    const string ModelDir = "Assets/Game/3D Models/Equipment";

    // Per-class flavor. weaponStat drives the MainHand; offhand handled in OffhandMods().
    static readonly (string id, string display, StatType weaponStat, string mainNoun, string offNoun)[] Classes =
    {
        ("warden",      "Warden",      StatType.Damage,    "Carbine", "Deflector"),
        ("ironclad",    "Ironclad",    StatType.Damage,    "Blade",   "Bulwark"),
        ("shadowblade", "Shadowblade", StatType.Damage,    "Dagger",  "Parrying Dagger"),
        ("cleric",      "Cleric",      StatType.HealPower, "Mace",    "Tome"),
        ("arcanist",    "Arcanist",    StatType.Damage,    "Staff",   "Focus"),
    };

    [MenuItem("BCE/Setup/Equipment ▶ Build Starter Items + Catalog")]
    public static void Build()
    {
        EnsureFolder(DefsDir);
        EnsureFolder(ResDir);

        var sword  = AssetDatabase.LoadAssetAtPath<GameObject>(SwordModel);
        var shield = AssetDatabase.LoadAssetAtPath<GameObject>(ShieldModel);
        if (sword == null)  Debug.LogWarning($"[EQUIP] Sword model not found at {SwordModel} — assign worldModelPrefab manually.");
        if (shield == null) Debug.LogWarning($"[EQUIP] Shield model not found at {ShieldModel} — assign worldModelPrefab manually.");

        var items = new List<ItemData>();

        // ── Sneaker upgrade set (has models) ────────────────────────────────────
        EnsureFolder($"{DefsDir}/_Sneaker");
        items.Add(MakeItem("_Sneaker", "sword_sneaker",  "Sneaker Blade",   EquipmentSlotType.MainHand, ItemRarity.Uncommon, sword,
                  new StatModifier(StatType.Damage,          ModifierKind.Percent, 0.08f)));
        items.Add(MakeItem("_Sneaker", "shield_sneaker", "Sneaker Bulwark", EquipmentSlotType.OffHand,  ItemRarity.Uncommon, shield,
                  new StatModifier(StatType.DamageReduction, ModifierKind.Percent, 0.06f),
                  new StatModifier(StatType.MaxHealth,       ModifierKind.Flat,    20f)));

        // ── Base loadout for every class (5 × 6 = 30, no models yet) ─────────────
        foreach (var c in Classes)
        {
            EnsureFolder($"{DefsDir}/{c.display}");

            items.Add(MakeItem(c.display, $"{c.id}_head_base",  $"{c.display} Helm",       EquipmentSlotType.Head,  ItemRarity.Common, null,
                      new StatModifier(StatType.MaxHealth, ModifierKind.Flat, 10f)));

            items.Add(MakeItem(c.display, $"{c.id}_chest_base", $"{c.display} Chestguard", EquipmentSlotType.Chest, ItemRarity.Common, null,
                      new StatModifier(StatType.MaxHealth, ModifierKind.Flat, 20f)));

            items.Add(MakeItem(c.display, $"{c.id}_feet_base",  $"{c.display} Boots",      EquipmentSlotType.Feet,  ItemRarity.Common, null,
                      new StatModifier(StatType.MoveSpeed, ModifierKind.Percent, 0.04f)));

            items.Add(MakeItem(c.display, $"{c.id}_hands_base", $"{c.display} Gloves",     EquipmentSlotType.Hands, ItemRarity.Common, null,
                      new StatModifier(StatType.CooldownReduction, ModifierKind.Percent, 0.03f)));

            items.Add(MakeItem(c.display, $"{c.id}_mainhand_base", $"{c.display} {c.mainNoun}", EquipmentSlotType.MainHand, ItemRarity.Common, null,
                      new StatModifier(c.weaponStat, ModifierKind.Percent, 0.05f)));

            items.Add(MakeItem(c.display, $"{c.id}_offhand_base",  $"{c.display} {c.offNoun}",  EquipmentSlotType.OffHand,  ItemRarity.Common, null,
                      OffhandMods(c.id)));
        }

        // ── Build / refresh the catalog ─────────────────────────────────────────
        string catalogPath = $"{ResDir}/EquipmentCatalog.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<EquipmentCatalog>(catalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<EquipmentCatalog>();
            AssetDatabase.CreateAsset(catalog, catalogPath);
        }
        catalog.items = items.ToArray();
        EditorUtility.SetDirty(catalog);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[EQUIP] Built {items.Count} items (2 sneaker + 30 base) + EquipmentCatalog at {catalogPath}. " +
                  "Swap sword_sneaker / shield_sneaker worldModelPrefab to the SneakerGeek models when imported.");
        Selection.activeObject = catalog;
    }

    static StatModifier[] OffhandMods(string classId)
    {
        switch (classId)
        {
            case "ironclad":    return new[] { new StatModifier(StatType.DamageReduction,   ModifierKind.Percent, 0.05f),
                                               new StatModifier(StatType.MaxHealth,         ModifierKind.Flat,    15f) };
            case "cleric":      return new[] { new StatModifier(StatType.HealPower,         ModifierKind.Percent, 0.04f) };
            case "arcanist":    return new[] { new StatModifier(StatType.CooldownReduction, ModifierKind.Percent, 0.04f) };
            case "shadowblade": return new[] { new StatModifier(StatType.Damage,            ModifierKind.Percent, 0.03f) };
            default:            return new[] { new StatModifier(StatType.DamageReduction,   ModifierKind.Percent, 0.03f) }; // warden
        }
    }

    static ItemData MakeItem(string subfolder, string serverId, string display, EquipmentSlotType slot,
                             ItemRarity rarity, GameObject model, params StatModifier[] mods)
    {
        string path = $"{DefsDir}/{subfolder}/{serverId}.asset";
        var item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
        if (item == null)
        {
            item = ScriptableObject.CreateInstance<ItemData>();
            AssetDatabase.CreateAsset(item, path);
        }

        item.itemName         = display;
        item.serverItemId     = serverId;
        item.description       = $"{display} — base {slot} gear.";
        item.stackable        = false;
        item.maxStackSize     = 1;
        item.itemType         = ItemType.Equipment;
        item.rarity           = rarity;
        item.equippable       = true;
        item.equipSlot        = slot;
        // Auto-wire a model at the convention path if one wasn't passed explicitly.
        if (model == null)
            model = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelDir}/{serverId}/{serverId}.fbx");
        item.worldModelPrefab = model;

        // Icon: prefer an item-specific PNG (sword_sneaker.png), else the shared slot icon.
        var icon = LoadSprite($"{IconDir}/{serverId}.png")
                ?? LoadSprite($"{IconDir}/icon_slot_{SlotKey(slot)}.png");
        if (icon != null) item.icon = icon;
        item.attachPosition   = Vector3.zero;
        item.attachEuler      = Vector3.zero;
        item.attachScale      = Vector3.one;
        item.baseModifiers    = mods;

        EditorUtility.SetDirty(item);
        return item;
    }

    // Ensure a PNG is imported as a single Sprite, then return it.
    static Sprite LoadSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;   // not present / not a texture

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }
        // Multiple-mode (with no slices) makes LoadAssetAtPath<Sprite> return null — force Single.
        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }
        if (changed) importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static string SlotKey(EquipmentSlotType slot)
    {
        switch (slot)
        {
            case EquipmentSlotType.Head:     return "head";
            case EquipmentSlotType.Chest:    return "chest";
            case EquipmentSlotType.Feet:     return "feet";
            case EquipmentSlotType.Hands:    return "hands";
            case EquipmentSlotType.MainHand:
            case EquipmentSlotType.Weapon:   return "mainhand";
            case EquipmentSlotType.OffHand:  return "offhand";
            default:                         return "";
        }
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf   = System.IO.Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
