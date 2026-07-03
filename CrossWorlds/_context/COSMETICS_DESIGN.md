# Crossworlds BCE — Cosmetics & Essence Drop System

Status: Phase 2 build target  
Last updated: 2026-06-28

---

## Design Goal

Give players a concrete reason to kill mobs beyond XP and gear. Essence materials drop
from specific enemy types at low rates and are consumed at the Forge to unlock cosmetic
hero skins. No new systems required — uses `DropTable.cs`, `items` table, `recipes` table,
and Unity's `MaterialPropertyBlock` for runtime color swaps.

**Rule: cosmetics are never stat-relevant. Skins are pure expression.**

---

## Essence Materials

Four essence types, one per enemy tier + boss. All use `item_type = 'material'`.

| Item ID | Name | Source | Drop Rate | Rarity |
|---|---|---|---|---|
| `essence_grunt` | Grunt Essence | Enemy_Grunt | 10% per kill | common |
| `essence_ranged` | Ranged Essence | Enemy_Ranged | 5% per kill | uncommon |
| `essence_elite` | Elite Essence | Enemy_Elite | 2% per kill | uncommon |
| `essence_void` | Void Essence | World Boss | 100% (1 per kill) | rare |

These drop via `DropTable.ScriptableObject` on each enemy type. Add them to the existing
grunt/ranged/elite drop table assets (or wire them in `DropTable.cs` as additional entries).

---

## Cosmetic Unlock Types

Three tiers of cosmetic, each requiring more effort to earn.

### Tier 1 — Ability Trail Recolor (any hero)
*"Your movement leaves a colored particle trail."*

Unlocked by: craft at Forge using grunt essences.

| Recipe ID | Name | Ingredients | Result |
|---|---|---|---|
| `recipe_trail_silver` | Silver Trail | 25× `essence_grunt` | `cosmetic_trail_silver` |
| `recipe_trail_ember` | Ember Trail | 40× `essence_grunt` + 10× `essence_ranged` | `cosmetic_trail_ember` |
| `recipe_trail_void` | Void Trail | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_trail_void` |

### Tier 2 — Cast VFX Variant (per hero)
*"Ability cast indicators pulse with a deeper, saturated version of your class color."*

Unlocked by: craft at Forge using ranged + elite essences.

| Recipe ID | Name | Ingredients | Result | Hero |
|---|---|---|---|---|
| `recipe_skin_vfx_warden` | Warden Aura | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_vfx_warden` | Warden |
| `recipe_skin_vfx_ironclad` | Ironclad Aura | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_vfx_ironclad` | Ironclad |
| `recipe_skin_vfx_arcanist` | Arcanist Aura | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_vfx_arcanist` | Arcanist |
| `recipe_skin_vfx_cleric` | Cleric Aura | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_vfx_cleric` | Cleric |
| `recipe_skin_vfx_shadow` | Shadow Aura | 20× `essence_ranged` + 5× `essence_elite` | `cosmetic_vfx_shadow` | Shadowblade |

### Tier 3 — Hero Skin (full color repalette)
*"Recolor your hero's material to an alternate palette. Visible to all players."*

Unlocked by: craft at Forge using elite essences + void essence (boss material).

Each hero has 2 alternate skins. Base skin is always the default class color.

| Hero | Skin Name | Color Theme | Ingredients |
|---|---|---|---|
| Warden | **Storm Warden** | Dark storm grey + lightning blue | 10× `essence_elite` + 2× `essence_void` |
| Warden | **Verdant Warden** | Deep forest green + amber | 8× `essence_elite` + 3× `essence_void` |
| Ironclad | **Obsidian Guard** | Matte black + molten orange seams | 10× `essence_elite` + 2× `essence_void` |
| Ironclad | **Gilded Vanguard** | Polished gold + white trim | 8× `essence_elite` + 3× `essence_void` |
| Arcanist | **Crimson Void** | Deep red + black void tendrils | 10× `essence_elite` + 2× `essence_void` |
| Arcanist | **Pale Arcanist** | Ice white + pale violet | 8× `essence_elite` + 3× `essence_void` |
| Cleric | **Shadow Cleric** | Deep violet + gold runes | 10× `essence_elite` + 2× `essence_void` |
| Cleric | **Dawn Cleric** | Warm ivory + sunrise orange | 8× `essence_elite` + 3× `essence_void` |
| Shadowblade | **Crimson Blade** | Blood red + black | 10× `essence_elite` + 2× `essence_void` |
| Shadowblade | **Phantom** | Near-invisible grey + subtle shimmer | 8× `essence_elite` + 3× `essence_void` |

---

## Server — Items to Seed

Add to `INSERT IGNORE INTO items` in `/opt/rod-auth/server.js`:

```sql
-- Essence materials
INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value, crafted)
VALUES
  ('essence_grunt',   'Grunt Essence',   'common',   'material', '{}', 5,  0),
  ('essence_ranged',  'Ranged Essence',  'uncommon', 'material', '{}', 15, 0),
  ('essence_elite',   'Elite Essence',   'uncommon', 'material', '{}', 50, 0),
  ('essence_void',    'Void Essence',    'rare',     'material', '{}', 200, 0);

-- Cosmetic trail items
INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value, crafted)
VALUES
  ('cosmetic_trail_silver', 'Silver Trail',   'uncommon', 'material', '{"cosmetic":"trail_silver"}', 0, 1),
  ('cosmetic_trail_ember',  'Ember Trail',    'uncommon', 'material', '{"cosmetic":"trail_ember"}',  0, 1),
  ('cosmetic_trail_void',   'Void Trail',     'rare',     'material', '{"cosmetic":"trail_void"}',   0, 1);

-- VFX aura cosmetics
INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value, crafted)
VALUES
  ('cosmetic_vfx_warden',   'Warden Aura',   'rare', 'material', '{"cosmetic":"vfx_warden"}',  0, 1),
  ('cosmetic_vfx_ironclad', 'Ironclad Aura', 'rare', 'material', '{"cosmetic":"vfx_ironclad"}',0, 1),
  ('cosmetic_vfx_arcanist', 'Arcanist Aura', 'rare', 'material', '{"cosmetic":"vfx_arcanist"}',0, 1),
  ('cosmetic_vfx_cleric',   'Cleric Aura',   'rare', 'material', '{"cosmetic":"vfx_cleric"}',  0, 1),
  ('cosmetic_vfx_shadow',   'Shadow Aura',   'rare', 'material', '{"cosmetic":"vfx_shadow"}',  0, 1);

-- Hero skin cosmetics
INSERT IGNORE INTO items (id, name, rarity, item_type, stat_bonus, sell_value, crafted)
VALUES
  ('cosmetic_skin_warden_storm',    'Storm Warden',     'epic', 'material', '{"cosmetic":"skin","hero":0,"palette":"storm"}',    0, 1),
  ('cosmetic_skin_warden_verdant',  'Verdant Warden',   'epic', 'material', '{"cosmetic":"skin","hero":0,"palette":"verdant"}',  0, 1),
  ('cosmetic_skin_ironclad_obsidian','Obsidian Guard',  'epic', 'material', '{"cosmetic":"skin","hero":1,"palette":"obsidian"}', 0, 1),
  ('cosmetic_skin_ironclad_gilded', 'Gilded Vanguard',  'epic', 'material', '{"cosmetic":"skin","hero":1,"palette":"gilded"}',   0, 1),
  ('cosmetic_skin_arcanist_crimson','Crimson Void',     'epic', 'material', '{"cosmetic":"skin","hero":2,"palette":"crimson"}',  0, 1),
  ('cosmetic_skin_arcanist_pale',   'Pale Arcanist',    'epic', 'material', '{"cosmetic":"skin","hero":2,"palette":"pale"}',     0, 1),
  ('cosmetic_skin_cleric_shadow',   'Shadow Cleric',    'epic', 'material', '{"cosmetic":"skin","hero":3,"palette":"shadow"}',   0, 1),
  ('cosmetic_skin_cleric_dawn',     'Dawn Cleric',      'epic', 'material', '{"cosmetic":"skin","hero":3,"palette":"dawn"}',     0, 1),
  ('cosmetic_skin_shadow_crimson',  'Crimson Blade',    'epic', 'material', '{"cosmetic":"skin","hero":4,"palette":"crimson"}',  0, 1),
  ('cosmetic_skin_shadow_phantom',  'Phantom',          'epic', 'material', '{"cosmetic":"skin","hero":4,"palette":"phantom"}',  0, 1);
```

---

## Server — Recipes to Seed

```sql
-- profession_id 1 = Smithing (forge)

-- Trail recipes
INSERT IGNORE INTO recipes (id, profession_id, skill_level_required, result_item_id, result_quantity)
VALUES
  ('recipe_trail_silver', 1, 1, 'cosmetic_trail_silver', 1),
  ('recipe_trail_ember',  1, 3, 'cosmetic_trail_ember',  1),
  ('recipe_trail_void',   1, 5, 'cosmetic_trail_void',   1);

INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity)
VALUES
  ('recipe_trail_silver', 'essence_grunt',  25),
  ('recipe_trail_ember',  'essence_grunt',  40),
  ('recipe_trail_ember',  'essence_ranged', 10),
  ('recipe_trail_void',   'essence_ranged', 20),
  ('recipe_trail_void',   'essence_elite',  5);

-- VFX aura recipes (one per hero)
INSERT IGNORE INTO recipes (id, profession_id, skill_level_required, result_item_id, result_quantity)
VALUES
  ('recipe_skin_vfx_warden',   1, 3, 'cosmetic_vfx_warden',   1),
  ('recipe_skin_vfx_ironclad', 1, 3, 'cosmetic_vfx_ironclad', 1),
  ('recipe_skin_vfx_arcanist', 1, 3, 'cosmetic_vfx_arcanist', 1),
  ('recipe_skin_vfx_cleric',   1, 3, 'cosmetic_vfx_cleric',   1),
  ('recipe_skin_vfx_shadow',   1, 3, 'cosmetic_vfx_shadow',   1);

INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity)
VALUES
  ('recipe_skin_vfx_warden',   'essence_ranged', 20),
  ('recipe_skin_vfx_warden',   'essence_elite',  5),
  ('recipe_skin_vfx_ironclad', 'essence_ranged', 20),
  ('recipe_skin_vfx_ironclad', 'essence_elite',  5),
  ('recipe_skin_vfx_arcanist', 'essence_ranged', 20),
  ('recipe_skin_vfx_arcanist', 'essence_elite',  5),
  ('recipe_skin_vfx_cleric',   'essence_ranged', 20),
  ('recipe_skin_vfx_cleric',   'essence_elite',  5),
  ('recipe_skin_vfx_shadow',   'essence_ranged', 20),
  ('recipe_skin_vfx_shadow',   'essence_elite',  5);

-- Hero skin recipes (2 per hero = 10 total)
INSERT IGNORE INTO recipes (id, profession_id, skill_level_required, result_item_id, result_quantity)
VALUES
  ('recipe_skin_warden_storm',     1, 5, 'cosmetic_skin_warden_storm',     1),
  ('recipe_skin_warden_verdant',   1, 5, 'cosmetic_skin_warden_verdant',   1),
  ('recipe_skin_ironclad_obsidian',1, 5, 'cosmetic_skin_ironclad_obsidian',1),
  ('recipe_skin_ironclad_gilded',  1, 5, 'cosmetic_skin_ironclad_gilded',  1),
  ('recipe_skin_arcanist_crimson', 1, 5, 'cosmetic_skin_arcanist_crimson', 1),
  ('recipe_skin_arcanist_pale',    1, 5, 'cosmetic_skin_arcanist_pale',    1),
  ('recipe_skin_cleric_shadow',    1, 5, 'cosmetic_skin_cleric_shadow',    1),
  ('recipe_skin_cleric_dawn',      1, 5, 'cosmetic_skin_cleric_dawn',      1),
  ('recipe_skin_shadow_crimson',   1, 5, 'cosmetic_skin_shadow_crimson',   1),
  ('recipe_skin_shadow_phantom',   1, 5, 'cosmetic_skin_shadow_phantom',   1);

INSERT IGNORE INTO recipe_ingredients (recipe_id, item_id, quantity)
VALUES
  ('recipe_skin_warden_storm',     'essence_elite', 10),
  ('recipe_skin_warden_storm',     'essence_void',  2),
  ('recipe_skin_warden_verdant',   'essence_elite', 8),
  ('recipe_skin_warden_verdant',   'essence_void',  3),
  ('recipe_skin_ironclad_obsidian','essence_elite', 10),
  ('recipe_skin_ironclad_obsidian','essence_void',  2),
  ('recipe_skin_ironclad_gilded',  'essence_elite', 8),
  ('recipe_skin_ironclad_gilded',  'essence_void',  3),
  ('recipe_skin_arcanist_crimson', 'essence_elite', 10),
  ('recipe_skin_arcanist_crimson', 'essence_void',  2),
  ('recipe_skin_arcanist_pale',    'essence_elite', 8),
  ('recipe_skin_arcanist_pale',    'essence_void',  3),
  ('recipe_skin_cleric_shadow',    'essence_elite', 10),
  ('recipe_skin_cleric_shadow',    'essence_void',  2),
  ('recipe_skin_cleric_dawn',      'essence_elite', 8),
  ('recipe_skin_cleric_dawn',      'essence_void',  3),
  ('recipe_skin_shadow_crimson',   'essence_elite', 10),
  ('recipe_skin_shadow_crimson',   'essence_void',  2),
  ('recipe_skin_shadow_phantom',   'essence_elite', 8),
  ('recipe_skin_shadow_phantom',   'essence_void',  3);
```

---

## Server — Drop Table Wiring

Add essence drops to each enemy's DropTable ScriptableObject (Unity), OR add them to
`DropTable.RollDrops()` logic server-side via a hardcoded secondary drop pass:

In `EnemyController.cs` `DeathSequence()` (server side), after existing loot:
```csharp
// Essence drop — cosmetic material, low rate
(string essenceId, float rate) essenceDrop = enemyType switch {
    EnemyType.Grunt  => ("essence_grunt",  0.10f),
    EnemyType.Ranged => ("essence_ranged", 0.05f),
    EnemyType.Elite  => ("essence_elite",  0.02f),
    _ => (null, 0f)
};
if (essenceDrop.essenceId != null && Random.value <= essenceDrop.rate)
    NetworkServer.Spawn(WorldItem.CreateForItem(essenceDrop.essenceId, 1, transform.position));
```

World Boss: award `essence_void` directly to all participants via `POST /api/inventory/add-item`
in the boss kill handler (guaranteed, 1 per character).

---

## Unity — Cosmetic Applier

`HeroCosmeticApplier.cs`  
Path: `Assets/Game/Characters/Scripts/HeroCosmeticApplier.cs`

On scene load: reads equipped cosmetics from inventory (`stat_bonus` JSON contains `"cosmetic"` key).
Applies cosmetics using `MaterialPropertyBlock` — no new materials created, no allocations.

### Color Palettes

```csharp
static readonly Dictionary<string, Color> SkinPalettes = new()
{
    // Warden
    ["warden_storm"]    = new Color(0.25f, 0.30f, 0.40f),  // storm grey
    ["warden_verdant"]  = new Color(0.15f, 0.45f, 0.20f),  // forest green

    // Ironclad
    ["ironclad_obsidian"] = new Color(0.12f, 0.12f, 0.14f), // matte black
    ["ironclad_gilded"]   = new Color(0.85f, 0.70f, 0.20f), // polished gold

    // Arcanist
    ["arcanist_crimson"] = new Color(0.55f, 0.05f, 0.05f),  // deep red
    ["arcanist_pale"]    = new Color(0.88f, 0.85f, 0.95f),  // ice white

    // Cleric
    ["cleric_shadow"]   = new Color(0.30f, 0.10f, 0.50f),   // deep violet
    ["cleric_dawn"]     = new Color(0.95f, 0.85f, 0.70f),   // warm ivory

    // Shadowblade
    ["shadow_crimson"]  = new Color(0.60f, 0.05f, 0.05f),   // blood red
    ["shadow_phantom"]  = new Color(0.55f, 0.55f, 0.58f),   // near-invisible grey
};
```

### Applying a skin
```csharp
void ApplySkin(string paletteKey)
{
    var block = new MaterialPropertyBlock();
    if (SkinPalettes.TryGetValue(paletteKey, out Color col))
    {
        block.SetColor("_BaseColor", col);
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.SetPropertyBlock(block);
    }
}
```

### Trail cosmetic
Spawns a `CosmeticTrailSystem` child object (ParticleSystem, world-space):
- `trail_silver`: white-silver, 8 particles/s, 0.4s lifetime
- `trail_ember`: orange-red, 10 particles/s, 0.5s lifetime, slight upward drift
- `trail_void`: deep purple, 12 particles/s, 0.6s lifetime, slow fade to black

### VFX aura cosmetic
Sets a flag on `AbilityCaster` → `GetCategoryColor()` returns the saturated hero color
instead of the default category color. Single bool per hero class.

---

## Player-Facing Loop

```
Kill Grunt × 250  →  collect 25 Grunt Essence  →  forge Silver Trail        (3–4 hours)
Kill Ranged × 400 →  collect 20 Ranged Essence \
Kill Elite  × 250 →  collect 5 Elite Essence    → forge Cleric Aura VFX     (8–12 hours)
Kill Boss   × 2   →  collect 2 Void Essence    \
Kill Elite  × 500 →  collect 10 Elite Essence   → forge Shadow Cleric skin  (20–30 hours)
```

Every mob kill has a tiny chance of being a cosmetic drop. The bar is visible in the
inventory as a stackable material. Players who are close to a threshold feel it.
The grind is transparent, not hidden behind RNG boxes.

---

## What This Is NOT

- Not a paid shop item — all skins earnable through normal play
- Not a stats-affecting item — `stat_bonus` JSON only used for cosmetic metadata
- Not a loot box — exact ingredient count known upfront, no random unlock
- Not character select locked — skins can be changed in Hub between runs
