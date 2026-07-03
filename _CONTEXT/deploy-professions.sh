#!/usr/bin/env bash
# ═══════════════════════════════════════════════════════════════════════════════
#  deploy-professions.sh — Professions system VPS deployment
#
#  Applies BOTH pending patches in one shot:
#    1. professions-full-patch.sql  — items, refined materials, consumables,
#                                     gear, smelt recipes, craft recipes
#    2. professions-server-patch.js — POST /api/professions/award-xp
#                                     GET  /api/professions/recipes/:characterId
#                                     POST /api/craft (extended)
#
#  Run from the repo root on your local machine:
#    scp _CONTEXT/deploy-professions.sh \
#        _CONTEXT/professions-full-patch.sql \
#        _CONTEXT/professions-server-patch.js \
#        ubuntu@playcrossworlds.com:~
#    ssh ubuntu@playcrossworlds.com "bash ~/deploy-professions.sh"
#
#  Rollback (if something breaks):
#    ssh ubuntu@playcrossworlds.com "bash ~/deploy-professions.sh --rollback"
# ═══════════════════════════════════════════════════════════════════════════════

set -euo pipefail

SERVER_JS="/opt/crossworlds-auth/server.js"
BACKUP="/opt/crossworlds-auth/server.js.pre-professions"
ENV_FILE="/opt/crossworlds-auth/.env"
SQL_PATCH="$HOME/professions-full-patch.sql"
JS_PATCH="$HOME/professions-server-patch.js"

# ── Colours ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GRN='\033[0;32m'; YLW='\033[1;33m'; NC='\033[0m'
info()  { echo -e "${GRN}[INFO]${NC}  $*"; }
warn()  { echo -e "${YLW}[WARN]${NC}  $*"; }
error() { echo -e "${RED}[ERR]${NC}   $*"; exit 1; }

# ── Rollback ─────────────────────────────────────────────────────────────────
if [[ "${1:-}" == "--rollback" ]]; then
  if [[ ! -f "$BACKUP" ]]; then
    error "No backup found at $BACKUP — nothing to roll back"
  fi
  warn "Rolling back server.js ..."
  sudo cp "$BACKUP" "$SERVER_JS"
  sudo systemctl restart crossworlds-auth
  sleep 2
  sudo systemctl is-active crossworlds-auth && info "Rollback complete — auth server running" \
    || error "Rollback applied but auth server failed to start — check journalctl"
  exit 0
fi

# ── Preflight ─────────────────────────────────────────────────────────────────
info "=== Professions deployment starting ==="

[[ -f "$SQL_PATCH" ]] || error "SQL patch not found: $SQL_PATCH"
[[ -f "$JS_PATCH"  ]] || error "JS patch not found: $JS_PATCH"
[[ -f "$SERVER_JS" ]] || error "server.js not found: $SERVER_JS"
[[ -f "$ENV_FILE"  ]] || error ".env not found: $ENV_FILE"

# Pull DB credentials from .env
DB_PASS=$(grep -E '^DB_PASS=' "$ENV_FILE" | cut -d= -f2- | tr -d '"'"'" )
DB_NAME=$(grep -E '^DB_NAME=' "$ENV_FILE" | cut -d= -f2- | tr -d '"'"'" 2>/dev/null || echo "crossworlds")
DB_USER=$(grep -E '^DB_USER=' "$ENV_FILE" | cut -d= -f2- | tr -d '"'"'" 2>/dev/null || echo "crossworlds")

[[ -n "$DB_PASS" ]] || error "DB_PASS not found in $ENV_FILE"

# ── Backup server.js ──────────────────────────────────────────────────────────
info "Backing up server.js → $BACKUP"
sudo cp "$SERVER_JS" "$BACKUP"

# ── Guard: check endpoints not already present ────────────────────────────────
if grep -q "professions/award-xp" "$SERVER_JS"; then
  warn "award-xp endpoint already in server.js — skipping JS patch (SQL still runs)"
  SKIP_JS=true
else
  SKIP_JS=false
fi

# ── Apply SQL patch ───────────────────────────────────────────────────────────
info "Running SQL patch ..."
mysql -u "$DB_USER" -p"$DB_PASS" "$DB_NAME" < "$SQL_PATCH" || error "SQL patch failed"
info "SQL patch applied"

# ── Apply JS patch ────────────────────────────────────────────────────────────
if [[ "$SKIP_JS" == "false" ]]; then
  info "Patching server.js ..."

  # Find the line number of GET /api/professions/:characterId to insert before it
  INSERT_BEFORE=$(grep -n "GET /api/professions" "$SERVER_JS" | head -1 | cut -d: -f1)

  if [[ -z "$INSERT_BEFORE" ]]; then
    # Fallback: insert before the last app.listen() call
    warn "GET /api/professions route not found — inserting before app.listen()"
    INSERT_BEFORE=$(grep -n "app.listen" "$SERVER_JS" | tail -1 | cut -d: -f1)
  fi

  [[ -n "$INSERT_BEFORE" ]] || error "Cannot find insertion point in server.js"

  # Split file and inject patch
  TMPFILE=$(mktemp)
  head -n "$((INSERT_BEFORE - 1))" "$SERVER_JS"    >  "$TMPFILE"
  echo ""                                            >> "$TMPFILE"
  echo "// ── Professions patch (auto-applied by deploy-professions.sh) ──" >> "$TMPFILE"
  cat "$JS_PATCH"                                    >> "$TMPFILE"
  echo ""                                            >> "$TMPFILE"
  tail -n "+${INSERT_BEFORE}" "$SERVER_JS"           >> "$TMPFILE"

  sudo cp "$TMPFILE" "$SERVER_JS"
  rm "$TMPFILE"
  info "server.js patched"
else
  warn "Skipped JS patch (endpoints already present)"
fi

# ── Restart auth server ───────────────────────────────────────────────────────
info "Restarting crossworlds-auth ..."
sudo systemctl restart crossworlds-auth
sleep 3

# ── Verify ────────────────────────────────────────────────────────────────────
if sudo systemctl is-active --quiet crossworlds-auth; then
  info "crossworlds-auth is running"
else
  error "crossworlds-auth failed to start — rolling back"
  sudo cp "$BACKUP" "$SERVER_JS"
  sudo systemctl restart crossworlds-auth
  error "Rolled back. Check: sudo journalctl -u crossworlds-auth -n 30 --no-pager"
fi

# Health check
HTTP=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/api/health)
if [[ "$HTTP" == "200" ]]; then
  info "Health check passed (HTTP $HTTP)"
else
  warn "Health check returned HTTP $HTTP — service is running but /api/health may need checking"
fi

# Smoke test new endpoint (no JWT needed to confirm route exists, expect 401 not 404)
AWARD_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:3000/api/professions/award-xp)
RECIPES_STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:3000/api/professions/recipes/1)
if [[ "$AWARD_STATUS" == "401" || "$AWARD_STATUS" == "403" ]]; then
  info "POST /api/professions/award-xp  → $AWARD_STATUS (route exists, auth required ✓)"
else
  warn "POST /api/professions/award-xp  → $AWARD_STATUS (expected 401/403)"
fi
if [[ "$RECIPES_STATUS" == "401" || "$RECIPES_STATUS" == "403" ]]; then
  info "GET  /api/professions/recipes/1 → $RECIPES_STATUS (route exists, auth required ✓)"
else
  warn "GET  /api/professions/recipes/1 → $RECIPES_STATUS (expected 401/403)"
fi

echo ""
info "=== Deployment complete ==="
echo ""
echo "  Rollback:  bash ~/deploy-professions.sh --rollback"
echo "  Logs:      sudo journalctl -u crossworlds-auth -n 30 --no-pager"
echo ""
