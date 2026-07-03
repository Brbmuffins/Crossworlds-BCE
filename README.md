<p align="center">
  <img src="Docs/logo.png" alt="Crossworlds BCE" width="220"/>
</p>

# Crossworlds BCE

**Genre:** 4–10 player co-op combat MMO  
**Engine:** Unity 6 (6000.4.10f1) · Universal Render Pipeline  
**Networking:** Mirror (KCP transport, port 7777) · Server-authoritative  
**Backend:** Node.js / Express 5 · MySQL · VPS auth server at `playcrossworlds.com`

---

## Quick Links

| | |
|--|--|
| **ROADMAP.md** | Current task list and status |
| **SNAPSHOT.md** | Architecture map, what's implemented vs stubbed |
| **`_CONTEXT/CLAUDE.md`** | DB schema, API endpoints, server conventions |
| **`_CONTEXT/VPS_SERVER.md`** | SSH, service commands, deploy, logs |
| Website / Download | https://playcrossworlds.com |
| Server Manager | http://playcrossworlds.com:4000 |
| GM Dashboard | http://playcrossworlds.com:4000/gm-dashboard |

---

## Classes

| # | Class | Role | Identity |
|---|-------|------|----------|
| 0 | **Warden** | Tank / Nature CC | Runic Snare, Battle Hymn, summon spirits — controls space and outlasts the fight |
| 1 | **Ironclad** | Tank / CC | Shieldwall Charge, Iron Rampart, Counter Blow — the anvil the team fights around |
| 2 | **Shadowblade** | DoT / Assassin | Shadow Veil, Dark Mark, Dark Harvest — stealth pressure and burst detonation |
| 3 | **Cleric** | Support / Heal | Soul Bond, Divine Spark, Temporal Grace — keeps the team alive under fire |
| 4 | **Arcanist** | Burst / Control | Arcane Step, Void Maw, Collapsing Void — spatial repositioning and burst payoff |

Each class has 4 equipped abilities + 1 ultimate. Class index positions are fixed — never renumber.

---

## Systems

### Networking
- Mirror host/client live end-to-end (KCP, UDP 7777)
- `RodNetworkAuthenticator` — JWT verify → `GET /character` → class + spawn position in `conn.authenticationData`
- `RodNetworkManager` — server-authoritative class selection; class prefabs dual-registered in `spawnPrefabs` and `NetworkClient.RegisterPrefab()`
- Dev mode bypass — one-click HOST without JWT for local testing
- `AuthManager` — session credential cache (Token + CharacterId) shared across LoginManager, CharacterSelectManager, PlayerIdentity, InventoryManager. Compiles in all build targets including Dedicated Server.

### Scene Flow
```
LoginScene(0) → CharacterSelect(1) → Hub(2) → [Arena_Copper via portal]
```
- `LoginManager` — MMO login UI, register panel, server IP field, animated title, dev HOST button
- `CharacterSelectManager` — 3D character preview via RenderTexture on layer 31; class list, ability readout, ENTER WORLD
- `RodPositionSaver` — server-side; `PATCH /character/position` on disconnect

### Combat Core (`Assets/Game/Combat/Scripts/`)

| Script | Purpose |
|---|---|
| `Health.cs` | Server-authoritative HP. Events: `onDeath`, `onDamageTaken`, `onHealthChanged`, `onKilledBy`, `onHealApplied`, `onDownedChanged` |
| `StatusEffectManager.cs` | Applies/ticks/expires Slow, Stagger, Silenced, Cursed, Weakened, Bound |
| `DropTable.cs` | ScriptableObject — `RollDrops()` → items + gold. Configurable weights |
| `WorldItem.cs` | NetworkBehaviour floor loot — floats + rotates, server-despawns after 90s |
| `CombatAudio.cs` | 7-slot AudioSource component: meleeHit, rangedHit, deathSFX, shieldSFX, waveAlert, abilityCast, healSFX. Clips auto-assigned from Retro Sci-Fi Pack via BCE editor menu |
| `ResourceNode.cs` | Mining node — F-key, POSTs to `/api/inventory/add-item`, awards profession XP, depletes + respawns |

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
- Death: spawns WorldItem at death position, drops gold

### Enemy Models (Tripo AI)

3 character models generated via Tripo text-to-3D pipeline and committed as LFS assets:

| Enemy | Model | Template ID |
|---|---|---|
| Grunt | Goblin warrior | `goblin_grunt` |
| Ranged | Skeleton archer | `skeleton_ranged` |
| Elite | Orc berserker | `troll_elite` |

Each has a clean v1.0-20240301 biped rig (16 paired L/R anatomical bones) and 6 FBX animation clips: `idle / walk / run / slash / hurt / fall`.

Assets at: `Assets/Game/Characters/Enemies/{Grunt,Ranged,Elite}/`

When you run `BCE/Setup/4a–4c`, `EnemyBuilder` automatically attaches the rigged GLB as a `Model` child and wires `EnemyAnimController` onto its `Animator`.

### Wave System

`WaveSpawner.cs` — NetworkBehaviour, server-driven:
- `StartWaves()` / `StopWaves()` called from portal trigger
- Escalates: `baseEnemiesPerWave + (wave−1) × enemiesAddedPerWave`
- 67% grunt / 33% ranged split; elite every N waves
- Waits for `enemiesAlive == 0` before advancing
- Announcements via `RodChatManager`

### World Boss — Null Architect

`WorldBossController.cs` — 4-phase NetworkBehaviour:

| Phase | Trigger | Mechanics |
|---|---|---|
| 1 | Fight start | Melee + reflect pulse AoE every 18s |
| Transition | HP ≤ 60% | 4s immunity → NullShard fracture spawns |
| 2 | Post-transition | Tether web (pair snap damage) + void drain AoE |
| 3 | HP ≤ 30% | Boss gains Weakened (+25% damage taken); void drain doubles |
| Final Surge | HP ≤ 10% | 3× speed + 3× attack for 15s |

`WorldBossHealthBar` — self-bootstrapping ScreenSpaceOverlay, phase colour shifts, marker lines at 60% and 30%.

### Inventory & Loot
- `InventoryManager` — self-bootstrapping singleton; 32-slot in-memory cache; POSTs to `/api/inventory/save` on pickup
- Gold pickups routed directly to `PlayerProgressManager.AwardGold()`
- Equipment slot sync via `POST /api/inventory/equip`

### Progression
- `PlayerProgressManager` — XP, gold, level; syncs to `/api/progress`
- `XpBar.cs` — bottom-centre XP bar, smooth fill, gold flash on level-up
- `CharacterSheetUI.cs` — C key: level, XP, gold, stat block
- `LevelUpScreen.cs` — full-screen class-coloured level-up burst animation

### Hub Systems
- `ForgeNPC.cs` — proximity E-key, opens `CraftingUI`, billboard prompt, gold point light
- `CraftingUI.cs` — scrollable recipes from `GET /api/recipes`, ingredient check, `POST /api/craft`
- `HangmanNPC.cs` — auto-placed in Hub by `BCE/Hub Setup/9`

### UI Systems
- **ESC Menu** — Resume / Logout / Quit; self-bootstrapping
- **Chat** (`RodChatManager`) — Enter/T; Mirror-networked
- **Who's Online** (`PlayerListUI`) — P key; class colour per player
- **Nameplates** — floating billboard, hides on local player, fades 20–40u
- **Ability Bar** — 4+1 strip, radial cooldown overlay + CD timer text
- **Arena Clear UI** — wave-complete banner + loot summary

### GM Console (`GmConsole.cs`)
Toggle with `` ` `` or **F1**. Access gated by `GM_USERS` allowlist.

| Command | Effect |
|---------|--------|
| `speed <n>` | Multiply move + sprint speed |
| `fly` | Toggle fly mode |
| `god` | Toggle invulnerability |
| `heal` | Full heal self |
| `kill` | Kill all enemies |
| `spawn [n]` | Spawn n test enemies |
| `wave [n]` | Start waves or jump to wave n |
| `tp <x> <y> <z>` | Teleport to coords |
| `pos` | Print world position |
| `players` | List all connected players |
| `goto <name>` | Teleport to player |
| `noclip` | Toggle colliders off |

### VFX & Audio
- brbmuffins Technologies particle pack (sparks, explosions, fire)
- brbmuffins Dark Arts fantasy pack (magic circles, lightning, fireballs)
- `EnemyDeathVFX` — crimson burst on death; procedural fallback if no prefab
- `LoginScreenVFX` — ambient login atmosphere
- `CombatAudio` — 7 Retro Sci-Fi Pack clips auto-assigned via `BCE/Hub World/Wire Combat Assets`

---

## Editor Automation (`BCE →` menu)

### Setup (run in order from scratch)

| Step | Menu Item | Output |
|---|---|---|
| 0 | `Setup/0 ▶ Create Character Select Scene` | CharacterSelect.unity |
| 1 | `Setup/1 ▶ Create Login Scene` | LoginScene.unity |
| 2 | `Setup/2 ▶ Clean GameWorld` | Removes stray NetworkManager components |
| 3 | `Setup/3 ▶ Fix Build Settings` | Login(0) → CharSelect(1) → Hub(2) |
| 4 | `Setup/4 ▶ Create Class Prefabs` | 5 hero prefabs, registered in NetworkManager |
| 4a | `Setup/4a ▶ Create Grunt Enemy Prefab` | Enemy_Grunt.prefab + Grunt_DropTable + Tripo mesh |
| 4b | `Setup/4b ▶ Create Ranged Enemy Prefab` | Enemy_Ranged.prefab + Ranged_DropTable + Tripo mesh |
| 4c | `Setup/4c ▶ Create Elite Enemy Prefab` | Enemy_Elite.prefab + Elite_DropTable + Tripo mesh |
| 4d | `Setup/4d ▶ Create WorldItem Prefab` | WorldItem.prefab |
| 4d | `Setup/4d ▶ Create Enemy AnimController` | EnemyAnimController.controller (uses Tripo clips) |
| 4e | `Setup/4e ▶ Create Wave Spawner (Arena)` | WaveSpawner + 4 spawn points in active scene |
| 5 | `Setup/5 ▶ Fix Animator Controllers` | Re-assigns AnimatorControllers to class prefabs |
| 6 | `Setup/6 ▶ Create World Boss (Null Architect)` | NullArchitect_Boss + NullShard.prefab |
| 7 | `Setup/7 ▶ Create Arena Scene` | Arena_Copper.unity |

### Hub Setup

| Step | Menu Item | Effect |
|---|---|---|
| 8 | `Hub Setup/8 - Add Forge and Mining NPCs` | ForgeNPC at (−12,0,−4) + 3 Copper Ore nodes |
| 9 | `Hub Setup/9 - Place HangmanNPC` | HangmanNPC in Hub scene |
| — | `Hub World/Wire Combat Assets` | Wires VFX prefabs, CombatAudio (7 clips), WaveSpawner enemy refs |
| — | `Hub World/Build Hub Scene` | Full Hub.unity rebuild from scratch |

**After 4a–4d:** set the **Avatar** field on each enemy prefab's Animator (Inspector — cannot be automated).  
**After any prefab change:** add new prefabs to **NetworkManager → Registered Spawnable Prefabs**.

---

## Server / API

Ports are frozen: **3000** auth · **4000** dashboard · **7777/UDP** game · **3001** Kuma

| Endpoint | Purpose |
|---|---|
| `POST /login` | Returns JWT |
| `POST /register` | Create account |
| `GET /character` | Spawn-path critical: class index + position + gear |
| `POST /character` | Create/confirm character |
| `PATCH /character/position` | Save position on disconnect |
| `POST /character/gear/equip` | Equip item |
| `GET /api/inventory/:id` | Load inventory slots |
| `POST /api/inventory/save` | Save all slots |
| `POST /api/inventory/equip` | Mark slot equipped |
| `GET /api/recipes` | Crafting recipe list |
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

Auto-backup, restart, verify, and auto-rollback on failure. Manual rollback: `--rollback`.

---

## Changelog

### 2026-07-03 — Enemy models, CombatAudio automation, compilation fixes

- **Tripo AI enemy models** — 3 unique character models (goblin grunt, skeleton archer, orc berserker) generated via text-to-3D with v1.0 biped rigs and 6 FBX animation clips each (idle/walk/run/slash/hurt/fall). Committed as LFS assets under `Assets/Game/Characters/Enemies/`.
- **EnemyBuilder auto-attach** — `BCE/Setup/4a–4c` now automatically finds the rigged GLB in the enemy's `rig/` subdir and attaches it as a `Model` child, disabling the placeholder capsule renderer.
- **EnemyAnimatorBuilder** — clip loading prefers Tripo animation subdirs over brbmuffins/Blink pack; falls back gracefully if clips absent.
- **CombatAudio auto-assignment** — `BCE/Hub World/Wire Combat Assets` now runs `PatchCombatAudio()` (Step 6), finding or creating a CombatAudio component and assigning all 7 Retro Sci-Fi Pack clips. Idempotent — only assigns null slots.
- **AuthManager compilation fix** — `AuthManager` stub moved outside `#if !UNITY_SERVER` in `InventoryManager.cs` so it compiles when Build Target is Dedicated Server. Resolves CS0103 across RodNetworkAuthenticator, LoginManager, CharacterSelectManager, PlayerProgressManager.
- **Editor builder guards** — `HubPlayableBuilder`, `HubSceneBuilder`, `RodPrefabBuilder`, `EnemyBuilder` updated to guard client-only type references (`CombatAudio`, `ResourceNode`, `PlayerAnimator`) with `#if !UNITY_SERVER` so they compile under Dedicated Server build target. Fixed `MeshFilter.enabled` CS1061 (MeshFilter has no `.enabled`).
- **Credential safety** — `.gitignore` now blocks `.env`, `*.local`, `*secret*`, `*api-key*` patterns.

### 2026-06-28 — Stability & networking bug-fix pass

- **Abilities** — `isLocalPlayer` guard prevents remote clones processing local input
- **Position save** — floats use `InvariantCulture`; fixes silent failure on `,` decimal locales
- **Spawning** — `OnCreatePlayer` rejects duplicate `CreatePlayerMessage`
- **Enemy AI** — drops dead/downed targets on acquire
- **Status effects** — re-applying refreshes magnitude/source
- **Damage redirect** — self-guard + re-entrancy guard eliminates infinite redirect loop
- **Chat** — server logs `[CHAT] <user>: <msg>`

---

## Known TODOs

| Priority | Item |
|----------|------|
| High | Arena scene — NavMesh not yet baked; WaveSpawner needs enemy prefabs wired in Inspector |
| High | Enemy Animator Avatar — must be set manually in Inspector after running `4a–4c` |
| High | `NetworkManager.spawnPrefabs` — Enemy_Grunt, Ranged, Elite, WorldItem must be added manually |
| Medium | Class abilities — `AbilityCaster.cs` has all 32 defined but per-hero wiring incomplete |
| Medium | `orientation:F3` — client still sends float as formatted string in `PATCH /character/position` |
| Medium | Ability icons — colour-coded placeholder squares; real sprite atlas needed |
| Medium | Profession XP — not yet persisted to `professions` table |
| Medium | Stale prefabs — `Engineer.prefab`, `Guardian.prefab`, `Wraith.prefab`, `Medic.prefab` in Prefabs/ |
| Low | Arcanist missing from CharacterSelect 3D preview |
| Low | Hit VFX — no impact sparks on melee connect |
| Low | Footstep SFX — silent |
| Low | Brandalf (6th hero model in `Assets/Game/Heroes/Brandalf/`) — DECISION PENDING: new class vs skin |

---

## Project Structure

```
Assets/Game/
  Abilities/Scripts/     deployables + ability behaviours
  Characters/Scripts/    class passives, NPC controller, ability pools
  Characters/Enemies/    Tripo AI models — Grunt / Ranged / Elite (rig + 6 FBX clips each)
  Combat/Scripts/        Health, EnemyController, WaveSpawner, WorldBossController,
                         StatusEffectManager, DropTable, WorldItem, CombatAudio, ResourceNode
  Networking/            RodNetworkManager, RodNetworkAuthenticator, PlayerIdentity,
                         PortalTransition, RodChatManager, ForgeNPC
  Systems/               InventoryManager, ItemCatalog, HeroMastery, AuthManager stub
  UI/                    HUDs, panels, GmConsole, LoginManager, PlayerProgressManager
  Editor/                BCE menu builders — EnemyBuilder, HubPlayableBuilder,
                         HubSceneBuilder, RodPrefabBuilder, EnemyAnimatorBuilder
  Scenes/                LoginScene(0), CharacterSelect(1), Hub(2)
  Prefabs/               5 hero prefabs + Enemy_Grunt/Ranged/Elite + WorldItem
  Heroes/Brandalf/       6th-hero model — DECISION PENDING
_CONTEXT/                server/API docs, VPS ops
tools/                   build-server.ps1, deploy-server.sh
```
