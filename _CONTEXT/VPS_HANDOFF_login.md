# Client ↔ Server Port Map & Login Reference

Last verified 2026-08-02 (external probe + client-repo edits landed). This documents what
the **client expects** and what the **VPS actually presents**, so the two never drift again.

---

## Verified state (2026-08-02)

| Env | Client auth endpoint | External probe | Status |
|---|---|---|---|
| **Prod** | `http://15.204.243.36:3000/login` | `401 {"error":"invalid credentials"}` on bad probe | **UP & correct** ✅ |
| **Dev/Staging** | `http://15.204.243.36:3010/login` | `401 {"error":"invalid credentials"}` on bad probe | **UP & correct** ✅ |

Staging is correctly deployed and **isolated** (own auth unit, own game port, own DB) — it
never touches prod accounts/characters. Nothing on the VPS needed changing; the drift was
purely client-side and is now fixed.

---

## What was fixed (client repo, git-tracked)

1. **`DevAuthPort` 3002 → 3010** — the client was pointing dev traffic at the wrong port.
   `Assets/Game/Systems/ServerConfig.cs`. Stale `:3002`/`:3001` comments across
   `ServerConfig.cs`, `LoginManager.cs`, `RodNetworkManager.cs`, `CharacterSelectUI.cs`
   were corrected to `:3010` in the same pass.
2. **Login error-message parse bug** — the client read the server error as `{"message":…}`
   but the auth API sends `{"error":…}`. Every 4xx (wrong password, banned, etc.) therefore
   displayed the misleading fallback **"Could not reach server."** Fixed in
   `Assets/Game/UI/LoginManager.cs` (`ErrorResponse.error` + accessor). Login failures now
   show the server's real message.

Both are review/probe-verified, not compile-verified — open the Unity editor once to confirm
a clean compile (the `ErrorResponse` rename touches a field and its single usage).

---

## Authoritative port map

Source of truth for the client side: `Assets/Game/Systems/ServerConfig.cs`.
Env is a **PlayerPref** (`environment` = `"prod"`/`"dev"`, default `"prod"`), not a build flag —
the same client binary hits either stack via the PROD|DEV toggle on the login screen.

| Thing | Prod | Dev/Staging | Client constant |
|---|---|---|---|
| Box IP | `15.204.243.36` | same box | `ServerConfig.DefaultServerIP` |
| **Auth API (tcp)** | **3000** | **3010** | `ProdAuthPort` / `DevAuthPort` |
| **Game Mirror/KCP (udp)** | **7777** | **7778** | `ProdGamePort` / `DevGamePort` |
| Auth base URL | `http://…:3000` | `http://…:3010` | `AuthBaseUrl` (plain HTTP) |
| Login call | `POST {AuthBaseUrl}/login` → `{ token }` | same on :3010 | body `{username,password}` |

The **dedicated game server** is one binary; prod vs dev is chosen by launch args in
`RodNetworkManager.ApplyServerLaunchArgs`: `-port` (7777/7778) and
`-authurl` (`http://127.0.0.1:3000` prod / `:3010` dev). No args ⇒ prod defaults. Prod and dev
game servers are separate systemd units with different args on the same box.

### Full box port reference (for cross-checking, not action)

| Port | Service | Notes |
|---|---|---|
| 3000/tcp | `crossworlds-auth` (prod auth API) | UP ✅ |
| 3010/tcp | dev/staging auth API | UP ✅ isolated stack + DB |
| 7777/udp | `crossworlds-server` (prod game) | UDP — verify on box, not externally |
| 7778/udp | dev game unit (`-port 7778 -authurl …:3010`) | UDP — verify on box |
| 3001/tcp | Uptime Kuma | UP (302) |
| 3500/tcp | SpacetimeDB | local only |
| 4000/tcp | `crossworlds-dashboard` | UP (400, expects params) |
| 5000/tcp | `rod-realtime` (co-op) | localhost-bound; not externally reachable (expected) |

---

## Login flow (reference)

`LoginManager` → `POST {AuthBaseUrl}/login` → `{ token: JWT }` → stored in PlayerPref
`jwt_token` → `CharacterSelect` → `NetworkManager.StartClient()` connects to
`{GameServerIP}:{ServerConfig.GamePort}`, and the game server validates the JWT against its
`-authurl` auth API. Auth REST (login) and the Mirror game connection are **independent
services** — a login failure is an auth-API/credential issue, never the game server.

## Quick VPS spot-checks (only if a service ever looks wrong)
```bash
sudo ss -ltnp | grep -E ':(3000|3010) '   # both auth APIs listening
sudo ss -ulnp | grep -E '7777|7778'        # game servers; want exactly ONE binder each
grep ExecStart /etc/systemd/system/crossworlds-server.service   # active prod binary + args
```
