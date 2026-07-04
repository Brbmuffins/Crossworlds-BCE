# Crossworlds BCE — Phase 3 Design Document

Status: Design only — do not build until Phase 2 playtest complete  
Author: CrossWorlds / Claude  
Last updated: 2026-06-28

---

## Design Philosophy

Phase 2 deepens the player (talents, guilds, quests). Phase 3 deepens the world.

The throughline: **every Phase 3 system should make the combat feel like it matters at a scale larger than a single arena run.** World bosses are a community event. Dungeons are a curated challenge. PvP is a proving ground. The status effect engine v2 gives all of it more texture.

---

## 3A — World Boss System

### Concept

A world boss spawns on a schedule (or manually by a GM) in a shared instance that any online player can join. It is not instanced per team — everyone fights the same boss at the same time. The Null Architect already exists as a 4-phase boss (`WorldBossController.cs`). Phase 3 expands this to a full roster with a spawn calendar.

### Boss Roster (initial 3)

**The Null Architect** *(already implemented)*  
Phase 1: melee + reflect pulse. Phase 2: tether web + void drain. Phase 3: +25% damage taken. Final Surge: 3× speed/attack.  
Recommended: 4–10 players. Drops: Architect's Fragment (crafting material for tier-2 gear).

**The Iron Warden** *(new)*  
A corrupted construct boss. Three active turret arms — each must be destroyed before the core takes damage. Arms respawn at 50% and 20% HP. Targets whichever player has lowest Threat stacks (punishes passive play — Ironclad shines here).  
Mechanics: Magnetize pulse (pulls all players together at 70% HP), Rampart Wall splits arena in two.  
Drops: Warden Core Shard (Warden-specific talent upgrade material).

**The Void Herald** *(new)*  
A Shadowblade-class boss. Alternates between visible and Shadow Veil stealth phases. During stealth, it curses all players with escalating Corruption stacks. Players must use Silence Ward or Dispel to break the stealth early.  
Mechanics: Dark Harvest on the player with highest debuff count at phase end. Instakill avoided only by Temporal Grace.  
Drops: Void Essence (Shadowblade-specific + Arcanist crafting material).

### Schema

```sql
world_boss_spawns — id, boss_id VARCHAR(64), scheduled_at TIMESTAMP, started_at TIMESTAMP NULL,
                    killed_at TIMESTAMP NULL, kill_count INT DEFAULT 0
world_boss_kills  — id, spawn_id FK, character_id FK, damage_dealt INT, killing_blow TINYINT,
                    reward_item_id VARCHAR(64) NULL, killed_at TIMESTAMP
```

### Endpoints

```
GET  /api/bosses/schedule          — upcoming boss spawns (public)
GET  /api/bosses/active            — currently active boss + HP% (polling, 2s interval)
POST /api/bosses/register-kill     — JWT, {characterId, spawnId, damageDelta, killingBlow}
```

### Unity Integration

- `WorldBossHUD.cs` (already have `WorldBossHealthBar.cs`) — extend with: timer to next spawn, "BOSS ACTIVE" alert banner, join portal activation
- `WorldBossJoinPortal.cs` — special portal in Hub that activates when boss is live, `ServerChangeScene("World_Boss_Arena")`
- Dashboard widget shows kill count, top damage dealers, time to next spawn

### Balance Notes

- Boss HP scales with number of players in the instance (base × 0.4 per player, min 1 player)
- Drops guaranteed on first kill per week per character (once-per-week flag in `world_boss_kills`)
- Second kill same week: reduced drop chance (20%)
- GM can manually trigger a spawn from dashboard: `POST /api/gm/boss/spawn`

---

## 3B — Dungeon Instance System

### Concept

Dungeons are private instances for 3–5 players. Each dungeon is a linear sequence: **Entry → Room 1 → Event → Room 2 → Boss Room**. Rooms are pre-built scenes. The server tracks progression state and unlocks doors between rooms only when a room's condition is met (all enemies dead, or a puzzle solved).

Unlike arenas (wave-based, infinite), dungeons are finite with a guaranteed endpoint and curated narrative context.

### Initial Dungeon: The Copper Vaults

*A collapsed dwarven mine overrun by corrupted constructs. 3 rooms + final boss.*

| Room | Type | Condition to clear |
|---|---|---|
| Entry Hall | Tutorial wave | Survive 2 waves of Grunt enemies |
| Vault Corridor | Puzzle + enemies | Activate 3 rune switches while enemies spawn |
| The Forge Chamber | Elite gauntlet | Kill Iron Warden mini (50% HP version) |
| Core Room | Boss | Defeat Vault Architect (scaled down Null Architect) |

Loot at end: guaranteed `material_copper_bar ×3` + rare roll for `ring_copper` or `vault_key_fragment` (Phase 3 crafting).

### Schema

```sql
dungeon_definitions — id VARCHAR(64) PK, name, min_players INT, max_players INT,
                      min_level INT, scene_sequence JSON  -- ["Dungeon_Copper_01","Dungeon_Copper_02",...]

dungeon_runs        — id, dungeon_id FK, leader_character_id FK, started_at TIMESTAMP,
                      completed_at TIMESTAMP NULL, player_count INT, current_room INT DEFAULT 0

dungeon_participants — run_id FK, character_id FK, joined_at TIMESTAMP,
                       damage_dealt INT DEFAULT 0, PRIMARY KEY(run_id, character_id)

dungeon_rewards     — run_id FK, character_id FK, item_id FK, awarded_at TIMESTAMP
```

### Server Logic

```
POST /api/dungeons/start        — JWT, {characterId, dungeonId} — creates run, leader joins
POST /api/dungeons/join         — JWT, {characterId, runId} — joins existing run
POST /api/dungeons/advance-room — JWT, {characterId, runId} — leader only, marks room complete
POST /api/dungeons/complete     — JWT, {characterId, runId} — awards loot to all participants
GET  /api/dungeons/runs/active  — JWT, {characterId} — find joinable runs near you
```

### Unity Integration

- `DungeonManager.cs` — NetworkBehaviour, server-authoritative room state. Listens for room-clear events from WaveSpawner. Unlocks next room door (enables `PortalTransition` on the door object).
- `DungeonHUD.cs` — shows current room name, "Room 2 / 4", remaining enemies, time elapsed
- `DungeonLobbyUI.cs` — Pre-dungeon lobby. Leader selects dungeon, others join via code. Shows player list + classes. "Enter Dungeon" button disabled until min_players met.
- Each dungeon room is a separate Unity scene, loaded additively or via `ServerChangeScene`

### Design Rules

- If a player disconnects mid-dungeon, their slot is held for 2 minutes (reconnect grace)
- Dungeon runs expire after 45 minutes (abandoned run flag in DB)
- No respawn mid-dungeon — if all players down, run fails. Temporal Grace is the safety valve.
- Dungeons are the primary sink for crafting materials. No dungeon = limited high-tier gear.

---

## 3C — PvP Arena

### Concept

Opt-in ranked 1v1 duels and 3v3 skirmishes. Players queue from the Hub. Matchmaking is ELO-based. All combat is the same ability system — no PvP-specific balance changes in Phase 3 (balance tuning in Phase 4 if needed).

The point: prove your build. PvP is where talent tree choices get tested head-to-head.

### Format

**Ranked Duel (1v1)**  
Best of 3 rounds. Round ends when one player reaches 0 HP. No respawn. Abilities fully reset between rounds. 3-second invulnerability at round start.

**Skirmish (3v3)**  
Single elimination round. First team with all 3 players downed loses. Team composition visible before match starts (but not talent choices).

### Schema

```sql
pvp_ratings       — character_id FK PK, elo_duel INT DEFAULT 1000,
                    elo_skirmish INT DEFAULT 1000, wins INT DEFAULT 0,
                    losses INT DEFAULT 0, last_match TIMESTAMP NULL

pvp_matches       — id, format ENUM(duel, skirmish), started_at TIMESTAMP,
                    completed_at TIMESTAMP NULL, winner_character_id INT NULL

pvp_participants  — match_id FK, character_id FK, team INT, result ENUM(win,loss,draw),
                    damage_dealt INT, healing_done INT, PRIMARY KEY(match_id, character_id)
```

### ELO System

Standard ELO, K-factor 32 for first 10 matches (placement), K-factor 16 thereafter.

```
POST /api/pvp/queue          — JWT, {characterId, format} — join matchmaking queue
DELETE /api/pvp/queue        — JWT, {characterId} — leave queue
GET  /api/pvp/match/active   — JWT, {characterId} — current match state
POST /api/pvp/match/result   — server-only, called by game server after match
GET  /api/pvp/leaderboard    — public, ?format=duel&limit=20
```

### Unity Integration

- `PvPQueueUI.cs` — Hub button "Enter PvP". Shows current ELO, queue status, estimated wait
- `PvPMatchHUD.cs` — during match: round counter, both player HP bars (opponent shown large at top), ELO delta preview
- Spectator mode: Mirror observer system, any queued player can spectate active matches

### Season System

Seasons run 8 weeks. Season rewards at end based on peak ELO bracket:
- Bronze (1000–1199): title "Challenger"
- Silver (1200–1399): title "Contender" + cosmetic color palette
- Gold (1400+): title "Warden of the Void" + exclusive armor skin (cosmetic only)

Season data stored in `pvp_ratings` extended with `season_id INT`, `peak_elo INT`.

---

## 3D — Status Effect Engine v2

### Current State (Phase 1)

6 effects: Slow, Stagger, Silenced, Cursed, Weakened, Bound  
Single `StatusEffectManager` component, `RemoveAll()` clears everything.

### Phase 3 Expansion: 20+ Effects

The goal is to make the status layer legible and deep — players should be able to read the battlefield from debuff icons and make decisions accordingly.

### New Effect Types

**Offensive (applied to enemies)**

| Effect | Mechanic | Primary Source |
|---|---|---|
| Burning | DoT, 5 dmg/s, fire damage type | Ember Surge (upgraded via talent) |
| Frozen | Cannot move or act for 2s, then Slow for 3s | New Arcanist ability (Phase 3) |
| Poisoned | DoT, 3 dmg/s, stacks up to 5× | New Shadowblade ability |
| Stunned | Full interrupt, 1.5s, breaks on damage | Shield Bash (upgrade path) |
| Blinded | Miss chance 40% on next 3 attacks | New shared ability |
| Exposed | Incoming physical damage +15% (separate from Weakened) | Iron Rampart AoE on hit |
| Marked | Visible through walls, revealed to allies | New Warden ability |

**Defensive (applied to allies)**

| Effect | Mechanic | Primary Source |
|---|---|---|
| Hasted | Move speed +25% for 4s | Temporal Grace area (upgrade) |
| Fortified | DR +15% stacking up to 3× | New Ironclad stance |
| Regenerating | +8 HP/s for 6s | Sacred Aegis on break |
| Focused | Next ability deals +20% damage | Phase Charge visual buff state |
| Warded | Immune to CC for 2s | Dispel upgrade (talent) |

### Implementation Changes

**`StatusEffectType` enum expansion** — add all new types to `StatusEffect.cs`

**`StatusEffectManager` v2 changes:**
- Split `RemoveAll()` into `RemoveDebuffs()` and `RemoveBuffs()` — Dispel should only cleanse debuffs
- Add `ConsumeDebuffStacks()` — already exists for Dark Harvest, extend to count by type
- Add `GetEffect(type)` — query single effect
- Add `IsImmune(type)` — Warded check before applying

**`StatusEffectHUD.cs`** — new script:
- Self-bootstrapping row of effect icons below player HP bar
- Each icon: colored border (red=debuff, green=buff), effect name, remaining duration bar
- Maximum 8 visible at once — prioritize by remaining duration

**Effect interaction rules:**
- Frozen breaks on any damage (like real CC break)
- Weakened + Exposed stack multiplicatively: 1.25 × 1.15 = 43.75% more damage taken
- Warded blocks new CC applications but doesn't remove existing ones
- Burning + Poisoned stack independently (different damage types)

### Schema Addition

No new DB tables needed — status effects are ephemeral (runtime only). The expansion is pure C# code.

---

## 3E — World Expansion

### Hub Districts

Current Hub is a single 200×200 plane. Phase 3 adds districts accessible via sub-portals in the Hub.

**Market District** — connects to Marketplace NPC cluster. Auction board, item inspection, trade brokers.  
**Training Grounds** — 1v1 practice duels with AI dummies. Talent tree testing without queue pressure.  
**Guild Hall Quarter** — private guild hall instances. Members-only space with storage chest and battle board (shared quest log).  
**The Void Gate** — PvP queue portal. Glows when your bracket has active matches.

### New Arena Biomes (Phase 3 Dungeon Scenes)

| Biome | Theme | Environmental Hazard |
|---|---|---|
| Volcanic Forge | Lava vents, copper deposits | Lava pools deal 10 dmg/s, erupt every 30s |
| Crystal Caverns | Frozen underground lake | Ice patches apply Slow; breaking crystals knocks back |
| Shadow Realm | Void corruption, zero ambient light | Torches must be lit or enemies are invisible; dark zones silence |

### Dynamic Time-of-Day

Server-side clock (real time, 24h → 1h in-game day). Time affects:
- Hub ambient lighting (Unity RealTimeLighting or baked sky swap)
- Night: spawn rates in outdoor zones +20%, rare enemy variants appear
- Dawn: vendors open, profession XP bonus for 15 real minutes
- No forced downtime — players online at all hours should still have content

---

## Phase 3 Priority Order

Build in this sequence — each system depends on the previous being stable:

1. **Status Effect Engine v2** — foundational, unblocks boss/dungeon design
2. **World Boss System** — highest community impact, simplest Unity integration
3. **Dungeon Instance System** — most complex, needs status engine + wave system solid
4. **PvP Arena** — needs matchmaking server, separate from main auth server ideally
5. **World Expansion** — art-heavy, needs all gameplay systems locked first

---

## What Phase 3 Is NOT

- Not a new monetization layer (that's Phase 4 if ever)
- Not mobile port
- Not cross-server play
- Not mounts/pets (cosmetic systems are Phase 4)
- Not voice chat integration
- Not open world (all content stays instanced for Phase 3)
