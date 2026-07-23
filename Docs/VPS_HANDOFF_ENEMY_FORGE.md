# VPS Handoff — Enemy Forge Build (2026-07-22)

Getting the VPS ready for the build containing the Enemy Forge networked enemy
(commits `bc78f27e` + `92ada395` by Sneakergeekz23, compile-guard fix `2df0a69e`).

**Service names:** the live systemd units are `crossworlds` (game server),
`crossworlds-auth` (3000), `crossworlds-dashboard` (4000). The `rod-*` names in
`_CONTEXT/VPS_SERVER.md` are legacy unit files pointing at dead paths — ignore them.

---

## 1. What this build ships

- **New networked enemy**: `model_aw_instigator_3D_rigged_Enemy` — scene-placed
  instances in **Ashen Wastelands** with Mirror sceneIds. Server-authoritative AI
  (chase/attack/death/respawn), NetworkTransform + NetworkAnimator sync.
- New enemy behaviours: delayed attack impact, get-hit anim RPC, corpse grounding,
  respawn with 2 s damage immunity, NavMesh-sampled respawn point.
- Compile-guard fixes that **unblock the server build** — every Build and Deploy
  since ~Jul 21 had failed, so the VPS is currently running a stale build.
  This deploy also brings in everything that piled up since then
  (damage numbers rework, crit strike, AW prefabs, etc.).
- Kill reporting: the enemy reports `enemyTemplateId = model_aw_instigator_3d_rigged_basic`
  via the existing client-side `POST /api/combat/kill` flow.

## 2. How the deploy arrives (no manual build step)

GitHub Actions (`Build and Deploy` on push to main) builds the Linux dedicated
server, then scp's `crossworlds-server.tar.gz` + `tools/deploy-server.sh` to
`/home/ubuntu/deploy/` and runs `sudo bash deploy-server.sh` — which backs up
`/game/Builds` to `/game/Builds.prev`, extracts, restarts `crossworlds`, verifies
it stays up 10 s, and **auto-rolls-back on failure**.

Check the run from any machine with `gh`:

```bash
gh run list --repo Brbmuffins/Crossworlds-BCE --workflow "Build and Deploy" --limit 3
```

The run to watch is **29959521367** (commit `2df0a69e`). If it's green, the VPS
already has the build — skip to step 4.

Manual fallback (if CI is down): `powershell -ExecutionPolicy Bypass -File tools\build-server.ps1`
locally, scp the tarball + `tools/deploy-server.sh` to the VPS, `sudo bash deploy-server.sh`.

## 3. BEFORE players hit it — seed the enemy template (kill credit)

The forged enemy's kills only pay XP/gold/loot if its template id exists in
`enemy_templates`. Without the row, `POST /api/combat/kill` 404s — kills still
work in-game, they just award nothing.

On the VPS:

```bash
mysql -u crossworlds -p"$(grep DB_PASS /opt/crossworlds-auth/.env | cut -d= -f2)" crossworlds
```

```sql
-- Confirm actual columns first (schema is authoritative, this doc is not):
SHOW CREATE TABLE enemy_templates;

-- Check it isn't already there:
SELECT id FROM enemy_templates WHERE id = 'model_aw_instigator_3d_rigged_basic';

-- Seed (values matched to the Unity prefab: maxHealth 100, damage 12, aggro 8;
-- tune xp/gold to taste — Ashen Wastelands is mid-tier content):
INSERT IGNORE INTO enemy_templates
  (id, display_name, max_hp, damage_min, damage_max, move_speed, aggro_range,
   xp_reward, gold_reward_min, gold_reward_max, loot_source_id)
VALUES
  ('model_aw_instigator_3d_rigged_basic', 'AW Instigator', 100, 10, 14, 3.5, 8,
   25, 2, 6, NULL);
```

`loot_source_id` NULL = no item drops. To add drops later, seed
`loot_tables.source_name` rows and point `loot_source_id` at that name
(pattern: goblin/troll/skeleton/mimic rows already seeded).

Anti-exploit is already in place server-side (hit-gate 30 s window + 2 s kill
cooldown, in-memory) — nothing to configure. Note both maps reset when
`crossworlds-auth` restarts; that's the accepted alpha behavior. **Do not restart
`crossworlds-auth` for this deploy — it isn't part of it.**

## 4. Post-deploy verification (on the VPS)

```bash
# Service came up and stayed up
systemctl status crossworlds --no-pager -n 5

# Listening on the game port
ss -ulnp | grep 7777

# Binary is the fresh one (timestamp should match the deploy)
ls -la /game/Builds/CrossworldsBCE.x86_64

# Server log — scene + spawn sanity
tail -50 /var/log/crossworlds.log
```

In the log, good signs:
- No `Could not spawn` / `Missing Prefab` errors on scene load.
- After a kill in Ashen Wastelands, this build logs respawn explicitly:
  `[EnemyController] Respawn started for 'model_aw_instigator_3D_rigged_Enemy...`
  then `Respawn completed ... agentOnNavMesh=True`.
- A red flag specific to this build: `Cannot respawn ... no NavMesh point was found`
  → the Ashen Wastelands NavMesh doesn't cover the spawn point; needs an editor
  NavMesh re-bake, not a VPS fix.

Auth API sanity (unchanged by this deploy, but confirm it's up since kill
credit depends on it):

```bash
curl -s http://localhost:3000/api/health
curl -s http://localhost:3000/api/enemies/model_aw_instigator_3d_rigged_basic
```

The second call returns the row from step 3 — if it errors, kill rewards won't pay.

## 5. Client parity — the invisible-enemy trap

The Ashen Wastelands **scene** and enemy **prefab** changed. Mirror matches
scene objects by sceneId and prefabs by assetId — an **old Windows client
against the new server will fail to spawn the new enemies** (invisible/missing
enemies, `Could not spawn` spam client-side).

→ **The client build is automated too**: the same CI run's `Client → GitHub
Release` job publishes a matching Windows client to the stable URL
`https://github.com/Brbmuffins/Crossworlds-BCE/releases/latest/download/Crossworlds-Windows.zip`
— that's the link to share. Players on an older download must re-grab it after
the deploy. Two caveats:

- The client job can finish (and update the Release) **before or without** the
  server deploy succeeding — if the server job fails or is cancelled, the
  "latest" client is temporarily ahead of the live server. Only announce after
  the **whole run** is green.
- Any old client zip still served from the VPS web root
  (`/var/www/crossworlds/downloads/`) is a stale side-channel — either update it
  to match or redirect players to the GitHub Release URL.

## 6. Rollback

```bash
cd /home/ubuntu/deploy && sudo bash deploy-server.sh --rollback
```

Restores `/game/Builds.prev` and restarts. Remember old-server + new-client has
the same parity problem in reverse — roll back the client link too if you roll
back the server.

## 7. Out of scope for this handoff

- `Meta GUID Guard` CI failure (23 stale .metas) — repo hygiene, doesn't block
  deploys; separate task already spun off.
- Enemy template *balancing* (level-100 placeholder on the prefab's Health) —
  gameplay tuning, not VPS readiness.
- No auth-server code or DB schema changes in this build beyond the one
  `enemy_templates` row above.
