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

- **1.4 — Wire CombatSessionTracker (session-log leftovers).** Files:
  `Assets/Game/Combat/Scripts/WaveSpawner.cs` (add
  `CombatSessionTracker.Local?.NotifyEnemySpawned(enemy)` in SpawnEnemy),
  `Assets/Game/Networking/RodNetworkManager.cs` (NotifyAllySpawned — note
  `PlayerIdentity.OnStartLocalPlayer` already notifies client-side; verify no double count),
  portal path calls `BeginSession()`. *Accept:* session-end POST fires with non-zero counts.
  *Deps:* 1.3; see Q3 — if `/api/combat/session/end` doesn't exist server-side, guard the POST
  behind a config flag instead. **READY**

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
  (`Assets/Game/NPC/HangmanNPC.cs`). *Accept:* E-interaction opens dialogue; arena entry path
  works. *Deps:* 1.3. **READY**

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

- **3.3 — Crafting loop client.** Files: `UI/CraftingUI.cs`, `Networking/ForgeNPC.cs`,
  `Combat/Scripts/ResourceNode.cs`; plus `CraftingManager.cs` pulled from the VPS if 0.5 lands
  that way. Forge NPC in Hub → `GET /api/recipes?profession=mining` + `GET /api/professions/:id`
  → `POST /api/craft` → refresh inventory; show server `error` strings verbatim (project
  convention). *Accept:* craft copper_bar from 3 shards in-game; failure shows server message.
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
