from __future__ import annotations

import argparse
from contextlib import closing
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import sqlite3
import subprocess
import sys

from .config import Settings
from .client import RobinhoodReadOnlyClient
from .discord import DiscordNotificationError, DiscordWebhook
from .shadow import ShadowStore
from .professional import EventRiskCalendar


PROJECT_ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = PROJECT_ROOT / "data"
STATUS_DATABASE = DATA_DIR / "scheduler-status.db"
URL_PATTERN = re.compile(r"https://discord\.com/api/webhooks/[^\s]+")


def _safe(text: str) -> str:
    return URL_PATTERN.sub("[discord-webhook-redacted]", text)


def _append_log(mode: str, text: str) -> None:
    log_dir = DATA_DIR / "logs"
    log_dir.mkdir(parents=True, exist_ok=True)
    date = datetime.now(timezone.utc).strftime("%Y-%m-%d")
    timestamp = datetime.now(timezone.utc).isoformat()
    with (log_dir / f"shadow-{date}.log").open("a", encoding="utf-8") as handle:
        handle.write(f"[{timestamp}] {mode}\n{_safe(text).strip()}\n")


def _record_run(mode: str, returncode: int) -> None:
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    with closing(sqlite3.connect(STATUS_DATABASE)) as connection:
        connection.execute(
            """CREATE TABLE IF NOT EXISTS task_runs (
                   mode TEXT PRIMARY KEY, completed_at TEXT NOT NULL, returncode INTEGER NOT NULL
               )"""
        )
        connection.execute(
            """INSERT INTO task_runs(mode, completed_at, returncode) VALUES (?, ?, ?)
               ON CONFLICT(mode) DO UPDATE SET
                   completed_at=excluded.completed_at, returncode=excluded.returncode""",
            (mode, datetime.now(timezone.utc).isoformat(), returncode),
        )
        connection.commit()


def _calendar_marker(key: str, *, write: bool = False) -> bool:
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    with closing(sqlite3.connect(STATUS_DATABASE)) as connection:
        connection.execute("CREATE TABLE IF NOT EXISTS calendar_notifications (marker TEXT PRIMARY KEY, sent_at TEXT NOT NULL)")
        exists = connection.execute("SELECT 1 FROM calendar_notifications WHERE marker=?", (key,)).fetchone() is not None
        if write:
            connection.execute("INSERT OR IGNORE INTO calendar_notifications(marker,sent_at) VALUES(?,?)",
                               (key, datetime.now(timezone.utc).isoformat()))
            connection.commit()
        return exists


def _calendar_message(events: list[dict[str, object]], title: str) -> str:
    lines = [
        f"📅 **{title}**",
        "_New shadow entries pause during these windows. Open observations remain monitored._",
        "",
        "**Upcoming events**",
    ]
    for event in events:
        start = datetime.fromtimestamp(int(event["start"]), timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
        end = datetime.fromtimestamp(int(event["end"]), timezone.utc).strftime("%H:%M UTC")
        lines.append(f"• **{start}–{end}** — {event['reason']}")
    if not events:
        lines.append("• No upcoming events are currently listed")
    lines.extend(["", "🔒 **Research mode — real orders remain disabled.**"])
    return "\n".join(lines)


def _run_calendar() -> int:
    try:
        settings = Settings.from_environment(require_credentials=False)
        if not settings.discord_calendar_webhook_url:
            raise ValueError("DISCORD_CALENDAR_WEBHOOK_URL is not configured")
        now = int(datetime.now(timezone.utc).timestamp())
        events = [event for event in EventRiskCalendar(DATA_DIR / "event-risk.json").events()
                  if int(event["end"]) >= now]
        digest = hashlib.sha256(json.dumps(events, sort_keys=True).encode()).hexdigest()
        webhook = DiscordWebhook(settings.discord_calendar_webhook_url)
        if not _calendar_marker(f"calendar:{digest}"):
            webhook.send(_calendar_message(events, "Event-Risk Calendar Updated"))
            _calendar_marker(f"calendar:{digest}", write=True)
        imminent = [event for event in events if now <= int(event["start"]) <= now + 48 * 3600]
        for event in imminent:
            marker = f"reminder:{event['start']}:{event['reason']}"
            if not _calendar_marker(marker):
                webhook.send(_calendar_message([event], "Event-Risk Reminder — Within 48 Hours"))
                _calendar_marker(marker, write=True)
        _append_log("calendar", f"exit=0; upcoming={len(events)}; imminent={len(imminent)}")
        _record_run("calendar", 0)
        return 0
    except Exception as error:
        _append_log("calendar", f"Calendar notification failure: {type(error).__name__}: {error}")
        _record_run("calendar", 1)
        return 1


def _last_runs() -> dict[str, tuple[str, int]]:
    if not STATUS_DATABASE.exists():
        return {}
    with closing(sqlite3.connect(STATUS_DATABASE)) as connection:
        rows = connection.execute(
            "SELECT mode, completed_at, returncode FROM task_runs WHERE mode IN ('scan', 'monitor')"
        ).fetchall()
    return {str(row[0]): (str(row[1]), int(row[2])) for row in rows}


def _health_message(summary: dict[str, object], runs: dict[str, tuple[str, int]]) -> str:
    def status(mode: str) -> str:
        value = runs.get(mode)
        if value is None:
            return "⚪ Not recorded yet"
        label = "🟢 Healthy" if value[1] == 0 else "🔴 Failed"
        return f"{label} — {value[0]}"

    evidence = summary.get("independent_signal_results", {})
    gate = summary.get("promotion_gate", {})
    gate_status = "🟢 Passed — manual review required" if gate.get("passed") else "🟡 Closed"
    return "\n".join([
        "💚 **Crypto Bot Daily Health**",
        "",
        "**Connections**",
        "• Robinhood signed access: 🟢 Connected",
        "• Discord health channel: 🟢 Connected",
        "",
        "**Automation**",
        f"• Market scan: {status('scan')}",
        f"• Position monitor: {status('monitor')}",
        "",
        "**Research progress**",
        f"• Independent signals: {evidence.get('signals', 0)}",
        f"• Open shadow observations: {summary.get('open_positions', 0)}",
        f"• Promotion gate: {gate_status}",
        "",
        "🔒 **Real orders are disabled.**",
    ])


def _run_health() -> int:
    settings = None
    try:
        settings = Settings.from_environment()
        if not settings.discord_health_webhook_url:
            raise ValueError("DISCORD_HEALTH_WEBHOOK_URL is not configured")
        RobinhoodReadOnlyClient(settings).account()
        store = ShadowStore(DATA_DIR / "shadow.db")
        store.initialize()
        DiscordWebhook(settings.discord_health_webhook_url).send(
            _health_message(store.summary(), _last_runs())
        )
        _append_log("health", "exit=0; Robinhood and Discord health checks passed")
        _record_run("health", 0)
        return 0
    except Exception as error:
        _append_log("health", f"Health failure: {type(error).__name__}: {error}")
        _record_run("health", 1)
        if settings and settings.discord_health_webhook_url and not isinstance(
            error, DiscordNotificationError
        ):
            try:
                DiscordWebhook(settings.discord_health_webhook_url).send(
                    "🚨 **Crypto Bot Health Check Failed**\n\n"
                    "Robinhood connectivity or local processing did not complete.\n"
                    "Please check the redacted local log for details.\n\n"
                    "🔒 **Real orders remain disabled.**"
                )
            except Exception as notification_error:
                _append_log("health", f"Health failure alert also failed: {notification_error}")
        return 1


def _acquire_lock(mode: str, stale_seconds: int = 900) -> tuple[int, Path] | None:
    DATA_DIR.mkdir(parents=True, exist_ok=True)
    path = DATA_DIR / f".shadow-{mode}.lock"
    try:
        descriptor = os.open(path, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    except FileExistsError:
        age = datetime.now().timestamp() - path.stat().st_mtime
        if age <= stale_seconds:
            return None
        path.unlink()
        descriptor = os.open(path, os.O_CREAT | os.O_EXCL | os.O_WRONLY)
    os.write(descriptor, str(os.getpid()).encode("ascii"))
    return descriptor, path


def run(mode: str) -> int:
    lock = _acquire_lock(mode)
    if lock is None:
        _append_log(mode, "Skipped because the previous run is still active.")
        return 0
    descriptor, lock_path = lock
    if mode in {"health", "calendar"}:
        try:
            return _run_health() if mode == "health" else _run_calendar()
        finally:
            os.close(descriptor)
            lock_path.unlink(missing_ok=True)
    command = "shadow-monitor" if mode == "monitor" else "shadow-scan"
    environment = os.environ.copy()
    environment["PYTHONPATH"] = str(PROJECT_ROOT / "src")
    try:
        result = subprocess.run(
            [sys.executable, "-m", "rh_crypto_bot.cli", command],
            cwd=PROJECT_ROOT,
            env=environment,
            capture_output=True,
            text=True,
            timeout=240,
            check=False,
        )
        output = f"exit={result.returncode}\nstdout:\n{result.stdout}\nstderr:\n{result.stderr}"
        _append_log(mode, output)
        if result.returncode:
            try:
                settings = Settings.from_environment(require_credentials=False)
                if settings.discord_webhook_url:
                    DiscordWebhook(settings.discord_webhook_url).send(
                        f"🚨 **Shadow {mode.title()} Failed**\n\n"
                        "The scheduled task did not complete successfully.\n"
                        "Please check the redacted local log for details.\n\n"
                        "🔒 **No real orders were submitted.**"
                    )
            except Exception as notification_error:
                _append_log(mode, f"Discord failure alert also failed: {notification_error}")
        _record_run(mode, result.returncode)
        return result.returncode
    except Exception as error:
        _append_log(mode, f"Scheduler failure: {type(error).__name__}: {error}")
        _record_run(mode, 1)
        return 1
    finally:
        os.close(descriptor)
        lock_path.unlink(missing_ok=True)


def main() -> None:
    parser = argparse.ArgumentParser(description="Local no-model shadow task runner")
    parser.add_argument("mode", choices=("monitor", "scan", "health", "calendar"))
    args = parser.parse_args()
    raise SystemExit(run(args.mode))


if __name__ == "__main__":
    main()
