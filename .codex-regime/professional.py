from __future__ import annotations

from contextlib import closing
from dataclasses import dataclass
from datetime import datetime, timezone
from decimal import Decimal
import hashlib
import json
from pathlib import Path
import random
import sqlite3


@dataclass(frozen=True)
class PortfolioRiskLimits:
    maximum_deployed: Decimal = Decimal("1200")
    maximum_daily_loss: Decimal = Decimal("0.02")
    maximum_consecutive_losses: int = 4


def portfolio_risk_check(*, open_notional: Decimal, proposed_notional: Decimal,
                         daily_return: Decimal, consecutive_losses: int,
                         limits: PortfolioRiskLimits) -> tuple[bool, list[str]]:
    reasons = []
    if open_notional + proposed_notional > limits.maximum_deployed:
        reasons.append("maximum deployed capital exceeded")
    if daily_return <= -limits.maximum_daily_loss:
        reasons.append("daily loss circuit breaker active")
    if consecutive_losses >= limits.maximum_consecutive_losses:
        reasons.append("consecutive-loss circuit breaker active")
    return not reasons, reasons


class EventRiskCalendar:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def active_reasons(self, timestamp: int, symbol: str) -> list[str]:
        reasons = []
        for event in self.events():
            symbols = set(event["symbols"])
            start, end = int(event["start"]), int(event["end"])
            if start <= timestamp <= end and (
                "ALL" in symbols or symbol.upper() in symbols
            ):
                reasons.append(str(event["reason"]))
        return reasons

    def events(self) -> list[dict[str, object]]:
        if not self.path.exists():
            return []
        payload = json.loads(self.path.read_text(encoding="utf-8"))
        parsed = []
        for event in payload.get("events", []):
            start, end = self._timestamp(event["start"]), self._timestamp(event["end"])
            if end < start:
                raise ValueError("event-risk end must not be earlier than start")
            parsed.append({"start": start, "end": end,
                           "symbols": sorted({str(v).upper() for v in event.get("symbols", ["ALL"])}),
                           "reason": str(event.get("reason", "scheduled event risk")),
                           "source": str(event.get("source", ""))})
        return sorted(parsed, key=lambda event: int(event["start"]))

    @staticmethod
    def _timestamp(value: object) -> int:
        if isinstance(value, (int, float)):
            return int(value)
        text = str(value).strip()
        if text.lstrip("-").isdigit():
            return int(text)
        parsed = datetime.fromisoformat(text.replace("Z", "+00:00"))
        if parsed.tzinfo is None:
            raise ValueError("event-risk ISO timestamps must include Z or a UTC offset")
        return int(parsed.timestamp())


def research_validation(returns: list[Decimal], *, simulations: int = 2000) -> dict[str, object]:
    if not returns:
        return {"samples": 0, "bootstrap_probability_positive": None,
                "monte_carlo_drawdown_95": None, "largest_winner_share": None}
    rng = random.Random(20260909)
    values = [float(value) for value in returns]
    bootstrap = sorted(sum(rng.choice(values) for _ in values) / len(values)
                       for _ in range(simulations))
    drawdowns = []
    for _ in range(simulations):
        shuffled = values[:]
        rng.shuffle(shuffled)
        equity = peak = 1.0
        worst = 0.0
        for value in shuffled:
            equity *= 1 + value
            peak = max(peak, equity)
            worst = max(worst, (peak - equity) / peak)
        drawdowns.append(worst)
    drawdowns.sort()
    positives = [value for value in values if value > 0]
    total_positive = sum(positives)
    return {"samples": len(values),
            "bootstrap_probability_positive": round(sum(v > 0 for v in bootstrap) / simulations, 4),
            "bootstrap_mean_interval_95": [round(bootstrap[int(simulations*.025)], 4),
                                           round(bootstrap[int(simulations*.975)], 4)],
            "monte_carlo_drawdown_95": round(drawdowns[int(simulations*.95)], 4),
            "largest_winner_share": None if not total_positive else round(max(positives)/total_positive, 4)}


class ProfessionalResearchStore:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def initialize(self) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        with closing(sqlite3.connect(self.path)) as connection:
            connection.executescript("""
              CREATE TABLE IF NOT EXISTS order_book_snapshots(id INTEGER PRIMARY KEY,recorded_at INTEGER,symbol TEXT,best_bid TEXT,best_ask TEXT,bid_depth TEXT,ask_depth TEXT,imbalance TEXT);
              CREATE TABLE IF NOT EXISTS execution_observations(id INTEGER PRIMARY KEY,recorded_at INTEGER,symbol TEXT,signal_timestamp INTEGER,quantity TEXT,ask TEXT,bid TEXT,spread TEXT,price_impact TEXT);
              CREATE TABLE IF NOT EXISTS simulated_orders(client_order_id TEXT PRIMARY KEY,symbol TEXT,side TEXT,quantity TEXT,state TEXT,updated_at INTEGER,detail TEXT);
              CREATE TABLE IF NOT EXISTS order_flow_minutes(minute INTEGER,symbol TEXT,best_bid TEXT,best_ask TEXT,spread TEXT,bid_depth TEXT,ask_depth TEXT,imbalance TEXT,buy_notional TEXT,sell_notional TEXT,trade_count INTEGER,PRIMARY KEY(minute,symbol));
              CREATE TABLE IF NOT EXISTS raw_stream_events(id INTEGER PRIMARY KEY,recorded_at INTEGER,symbol TEXT,channel TEXT,payload TEXT);
              CREATE TABLE IF NOT EXISTS stream_focus(symbol TEXT PRIMARY KEY,expires_at INTEGER NOT NULL);
              CREATE TABLE IF NOT EXISTS stream_status(id INTEGER PRIMARY KEY CHECK(id=1),last_message_at INTEGER,reconnects INTEGER NOT NULL DEFAULT 0,last_error TEXT);
              CREATE TABLE IF NOT EXISTS order_flow_metadata(key TEXT PRIMARY KEY,value TEXT NOT NULL);
            """)
            connection.commit()

    def focus(self, symbol: str, expires_at: int) -> None:
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute("INSERT INTO stream_focus(symbol,expires_at) VALUES(?,?) ON CONFLICT(symbol) DO UPDATE SET expires_at=excluded.expires_at", (symbol.upper(), expires_at))
            connection.commit()

    def focused_symbols(self, now: int) -> set[str]:
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute("DELETE FROM stream_focus WHERE expires_at<?", (now,))
            rows = connection.execute("SELECT symbol FROM stream_focus").fetchall()
            connection.commit()
        return {str(row[0]) for row in rows}

    def flow_health(self, symbol: str, now: int, maximum_age: int = 120) -> tuple[bool, str]:
        with closing(sqlite3.connect(self.path)) as connection:
            row = connection.execute("SELECT MAX(minute) FROM order_flow_minutes WHERE symbol=?", (symbol.upper(),)).fetchone()
            status = connection.execute("SELECT last_message_at,last_error FROM stream_status WHERE id=1").fetchone()
        if not row or row[0] is None or not status or status[0] is None:
            return False, "continuous order-flow data unavailable"
        if status[1]:
            return False, f"continuous order-flow recovery active: {status[1]}"
        age = now - int(row[0])
        connection_age = now - int(status[0])
        if age > maximum_age or connection_age > maximum_age:
            return False, f"continuous order-flow data stale by {max(age, connection_age)} seconds"
        return True, "fresh"

    def record_book(self, symbol: str, book: dict[str, Decimal]) -> None:
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute("INSERT INTO order_book_snapshots(recorded_at,symbol,best_bid,best_ask,bid_depth,ask_depth,imbalance) VALUES(?,?,?,?,?,?,?)",
                (int(datetime.now(timezone.utc).timestamp()),symbol.upper(),*(str(book[k]) for k in ("best_bid","best_ask","bid_depth","ask_depth","imbalance"))))
            connection.commit()

    def record_execution(self, symbol: str, signal_timestamp: int, quantity: Decimal,
                         ask: Decimal, bid: Decimal, impact: Decimal) -> None:
        spread = (ask-bid)/((ask+bid)/2)
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute("INSERT INTO execution_observations(recorded_at,symbol,signal_timestamp,quantity,ask,bid,spread,price_impact) VALUES(?,?,?,?,?,?,?,?)",
                (int(datetime.now(timezone.utc).timestamp()),symbol.upper(),signal_timestamp,str(quantity),str(ask),str(bid),str(spread),str(impact)))
            connection.commit()

    def simulate_order(self, symbol: str, signal_timestamp: int, quantity: Decimal,
                       state: str, detail: str) -> str:
        if state not in {"proposed","accepted","partial_fill","filled","rejected","cancelled"}:
            raise ValueError("invalid simulated order state")
        client_id = hashlib.sha256(f"{symbol.upper()}:{signal_timestamp}:{quantity}:buy".encode()).hexdigest()[:32]
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute("INSERT INTO simulated_orders(client_order_id,symbol,side,quantity,state,updated_at,detail) VALUES(?,?,?,?,?,?,?) ON CONFLICT(client_order_id) DO UPDATE SET state=excluded.state,updated_at=excluded.updated_at,detail=excluded.detail",
                (client_id,symbol.upper(),"buy",str(quantity),state,int(datetime.now(timezone.utc).timestamp()),detail))
            connection.commit()
        return client_id
