#!/usr/bin/env bash
# deploy-server.sh — VPS-side deploy of the Unity dedicated server.
# Usage (on the VPS, with crossworlds-server.tar.gz in the same directory):
#   sudo bash deploy-server.sh
#
# Backs up the current build, extracts the new one, restarts the service,
# and verifies it stays up. Rollback: sudo bash deploy-server.sh --rollback
set -euo pipefail

BUILDS=/game/Builds
TAR=crossworlds-server.tar.gz
BACKUP=/game/Builds.prev

if [[ "${1:-}" == "--rollback" ]]; then
  [[ -d $BACKUP ]] || { echo "No backup at $BACKUP"; exit 1; }
  systemctl stop crossworlds
  rm -rf "$BUILDS" && mv "$BACKUP" "$BUILDS"
  systemctl start crossworlds
  echo "Rolled back. Status:"; systemctl status crossworlds --no-pager -n 5
  exit 0
fi

[[ -f $TAR ]] || { echo "$TAR not found next to this script"; exit 1; }

echo "== Backing up current build"
rm -rf "$BACKUP"
[[ -d $BUILDS ]] && cp -a "$BUILDS" "$BACKUP"

echo "== Deploying"
systemctl stop crossworlds
mkdir -p "$BUILDS"
tar -xzf "$TAR" -C "$BUILDS"
chmod +x "$BUILDS/CrossworldsBCE.x86_64"
systemctl start crossworlds

echo "== Verifying (10s)"
sleep 10
if systemctl is-active --quiet crossworlds; then
  echo "OK — service running. Recent log:"
  tail -n 10 /var/log/crossworlds.log 2>/dev/null || journalctl -u crossworlds -n 10 --no-pager
else
  echo "FAILED — service not running. Rolling back."
  systemctl stop crossworlds || true
  rm -rf "$BUILDS" && mv "$BACKUP" "$BUILDS"
  systemctl start crossworlds
  exit 1
fi
