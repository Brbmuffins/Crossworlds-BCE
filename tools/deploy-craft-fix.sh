#!/usr/bin/env bash
# deploy-craft-fix.sh
#
# Replaces the POST /api/craft handler on the VPS with the corrected version
# (bounded slot search, transaction-fresh slot lookup, equipped-safe stacking,
# and removal of any duplicate handler registration).
#
# Run from the repo root:
#   bash tools/deploy-craft-fix.sh
#
# Rollback:
#   bash tools/deploy-craft-fix.sh --rollback
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

HOST="ubuntu@playcrossworlds.com"
REMOTE_SERVER="/opt/crossworlds-auth/server.js"
REMOTE_BACKUP="/opt/crossworlds-auth/server.js.pre-craftfix"
FIX_FILE="_CONTEXT/professions-craft-fix.js"
CODEMOD="_CONTEXT/apply-craft-fix.js"

# ── Rollback ────────────────────────────────────────────────────────────────
if [[ "${1:-}" == "--rollback" ]]; then
  echo "[rollback] Restoring server.js from ${REMOTE_BACKUP}..."
  ssh "$HOST" "test -f ${REMOTE_BACKUP} && sudo cp ${REMOTE_BACKUP} ${REMOTE_SERVER} && sudo systemctl restart crossworlds-auth && sleep 2 && sudo systemctl is-active crossworlds-auth"
  echo "[rollback] Done."
  exit 0
fi

for f in "$FIX_FILE" "$CODEMOD"; do
  [[ -f "$f" ]] || { echo "[ERROR] Missing $f"; exit 1; }
done

echo "[1/6] Copying fix + codemod to VPS..."
scp "$FIX_FILE" "${HOST}:~/professions-craft-fix.js"
scp "$CODEMOD"  "${HOST}:~/apply-craft-fix.js"

echo "[2/6] Backing up server.js..."
ssh "$HOST" "sudo cp ${REMOTE_SERVER} ${REMOTE_BACKUP}"
echo "      Backup: ${REMOTE_BACKUP}"

echo "[3/6] Applying codemod (writes to a temp copy first)..."
ssh "$HOST" bash <<'REMOTE'
set -euo pipefail
SERVER=/opt/crossworlds-auth/server.js
WORK=$(mktemp)
sudo cp "$SERVER" "$WORK"

node ~/apply-craft-fix.js "$WORK" ~/professions-craft-fix.js

echo "      Syntax-checking patched file..."
node --check "$WORK"

sudo cp "$WORK" "$SERVER"
rm -f "$WORK" ~/apply-craft-fix.js ~/professions-craft-fix.js
echo "      server.js updated."
REMOTE

echo "[4/6] Restarting crossworlds-auth..."
ssh "$HOST" "sudo systemctl restart crossworlds-auth"
sleep 3

echo "[5/6] Verifying service is active..."
if ! ssh "$HOST" "sudo systemctl is-active --quiet crossworlds-auth"; then
  echo "      [FAIL] service not active — rolling back"
  ssh "$HOST" "sudo cp ${REMOTE_BACKUP} ${REMOTE_SERVER} && sudo systemctl restart crossworlds-auth"
  echo "      Rolled back. Check: ssh ${HOST} 'sudo journalctl -u crossworlds-auth -n 40 --no-pager'"
  exit 1
fi

echo "[6/6] Smoke test..."
HEALTH=$(ssh "$HOST" "curl -s -o /dev/null -w '%{http_code}' http://localhost:3000/api/health")
CRAFT=$(ssh "$HOST" "curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:3000/api/craft")

echo "      /api/health          -> $HEALTH  (expect 200)"
echo "      POST /api/craft       -> $CRAFT   (expect 401/403 — route exists, auth required)"

if [[ "$HEALTH" == "200" && ( "$CRAFT" == "401" || "$CRAFT" == "403" ) ]]; then
  echo ""
  echo "[OK] craft-fix deployed. Rollback if needed: bash tools/deploy-craft-fix.sh --rollback"
else
  echo ""
  echo "[WARN] Unexpected status codes — inspect logs:"
  echo "       ssh ${HOST} 'sudo journalctl -u crossworlds-auth -n 40 --no-pager'"
  exit 1
fi
