# ROADMAP.md — Crossworlds BCE, Phased Work Plan

**Date:** 2026-07-03 · Companion to [SNAPSHOT.md](SNAPSHOT.md) (read it first — it defines terms and file locations).
The previous week-by-week status tracker was preserved at `_CONTEXT/ROADMAP_WEEKS.md`.

**Governing ethos — minimum change.** Every task below modifies the least code necessary.
No rewrites of working systems: the Mirror combat core, the Node API, and the old gear
endpoints are working — tasks extend or wire them, never restructure them. If a task
seems to require touching more than its listed files, stop and re-scope.

**Task format.** Each task is sized for one fresh Sonnet session with zero prior
conversation: it names its files, acceptance criteria, and dependencies. Tasks marked
**⚠ DECISION** need an answer from the project owner first (questions collected at the end).

---

## Phase 0 — Repo Stabilization

*Entry:* now. *Exit:* clean `git status`, one canonical doc set, no secrets in git.
*Why first:* ~4,000 lines of uncommitted work is the single biggest data-loss and
"which version is real?" risk. Everything later assumes a trustworthy tree.

> **Status 2026-07-03:** 0.1 ✅ (11 topical commits; local main is ahead of origin and
> 1 behind — pull/merge then push). 0.2 ✅ working tree (VPS rotation still owner action).
> 0.3 ✅ (52 `.bak` diff-verified as pre-Unity-6-migration snapshots, tombstones +
> fuse_hidden artifacts removed). 1.2 ✅ code side (`networkedPrefabs` array — Inspector
> assignment still an editor step). 1.4 ✅ (hooks moved to client-side `OnStartClient`,
> which also covers dedicated-server clients). 1.5 ✅. `.gitattributes` LF rules added.
> 0.4 ✅ — root `CLAUDE.md` is now the canonical agent context (session playbook:
> ground rules, CLI can/cannot, verification bar, how to pick tasks); the other two
> CLAUDE.md files defer to it.
> **5.1 pipeline ✅ (2026-07-03):** remote merged (Hub.unity union-merged — hub-combat
> objects + geography/wisps both kept), headless Linux server build SUCCEEDS via
> `tools/build-server.ps1` (Unity 6000.4.10f1, IL2CPP, 1.9GB) — this also
> build-verified all C# changes. Deploy via `tools/deploy-server.sh`.
> **2.1 ✅ / 2.2 ✅ / 2.3 ✅ (2026-07-03):** ServerConfig, SceneNames, ApiContractTests — all
> IP literals and scene-name strings centralized; contract test menu item added.
> **1.3 session wiring ✅ / 1.5 ✅ (2026-07-03):** WaveSpawner and HubReturnTrigger wired
> to ArenaSessionController; GmConsole and all client-only UI/Systems scripts guarded with
> `#if !UNITY_SERVER` (InventoryManager, FloatingDamageText, plus 8 UI files committed
> earlier). AbilityCaster FloatingDamageText call-sites individually guarded.
> **Server-build audit + combat/equip APIs (2026-07-03):** Full cross-reference sweep fixed
> — PlayerProgressManager guard narrowed to Bootstrap only (class stays server-compilable);
> ResourceNode, WorldItem, WorldBossController, PlayerIdentity individually guarded.
> EnemyController: added `enemyTemplateId` field + `PostCombatKill` coroutine — on death
> posts hit+kill to /api/combat/kill, refreshes PlayerProgressManager from server.
> InventoryManager: added `OnItemEquipped` + `PostEquip` coroutine (POST /api/inventory/equip).
> ItemData: added `id` field for DB item_id bridge. InventorySlot/EquipmentSlot wired.
> **Editor steps needed:** assign `enemyTemplateId` on enemy prefabs; assign `id` on ItemData SOs.
> **URL / AuthManager sweep (2026-07-03):** HeroMasteryManager, HeroCosmeticApplier,
> PlayerProgressManager, CharacterSelectManager, CraftingUI, InventoryBagUI,
> RodNetworkAuthenticator — all now prefer AuthManager.Token/CharacterId with PlayerPrefs fallback.
> Zero remaining raw `PlayerPrefs.GetString("jwt_token")` calls (all are now fallbacks only).
> XpBar (3.1) and LevelUpScreen code-complete and confirmed wired to PlayerProgressManager.
> 3.1 ✅ code-side. 3.2 code-side complete (stat recalc chain verified). 3.4 code-side complete.
> **HeroMasteryManager:** now accepts AuthManager creds before PlayerIdentity spawns;
> RetryApplyBonuses coroutine re-applies when CharacterStats appears post-spawn.
> **1.4 ✅ code-side:** CombatSessionTracker fully wired — WaveSpawner notifies via Rpc,
> PlayerIdentity.OnStartLocalPlayer calls NotifyAllySpawned, ArenaAutoStarter calls BeginSession().
> **Blocked on owner auth:** `git push` (~50 commits ahead) and `git lfs pull`
> (2 missing LFS objects: Wisp_Mob.prefab, grass.png) need GitHub Desktop auth.

- **0.1 — Commit the working tree in reviewable slices.** Files: everything in `git status`.
  Group commits by system (prefabs+scenes / combat scripts / UI / networking / docs); do not
  squash into one blob. Add `.gitattributes` with `*.cs text eol=lf`, `*.unity -text`,
  `*.prefab -text` to stop the CRLF churn inflating every diff.
  *Accept:* `git status` clean; `git log` shows ≥4 topical commits. *Deps:* none. **READY**

- **0.2 — Purge credentials from tracked docs.** Files: `_CONTEXT/CLAUDE.md` (§Credentials),
  `_CONTEXT/VPS_SERVER.md` (check), `CrossWorlds/CLAUDE.md`. Replace literal MySQL/dashboard
  passwords with "see `/opt/*/.env` on the VPS". Rotating the exposed passwords on the VPS is
  a manual owner step — flag it, don't do it from Unity-side sessions.
  *Accept:* `git grep` for the leaked passwords returns nothing in the working tree.
  *Deps:* 0.1. **READY** (rotation itself is owner action)

- **0.3 — Delete dead files.** Files: all 34 `Assets/**/*.cs.bak`, tombstone stubs
  `Assets/Game/Combat/{EnemyController,WorldBossController,EnemyProjectile,DropTable,WaveSpawner,WorldItem,Health}.cs`
  (+ their `.meta`). Diff each `.bak` against its live twin first — if a `.bak` is newer or
  divergent, surface it instead of deleting.
  *Accept:* zero `.bak` under `Assets/`; Unity compiles. *Deps:* 0.1. **READY**

- **0.4 — Consolidate to one CLAUDE.md.** Files: create root `CLAUDE.md` as the single source
  (merge `_CONTEXT/CLAUDE.md` + `CrossWorlds/CLAUDE.md`, keeping the newer endpoint list and
  Warden/Ironclad hero names); shrink the other two to a one-line pointer. Resolve the
  `rod_online` vs `crossworlds` DB-name contradiction by checking the VPS `.env` (or list it
  as "verify on VPS"). *Accept:* exactly one full CLAUDE.md. *Deps:* 0.2. **READY**

- **0.5 — Reconcile the two client stacks. ⚠ DECISION** The repo's `EnemyController`/`Health`/
  `InventoryManager` vs the VPS `unity-scripts/` set (`EnemyAI`, `PlayerHealth`, `ApiClient`,
  `CraftingManager`, `EnemyTemplateRegistry`…). Recommendation: repo Mirror-native scripts win
  for combat; pull `ApiClient.cs` + `CraftingManager.cs` down from the VPS (nothing in-repo does
  crafting HTTP). Owner must confirm before any deletion. *Accept:* decision recorded in
  CLAUDE.md; needed VPS scripts copied into `Assets/Game/`; losers archived outside `Assets/`.
  *Deps:* 0.1, 0.4.

---

## Phase 1 — Make the Core Loop Actually Run

*Entry:* Phase 0 exit. *Exit:* two clients can log in → hub → portal → arena → survive waves
→ loot drops → pickup persists → return to hub, on a headless server build, no console errors.
Highest-value phase: every piece exists, only glue is missing.

- **1.1 — Build the first arena scene.** Files: new `Assets/Game/Scenes/Arena_Copper.unity`,
  Build Settings. Use existing editor tooling (`Assets/Game/Editor/RodCombatWorldBuilder.cs` /
  BCE menu) rather than hand-building: ground, NavMesh bake, 4 spawn points, `WaveSpawner`
  (BCE Setup 4e), `ArenaSessionController`, `HubReturnTrigger`
  (`Assets/Game/Scene/HubReturnTrigger.cs`), lighting. *Accept:* scene in Build Settings;
  NavMesh baked; hosting in-editor and entering the arena spawns wave 1. *Deps:* 0.3. **READY**

- **1.2 — Register networked prefabs.** Files: NetworkManager object in `Hub.unity` (or code
  registration in `RodNetworkManager.OnStartClient`, mirroring the existing classPrefabs
  pattern). Add `Enemy_Grunt`, `Enemy_Ranged`, `Enemy_Elite`, `WorldItem`, boss prefab to
  `spawnPrefabs`. *Accept:* no "Could not spawn assetId" on a client when waves start.
  *Deps:* 1.1 to verify. **READY**

- **1.3 — Wire PortalTransition end-to-end.** Files: `Hub.unity` (portal object),
  `Assets/Game/Networking/PortalTransition.cs` (only if defaults need fixing),
  `Assets/Game/Scene/ArenaPortalTrigger.cs`. Arena-side trigger must call
  `WaveSpawner.StartWaves()` and `ArenaSessionController.BeginSession()`; `HubReturnTrigger`
  calls `EndSession()`. *Accept:* two clients warp together, waves run, both return to hub.
  *Deps:* 1.1, 1.2. **READY**

- **1.4 — Wire CombatSessionTracker (session-log leftovers).** ✅ code-side (2026-07-03)
  WaveSpawner.RpcNotifyEnemySpawned + RpcNotifyWaveComplete already call CombatSessionTracker.
  PlayerIdentity.OnStartLocalPlayer calls NotifyAllySpawned. ArenaAutoStarter calls BeginSession().
  CombatSessionTracker.PostSessionStats posts to /api/combat/session/end (guarded — logs warning on
  404 but doesn't crash). All hooks verified via grep.

- **1.5 — GmConsole server guard.** Files: `Assets/Game/UI/GmConsole.cs`. Wrap class body in
  `#if !UNITY_SERVER … #endif` (pattern already in `PlayerIdentity.cs:33`). This bug is listed
  in every status doc and is still unfixed. *Accept:* headless server log clean; client
  unchanged. *Deps:* none. **READY** (tiny; bundle with 1.2)

- **1.6 — Loot persistence smoke test.** Verification task; expect zero code changes. Kill
  enemies in Arena_Copper, confirm `WorldItem` spawns via `DropTable`, pickup calls
  `InventoryManager.OnItemPickedUp` → `POST /api/inventory/save`, item survives relog. Fix
  only what the test exposes, minimally. *Accept:* written test log; item persists.
  *Deps:* 1.1–1.4. **READY**

- **1.7 — Place HangmanNPC in Hub.** Files: `Hub.unity`. Component + dialogue UI already exist
  (`Assets/Game/NPC/HangmanNPC.cs`). HangmanDialogueUI is self-bootstrapping.
  **Editor step:** BCE → Hub Setup → 9 - Place HangmanNPC (Arena Entrance) → Ctrl+S.
  *Accept:* E-interaction opens dialogue; arena entry path works. *Deps:* 1.3. **READY**

---

## Phase 2 — Configuration & Fragility Fixes

*Entry:* core loop runs. *Exit:* one place to change server address; JSON layer has a smoke test.

- **2.1 — Central server config.** Files: new `Assets/Game/Systems/ServerConfig.cs` (static
  class exposing `GameServerAddress` / `AuthBaseUrl`, honoring the `PlayerPrefs("serverIP")`
  override `InventoryManager` already uses); edit the live scripts hardcoding `15.204.243.36`:
  `RodNetworkManager`, `RodNetworkAuthenticator`, `LoginManager`, `CharacterSelectUI`,
  `CharacterSelectManager`, `Editor/RodProjectSettings`, `Editor/RodEditorSetup`, plus
  serialized fields in `LoginScene.unity`. Keep Inspector fields as overrides defaulting to
  config — no behavior shift. *Accept:* `git grep 15.204.243.36 -- 'Assets/Game/**/*.cs'` → only
  ServerConfig; login + connect still work. *Deps:* Phase 1. **READY**

- **2.2 — Scene-name constants.** Files: new `Assets/Game/Systems/SceneNames.cs`; touch
  `RodNetworkManager.Awake`, `PortalTransition`, `HubReturnTrigger`, `ArenaPortalTrigger`.
  *Accept:* no scene-name string literals left in those files. *Deps:* 1.1. **READY**

- **2.3 — API contract smoke tests.** Files: new `Assets/Game/Editor/ApiContractTests.cs` —
  an editor menu command hitting `/api/health`, `/api/items`, `/api/inventory/:id`,
  `/api/enemies` and asserting `JsonUtility` parses non-default values. Targets the class of
  bug that already burned time (orientation `F3` strings, NaN slot_index). *Accept:* menu
  command prints PASS per endpoint against the live VPS. *Deps:* 2.1. **READY**

- **2.4 — Duplicate scene cleanup. ⚠ DECISION** `Login.unity` and `LoginScene.unity` both
  exist; `RodNetworkManager` hardcodes `LoginScene`. Confirm `Login.unity` is dead, then
  delete. *Deps:* owner confirm.

- **2.5 — Ambient NPCs are damageable but not networked. PARTIALLY RESOLVED 2026-07-15.**
  `Health` is a `NetworkBehaviour`, but `Editor/FieldGhoulSetupBuilder.cs` deliberately strips
  `NetworkIdentity` from the Field Ghouls (its comment: NetworkIdentity hides scene objects when
  no server is running) while still adding `Health`. Consequences: (a) Mirror logs
  `Health on <name> requires a NetworkIdentity` at load/import; (b) damage is client-local with
  no server authority, so each client kills their own copy — a Mirror-discipline violation, not
  just console noise.

  **Ogre: done.** Owner reversed the earlier defer and chose to network the O'gar Brute
  (`3D Models/Enemies/Ogres/O'gar Brute/Idle.prefab`, placed in Darkwood). New builder
  `Editor/OgreNetworkSetupBuilder.cs` → **BCE/Setup/4o** adds NetworkIdentity +
  NetworkTransformUnreliable (ServerToClient) + NetworkAnimator (server authority), matching the
  Enemy_Grunt/Ranged/Elite config in `NetworkSyncFixer` (4n). No AI changes were needed:
  `FieldGhoulNPC` already gates Update/WanderLoop/OnDamagedBy behind `CanRunServerSide()`, so the
  NavMesh wander runs server-only and won't fight the NetworkTransform. *Editor steps:* run 4o,
  then **open Darkwood.unity and save it** (Mirror bakes the scene-object sceneId on save — the
  placed ogre won't spawn for clients otherwise), then rebuild/redeploy the server.

  **Hub ghouls: still open. ⚠ DECISION.** They keep the strip-identity shape and remain
  damageable-but-unnetworked. Options: give them the 4o treatment (accept editor-hidden without
  a server), or split a non-networked `DummyHealth` for pure scenery. Deliberately not touched —
  `FieldGhoulSetupBuilder` is a working system and the owner asked only for the ogre.
  *Deps:* owner decision.

- **2.6 — GUID churn from out-of-editor asset moves. DONE 2026-07-15 (needs editor confirm).**
  *Culprit:* commit `b0aec3c1` "aw_assets buidout" (Todd King, 2026-07-15 15:44), merged in via
  `cdf4bb83` — it regenerated **124 `.meta` GUIDs** under `Assets/TripoModels/Incoming Batch/`,
  dangling every reference authored against the old ones. This is why Darkwood and LoginScene
  "worked yesterday" and broke today. (`527fb6b7` did the same earlier; guids have ping-ponged
  across several commits, so *always* diff against the specific pre-churn commit.)
  *Repair:* restored the pre-churn GUIDs into the current metas — 33 files, verified safe on five
  conditions each (guid actually churned; current meta still holds the churn guid; old-vs-current
  meta differs **only** on the guid line, so importer settings and internal fileIDs are identical;
  no current meta owns the old guid; nothing references the new guid; the old guid *is* actually
  referenced). The other 88 churned metas were skipped as harmless — nothing referenced their old
  guids. Script kept at `tools/guid_restore.py`; re-point `BEFORE`/`AFTER` at the next churn pair
  and dry-run before `--apply`.
  *Prevention:* move assets from inside Unity's Project window, or move the `.meta` with the file.
  Worth telling Todd — an out-of-editor reorganization will do this again.
  *Accept:* open Darkwood + LoginScene with a cleared console → no "Missing Prefab" errors.

- **2.7 — The ability deployables were never built. ⚠ BLOCKS "see AOE spells in the world".**
  Discovered 2026-07-15 while verifying 4d, and it corrects an earlier claim that this was
  "code done, pending one editor step". The CODE is done and committed (`DeployableNet`,
  `AbilityCaster.SpawnDeployableAt` → `NetworkServer.Spawn` guarded on NetworkIdentity, all 7
  behaviours gated to authority). **The PREFABS do not exist.** Evidence:
  - No prefab anywhere references ShockMineBehaviour / SingularityBehaviour / LastBastionWall /
    NullFieldZone / RestorationBeacon / NaniteSwarmBehaviour / TurretController. 4d searches for
    prefabs carrying those behaviours, so it finds and fixes nothing.
  - Every dedicated slot on AbilityCaster is null (`{fileID: 0}`): shockMinePrefab,
    naniteSwarmPrefab, singularityPrefab, eventHorizonPrefab, lastBastionPrefab, nullFieldPrefab,
    beaconPrefab, phaseRelayPrefab, shadowRelayPrefab.
  - The `ability.deployablePrefab` fallbacks that ARE set (13 of 36 on Arcanist) point at raw
    brbmuffins VFX: Magic circle, Freeze circle, Healing circle, Death magic circle, Ground
    spikes, Mana wall — all `Identity=0`, and all but one `scripts=0`. `SpawnDeployableAt` does
    `go.GetComponent<ShockMineBehaviour>()` and silently gets null, so they are cosmetic circles
    with no gameplay and no replication. `turretPrefab` points at an FBX, not a prefab.

  *Consequence:* deployable abilities currently spawn a decorative circle on the server that no
  client ever sees. Cast VFX, cast anims, hit VFX and damage DO replicate (RpcCastConfirmed /
  RpcPlayHitVFX / server-side ApplyCircle etc.), so the spells still read as "working" in play.

  *Task:* build one prefab per deployable type — VFX visual (reuse the brbmuffins circle already
  referenced) + the matching behaviour + NetworkIdentity + trigger collider — then assign them to
  the AbilityCaster slots on the 5 hero prefabs and register in RodNetworkManager. Scriptable as a
  builder (`SerializedObject` can write the Inspector refs); needs design input on radius/duration
  per deployable. *Accept:* two clients both see a mine/wall/zone appear and it damages/blocks.
  *Deps:* none — 4d already handles the identity step once the prefabs exist. **READY**

---

## Phase 3 — Player-Facing Completion (legacy roadmap Weeks 5–7)

*Entry:* Phase 1 exit. Independent of Phase 2. *Exit:* playtest-presentable client.

- **3.1 — XP bar + level-up flow.** Files: `Assets/Game/UI/XpBar.cs`, `UI/LevelUpScreen.cs`,
  `UI/PlayerProgressManager.cs` (fetch/save already implemented). Wire into HUD; confirm
  `OnLevelUp` fires and `SaveProgress()` posts on level-up and hub return only (not per kill —
  server-doc convention). *Accept:* kills → XP bar moves → level-up screen → DB row updated.
  *Deps:* 1.6. **READY**

- **3.2 — Character sheet + equip stats.** Files: `UI/CharacterSheetUI.cs`,
  `Items/Scripts/{CharacterStats,EquipmentUI,EquipmentSlot,TooltipUI}.cs`, `UI/InventoryBagUI.cs`.
  Verify equip → `POST /api/inventory/equip` → stat recalc from `stat_bonus` JSON. *Accept:*
  equipping a seeded item changes displayed stats and persists across relog. *Deps:* 1.6. **READY**

- **3.3 — Crafting loop client.** ✅ code-side (2026-07-03 profession session)
  `ForgeCraftingPanel.cs` — Smelt + Craft tabs, progress bar, ingredient shortage highlight in red.
  `ConsumableEffect.cs` — hp_regen, resist_void, resist_blast, speed, damage_amp with duration.
  VPS: `POST /api/professions/award-xp`, `GET /api/professions/recipes/:charId`, `POST /api/craft`
  (extended: XP award, transaction, level-up loop, smelt support). SQL: 5 raw materials, 5 refined,
  6 consumables (Void Resist Flask, Iron Warden Blast Kit, etc.), 8 crafted gear items, 19 recipes.
  Server patched and live (2026-07-03).
  *Editor steps remaining:* wire ForgeNPC → `ForgeCraftingPanel.Open()`; build RecipeRowPrefab UI
  prefab; add `GetItemCount(itemId)` to `InventoryManager`.
  *Accept:* craft copper ingot from 3 ore in-game; Void Resist Flask reduces boss void damage 25%.
  *Deps:* 0.5, 1.6.

- **3.4 — Combat feedback polish.** Files:
  `Combat/Scripts/{FloatingDamageText,EnemyDeathVFX,EnemyHealthBar}.cs` + VFX prefab hookups on
  enemy prefabs (fields already exist on `EnemyController`). Client-side only — keep inside
  `#if !UNITY_SERVER`. *Accept:* damage numbers on hit, VFX on death, server build unaffected.
  *Deps:* 1.1. **READY**

- **3.5 — Brandalf. ⚠ DECISION** Sixth hero (prefab, `classPrefabs[5]`, server `CLASS_NAMES` +
  validator bump, `PlayerIdentity.ClassNames`, ability kit, CharacterSelect entry) or Arcanist
  model swap (swap model on the Arcanist prefab only)? Current wiring
  (`CharacterSelectManager.cs:106`) treats Brandalf as the Arcanist's preview model. Sixth-hero
  scope is ~5× larger and touches the Node server. *Deps:* owner decision.

- **3.6 — TutorialIsland scope. ⚠ DECISION** Scene exists with Brandalf content; absent from
  every documented flow. In or out for the playtest? *Deps:* owner.

---

## Phase 4 — Security Hardening (pre-playtest minimum)

*Entry:* Phase 1 exit. *Exit:* the documented alpha caveats are fixed or consciously accepted in writing.

- **4.1 — Server-side player HP. ⚠ DECISION (timing).** Docs defer this to Phase 2, but it's
  the biggest cheat vector (hacked client ignores all damage). Minimum-change option: `Health`
  exists and `EnemyController` already runs `[Server]` — route enemy damage through a server
  `Health` on player prefabs and sync HP down. Touches:
  `Combat/Scripts/{EnemyController,EnemyProjectile,Health}.cs`, the five class prefabs,
  `UI/PlayerHealthBar.cs`. *Accept:* a client that suppresses local damage still dies.
  *Deps:* 1.x stable; owner call on before/after playtest.

- **4.2 — Progress-save sanity caps (server side).** Files: `/opt/crossworlds-auth/server.js`
  (VPS, not this repo — run as a server session per CLAUDE.md process). Reject `save-progress`
  deltas beyond plausible bounds (level jumps, gold spikes cross-checked against
  `gold_transactions`). Validation only, no schema change. *Accept:* curl with absurd values →
  400; normal play unaffected. *Deps:* VPS access. **READY**

- **4.3 — Arena session tokens.** Explicitly deferred to Phase 2 by project docs — keep
  deferred; listed so it isn't lost.

---

## Phase 5 — Playtest Readiness (legacy roadmap Week 8)

*Entry:* Phases 1–3 done, Phase 4 decided. *Exit:* 10–20 player playtest executed.

- **5.1 — Headless server build + deploy check.** Files: `Assets/Game/Editor/BuildScript.cs`.
  Build Linux server, verify the auto-start path (`RodNetworkManager.Start`,
  GraphicsDeviceType.Null branch), deploy per CLAUDE.md, tail `/var/log/crossworlds.log` clean
  for 10 minutes. **READY**
- **5.2 — Client build + download page refresh.** Zip → `/var/www/*/downloads/`. **READY**
- **5.3 — Uptime Kuma setup** (port 3001, unconfigured). Owner/VPS task.
- **5.4 — Stress script:** N headless clients or a bot loop hammering `/api/combat/*` to
  validate the hit-gate/cooldown under load. *Deps:* 5.1.

---

## Phase 6 — Multi-Zone Persistent World (added 2026-07-23)

*Entry:* Phase 1 exit. *Exit:* several zones resident on one server simultaneously; each player
moves between them independently; players in Darkwood cost nothing to a player in Hub; an empty
zone is unloaded entirely.

**The problem.** The server is single-scene-at-a-time. `ServerChangeScene`
(`WaypointMapTrigger.cs:298`, `HubReturnTrigger.cs:201`, `GmCommandRouter.cs:144`,
`3D Models/Enemies/HangmanNPC.cs:148`) yanks *every* connected player into the destination when
one person travels. `PortalTransition.cs:88` works around this with a client-local
`SceneManager.LoadScene` that never tells the server — so the client is in the arena while the
server still has that player's identity in Hub, observing Hub objects. There is also **no interest
management configured at all**: Mirror ships five IM components under
`Assets/Mirror/Components/InterestManagement/`, none is referenced by `Assets/Game` and none
appears in any scene YAML, so every client observes every spawned object on the server.

**The design.** The active scene becomes an empty *container* holding only the NetworkManager.
Every zone — Hub included — is loaded additively on top of it, server-side, on demand. Players are
filed into their zone with `SceneManager.MoveGameObjectToScene` (server only) and told to load it
with a per-connection `SceneMessage { sceneOperation = SceneOperation.LoadAdditive }`.
`SceneDistanceInterestManagement` then scopes observers by scene, then by distance within a zone.
Reference implementation already in-tree:
`Assets/Mirror/Examples/MultipleAdditiveScenes/Scripts/MultiSceneNetManager.cs`.

**Scale ceiling — read before planning around this.** One Mirror process = one world. This design
lets that process host many zones with independent players; it does **not** spread load across
machines. The binding constraint is server-side mob simulation (a `NavMeshAgent` per mob ticks
whether or not anyone is nearby), so the realistic ceiling is tens of concurrent players. True
sharding (zone servers + gateway + cross-process handoff) is a separate, much larger project and
should not be built until players actually hit the limit — per-zone scenes are the seam to cut
along later. The same mechanism also yields instanced content free: VoidDungeon becomes N additive
copies of one scene rather than one shared copy.

- **6.1 — Scene-aware spawning audit.** `Object.Instantiate` places objects in the *active* scene,
  which becomes the empty container in 6.3 — so every server spawn would land outside its zone and
  interest management would file the entire world under "container". Add
  `SceneManager.MoveGameObjectToScene(obj, <owner>.gameObject.scene)` immediately before each
  `NetworkServer.Spawn`. Call sites: `Combat/Scripts/WaveSpawner.cs:240`,
  `Combat/Scripts/EnemyController.cs:678` (projectile) and `:817` (drop),
  `Combat/Scripts/WorldBossController.cs:287` (shard) and `:544` (drop),
  `Combat/Scripts/IronWardenController.cs:273` (turret) and `:407` (drop),
  `Combat/Scripts/NullArchitectArenaStarter.cs:51`, `UI/AbilityCaster.cs:3717` (deployable) and
  `:4050` (turret). `RodNetworkManager.cs:170` (ChatManager) is deliberately excluded — see 6.5.
  Player spawns at `RodNetworkManager.cs:251` and `:410` are handled by 6.3.
  *Accept:* every `NetworkServer.Spawn` in `Assets/Game` is preceded by an explicit scene
  placement or a comment naming why it is exempt; behavior unchanged under the current
  single-scene setup (this task is a deliberate no-op today). *Deps:* none.
  **✅ code-side 2026-07-23.** New `Networking/ZoneScene.cs` (`PlaceWith(obj, owner)` /
  `PlaceIn(obj, scene)`) centralizes the rule — root-object check, scene validity, DDOL skip,
  and an early-out when the object is already in the right scene, which is what makes it a
  no-op today. All 10 real call sites now call it immediately before their `Spawn`;
  `RodNetworkManager.cs:174` (ChatManager) carries an inline comment naming why it is exempt
  and cross-referencing 6.5. **Not compile-verified** — open the editor once to confirm.

- **6.2 — Zone persistence is currently fake.** `Networking/RodPositionSaver.cs:67` hardcodes
  `"map":"GameWorld"` into the `PATCH /character/position` body. The DB column exists but every
  character claims to be in the same nonexistent map, so "which zone was I in?" is not persisted —
  log out in Darkwood and the server has no idea where to put you back. Second defect in the same
  file: saves fire only from `OnDestroy` and `OnApplicationQuit` (`:24-25`), so a crash or OOM kill
  loses every online player's position. Fix: send the player's real zone scene name, and add a
  periodic save tick (30–60s) plus save-on-zone-change. Companion VPS-side work is written up in
  `_CONTEXT/HANDOFF_zone_persistence.md` — run that as a server session per CLAUDE.md.
  *Accept:* log out in zone A, log back in, spawn in zone A at the saved coordinates; `kill -9` the
  server and lose at most one save interval. *Deps:* VPS handoff for the API half.
  **✅ client half 2026-07-23.** `RodPositionSaver` rewritten: reports the player object's real
  scene instead of the `"GameWorld"` literal, periodic save every 45s (staggered per player so
  simultaneous joins don't burst the auth server), and a public `SaveNow()` for 6.4 to call before
  a zone change. `SceneNames` gained `Zones` / `IsZone` / `NormalizeZone` — unknown and legacy
  values collapse to Hub so a bad row can never strand a player in a nonexistent scene.
  `CharacterResponse.map` is parsed and lands on `RodPlayerAuth.zone`; a non-zone stored value
  logs a warning naming the VPS backfill as the fix. Deliberately still PATCHes the existing
  `/character/position` body field `map` (Unity has always sent it) rather than depending on the
  new `/api/character/:id/zone` endpoint — so the client half is useful before the VPS half lands
  and breaks nothing if it never does. **Still open:** nothing consumes `RodPlayerAuth.zone` at
  spawn time — the server is single-scene until 6.3, so a saved zone cannot be honored yet.
  **Not compile-verified.**

- **6.3 — Container scene + ZoneManager.** The core task. New empty
  `Assets/Game/Scenes/_Container.unity`; `RodNetworkManager.cs:63` `onlineScene` points at it
  instead of `SceneNames.HubPath` (`offlineScene` stays — Mirror's disconnect navigation depends on
  it). New `Assets/Game/Networking/ZoneManager.cs` owning: additive load with
  `LoadSceneParameters { loadSceneMode = Additive, localPhysicsMode = LocalPhysicsMode.Physics3D }`,
  a player ref-count **keyed on scene handle, not scene name** (open question 8 — instanced
  dungeons mean several live scenes share a name), `SceneMessage` LoadAdditive/UnloadAdditive per connection,
  `MoveGameObjectToScene` for the player object, and `UnloadSceneAsync` when a zone's count hits
  zero. `RodNetworkManager.OnServerSceneChanged` / `PlaceHubReturnPlayers` /
  `RespawnMissingPlayersAfterSceneChange` (`:277-352`) exist only to repair the mass-teleport and
  are deleted. *Accept:* two clients in two different zones simultaneously, each seeing only their
  own zone's objects; the last player leaving a zone unloads it (confirm via server log).
  *Deps:* 6.1, 6.2.
  **✅ code-side 2026-07-23. ⚠ NOT RUNNABLE UNTIL 6.4 — see warning below.**
  New `Networking/ZoneManager.cs`: additive load with `LocalPhysicsMode.Physics3D`,
  occupancy keyed on **scene handle** (instanced dungeons share a name), per-connection
  `SceneMessage` Load/UnloadAdditive, `MoveGameObjectToScene`, and a delayed unload
  (`unloadDelaySeconds`, default 30) so portalling out and straight back doesn't thrash a
  scene load. `PrepareZone` is public so the initial spawn and later zone changes share one
  path. `RodNetworkManager.onlineScene` → `SceneNames.ContainerPath`; `OnCreatePlayer` is now
  the `SpawnPlayerIntoZone` coroutine that spawns into the player's saved zone (6.2's
  `RodPlayerAuth.zone` is finally consumed); `OnServerDisconnect` frees the zone slot.
  `OnServerSceneChanged` + `PlaceHubReturnPlayers` + `RespawnMissingPlayersAfterSceneChange`
  + `SpawnPlayerForSceneChange` deleted (157 lines) along with two helpers they orphaned.
  `HubReturnSpawnPoint.FindInScene` added — scene-scoped, and deliberately without the
  `GameObject.Find` fallbacks, which are global and would reintroduce the cross-zone bug
  (part of 6.5 pulled forward because ZoneManager needs it).
  Three Mirror behaviours this depends on, verified against `Mirror/Core/NetworkManager.cs`
  rather than assumed: clients instantiate spawned objects into their ACTIVE scene, so an
  additive zone scene on a client holds only geometry and unloading it cannot destroy
  networked objects; `ClientChangeScene` returns early when `NetworkServer.active`, so a host
  client never processes its own `SceneMessage`; and `NetworkClient.isLoadingScene` pauses
  message processing during the load, so spawn traffic queues rather than being lost.
  Used `yield return null` rather than the Mirror example's `WaitForEndOfFrame`, which can
  fail to resume in headless batchmode — the production path.
  **Editor step: BCE ▶ Setup ▶ 6z** (new `Editor/MultiZoneSetupBuilder.cs`) creates
  `_Container.unity`, registers every zone in Build Settings, and adds ZoneManager to the
  RodNetworkManager GameObject. **Nothing works until 6z is run.** **Not compile-verified.**

> **✅ RESOLVED 2026-07-23 by 6.4.** No live `ServerChangeScene` calls remain in `Assets/Game`
> (`grep -rn "ServerChangeScene(" --include=*.cs Assets/Game | grep -v "//"` → empty). 6.3 and
> 6.4 landed together as intended.

- **6.4 — Route all travel through ZoneManager.** Replace the four `ServerChangeScene` call sites
  (`Scene/WaypointMapTrigger.cs:298`, `Scene/HubReturnTrigger.cs:201`,
  `Networking/GmCommandRouter.cs:144`, `3D Models/Enemies/HangmanNPC.cs:148`) with
  `ZoneManager.MovePlayerToZone(conn, zoneName, spawnId)`. Delete the client-local `LoadScene` hack
  in `Networking/PortalTransition.cs:82-89` — `TargetBeginTransition` becomes a request into the
  same path. *Accept:* one player takes a portal or waypoint and nobody else's view changes;
  the traveller keeps their identity, inventory, and HP across the move. *Deps:* 6.3.
  **✅ code-side 2026-07-23.** **Six** call sites, not four — the original audit missed
  `Scene/ArenaPortalTrigger.cs:106`, which also called `ServerChangeScene`. All now route through
  `ZoneManager.MovePlayerToZone`: WaypointMapTrigger, HubReturnTrigger, GmCommandRouter,
  HangmanNPC, ArenaPortalTrigger, PortalTransition. The client-local `LoadScene` hack in
  PortalTransition is gone — `TargetBeginTransition` is now cosmetics only (chat line + loading
  screen) and the move is server-authoritative.
  Three bugs fixed that only became reachable once travel actually worked:
  (a) `ZoneManager.PlaceAtSpawnPoint` was missing `NetworkTransformBase.ServerTeleport`, so a
  cross-zone jump would interpolate the player across the whole map on every client — found by
  reading the GM `/arrive` helper this task deleted, which had it right;
  (b) `PortalTransition._entered` / `_enteringLocally` were never cleared, so a portal worked
  exactly once per server lifetime — harmless when travel was broken, fatal now;
  (c) `HubReturnTrigger.EndArenaSessionIfNeeded` killed the arena session and stopped waves
  unconditionally, which with co-op instances would end a team's run when one member left. Now
  guarded on `ZoneManager.OccupantCount(sender) > 1` and the `WaveSpawner` lookup is scoped to
  the leaver's zone instead of `FindAnyObjectByType`.
  Dead code removed: `ChangeToHubScene`, `ChangeScene`, `PrepareHubArrival`,
  `TryPlaceSenderAtCurrentSceneSpawn`, `SceneMatchesCurrent`. `HubReturnArrival` survives only
  on WaypointMapTrigger's fully-offline path. **Not compile-verified.**

- **6.5 — The two traps that break silently.** (a) `Scene/HubReturnSpawnPoint.cs:17` uses a global
  `FindObjectsByType`, and `GetStartPosition()` searches all Mirror start positions — with every
  zone resident at once both return spawn points from the wrong map. Both need a `Scene` filter.
  (b) `RodNetworkManager.cs:169` marks the ChatManager `DontDestroyOnLoad`, which moves it to
  Unity's DDOL pseudo-scene. `SceneInterestManagement.OnCheckObserver` is strict scene equality
  (`identity.gameObject.scene == newObserver.identity.gameObject.scene`), so the DDOL scene matches
  no player, the ChatManager gets zero observers, and **chat goes silent for everyone**. Chat is
  global (open question 9), so the fix is a custom `SceneDistanceInterestManagement` subclass that
  force-adds every connection as an observer for identities flagged world-global — keep the single
  DDOL ChatManager. *Accept:* travel to a zone and land on that zone's spawn point; two players in
  different zones see each other's chat messages. *Deps:* 6.3.

- **6.6 — Enable interest management.** Add `SceneDistanceInterestManagement` to the
  RodNetworkManager GameObject and tune `visRange`; add `DistanceInterestManagementCustomRange` to
  bosses so they stay visible further out than a grunt. Only one IM component may exist per
  NetworkManager, so pick SceneDistance up front rather than starting with Scene and migrating.
  *Accept:* profiler/log shows a client in Hub receiving no Darkwood object spawns; mobs across a
  large zone stop arriving until approached. *Deps:* 6.3, 6.5. **Editor step** (component add +
  Inspector tuning).

- **6.7 — Offset each zone in world space. Editor step.** Legacy baked NavMesh from additive
  scenes merges into one navmesh; if two zones occupy the same coordinates, agents path between
  maps and colliders interpenetrate. `LocalPhysicsMode.Physics3D` fixes the physics half but not
  NavMesh. Since these are distinct hand-built maps rather than instances of one arena, give each
  zone a distinct world-space origin (Hub x=0, Darkwood x=10000, Ashen x=20000, …), move each
  map's root, and rebake. Cheaper and more debuggable than runtime machinery. *Accept:* no mob in
  zone A can path into zone B; nothing renders at a neighbouring zone's coordinates.
  *Deps:* 6.3. **⚠ Also affects 6.2** — saved coordinates are absolute, so this must land before
  players accumulate saved positions, or those positions need migrating.

- **6.8 — Open-world mob spawner.** `Combat/Scripts/WaveSpawner.cs` is an arena construct (waves,
  difficulty ramp, session tracking) and is the wrong shape for a persistent zone. New component:
  spawn points with per-mob respawn timers and a zone population cap, populated when the zone
  loads and torn down when it unloads. Do not modify WaveSpawner — arenas keep using it.
  *Accept:* enter Darkwood, kill a mob, it respawns on its timer; leave and return and the zone
  repopulates from scratch. *Deps:* 6.3. Needs combat-design input on density and respawn timing.

**Ordering.** 6.1 and 6.2 are independently safe and land first (6.1 is a no-op today). 6.3 is the
core and everything else waits on it. 6.7 should land before real players accumulate saved
positions.

**Bandwidth note.** Two known bugs go from "multiplayer is janky" to "the world does not function"
at this scale: hero and base-enemy prefabs are missing `NetworkTransform` (fix: BCE ▶ Setup ▶ 4n,
`Editor/NetworkSyncFixer.cs`), and ability deployables still lack prefabs entirely (task 2.7).
Neither is a Phase 6 task, but Phase 6 is not shippable with either outstanding.

---

## Standards note (from the audit brief)

UniTask, DOTween, and A* Pro are **not in the project** (see SNAPSHOT.md §0). Adopting UniTask
now would touch ~63 files for zero player value — **recommend against** during Phases 1–5;
revisit post-playtest only if coroutine churn becomes a real maintenance cost. That is the
minimum-change ethos applied to the standards themselves.

---

## Open Questions for the Owner (blocking the ⚠ tasks)

1. **Project identity:** the brief described "Corrosion / Entropy gear-decay / SQLite / A* /
   UniTask / six classes." None of that exists in this repo. Is that a future design direction
   to plan toward, a different project, or stale notes? If Entropy gear-decay is a real planned
   system, it deserves its own phase — say the word and it gets specced.
2. **Brandalf:** sixth hero or Arcanist model swap? (Task 3.5)
3. **Mastery/session endpoints:** do `/api/mastery/*` and `/api/combat/session/end` exist on
   the VPS? Shipped client code calls them, but no API doc lists them. (Task 1.4)
4. **Client stack winner:** confirm repo Mirror scripts over VPS `unity-scripts/` for combat,
   and pulling `ApiClient`/`CraftingManager` down? (Task 0.5)
5. **Server-side player HP:** fix before the playtest or accept for alpha? (Task 4.1)
6. **Dead files:** OK to delete `Login.unity` and archive the losing duplicate scripts?
   (Tasks 0.3 / 2.4)
7. **Credential rotation:** the MySQL and dashboard passwords printed in `_CONTEXT/CLAUDE.md`
   are in git history — rotate them on the VPS after task 0.2?
8. ~~**Which zones are shared vs instanced?**~~ **ANSWERED 2026-07-23: open zones shared,
   dungeons instanced.** Hub / Darkwood / Ashen Wastelands / Toujam Basin / GM Island /
   Gathering Zone are single shared copies; VoidDungeon and arenas get one additive copy per
   party. Consequence for 6.3: **ZoneManager must key on scene `handle`, not scene name** —
   instanced copies share a name, so a name-keyed ref-count would merge two parties' dungeons
   into one entry and unload a scene somebody is still standing in.
9. ~~**Chat scope?**~~ **ANSWERED 2026-07-23: global.** Chat spans all zones. The DDOL
   ChatManager therefore stays a single world-global instance, and 6.5's fix is the custom IM
   subclass that force-adds every connection as an observer for flagged global identities —
   not the one-ChatManager-per-zone option.
