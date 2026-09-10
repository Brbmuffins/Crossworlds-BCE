from __future__ import annotations

from decimal import Decimal, InvalidOperation
import json
from urllib.error import HTTPError, URLError
from urllib.parse import urlsplit
from urllib.request import Request, urlopen


class DiscordNotificationError(RuntimeError):
    pass


def _percent(value: object) -> str:
    if value is None:
        return "Unavailable"
    try:
        number = Decimal(str(value)) * 100
    except InvalidOperation:
        return str(value)
    return f"{number:+.2f}%"


def _money(value: object) -> str:
    try:
        number = Decimal(str(value))
    except InvalidOperation:
        return str(value)
    sign = "+" if number > 0 else ""
    return f"{sign}${number:,.2f}"


def _coin(symbol: object) -> str:
    text = str(symbol).upper()
    return text.removesuffix("-USD")


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
        # Preserve intentional line breaks while removing excess whitespace within each line.
        content = "\n".join(" ".join(line.split()) for line in message.splitlines()).strip()[:1800]
        if not content:
            raise ValueError("Discord message cannot be empty")
        request = Request(
            self._url,
            data=json.dumps({"content": content, "allowed_mentions": {"parse": []}}).encode(),
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
    altcoins = [_coin(symbol) for symbol in normalized if symbol != "BTC-USD"]
    benchmark = "Bitcoin (BTC)" if "BTC-USD" in normalized else "None"
    altcoin_lines = "\n".join(f"• {symbol}" for symbol in altcoins) or "• None"
    return (
        "📋 **Tracked Markets**\n"
        "_Research mode — no real orders_\n\n"
        f"**Benchmark**\n• {benchmark}\n\n"
        f"**Altcoins ({len(altcoins)})**\n{altcoin_lines}\n\n"
        f"**Total:** {len(normalized)} markets"
    )


def promotion_ready_message(summary: dict[str, object]) -> str | None:
    gate = summary.get("promotion_gate")
    if not isinstance(gate, dict) or not gate.get("passed"):
        return None
    evidence = summary.get("independent_signal_results")
    if not isinstance(evidence, dict):
        evidence = {}
    interval = evidence.get("confidence_interval_95")
    interval_text = "Unavailable"
    if isinstance(interval, list) and len(interval) == 2:
        interval_text = f"{_percent(interval[0])} to {_percent(interval[1])}"
    sample_ready = "Yes" if evidence.get("automation_sample_ready", False) else "No"
    return (
        "✅ **Research Gate Passed**\n"
        "_Manual review is required before the next phase_\n\n"
        f"**Evidence**\n"
        f"• Independent signals: {evidence.get('signals', 0)}\n"
        f"• Average net return: {_percent(evidence.get('average_net_return'))}\n"
        f"• Profit factor: {evidence.get('profit_factor', 'Unavailable')}\n"
        f"• Maximum drawdown: {_percent(evidence.get('maximum_drawdown'))}\n"
        f"• 95% confidence range: {interval_text}\n"
        f"• Bitcoin benchmark: {_percent(evidence.get('average_btc_benchmark_return'))}\n"
        f"• Market benchmark: {_percent(evidence.get('average_market_benchmark_return'))}\n"
        f"• 50-signal sample ready: {sample_ready}\n\n"
        "🔒 **Live trading remains disabled.**"
    )


def action_message(actions: list[dict[str, object]], summary: dict[str, object]) -> str | None:
    if not actions:
        return None
    opens = [item for item in actions if str(item.get("action", "")).startswith("opened")]
    closes = [item for item in actions if item.get("action") == "closed"]
    lines = ["🧪 **Shadow Trading Update**", "_Simulation only — no real orders_"]
    if opens:
        lines.extend(["", "**Opened observations**"])
        for item in opens:
            detail = f"• {_coin(item.get('symbol'))} — {_money(item.get('requested_notional', 0))} test size"
            if item.get("target_net_return") is not None:
                detail += f" — target {_percent(item['target_net_return'])}"
            lines.append(detail)
    if closes:
        lines.extend(["", "**Closed observations**"])
        reason_labels = {"target": "Target reached", "stop": "Stop reached", "expired": "Time expired"}
        for item in closes:
            reason = reason_labels.get(str(item.get("reason", "")), str(item.get("reason", "Unknown")).title())
            lines.append(f"• {_coin(item.get('symbol'))} — {reason}")
    lines.extend([
        "",
        "**Portfolio summary**",
        f"• Open observations: {summary.get('open_positions', 0)}",
        f"• Closed observations: {summary.get('closed_positions', 0)}",
        f"• Hypothetical P/L: {_money(summary.get('total_hypothetical_profit_loss', 0))}",
    ])
    return "\n".join(lines)
