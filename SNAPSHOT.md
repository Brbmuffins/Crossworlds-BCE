# SNAPSHOT.md — Crossworlds BCE, State of the Project

**Audit date:** 2026-07-03 (full audit) — **delta update 2026-07-25 below supersedes stale claims**
**Auditor:** Claude (senior-lead audit pass)
**Scope:** `D:\Crossworlds` working tree (branch `main`, HEAD `ee7706d`) + server docs in `_CONTEXT/` and `CrossWorlds/CLAUDE.md`

---

## ⚡ Delta update — 2026-07-25 (HEAD `2daa100f`, verified against code)

Three weeks and ~275 CI deploys have passed since the full audit. Verified changes
that override statements below:

- **Class 0 renamed: Warden → Marauder.** `PlayerIdentity.ClassNames` =
  `{ "Marauder", "Ironclad", "Shadowblade", "Cleric", "Arcanist" }`; roles per
  CharacterSelect: Marauder Damage·Control, Ironclad Tank·CC, Shadowblade
  Stealth·Burst, Cleric Support·Sustain, Arcanist Assassin·Mobility. Indices
  unchanged. ("Iron Warden" = world boss, `IronWardenController` — not the class.)
- **The "no arena scene" blocker (§2) is RESOLVED.** Build list now has 9 scenes:
  LoginScene, CharacterSelect, HUB, **Darkwood, Ashen Wastelands, Toujam Basin,
  GM Island, VoidDungeon**, _Container. `Arena_Copper` / TutorialIsland references
  below are historical.
- **Enemy Forge exists**: `Assets/Game/Editor/EnemyForge/` — data-driven enemy
  authoring (Definition/Builder/Validator/Deployment/AnimationLibrary/Window),
  generating prefabs into `Assets/Game/Prefabs/EnemyForge/`. New enemy types
  (Templar, Wizard, Chaos Weaver w/ ChainLightning) are Forge content, not classes.
  `EnemyAI.cs` now lives in `Combat/Scripts/` alongside `EnemyController`.
- **`GmConsole.cs` guard bug (§2) is FIXED** — file opens with
  `#if UNITY_EDITOR || !UNITY_SERVER`.
- **`Assets/Game/Heroes/` (Brandalf model dir) is gone**; only editor tooling
  (`BrandalfSetupBuilder`, `CharacterModelSwapper`) remains. Decision still open.
- **No mount code exists** in `Assets/Game` despite a wiki "Mounts & Traversal" page.
- Working tree is clean and synced with origin/main (the §2 "uncommitted month of
  work" is resolved); CI (GitHub Actions "Build and Deploy") is the live pipeline.
- **2026-07-26 audit addendum** (delta above verified at `2daa100f`; since then):
  Marauder Crashing Leap implementation (`451913e4`), Enemy Forge + online-player
  rework (`109942a3`) — `CastAnimator.PlayCast` / `EnemyController.PlayCastAnimation`
  signatures changed, all call sites updated. Audit rewrote the 8 remaining bare
  `#if !UNITY_SERVER` guards to `#if UNITY_EDITOR || !UNITY_SERVER` (6 UI/Systems
  files + AfkStationBuilder + HubSceneBuilder) — review-only, pending editor compile.
  Origin is 1 commit ahead of local (`65138e5e` Darkwood/Marauder VFX); pull via
  GitHub Desktop. Tree itself is clean.

Sections below are accurate as of 2026-07-03 except where contradicted above.
A fresh full audit is warranted; this delta corrects only verified headline facts.

---

## 0. Reality Check — Brief vs. Repo

The audit brief described "Corrosion (Rate_of_Decay)" with SQLite, A* Pathfinding Pro,
DOTween, UniTask, an "Entropy" gear-decay system, and six classes. **None of that is in
this repository.** What actually exists:

| Brief said | Repo actually has |
|---|---|
| Codename "Corrosion / Rate_of_Decay" | **Crossworlds BCE** — "RoD" survives only in code prefixes (`Rod*` script/class names like `RodNetworkManager`); the live services and DB were renamed to `crossworlds-*` / `crossworlds` |
| SQLite persistence | **MySQL 8 on a remote VPS**, accessed via a Node.js/Express REST API (port 3000). Unity never touches a DB directly. |
| A* Pathfinding Pro | **Unity built-in NavMesh** (`NavMeshAgent` in `EnemyController`, `EnemyAI`, `NpcController`) |
| DOTween / UniTask | **Neither installed.** Animation is manual lerp/coroutine; async is 142 `StartCoroutine` call sites across 63 files + `UnityWebRequest` coroutines for all HTTP. |
| "Entropy" gear-decay system | **Does not exist.** The only "decay" hits are unrelated (status-effect falloff in `StatusEffectManager`, `PassiveOverengineered`, `NullFieldZone`). |
| Six classes | **Five heroes:** 0=Warden, 1=Ironclad, 2=Shadowblade, 3=Cleric, 4=Arcanist. A sixth model, **Brandalf**, was recently added (`Assets/Game/Heroes/Brandalf/`) but is currently wired as the *Arcanist's preview model* in `CharacterSelectManager.cs:106`, not a new class. |

Mirror/KCP self-hosted on UDP 7777 is the one claim that matches. Everything below
documents the repo as it is, not as the brief imagined it.

---

## 1. Architecture Map

### 1.1 Topology

```
Unity Client ──KCP/UDP 7777──► Unity Dedicated Server (Linux headless, Mirror)
     │                                   │
     │ HTTPS/HTTP REST (JWT)             │ REST (server-side verify)
     ▼                                   ▼
Node.js Auth/Game API :3000 ──────► MySQL 8 (rod_online / crossworlds DB)
Node.js Dashboard     :4000 (admin/GM, Socket.io)
Nginx :80/443 (playcrossworlds.com, downloads)
```

Authority is **split across two servers**:

- **Unity game server (Mirror)** — authoritative for real-time state: enemy AI,
  waves, boss phases, ability effects, deployables, chat, player spawn/position.
- **Node auth API** — authoritative for *persistence*: accounts, characters,
  inventory, progression, crafting, and kill-reward validation (hit-gate +
  kill-cooldown maps documented in `_CONTEXT/CLAUDE.md`).

### 1.2 Login → play flow

1. `LoginManager` (LoginScene) → `POST /login` → JWT stored in `PlayerPrefs("jwt_token")`.
2. `CharacterSelectManager` → class choice stored in `PlayerPrefs("SelectedCharacter")`.
3. Mirror connect → `RodNetworkAuthenticator` sends JWT; the **Unity server** calls
   `GET /character` to verify and fetch class + saved position (`RodPlayerAuth`).
4. `RodNetworkManager.OnCreatePlayer` spawns the class prefab server-side, attaches
   `RodPositionSaver`, then sets `PlayerIdentity` SyncVars (name, classIndex, characterId).
5. Hub scene; `PortalTransition` / `HangmanNPC` trigger `ServerChangeScene` to an arena;
   `WaveSpawner` runs the wave loop; `HubReturnTrigger` returns everyone.

Dev bypass: in-editor, JWT `"dev"` skips the auth server entirely
(`RodNetworkAuthenticator.cs:68-90`) — editor-only by `#if UNITY_EDITOR`, safe in builds.

### 1.3 Core systems and where `[Server]` lives

| System | Key files | Authority |
|---|---|---|
| Networking/session | `RodNetworkManager`, `RodNetworkAuthenticator`, `PlayerIdentity`, `RodPositionSaver`, `RodChatManager` | Server-auth spawn & class; SyncVars for identity |
| Enemy combat | `Combat/Scripts/EnemyController` (FSM, `[Server]` loop + 14 attrs), `EnemyProjectile`, `Health`, `EnemyDeathHandler`, `StatusEffectManager` | Server; clients get VFX/health-bar hooks under `#if !UNITY_SERVER` |
| Waves/arena | `WaveSpawner` (16 Mirror attrs), `WaveManager`, `ArenaSessionController`, `WaveChest`, `WorldBossController` (28 attrs) | Server coroutine loops; SyncVars for HUD |
| Abilities | `AbilityCaster` + serializable `AbilityDef` (data-driven: shape, charge, chain, pull, shield, deployable), per-class handlers (`DashHandler`, `StealthHandler`, `SiegeModeHandler`, `IronTetherHandler`, …), deployables (`DeployableManager`, `PhaseRelayDeployable`, `BastionNode`, …), `SnapshotSystem` | Mixed — Commands→server effects; needs per-file review before hardening |
| Class passives | `Characters/Scripts/Passive*` (BountySystem, PhaseCharge, ThreatProtocol, TriageLoop, Overengineered) + `ClassPassive`, `ClassAbilityPool` | Mostly server |
| Persistence singletons (client-side REST) | `Systems/InventoryManager`, `UI/PlayerProgressManager`, `Systems/ItemCatalogManager`, `Systems/HeroMasteryManager`, `Combat/Scripts/CombatSessionTracker` | **Client-initiated** HTTP with PlayerPrefs JWT — see §4 |
| Loot | `DropTable` (ScriptableObject weighted rolls), `WorldItem` (net-spawned pickup), `WaveChest` | Server rolls/spawns; pickup → client `InventoryManager` → `POST /api/inventory/save` |
| UI | ~30 scripts (bag, crafting, XP bar, boss bar, radar, status HUD, GM console…) | Client |
| Editor tooling | `Editor/` builders (`RodHubSceneBuilder`, `EnemyBuilder`, `WorldBossBuilder`, `BrandalfSetupBuilder`, `BuildScript`, …) | Editor-only, generates scenes/prefabs — load-bearing for reproducibility |

### 1.4 Dependency graph (what breaks what)

```
Node API schema/response shapes
  └─► InventoryManager / PlayerProgressManager / HeroMasteryManager / CraftingUI
        (JsonUtility parsers — any server field rename silently breaks parsing)
RodNetworkAuthenticator ─► GET /character  (auth server down = nobody can join prod)
RodNetworkManager.classPrefabs[0-4] ─► class index order everywhere
  (PlayerIdentity.ClassNames, CharacterSelect, server CLASS_NAMES must stay in lock-step)
Health ─► EnemyController / WorldBossController / EnemyDeathHandler / EnemyHealthBar
WaveSpawner ─► ArenaSessionController, CombatSessionTracker, WaveHUD, WaveChest, mastery XP
DropTable + WorldItem prefab ─► InventoryManager ─► /api/inventory/save
Scene names as strings ("Arena_Copper", offline/onlineScene paths in RodNetworkManager.Awake)
  ─► PortalTransition / HubReturnTrigger — rename a scene, break the portal silently
Old gear endpoints/tables (item_template, character_gear…) — SACRED, Unity spawn depends on them
```

---

## 2. Implemented vs. Half-Finished vs. Planned

### ✅ Implemented and believed working
- Login → character select → hub multiplayer loop (JWT auth, server-auth class spawn, saved positions, nameplates, chat, player list, ESC menu).
- Server-side kill validation on Node (`/api/combat/hit` + `/api/combat/kill`, transactional XP/gold/loot, in-memory anti-exploit gates).
- Enemy FSM (melee/ranged/elite), waves with escalation and elite cadence, world boss with phases, status effects.
- Ability framework (data-driven `AbilityDef`), five class kits with passives and deployables.
- Inventory/progression/crafting **APIs** live on VPS; client `InventoryManager` and `PlayerProgressManager` written.
- Editor builder suite that reconstructs scenes/prefabs from menu items.

### 🔶 Half-finished (honest list)
- **No arena scene exists.** `Assets/Game/Scenes/` has Login, LoginScene (both!), CharacterSelect, Hub, TutorialIsland. `PortalTransition` defaults to `"Arena_Copper"` — that scene is not in the repo. The core portal→arena→waves loop cannot run end-to-end.
- **Wire-up checklist from 2026-06-29 session log still open** (`CrossWorlds/CLAUDE.md`): WaveSpawner→CombatSessionTracker notify, NetworkManager spawnPrefabs registration for Enemy_* and WorldItem, HangmanNPC placement, BeginSession call.
- **`GmConsole.cs` still lacks the `#if !UNITY_SERVER` guard** — the open bug in every status doc; grep confirms no guard present.
- **Brandalf**: model, animator, `BrandalfSetupBuilder`, and TutorialIsland scene exist; currently only a character-select preview model for Arcanist. Sixth-class or skin decision pending.
- **HeroMasteryManager / CombatSessionTracker** call `/api/mastery/*` and `/api/combat/session/end` — **these endpoints are not documented in any server CLAUDE.md**. Either the docs are stale or the client is calling endpoints that don't exist. Unverified from the repo.
- **Phase-2 UI shipped early**: `GuildPanelUI`, `QuestLogUI`, `QuestTracker`, `TalentTreeUI`, `TalentModifierApplier` exist client-side with **no server endpoints** (schema stubs only). Dead UI until Phase 2.
- **Entire working tree is uncommitted**: ~76 files, +3,946/−3,115 lines of un-committed churn on `main`, including prefabs and both scenes. **34 `.cs.bak` files** sit inside `Assets/` (Unity compiles nothing from `.bak`, but they clutter diffs and drift from their sources). Root-level `Assets/Game/Combat/*.cs` are "MOVED" tombstone stubs.
- **VPS-only scripts never pulled locally**: `ApiClient.cs`, `EnemyTemplate.cs`, `EnemyTemplateRegistry.cs`, `EnemyAI.cs`, `PlayerHealth.cs`, `HUDManager.cs`, `CraftingManager.cs` live at `/opt/crossworlds-auth/unity-scripts/` per docs; the repo has parallel, *different* implementations (`EnemyController` vs `EnemyAI`, `InventoryManager` duplicated in `Systems/` and `Items/Scripts/`). Two competing client stacks for the same job.

### ○ Planned (do not build yet, per project docs)
Marketplace, guilds, quests, talent trees (server side), more dungeons, arena session tokens, server-side player HP. DB stubs exist (`gold_transactions`, `marketplace_listings`, `guilds`, `guild_members`).

---

## 3. Known Pain Points

1. **Hardcoded server IP `15.204.243.36` in 10 files** (`RodNetworkManager`, `RodNetworkAuthenticator`, `LoginManager`, `CharacterSelectUI/Manager`, editor setup, LoginScene serialized fields). A server move = 10 edits + scene re-serialization.
2. **Documentation triplication**: three diverging CLAUDE.md files (`_CONTEXT/`, `CrossWorlds/`, VPS copy) disagree on class names (Engineer/Guardian vs Warden/Ironclad), DB name (`rod_online` vs `crossworlds`), and endpoint lists. Plaintext production credentials that once lived in these docs (MySQL password, dashboard admin password) have been **redacted from the working tree** (2026-07-26), but they **remain in git history** — rotate them on the VPS (ROADMAP Q7).
3. **JSON fragility**: all client parsing is `JsonUtility` against snake_case ad-hoc classes; the server has already burned time on float-format (`orientation:F3`) and NaN-slot bugs. No shared DTO layer.
4. **Client-trusted progression writes**: `POST /api/character/save-progress` accepts whatever level/xp/gold the client sends (JWT-gated but value-trusted). `/api/combat/kill` mitigates for kills, but the save path remains an open cheat vector — acknowledged in docs as Phase 2.
5. **No server-side player HP** — `Health` on players is dealt with client-side by EnemyAI per docs; hacked clients can no-damage.
6. **Anti-exploit maps are in-process** — restart clears hit gates/cooldowns (accepted alpha risk).
7. **Scene-name strings** wiring portals/return triggers; `RodNetworkManager.Awake` hardcodes full scene *paths*.
8. **Duplicate/competing scripts** (§2) and 34 `.bak` files make it genuinely unclear which implementation is canonical for enemies, health, inventory.
9. **Line-ending churn**: repo has LF files being rewritten to CRLF (no `.gitattributes`), inflating every diff.

---

## 4. Code-Standards Adherence

| Standard | Verdict |
|---|---|
| Mirror over NGO | ✅ Pure Mirror/KCP throughout; no NGO references. |
| UniTask over coroutines | ❌ **UniTask is not even installed.** 142 `StartCoroutine` sites; all HTTP is coroutine + `UnityWebRequest`. If this is a real standard, it's aspirational, not practiced. |
| No magic numbers | 🔶 Mixed. Gameplay tuning is mostly Inspector-exposed `[Header]` fields (good). But literals persist: spawn-ring radius `3f`/`Mathf.PI*2f` in `RodNetworkManager`, `XpToNext=100` default, `MaxSlots=32`, prompt radii, timeouts (`req.timeout=8`), hardcoded ports/IPs. Server constants are properly named (`HIT_WINDOW_MS`, `KILL_COOLDOWN_MS`). |
| `[Server]` on all game state | 🔶 Good in combat core (`EnemyController`, `WaveSpawner`, `WorldBossController`, `ArenaSessionController`, `PortalTransition` all gate mutations). **But** the persistence layer (inventory, XP/gold saves, mastery) is client-initiated REST outside Mirror entirely — game state authority effectively bypasses `[Server]` for anything that persists. |
| DOTween / A* Pro / SQLite | ❌ Not present (see §0). |

---

## 5. Bottom Line

The Mirror layer and Node API are in decent shape individually. The project's real risks are
(a) **the glue is missing** — no arena scene, unwired session tracking, unregistered spawn
prefabs mean the advertised core loop doesn't actually run; (b) **two parallel client stacks**
(repo scripts vs VPS `unity-scripts/`) with no declared winner; (c) **repo hygiene** — a month
of work sitting uncommitted with credentials in a tracked doc. The roadmap
([ROADMAP.md](ROADMAP.md)) starts there, not with new features.
