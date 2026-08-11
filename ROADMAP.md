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

> **PVP zone night lighting (2026-08-10):** `PVPZONE` retains its existing Night Moon
> Mid skybox and now uses cool low-intensity moonlight, reduced ambient/reflection
> contribution, and subtle blue fog so it reads as night while preserving combat visibility.

> **Cat animation material wiring (2026-08-10):** The `catidle` and `catwalk` FBX
> material slots now share a URP cat material using the supplied black-cat base-color texture.

> **Necromancer animation material wiring (2026-08-10):** All ten Necromancer FBX
> models share a URP material using the supplied hooded-undead base-color texture.

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

> **Status 2026-07-26 (audit sweep):** The "~50 commits ahead / push blocked" note
> above is RESOLVED — CI (GitHub Actions) is the live build+deploy pipeline and the
> branch has been syncing. As of this audit local main is 2 ahead (audit commits) /
> 1 behind origin (`65138e5e` "Darkwood changes - Marauder VFX updates") — pull/push
> via GitHub Desktop, not CLI. `tools/deploy-server.sh` is RETIRED; CI deploys to
> numbered `/game/<runid>/` dirs (see CLAUDE.md). Unity is 6000.4.11f1 (not 6000.4.10f1).
>
> **Progression framework 2026-08-05:** Unity now refreshes the canonical character
> record after each accepted `/api/combat/kill`, uses the consistent
> `100 × targetLevel^1.5` display curve, and applies authenticated STR/AGI/INT/VIT to
> server-owned `CharacterStats` SyncVars at spawn. Mob XP is derived from enemy level
> (`10 × level + 5`) and a broad reward category: grunt ×1, brute ×1.5, elite ×2,
> boss ×5. Per-monster XP catalog rows are not required.
> Follow-up fixed the missing `/api/combat/hit` bridge (which caused the VPS hit gate
> to reject every kill), added accepted-reward `+XP` HUD feedback, and sends the
> server level/category plus a per-spawn network instance ID. Gunda is grunt; the
> Darkwood Ogre is brute. The matching auth-server contract still needs VPS deployment.
>
> **Spell Forge instant cast 2026-08-06:** `AbilityDef.instantCast` lets an authored
> spell skip the aim/confirm decision phase and snapshot the current cursor aim on
> hotkey press. Normal cast time, animation, cost, cooldown, server authority, and
> effects still apply; Spell Forge and player spellbook UI expose the delivery mode.
> The Animation tab also exposes a per-spell playback-speed multiplier and applies
> it consistently to live preview, ordinary casts, variants, and movement casts.
> Spell Forge crowd control now includes an authored Knock Up type with configurable
> height and duration; server-side target movement is synchronized to clients.
> Chargeable circle damage and its pulse sequence now share the committed visual
> radius, fixing Screaming Flames' hit area at larger charge sizes.
>
> **GM flight controls 2026-08-09:** Both authenticated `/fly` and the local GM
> console now use one Rigidbody flight mode in `PlayerMovement`. WASD provides
> horizontal flight, Space rises, Ctrl/C descends, Shift boosts, and releasing
> controls stops immediately instead of preserving airborne inertia.
>
> **GM cinematic free camera 2026-08-09:** Authenticated GMs can use `/freecam`
> to detach the active camera for recording while the player remains parked.
> Movement is inertia-free with vertical controls, fast and precision modifiers,
> configurable 0.25-100 units/second speed, and automatic return to the saved
> third-person orbit when disabled. Freecam hides all loaded screen-space and
> world-space Canvases for clean capture, restores their prior states on exit,
> suppresses the legacy IMGUI quest window, and provides a server-verified Escape
> shortcut so the hidden chat is not needed.
>
> **PVP zone framework 2026-08-10:** The saved `PVPZONE` scene is a registered,
> persistable additive zone and default world-map destination, with GM `/arrive pvp`
> support and a terrain-aware editor action for its arrival/respawn point. Existing
> Enemy-targeted player abilities, crowd control, deployables, projectiles, and turrets
> treat other players as opponents only when both players share PVPZONE. `Health`
> provides a final server-side player-damage gate everywhere else, and downed players
> in PVPZONE return to a random matching zone spawn after the normal respawn delay;
> map and GM arrivals use the same server-selected spawn pool.
>
> **Player run speed 2026-08-10:** Normal locomotion now uses the former sprint
> speed of 9 units/second on all five active class prefabs and future PlayerMovement
> defaults. Sprint remains at 9, preserving its input/animation hook without an
> additional speed increase. Player animation normalization and local GM speed
> scaling use the new authored run/sprint baselines.
>
> **Circle aim horizon fallback 2026-08-10:** Cursor rays aimed above the visible
> terrain no longer collapse circle indicators to minimum distance over the caster.
> The horizontal cursor direction is retained and the indicator clamps to the
> ability's maximum range, keeping blink/dash destinations stable at the horizon.
>
> **Dravos death import repair 2026-08-10:** `Standing React Death Backward` now
> remaps its embedded Tripo material to the tracked Dravos base-color texture through
> a URP/Lit material, and imports as Humanoid with its own avatar like the existing
> Dravos animation set.
>
> **Player hero death animations 2026-08-10:** Cleric, Marauder, and Arcanist now
> enter their class-specific death animation from the replicated `IsDead` downed
> state and return to idle after revival. Marauder's disabled `PlayerAnimator` was
> enabled; Arcanist received the missing driver and Dravos death controller state;
> Cleric's Brandalf death clip is Humanoid and its controller now enters on `IsDead`.
> The `#if !UNITY_SERVER` guards described in the 1.5 note above were the *bare* form;
> the required convention is `#if UNITY_EDITOR || !UNITY_SERVER`, and the 8 remaining
> bare guards (HeroCosmeticApplier, StatusEffectHUD, ShieldValueHUD, GoldHUD,
> ClericRadarUI, CharacterSheetUI, AfkStationBuilder, HubSceneBuilder) were rewritten
> by the audit — review-only, needs one editor open to confirm compilation.

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

- **6.7 — Zone spatial overlap. ⚠ RE-SCOPED 2026-07-23 — do not execute the original plan.**
  The original spec said: offset every zone in world space (Hub x=0, Darkwood x=10000, …), move
  each map's root, rebake. Measuring the actual scenes before doing that changed the answer.

  **What was measured.** The zones do overlap, heavily — footprints from scene YAML:
  HUB `x[-148..744] z[-8..498]`, Darkwood `x[-571..71] z[-457..292]`,
  Ashen Wastelands `x[-500..1000] z[-1500..586]`, VoidDungeon `x[-12..34] z[-12..87]`.
  All stacked around the origin. (Toujam Basin and GM Island report nothing under a plain
  `m_LocalPosition` scan — their roots are prefab instances, whose overrides serialize
  differently. That alone makes a scripted root-mover unreliable.)

  **But two of the three problems turned out not to exist:**
  - *Rendering overlap is not a client problem.* A client holds exactly ONE zone: ZoneManager
    sends `UnloadAdditive` for the old zone on every travel. Only the brief transition window
    has two, which is task 6.9's territory, not this one. The server is headless and renders
    nothing.
  - *Physics overlap is already solved.* ZoneManager loads every zone with
    `LocalPhysicsMode.Physics3D`, so each zone gets its own physics scene.

  **The one real remaining issue is NavMesh.** These zones use **NavMeshSurface**, not legacy
  scene bakes (`Assets/Game/Scenes/<Zone>/NavMesh-*.asset` — Darkwood 3.8 MB, Ashen 8.9 MB).
  Every loaded surface registers into one global navmesh via `NavMesh.AddNavMeshData`, so
  overlapping surfaces sharing an agent type let an agent path from one zone into another.

  **Cost of the original plan is much higher than specced:** moving roots invalidates every one
  of those multi-megabyte bakes (full rebake per zone, an editor operation a CLI session cannot
  run or verify), prefab-instance roots resist scripted moving, and float precision degrades
  meaningfully past ~10k units from the origin.

  **Options, cheapest first — needs an owner decision:**
  1. **Measure, then probably do nothing.** Mobs already leash: `EnemyController.leashRadius`
     and `EnemyWanderAI.leashRadius` default to 20 units, and `EnemyWanderAI` returns to origin
     past that. A mob would have to path 20+ units off its spawn to cross into another zone's
     surface, and leashing pulls it back first. Verify in play before spending anything.
  2. **Per-zone NavMesh agent type.** Give each zone's NavMeshSurface its own agent type ID so
     agents physically cannot traverse another zone's surface. No geometry moves, but still a
     rebake per surface plus setting `agent.agentTypeID` per zone at spawn.
  3. **The original world-space offset.** Correct and permanent, but the most expensive and the
     only one that invalidates saved player coordinates.

  *Recommendation:* option 1 — verify against a running server first. This is the one Phase 6
  task whose cost/benefit does not currently justify doing it. *Deps:* 6.3, and a playtest.
  **⚠ If option 3 is ever chosen** it must land before players accumulate saved positions, or
  those absolute coordinates need migrating (keep Hub at offset 0 to spare existing rows).

- **6.9 — One camera, not one per zone. Editor step.** Discovered 2026-07-23 right after 6z ran:
  every zone scene carries its own Camera and Audio Listener (HUB: 1 camera + 4 lights;
  Darkwood: 1 camera + 7 lights). That was correct when exactly one zone was ever loaded. With
  additive zones it breaks in two places: during travel the client briefly holds BOTH the old and
  new zone, so two cameras and two audio listeners are live at once (expect Unity's "there are 2
  audio listeners in the scene" warning and possibly the wrong camera rendering); and the server
  ends up with one camera per resident zone for no reason. Fix: strip the camera and audio
  listener from each zone scene and move to a single rig — on the player prefab or in the
  container — leaving zone scenes to carry lighting and geometry only. *Accept:* travel between
  two zones with the console open and see no duplicate-listener warning; the view never cuts to
  the wrong camera. *Deps:* 6.3.
  **✅ 2026-07-23 — but NOT the way this task originally specced it.** Stripping the zone cameras
  for one shared container camera would discard each zone's camera settings (culling mask, clear
  flags, projection, post-processing setup), leaving one camera configured correctly for no zone
  at all. New `Networking/ZoneCameraDirector.cs` instead **enables the camera belonging to the
  player's current zone and disables the rest** — per-zone look preserved, ambiguity removed, and
  no destructive edits to six scenes. `Camera.allCameras` returns only enabled cameras, so
  `PlayerMovement.ResolveCamera` then sees exactly one.
  Severity was understated when this was filed: it is not just a duplicate-AudioListener nuisance.
  `PlayerMovement` is camera-relative, so `Camera.main` returning another zone's camera rotates
  WASD by however that camera faces — the "movement is a mix of WASD" bug found in testing.
  Also rebinds on zone change, since `PlayerMovement` caches its camera in `Start` and would
  otherwise keep using the previous zone's camera after travelling.
  Client-only. **Not yet play-tested.**

- **6.8 — Open-world mob spawner.** `Combat/Scripts/WaveSpawner.cs` is an arena construct (waves,
  difficulty ramp, session tracking) and is the wrong shape for a persistent zone. New component:
  spawn points with per-mob respawn timers and a zone population cap, populated when the zone
  loads and torn down when it unloads. Do not modify WaveSpawner — arenas keep using it.
  *Accept:* enter Darkwood, kill a mob, it respawns on its timer; leave and return and the zone
  repopulates from scratch. *Deps:* 6.3. Needs combat-design input on density and respawn timing.

**Ordering.** 6.1 and 6.2 are independently safe and land first (6.1 is a no-op today). 6.3 is the
core and everything else waits on it. 6.7 should land before real players accumulate saved
positions.

> **⚠ Two zones in `SceneNames.Zones` have no scene file:** `Arena_Copper` (task 1.1, never
> built) and `Gathering Zone`. `NormalizeZone` accepts both as valid, and `PortalTransition`
> defaults `arenaSceneName` to `SceneNames.ArenaCopper` — so a portal left on defaults sends
> players somewhere that does not exist. ZoneManager now pre-checks
> `Application.CanStreamedLevelBeLoaded` and falls back to Hub rather than stranding the player
> with no scene at all (which at login means they never spawn). **Remove them from `Zones[]` or
> build the scenes** — the fallback is a safety net, not a fix. The VPS allowlist in
> `_CONTEXT/HANDOFF_zone_persistence.md` must not include them either.

**Bandwidth note.** Two known bugs go from "multiplayer is janky" to "the world does not function"
at this scale: hero and base-enemy prefabs are missing `NetworkTransform` (fix: BCE ▶ Setup ▶ 4n,
`Editor/NetworkSyncFixer.cs`), and ability deployables still lack prefabs entirely (task 2.7).
Neither is a Phase 6 task, but Phase 6 is not shippable with either outstanding.

---

## Phase 7 — Party System (added 2026-07-23)

*Entry:* Phase 6 through 6.6. *Exit:* two players can group up and enter the same copy of an
instanced zone, and see each other's health in the HUD.

**Why this exists.** Phase 6 delivered shared-world co-op: two people in Darkwood see each other
and fight the same mobs. Instanced content did **not** come with it. `ZoneManager` keys instances
on `instanceKey`, which defaults to the connection id, so two friends both entering VoidDungeon
get two separate private copies and never meet. Dungeons and arenas are solo until this phase
lands. That is the gap between "shared world" and "play together" for the content that matters.

**Deliberate design constraint.** `instanceKey` is already threaded through
`ZoneManager.MovePlayerToZone(conn, zone, spawnId, instanceKey)` and ignored by shared zones.
Wiring parties is a one-line change at each of the six travel call sites — passing the party id
instead of null. Do not redesign ZoneManager for this.

**Recommended scope: parties are ephemeral.** A party lives in server memory and dies when the
last member disconnects. No DB tables, no VPS work, no cross-relog persistence. That is how most
ARPGs behave, it removes the entire persistence surface from v1, and it can be upgraded later
without changing the client contract. Only revisit if guilds or persistent groups get designed —
see the note in `_CONTEXT/HANDOFF_zone_persistence.md`.

- **7.1 — Server-side party state.** New `Assets/Game/Networking/PartyManager.cs`, server-only,
  same shape as ZoneManager (plain MonoBehaviour on the NetworkManager GameObject, `Instance`
  singleton, no `[Server]` attributes — Mirror's weaver rejects those outside a NetworkBehaviour).
  Owns: party id generation, `Dictionary<int partyId, List<int connId>>`, leader, a
  `maxPartySize` cap (recommend 4, matching `DynamicDifficultyScaler`'s co-op assumptions), and
  invite/accept/decline/leave/kick with a pending-invite table that expires. Disconnect must
  remove the member and promote a new leader — reuse the `OnServerDisconnect` hook that already
  calls `ZoneManager.OnPlayerDisconnected`. *Accept:* two connections can form a party and the
  server log shows correct membership through invite, leave, kick, and disconnect.
  *Deps:* 6.6. **READY**

- **7.2 — Party network messages + client mirror.** Mirror messages for the invite handshake
  (server-authoritative — the client asks, never asserts) and a `SyncList`/message push of the
  member roster to each member. Follow the existing `RodChatManager` pattern rather than inventing
  a new one. Client-side singleton behind `#if UNITY_EDITOR || !UNITY_SERVER`, notified from
  `OnStartClient` and NOT from server spawn paths — the host-mode-only bug in CLAUDE.md.
  *Accept:* both clients see identical roster state after every operation. *Deps:* 7.1.

- **7.3 — Party UI.** Invite prompt, party frames (name / class / HP), leave and kick controls.
  Client-only, guarded. `/invite <name>` and `/leave` chat commands via `GmCommandRouter`'s
  existing parser are the cheap first cut and worth doing before the panel.
  *Accept:* invite by name from chat, accept from a prompt, see teammate HP update live.
  *Deps:* 7.2.

- **7.4 — Wire parties into instancing.** Pass the party id as `instanceKey` at the six travel
  call sites (`WaypointMapTrigger`, `HubReturnTrigger`, `GmCommandRouter`, `HangmanNPC`,
  `ArenaPortalTrigger`, `PortalTransition`). Decide and record the entry rule: does the whole
  party get pulled in when the leader enters, or does each member walk in themselves and land in
  the leader's existing instance? Recommend the latter — it needs no extra machinery, since
  `AcquireZone` already reuses an existing instance for a matching key.
  *Accept:* two partied players entering VoidDungeon separately end up in ONE copy and can see
  each other; two unpartied players get two copies. *Deps:* 7.1, 6.3. **This is the task that
  makes dungeons co-op.**

- **7.5 — Party-aware combat plumbing.** `DynamicDifficultyScaler.GetScaling(wave, playerCount)`
  currently counts players by proximity or connection; it should scale on party size in an
  instance. Also confirm XP/loot attribution across a party — `EnemyController.PostCombatKill`
  posts to `/api/combat/kill` per killer, so a party wipes out shared-credit rules unless the
  server is told. **⚠ Needs a design decision** (shared XP? contribution-weighted? loot rolls?)
  and, unlike the rest of Phase 7, likely VPS work. *Deps:* 7.4, owner decision.

---

## Phase 8 — Performance & Optimization (added 2026-08-02)

*Entry:* stable build. *Exit:* no per-frame or per-combat-event GC spikes in the profiler during
normal play. *Why a phase, not drive-by fixes:* the combat hot paths are already well-optimized
(EnemyController runs a 5 Hz brain with cached refs; player/portal scans are throttled and cached;
no LINQ in hot paths). The remaining wins are allocation churn, and each has a clear scope boundary
that must be respected — this phase is minimum-change GC reduction, **not** a rewrite.

> **Done 2026-08-02 (context, not a task):** the aim-indicator fill-mesh projection in
> `UI/AbilityCaster.cs` (`UpdateProjectedCircleFill`/`RectFill`/`ConeFill`) reallocated
> `mesh.vertices` + `mesh.uv` every frame while aiming (~15 KB/frame; the circle disk is 769 verts).
> Now reuses shared `List<Vector3>`/`List<Vector2>` buffers via `Mesh.GetVertices`/`GetUVs`/
> `SetVertices`. Behaviour-identical; review-only, confirm in editor. See
> [[Known Issues & Tech Debt]] §Performance.

- **8.1 — Pool the frequently-spawned LOCAL VFX.** New `Assets/Game/Systems/VfxPool.cs` — a simple
  prefab-keyed `Dictionary<GameObject, Queue<GameObject>>` pool with `Get(prefab, pos, rot)` and a
  timed `Release(go, seconds)` that re-parents to the pool and resets `ParticleSystem`s (Clear +
  Play on reuse) instead of `Destroy`. Route the **client-side, non-networked, auto-despawning**
  VFX through it: impact/hit VFX in `UI/AbilityCaster.cs` (the `RpcPlayHitVFX`/cast-VFX
  `Instantiate` sites ~2966/2986/3046/4261), `Combat/Scripts/ChainLightningVFXProfile.cs:115`,
  `Combat/Scripts/ElementalLightningVFXProfile.cs:81`, `Combat/Scripts/ClericHealVFX.cs`
  (72/84/108), `Combat/Scripts/EnemyDeathVFX.cs:46`, and the one-shot burst VFX in the ability
  behaviours (`NaniteSwarmBehaviour.cs:80`, `ShockMineBehaviour.cs:71`, `SingularityBehaviour.cs:101`,
  `RestorationBeacon.cs:75`). These are the high-frequency, short-lived effects whose create/destroy
  churn is the steady GC source during a busy wave.
  **⚠ Hard scope boundary — do NOT pool these:** anything spawned via `NetworkServer.Spawn`
  (enemies at `WaveSpawner.cs:193`/`WaveChest.cs:118`, projectiles/drops in `EnemyController`,
  boss shards/drops in `WorldBossController`, turret/drops in `IronWardenController`) — Mirror owns
  that lifecycle and pooling it is a separate, much larger task; and the persistent idle VFX
  parented to a deployable (`ShockMine`/`Singularity`/`NullFieldZone`/relay `idleVFX`), whose
  lifetime is already tied to the deployable and which are spawned once, not churned.
  *Accept:* profiler shows flat GC Alloc during sustained combat where before it spiked on every
  hit/death; VFX look and time out exactly as before; nothing networked changed. *Deps:* none.
  **READY** (client-only; guard the pool `#if UNITY_EDITOR || !UNITY_SERVER`).

- **8.2 — Allocating physics overlaps (deferred, deliberate — kept here so it isn't lost).**
  ~17 `ZonePhysics.OverlapSphere` / `Physics.OverlapSphere` calls allocate per call and none use
  `NonAlloc`. All observed sites are on-cast / on-hit (e.g. `EnemyHeavyAttack.ExecuteAbility`), not
  per-frame, so the benefit is negligible, and `NonAlloc` with a fixed buffer risks silently
  truncating hits (a real gameplay bug). Changing the shared `ZonePhysics` wrapper is also
  foundational. **Do not convert** unless a query moves into a per-frame path — and then only with
  a generously sized buffer and an overflow check. *Deps:* none.

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
