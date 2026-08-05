# Crossworlds BCE (Unity) — Codex Context

Unity 6 client + dedicated server for Crossworlds BCE, a co-op action RPG.
Mirror/KCP on UDP 7777; persistence via the Node/Express auth API on the VPS.
This file is the single source of context — `_CONTEXT/AGENTS.md` (server/API detail)
and `CrossWorlds/AGENTS.md` (legacy) defer to it.

**A parallel Three.js browser client lives at `D:\Crossworlds Web\Cross Worlds Web`
(own AGENTS.md). Don't confuse the two: this repo is the Unity game.**

## Read before working

| Doc | When |
|---|---|
| `SNAPSHOT.md` | architecture map — full audit 2026-07-03 + delta 2026-07-25; older sections may lag |
| `ROADMAP.md` | the local task list (the HOW); check the Status note for what's done |
| http://15.204.243.36/roadmap.html | TEAM roadmap (the WHAT) — Phase 1 vertical slice; owner-facing priority source |
| `_CONTEXT/AGENTS.md` | DB schema, API endpoints, server conventions, anti-exploit design |
| `_CONTEXT/VPS_SERVER.md` | service commands, deploy, logs |
| `CrossWorlds/_context/*.md` | design docs (combat, mastery, healing, retention) |

## Ground rules

- **Old gear endpoints/tables are sacred** (`/character`, `/character/gear/equip`,
  `item_template`, `character_gear`, …) — Unity calls them on every spawn. Never modify.
- **Server work happens on the VPS, not here** (`/opt/crossworlds-auth/server.js`,
  SSH per `_CONTEXT/VPS_SERVER.md`). This repo is client + Unity dedicated server only.
- **Class/hero indices** (server CLASS_NAMES, mirrored in `PlayerIdentity.ClassNames`,
  `RodNetworkManager.classPrefabs`, CharacterSelect): 0=**Marauder** (legacy names:
  Warden, Engineer), 1=Ironclad(=Guardian legacy), 2=Shadowblade, 3=Cleric, 4=Arcanist.
  Verified in code 2026-07-25. Legacy docs use the old names; the index positions are
  what matters — never renumber. ("Iron Warden" is a world boss, unrelated to the class.)
- **Mirror discipline**: `[Server]` on every game-state mutation; client-only code
  (VFX, UI, HUD attach) behind `#if UNITY_EDITOR || !UNITY_SERVER` — **never
  `#if !UNITY_SERVER` alone**. The editor's active build target is Dedicated Server,
  so `UNITY_SERVER` is defined there too; `!UNITY_SERVER` silently strips client code
  in the editor. `UNITY_EDITOR || !UNITY_SERVER` keeps it compiled for editor play-mode
  while still excluding it from actual server builds. Client-side singletons
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
  Editor/EnemyForge/   data-driven enemy authoring suite (definitions, builder,
                       validator, deployment, animation library) — enemies like
                       Templar/Wizard/Chaos Weaver are Forge content, NOT player classes
  Scenes/              build list (2026-07-25): LoginScene(0), CharacterSelect(1),
                       HUB(2), Darkwood(3), Ashen Wastelands(4), Toujam Basin(5),
                       GM Island(6), VoidDungeon(7), _Container(8) — combat zones EXIST;
                       "no arena scene" docs are stale
  Prefabs/EnemyForge/  Forge-generated enemy prefabs (old 5-hero/Enemy_Grunt layout gone)
CrossWorlds/           legacy staging tree + design docs — read-only reference
                       (Brandalf: Assets/Game/Heroes/ dir is GONE; only editor tooling
                       remains — BrandalfSetupBuilder etc. Decision still pending, don't wire)
_CONTEXT/              server/API docs
tools/                 build-server.ps1 (headless Linux server build + package),
                       deploy-server.sh (RETIRED /game/Builds path — CI deploys now)
```

## Build & deploy (dedicated server)

Unity version comes from `ProjectSettings/ProjectVersion.txt` (6000.4.11f1 as of
2026-07-25 — older docs saying 6000.4.10f1 / 6000.0.77f1 are stale).

**Live pipeline is CI (GitHub Actions), not manual scp.** On push to `main`,
`Brbmuffins/Crossworlds-BCE` builds via `BuildScript.BuildDedicatedServer`
(`Assets/Game/Editor/BuildScript.cs` — its scene list is now derived from
Scenes-In-Build, so every build-profile scene, incl. `_Container`, is included)
and deploys to a **numbered run dir `/game/<runid>/`** on the VPS, then repoints
the `crossworlds-server.service` unit's `ExecStart` at that dir and restarts it.
So the active binary path changes every deploy — read the unit to find it:
`grep ExecStart /etc/systemd/system/crossworlds-server.service`.

- **Active game-server unit is `crossworlds-server`** (NOT `rod-server` or
  `crossworlds` — both retired 2026-07-25). Auth is `crossworlds-auth` (:3000),
  dashboard `crossworlds-dashboard` (:4000), `rod-realtime` co-op (:5000),
  `spacetimedb` (local :3500). See `verified-live-vps-topology` memory / wiki.
- Restart / inspect: `sudo systemctl restart crossworlds-server`,
  `sudo journalctl -u crossworlds-server -n 50 --no-pager`,
  `sudo ss -ulnp | grep 7777` (want exactly one binder).

Manual local build (compile check / out-of-band deploy):
`powershell -ExecutionPolicy Bypass -File tools\build-server.ps1` (refuses to build
if LFS pointer files remain; needs `git lfs pull` first — LFS/push auth is NOT
available to CLI agent sessions, only GitHub Desktop's token). The old
`deploy-server.sh` → `/game/Builds` → `crossworlds.service` path is **retired**;
don't deploy into `/game/Builds` (deleted).

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

Git: commit in topical slices with descriptive messages.
Check `git status -sb` first — origin may be ahead (it was on 2026-07-03).
