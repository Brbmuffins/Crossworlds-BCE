# Crossworlds BCE — Roadmap

Last updated: 2026-06-28  
Hero-class pivot: classes renamed to heroes — Warden, Ironclad, Shadowblade, Cleric, Arcanist

---

## Legend

| Symbol | Meaning |
|---|---|
| ✅ | Done — server + client |
| 🟦 | Done this sprint |
| 🔶 | In progress / partial |
| ○ | Not started |

---

## Week 1 — Foundation ✅

- [x] Mirror networking, KCP transport, UDP 7777
- [x] JWT auth flow — register, login, character create
- [x] RodNetworkAuthenticator — JWT verify → character fetch → spawn
- [x] RodNetworkManager — class prefab spawn, saved position
- [x] LoginScene + CharacterSelect scenes
- [x] Hub scene — environment, portals, spawn points
- [x] RodChatManager — networked chat, fade, input
- [x] EscMenu, PlayerListUI, PlayerNameplate, GmConsole
- [x] BCE editor menu — steps 0–5, Build Hub Scene

---

## Week 2 — Portal System 🔶

- [x] Server: portal/arena scene infrastructure (scene list, build settings)
- [x] `PortalTransition.cs` — E to enter, `ServerChangeScene`, billboard prompt
- [ ] Arena scene — needs NavMesh baked, WaveSpawner placed, added to Build Settings
- [ ] Portal prefabs added to `NetworkManager.spawnPrefabs`

---

## Week 3 — Combat / Classes 🔶

- [x] `Health.cs` — authoritative HP, UnityEvents (onDeath, onDamageTaken, onHealthChanged, onKilledBy, onHealApplied, onDownedChanged)
- [x] `StatusEffect.cs` + `StatusEffectManager.cs` — Slow, Stagger, Silenced, Cursed, Weakened, Bound
- [x] `EnemyController.cs` — Idle/Chase/Attack/Dead FSM, leash, status-gated
- [x] `EnemyProjectile.cs` — server-spawned ranged projectile
- [x] BCE `4a–4c` — Grunt / Ranged / Elite prefabs + DropTables
- [ ] Class abilities — `AbilityCaster.cs` exists (32 abilities, 5 classes) but per-class wiring incomplete
- [ ] Enemy animations — capsule placeholders, real meshes not assigned

---

## Week 4 — Loot + Inventory 🔶

- [x] `DropTable.cs` — weighted item+gold rolls, ScriptableObject
- [x] `WorldItem.cs` — floor loot pickup, network-synced, 90s despawn
- [x] `WaveSpawner.cs` — server-authoritative, escalating waves, elite every N
- [x] BCE `4d` — WorldItem prefab
- [x] BCE `4e` — WaveSpawner + spawn points in scene
- [x] Server: `GET /api/inventory/:characterId`, `POST /api/inventory/save`, `POST /api/inventory/equip`, `POST /api/inventory/add-item`
- [x] `InventoryBagUI.cs` — B key, 4×6 slot grid, equip/unequip, fetches API
- [x] `PlayerIdentity.characterId` SyncVar — wired through auth → spawn
- [ ] **NetworkManager spawnPrefabs** — Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem must be manually added in Inspector
- [ ] Item icons / sprite atlas — slots show color-coded placeholder, no real icons yet
- [ ] Loot pickup feedback — no sound/VFX on WorldItem collect

---

## Week 5 — Progression ✅

- [x] Server: `POST /api/character/save-progress` — level, xp, gold, str/agi/int/vit
- [x] `PlayerProgressManager.cs` — fetches at login, saves on level-up/scene exit, `AwardXp()` / `AwardGold()`, `ClassIndex`
- [x] `XpBar.cs` — bottom-centre HUD, smooth fill, level-up gold flash + label punch
- [x] `CharacterSheetUI.cs` — C key panel, level/xp/gold + str/agi/int/vit stat block
- [x] `LevelUpScreen.cs` — full-screen class-coloured flash + "LEVEL UP!" burst
- [x] XP/gold awards wired — EnemyController death → RpcAwardProgress → PlayerProgressManager.Local

---

## Week 6 — Crafting 🟦

- [x] Server: `GET /api/recipes?profession=mining`, `POST /api/craft`
- [x] Server: `professions`, `recipes`, `recipe_ingredients` tables seeded
- [x] `CraftingUI.cs` — F key panel, scrollable recipe list, ingredient display, POST /api/craft, auto-refreshes inventory
- [x] `ForgeNPC.cs` — Hub NPC, proximity E-key trigger, opens CraftingUI, billboard prompt, throttled player scan
- [x] `ResourceNode.cs` — Mining node, F-key interaction, POST /api/inventory/add-item, profession XP, respawn timer, billboard prompt
- [x] `HubSceneBuilder.cs` step 8 — `BCE/Hub Setup/8 - Add Forge and Mining NPCs` adds Forge Master at (-12,0,-4) + 3 Copper Ore nodes
- [ ] Mining portal — dedicated sub-scene or trigger zone with ore cluster
- [ ] Profession XP persisted — crafting/mining actions need `save-progress` with skill_xp

---

## Week 7 — Polish 🟦

- [x] `FloatingDamageText.cs` — pooled world-space damage numbers, pool of 32, crit flash
- [x] `EnemyHealthBar.cs` — world-space slim bar above each enemy; colour shifts red→orange→yellow
- [x] `AbilityHUD.cs` — self-bootstrapping 4+1 bottom-right icon strip with radial cooldown overlay
- [x] `WorldBossHealthBar.cs` — full-width boss HP bar
- [x] `ArenaClearUI.cs` — wave-complete banner, loot summary overlay
- [x] `EnemyDeathVFX.cs` — procedural crimson burst fallback when no VFX prefab assigned; auto-destroys in 2.5s
- [x] `LoginManager.cs` — branding polish: "FORGE YOUR LEGEND. DEFY THE VOID.", panel "ENTER THE VOID", site icon fetch
- [x] `combat-atlas.html` — full interactive combat system: all 32 abilities, 5 hero tabs, 21 synergy pairs, build bar, hostable HTML
- [ ] Hit VFX — impact sparks/slash on melee connect
- [ ] SFX — footsteps, ability sounds, hit sounds, death sounds
- [ ] Ability icons — placeholder colored squares; real icon atlas needed

---

## Week 8 — Playtest ○

- [ ] Invite 10–20 testers
- [ ] Stress test Mirror under concurrent connections
- [ ] Collect feedback → Phase 2 priority vote
- [ ] Fix top reported bugs
- [ ] Build + deploy stable alpha build

---

## Phase 2 — Current Sprint Progress 🔶

*Session A (VPS/server) and Unity client session running in parallel. Last updated: 2026-06-28.*

### Server (Session A — VPS) ✅ COMPLETE
- [x] Talent tree schema (75 nodes), `GET /api/talents/tree/:heroClass`, `GET /api/talents/:characterId`, `POST /api/talents/invest`, `POST /api/talents/respec` (costs 100g)
- [x] Guild schema, `POST /api/guilds/create` (costs 500g), invite, leave, `GET /api/guilds/:id`, `PATCH /api/guilds/motd`
- [x] Quest schema (10 starter quests), `GET /api/quests/available`, `POST /api/quests/accept`, `POST /api/quests/progress`, `POST /api/quests/complete` (atomic XP+gold+item award)
- [x] `combat_sessions` + `character_combat_stats` — `POST /api/combat/session/end` (archive + upsert + gold + mastery XP in one tx), `POST /api/combat/death`, `GET /api/combat/stats/:characterId` (KDA + last 5 sessions)
- [x] Leaderboards: `GET /api/leaderboard/damage`, `/healing`, `/waves`, `/mastery?heroClass=N` (all public)
- [x] Hero mastery: `GET /api/mastery/:characterId` (auto-creates 5 rows), `POST /api/mastery/award`
- [x] `/combat-atlas.html` → 301 redirect to `/combat/` (full pentagon atlas)

### Unity Client — Phase 2 Systems (staged in `CrossWorlds/_scripts/` — copy to Assets)
- [x] `TalentTreeUI.cs` + `TalentModifierApplier.cs` — talent panel + stat pipeline
- [x] `GuildPanelUI.cs` — G key guild panel
- [x] `QuestLogUI.cs` + `QuestTracker.cs` — Q key log + HUD tracker
- [x] `OnlinePlayersHUD.cs` — online player count
- [x] `InventoryManager.cs` — extended inventory
- [x] `Phase2Builder.cs` — BCE editor menu for Phase 2 setup

**Copy destinations:** UI scripts → `Assets/Game/UI/` | `Phase2Builder.cs` → `Assets/Game/Editor/` | `TalentModifierApplier.cs` → `Assets/Game/Characters/Scripts/`

### Unity Client — Combat & Healing Feel (written by Session B)
- [x] `FloatingDamageText.cs` — extended with `DamageType.Heal`, `HealCrit`, `Shield`, `TriageReturn`
- [x] `SoulBondTether.cs` — LineRenderer amber tether between Cleric and bonded ally
- [x] `ShieldValueHUD.cs` — world-space shield bar above shielded ally
- [x] `ClericRadarUI.cs` — low-HP ally radar, pulsing outlines, portrait widget (Cleric-only)
- [x] `StatusEffectHUD.cs` — 6-icon row showing active status effects + timers
- [x] `CombatSessionTracker.cs` — session damage/healing/kill tracking, POST /api/combat/session/end
- [x] `ClassPoolBuilder.cs` — editor tool to generate ClassAbilityPool ScriptableObjects for all 5 heroes

⚠️ **Wire-up needed:** `WaveSpawner.cs` → call `CombatSessionTracker.Local.NotifyEnemySpawned(go)` on each enemy spawn. `RodNetworkManager.cs` → call `NotifyAllySpawned(go)` from player spawn path.

### Design Docs Written (all in `_context/`)
- [x] `PHASE3_DESIGN.md` — World Bosses, Dungeons, PvP, Status v2
- [x] `RETENTION_DESIGN.md` — Daily/weekly rewards, season system
- [x] `HEALING_DESIGN.md` — Cleric feel, tether, radar, shield HUD
- [x] `HERO_MASTERY_DESIGN.md` — Smite-style per-hero mastery, schema, endpoints
- [x] `COSMETICS_DESIGN.md` — Essence drops, skin crafting SQL, color palettes

---

## Phase 2 — Guild & Talent Era (Post-Playtest)

Schema stubs exist in DB. Build order determined by playtest vote. Estimated 6–8 weeks.

### 2A — Talent Trees (~2 weeks)
- Per-hero talent web: 3 branches × 5 tiers per hero, 75 total nodes across all 5 heroes
- `talents` table: `id, hero_class, branch, tier, name, description, modifier_json`
- `character_talents` table: `character_id, talent_id, points_spent`
- Server: `GET /api/talents/:characterId`, `POST /api/talents/invest`
- Unity: `TalentTreeUI.cs` — interactive web panel inspired by combat-atlas.html, in-game
- Modifier pipeline: talents feed stat multipliers into `AbilityCaster` and `Health`
- Respec cost: gold-gated, cooldown 24h real-time

### 2B — Guild System (~2 weeks)
- `guilds` table already exists: `id, name, tag, leader_id, motd, created_at`
- `guild_members` table: `guild_id, character_id, rank, joined_at`
- Server: `POST /api/guilds/create`, `POST /api/guilds/invite`, `GET /api/guilds/:id`, `POST /api/guilds/leave`
- Unity: `GuildPanelUI.cs` — G key, member roster, MOTD editor, invite flow
- Guild chat channel — `[GUILD]` prefix in RodChatManager, filtered by guild_id
- Guild hall hub instance — private server scene for guild members

### 2C — Quest System (~2 weeks)
- `quests` table: `id, name, type(kill/gather/escort/explore), target_id, target_count, xp_reward, gold_reward, item_reward_id`
- `character_quests` table: `character_id, quest_id, status(active/complete/failed), progress`
- Server: `GET /api/quests/available/:characterId`, `POST /api/quests/accept`, `POST /api/quests/progress`
- Unity: `QuestLogUI.cs` — Q key, active/available/complete tabs, progress bars
- `QuestTracker.cs` — self-bootstrapping HUD widget, shows active quest name + progress
- Quest hooks into WaveSpawner (kill quests), ResourceNode (gather quests)

### 2D — Marketplace (~1 week)
- `marketplace_listings` table already exists: `id, seller_id, item_id, quantity, price, listed_at`
- Server: `GET /api/marketplace`, `POST /api/marketplace/list`, `POST /api/marketplace/buy`
- Unity: `MarketplaceUI.cs` — M key, sortable listing grid, buy/sell flow
- Gold transaction log: `gold_transactions` table already exists

---

## Phase 3 — Endgame Systems (6+ months out)

Design intent only — do not build until Phase 2 playtest complete.

### 3A — World Bosses
- Instanced world-boss arenas, 5–10 players, timed enrage
- Boss health bar synced across all connected clients (already have `WorldBossHealthBar.cs`)
- Unique loot tables: rare crafting materials not in normal drop rotation
- `world_boss_kills` leaderboard table

### 3B — Dungeon Instances
- Persistent multi-room dungeons: room → event → boss structure
- 3–5 players, Mirror scene-per-instance isolation
- `DungeonManager.cs` server-side: tracks room progression, spawns, door unlocks
- Dungeon-exclusive recipes and gear tiers

### 3C — Status Effect Engine v2
- Phase 1 has 6 status effects (Slow, Stagger, Silenced, Cursed, Weakened, Bound)
- Phase 3 expands: 20+ effects, stackable, duration refresh, visual indicators per effect
- New effects: Burning, Frozen, Poisoned, Rooted, Stunned, Blinded, Enraged, Shielded
- `StatusEffectHUD.cs` — icon row below player health showing active effects + timers

### 3D — World Expansion
- Additional Hub districts: Market District, Guild Hall Quarter, Training Grounds
- New arena biomes: Volcanic Forge, Crystal Caverns, Shadow Realm
- Dynamic time-of-day affecting spawn rates and NPC dialogue
- Server-side world events: invasion waves, boss rush weekends

### 3E — PvP Arena
- Opt-in ranked duels (1v1) and skirmishes (3v3)
- ELO rating system: `pvp_ratings` table
- Spectator mode using Mirror's observer system
- Season rewards: exclusive cosmetic items

---

## Permanent Technical Debt

| Item | Impact |
|---|---|
| `orientation:F3` bug in `PATCH /character/position` | Medium — position save sometimes malformed |
| `GmConsole.cs` missing `#if !UNITY_SERVER` guard — spams errors server-side | Medium |
| `CmdSendChat` missing `[CHAT]` server log line | Low |
| Enemy prefabs need real meshes (currently capsule placeholders) | High for ship |
| `NetworkManager.spawnPrefabs` — must manually add Enemy_Grunt/Ranged/Elite, WorldItem | High — spawns silently fail without this |
| Ability icons — color squares only, no real sprite atlas | Medium |
| Class abilities in `AbilityCaster.cs` not fully hooked per hero | High for Week 3 |
| Arcanist missing from CharacterSelect 3D preview | Low |
| Stale prefabs: Engineer/Guardian/Wraith/Medic in Assets/Game/Prefabs/ | Low — confusing but harmless |
