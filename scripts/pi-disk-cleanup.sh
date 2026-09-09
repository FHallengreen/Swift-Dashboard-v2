#!/usr/bin/env bash
# Weekly disk reclaim for the Raspberry Pi deploy host.
#
# Safe by design:
#   - never touches named volumes (db-data holds MySQL)
#   - never removes images used by a running container
#   - only escalates to a full image prune when disk is genuinely tight
#
# Install: see docs/Raspberry/Setup.md
set -uo pipefail

LOW_WATER_KB=$((5 * 1024 * 1024))   # 5 GB
RUNNER_HOME="${RUNNER_HOME:-/home/pi}"   # cron runs as root, so do not rely on $HOME
RUNNER_DIRS=("$RUNNER_HOME/github-runner" "$RUNNER_HOME/actions-runner")
DIAG_KEEP=10

log() { echo "[pi-disk-cleanup] $*"; logger -t pi-disk-cleanup "$*" 2>/dev/null || true; }
avail_kb() { df --output=avail / | tail -1 | tr -d ' '; }

log "start - $(avail_kb) KB free"

# --- GitHub runner diagnostic logs (unbounded by default) -------------------
for dir in "${RUNNER_DIRS[@]}"; do
  [ -d "$dir/_diag" ] || continue
  find "$dir/_diag" -maxdepth 1 -type f \( -name '*.log' -o -name '*.json' \) \
       -printf '%T@ %p\n' 2>/dev/null \
    | sort -rn | tail -n "+$((DIAG_KEEP + 1))" | cut -d' ' -f2- \
    | xargs -r rm -f
  log "trimmed $dir/_diag to newest $DIAG_KEEP files"
done

# --- Docker: cheap reclaim --------------------------------------------------
docker container prune -f
docker image prune -f
docker network prune -f
docker builder prune -f --filter 'until=168h' --keep-storage 2GB
log "pruned stopped containers, dangling images, old build cache"

# --- Docker: escalate only if still tight -----------------------------------
if [ "$(avail_kb)" -lt "$LOW_WATER_KB" ]; then
  log "under 5GB free - pruning all unused images and full build cache"
  docker image prune -af
  docker builder prune -af
fi

# --- Journal ----------------------------------------------------------------
if command -v journalctl >/dev/null 2>&1; then
  sudo journalctl --vacuum-time=14d >/dev/null 2>&1 \
    && log "vacuumed journal to 14d"
fi

# --- APT cache --------------------------------------------------------------
sudo apt-get clean >/dev/null 2>&1 && log "cleared apt cache"

log "done - $(avail_kb) KB free"
df -h /
