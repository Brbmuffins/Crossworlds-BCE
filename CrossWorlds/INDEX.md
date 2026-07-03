# Crossworlds BCE — Context Index

Route AI agents here first. Load ONE file per troubleshooting session.
Each file is self-contained: key files, known pitfalls, current state, active TODOs.

---

## Quick Facts

| Key | Value |
|---|---|
| Engine | Unity 6000.0.77f1, URP, IL2CPP |
| Networking | Mirror + KCP transport, UDP 7777 |
| Server IP | 15.204.243.36 |
| Website | https://playcrossworlds.com (A record live, SSL active) |
| Download page | https://playcrossworlds.com (download + how-to-play) |
| Client download | https://playcrossworlds.com/downloads/CrossworldsBCE.zip |
| Auth server | Port 3000 — `/opt/rod-auth/server.js` |
| Dashboard | Port 4000 — `/opt/rod-dashboard/server.js` |
| Database | MySQL 8 — DB `rod_online`, user `rodgame` |
| Scene order | LoginScene(0) → CharacterSelect(1) → Hub(2) |
| Heroes (Classes) | Warden(0), Ironclad(1), Shadowblade(2), Cleric(3), Arcanist(4) |
| Phase 1 status | ✅ COMPLETE — server + all Unity client scripts done |
| Phase 2 status | 🔶 IN PROGRESS — VPS session (talents/guilds/quests) + Unity scripts staged in `_scripts/` |

---

## ⚠️ Legacy Naming — DO NOT RENAME

VPS paths and DB names still use `rod` from the original project name **"Rate of Decay Online"**.
The game is now **Crossworlds BCE** but renaming these would break live services.

**Leave these names exactly as they are:**
- DB: `rod_online`, user `rodgame`
- Dirs: `/opt/rod-auth/`, `/opt/rod-dashboard/`, `/var/www/rod/`
- Nginx config: `/etc/nginx/sites-available/rod`
- systemd services are already renamed: `crossworlds-auth`, `crossworlds-dashboard`, `crossworlds`

---

## Context Files — Load When Troubleshooting

| File | Use when working on... |
|---|---|
| [`_context/NETWORKING.md`](_context/NETWORKING.md) | Mirror spawn failures, assetId errors, prefab registration, `Could not spawn`, player not appearing |
| [`_context/AUTH_LOGIN.md`](_context/AUTH_LOGIN.md) | Login flow, JWT, CharacterSelect, character data, 401/403 errors, auth server calls |
| [`_context/UI_INPUT.md`](_context/UI_INPUT.md) | Chat typing moves player, ESC menu, camera orbit, cursor lock, EventSystem, WASD during chat |
| [`_context/SCENE_SETUP.md`](_context/SCENE_SETUP.md) | Hub scene rebuild, class prefabs, NetworkManager settings, build settings, portal |
| [`_context/VPS_SERVER.md`](_context/VPS_SERVER.md) | Deploying builds, systemd services, reading logs, Nginx, SSL, DB access, dashboard |
| [`_context/COMBAT.md`](_context/COMBAT.md) | Class abilities, enemy design, drop tables, NavMesh, arena, damage systems |

---

## Source of Truth Docs

| Doc | Purpose |
|---|---|
| [`CROSSWORLDS.md`](CROSSWORLDS.md) | Master reference — full schema, all API endpoints, services, conventions, integration map |
| [`CLAUDE.md`](CLAUDE.md) | Claude Code VPS agent — behavior rules, process, code conventions |
| [`CLAUDE_CONTEXT.md`](CLAUDE_CONTEXT.md) | Paste-in primer for Claude Chat sessions |
| [`_design/ROADMAP.md`](_design/ROADMAP.md) | Phase 1 status week-by-week, open bugs, Phase 2 stubs |
| [`_context/index.md`](_context/index.md) | Feature status by phase, hero mastery schema, open bugs table |
| [`_context/COMBAT.md`](_context/COMBAT.md) | Class abilities, enemy design, damage, arena |
| [`_context/PHASE3_DESIGN.md`](_context/PHASE3_DESIGN.md) | World Bosses, Dungeons, PvP, Status Effect Engine v2 |
| [`_context/RETENTION_DESIGN.md`](_context/RETENTION_DESIGN.md) | Daily login, daily quests, weekly challenges, season system |
| [`_context/HEALING_DESIGN.md`](_context/HEALING_DESIGN.md) | Cleric heal feel, SoulBondTether, ShieldValueHUD, ClericRadarUI |
| [`_context/HERO_MASTERY_DESIGN.md`](_context/HERO_MASTERY_DESIGN.md) | Smite-style per-hero mastery — XP table, rewards, schema, endpoints |
| [`_context/COSMETICS_DESIGN.md`](_context/COSMETICS_DESIGN.md) | Essence drops, skin crafting, color palettes, SQL inserts |

---

## Active Bugs (don't lose track)

| # | Bug | File / Area | Status |
|---|---|---|---|
| 1 | `orientation:F3` — float sent as formatted string | Unity `PATCH /character/position` | Open |
| 2 | `GmConsole.cs` — missing `#if !UNITY_SERVER` guard | GmConsole.cs | Fixed in code — confirm on next deploy |
| 3 | `CmdSendChat` — missing `[CHAT]` server log line | RodChatManager.cs | Open |
| 4 | Enemy prefabs — capsule placeholders, real meshes needed | Prefabs/ | Open |
| 5 | `NetworkManager.spawnPrefabs` — must manually add Enemy_Grunt, Enemy_Ranged, Enemy_Elite, WorldItem | Inspector | Open |
| 6 | Class abilities not fully wired per hero in AbilityCaster | AbilityCaster.cs | Open |

---

## VPS / Ops TODOs

| Task | Notes |
|---|---|
| Uptime Kuma web UI setup | Running on port 3001 — UI not configured yet |
| `StartLimitIntervalSec=0` on `crossworlds.service` | Would allow infinite restarts instead of stopping after 5 rapid crashes |
| Discord link on download page | Shows "Coming Soon" on `/var/www/rod/index.html` |
