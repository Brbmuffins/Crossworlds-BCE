#!/usr/bin/env bash
# deploy-server.sh — VPS-side deploy of the Unity dedicated server.
# Usage (on the VPS, with crossworlds-server.tar.gz in the same directory):
#   sudo bash deploy-server.sh
#   sudo bash deploy-server.sh --rollback
#
# Self-healing by design: the game unit name and its install directory are
# DISCOVERED from systemd at run time, not hardcoded. This survives the
# 2026-07-25 retirement of `crossworlds.service` (now `crossworlds-server`)
# and the move off the fixed `/game/Builds` path — the script deploys into
# whatever directory the live unit's ExecStart actually points at.
set -euo pipefail

TAR=crossworlds-server.tar.gz

# ── Discover the live game unit ──────────────────────────────────────────
# Prefer the current name, fall back to the retired one, so this keeps
# working across the rename in either direction.
UNIT=""
for candidate in crossworlds-server crossworlds; do
  if systemctl cat "$candidate" >/dev/null 2>&1; then
    UNIT="$candidate"
    break
  fi
done
[[ -n "$UNIT" ]] || { echo "FATAL: no crossworlds game unit found (tried crossworlds-server, crossworlds)"; exit 1; }

# ── Discover the install dir from the unit's ExecStart ───────────────────
# ExecStart is e.g. `/game/<runid>/CrossworldsBCE.x86_64 -batchmode ...`.
# The install dir is the directory that binary lives in — whatever it is.
EXEC_BIN="$(systemctl show -p ExecStart --value "$UNIT" \
            | grep -oE '/[^ ]*CrossworldsBCE\.x86_64' | head -n1)"
[[ -n "$EXEC_BIN" ]] || { echo "FATAL: could not parse ExecStart binary path for unit '$UNIT'"; exit 1; }
BUILDS="$(dirname "$EXEC_BIN")"
# Safety rail: never operate outside /game (guards against a mangled parse
# turning `rm -rf` loose on the wrong directory).
case "$BUILDS" in
  /game/*) : ;;
  *) echo "FATAL: refusing to deploy into unexpected dir '$BUILDS' (must be under /game)"; exit 1 ;;
esac
BACKUP="${BUILDS%/}.prev"

echo "== Target unit:      $UNIT"
echo "== Install dir:      $BUILDS"
echo "== Backup dir:       $BACKUP"

# ── Rollback path ────────────────────────────────────────────────────────
if [[ "${1:-}" == "--rollback" ]]; then
  [[ -d $BACKUP ]] || { echo "No backup at $BACKUP"; exit 1; }
  systemctl stop "$UNIT"
  rm -rf "$BUILDS" && mv "$BACKUP" "$BUILDS"
  systemctl start "$UNIT"
  echo "Rolled back. Status:"; systemctl status "$UNIT" --no-pager -n 5
  exit 0
fi

[[ -f $TAR ]] || { echo "$TAR not found next to this script"; exit 1; }

echo "== Backing up current build"
rm -rf "$BACKUP"
[[ -d $BUILDS ]] && cp -a "$BUILDS" "$BACKUP"

echo "== Deploying"
systemctl stop "$UNIT"
mkdir -p "$BUILDS"
tar -xzf "$TAR" -C "$BUILDS"
chmod +x "$BUILDS/CrossworldsBCE.x86_64"
systemctl start "$UNIT"

echo "== Verifying (10s)"
sleep 10
if systemctl is-active --quiet "$UNIT"; then
  echo "OK — $UNIT running. Recent log:"
  tail -n 10 /var/log/crossworlds.log 2>/dev/null || journalctl -u "$UNIT" -n 10 --no-pager
else
  echo "FAILED — $UNIT not running. Rolling back."
  systemctl stop "$UNIT" || true
  if [[ -d $BACKUP ]]; then
    rm -rf "$BUILDS" && mv "$BACKUP" "$BUILDS"
    systemctl start "$UNIT"
  else
    echo "WARNING: no backup existed to roll back to."
  fi
  exit 1
fi
