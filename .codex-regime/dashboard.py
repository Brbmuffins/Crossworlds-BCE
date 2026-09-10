from __future__ import annotations

from contextlib import closing
from datetime import UTC, datetime
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
import json
from pathlib import Path
import sqlite3
from urllib.parse import urlsplit

from .professional import EventRiskCalendar
from .shadow import ShadowStore

PROJECT_ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = PROJECT_ROOT / "data"
ASSET = Path(__file__).with_name("dashboard.html")
TRACKED = ["BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD", "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD"]


def _rows(path: Path, query: str, parameters: tuple = ()) -> list[dict[str, object]]:
    if not path.exists():
        return []
    try:
        with closing(sqlite3.connect(f"file:{path.as_posix()}?mode=ro", uri=True, timeout=1)) as connection:
            connection.row_factory = sqlite3.Row
            return [dict(row) for row in connection.execute(query, parameters)]
    except sqlite3.Error:
        return []


def dashboard_data(data_dir: Path = DATA_DIR, now: int | None = None) -> dict[str, object]:
    now = now or int(datetime.now(UTC).timestamp())
    shadow_path = data_dir / "shadow.db"
    store = ShadowStore(shadow_path)
    summary = store.summary() if shadow_path.exists() else {"promotion_gate": {}, "independent_signal_results": {}}
    tasks = _rows(data_dir / "scheduler-status.db", "SELECT mode,completed_at,returncode FROM task_runs ORDER BY mode")
    status = _rows(data_dir / "professional.db", "SELECT last_message_at,reconnects,last_error FROM stream_status WHERE id=1")
    flow = status[0] if status else {}
    flow_age = None if not flow.get("last_message_at") else max(0, now - int(flow["last_message_at"]))
    latest_flow = _rows(data_dir / "professional.db", "SELECT symbol,minute,best_bid,best_ask,spread,imbalance,buy_notional,sell_notional,trade_count FROM order_flow_minutes WHERE (symbol,minute) IN (SELECT symbol,MAX(minute) FROM order_flow_minutes GROUP BY symbol) ORDER BY symbol")
    audit = _rows(shadow_path, "SELECT recorded_at,symbol,action,detail FROM shadow_audit ORDER BY id DESC LIMIT 60")
    latest_by_symbol: dict[str, dict[str, object]] = {}
    for item in audit:
        latest_by_symbol.setdefault(str(item["symbol"]), item)
    meaningful = [item for item in audit if item["action"] != "no_trade"][:12]
    open_positions = _rows(shadow_path, "SELECT symbol,requested_notional,entry_price,target_price,stop_price,expires_at,last_mark_price,last_mark_at FROM shadow_positions WHERE status='open' ORDER BY symbol,CAST(requested_notional AS REAL)")
    events = [event for event in EventRiskCalendar(data_dir / "event-risk.json").events() if int(event["end"]) >= now][:6]
    evidence = summary.get("independent_signal_results", {})
    gate = summary.get("promotion_gate", {})
    task_map = {str(item["mode"]): item for item in tasks}
    health_run = task_map.get("health")
    return {
        "generated_at": now,
        "mode": "Shadow research only",
        "orders_enabled": False,
        "connections": {
            "robinhood": {"ok": bool(health_run and health_run["returncode"] == 0), "label": "Last signed check passed" if health_run and health_run["returncode"] == 0 else "Signed check needs attention", "checked_at": None if not health_run else health_run["completed_at"]},
            "order_flow": {"ok": flow_age is not None and flow_age <= 120 and not flow.get("last_error"), "label": "Live market stream" if flow_age is not None and flow_age <= 120 and not flow.get("last_error") else "Market stream needs attention", "age_seconds": flow_age, "reconnects": flow.get("reconnects", 0), "error": flow.get("last_error")},
            "discord": {"ok": bool(health_run and health_run["returncode"] == 0), "label": "Last notification check passed" if health_run and health_run["returncode"] == 0 else "Notification check needs attention"},
        },
        "automation": tasks,
        "performance": {
            "pnl": summary.get("total_hypothetical_profit_loss", "0.00"),
            "open": summary.get("open_positions", 0),
            "closed": summary.get("closed_positions", 0),
            "wins": summary.get("wins", 0),
            "signals": evidence.get("signals", 0),
            "average_return": evidence.get("average_net_return"),
            "profit_factor": evidence.get("profit_factor"),
            "drawdown": evidence.get("maximum_drawdown"),
        },
        "gate": {"passed": bool(gate.get("passed")), "reasons": gate.get("reasons", []), "target": 25, "signals": evidence.get("signals", 0)},
        "open_positions": open_positions,
        "market_status": [latest_by_symbol.get(symbol, {"symbol": symbol, "action": "waiting", "detail": "Waiting for the next completed scan", "recorded_at": None}) for symbol in TRACKED],
        "recent_activity": meaningful,
        "flow": latest_flow,
        "sizes": summary.get("size_comparison", []),
        "coins": summary.get("coin_results", []),
        "events": events,
    }


class DashboardHandler(BaseHTTPRequestHandler):
    def _send(self, status: int, content_type: str, body: bytes) -> None:
        self.send_response(status)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(body)))
        self.send_header("Cache-Control", "no-store")
        self.send_header("X-Content-Type-Options", "nosniff")
        self.send_header("Content-Security-Policy", "default-src 'self'; style-src 'self' 'unsafe-inline'; script-src 'self' 'unsafe-inline'; connect-src 'self'")
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self) -> None:  # noqa: N802
        path = urlsplit(self.path).path
        if path == "/api/status":
            body = json.dumps(dashboard_data(), separators=(",", ":")).encode()
            self._send(200, "application/json; charset=utf-8", body)
        elif path in {"/", "/index.html"}:
            self._send(200, "text/html; charset=utf-8", ASSET.read_bytes())
        else:
            self._send(404, "text/plain; charset=utf-8", b"Not found")

    def log_message(self, format: str, *args: object) -> None:
        return


def serve(host: str = "127.0.0.1", port: int = 8765) -> None:
    if host not in {"127.0.0.1", "localhost"}:
        raise ValueError("Dashboard may only bind to this computer")
    server = ThreadingHTTPServer((host, port), DashboardHandler)
    print(f"Crypto bot dashboard: http://{host}:{port}")
    server.serve_forever()
