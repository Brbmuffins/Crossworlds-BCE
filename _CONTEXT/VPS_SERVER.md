# VPS & Server — Troubleshooting Context

**When to load:** Deploying builds, server not starting, service crashes, log inspection, auth server issues, dashboard, upload process, binary name problems.

> Canonical topology lives in the root `CLAUDE.md` and the wiki `Build & Deploy` /
> `Architecture & Topology` pages. Where this file disagrees, those win.

---

## Key Info

| Item | Value |
|------|-------|
| Server IP | `15.204.243.36` (playcrossworlds.com) |
| SSH | `ubuntu@15.204.243.36` (sudo), port 22 |
| Game binary | `/game/<runid>/CrossWords.x86_64` — **numbered CI run dir, changes every deploy** |
| Game data dir | `/game/<runid>/CrossWords_Data/` |
| Find the live run dir | `grep ExecStart /etc/systemd/system/crossworlds-server.service` |
| Game log | `/var/log/crossworlds.log` |
| Auth server path | `/opt/crossworlds-auth/` |
| Dashboard path | `/opt/crossworlds-dashboard/` |
| Client download | `/var/www/crossworlds/downloads/WindowsClient.zip` (alias `CrossworldsBCE.zip`) |
| Credentials | `SERVER_REFERENCE.md` (PRIVATE — do not share) |

> [!warning] `/game/Builds` is retired
> The old fixed `/game/Builds` path and the `crossworlds` / `rod-server` units were
> retired 2026-07-25. The active game unit is **`crossworlds-server`** and it runs from
> a numbered run dir. Always resolve the real path from the unit's `ExecStart`.

---

## Services

| Service name | Port | What it is |
|-------------|------|------------|
| `crossworlds-server` | 7777 UDP | Unity game server (Mirror/KCP) — **active unit** |
| `crossworlds-auth` | 3000 | Node.js auth + character API |
| `crossworlds-dashboard` | 4000 | GM/admin web dashboard |
| `rod-realtime` | 5000 (local) | Socket.io co-op relay — **still live under its legacy name, do NOT rename** |
| `spacetimedb` | 3500 (local) | SpacetimeDB instance |
| nginx | 80/443 | Public site / downloads (SSL via Certbot) |
| Uptime Kuma | 3001 | Monitoring |

Retired (may still have leftover unit files on disk): `rod-server`, `rod-auth`,
`rod-dashboard`, `crossworlds`.

---

## Essential Commands

```bash
# Check all services
systemctl status crossworlds-server crossworlds-auth crossworlds-dashboard

# Live game server log
tail -f /var/log/crossworlds.log

# Restart game server
systemctl restart crossworlds-server

# Check UDP port 7777 is open (want exactly one binder)
ss -ulnp | grep 7777

# Find + check the live binary (numbered run dir)
GAME_BIN=$(systemctl show -p ExecStart --value crossworlds-server | grep -oE '/[^ ]*CrossWords\.x86_64' | head -1)
ls -la "$GAME_BIN"

# Check what's listening
ss -tlnp
```

---

## Deploying a New Build

**The live path is CI (GitHub Actions), not manual scp.** On push to `main` (and daily
at `0 10 * * *` UTC), the workflow builds the Linux dedicated server, ships the tarball
+ `tools/deploy-server.sh` to the VPS, and runs the deploy. `deploy-server.sh`
self-detects the `crossworlds-server` unit and its run dir, backs up, extracts in place,
restarts, and auto-rolls-back on failure.

### Manual / out-of-band server deploy
```bash
# Local: build (emits CrossWords.x86_64 / CrossWords_Data) then upload
powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
scp build/crossworlds-server.tar.gz tools/deploy-server.sh ubuntu@15.204.243.36:~
# On the VPS:
sudo bash deploy-server.sh            # rollback: sudo bash deploy-server.sh --rollback
```

### Client Build (Windows)
1. Unity → `BuildScript.BuildWindowsClient` (or **File → Build Settings** → Windows x86_64)
2. Zip the output and replace `/var/www/crossworlds/downloads/WindowsClient.zip`

### FileZilla / SFTP Settings
- Host: `15.204.243.36`, Port: 22, Protocol: SFTP, User: `ubuntu`

---

## Binary Name — Critical

The systemd unit's `ExecStart` and the deployed binary must agree. The live name is
**`CrossWords.x86_64`** (spelling: `CrossWords`, capital W, no "BCE") with a matching
**`CrossWords_Data/`** folder. `BuildScript.locationPathName` emits exactly this, so the
build → pack → deploy chain stays consistent; don't rename it downstream.

Past incident: the binary was once renamed but the service file still pointed to the old
name → server silently failed to start. If you ever change the name, update the unit and:
```bash
grep ExecStart /etc/systemd/system/crossworlds-server.service   # confirm the path/name
sudo systemctl daemon-reload
sudo systemctl restart crossworlds-server
```

---

## VPS Health Check (Claude Code Prompt)

Give this to Claude Code running on the VPS for a full health check:

```
Check the Crossworlds BCE game server health:
1. systemctl status crossworlds-server crossworlds-auth crossworlds-dashboard
2. ss -ulnp | grep 7777 (UDP port open? exactly one binder?)
3. tail -20 /var/log/crossworlds.log
4. grep ExecStart /etc/systemd/system/crossworlds-server.service, then ls -la that binary
5. curl -s http://localhost:3000/api/health
6. Report any errors or unexpected state
```

---

## Dashboard URLs

| URL | Access |
|-----|--------|
| `https://playcrossworlds.com` | Public download page |
| `http://15.204.243.36:4000` | Manager dashboard (HTTP Basic Auth) |
| `http://15.204.243.36:4000/gm-dashboard?token=<TOKEN>` | GM dashboard (token in VPS .env) |

GM Dashboard shows: server status, spawn events, last 50 log lines (color-coded), restart button, log download, Uptime Kuma link.

---

## Auth Server Notes

- **DO NOT restart `crossworlds-auth` carelessly** — it handles all active JWTs and DB connections
- Auth server auto-starts on VPS reboot via systemd
- Logs: `journalctl -u crossworlds-auth -f`
- Config: `/opt/crossworlds-auth/.env` (JWT secret, DB credentials — see `SERVER_REFERENCE.md`)

---

## Known Pitfalls

| Symptom | Cause | Fix |
|---------|-------|-----|
| Server starts then crashes | UnityPlayer.so version mismatch | Deploy a matching full build (don't hand-mix .so files) |
| `Could not spawn` errors in game | Old binary on VPS after prefab rebuild | Redeploy a fresh server build |
| Game server not listening on 7777 | Binary name/path wrong in systemd, or crash | Check `ExecStart` path, check log for crash |
| Deploy "succeeds" but nothing changes | Binary extracted to a dir the unit doesn't run | Confirm deploy dir == `ExecStart` dir |
| Players connect but see no other players | Client and server have different prefab assetIds | Rebuild BOTH client and server after any prefab changes |
| Auth server returns 500 | DB connection issue or bad .env | Check `journalctl -u crossworlds-auth`, verify MySQL is running |

---

## Active TODOs

- HTTPS is live via Certbot; keep the cert renewing
- Rotate the DB/dashboard credentials that leaked into git history (ROADMAP Q7)
- Clean up retired unit files (`rod-server`, `rod-auth`, `rod-dashboard`, `crossworlds`) on disk
