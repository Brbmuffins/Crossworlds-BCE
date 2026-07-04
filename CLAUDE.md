# Crossworlds BCE (Unity) — Claude Context

Unity 6 client + dedicated server for Crossworlds BCE, a co-op action RPG.
Mirror/KCP on UDP 7777; persistence via the Node/Express auth API on the VPS.
This file is the single source of context — `_CONTEXT/CLAUDE.md` (server/API detail)
and `CrossWorlds/CLAUDE.md` (legacy) defer to it.

**A parallel Three.js browser client lives at `D:\Crossworlds Web\Cross Worlds Web`
(own CLAUDE.md). Don't confuse the two: this repo is the Unity game.**

## Read before working

| Doc | When |
|---|---|
| `SNAPSHOT.md` | architecture map, what's implemented vs stubbed, dependency graph |
| `ROADMAP.md` | the task list — **work from here**; check the Status note for what's done |
| `_CONTEXT/CLAUDE.md` | DB schema, API endpoints, server conventions, anti-exploit design |
| `_CONTEXT/VPS_SERVER.md` | service commands, deploy, logs |
| `CrossWorlds/_context/*.md` | design docs (combat, mastery, healing, retention) |

## Ground rules

- **Old gear endpoints/tables are sacred** (`/character`, `/character/gear/equip`,
  `item_template`, `character_gear`, …) — Unity calls them on every spawn. Never modify.
- **Server work happens on the VPS, not here** (`/opt/crossworlds-auth/server.js`,
  SSH per `_CONTEXT/VPS_SERVER.md`). This repo is client + Unity dedicated server only.
- **Class/hero indices** (server CLASS_NAMES, mirrored in `PlayerIdentity.ClassNames`,
  `RodNetworkManager.classPrefabs`, CharacterSelect): 0=Warden(=Engineer legacy),
  1=Ironclad(=Guardian legacy), 2=Shadowblade, 3=Cleric, 4=Arcanist. Legacy docs use the
  old names; the index positions are what matters — never renumber.
- **Mirror discipline**: `[Server]` on every game-state mutation; client-only code
  (VFX, UI, HUD attach) behind `#if !UNITY_SERVER`. Client-side singletons
  (`CombatSessionTracker`, `InventoryManager`, …) are notified from `OnStartClient`
  hooks, NOT from server-side spawn paths (host-mode-only bug).
- **Ports frozen**: 3000 auth, 4000 dashboard, 7777/UDP game, 3001 Kuma.
- **Minimum change.** No refactors while fixing; no rewrites of working systems.
- New `/api/*` responses: `{success, data}` / `{success, error}` — error strings are
  player-readable, show verbatim.
- Never commit credentials. Passwords live in `.env` files on the VPS only
  (they leaked into git history once — see ROADMAP Q7, rotation pending).

## What a CLI session CAN and CANNOT do here

**Can:** edit C# scripts, editor-builder scripts, prefab/scene YAML (carefully),
docs; run git; grep/audit. Commit in topical slices with descriptive messages.

**Cannot:** compile or run Unity, bake NavMesh, use the BCE editor menus, assign
Inspector references, or create .unity scenes properly. Tasks needing these are
marked "editor step" in ROADMAP.md — implement the script side, then list the exact
editor clicks for the user (menu path, GameObject, field, value). After any C# batch,
tell the user to open the editor once to confirm compilation — say so explicitly in
the report; do not claim build-verified.

## Layout

```
Assets/Game/
  Abilities/Scripts/   deployables + ability behaviours (mines, walls, zones)
  Characters/Scripts/  class passives, NPC controller, ability pools
  Combat/Scripts/      THE combat core: EnemyController, WaveSpawner, Health,
                       WorldBossController, StatusEffectManager, CombatSessionTracker,
                       DropTable, WorldItem (root-level Combat/*.cs stubs are deleted)
  Networking/          RodNetworkManager, RodNetworkAuthenticator, PlayerIdentity,
                       PortalTransition, RodChatManager, ForgeNPC
  Systems/             client REST singletons (InventoryManager, ItemCatalog, HeroMastery)
  UI/                  HUDs, panels, GmConsole, LoginManager, PlayerProgressManager
  Editor/              BCE menu builders — scenes/prefabs are reproducible from these;
                       prefer extending a builder over hand-editing scene YAML
  Scenes/              LoginScene(0), CharacterSelect(1), Hub(2); NO arena scene yet
  Prefabs/             5 hero prefabs + Enemy_Grunt/Ranged/Elite
  Heroes/Brandalf/     6th-hero model — DECISION PENDING (skin vs class), don't wire
CrossWorlds/           legacy staging tree + design docs — read-only reference
_CONTEXT/              server/API docs
tools/                 build-server.ps1 (headless Linux server build + package),
                       deploy-server.sh (VPS-side deploy with backup/rollback)
```

## Build & deploy (dedicated server)

Unity version comes from `ProjectSettings/ProjectVersion.txt` (6000.4.10f1 as of
2026-07-03 — older docs saying 6000.0.77f1 are stale). Pipeline:

1. `git lfs pull` (interactive shell or GitHub Desktop — LFS/push auth is NOT
   available to CLI agent sessions; wincred has no git token, only Desktop's).
2. `powershell -ExecutionPolicy Bypass -File tools\build-server.ps1`
   — refuses to build if LFS pointer files remain; renames output to the
   `CrossworldsBCE.x86_64` / `CrossworldsBCE_Data` pair the systemd unit expects;
   produces `build\crossworlds-server.tar.gz`.
3. `scp build\crossworlds-server.tar.gz tools\deploy-server.sh ubuntu@playcrossworlds.com:~`
4. On the VPS: `sudo bash deploy-server.sh` (auto-backup, restart, verify,
   auto-rollback on failure; manual rollback: `--rollback`).

## Verification bar for code changes

1. `git diff` review against Mirror discipline above (no compile available).
2. Grep for other references to anything renamed/guarded (e.g. a class newly inside
   `#if !UNITY_SERVER` must not be referenced from server-compiled code — Editor/
   scripts are safe, they never compile with UNITY_SERVER).
3. State in the report which changes are review-only vs runtime-verified.
4. Update ROADMAP.md status when a task completes; keep SNAPSHOT.md accurate if the
   architecture facts change.

## Working the roadmap

Pick the topmost READY task in ROADMAP.md whose dependencies are met. ⚠ DECISION
tasks are blocked on the owner — ask, don't guess (open questions listed at the
bottom of ROADMAP.md). Tasks are sized for one session; if one balloons, stop and
re-scope rather than sprawling.

Git: commit in topical slices; `Co-Authored-By: Claude Fable 5 <noreply@anthropic.com>`.
Check `git status -sb` first — origin may be ahead (it was on 2026-07-03).
