<p align="center">
  <img src="Docs/logo.png" alt="Crossworlds BCE" width="280"/>
</p>

<h3 align="center">4–10 player co-op action RPG · Server-authoritative combat · 5 classes · Dodge, cast, survive</h3>

<p align="center">
  <a href="https://playcrossworlds.com"><strong>▶ Play Now (Browser)</strong></a> &nbsp;·&nbsp;
  <a href="Docs/ClassSpec.html">Class Reference</a> &nbsp;·&nbsp;
  <a href="Docs/SpellBook.html">Spellbook</a> &nbsp;·&nbsp;
  <a href="https://playcrossworlds.com">playcrossworlds.com</a>
</p>

---

## What is Crossworlds BCE?

Crossworlds BCE is a multiplayer co-op RPG built in Unity 6. Up to 10 players fight through wave-based arenas, take on a world boss, and progress their characters via loot, crafting, and class mastery. Combat is Smite-inspired: directional skill shots you have to aim, telegraphed enemy attacks you have to dodge, and a 2-charge dodge roll with full i-frames.

The backend runs 24/7 on a VPS — log in, pick a class, and drop into the Hub with anyone online.

<p align="center">
  <img src="Docs/multiplayer-chat-working.png" alt="Multiplayer in action" width="700"/>
  <br/><em>Live multiplayer — networked chat, nameplates, and party HUD</em>
</p>

---

## Classes

Five distinct classes, each with 4 equipped abilities + 1 ultimate. All abilities are shown in the [Class Reference](Docs/ClassSpec.html) and [Spellbook](Docs/SpellBook.html).

| | Class | Role | Playstyle |
|---|---|---|---|
| ⚙ | **Warden** | Tank / Engineer | Places runic traps, turrets, and constructs. Controls space. Outlasts the fight rather than winning it. |
| 🛡 | **Ironclad** | Tank / Frontline | The team's wall. Charges through enemies, raises stone walls, and absorbs hits to release as burst damage. |
| 🗡 | **Shadowblade** | Assassin / DoT | Vanishes, applies debuffs, then detonates them. Dark Harvest consumes every debuff stack on the field at once. |
| ✨ | **Cleric** | Support / Heal | Keeps the team breathing — Soul Bond reroutes damage to self, Temporal Grace rewinds the whole team 5 seconds. |
| ⚡ | **Arcanist** | Burst / Control | Teleports, pulls enemies into a singularity, then collapses it. High skill ceiling, massive payoff. |

---

## Spellbook — All 32 Abilities

> **[→ Open full Spellbook](Docs/SpellBook.html)** · **[→ Class Reference](Docs/ClassSpec.html)**

The in-game spellbook opens with **Tab** — browse all abilities, hover for descriptions, click + press 1–4 to equip.

### Shared (any class)

| Ability | Type | Description |
|---|---|---|
| **Void Bolt** | Skill Shot | Aimed bolt of void energy. Charge for 15→45 damage — must aim and dodge. |
| **Runic Sentinel** | Deploy | Stationary turret that fires at nearby enemies. |
| **Mending Circle** | AoE Heal | Heals all allies standing in a rune circle. |
| **Storm Lash** | Line | Rushing storm wall through a line — chargeable. |
| **Ember Surge** | AoE | Fire burst at target point, chargeable. |
| **Mind Spike** | AoE | Heavy single-target psychic spike. |
| **Binding Wave** | AoE | Void pulse that damages and roots enemies. |
| **Arcane Ward** | Instant | 50-absorb barrier on self, lasts 5 seconds. |

### Warden

| Ability | Type | Description |
|---|---|---|
| **Runic Snare** | Deploy | Proximity rune trap — detonates for 40 on trigger. |
| **Battle Hymn** | Aura | Reduces ally cooldowns in range. |
| **Spirit Redirect** | Command | Redirects active turret to focus target. |
| **Mend** | Single-target | Direct heal + debuff cleanse on one ally. |
| **Conjurer's Surge** *(Ult)* | Instant | All active constructs fire simultaneously at full power. |

### Ironclad

| Ability | Type | Description |
|---|---|---|
| **Counter Blow** | Absorb | 3s absorption stance — releases stored damage as 60-dmg cone. |
| **Gravity Slam** | AoE | Pulls all nearby enemies to the impact point. |
| **Shieldwall Charge** | Line | Charge forward, 25 dmg + Threat stacks per hit. |
| **Stalwart Stance** | Stance | 40% DR + 3× Threat gen for 6s. Can't move. |
| **Rune Chain** | Tether | Leashes one enemy 5s; absorbs 15% of their attacks on allies. |
| **Iron Rampart** *(Ult)* | Deploy | Full-width wall blocks all projectiles for 10s. |

### Arcanist

| Ability | Type | Description |
|---|---|---|
| **Arcane Step** | Teleport | Phase-shifts to the target point instantly. |
| **Void Maw** | Pull + AoE | 3s pull → 20 burst. |
| **Forked Lightning** | Chain | Arcs 30/25/20/15 dmg through up to 4 enemies. |
| **Collapsing Void** *(Ult)* | Pull + AoE | Massive 12u pull, 3s collapse, 60 AoE + Weakened. |

### Cleric

| Ability | Type | Description |
|---|---|---|
| **Soul Bond** | Tether | Routes target ally's incoming damage to you for 5s. |
| **Spirit Wisps** | Mobile Heal | Wisps drift toward allies and chip enemies they pass. |
| **Divine Spark** | Revive / Dmg | Revives downed ally at 30% HP, or 60 holy dmg to undead. |
| **Sacred Aegis** | Shield | Living shield grows to 80 absorb as ally takes hits. |
| **Dispel** | Instant | Purges all debuffs from a target ally. |
| **Temporal Grace** *(Ult)* | Rewind | Rewinds the whole team 5 seconds — HP, position, debuffs. |

### Shadowblade

| Ability | Type | Description |
|---|---|---|
| **Shadow Veil** | Stealth | 4s invisibility. Breaking with Mind Spike: +50% damage. |
| **Silence Ward** | Deploy | Fog field silences enemy abilities + applies Cursed DoT. |
| **Dark Harvest** *(Ult)* | Detonate | Consumes all debuff stacks on nearby enemies: 20 dmg per stack. |

---

## Combat System

**Smite-inspired, server-authoritative:**

- **Skill shots** — Void Bolt fires a traveling projectile you must aim. Thin beam indicator shows trajectory. Charge up to triple damage.
- **Dodge roll** — **Left Alt** or **V**. 2 charges, 5s recharge. Full i-frames for 0.35s. Roll direction follows movement.
- **Enemy telegraphs** — Enemies flash a red AoE indicator 0.45s before their attack lands. Dodge it.
- **AoE shapes** — Circle, Cone, Line, Skill Shot. All server-side hit detection.
- **Status effects** — Slow, Stagger, Silenced, Cursed (DoT), Weakened (+25% dmg taken), Bound (rooted).
- **Damage chain** — base dmg → Weakened ×1.25 → DR reduction → gear DR → redirect (Soul Bond) → shield absorb → HP.

---

## Features

| System | Status |
|---|---|
| Login / register / JWT auth | ✅ |
| Character select — 5 classes, 3D preview | ✅ |
| Hub world — forge, mining, crafting, inventory | ✅ |
| Wave arena — escalating waves, elite every N | ✅ |
| World boss — Null Architect, 4-phase fight | ✅ |
| Smite-style combat — skill shots, dodge, telegraphs | ✅ |
| Spellbook — 32 abilities, hover tooltips, equip | ✅ |
| Multiplayer (Mirror/KCP) — 4–10 players, server auth | ✅ |
| Enemy AI — aggro, leash, status effects, telegraphs | ✅ |
| Tripo AI enemy models — biped rigs, 6 anim clips each | ✅ |
| Loot / drops / gold | ✅ |
| XP / level / character sheet | ✅ |
| Chat, nameplates, who's online | ✅ |
| GM console (`\`` / F1) | ✅ |
| Profession XP persistence | 🔶 pending |
| Ability icons (real sprites) | 🔶 pending |
| Hit VFX on melee connect | 🔶 pending |
| NavMesh bake in Arena scene | ⚠ editor step |

---

## Quick Start (Play)

1. Go to **https://playcrossworlds.com** for the browser client
2. Or download the Windows client from the Releases page
3. Register an account, pick a class, **Enter World**

Server is online 24/7 at `playcrossworlds.com:7777`.

---

## Quick Links

| | |
|--|--|
| **ROADMAP.md** | Current task list and status |
| **SNAPSHOT.md** | Architecture map, what's implemented vs stubbed |
| **`Docs/ClassSpec.html`** | Full class reference with passives, stat bars, play patterns |
| **`Docs/SpellBook.html`** | Full spellbook — filterable by class / category |
| **`_CONTEXT/CLAUDE.md`** | DB schema, API endpoints, server conventions |
| **`_CONTEXT/VPS_SERVER.md`** | SSH, service commands, deploy, logs |
| **`web/`** | Three.js browser client — [Cross-Worlds-Web](https://github.com/Brbmuffins/Cross-Worlds-Web) submodule |
| Server Manager | http://playcrossworlds.com:4000 |

---

---

# Developer Reference

> Everything below is for contributors and the development record.

---

## Tech Stack

**Engine:** Unity 6 (6000.4.10f1) · Universal Render Pipeline  
**Networking:** Mirror (KCP transport, UDP 7777) · Server-authoritative  
**Backend:** Node.js / Express 5 · MySQL · VPS at `playcrossworlds.com`  
**Auth:** JWT · `AuthManager` credential cache — compiles in all build targets including Dedicated Server

---

## Classes — Implementation

Each class has fixed index positions — **never renumber**.

| # | Class | Legacy Name |
|---|---|---|
| 0 | Warden | Engineer |
| 1 | Ironclad | Guardian |
| 2 | Shadowblade | Wraith |
| 3 | Cleric | Medic |
| 4 | Arcanist | Phaser |

---

## Systems

### Networking
- Mirror host/client (KCP, UDP 7777)
- `RodNetworkAuthenticator` — JWT verify → `GET /character` → class + spawn position in `conn.authenticationData`
- `RodNetworkManager` — server-authoritative class selection; class prefabs dual-registered in `spawnPrefabs` and `NetworkClient.RegisterPrefab()`
- Dev mode bypass — one-click HOST without JWT for local testing

### Scene Flow
```
LoginScene(0) → CharacterSelect(1) → Hub(2) → [Arena_Copper via portal]
```
- `LoginManager` — MMO login UI, register panel, server IP field, animated title, dev HOST button
- `CharacterSelectManager` — 3D character preview via RenderTexture on layer 31
- `RodPositionSaver` — server-side; `PATCH /character/position` on disconnect

### Combat Core (`Assets/Game/Combat/Scripts/`)

| Script | Purpose |
|---|---|
| `Health.cs` | Server-authoritative HP. `CanMutateCombatState()` guard. Events: `onDeath`, `onDamageTaken`, `onKilledBy`, `onDownedChanged` |
| `StatusEffectManager.cs` | Applies/ticks/expires Slow, Stagger, Silenced, Cursed, Weakened, Bound |
| `PlayerProjectile.cs` | Traveling skill-shot projectile (player → enemy). Server-spawned. Configure `speed` + `maxRange`. |
| `EnemyProjectile.cs` | Linear ranged enemy projectile (enemy → player). Self-destructs on hit or timeout. |
| `DropTable.cs` | ScriptableObject — `RollDrops()` → items + gold |
| `WorldItem.cs` | NetworkBehaviour floor loot — floats, server-despawns after 90s |
| `CombatAudio.cs` | 7-slot AudioSource. Auto-assigned from Retro Sci-Fi Pack via `BCE/Hub World/Wire Combat Assets` |
| `ResourceNode.cs` | Mining node — F-key, POSTs to `/api/inventory/add-item`, profession XP |

### Enemy AI

`EnemyController.cs` — server-authoritative state machine:
```
Idle ──(aggroRadius)──► Chase ──(attackRange)──► Attack
  ▲                         │
  └── leash to spawn ───────┘
Dead ◄── Health.onDeath
```
- Stagger skips attack tick; Bound stops NavMeshAgent; Slow scales agent speed
- Ranged variant fires `EnemyProjectile`, backs off if too close
- **Telegraph** — `AttackSequence()` coroutine shows `RpcShowTelegraph` red indicator `telegraphDuration` seconds before attack lands

### Enemy Models (Tripo AI)

3 biped-rigged models committed as LFS assets:

| Enemy | Model | Path |
|---|---|---|
| Grunt | Goblin warrior | `Assets/Game/Characters/Enemies/Grunt/` |
| Ranged | Skeleton archer | `Assets/Game/Characters/Enemies/Ranged/` |
| Elite | Orc berserker | `Assets/Game/Characters/Enemies/Elite/` |

Each: v1.0-20240301 biped rig + 6 FBX clips (idle/walk/run/slash/hurt/fall). Run `BCE/Setup/4a–4c` to auto-attach.

### Wave System

`WaveSpawner.cs` — escalates: `baseEnemiesPerWave + (wave−1) × enemiesAddedPerWave`. 67% grunt / 33% ranged; elite every N waves.

### World Boss — Null Architect

`WorldBossController.cs` — 4-phase fight:

| Phase | Trigger | Mechanics |
|---|---|---|
| 1 | Fight start | Melee + reflect pulse AoE every 18s |
| Transition | HP ≤ 60% | 4s immunity → NullShard fracture spawns |
| 2 | Post-transition | Tether web (pair snap) + void drain AoE |
| 3 | HP ≤ 30% | Boss gains Weakened; void drain doubles |
| Final Surge | HP ≤ 10% | 3× speed + 3× attack for 15s |

### Inventory & Progression
- `InventoryManager` — 32-slot; POSTs to `/api/inventory/save` on pickup
- `PlayerProgressManager` — XP, gold, level → `/api/progress`
- `XpBar`, `CharacterSheetUI` (C key), `LevelUpScreen`

### Hub Systems
- `ForgeNPC` — proximity E-key → `CraftingUI`, recipes from `GET /api/recipes`, crafts via `POST /api/craft`
- `HangmanNPC` — auto-placed by `BCE/Hub Setup/9`

### UI Systems
- **ESC Menu** — Resume / Logout / Quit
- **Chat** (`RodChatManager`) — Enter/T; Mirror-networked
- **Who's Online** (`PlayerListUI`) — P key; class colour per player
- **Nameplates** — billboard, hides on local player, fades 20–40u
- **Ability Bar** — 4-slot strip, radial cooldown overlay
- **Spellbook** — Tab; 32 cards with icon, type badge, damage, CD; hover tooltip (`AbilityTooltipUI`, sort order 201); click + 1–4 to equip
- **Arena Clear UI** — wave-complete banner + loot summary

### GM Console
Toggle with `` ` `` or **F1**. Gated by `GM_USERS`.

| Command | Effect |
|---------|--------|
| `speed <n>` | Multiply move speed |
| `fly` | Toggle fly mode |
| `god` | Toggle invulnerability |
| `heal` | Full heal |
| `kill` | Kill all enemies |
| `spawn [n]` | Spawn n test enemies |
| `wave [n]` | Start / jump to wave n |
| `tp <x> <y> <z>` | Teleport |
| `pos` | Print world position |
| `players` | List connected players |
| `goto <name>` | Teleport to player |
| `noclip` | Toggle colliders |

---

## Editor Automation (`BCE →` menu)

### Setup (run in order)

| Step | Menu Item | Output |
|---|---|---|
| 0 | `Setup/0 ▶ Create Character Select Scene` | CharacterSelect.unity |
| 1 | `Setup/1 ▶ Create Login Scene` | LoginScene.unity |
| 2 | `Setup/2 ▶ Clean GameWorld` | Removes stray NetworkManager components |
| 3 | `Setup/3 ▶ Fix Build Settings` | Login(0) → CharSelect(1) → Hub(2) |
| 4 | `Setup/4 ▶ Create Class Prefabs` | 5 hero prefabs + NetworkManager registration |
| 4a | `Setup/4a ▶ Create Grunt Enemy Prefab` | Enemy_Grunt.prefab + Tripo mesh |
| 4b | `Setup/4b ▶ Create Ranged Enemy Prefab` | Enemy_Ranged.prefab + Tripo mesh |
| 4c | `Setup/4c ▶ Create Elite Enemy Prefab` | Enemy_Elite.prefab + Tripo mesh |
| 4d | `Setup/4d ▶ Create WorldItem Prefab` | WorldItem.prefab |
| 4d | `Setup/4d ▶ Create Enemy AnimController` | EnemyAnimController.controller |
| 4e | `Setup/4e ▶ Create Wave Spawner (Arena)` | WaveSpawner + 4 spawn points |
| 5 | `Setup/5 ▶ Fix Animator Controllers` | Re-assigns AnimatorControllers to class prefabs |
| 6 | `Setup/6 ▶ Create World Boss (Null Architect)` | NullArchitect_Boss + NullShard.prefab |
| 7 | `Setup/7 ▶ Create Arena Scene` | Arena_Copper.unity |

### Hub Setup

| Step | Menu Item | Effect |
|---|---|---|
| 8 | `Hub Setup/8 - Add Forge and Mining NPCs` | ForgeNPC + 3 Copper Ore nodes |
| 9 | `Hub Setup/9 - Place HangmanNPC` | HangmanNPC in Hub |
| — | `Hub World/Wire Combat Assets` | VFX, CombatAudio (7 clips), WaveSpawner enemy refs |
| — | `Hub World/Build Hub Scene` | Full Hub.unity rebuild |

**After 4a–4d:** set Avatar on each enemy Animator (Inspector only).  
**After any prefab change:** add to NetworkManager → Registered Spawnable Prefabs.

---

## Server / API

Ports: **3000** auth · **4000** dashboard · **7777/UDP** game · **3001** Kuma

| Endpoint | Purpose |
|---|---|
| `POST /login` | Returns JWT |
| `POST /register` | Create account |
| `GET /character` | **Sacred** — class index + position + gear on every spawn |
| `POST /character` | Create/confirm character |
| `PATCH /character/position` | Save position on disconnect |
| `POST /character/gear/equip` | Equip item |
| `GET /api/inventory/:id` | Load inventory |
| `POST /api/inventory/save` | Save all slots |
| `GET /api/recipes` | Crafting recipes |
| `POST /api/craft` | Attempt craft |
| `GET /api/progress/:id` | XP, level, gold |
| `POST /api/progress/award-xp` | Grant XP |

Old gear endpoints (`/character`, `/character/gear/equip`, `item_template`, `character_gear`) are **sacred** — Unity calls them on every spawn. Never modify.

---

## Build & Deploy

```
1. git lfs pull          (GitHub Desktop — CLI has no auth token)
2. powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
3. scp build\crossworlds-server.tar.gz tools\deploy-server.sh ubuntu@playcrossworlds.com:~
4. ssh ubuntu@playcrossworlds.com "sudo bash deploy-server.sh"
```

Auto-backup, restart, verify, auto-rollback on failure. Manual rollback: `--rollback`.

---

## Changelog

### 2026-07-03 — Smite-style combat, spellbook directory

- **Skill shots** (`PlayerProjectile.cs`) — `AbilityShape.SkillShot` fires a traveling projectile. Void Bolt converted: range 14, speed 20, 15–45 charged dmg.
- **Enemy telegraphs** — `AttackSequence()` coroutine: red `RpcShowTelegraph` indicator 0.45s before attack lands.
- **Spellbook** — Tab opens 32-ability grid. Cards show icon, type badge, damage, CD. Hover → `AbilityTooltipUI` (description, stats, range). Click + 1–4 to equip.
- **Ability descriptions** — all 32 `AbilityDef` entries have `description` filled in. `[TextArea]` in Inspector.
- **Dodge roll** — Left Alt / V, 2 charges, i-frames — already in `PlayerMovement`, now documented.

### 2026-07-03 — Enemy models, CombatAudio, compilation fixes

- **Tripo AI enemy models** — 3 rigged models (goblin grunt, skeleton archer, orc berserker) with v1.0 biped rigs + 6 FBX clips each. LFS assets.
- **EnemyBuilder auto-attach** — `BCE/Setup/4a–4c` attaches Tripo mesh as `Model` child, wires AnimController.
- **CombatAudio auto-assignment** — `BCE/Hub World/Wire Combat Assets` assigns 7 Retro Sci-Fi Pack clips. Idempotent.
- **AuthManager fix** — stub moved outside `#if !UNITY_SERVER` in `InventoryManager.cs`. Resolves CS0103.
- **Editor builder guards** — `CombatAudio`, `ResourceNode`, `PlayerAnimator` refs guarded with `#if !UNITY_SERVER` in editor scripts. Fixed `MeshFilter.enabled` CS1061.
- **`.gitignore`** — blocks `.env`, `*.local`, `*secret*`, `*api-key*`.

### 2026-06-28 — Stability & networking

- `isLocalPlayer` guard on abilities; `InvariantCulture` for position floats; duplicate `CreatePlayerMessage` rejection; enemy AI drops dead targets; damage redirect re-entrancy guard; `[CHAT]` server logging.

---

## Known TODOs

| Priority | Item |
|----------|------|
| High | Arena NavMesh — not yet baked; WaveSpawner needs enemy prefabs wired |
| High | Enemy Animator Avatar — set manually in Inspector after `4a–4c` |
| High | `PlayerProjectile` prefab — create, assign to `AbilityCaster.playerProjectilePrefab`, register in NetworkManager |
| Medium | `orientation:F3` — client sends float as formatted string in `PATCH /character/position` |
| Medium | Ability icons — placeholder squares; real sprite atlas needed |
| Medium | Profession XP — not yet persisted to `professions` table |
| Medium | Stale prefabs — `Engineer.prefab`, `Guardian.prefab`, `Wraith.prefab`, `Medic.prefab` in Prefabs/ |
| Low | Arcanist missing from CharacterSelect 3D preview |
| Low | Hit VFX — no impact sparks on melee connect |
| Low | Footstep SFX — silent |
| Low | Brandalf (`Assets/Game/Heroes/Brandalf/`) — DECISION PENDING: new class vs skin |

---

## Project Structure

```
Assets/Game/
  Abilities/Scripts/     deployables + ability behaviours (mines, walls, zones)
  Characters/Scripts/    class passives, NPC controller, ability pools
  Characters/Enemies/    Tripo AI models — Grunt / Ranged / Elite
  Combat/Scripts/        Health, EnemyController, WaveSpawner, WorldBossController,
                         StatusEffectManager, DropTable, WorldItem, CombatAudio,
                         PlayerProjectile, EnemyProjectile, ResourceNode
  Networking/            RodNetworkManager, RodNetworkAuthenticator, PlayerIdentity,
                         PortalTransition, RodChatManager, ForgeNPC
  Systems/               InventoryManager, ItemCatalog, HeroMastery
  UI/                    HUDs, AbilityCaster, AbilityBar, AbilityTooltipUI,
                         GmConsole, LoginManager, PlayerProgressManager
  Editor/                BCE menu builders
  Scenes/                LoginScene(0), CharacterSelect(1), Hub(2)
  Prefabs/               5 hero prefabs + Enemy_Grunt/Ranged/Elite + WorldItem
  Heroes/Brandalf/       6th-hero model — DECISION PENDING
Docs/
  ClassSpec.html         Full class reference (open in browser)
  SpellBook.html         Full spellbook (open in browser)
  logo.png, multiplayer-chat-working.png
  Inspiration art/       Reference art
_CONTEXT/                server/API docs, VPS ops
tools/                   build-server.ps1, deploy-server.sh
web/                     Three.js browser client (git submodule → Cross-Worlds-Web)
```
