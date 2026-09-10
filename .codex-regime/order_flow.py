from __future__ import annotations

import argparse
import asyncio
import json
import sqlite3
from collections import defaultdict
from contextlib import closing
from datetime import UTC, datetime
from decimal import Decimal
from pathlib import Path

import websockets

from .professional import ProfessionalResearchStore

DEFAULT_SYMBOLS = [
    "BTC-USD",
    "ETH-USD",
    "XRP-USD",
    "ZEC-USD",
    "SOL-USD",
    "LINK-USD",
    "ADA-USD",
    "DOGE-USD",
    "UNI-USD",
    "SUI-USD",
    "XLM-USD",
]
ENDPOINT = "wss://advanced-trade-ws.coinbase.com"
FLOW_FEATURE_VERSION = "2-aggressor-side-and-sequence-guard"


class SequenceGapError(RuntimeError):
    """Local stream state is unsafe and must be rebuilt from snapshots."""


class SequenceGuard:
    def __init__(self) -> None:
        self._previous: int | None = None

    def accept(self, sequence: object) -> bool:
        try:
            current = int(sequence)
        except (TypeError, ValueError) as error:
            raise SequenceGapError("missing or invalid feed sequence") from error
        if self._previous is None:
            self._previous = current
            return True
        if current <= self._previous:
            return False
        if current != self._previous + 1:
            raise SequenceGapError(
                f"feed sequence gap; expected {self._previous + 1}, received {current}"
            )
        self._previous = current
        return True


class MinuteFlow:
    def __init__(self, symbols: list[str]) -> None:
        self.books = {symbol: {"bid": {}, "ask": {}} for symbol in symbols}
        self.ready: set[str] = set()
        self.trades = defaultdict(lambda: {"buy": Decimal(0), "sell": Decimal(0), "count": 0})

    def update_book(self, symbol: str, updates: list[dict], *, snapshot: bool = False) -> None:
        if symbol not in self.books:
            return
        if snapshot:
            self.books[symbol] = {"bid": {}, "ask": {}}
            self.ready.discard(symbol)
        elif symbol not in self.ready:
            return
        for update in updates:
            side = str(update.get("side", "")).lower()
            side = "ask" if side in {"offer", "ask"} else side
            if side not in {"bid", "ask"}:
                continue
            price = Decimal(str(update["price_level"]))
            quantity = Decimal(str(update["new_quantity"]))
            if quantity == 0:
                self.books[symbol][side].pop(price, None)
            else:
                self.books[symbol][side][price] = quantity
        if snapshot:
            self.ready.add(symbol)

    def update_trades(self, symbol: str, trades: list[dict]) -> None:
        if symbol not in self.books:
            return
        for trade in trades:
            maker_side = str(trade.get("side", "")).upper()
            if maker_side not in {"BUY", "SELL"}:
                continue
            aggressor = "buy" if maker_side == "SELL" else "sell"
            notional = Decimal(str(trade["price"])) * Decimal(str(trade["size"]))
            self.trades[symbol][aggressor] += notional
            self.trades[symbol]["count"] += 1

    def rows(self, minute: int) -> list[tuple]:
        rows = []
        for symbol, sides in self.books.items():
            if symbol not in self.ready or not sides["bid"] or not sides["ask"]:
                continue
            bids = sorted(sides["bid"].items(), reverse=True)[:25]
            asks = sorted(sides["ask"].items())[:25]
            best_bid, best_ask = bids[0][0], asks[0][0]
            bid_depth = sum((price * quantity for price, quantity in bids), Decimal(0))
            ask_depth = sum((price * quantity for price, quantity in asks), Decimal(0))
            total = bid_depth + ask_depth
            trade = self.trades[symbol]
            rows.append(
                (
                    minute,
                    symbol,
                    str(best_bid),
                    str(best_ask),
                    str((best_ask - best_bid) / ((best_ask + best_bid) / 2)),
                    str(bid_depth),
                    str(ask_depth),
                    str((bid_depth - ask_depth) / total),
                    str(trade["buy"]),
                    str(trade["sell"]),
                    int(trade["count"]),
                )
            )
        self.trades.clear()
        return rows


def ensure_feature_version(path: Path) -> bool:
    """Discard incompatible derived minutes once when their definition changes."""
    with closing(sqlite3.connect(path)) as connection:
        connection.execute(
            "CREATE TABLE IF NOT EXISTS order_flow_metadata(key TEXT PRIMARY KEY,value TEXT NOT NULL)"
        )
        row = connection.execute(
            "SELECT value FROM order_flow_metadata WHERE key='feature_version'"
        ).fetchone()
        changed = row is None or str(row[0]) != FLOW_FEATURE_VERSION
        if changed:
            connection.execute("DELETE FROM order_flow_minutes")
            connection.execute(
                "INSERT INTO order_flow_metadata(key,value) VALUES('feature_version',?) ON CONFLICT(key) DO UPDATE SET value=excluded.value",
                (FLOW_FEATURE_VERSION,),
            )
            connection.commit()
        return changed


def persist_minute(
    path: Path, rows: list[tuple], last_message: int, reconnects: int, error: str | None = None
) -> None:
    with closing(sqlite3.connect(path)) as connection:
        connection.executemany(
            "INSERT OR REPLACE INTO order_flow_minutes VALUES(?,?,?,?,?,?,?,?,?,?,?)", rows
        )
        connection.execute(
            "INSERT INTO stream_status(id,last_message_at,reconnects,last_error) VALUES(1,?,?,?) ON CONFLICT(id) DO UPDATE SET last_message_at=excluded.last_message_at,reconnects=excluded.reconnects,last_error=excluded.last_error",
            (last_message, reconnects, error),
        )
        connection.execute(
            "DELETE FROM raw_stream_events WHERE recorded_at<?", (last_message - 72 * 3600,)
        )
        connection.commit()


def process_payload(flow: MinuteFlow, guard: SequenceGuard, payload: dict) -> None:
    if not guard.accept(payload.get("sequence_num")):
        return
    channel = str(payload.get("channel", ""))
    if channel not in {"l2_data", "market_trades"}:
        return
    by_symbol: dict[str, list[dict]] = defaultdict(list)
    for event in payload.get("events", []):
        symbol = str(event.get("product_id", ""))
        if symbol:
            by_symbol[symbol].append(event)
    for symbol, events in by_symbol.items():
        for event in events:
            event_snapshot = str(event.get("type", "")).lower() == "snapshot"
            if channel == "l2_data":
                flow.update_book(symbol, event.get("updates", []), snapshot=event_snapshot)
            else:
                flow.update_trades(symbol, event.get("trades", []))


async def collect(database: Path, symbols: list[str]) -> None:
    store = ProfessionalResearchStore(database)
    store.initialize()
    ensure_feature_version(database)
    reconnects = 0
    focus_checked = 0
    focused: set[str] = set()
    while True:
        try:
            flow, guard, last_minute = MinuteFlow(symbols), SequenceGuard(), 0
            async with websockets.connect(
                ENDPOINT,
                ping_interval=20,
                ping_timeout=20,
                max_queue=4096,
                max_size=8 * 1024 * 1024,
            ) as socket:
                for channel in ("level2", "market_trades", "heartbeats"):
                    await socket.send(
                        json.dumps(
                            {"type": "subscribe", "product_ids": symbols, "channel": channel}
                        )
                    )
                async for raw in socket:
                    now = int(datetime.now(UTC).timestamp())
                    payload = json.loads(raw)
                    if now - focus_checked >= 30:
                        focused, focus_checked = store.focused_symbols(now), now
                    process_payload(flow, guard, payload)
                    channel = str(payload.get("channel", ""))
                    for event in payload.get("events", []):
                        symbol = str(event.get("product_id", ""))
                        if symbol in focused:
                            with closing(sqlite3.connect(database)) as connection:
                                connection.execute(
                                    "INSERT INTO raw_stream_events(recorded_at,symbol,channel,payload) VALUES(?,?,?,?)",
                                    (now, symbol, channel, raw[:10000]),
                                )
                                connection.commit()
                    minute = now // 60 * 60
                    if last_minute and minute > last_minute:
                        persist_minute(database, flow.rows(last_minute), now, reconnects)
                    last_minute = minute
        except asyncio.CancelledError:
            raise
        # A malformed payload, database fault, or transport failure can all make the
        # stateful book unsafe. Fail closed and rebuild from fresh snapshots.
        except Exception as error:  # noqa: BLE001
            reconnects += 1
            persist_minute(
                database,
                [],
                int(datetime.now(UTC).timestamp()),
                reconnects,
                f"{type(error).__name__}: {error}"[:500],
            )
            await asyncio.sleep(min(60, 2 ** min(reconnects, 5)))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--database", default="data/professional.db")
    parser.add_argument("--symbols", nargs="+", default=DEFAULT_SYMBOLS)
    args = parser.parse_args()
    asyncio.run(collect(Path(args.database), [value.upper() for value in args.symbols]))


if __name__ == "__main__":
    main()
