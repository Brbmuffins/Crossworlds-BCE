from __future__ import annotations

import json
from urllib.error import HTTPError, URLError
from urllib.parse import urlsplit
from urllib.request import Request, urlopen


class DiscordNotificationError(RuntimeError):
    pass


class DiscordWebhook:
    def __init__(self, url: str, *, timeout_seconds: float = 10.0) -> None:
        parsed = urlsplit(url.strip())
        if (
            parsed.scheme != "https"
            or parsed.hostname != "discord.com"
            or not parsed.path.startswith("/api/webhooks/")
        ):
            raise ValueError("DISCORD_WEBHOOK_URL must be an official Discord HTTPS webhook")
        self._url = url.strip()
        self.timeout_seconds = timeout_seconds

    def send(self, message: str) -> None:
        content = " ".join(message.split())[:1800]
        if not content:
            raise ValueError("Discord message cannot be empty")
        request = Request(
            self._url,
            data=json.dumps({"content": content, "allowed_mentions": {"parse": []}}).encode("utf-8"),
            method="POST",
            headers={"Content-Type": "application/json", "User-Agent": "rh-crypto-bot/0.1"},
        )
        try:
            with urlopen(request, timeout=self.timeout_seconds) as response:
                if response.status not in {200, 204}:
                    raise DiscordNotificationError(f"Discord returned HTTP {response.status}")
        except HTTPError as exc:
            raise DiscordNotificationError(f"Discord returned HTTP {exc.code}") from exc
        except (URLError, TimeoutError) as exc:
            raise DiscordNotificationError("Unable to reach Discord") from exc


def tracked_markets_message(symbols: list[str]) -> str:
    normalized = sorted({symbol.upper() for symbol in symbols})
    benchmarks = [symbol for symbol in normalized if symbol == "BTC-USD"]
    altcoins = [symbol for symbol in normalized if symbol != "BTC-USD"]
    benchmark_text = ", ".join(benchmarks) if benchmarks else "none"
    altcoin_text = ", ".join(altcoins) if altcoins else "none"
    return (
        "Crypto shadow bot tracked markets (research only; no real orders). "
        f"Benchmark coin: {benchmark_text}. "
        f"Altcoins ({len(altcoins)}): {altcoin_text}. "
        f"Total tracked markets: {len(normalized)}."
    )


def promotion_ready_message(summary: dict[str, object]) -> str | None:
    gate = summary.get("promotion_gate")
    if not isinstance(gate, dict) or not gate.get("passed"):
        return None
    evidence = summary.get("independent_signal_results")
    if not isinstance(evidence, dict):
        evidence = {}
    interval = evidence.get("confidence_interval_95")
    interval_text = "unavailable"
    if isinstance(interval, list) and len(interval) == 2:
        interval_text = f"{interval[0]} to {interval[1]}"
    return (
        "Crypto shadow milestone reached: every research promotion prerequisite passed. "
        f"Independent signals={evidence.get('signals', 0)}; "
        f"average net return={evidence.get('average_net_return')}; "
        f"profit factor={evidence.get('profit_factor')}; "
        f"maximum drawdown={evidence.get('maximum_drawdown')}; "
        f"95% confidence interval={interval_text}; "
        f"BTC benchmark={evidence.get('average_btc_benchmark_return')}; "
        f"market benchmark={evidence.get('average_market_benchmark_return')}. "
        f"50-signal automation sample ready={evidence.get('automation_sample_ready', False)}. "
        "Ready for manual review of the next phase. Live trading remains disabled."
    )


def action_message(actions: list[dict[str, object]], summary: dict[str, object]) -> str | None:
    if not actions:
        return None
    opens = [item for item in actions if str(item.get("action", "")).startswith("opened")]
    closes = [item for item in actions if item.get("action") == "closed"]
    parts = ["Crypto shadow update (experimental; no real orders)."]
    if opens:
        symbols = sorted({str(item.get("symbol")) for item in opens})
        parts.append(f"Opened {len(opens)} sizing observations: {', '.join(symbols)}.")
    if closes:
        descriptions = [f"{item.get('symbol')} ({item.get('reason')})" for item in closes]
        parts.append(f"Closed {len(closes)}: {', '.join(descriptions)}.")
    parts.append(
        f"Open={summary.get('open_positions', 0)}; closed={summary.get('closed_positions', 0)}; "
        f"hypothetical P/L=${summary.get('total_hypothetical_profit_loss', '0.00')}."
    )
    return " ".join(parts)
