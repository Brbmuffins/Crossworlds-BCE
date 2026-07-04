# Crossworlds BCE — Context Index

This folder is the source of truth for design, architecture, and feature specs.
Load the relevant file before making changes in any area.

Last updated: 2026-06-30

---

## Quick Reference

| File | Load when... |
|---|---|
| [COMBAT.md](COMBAT.md) | Designing abilities, combat feel, enemy AI, damage, classes |
| [NETWORKING.md](NETWORKING.md) | Mirror setup, KCP transport, SyncVar, Commands, RPCs |
| [AUTH_LOGIN.md](AUTH_LOGIN.md) | JWT flow, login/register, character spawn, auth server |
| [UI_INPUT.md](UI_INPUT.md) | Unity Input System, UI scripts, HUD, key bindings |
| [SCENE_SETUP.md](SCENE_SETUP.md) | Scene order, build settings, editor automation menu |
| [VPS_SERVER.md](VPS_SERVER.md) | SSH, systemd services, Nginx, logs, deploy steps |
| [COMBAT_ATLAS.md](../COMBAT_ATLAS.md) | Full ability data — all 32 spells, real stats, synergies |
| [PHASE3_DESIGN.md](PHASE3_DESIGN.md) | Phase 3 systems — World Bosses, Dungeons, PvP, Status v2 |
| [RETENTION_DESIGN.md](RETENTION_DESIGN.md) | Daily login, daily quests, weekly challenges, seasons |
| [HEALING_DESIGN.md](HEALING_DESIGN.md) | Cleric feel, healing feedback scripts, sound intent |
| [HERO_MASTERY_DESIGN.md](HERO_MASTERY_DESIGN.md) | Smite-style per-hero mastery — XP, levels 1–10, rewards, skins |
| [COSMETICS_DESIGN.md](COSMETICS_DESIGN.md) | Essence drops, skin crafting, color palettes, trail/VFX cosmetics |

---

## Feature Status by Phase

### Phase 1 — Complete ✅
Core loop is live: login → character select → Hub → portal → arena → loot → crafting.

| System | Server | Unity Client |
|---|---|---|
| Auth, login, JWT | ✅ | ✅ |
| 5 heroes, class prefabs | ✅ | ✅ |
| Mirror networking, KCP | ✅ | ✅ |
| Hub scene, portals | ✅ | ✅ |
| Combat (Health, enemies, waves) | ✅ | ✅ |
| Loot, WorldItem, DropTable | ✅ | ✅ |
| Inventory bag UI | ✅ | ✅ |
| Progression (XP, level, gold) | ✅ | ✅ |
| Crafting (recipes, professions) | ✅ | ✅ |
| Forge NPC + Mining nodes | ✅ | ✅ |
| XP bar, CharacterSheet, LevelUp screen | — | ✅ |
| AbilityHUD, FloatingDamageText, EnemyHealthBar | — | ✅ |
| ArenaClearUI, WaveHUD, WorldBossHealthBar | — | ✅ |
| Login branding (playcrossworlds.com) | — | ✅ |
| GmConsole, EscMenu, Chat, PlayerListUI | ✅ | ✅ |

### Phase 2 — Server Complete ✅ / Unity Pending 🔶

**VPS Session A — COMPLETE (9 new tables, 17 new endpoints, 2 bug fixes)**
- `talents` — 75 rows seeded (5 heroes × 3 branches × 5 tiers)
- `character_talents` — points invested, max 15 total
- `guilds` — tag + motd columns added
- `guild_chat_log` — cascade-deletes on guild drop
- `quests` — 10 starter quests (3 kill, 3 gather, 2 explore, 2 mixed)
- `character_quests` — active → complete flow with completed_at
- `combat_sessions` — per-session archive
- `character_combat_stats` — lifetime totals + best runs (upsert)
- `hero_mastery` — 10-level XP ladder per hero per character
- `/combat-atlas.html` → 301 redirects to `/combat/` (full pentagon atlas)

**Unity scripts migrated to Assets/Game/ (2026-06-30):**
- `TalentTreeUI.cs`, `TalentModifierApplier.cs` → `Assets/Game/UI/` + `Characters/Scripts/`
- `GuildPanelUI.cs`, `QuestLogUI.cs`, `QuestTracker.cs`, `OnlinePlayersHUD.cs`, `InventoryManager.cs` → `Assets/Game/UI/`
- `Phase2Builder.cs` → `Assets/Game/Editor/`
- `_scripts/` staging folder is now stale — safe to delete from project root

**Combat + Healing scripts (written by Session B — locate in session output):**
- `FloatingDamageText.cs` extended, `SoulBondTether.cs`, `ShieldValueHUD.cs`
- `ClericRadarUI.cs`, `StatusEffectHUD.cs`, `CombatSessionTracker.cs`, `ClassPoolBuilder.cs`

| System | Server | Unity Client |
|---|---|---|
| **Talent Trees** | ✅ | ✅ TalentTreeUI.cs, TalentModifierApplier.cs in Assets/Game (wire-up pending) |
| **Guild System** | ✅ | ✅ GuildPanelUI.cs in Assets/Game (wire-up pending) |
| **Quest System** | ✅ | ✅ QuestLogUI.cs, QuestTracker.cs in Assets/Game (wire-up pending) |
| **Combat Stats + Leaderboards** | ✅ | ✅ CombatSessionTracker |
| **Hero Mastery** | ✅ | ✅ HeroMasteryManager, HeroMasteryHUD, HeroMasteryUI |
| **Healing Feel Scripts** | — | ✅ Written |
| **Cosmetics (essence items + recipes)** | 🔶 SQL pending seed | ✅ HeroCosmeticApplier.cs wired |
| **NPC Interaction System** | — | ✅ INPCInteractable, NPCInteractionManager, HangmanNPC, HangmanDialogueUI |
| **Scene Triggers** | — | ✅ ArenaPortalTrigger, HubReturnTrigger |
| **Inventory / Items** | ✅ | ✅ InventoryManager, ItemCatalogManager, ItemTooltipUI, InventoryBagUI |
| **Progression / Stats HUD** | ✅ | ✅ PlayerProgressManager, XpBar, GoldHUD, CharacterStatsHUD |
| **Marketplace** | ○ | ○ |
| **Daily Login Reward** | ○ | ○ |
| **Daily Quests** | ○ | ○ |
| **Weekly Challenge + Milestone** | ○ | ○ |

### Phase 3 — Design Only ○
*Do not build until Phase 2 playtest complete. See [PHASE3_DESIGN.md](PHASE3_DESIGN.md).*

| System | Notes |
|---|---|
| Status Effect Engine v2 | 20+ effects, stack/type split, StatusEffectHUD |
| World Boss Roster | Null Architect (exists), Iron Warden, Void Herald |
| Dungeon Instances | 3–5 player, room-by-room, Copper Vaults first |
| PvP Arena | 1v1 ranked duel, 3v3 skirmish, ELO, seasons |
| World Expansion | Hub districts, new biomes, time-of-day |

---

## Hero Mastery System (Smite-style) — Design Note

Each hero has an independent mastery track separate from account level. Playing a hero earns Mastery XP toward that hero's mastery level (cap: 10). Higher mastery unlocks:

- **Mastery 2:** Alternate ability skin (color palette shift on cast VFX)
- **Mastery 5:** Mastery border on nameplate + class-specific title
- **Mastery 8:** Stat bonus: +5% damage/healing for that hero only (minor — not pay-to-win)
- **Mastery 10:** "Master [ClassName]" title + cosmetic aura

Schema:
```sql
hero_mastery — character_id FK, hero_class INT, mastery_xp INT DEFAULT 0,
               mastery_level INT DEFAULT 0,
               PRIMARY KEY(character_id, hero_class)
```

Mastery XP sources: arena kills (1/kill), dungeon completion (50), world boss damage (variable), PvP wins (25).

---

## Open Bugs — Track Until Closed

| # | Bug | File | Status |
|---|---|---|---|
| 1 | `orientation:F3` — float as string in position PATCH | Unity RodPositionSaver | Open |
| 2 | `GmConsole.cs` — server build spam (missing `#if !UNITY_SERVER`) | GmConsole.cs | ✅ Fixed — full file wrapped in guard |
| 3 | `CmdSendChat` — missing `[CHAT]` server log | RodChatManager.cs | Open |
| 4 | Enemy prefabs — capsule placeholders, real meshes needed | Prefabs/ | Open |
| 5 | `NetworkManager.spawnPrefabs` — must manually add enemies + WorldItem | Inspector | Open |
| 6 | Class abilities not fully wired per hero in AbilityCaster | AbilityCaster.cs | Open |
| 7 | **Wire-up needed:** call `CombatSessionTracker.Local.NotifyEnemySpawned(go)` from `WaveSpawner` on each enemy spawn, and `NotifyAllySpawned(go)` from player spawn path | WaveSpawner.cs + RodNetworkManager.cs | Open |

---

## Key File Locations

```
Unity project root:      D:\Crossworlds\Assets\Game\
Combat scripts:          D:\Crossworlds\Assets\Game\Combat\Scripts\
UI scripts:              D:\Crossworlds\Assets\Game\UI\
Character scripts:       D:\Crossworlds\Assets\Game\Characters\Scripts\
Editor automation:       D:\Crossworlds\Assets\Game\Editor\
Design docs:             D:\Crossworlds\CrossWorlds\_context\
Roadmap:                 D:\Crossworlds\ROADMAP.md
Combat spec:             D:\Crossworlds\COMBAT_ATLAS.md

VPS auth server:         /opt/rod-auth/server.js
VPS dashboard:           /opt/rod-dashboard/server.js
VPS web root:            /var/www/rod/
Combat atlas (web):      /var/www/crossworlds/combat/index.html
Game binary:             /game/Builds/CrossworldsBCE.x86_64
Logs:                    /var/log/crossworlds.log
```
