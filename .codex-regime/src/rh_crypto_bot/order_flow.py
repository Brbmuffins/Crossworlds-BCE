from __future__ import annotations

import argparse
import asyncio
from collections import defaultdict
from datetime import datetime, timezone
from decimal import Decimal
import json
from pathlib import Path
import sqlite3
from contextlib import closing

import websockets

from .professional import ProfessionalResearchStore

DEFAULT_SYMBOLS = ["BTC-USD","ETH-USD","XRP-USD","ZEC-USD","SOL-USD","LINK-USD","ADA-USD","DOGE-USD","UNI-USD","SUI-USD","XLM-USD"]
ENDPOINT = "wss://advanced-trade-ws.coinbase.com"


class MinuteFlow:
    def __init__(self, symbols: list[str]) -> None:
        self.books = {symbol: {"bid": {}, "ask": {}} for symbol in symbols}
        self.trades = defaultdict(lambda: {"buy": Decimal("0"), "sell": Decimal("0"), "count": 0})

    def update_book(self, symbol: str, updates: list[dict]) -> None:
        if symbol not in self.books:
            return
        for update in updates:
            side = str(update.get("side", "")).lower()
            side = "ask" if side in {"offer", "ask"} else side
            if side not in {"bid", "ask"}:
                continue
            price, quantity = Decimal(str(update["price_level"])), Decimal(str(update["new_quantity"]))
            if quantity == 0:
                self.books[symbol][side].pop(price, None)
            else:
                self.books[symbol][side][price] = quantity

    def update_trades(self, symbol: str, trades: list[dict]) -> None:
        if symbol not in self.books:
            return
        for trade in trades:
            side = str(trade.get("side", "sell")).lower()
            key = "buy" if side == "buy" else "sell"
            self.trades[symbol][key] += Decimal(str(trade["price"])) * Decimal(str(trade["size"]))
            self.trades[symbol]["count"] += 1

    def rows(self, minute: int) -> list[tuple]:
        rows = []
        for symbol, sides in self.books.items():
            if not sides["bid"] or not sides["ask"]:
                continue
            bids = sorted(sides["bid"].items(), reverse=True)[:25]
            asks = sorted(sides["ask"].items())[:25]
            best_bid, best_ask = bids[0][0], asks[0][0]
            bid_depth = sum((p*q for p,q in bids), Decimal("0")); ask_depth = sum((p*q for p,q in asks), Decimal("0"))
            total = bid_depth + ask_depth
            trade = self.trades[symbol]
            rows.append((minute,symbol,str(best_bid),str(best_ask),str((best_ask-best_bid)/((best_ask+best_bid)/2)),str(bid_depth),str(ask_depth),str((bid_depth-ask_depth)/total),str(trade["buy"]),str(trade["sell"]),int(trade["count"])))
        self.trades.clear()
        return rows


def persist_minute(path: Path, rows: list[tuple], last_message: int, reconnects: int, error: str | None = None) -> None:
    with closing(sqlite3.connect(path)) as connection:
        connection.executemany("INSERT OR REPLACE INTO order_flow_minutes VALUES(?,?,?,?,?,?,?,?,?,?,?)", rows)
        connection.execute("INSERT INTO stream_status(id,last_message_at,reconnects,last_error) VALUES(1,?,?,?) ON CONFLICT(id) DO UPDATE SET last_message_at=excluded.last_message_at,reconnects=excluded.reconnects,last_error=excluded.last_error", (last_message,reconnects,error))
        connection.execute("DELETE FROM raw_stream_events WHERE recorded_at<?", (last_message-72*3600,))
        connection.commit()


async def collect(database: Path, symbols: list[str]) -> None:
    store = ProfessionalResearchStore(database); store.initialize()
    flow = MinuteFlow(symbols); reconnects = 0; last_minute = 0; focus_checked = 0; focused = set()
    while True:
        try:
            async with websockets.connect(ENDPOINT, ping_interval=20, ping_timeout=20,
                                          max_queue=4096, max_size=8 * 1024 * 1024) as socket:
                for channel in ("level2", "market_trades", "heartbeats"):
                    await socket.send(json.dumps({"type":"subscribe","product_ids":symbols,"channel":channel}))
                async for raw in socket:
                    now = int(datetime.now(timezone.utc).timestamp()); payload = json.loads(raw)
                    if now - focus_checked >= 30:
                        focused = store.focused_symbols(now); focus_checked = now
                    channel = str(payload.get("channel", ""))
                    for event in payload.get("events", []):
                        symbol = str(event.get("product_id", ""))
                        if channel == "l2_data": flow.update_book(symbol, event.get("updates", []))
                        elif channel == "market_trades": flow.update_trades(symbol, event.get("trades", []))
                        if symbol in focused:
                            with closing(sqlite3.connect(database)) as connection:
                                connection.execute("INSERT INTO raw_stream_events(recorded_at,symbol,channel,payload) VALUES(?,?,?,?)", (now,symbol,channel,raw[:10000])); connection.commit()
                    minute = now // 60 * 60
                    if last_minute and minute > last_minute:
                        persist_minute(database, flow.rows(last_minute), now, reconnects)
                    last_minute = minute
        except asyncio.CancelledError:
            raise
        except Exception as error:
            reconnects += 1
            persist_minute(database, [], int(datetime.now(timezone.utc).timestamp()), reconnects, f"{type(error).__name__}: {error}"[:500])
            await asyncio.sleep(min(60, 2 ** min(reconnects, 5)))


def main() -> None:
    parser=argparse.ArgumentParser(); parser.add_argument("--database",default="data/professional.db"); parser.add_argument("--symbols",nargs="+",default=DEFAULT_SYMBOLS); args=parser.parse_args()
    asyncio.run(collect(Path(args.database), [value.upper() for value in args.symbols]))

if __name__ == "__main__": main()
