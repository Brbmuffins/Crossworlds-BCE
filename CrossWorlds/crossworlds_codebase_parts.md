# Crossworlds BCE — Codebase Partitioning Plan

> **Purpose:** Divide the Unity client + auth server into isolated, well-named slices so that any bug, feature, or AI agent task can be scoped to one (or at most two) parts with minimal bleed-over.

---

## The Seven Parts

| # | Part Name | Layer | Status |
|---|-----------|-------|--------|
| 1 | **Auth & Identity** | Server + Unity | ✅ Stable |
| 2 | **Combat & Enemies** | Unity (server-adjacent) | 🔶 In progress |
| 3 | **Loot & World Items** | Unity + Server | 🔶 Active week 4 |
| 4 | **Inventory & Equipment** | Unity + Server | 🔶 Active week 4 |
| 5 | **Progression & Stats** | Unity + Server | ○ Next |
| 6 | **UI Shell** | Unity | 🔶 Parallel to 3–5 |
| 7 | **Scene & Navigation** | Unity | ✅ Mostly stable |

> **NPC/Dialogue** is small enough to live inside Part 7 (Scene & Navigation) for now. Promote it to its own part if Hangman or other NPCs grow significantly.

---

## Part 1 — Auth & Identity

### Purpose
Everything that proves who the player is and what character they own. Login, token issuance, character creation/selection, and the `PlayerIdentity` component that every other Unity system reads.

### Files & Folders
**Server:**
- `POST /register`, `POST /login` — `/opt/rod-auth/server.js` (auth block)
- `GET /api/health`

**Unity:**
- `Assets/Game/Player/PlayerIdentity.cs` (and related player-init scripts)
- Login scene logic (LoginScene 0)
- CharacterSelect scene logic (CharacterSelect 1)

### Depends On
- Nothing. This is the root of the dependency tree.

### What Other Parts Depend On It
- **All parts** read `PlayerIdentity` for `characterId` and the JWT token before calling any API.

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| Can't log in / bad credentials | ✅ Yes |
| JWT rejected on any `/api/*` endpoint | ✅ Yes (check token generation + expiry) |
| Character not found after login | ✅ Yes |
| Player spawns with wrong class | ✅ Yes (`class_index` comes from `/character` response) |
| Stats wrong after equip | ❌ Crosses into Part 4 |

### Open Issues in This Part
- _(None currently listed — auth is stable)_

---

## Part 2 — Combat & Enemies

### Purpose
All logic that happens inside an arena fight: spawning enemies, calculating and applying damage, ability casting, death handling, and waves. This part does **not** own loot drops — it only signals that an enemy died.

### Files & Folders
```
Assets/Game/Combat/Scripts/
  EnemyDeathHandler.cs       — fires the "enemy died" event; calls into Part 3 for drops
  EnemyDropTable.cs          — ScriptableObject: what items drop at what rates
  WaveSpawner.cs             — controls wave timing and enemy count
  AbilityCaster.cs           — player ability execution (class-specific)
  FloatingDamageText.cs      — VFX only, no game logic
```

**Server (read-only, no writes from combat):**
- No direct server calls during combat. Stats are read at spawn via Part 5.
- Kill results (XP, loot) are flushed to server on hub return via Parts 3 and 5.

### Depends On
- **Part 1** — needs `PlayerIdentity` to know the player's class for ability lookup
- **Part 5** — reads `CharacterStats` to know damage/health values
- **Part 3** — `EnemyDeathHandler` triggers loot drop logic in `WorldItem`

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| Enemy doesn't die at 0 HP | ✅ Yes (`EnemyDeathHandler`) |
| Enemy dies but no loot spawns | ❌ Boundary: Part 2 → Part 3 (`EnemyDeathHandler` calls `WorldItem` spawn) |
| Wrong damage numbers | ✅ Yes (`AbilityCaster` + `CharacterStats` from Part 5) |
| Wave doesn't end / next wave won't start | ✅ Yes (`WaveSpawner`) |
| Ability fires but no animation | ✅ Yes (VFX wiring in `AbilityCaster`) |

### Open Issues
- Enemy prefabs need NavMesh agents + aggro radius — not yet implemented
- Hit confirmation (server-authoritative or client-side?) still TBD

---

## Part 3 — Loot & World Items

### Purpose
Everything from "enemy drops an item" to "item lands in inventory." Covers drop probability, the physical `WorldItem` in the scene, pickup detection, and the API write that persists the new inventory state.

### Files & Folders
```
Assets/Game/Combat/Scripts/
  EnemyDropTable.cs          — shared with Part 2 (the ScriptableObject definition lives here;
                               Part 2 reads it, Part 3 evaluates it)
  WorldItem.cs               — the floating pickup object; handles rarity glow, collision,
                               calls POST /api/inventory/save on pickup

Assets/Game/Systems/
  ItemCatalogManager.cs      — loads and caches item definitions from /items or local SO data
  InventoryManager.cs        — shared with Part 4; owns the in-memory slot array;
                               Part 3 writes new slots, Part 4 equips them
```

**Server endpoints owned by this part:**
```
GET  /api/inventory/:characterId    — initial load on scene entry
POST /api/inventory/save            — called on every pickup and on hub return
GET  /items                         — item_template list (old system, read-only)
```

**DB tables touched:**
- `inventory` (write), `items` (read), `item_template` (read-only, old system)

### Depends On
- **Part 1** — JWT + `characterId`
- **Part 2** — `EnemyDeathHandler` triggers the drop roll
- **Part 6** — pickup success should flash a HUD notification (optional coupling)

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| Loot isn't dropping at all | ✅ Start in `EnemyDropTable` + `EnemyDeathHandler` |
| Loot drops but `WorldItem` doesn't spawn | ✅ `WorldItem` prefab / instantiation |
| Item picked up but not in bag | ✅ `POST /api/inventory/save` — check request payload + server log |
| Item in DB but not showing in bag UI | ❌ Crosses into Part 4 (InventoryManager → UI) |
| Wrong item drops (wrong rarity or type) | ✅ `EnemyDropTable` ScriptableObject |

---

## Part 4 — Inventory & Equipment

### Purpose
The full lifecycle of items once they're in the player's bag: displaying the grid UI, tooltips, equipping/unequipping, applying stat bonuses from equipped gear, and persisting equip state to the server.

### Files & Folders
```
Assets/Game/Systems/
  InventoryManager.cs        — shared owner of the slot array (see Part 3 note)
  HeroCosmeticApplier.cs     — applies visual changes when gear is equipped

Assets/Game/UI/
  InventoryBagUI.cs          — 4×6 grid, drag-and-drop, right-click equip
  ItemTooltipUI.cs           — hover tooltip with stats
```

**Server endpoints owned by this part:**
```
POST /api/inventory/equip           — {characterId, slot_index, equipped:0|1}
GET  /api/inventory/:characterId    — also used here on scene load
```

**DB tables touched:**
- `inventory` (equip flag), `items` (stat_bonus JSON read)

### Depends On
- **Part 1** — JWT + `characterId`
- **Part 3** — `InventoryManager` is populated by Part 3 on pickup/load
- **Part 5** — equipping an item recalculates `CharacterStats`
- **Part 6** — `InventoryBagUI` is a UI component; follows Part 6 UI conventions

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| Bag UI doesn't open | ✅ `InventoryBagUI` |
| Item icon missing in slot | ✅ `ItemCatalogManager` (icon_id lookup) |
| Equip button does nothing | ✅ `POST /api/inventory/equip` call path |
| Stats don't change after equip | ❌ Boundary: Part 4 → Part 5 (`stat_bonus` → `CharacterStats`) |
| Cosmetic doesn't update on equip | ✅ `HeroCosmeticApplier` |
| Inventory resets on reconnect | ✅ `GET /api/inventory/` load path |

---

## Part 5 — Progression & Stats

### Purpose
XP gain, leveling up, gold tracking, the `CharacterStats` data model, and stat recalculation from base class values plus equipped item bonuses. Also owns profession skill levels and the crafting loop.

### Files & Folders
```
Assets/Game/Systems/
  CharacterStats.cs          — single source of truth for current HP/mana/str/agi/int/vit
  ProgressionManager.cs      — XP accumulation, level-up logic, gold delta tracking
  HeroMasteryManager.cs      — hero mastery XP + unlock tracking (separate from char level)
  CombatSessionTracker.cs    — tallies kills/damage for post-session summary
  ItemCatalogManager.cs      — shared; Part 5 reads stat_bonus from items to feed CharacterStats

Assets/Game/UI/
  CharacterStatsHUD.cs       — live HP/mana bars
  GoldHUD.cs                 — gold counter
  XpHUD.cs                   — XP bar + level display
  HeroMasteryHUD.cs          — mastery progress bar
  HeroMasteryUI.cs           — full mastery panel

Assets/Game/NPC/             — crafting-adjacent
  HangmanNPC.cs              — (can be moved to Part 7; listed here because Hangman is
                               a stand-in for a crafting/profession NPC archetype)
```

**Server endpoints owned by this part:**
```
POST /api/character/save-progress   — level, xp, gold, stats
GET  /api/professions/:characterId
GET  /api/recipes?profession=mining
POST /api/craft
```

**DB tables touched:**
- `characters` (level/xp/gold/stats), `professions`, `recipes`, `recipe_ingredients`, `items` (read)

### Depends On
- **Part 1** — JWT + `characterId`
- **Part 4** — equipped item list is needed to compute `CharacterStats` total

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| XP not increasing on kill | ✅ `ProgressionManager` |
| Level-up not triggering | ✅ `ProgressionManager` |
| Stats not saved on hub return | ✅ `POST /api/character/save-progress` call |
| Gold not persisting | ✅ `ProgressionManager` + same endpoint |
| Craft fails even with materials | ✅ `POST /api/craft` — check server log for error string |
| Stat total wrong (base vs. equipped) | ❌ Boundary: Part 5 ↔ Part 4 (`CharacterStats` recalc) |

---

## Part 6 — UI Shell

### Purpose
Shared UI infrastructure: HUD layout, common popup/modal patterns, notification toasts, and any UI components that don't belong to a single gameplay system. Think of this as the "design system" layer — it provides widgets that Parts 3, 4, and 5 use, but it holds no game logic itself.

### Files & Folders
```
Assets/Game/UI/
  CharacterStatsHUD.cs       — owned by Part 5 for data, Part 6 for layout/rendering
  GoldHUD.cs                 — same dual-ownership pattern
  XpHUD.cs
  HeroMasteryHUD.cs
  HeroMasteryUI.cs
  InventoryBagUI.cs          — layout/rendering owned here; data owned by Part 4
  ItemTooltipUI.cs

Assets/Game/NPC/
  HangmanDialogueUI.cs       — dialogue popup; UI Shell conventions apply
```

> **Dual-ownership note:** Each HUD script is listed under its "data owner" part AND here. When a HUD has a visual bug (layout, color, animation), look in Part 6. When a HUD shows wrong numbers, look in the data-owner part.

### Depends On
- All parts that produce data to display (Parts 3, 4, 5)
- No server calls of its own

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| HUD panel in wrong screen position | ✅ Part 6 |
| Tooltip flickers or clips | ✅ Part 6 |
| Wrong number shown in XP bar | ❌ Part 5 data |
| Bag grid slots misaligned | ✅ Part 6 |
| Popup/modal doesn't close | ✅ Part 6 |

---

## Part 7 — Scene & Navigation

### Purpose
Scene transitions (Hub ↔ Portal ↔ Arena), portal/trigger colliders, hub return logic, and NPC interaction management. Owns the physical scene layout and the rules for moving between scenes.

### Files & Folders
```
Assets/Game/Scene/
  ArenaPortalTrigger.cs      — player enters portal → load arena scene + notify server
  HubReturnTrigger.cs        — player returns to hub → flush data (calls Parts 3, 5)

Assets/Game/NPC/
  NPCInteractionManager.cs   — proximity detection + interaction dispatch
  HangmanNPC.cs              — Hangman game logic (content NPC)
  HangmanDialogueUI.cs       — UI owned by Part 6 conventions but triggered here
```

**Server interactions (orchestrates, doesn't own):**
- On hub return: calls `POST /api/inventory/save` (Part 3) + `POST /api/character/save-progress` (Part 5) + `PATCH /character/position`

### Depends On
- **Part 1** — needs `PlayerIdentity` for position save
- **Part 3** — triggers inventory flush on hub return
- **Part 5** — triggers progress flush on hub return

### Bug Isolation
| Symptom | Isolated here? |
|---------|---------------|
| Portal doesn't load arena | ✅ `ArenaPortalTrigger` |
| Hub return doesn't trigger | ✅ `HubReturnTrigger` |
| Data lost on hub return | ❌ Boundary: Part 7 triggers Parts 3 + 5 flush; check all three |
| NPC interaction not firing | ✅ `NPCInteractionManager` |
| Position not saved on disconnect | ✅ `PATCH /character/position` (also see known bug: `orientation:F3`) |

---

## Dependency Graph

```
                    ┌─────────────────┐
                    │  1. Auth &      │
                    │   Identity      │
                    └────────┬────────┘
                             │  (all parts read PlayerIdentity + JWT)
          ┌──────────────────┼──────────────────────────┐
          │                  │                           │
  ┌───────▼──────┐  ┌────────▼────────┐       ┌────────▼────────┐
  │ 2. Combat &  │  │ 3. Loot &       │       │ 7. Scene &      │
  │   Enemies    │  │   World Items   │       │   Navigation    │
  └───────┬──────┘  └────────┬────────┘       └────────┬────────┘
          │  (death event)   │  (slot array)            │  (flush on return)
          └──────────────────▼──────────────────────────┘
                    ┌────────▼────────┐
                    │ 4. Inventory &  │
                    │   Equipment     │
                    └────────┬────────┘
                             │  (equipped bonuses)
                    ┌────────▼────────┐
                    │ 5. Progression  │
                    │   & Stats       │
                    └────────┬────────┘
                             │  (data to display)
                    ┌────────▼────────┐
                    │  6. UI Shell    │
                    └─────────────────┘
```

---

## Troubleshooting Cheat Sheet

| "What's broken?" | Check first | Check second |
|------------------|-------------|--------------|
| Loot isn't dropping | Part 2 — `EnemyDropTable` + `EnemyDeathHandler` | Part 3 — `WorldItem` spawn |
| Picked up item but bag is empty | Part 3 — `POST /api/inventory/save` payload + server log | Part 4 — `InventoryManager` load path |
| Player stats wrong after equip | Part 4 — `POST /api/inventory/equip` | Part 5 — `CharacterStats.Recalculate()` |
| XP / level not saving | Part 5 — `ProgressionManager` flush on hub return | Part 7 — `HubReturnTrigger` calling the flush |
| Can't log in | Part 1 — auth server logs | Part 1 — `POST /login` curl test |
| Arena won't load | Part 7 — `ArenaPortalTrigger` | Mirror/network layer |
| Craft fails unexpectedly | Part 5 — `POST /api/craft` server error string | Part 3 — inventory has the right materials |
| HUD shows wrong values | Part 6 — confirm correct data binding | Data-owner part (5 for XP/gold, 4 for stats) |
| Position not saved | Part 7 — `HubReturnTrigger` / disconnect handler | Known bug: `orientation:F3` string format |

---

## Proposed Folder Restructure (optional, non-breaking)

The current layout (`Combat/`, `Systems/`, `UI/`, `Scene/`, `NPC/`, `Player/`) maps loosely to these parts but blurs the lines — `Systems/` is a catch-all that spans Parts 3, 4, and 5.

Proposed target structure (migrate gradually; update `.meta` files in place):

```
Assets/Game/
  _Core/                  ← Part 1: PlayerIdentity, AuthManager, NetworkSetup
  Combat/                 ← Part 2: EnemyDeathHandler, WaveSpawner, AbilityCaster, FloatingDamageText
  Loot/                   ← Part 3: WorldItem, EnemyDropTable, ItemCatalogManager
  Inventory/              ← Part 4: InventoryManager, HeroCosmeticApplier
  Progression/            ← Part 5: CharacterStats, ProgressionManager, HeroMasteryManager,
                          │          CombatSessionTracker
  UI/                     ← Part 6: All HUD + panel scripts (data-neutral)
  Scene/                  ← Part 7: ArenaPortalTrigger, HubReturnTrigger
  NPC/                    ← Part 7 (sub): NPCInteractionManager, HangmanNPC, HangmanDialogueUI
```

**Migration safety rules:**
1. Move one folder at a time. Compile and test after each move.
2. Unity `.meta` files travel with their script — never move a `.cs` without its `.meta`.
3. Any `[SerializeField]` references in the Inspector will survive a folder move as long as the GUIDs in `.meta` files are unchanged.
4. `using` namespace declarations don't need to change (Unity doesn't enforce namespace = folder path).

---

## Handoff Template for AI Agents / Developers

When scoping a task to one part, use this briefing block:

```
Part: <name>
Scope: <list the specific .cs files in scope>
Off-limits: <parts not to touch>
Server endpoints involved: <list>
DB tables involved: <list>
Entry point for this task: <the one method/class where work begins>
Success test: <the exact in-game action + expected result>
```

**Example — "Loot not persisting after pickup":**
```
Part: 3 — Loot & World Items
Scope: WorldItem.cs, InventoryManager.cs
Off-limits: Parts 1, 2, 4, 5, 6, 7
Server endpoints: POST /api/inventory/save
DB tables: inventory
Entry point: WorldItem.OnTriggerEnter() → the POST call
Success test: Pick up a copper_shard in arena → open bag → item appears → return to hub → relog → item still there
```

---

*Generated: 2026-06-30 | Crossworlds BCE Phase 1 | Based on Unity client structure + auth server API*
