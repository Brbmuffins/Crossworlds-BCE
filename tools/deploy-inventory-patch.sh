#!/usr/bin/env bash
# deploy-inventory-patch.sh
#
# Deploys POST /api/inventory/add-item to the VPS auth server.
# Run from the repo root:
#   bash tools/deploy-inventory-patch.sh
#
# What this does:
#   1. SCPs the patch JS to the VPS
#   2. Backs up server.js
#   3. Inserts the endpoint block after POST /api/inventory/save
#   4. Restarts crossworlds-auth
#   5. Smoke-tests (expects 401 — route exists, auth required)
# ─────────────────────────────────────────────────────────────────────────────

set -euo pipefail

HOST="ubuntu@playcrossworlds.com"
REMOTE_SERVER="/opt/crossworlds-auth/server.js"
REMOTE_BACKUP="/opt/crossworlds-auth/server.js.bak-$(date +%Y%m%d-%H%M%S)"
PATCH_FILE="_CONTEXT/inventory-add-item-patch.js"

if [[ ! -f "$PATCH_FILE" ]]; then
  echo "[ERROR] Patch file not found: $PATCH_FILE"
  exit 1
fi

echo "[1/5] Copying patch to VPS..."
scp "$PATCH_FILE" "${HOST}:~/inventory-add-item-patch.js"

echo "[2/5] Backing up server.js..."
ssh "$HOST" "cp ${REMOTE_SERVER} ${REMOTE_BACKUP}"
echo "      Backup: ${REMOTE_BACKUP}"

echo "[3/5] Inserting endpoint..."
ssh "$HOST" bash <<'REMOTE'
set -euo pipefail

PATCH=~/inventory-add-item-patch.js
SERVER=/opt/crossworlds-auth/server.js

# Guard: skip if already patched
if grep -q "inventory/add-item" "$SERVER"; then
  echo "      [SKIP] endpoint already present in server.js"
  rm -f "$PATCH"
  exit 0
fi

# Find the line number of the first occurrence of /api/inventory/save
# and insert the patch block before it (so add-item sits just above save).
ANCHOR=$(grep -n "api/inventory/save" "$SERVER" | head -1 | cut -d: -f1)

if [[ -z "$ANCHOR" ]]; then
  echo "[ERROR] Could not find /api/inventory/save anchor in server.js"
  exit 1
fi

echo "      Inserting before line ${ANCHOR} (POST /api/inventory/save)"

# Strip the header comment block from the patch (lines starting with // ═)
# so only the app.post(...) block lands in server.js.
PATCH_BODY=$(sed '/^\/\/ ═/d' "$PATCH")

# Build the patched file: lines before anchor + blank line + patch + rest
head -n $((ANCHOR - 1)) "$SERVER" > /tmp/server_new.js
echo "" >> /tmp/server_new.js
echo "$PATCH_BODY" >> /tmp/server_new.js
echo "" >> /tmp/server_new.js
tail -n +$ANCHOR "$SERVER" >> /tmp/server_new.js

mv /tmp/server_new.js "$SERVER"
rm -f "$PATCH"
echo "      Done."
REMOTE

echo "[4/5] Restarting crossworlds-auth..."
ssh "$HOST" "sudo systemctl restart crossworlds-auth"
sleep 2

echo "[5/5] Smoke test..."
STATUS=$(ssh "$HOST" "curl -s -o /dev/null -w '%{http_code}' -X POST http://localhost:3000/api/inventory/add-item")

if [[ "$STATUS" == "401" ]]; then
  echo "      PASS — HTTP $STATUS (route exists, auth required)"
  echo ""
  echo "[OK] inventory-add-item deployed successfully."
else
  echo "      FAIL — HTTP $STATUS (expected 401)"
  echo ""
  echo "[WARN] Check logs: ssh ${HOST} 'sudo journalctl -u crossworlds-auth -n 30 --no-pager'"
  exit 1
fi
