# VPS & Server — Operations Reference

**Last audited:** 2026-07-03 (Claude Code audit — see `VPS_AUDIT_PROMPT.md` for the prompt used)

---

## Key Info

| Item | Value |
|------|-------|
| Server IP | `15.204.243.36` |
| Hostname | `playcrossworlds.com` |
| SSH | `ssh ubuntu@playcrossworlds.com` |
| Game binary | `/game/Builds/CrossworldsBCE.x86_64` |
| Game data dir | `/game/Builds/CrossworldsBCE_Data/` |
| Game log | `/var/log/crossworlds.log` |
| Auth server | `/opt/crossworlds-auth/server.js` |
| Dashboard | `/opt/crossworlds-dashboard/server.js` |
| Realtime server | `/opt/rod-realtime/server.js` |
| Web root | `/var/www/crossworlds/` |
| Client download | `/var/www/crossworlds/downloads/CrossworldsBCE.zip` |

---

## Services

| Service name | Port | State (2026-07-03) | What |
|---|---|---|---|
| `crossworlds` | 7777/UDP | active/running | Unity game server (Mirror/KCP) |
| `crossworlds-auth` | 3000/TCP | active/running | Node.js auth + character + game API |
| `crossworlds-dashboard` | 4000/TCP | active/running | GM/admin web dashboard + Socket.io |
| `rod-realtime` | 5000/TCP (local) | active/running | Realtime relay — `/opt/rod-realtime/server.js` |
| nginx | 80/443 | active | Public download page, SSL via Certbot |
| Uptime Kuma | 3001 | active | Monitoring (web UI needs config) |

> **Legacy unit files on disk** (point to non-existent paths — harmless, can be removed):
> `rod-server.service` → `/game/Builds/Portalis.x86_64` (path gone)
> `rod-auth.service` → `/opt/rod-auth` (path gone)
> `rod-dashboard.service` → `/opt/rod-dashboard` (path gone)

---

## Essential Commands

```bash
# Status
sudo systemctl status crossworlds crossworlds-auth crossworlds-dashboard rod-realtime

# Restart
sudo systemctl restart crossworlds-auth
sudo systemctl restart crossworlds-dashboard
sudo systemctl restart crossworlds
# rod-realtime: restart only if needed — not required for most deploys

# Logs
sudo journalctl -u crossworlds-auth -n 50 --no-pager
sudo journalctl -u crossworlds -n 50 --no-pager
sudo journalctl -u rod-realtime -n 50 --no-pager
tail -f /var/log/crossworlds.log

# Ports
ss -ulnp | grep 7777    # game UDP
ss -tlnp                # all TCP (3000, 4000, 5000, 80, 443)

# Binary
ls -la /game/Builds/CrossworldsBCE.x86_64

# Health check
curl -s http://localhost:3000/api/health

# Database
mysql -u crossworlds -p crossworlds   # password in /opt/crossworlds-auth/.env → DB_PASS
```

---

## Deploying a New Build

```bash
# 1. Build locally (PowerShell)
powershell -ExecutionPolicy Bypass -File tools\build-server.ps1
# Output: build\crossworlds-server.tar.gz

# 2. Upload
scp build\crossworlds-server.tar.gz tools\deploy-server.sh ubuntu@playcrossworlds.com:~

# 3. On VPS
sudo bash deploy-server.sh             # auto-backup, restart, verify, auto-rollback on failure
sudo bash deploy-server.sh --rollback  # manual rollback
```

**Binary name is critical.** The systemd service `ExecStart` must match exactly:

```bash
# Verify
grep ExecStart /etc/systemd/system/crossworlds.service
# Expected: /game/Builds/CrossworldsBCE.x86_64 -batchmode -nographics ...

# If you rename the binary:
systemctl daemon-reload && systemctl restart crossworlds
```

### Current crossworlds.service (as of 2026-07-03 audit)

```ini
[Unit]
Description=Crossworlds (BCE) - Game Server (Mirror/KCP :7777)
After=network.target crossworlds-auth.service
StartLimitIntervalSec=0              # must be in [Unit], not [Service]

[Service]
Type=simple
User=ubuntu
WorkingDirectory=/game/Builds
ExecStart=/game/Builds/CrossworldsBCE.x86_64 -batchmode -nographics -logFile /var/log/crossworlds.log
Restart=on-failure
RestartSec=10
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
```

> `StartLimitIntervalSec=0` was previously in `[Service]` (silently ignored by systemd).
> Fixed 2026-07-03 — moved to `[Unit]`. Without it, the game server could hit systemd's
> burst restart limit after a crash and stop recovering.

---

## Dashboard URLs

| URL | Access |
|-----|--------|
| `http://15.204.243.36` | Public download page |
| `http://15.204.243.36:4000` | Manager dashboard (HTTP Basic Auth) |
| `http://15.204.243.36:4000/gm-dashboard?token=<TOKEN>` | GM dashboard (token in VPS .env) |

GM Dashboard: server status, spawn events, last 50 log lines, restart button, log download.

---

## Known Pitfalls

| Symptom | Cause | Fix |
|---------|-------|-----|
| Server starts then crashes | `UnityPlayer.so` version mismatch | Upload matching `UnityPlayer.so` from same build session |
| `Could not spawn` in game | Old binary on VPS after prefab rebuild | Upload fresh server build |
| Server not listening on 7777 | Wrong binary name in systemd or crash | Check `ExecStart`; check `/var/log/crossworlds.log` |
| Players can't see each other | Client/server have different prefab assetIds | Rebuild BOTH client and server after any prefab change |
| Auth server returns 500 | DB connection issue or bad .env | `journalctl -u crossworlds-auth`; verify MySQL running |
| Game server won't restart after crash | `StartLimitIntervalSec` in wrong section | Must be in `[Unit]` not `[Service]`; `daemon-reload` after fix |
| Port scanner noise in game log | External probes hitting UDP 7777 | Normal — `invalid channel header: 92` is expected background noise |

---

## Auth Server Notes

- **DO NOT restart `crossworlds-auth` carelessly** — it handles all active JWTs and DB connections
- Auth server auto-starts on VPS reboot via systemd
- Logs: `journalctl -u crossworlds-auth -f`
- Config: `/opt/crossworlds-auth/.env` (JWT secret, DB credentials — never log or expose)

---

## VPS Health Check (quick paste)

```bash
sudo systemctl status crossworlds crossworlds-auth crossworlds-dashboard --no-pager
ss -ulnp | grep 7777
curl -s http://localhost:3000/api/health
curl -s http://localhost:3000/api/maintenance/status
tail -20 /var/log/crossworlds.log
```

---

## Credentials

| Secret | Location |
|--------|----------|
| MySQL password | `DB_PASS` in `/opt/crossworlds-auth/.env` |
| JWT secret | `JWT_SECRET` in `/opt/crossworlds-auth/.env` |
| Admin API token | `ADMIN_TOKEN` in `/opt/crossworlds-auth/.env` and `/opt/crossworlds-dashboard/.env` |
| Dashboard HTTP Basic Auth | nginx config / dashboard `.env` |

**Never commit credentials.** Previous versions of this file leaked both passwords into
git history — rotate them on the VPS if not already done (ROADMAP Q7).

---

## Active TODOs (post-audit 2026-07-03)

| Priority | Item |
|----------|------|
| 🔴 | Rotate credentials (leaked into git history — ROADMAP Q7) |
| 🟡 | **CLASS_NAMES decision** — live server still has `['Engineer','Guardian',...]` at indices 0–1; live characters in DB have those names. Changing to `['Warden','Ironclad',...]` requires coordinated Unity deploy + DB migration. See note below. |
| 🟡 | Document `rod-realtime` (port 5000) — what does it do, who calls it, does it need monitoring? |
| 🟡 | Remove stale legacy unit files: `rod-server.service`, `rod-auth.service`, `rod-dashboard.service` |
| 🟢 | HTTPS / Cloudflare SSL — all traffic plain HTTP; JWT in transit unencrypted |
| 🟢 | Configure Uptime Kuma web UI at `http://15.204.243.36:3001` |
| 🟢 | CI/CD secrets (`.github/workflows/build-and-deploy.yml` exists, needs secrets) |

### CLASS_NAMES Migration Plan (when ready)

```bash
# 1. Deploy Unity client with updated PlayerIdentity.ClassNames = ["Warden","Ironclad",...]
# 2. Deploy Unity dedicated server with same array
# 3. Run on VPS:
#    UPDATE characters SET class_name = 'Warden'   WHERE class_index = 0;
#    UPDATE characters SET class_name = 'Ironclad' WHERE class_index = 1;
# 4. Update server.js CLASS_NAMES array:
#    const CLASS_NAMES = ['Warden', 'Ironclad', 'Shadowblade', 'Cleric', 'Arcanist'];
# 5. sudo systemctl restart crossworlds-auth
# Validator is already correct: class_index > 4
```
