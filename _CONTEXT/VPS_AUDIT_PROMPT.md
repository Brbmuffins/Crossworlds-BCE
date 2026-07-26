# Crossworlds BCE — VPS Audit & Update Prompt

Paste the entire contents of this file as your opening message in a Claude Code session
that is SSH'd into `ubuntu@playcrossworlds.com` (or run `claude` directly on the VPS).

---

You are the senior backend developer for **Crossworlds BCE**, a live multiplayer action RPG.
Audit the VPS at `ubuntu@playcrossworlds.com` and bring it to the current documented state.
Work methodically: read before touching, fix minimally, verify after each change.

**Never touch:**
- `/login`, `/character`, `/character/gear/equip`, `GET /items` — Unity calls these on every spawn
- `item_template`, `item_instance`, `character_gear` tables
- Credentials or `.env` files (read them, never log or overwrite them)
- Port numbers (3000 auth · 4000 dashboard · 7777/UDP game · 3001 Kuma — all frozen)

**Response convention for all new endpoints:**
```js
{ success: true, data: { ... } }
{ success: false, error: "player-readable string" }
```

---

## Phase 1 — Locate Everything

Run these exactly. Report the full output of each before doing anything else.

```bash
# 1. What systemd services actually exist?
systemctl list-units --type=service | grep -E 'rod|crossworlds'

# 2. What are the exact ExecStart lines?
for svc in rod-server rod-auth rod-dashboard crossworlds crossworlds-auth crossworlds-dashboard; do
  f="/etc/systemd/system/${svc}.service"
  [ -f "$f" ] && echo "=== $f ===" && grep -E 'ExecStart|WorkingDirectory|User' "$f"
done

# 3. What Node server paths actually exist on disk?
ls -la /opt/rod-auth/server.js 2>/dev/null && echo "EXISTS: /opt/rod-auth/server.js"
ls -la /opt/crossworlds-auth/server.js 2>/dev/null && echo "EXISTS: /opt/crossworlds-auth/server.js"
ls -la /opt/rod-dashboard/server.js 2>/dev/null && echo "EXISTS: /opt/rod-dashboard/server.js"
ls -la /opt/crossworlds-dashboard/server.js 2>/dev/null && echo "EXISTS: /opt/crossworlds-dashboard/server.js"

# 4. What web roots exist?
ls /var/www/ 2>/dev/null

# 5. Binary — path is the numbered run dir in the unit's ExecStart (not /game/Builds)
GAME_BIN=$(systemctl show -p ExecStart --value crossworlds-server 2>/dev/null | grep -oE '/[^ ]*CrossWords\.x86_64' | head -1)
ls -la "$GAME_BIN" 2>/dev/null || echo "MISSING: game binary (checked: ${GAME_BIN:-<no ExecStart>})"

# 6. All services status
sudo systemctl status --no-pager $(systemctl list-units --type=service --plain --no-legend | grep -Eo 'rod-[^ ]+|crossworlds[^ ]*\.service' | tr '\n' ' ') 2>/dev/null | head -80

# 7. Port check
ss -tlnp | grep -E '3000|4000|7777'
ss -ulnp | grep 7777
```

---

## Phase 2 — Database Audit

Identify the correct auth server path from Phase 1, then:

```bash
# Read the actual DB name and user from .env (do NOT log the password)
AUTH_ENV=$(ls /opt/crossworlds-auth/.env /opt/rod-auth/.env 2>/dev/null | head -1)
echo "Using env: $AUTH_ENV"
grep -E '^DB_(HOST|USER|NAME|DATABASE)' "$AUTH_ENV" 2>/dev/null
```

Then connect to MySQL and verify every table in the schema:

```sql
-- Run via: mysql -u <user> -p <dbname>
-- (Use the DB_USER and DB_NAME values from the .env above)

SHOW TABLES;

-- Old system — must exist, must not be altered
SHOW CREATE TABLE item_template\G
SHOW CREATE TABLE item_instance\G
SHOW CREATE TABLE character_gear\G
DESCRIBE loot_tables;

-- New system — verify columns
DESCRIBE characters;
DESCRIBE items;
DESCRIBE inventory;
DESCRIBE professions;
DESCRIBE recipes;
DESCRIBE recipe_ingredients;
DESCRIBE enemy_templates;

-- Phase 2 stubs
SHOW CREATE TABLE gold_transactions\G
SHOW CREATE TABLE broadcast_messages\G

-- Class names check
SELECT CLASS_NAMES FROM information_schema.TABLES WHERE 1=0; -- placeholder
-- Actually: grep the server.js for CLASS_NAMES array
```

```bash
# Grep server.js for class name array
grep -n 'CLASS_NAMES\|classNames\|class_names' $(ls /opt/crossworlds-auth/server.js /opt/rod-auth/server.js 2>/dev/null | head -1)
```

**Expected class array (indices are canonical — never renumber):**
```js
const CLASS_NAMES = ['Warden', 'Ironclad', 'Shadowblade', 'Cleric', 'Arcanist'];
// Index 0=Warden 1=Ironclad 2=Shadowblade 3=Cleric 4=Arcanist
// Validator must be: classIndex > 4 (not > 3)
```

---

## Phase 3 — API Endpoint Audit

Grep server.js to confirm every required endpoint exists:

```bash
SRV=$(ls /opt/crossworlds-auth/server.js /opt/rod-auth/server.js 2>/dev/null | head -1)

echo "=== ENDPOINT CHECKLIST ==="
for pattern in \
  "GET.*\/api\/health" \
  "GET.*\/api\/items" \
  "POST.*\/api\/character\/save-progress" \
  "GET.*\/api\/inventory\/" \
  "POST.*\/api\/inventory\/save" \
  "POST.*\/api\/inventory\/equip" \
  "GET.*\/api\/professions\/" \
  "GET.*\/api\/recipes" \
  "POST.*\/api\/craft" \
  "POST.*\/api\/loot\/roll" \
  "POST.*\/api\/loot\/drop" \
  "POST.*\/api\/gold\/adjust" \
  "GET.*\/api\/character\/stats\/" \
  "GET.*\/api\/enemies" \
  "POST.*\/api\/enemies\/" \
  "POST.*\/api\/combat\/hit" \
  "POST.*\/api\/combat\/kill" \
  "GET.*\/api\/maintenance\/status" \
  "GET.*\/api\/broadcast\/pending" \
  "GET.*\/api\/admin\/stats" \
  "POST.*\/api\/admin\/broadcast" \
  "POST.*\/api\/admin\/maintenance\/toggle"
do
  grep -qE "$pattern" "$SRV" \
    && echo "  OK  $pattern" \
    || echo "  MISSING  $pattern"
done
```

```bash
# Verify combat anti-exploit maps exist
echo "=== COMBAT ANTI-EXPLOIT ==="
grep -n 'recentHits\|lastKillTime\|HIT_WINDOW_MS\|KILL_COOLDOWN_MS' "$SRV"
```

**Required anti-exploit block (if missing, add near the top of server.js):**
```js
const HIT_WINDOW_MS    = 30_000;  // hit must arrive within 30s before kill
const KILL_COOLDOWN_MS =  2_000;  // min 2s between kills per character
const recentHits    = new Map();  // key: `${charId}:${enemyTemplateId}` → timestamp
const lastKillTime  = new Map();  // key: charId → timestamp
// Prune recentHits every 60s
setInterval(() => {
  const cutoff = Date.now() - HIT_WINDOW_MS;
  for (const [k, t] of recentHits) if (t < cutoff) recentHits.delete(k);
}, 60_000);
```

---

## Phase 4 — Schema Migrations (run only if column/table is missing)

Use this safe pattern for every schema addition — MySQL 8 has no `ADD COLUMN IF NOT EXISTS`:

```sql
-- Template — replace tbl/col/TYPE/DEFAULT as needed
SET @sql = IF(
  (SELECT COUNT(*) FROM information_schema.COLUMNS
   WHERE TABLE_SCHEMA = DATABASE()
     AND TABLE_NAME   = 'tbl'
     AND COLUMN_NAME  = 'col') = 0,
  'ALTER TABLE tbl ADD COLUMN col TYPE NOT NULL DEFAULT DEFAULT_VAL',
  'SELECT 1'
);
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
```

### Required columns in `characters`

```sql
-- Run these in order; each is safe to run even if column already exists
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='level')=0,'ALTER TABLE characters ADD COLUMN level INT NOT NULL DEFAULT 1','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='xp')=0,'ALTER TABLE characters ADD COLUMN xp INT NOT NULL DEFAULT 0','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='gold')=0,'ALTER TABLE characters ADD COLUMN gold INT NOT NULL DEFAULT 0','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='stat_str')=0,'ALTER TABLE characters ADD COLUMN stat_str INT NOT NULL DEFAULT 10','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='stat_agi')=0,'ALTER TABLE characters ADD COLUMN stat_agi INT NOT NULL DEFAULT 10','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='stat_int')=0,'ALTER TABLE characters ADD COLUMN stat_int INT NOT NULL DEFAULT 10','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='characters' AND COLUMN_NAME='stat_vit')=0,'ALTER TABLE characters ADD COLUMN stat_vit INT NOT NULL DEFAULT 10','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
```

### Required columns in `loot_tables`

```sql
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='loot_tables' AND COLUMN_NAME='source_name')=0,'ALTER TABLE loot_tables ADD COLUMN source_name VARCHAR(64) NULL','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
SET @sql = IF((SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='loot_tables' AND COLUMN_NAME='new_item_id')=0,'ALTER TABLE loot_tables ADD COLUMN new_item_id VARCHAR(64) NULL','SELECT 1'); PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;
```

### Create missing tables (safe — CREATE TABLE IF NOT EXISTS)

```sql
CREATE TABLE IF NOT EXISTS enemy_templates (
  id               VARCHAR(64) PRIMARY KEY,
  display_name     VARCHAR(128) NOT NULL,
  max_hp           INT NOT NULL DEFAULT 100,
  damage_min       INT NOT NULL DEFAULT 5,
  damage_max       INT NOT NULL DEFAULT 15,
  move_speed       FLOAT NOT NULL DEFAULT 3.5,
  aggro_range      FLOAT NOT NULL DEFAULT 8.0,
  xp_reward        INT NOT NULL DEFAULT 10,
  gold_reward_min  INT NOT NULL DEFAULT 1,
  gold_reward_max  INT NOT NULL DEFAULT 5,
  loot_source_id   VARCHAR(64) NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS items (
  id         VARCHAR(64) PRIMARY KEY,
  name       VARCHAR(128) NOT NULL,
  rarity     VARCHAR(32) NOT NULL DEFAULT 'common',
  item_type  VARCHAR(64) NOT NULL,
  stat_bonus JSON NULL,
  sell_value INT NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS inventory (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  character_id INT NOT NULL,
  slot_index   INT NOT NULL,
  item_id      VARCHAR(64) NOT NULL,
  quantity     INT NOT NULL DEFAULT 1,
  equipped     TINYINT NOT NULL DEFAULT 0,
  UNIQUE KEY uq_char_slot (character_id, slot_index)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS professions (
  id            INT AUTO_INCREMENT PRIMARY KEY,
  character_id  INT NOT NULL,
  profession_id VARCHAR(64) NOT NULL,
  skill_level   INT NOT NULL DEFAULT 1,
  skill_xp      INT NOT NULL DEFAULT 0,
  UNIQUE KEY uq_char_prof (character_id, profession_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS recipes (
  id                      INT AUTO_INCREMENT PRIMARY KEY,
  profession_id           VARCHAR(64) NOT NULL,
  skill_level_required    INT NOT NULL DEFAULT 1,
  result_item_id          VARCHAR(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS recipe_ingredients (
  id        INT AUTO_INCREMENT PRIMARY KEY,
  recipe_id INT NOT NULL,
  item_id   VARCHAR(64) NOT NULL,
  quantity  INT NOT NULL DEFAULT 1
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS gold_transactions (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  character_id INT NOT NULL,
  amount       INT NOT NULL,
  reason       VARCHAR(128) NULL,
  created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE IF NOT EXISTS broadcast_messages (
  id           INT AUTO_INCREMENT PRIMARY KEY,
  message      TEXT NOT NULL,
  delivered    TINYINT NOT NULL DEFAULT 0,
  created_at   TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
```

### Seed enemy_templates (INSERT IGNORE — safe to re-run)

```sql
INSERT IGNORE INTO enemy_templates
  (id, display_name, max_hp, damage_min, damage_max, move_speed, aggro_range, xp_reward, gold_reward_min, gold_reward_max, loot_source_id)
VALUES
  ('goblin',    'Goblin Scout',    60,  5, 12,  3.5, 8.0,  10, 1, 3,  'goblin'),
  ('troll',     'Cave Troll',     200, 12, 22,  2.0, 6.0,  35, 3, 8,  'troll'),
  ('skeleton',  'Skeleton Archer', 80,  8, 16,  3.0, 10.0, 20, 2, 5,  'skeleton'),
  ('mimic',     'Mimic Chest',    150, 20, 30,  1.5, 4.0,  50, 5, 15, 'mimic');
```

---

## Phase 5 — Service Name Reconciliation

> **Goal:** the actual systemd service names on disk may still be `rod-server`, `rod-auth`, `rod-dashboard`.
> Do NOT rename the services unless you confirm the ExecStart paths still match after renaming.
> Update `VPS_SERVER.md` in the repo to reflect whichever names are actually active.

```bash
# Confirm which names respond
sudo systemctl is-active rod-server crossworlds rod-auth crossworlds-auth rod-dashboard crossworlds-dashboard 2>/dev/null
```

If the live service is still named `rod-auth` but the Node code is at `/opt/crossworlds-auth/server.js`, that's fine — the name is cosmetic. Report the actual active names; do not rename services mid-session as that requires daemon-reload and a restart window.

If the `StartLimitIntervalSec=0` guard is missing from the game server service file:

```bash
# Check
grep 'StartLimitIntervalSec' /etc/systemd/system/$(systemctl list-units --plain --no-legend | grep -Eo 'rod-server|crossworlds\.service' | head -1).service

# Add if missing (replace SERVICE_NAME with the real name)
sudo sed -i '/^\[Service\]/a StartLimitIntervalSec=0' /etc/systemd/system/SERVICE_NAME.service
sudo systemctl daemon-reload
```

---

## Phase 6 — Health Verification

Run after all changes are complete:

```bash
# Full health sweep
sudo systemctl status --no-pager $(systemctl list-units --plain --no-legend | grep -Eo 'rod-[^ ]+\.service|crossworlds[^ ]*\.service' | tr '\n' ' ')

curl -s http://localhost:3000/api/health | python3 -m json.tool

# Smoke test public endpoints (no auth required)
curl -s http://localhost:3000/api/maintenance/status | python3 -m json.tool
curl -s http://localhost:3000/api/enemies | python3 -m json.tool | head -30

# Confirm JWT auth works (use a real token from an active session, or skip if none available)
# curl -s -H "Authorization: Bearer TOKEN" http://localhost:3000/api/inventory/1

# UDP game port
ss -ulnp | grep 7777 && echo "UDP 7777 OK" || echo "WARNING: UDP 7777 not open"

# Recent logs — no panic/fatal lines
sudo journalctl -u crossworlds-auth -n 30 --no-pager 2>/dev/null || sudo journalctl -u rod-auth -n 30 --no-pager
tail -20 /var/log/crossworlds.log
```

---

## Phase 7 — Report

After completing all phases, report:

1. **Service names found** — what is actually running and at what paths
2. **Schema delta** — which tables/columns were missing and were added
3. **Missing endpoints** — any `/api/*` routes not found in server.js (list them; do not add without review)
4. **CLASS_NAMES** — current value vs expected `['Warden','Ironclad','Shadowblade','Cleric','Arcanist']` with validator `> 4`
5. **Anti-exploit** — present / missing / partial
6. **StartLimitIntervalSec** — present / added
7. **Any errors** in the final health sweep
8. **What was NOT touched** — confirm old gear endpoints and `.env` files were read-only

Update `/opt/crossworlds-auth/CLAUDE.md` (or equivalent on the server) to reflect any structural changes made.
