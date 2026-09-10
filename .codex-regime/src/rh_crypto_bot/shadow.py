from __future__ import annotations

from contextlib import closing
from dataclasses import dataclass
from datetime import datetime, timezone
from decimal import Decimal, InvalidOperation, ROUND_DOWN
from pathlib import Path
import sqlite3
import math
from typing import Any

from .costs import RoundTripCosts, net_return, required_exit_price


@dataclass(frozen=True)
class ShadowPosition:
    id: int
    symbol: str
    signal_timestamp: int
    opened_at: int
    entry_price: Decimal
    quantity: Decimal
    requested_notional: Decimal
    notional: Decimal
    target_price: Decimal
    stop_price: Decimal
    expires_at: int
    fee_rate: Decimal
    rationale: str
    last_mark_price: Decimal | None = None
    last_mark_at: int | None = None


def _rows(payload: Any) -> list[dict[str, Any]]:
    if isinstance(payload, dict):
        results = payload.get("results")
        if isinstance(results, list):
            return [row for row in results if isinstance(row, dict)]
        return [payload]
    return []


def execution_price(payload: Any, side: str) -> Decimal:
    """Extract Robinhood's quantity-specific, spread-inclusive execution estimate."""
    keys = {
        "ask": ("ask_inclusive_of_buy_spread", "ask_price", "ask"),
        "bid": ("bid_inclusive_of_sell_spread", "bid_price", "bid"),
    }
    if side not in keys:
        raise ValueError("side must be ask or bid")
    for row in _rows(payload):
        for key in keys[side]:
            raw = row.get(key)
            try:
                price = Decimal(str(raw))
            except (InvalidOperation, TypeError):
                continue
            if price > 0:
                return price
    raise ValueError(f"Robinhood estimate omitted a valid {side} price")


def quantity_for_notional(notional: Decimal, reference_ask: Decimal) -> Decimal:
    if notional <= 0 or reference_ask <= 0:
        raise ValueError("notional and reference ask must be positive")
    quantity = (notional / reference_ask).quantize(Decimal("0.00000001"), rounding=ROUND_DOWN)
    if quantity <= 0:
        raise ValueError("notional is too small for an eight-decimal quantity")
    return quantity


def performance_statistics(returns: list[Decimal]) -> tuple[Decimal | None, Decimal]:
    """Return profit factor and compounded-equity maximum drawdown."""
    gains = sum((value for value in returns if value > 0), Decimal("0"))
    losses = -sum((value for value in returns if value < 0), Decimal("0"))
    profit_factor = None if losses == 0 and gains > 0 else (
        Decimal("0") if losses == 0 else gains / losses
    )
    equity = Decimal("1")
    peak = equity
    maximum_drawdown = Decimal("0")
    for value in returns:
        equity *= Decimal("1") + value
        peak = max(peak, equity)
        if peak > 0:
            maximum_drawdown = max(maximum_drawdown, (peak - equity) / peak)
    return profit_factor, maximum_drawdown


class ShadowStore:
    def __init__(self, path: str | Path) -> None:
        self.path = Path(path)

    def initialize(self) -> None:
        self.path.parent.mkdir(parents=True, exist_ok=True)
        with closing(sqlite3.connect(self.path)) as connection:
            legacy_sql = connection.execute(
                "SELECT sql FROM sqlite_master WHERE type='table' AND name='shadow_positions'"
            ).fetchone()
            if legacy_sql and "requested_notional" not in legacy_sql[0]:
                connection.execute("ALTER TABLE shadow_positions RENAME TO shadow_positions_legacy")
            connection.executescript(
                """
                CREATE TABLE IF NOT EXISTS shadow_positions (
                    id INTEGER PRIMARY KEY,
                    symbol TEXT NOT NULL,
                    signal_timestamp INTEGER NOT NULL,
                    opened_at INTEGER NOT NULL,
                    entry_price TEXT NOT NULL,
                    quantity TEXT NOT NULL,
                    requested_notional TEXT NOT NULL,
                    notional TEXT NOT NULL,
                    target_price TEXT NOT NULL,
                    stop_price TEXT NOT NULL,
                    expires_at INTEGER NOT NULL,
                    fee_rate TEXT NOT NULL,
                    rationale TEXT NOT NULL,
                    status TEXT NOT NULL DEFAULT 'open',
                    closed_at INTEGER,
                    exit_price TEXT,
                    net_return TEXT,
                    profit_loss TEXT,
                    exit_reason TEXT,
                    last_mark_price TEXT,
                    last_mark_at INTEGER,
                    btc_benchmark_return TEXT,
                    market_benchmark_return TEXT,
                    UNIQUE(symbol, signal_timestamp, requested_notional)
                );
                CREATE TABLE IF NOT EXISTS shadow_audit (
                    id INTEGER PRIMARY KEY,
                    recorded_at INTEGER NOT NULL,
                    symbol TEXT NOT NULL,
                    action TEXT NOT NULL,
                    detail TEXT NOT NULL
                );
                CREATE TABLE IF NOT EXISTS shadow_notifications (
                    notification_key TEXT PRIMARY KEY,
                    sent_at INTEGER NOT NULL
                );
                """
            )
            if legacy_sql and "requested_notional" not in legacy_sql[0]:
                connection.execute(
                    """
                    INSERT INTO shadow_positions
                        (id, symbol, signal_timestamp, opened_at, entry_price, quantity,
                         requested_notional, notional, target_price, stop_price, expires_at,
                         fee_rate, rationale, status, closed_at, exit_price, net_return,
                         profit_loss, exit_reason)
                    SELECT id, symbol, signal_timestamp, opened_at, entry_price, quantity,
                           notional, notional, target_price, stop_price, expires_at,
                           fee_rate, rationale, status, closed_at, exit_price, net_return,
                           profit_loss, exit_reason
                    FROM shadow_positions_legacy
                    """
                )
                connection.execute("DROP TABLE shadow_positions_legacy")
            columns = {
                row[1] for row in connection.execute("PRAGMA table_info(shadow_positions)")
            }
            for name, kind in (
                ("last_mark_price", "TEXT"),
                ("last_mark_at", "INTEGER"),
                ("btc_benchmark_return", "TEXT"),
                ("market_benchmark_return", "TEXT"),
            ):
                if name not in columns:
                    connection.execute(f"ALTER TABLE shadow_positions ADD COLUMN {name} {kind}")
            connection.commit()

    def audit(self, symbol: str, action: str, detail: str) -> None:
        now = int(datetime.now(timezone.utc).timestamp())
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute(
                "INSERT INTO shadow_audit(recorded_at, symbol, action, detail) VALUES (?, ?, ?, ?)",
                (now, symbol.upper(), action, detail),
            )
            connection.commit()

    def notification_sent(self, notification_key: str) -> bool:
        with closing(sqlite3.connect(self.path)) as connection:
            row = connection.execute(
                "SELECT 1 FROM shadow_notifications WHERE notification_key=?",
                (notification_key,),
            ).fetchone()
        return row is not None

    def independent_returns(self) -> list[Decimal]:
        with closing(sqlite3.connect(self.path)) as connection:
            rows = connection.execute("""SELECT symbol,signal_timestamp,requested_notional,net_return
                FROM shadow_positions WHERE status='closed' ORDER BY closed_at,id""").fetchall()
        selected = {}
        for row in rows:
            key = (row[0], int(row[1]))
            if key not in selected or Decimal(row[2]) < Decimal(selected[key][2]):
                selected[key] = row
        return [Decimal(row[3]) for row in selected.values()]

    def portfolio_risk_state(self) -> tuple[Decimal, Decimal, int]:
        today = int(datetime.now(timezone.utc).replace(hour=0, minute=0, second=0, microsecond=0).timestamp())
        with closing(sqlite3.connect(self.path)) as connection:
            open_notional = Decimal(str(connection.execute("""SELECT COALESCE(SUM(value),0)
                FROM (SELECT MAX(CAST(notional AS REAL)) AS value FROM shadow_positions
                      WHERE status='open' GROUP BY symbol,signal_timestamp)""").fetchone()[0]))
            rows = connection.execute("""SELECT symbol,signal_timestamp,requested_notional,net_return,closed_at
                FROM shadow_positions WHERE status='closed' ORDER BY closed_at DESC,id DESC""").fetchall()
        selected = {}
        for row in rows:
            key = (row[0], int(row[1]))
            if key not in selected or Decimal(row[2]) < Decimal(selected[key][2]):
                selected[key] = row
        independent = sorted(selected.values(), key=lambda row: int(row[4] or 0), reverse=True)
        daily = [Decimal(row[3]) for row in independent if int(row[4] or 0) >= today]
        daily_return = Decimal("1")
        for value in daily:
            daily_return *= Decimal("1") + value
        losses = 0
        for row in independent:
            if Decimal(row[3]) >= 0:
                break
            losses += 1
        return open_notional, daily_return - 1, losses

    def mark_notification_sent(self, notification_key: str) -> None:
        now = int(datetime.now(timezone.utc).timestamp())
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute(
                "INSERT OR IGNORE INTO shadow_notifications(notification_key, sent_at) VALUES (?, ?)",
                (notification_key, now),
            )
            connection.commit()

    def open_position(
        self,
        *,
        symbol: str,
        signal_timestamp: int,
        entry_price: Decimal,
        quantity: Decimal,
        requested_notional: Decimal,
        target_price: Decimal,
        stop_price: Decimal,
        expires_at: int,
        fee_rate: Decimal,
        rationale: str,
    ) -> bool:
        now = int(datetime.now(timezone.utc).timestamp())
        notional = entry_price * quantity
        with closing(sqlite3.connect(self.path)) as connection:
            cursor = connection.execute(
                """
                INSERT OR IGNORE INTO shadow_positions
                    (symbol, signal_timestamp, opened_at, entry_price, quantity,
                     requested_notional, notional,
                     target_price, stop_price, expires_at, fee_rate, rationale)
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
                """,
                (
                    symbol.upper(), signal_timestamp, now, str(entry_price), str(quantity),
                    str(requested_notional), str(notional), str(target_price), str(stop_price), expires_at,
                    str(fee_rate), rationale,
                ),
            )
            connection.commit()
            return cursor.rowcount == 1

    def open_positions(self) -> list[ShadowPosition]:
        with closing(sqlite3.connect(self.path)) as connection:
            rows = connection.execute(
                """
                SELECT id, symbol, signal_timestamp, opened_at, entry_price, quantity,
                       requested_notional, notional, target_price, stop_price, expires_at,
                       fee_rate, rationale, last_mark_price, last_mark_at
                FROM shadow_positions WHERE status = 'open' ORDER BY id
                """
            ).fetchall()
        return [
            ShadowPosition(
                int(row[0]), row[1], int(row[2]), int(row[3]), Decimal(row[4]),
                Decimal(row[5]), Decimal(row[6]), Decimal(row[7]), Decimal(row[8]),
                Decimal(row[9]), int(row[10]), Decimal(row[11]), row[12],
                None if row[13] is None else Decimal(row[13]),
                None if row[14] is None else int(row[14]),
            )
            for row in rows
        ]

    def mark_position(self, position_id: int, price: Decimal, marked_at: int) -> None:
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute(
                """UPDATE shadow_positions SET last_mark_price=?, last_mark_at=?
                   WHERE id=? AND status='open'""",
                (str(price), marked_at, position_id),
            )
            connection.commit()

    def signal_allowed(self, symbol: str, signal_timestamp: int, cooldown_seconds: int) -> bool:
        with closing(sqlite3.connect(self.path)) as connection:
            latest = connection.execute(
                "SELECT MAX(signal_timestamp) FROM shadow_positions WHERE symbol=?",
                (symbol.upper(),),
            ).fetchone()[0]
            open_count = connection.execute(
                "SELECT COUNT(*) FROM shadow_positions WHERE symbol=? AND status='open'",
                (symbol.upper(),),
            ).fetchone()[0]
        return not open_count and (latest is None or signal_timestamp - int(latest) >= cooldown_seconds)

    def close_position(
        self,
        position: ShadowPosition,
        exit_price: Decimal,
        reason: str,
        *,
        btc_benchmark_return: Decimal | None = None,
        market_benchmark_return: Decimal | None = None,
    ) -> None:
        costs = RoundTripCosts(position.fee_rate, position.fee_rate)
        result = net_return(position.entry_price, exit_price, costs)
        now = int(datetime.now(timezone.utc).timestamp())
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute(
                """
                UPDATE shadow_positions
                SET status='closed', closed_at=?, exit_price=?, net_return=?,
                    profit_loss=?, exit_reason=?, last_mark_price=?, last_mark_at=?,
                    btc_benchmark_return=?, market_benchmark_return=?
                WHERE id=? AND status='open'
                """,
                (
                    now, str(exit_price), str(result), str(position.notional * result), reason,
                    str(exit_price), now,
                    None if btc_benchmark_return is None else str(btc_benchmark_return),
                    None if market_benchmark_return is None else str(market_benchmark_return),
                    position.id,
                ),
            )
            connection.commit()

    def positions_missing_benchmarks(self) -> list[tuple[int, int, int, Decimal]]:
        with closing(sqlite3.connect(self.path)) as connection:
            rows = connection.execute(
                """SELECT id, opened_at, closed_at, fee_rate FROM shadow_positions
                   WHERE status='closed' AND closed_at IS NOT NULL
                     AND (btc_benchmark_return IS NULL OR market_benchmark_return IS NULL)"""
            ).fetchall()
        return [(int(row[0]), int(row[1]), int(row[2]), Decimal(row[3])) for row in rows]

    def set_benchmarks(self, position_id: int, btc_return: Decimal, market_return: Decimal) -> None:
        with closing(sqlite3.connect(self.path)) as connection:
            connection.execute(
                """UPDATE shadow_positions
                   SET btc_benchmark_return=?, market_benchmark_return=? WHERE id=?""",
                (str(btc_return), str(market_return), position_id),
            )
            connection.commit()

    def summary(self) -> dict[str, object]:
        with closing(sqlite3.connect(self.path)) as connection:
            open_count = connection.execute(
                "SELECT COUNT(*) FROM shadow_positions WHERE status='open'"
            ).fetchone()[0]
            rows = connection.execute(
                """SELECT requested_notional, entry_price, exit_price, net_return, profit_loss,
                          symbol, signal_timestamp, btc_benchmark_return, market_benchmark_return,
                          closed_at
                   FROM shadow_positions WHERE status='closed'"""
            ).fetchall()
            open_rows = connection.execute(
                """SELECT requested_notional, entry_price, notional, fee_rate, symbol,
                          signal_timestamp, last_mark_price, last_mark_at
                   FROM shadow_positions WHERE status='open'"""
            ).fetchall()
        returns = [Decimal(row[3]) for row in rows]
        pnl = sum((Decimal(row[4]) for row in rows), Decimal("0"))
        comparisons = []
        for size in sorted({Decimal(row[0]) for row in rows}):
            sized = [row for row in rows if Decimal(row[0]) == size]
            size_returns = [Decimal(row[3]) for row in sized]
            size_pnl = sum((Decimal(row[4]) for row in sized), Decimal("0"))
            comparisons.append({
                "requested_notional": str(size),
                "closed_positions": len(sized),
                "wins": sum(value > 0 for value in size_returns),
                "average_entry_price": str(
                    (sum(Decimal(row[1]) for row in sized) / len(sized)).quantize(Decimal("0.00000001"))
                ),
                "average_exit_price": str(
                    (sum(Decimal(row[2]) for row in sized) / len(sized)).quantize(Decimal("0.00000001"))
                ),
                "average_net_return": str(
                    (sum(size_returns) / len(size_returns)).quantize(Decimal("0.0001"))
                ),
                "total_hypothetical_profit_loss": str(size_pnl.quantize(Decimal("0.01"))),
            })
        signal_rows = {}
        for row in rows:
            key = (row[5], int(row[6]))
            if key not in signal_rows or Decimal(row[0]) < Decimal(signal_rows[key][0]):
                signal_rows[key] = row
        independent = list(signal_rows.values())
        independent_returns = [Decimal(row[3]) for row in independent]
        chronological_returns = [
            Decimal(row[3]) for row in sorted(independent, key=lambda item: int(item[9] or 0))
        ]
        profit_factor, maximum_drawdown = performance_statistics(chronological_returns)
        coin_results = []
        for symbol in sorted({row[5] for row in independent}):
            coin = [row for row in independent if row[5] == symbol]
            coin_returns = [Decimal(row[3]) for row in coin]
            coin_results.append({
                "symbol": symbol,
                "independent_signals": len(coin),
                "wins": sum(value > 0 for value in coin_returns),
                "average_net_return": str((sum(coin_returns) / len(coin_returns)).quantize(Decimal("0.0001"))),
                "total_baseline_profit_loss": str(sum((Decimal(row[4]) for row in coin), Decimal("0")).quantize(Decimal("0.01"))),
            })
        count = len(independent_returns)
        mean = sum(independent_returns, Decimal("0")) / count if count else Decimal("0")
        if count > 1:
            variance = sum((float(value - mean) ** 2 for value in independent_returns)) / (count - 1)
            margin = Decimal(str(1.96 * math.sqrt(variance / count)))
            confidence = (mean - margin, mean + margin)
        else:
            confidence = None
        btc_values = [Decimal(row[7]) for row in independent if row[7] is not None]
        market_values = [Decimal(row[8]) for row in independent if row[8] is not None]
        reasons = []
        if count < 25:
            reasons.append(f"need at least 25 independent signals; have {count}")
        if mean < Decimal("0.0025"):
            reasons.append("average net return is below 0.25% per independent signal")
        if profit_factor is not None and profit_factor < Decimal("1.25"):
            reasons.append("profit factor is below 1.25")
        if maximum_drawdown > Decimal("0.15"):
            reasons.append("maximum drawdown exceeds 15%")
        if confidence is None or confidence[0] <= 0:
            reasons.append("95% confidence lower bound is not positive")
        if not btc_values or mean <= sum(btc_values) / len(btc_values):
            reasons.append("has not outperformed fee-adjusted BTC buy-and-hold")
        if not market_values or mean <= sum(market_values) / len(market_values):
            reasons.append("has not outperformed fee-adjusted equal-weight market")
        unrealized = []
        for row in open_rows:
            if row[6] is None:
                continue
            value = net_return(
                Decimal(row[1]), Decimal(row[6]),
                RoundTripCosts(Decimal(row[3]), Decimal(row[3])),
            )
            unrealized.append({
                "symbol": row[4], "signal_timestamp": int(row[5]),
                "requested_notional": str(Decimal(row[0])),
                "net_return": str(value.quantize(Decimal("0.0001"))),
                "hypothetical_profit_loss": str((Decimal(row[2]) * value).quantize(Decimal("0.01"))),
                "marked_at": int(row[7]),
            })
        return {
            "mode": "experimental_shadow_only",
            "open_positions": int(open_count),
            "closed_positions": len(rows),
            "wins": sum(value > 0 for value in returns),
            "total_hypothetical_profit_loss": str(pnl.quantize(Decimal("0.01"))),
            "average_net_return": (
                None if not returns else str((sum(returns) / len(returns)).quantize(Decimal("0.0001")))
            ),
            "size_comparison": comparisons,
            "independent_signal_results": {
                "signals": count,
                "wins": sum(value > 0 for value in independent_returns),
                "average_net_return": None if not count else str(mean.quantize(Decimal("0.0001"))),
                "confidence_interval_95": None if confidence is None else [
                    str(confidence[0].quantize(Decimal("0.0001"))),
                    str(confidence[1].quantize(Decimal("0.0001"))),
                ],
                "cash_benchmark_return": "0",
                "average_btc_benchmark_return": None if not btc_values else str((sum(btc_values) / len(btc_values)).quantize(Decimal("0.0001"))),
                "average_market_benchmark_return": None if not market_values else str((sum(market_values) / len(market_values)).quantize(Decimal("0.0001"))),
                "profit_factor": "infinite" if profit_factor is None else str(profit_factor.quantize(Decimal("0.01"))),
                "maximum_drawdown": str(maximum_drawdown.quantize(Decimal("0.0001"))),
                "minimum_average_net_return": "0.0025",
                "minimum_profit_factor": "1.25",
                "maximum_allowed_drawdown": "0.15",
                "preferred_signals_for_automation_review": 50,
                "automation_sample_ready": count >= 50,
            },
            "coin_results": coin_results,
            "open_unrealized": unrealized,
            "promotion_gate": {"passed": not reasons, "recommendations_enabled": False, "reasons": reasons},
        }


def shadow_levels(entry: Decimal, target: Decimal, stop: Decimal, fee: Decimal) -> tuple[Decimal, Decimal]:
    costs = RoundTripCosts(fee, fee)
    return (
        required_exit_price(entry, target, costs),
        required_exit_price(entry, -stop, costs),
    )
