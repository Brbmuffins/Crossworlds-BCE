from __future__ import annotations

from dataclasses import dataclass
from decimal import Decimal
import json
from typing import Any
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from urllib.request import Request, urlopen


GRANULARITY_SECONDS = {
    "ONE_HOUR": 60 * 60,
    "FOUR_HOUR": 4 * 60 * 60,
}


@dataclass(frozen=True, order=True)
class Candle:
    start: int
    low: Decimal
    high: Decimal
    open: Decimal
    close: Decimal
    volume: Decimal

    def validate(self) -> None:
        if self.start < 0:
            raise ValueError("Candle timestamp cannot be negative")
        if min(self.low, self.high, self.open, self.close) <= 0:
            raise ValueError("Candle prices must be positive")
        if self.volume < 0:
            raise ValueError("Candle volume cannot be negative")
        if self.low > min(self.open, self.close) or self.high < max(self.open, self.close):
            raise ValueError("Candle OHLC values are inconsistent")


class MarketDataError(RuntimeError):
    pass


class CoinbasePublicMarketData:
    """Unauthenticated Coinbase candles used for research, never execution pricing."""

    base_url = "https://api.coinbase.com"
    provider_name = "coinbase"

    def __init__(self, *, timeout_seconds: float = 15.0) -> None:
        self.timeout_seconds = timeout_seconds

    def candles(
        self,
        symbol: str,
        granularity: str,
        *,
        start: int,
        end: int,
        limit: int = 350,
    ) -> list[Candle]:
        if granularity not in GRANULARITY_SECONDS:
            raise ValueError(f"Unsupported granularity: {granularity}")
        if start >= end:
            raise ValueError("start must be earlier than end")
        if limit < 1 or limit > 350:
            raise ValueError("limit must be between 1 and 350")
        product_id = symbol.upper()
        query = urlencode(
            {
                "start": str(start),
                "end": str(end),
                "granularity": granularity,
                "limit": str(limit),
            }
        )
        url = (
            f"{self.base_url}/api/v3/brokerage/market/products/"
            f"{product_id}/candles?{query}"
        )
        request = Request(
            url,
            method="GET",
            headers={"Accept": "application/json", "User-Agent": "rh-crypto-bot/0.1"},
        )
        try:
            with urlopen(request, timeout=self.timeout_seconds) as response:
                payload: dict[str, Any] = json.loads(response.read().decode("utf-8"))
        except HTTPError as exc:
            detail = exc.read().decode("utf-8", errors="replace")[:500]
            raise MarketDataError(f"Coinbase returned HTTP {exc.code}: {detail}") from exc
        except (URLError, TimeoutError, json.JSONDecodeError) as exc:
            raise MarketDataError(f"Unable to read Coinbase candles: {exc}") from exc

        parsed: list[Candle] = []
        for item in payload.get("candles", []):
            candle = Candle(
                start=int(item["start"]),
                low=Decimal(str(item["low"])),
                high=Decimal(str(item["high"])),
                open=Decimal(str(item["open"])),
                close=Decimal(str(item["close"])),
                volume=Decimal(str(item["volume"])),
            )
            candle.validate()
            parsed.append(candle)
        return sorted(parsed)

    def candles_range(
        self,
        symbol: str,
        granularity: str,
        *,
        start: int,
        end: int,
    ) -> list[Candle]:
        """Retrieve and de-duplicate an arbitrary interval in API-sized pages."""
        if granularity not in GRANULARITY_SECONDS:
            raise ValueError(f"Unsupported granularity: {granularity}")
        if start >= end:
            raise ValueError("start must be earlier than end")
        interval = GRANULARITY_SECONDS[granularity]
        page_span = interval * 300
        by_timestamp: dict[int, Candle] = {}
        page_start = start
        while page_start < end:
            page_end = min(page_start + page_span, end)
            page = self.candles(
                symbol,
                granularity,
                start=page_start,
                end=page_end,
                limit=350,
            )
            for candle in page:
                if start <= candle.start < end:
                    by_timestamp[candle.start] = candle
            page_start = page_end
        return [by_timestamp[timestamp] for timestamp in sorted(by_timestamp)]

    def products(self) -> list[dict[str, Any]]:
        query = urlencode({"product_type": "SPOT", "limit": "1000"})
        url = f"{self.base_url}/api/v3/brokerage/market/products?{query}"
        request = Request(
            url,
            method="GET",
            headers={"Accept": "application/json", "User-Agent": "rh-crypto-bot/0.1"},
        )
        try:
            with urlopen(request, timeout=self.timeout_seconds) as response:
                payload: dict[str, Any] = json.loads(response.read().decode("utf-8"))
        except HTTPError as exc:
            detail = exc.read().decode("utf-8", errors="replace")[:500]
            raise MarketDataError(f"Coinbase returned HTTP {exc.code}: {detail}") from exc
        except (URLError, TimeoutError, json.JSONDecodeError) as exc:
            raise MarketDataError(f"Unable to read Coinbase products: {exc}") from exc
        return list(payload.get("products", []))

    def order_book(self, symbol: str, *, levels: int = 25) -> dict[str, Decimal]:
        url = f"https://api.exchange.coinbase.com/products/{symbol.upper()}/book?level=2"
        request = Request(url, headers={"Accept": "application/json", "User-Agent": "rh-crypto-bot/0.1"})
        try:
            with urlopen(request, timeout=self.timeout_seconds) as response:
                payload = json.loads(response.read().decode("utf-8"))
            bids = [(Decimal(row[0]), Decimal(row[1])) for row in payload.get("bids", [])[:levels]]
            asks = [(Decimal(row[0]), Decimal(row[1])) for row in payload.get("asks", [])[:levels]]
        except (HTTPError, URLError, TimeoutError, json.JSONDecodeError, IndexError) as exc:
            raise MarketDataError(f"Unable to read Coinbase order book: {exc}") from exc
        if not bids or not asks:
            raise MarketDataError("Coinbase order book was empty")
        bid_depth = sum((price * size for price, size in bids), Decimal("0"))
        ask_depth = sum((price * size for price, size in asks), Decimal("0"))
        total = bid_depth + ask_depth
        return {"best_bid": bids[0][0], "best_ask": asks[0][0], "bid_depth": bid_depth,
                "ask_depth": ask_depth, "imbalance": (bid_depth - ask_depth) / total}
